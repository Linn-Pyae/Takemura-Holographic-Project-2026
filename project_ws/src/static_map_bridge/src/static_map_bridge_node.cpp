#include "static_map_bridge/packet_protocol.hpp"

#include <rclcpp/rclcpp.hpp>
#include <visualization_msgs/msg/marker.hpp>
#include <visualization_msgs/msg/marker_array.hpp>

#include <sys/socket.h>
#include <sys/un.h>
#include <unistd.h>

#include <cerrno>
#include <cmath>
#include <cstddef>
#include <cstdint>
#include <cstring>
#include <memory>
#include <stdexcept>
#include <string>
#include <vector>

namespace static_map_bridge
{

class StaticMapBridgeNode : public rclcpp::Node
{
public:
  StaticMapBridgeNode()
  : Node("static_map_bridge_node")
  {
    socket_path_ = declare_parameter<std::string>(
      "socket_path", "/tmp/takemura-static-map.sock");
    const auto maximum_bytes = declare_parameter<int64_t>(
      "max_datagram_bytes", static_cast<int64_t>(kDefaultMaxDatagramBytes));
    if (maximum_bytes < static_cast<int64_t>(kHeaderBytes + kSegmentBytes) ||
      maximum_bytes > 65536)
    {
      throw std::invalid_argument("max_datagram_bytes must be in [44, 65536]");
    }
    max_datagram_bytes_ = static_cast<std::size_t>(maximum_bytes);
    if (socket_path_.empty() || socket_path_.size() >= sizeof(sockaddr_un::sun_path)) {
      throw std::invalid_argument("socket_path is empty or too long for sockaddr_un");
    }

    socket_fd_ = ::socket(AF_UNIX, SOCK_DGRAM, 0);
    if (socket_fd_ < 0) {
      throw std::runtime_error(
              std::string("failed to create Unix datagram socket: ") + std::strerror(errno));
    }

    rclcpp::QoS qos(rclcpp::KeepLast(1));
    qos.reliable().transient_local();
    subscription_ = create_subscription<visualization_msgs::msg::MarkerArray>(
      "/static_map/lines", qos,
      [this](visualization_msgs::msg::MarkerArray::ConstSharedPtr message) {
        handle_markers(*message);
      });

    RCLCPP_INFO(
      get_logger(),
      "static map bridge: /static_map/lines -> %s, max_datagram_bytes=%zu, max_segments=%zu",
      socket_path_.c_str(), max_datagram_bytes_,
      max_segments_per_packet(max_datagram_bytes_));
  }

  ~StaticMapBridgeNode() override
  {
    if (socket_fd_ >= 0) {
      ::close(socket_fd_);
    }
  }

private:
  void handle_markers(const visualization_msgs::msg::MarkerArray & message)
  {
    std::vector<LineSegment> segments;
    std::size_t ignored_odd_points = 0;
    std::size_t ignored_non_finite = 0;

    for (const auto & marker : message.markers) {
      if (marker.type != visualization_msgs::msg::Marker::LINE_LIST ||
        marker.action != visualization_msgs::msg::Marker::ADD)
      {
        continue;
      }
      if ((marker.points.size() % 2U) != 0U) {
        ++ignored_odd_points;
      }
      for (std::size_t index = 0; index + 1U < marker.points.size(); index += 2U) {
        const auto & first = marker.points[index];
        const auto & second = marker.points[index + 1U];
        if (!std::isfinite(first.x) || !std::isfinite(first.y) ||
          !std::isfinite(second.x) || !std::isfinite(second.y))
        {
          ++ignored_non_finite;
          continue;
        }
        // Keep the LiDAR coordinate system. Renderer-specific (-y, -x) is not
        // performed by this bridge.
        segments.push_back(LineSegment{
          static_cast<float>(first.x), static_cast<float>(first.y),
          static_cast<float>(second.x), static_cast<float>(second.y)});
      }
    }

    if (ignored_odd_points != 0U || ignored_non_finite != 0U) {
      RCLCPP_WARN(
        get_logger(), "ignored malformed LINE_LIST data: odd_markers=%zu non_finite=%zu",
        ignored_odd_points, ignored_non_finite);
    }

    const std::uint64_t sequence = next_sequence_++;
    try {
      const auto packets = packetize_segments(segments, sequence, max_datagram_bytes_);
      std::size_t sent_packets = 0;
      for (const auto & packet : packets) {
        if (!send_packet(packet)) {
          return;
        }
        ++sent_packets;
      }
      RCLCPP_DEBUG(
        get_logger(), "sent map sequence=%llu segments=%zu packets=%zu",
        static_cast<unsigned long long>(sequence), segments.size(), sent_packets);
    } catch (const std::exception & error) {
      RCLCPP_ERROR(
        get_logger(), "failed to encode map sequence=%llu: %s",
        static_cast<unsigned long long>(sequence), error.what());
    }
  }

  bool send_packet(const std::vector<std::uint8_t> & packet)
  {
    sockaddr_un destination{};
    destination.sun_family = AF_UNIX;
    std::strncpy(destination.sun_path, socket_path_.c_str(), sizeof(destination.sun_path) - 1U);
    const socklen_t destination_length = static_cast<socklen_t>(
      offsetof(sockaddr_un, sun_path) + socket_path_.size() + 1U);

    const ssize_t sent = ::sendto(
      socket_fd_, packet.data(), packet.size(), 0,
      reinterpret_cast<const sockaddr *>(&destination), destination_length);
    if (sent < 0) {
      RCLCPP_WARN_THROTTLE(
        get_logger(), *get_clock(), 5000,
        "cannot send static map packet to %s: %s (start the future receiver first)",
        socket_path_.c_str(), std::strerror(errno));
      return false;
    }
    if (static_cast<std::size_t>(sent) != packet.size()) {
      RCLCPP_WARN_THROTTLE(
        get_logger(), *get_clock(), 5000,
        "short Unix datagram write to %s: sent=%zd expected=%zu",
        socket_path_.c_str(), sent, packet.size());
      return false;
    }
    return true;
  }

  std::string socket_path_;
  std::size_t max_datagram_bytes_{kDefaultMaxDatagramBytes};
  int socket_fd_{-1};
  std::uint64_t next_sequence_{1};
  rclcpp::Subscription<visualization_msgs::msg::MarkerArray>::SharedPtr subscription_;
};

}  // namespace static_map_bridge

int main(int argc, char ** argv)
{
  rclcpp::init(argc, argv);
  rclcpp::spin(std::make_shared<static_map_bridge::StaticMapBridgeNode>());
  rclcpp::shutdown();
  return 0;
}
