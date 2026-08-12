#include "renderer_bridge/person_packet.hpp"

#include <sys/socket.h>
#include <sys/un.h>
#include <unistd.h>

#include <cerrno>
#include <cstddef>
#include <cstdlib>
#include <cstring>
#include <functional>
#include <optional>
#include <stdexcept>
#include <string>
#include <unordered_map>
#include <vector>

#include "rclcpp/rclcpp.hpp"
#include "std_msgs/msg/string.hpp"

namespace {

struct TrackInfo {
  std::int32_t id{0};
  float x{0.0f};
  float y{0.0f};
  std::string name;
};

std::optional<float> readNumberAfterColon(const std::string &json,
                                          std::size_t key_pos) {
  const auto colon = json.find(':', key_pos);
  if (colon == std::string::npos) {
    return std::nullopt;
  }
  char *end = nullptr;
  const float value = std::strtof(json.c_str() + colon + 1, &end);
  if (end == json.c_str() + colon + 1) {
    return std::nullopt;
  }
  return value;
}

std::optional<std::string> readQuotedStringAfterColon(const std::string &json,
                                                      std::size_t key_pos) {
  const auto colon = json.find(':', key_pos);
  if (colon == std::string::npos) {
    return std::nullopt;
  }
  const auto first_quote = json.find('"', colon + 1);
  if (first_quote == std::string::npos) {
    return std::nullopt;
  }
  const auto second_quote = json.find('"', first_quote + 1);
  if (second_quote == std::string::npos) {
    return std::nullopt;
  }
  return json.substr(first_quote + 1, second_quote - first_quote - 1);
}

// Parse objects from track_node JSON:
// [{"id": 1, "name": "HARRY", "x": 3.37, "y": -2.81, "missed_frames": 0}, ...]
std::vector<TrackInfo> parseTracksInfoJson(const std::string &json) {
  std::vector<TrackInfo> tracks;
  std::size_t pos = 0;

  while (true) {
    const auto obj_start = json.find('{', pos);
    if (obj_start == std::string::npos) {
      break;
    }
    const auto obj_end = json.find('}', obj_start + 1);
    if (obj_end == std::string::npos) {
      break;
    }

    const std::string object = json.substr(obj_start, obj_end - obj_start + 1);
    const auto id_key = object.find("\"id\"");
    const auto x_key = object.find("\"x\"");
    const auto y_key = object.find("\"y\"");
    if (id_key == std::string::npos || x_key == std::string::npos ||
        y_key == std::string::npos) {
      pos = obj_end + 1;
      continue;
    }

    const auto id_opt = readNumberAfterColon(object, id_key);
    const auto x_opt = readNumberAfterColon(object, x_key);
    const auto y_opt = readNumberAfterColon(object, y_key);
    if (!id_opt.has_value() || !x_opt.has_value() || !y_opt.has_value()) {
      pos = obj_end + 1;
      continue;
    }

    TrackInfo track;
    track.id = static_cast<std::int32_t>(*id_opt);
    track.x = *x_opt;
    track.y = *y_opt;

    const auto name_key = object.find("\"name\"");
    if (name_key != std::string::npos) {
      if (const auto name_opt = readQuotedStringAfterColon(object, name_key)) {
        track.name = *name_opt;
      }
    }

    tracks.push_back(track);
    pos = obj_end + 1;
  }

  return tracks;
}

} // namespace

class RendererBridgeNode : public rclcpp::Node {
public:
  RendererBridgeNode() : Node("renderer_bridge_node") {
    socket_path_ = declare_parameter<std::string>(
        "socket_path", "/tmp/takemura-renderer.sock");
    track_info_topic_ = declare_parameter<std::string>("track_info_topic",
                                                       "/person_tracks_info");

    openSocket();

    sub_ = create_subscription<std_msgs::msg::String>(
        track_info_topic_, 10,
        std::bind(&RendererBridgeNode::onTracksInfo, this,
                  std::placeholders::_1));

    RCLCPP_INFO(get_logger(), "Forwarding %s -> Unix socket %s",
                track_info_topic_.c_str(), socket_path_.c_str());
  }

  ~RendererBridgeNode() override {
    if (socket_fd_ >= 0) {
      ::close(socket_fd_);
      socket_fd_ = -1;
    }
  }

private:
  void openSocket() {
    socket_fd_ = ::socket(AF_UNIX, SOCK_DGRAM, 0);
    if (socket_fd_ < 0) {
      throw std::runtime_error("socket() failed");
    }

    std::memset(&dest_addr_, 0, sizeof(dest_addr_));
    dest_addr_.sun_family = AF_UNIX;

    if (socket_path_.size() >= sizeof(dest_addr_.sun_path)) {
      throw std::runtime_error("socket path is too long");
    }

    std::memcpy(dest_addr_.sun_path, socket_path_.c_str(),
                socket_path_.size() + 1);
    dest_addr_len_ = static_cast<socklen_t>(offsetof(sockaddr_un, sun_path) +
                                            socket_path_.size() + 1);
  }

  void onTracksInfo(const std_msgs::msg::String::SharedPtr msg) {
    const auto tracks = parseTracksInfoJson(msg->data);
    if (tracks.empty()) {
      if (!msg->data.empty() && msg->data != "[]") {
        RCLCPP_WARN_THROTTLE(get_logger(), *get_clock(), 2000,
                             "Could not parse track JSON: %s",
                             msg->data.c_str());
      }
      return;
    }

    const std::int64_t timestamp_ns =
        static_cast<std::int64_t>(this->now().nanoseconds());

    int sent_ok = 0;
    for (const auto &track : tracks) {
      mapipc::PersonUpdate update;
      update.id = track.id;
      update.x = track.x;
      update.y = track.y;
      update.sequence = ++sequence_by_id_[track.id];
      update.timestamp = timestamp_ns;
      update.name =
          track.name.empty() ? ("ID" + std::to_string(track.id)) : track.name;

      const mapipc::PersonPacketBytes packet =
          mapipc::serializePersonPacket(update);

      const ssize_t sent =
          ::sendto(socket_fd_, packet.data(), packet.size(), 0,
                   reinterpret_cast<sockaddr *>(&dest_addr_), dest_addr_len_);

      if (sent < 0) {
        RCLCPP_WARN_THROTTLE(get_logger(), *get_clock(), 2000,
                             "sendto failed (start map_renderer first): %s",
                             std::strerror(errno));
      } else {
        ++sent_ok;
      }
    }

    if (sent_ok > 0) {
      RCLCPP_INFO_THROTTLE(get_logger(), *get_clock(), 1000,
                           "sent %d/%zu person packet(s) -> %s", sent_ok,
                           tracks.size(), socket_path_.c_str());
    }
  }

  std::string socket_path_;
  std::string track_info_topic_;
  int socket_fd_{-1};
  sockaddr_un dest_addr_{};
  socklen_t dest_addr_len_{0};
  std::unordered_map<std::int32_t, std::uint64_t> sequence_by_id_;
  rclcpp::Subscription<std_msgs::msg::String>::SharedPtr sub_;
};

int main(int argc, char **argv) {
  rclcpp::init(argc, argv);
  rclcpp::spin(std::make_shared<RendererBridgeNode>());
  rclcpp::shutdown();
  return 0;
}
