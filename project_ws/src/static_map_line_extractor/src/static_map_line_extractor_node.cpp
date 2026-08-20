#include <cmath>
#include <cstdint>
#include <functional>
#include <memory>
#include <stdexcept>
#include <utility>
#include <vector>

#include "geometry_msgs/msg/point.hpp"
#include "geometry_msgs/msg/quaternion.hpp"
#include "nav_msgs/msg/occupancy_grid.hpp"
#include "rclcpp/rclcpp.hpp"
#include "static_map_line_extractor/line_extractor.hpp"
#include "visualization_msgs/msg/marker.hpp"
#include "visualization_msgs/msg/marker_array.hpp"

using std::placeholders::_1;
using static_map_line_extractor::ExtractionParameters;
using static_map_line_extractor::GridGeometry;
using static_map_line_extractor::LineExtractor;

namespace {

double quaternion_yaw(const geometry_msgs::msg::Quaternion &quaternion) {
  const double sin_yaw =
      2.0 * (quaternion.w * quaternion.z + quaternion.x * quaternion.y);
  const double cos_yaw =
      1.0 - 2.0 * (quaternion.y * quaternion.y +
                   quaternion.z * quaternion.z);
  return std::atan2(sin_yaw, cos_yaw);
}

}  // namespace

/**
 * ROS adapter for the ROS-independent LineExtractor.
 *
 * Input value 0 is treated as unconfirmed/unknown, never as proven free space.
 * Only cells at or above occupied_threshold participate in contour extraction.
 */
class StaticMapLineExtractorNode : public rclcpp::Node {
 public:
  StaticMapLineExtractorNode() : Node("static_map_line_extractor_node") {
    ExtractionParameters parameters;
    parameters.occupied_threshold =
        declare_parameter<int>("occupied_threshold", 100);
    parameters.min_contour_area =
        declare_parameter<double>("min_contour_area", 0.0);
    parameters.contour_epsilon =
        declare_parameter<double>("contour_epsilon", 0.10);
    parameters.minimum_line_length =
        declare_parameter<double>("minimum_line_length", 0.30);
    parameters.maximum_line_gap =
        declare_parameter<double>("maximum_line_gap", 0.20);
    parameters.minimum_component_cells =
        declare_parameter<int>("minimum_component_cells", 3);
    line_width_ = declare_parameter<double>("line_width", 0.04);
    update_period_ = declare_parameter<double>("update_period", 0.20);

    if (!std::isfinite(line_width_) || line_width_ <= 0.0) {
      throw std::invalid_argument("line_width must be positive and finite");
    }
    if (!std::isfinite(update_period_) || update_period_ < 0.0) {
      throw std::invalid_argument("update_period must be non-negative and finite");
    }
    extractor_ = std::make_unique<LineExtractor>(parameters);

    auto qos = rclcpp::QoS(rclcpp::KeepLast(1)).reliable().transient_local();
    grid_subscription_ = create_subscription<nav_msgs::msg::OccupancyGrid>(
        "/static_map/debug_grid", qos,
        std::bind(&StaticMapLineExtractorNode::grid_callback, this, _1));
    line_publisher_ = create_publisher<visualization_msgs::msg::MarkerArray>(
        "/static_map/lines", qos);

    RCLCPP_INFO(
        get_logger(),
        "static map line extractor ready: occupied>=%d, epsilon=%.3f m, "
        "minimum length=%.3f m, maximum gap=%.3f m",
        parameters.occupied_threshold, parameters.contour_epsilon,
        parameters.minimum_line_length, parameters.maximum_line_gap);
  }

 private:
  bool should_process(const rclcpp::Time &now) {
    if (!has_last_update_time_) {
      last_update_time_ = now;
      has_last_update_time_ = true;
      return true;
    }
    const double elapsed = (now - last_update_time_).seconds();
    if (elapsed >= 0.0 && elapsed < update_period_) {
      return false;
    }
    last_update_time_ = now;
    return true;
  }

  void grid_callback(const nav_msgs::msg::OccupancyGrid::SharedPtr grid) {
    if (!should_process(this->now())) {
      return;
    }

    const auto &origin = grid->info.origin;
    GridGeometry geometry{
        static_cast<int>(grid->info.width),
        static_cast<int>(grid->info.height),
        static_cast<double>(grid->info.resolution),
        origin.position.x,
        origin.position.y,
        quaternion_yaw(origin.orientation),
    };

    try {
      const auto result = extractor_->extract(grid->data, geometry);
      publish_lines(*grid, result.lines);
      ++published_count_;
      if (published_count_ == 1U || published_count_ % 25U == 0U) {
        RCLCPP_INFO(get_logger(),
                    "published %zu line segments from %zu contours in frame %s",
                    result.lines.size(), result.simplified_contours.size(),
                    grid->header.frame_id.c_str());
      }
    } catch (const std::exception &error) {
      RCLCPP_ERROR(get_logger(), "line extraction failed: %s", error.what());
    }
  }

  void publish_lines(
      const nav_msgs::msg::OccupancyGrid &grid,
      const std::vector<static_map_line_extractor::LineSegment> &lines) {
    visualization_msgs::msg::Marker marker;
    marker.header = grid.header;
    marker.ns = "static_environment_lines";
    marker.id = 0;
    marker.type = visualization_msgs::msg::Marker::LINE_LIST;
    marker.action = visualization_msgs::msg::Marker::ADD;
    marker.pose.orientation.w = 1.0;
    marker.scale.x = line_width_;
    marker.color.r = 0.10F;
    marker.color.g = 0.90F;
    marker.color.b = 1.00F;
    marker.color.a = 1.00F;
    marker.points.reserve(lines.size() * 2U);

    for (const auto &line : lines) {
      geometry_msgs::msg::Point start;
      start.x = line.start.x;
      start.y = line.start.y;
      start.z = 0.0;
      marker.points.push_back(start);

      geometry_msgs::msg::Point end;
      end.x = line.end.x;
      end.y = line.end.y;
      end.z = 0.0;
      marker.points.push_back(end);
    }

    visualization_msgs::msg::MarkerArray output;
    output.markers.push_back(std::move(marker));
    line_publisher_->publish(output);
  }

  double line_width_{0.04};
  double update_period_{0.20};
  bool has_last_update_time_{false};
  rclcpp::Time last_update_time_{0, 0, RCL_ROS_TIME};
  std::uint64_t published_count_{0U};
  std::unique_ptr<LineExtractor> extractor_;
  rclcpp::Subscription<nav_msgs::msg::OccupancyGrid>::SharedPtr
      grid_subscription_;
  rclcpp::Publisher<visualization_msgs::msg::MarkerArray>::SharedPtr
      line_publisher_;
};

int main(int argc, char *argv[]) {
  rclcpp::init(argc, argv);
  try {
    rclcpp::spin(std::make_shared<StaticMapLineExtractorNode>());
  } catch (const std::exception &error) {
    RCLCPP_FATAL(rclcpp::get_logger("static_map_line_extractor"), "%s",
                 error.what());
    rclcpp::shutdown();
    return 1;
  }
  rclcpp::shutdown();
  return 0;
}
