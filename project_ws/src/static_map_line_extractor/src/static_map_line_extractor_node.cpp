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
 * Only cells at or above occupied_threshold participate in shape extraction.
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
        declare_parameter<int>("minimum_component_cells", 8);
    parameters.wall_min_length =
        declare_parameter<double>("wall_min_length", 1.20);
    parameters.wall_min_aspect_ratio =
        declare_parameter<double>("wall_min_aspect_ratio", 3.50);
    parameters.minimum_block_size =
        declare_parameter<double>("minimum_block_size", 0.40);
    parameters.wall_merge_angle_degrees =
        declare_parameter<double>("wall_merge_angle_degrees", 12.0);
    parameters.wall_merge_distance =
        declare_parameter<double>("wall_merge_distance", 0.30);
    parameters.wall_merge_gap =
        declare_parameter<double>("wall_merge_gap", 0.50);
    wall_line_width_ = declare_parameter<double>("wall_line_width", 0.12);
    block_line_width_ = declare_parameter<double>("block_line_width", 0.07);
    update_period_ = declare_parameter<double>("update_period", 0.20);

    if (!std::isfinite(wall_line_width_) || wall_line_width_ <= 0.0 ||
        !std::isfinite(block_line_width_) || block_line_width_ <= 0.0) {
      throw std::invalid_argument("shape line widths must be positive and finite");
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
        "static map shape extractor ready: occupied>=%d, wall>=%.2f m "
        "aspect>=%.2f, block>=%.2f m, merge gap=%.2f m",
        parameters.occupied_threshold, parameters.wall_min_length,
        parameters.wall_min_aspect_ratio,
        parameters.minimum_block_size, parameters.wall_merge_gap);
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
      publish_shapes(*grid, result);
      ++published_count_;
      if (published_count_ == 1U || published_count_ % 25U == 0U) {
        RCLCPP_INFO(get_logger(),
                    "published coarse map: walls=%zu blocks=%zu flattened=%zu "
                    "from %zu components in frame %s",
                    result.wall_lines.size(), result.blocks.size(),
                    result.lines.size(), result.simplified_contours.size(),
                    grid->header.frame_id.c_str());
      }
    } catch (const std::exception &error) {
      RCLCPP_ERROR(get_logger(), "line extraction failed: %s", error.what());
    }
  }

  static void append_line(visualization_msgs::msg::Marker &marker,
                          const static_map_line_extractor::LineSegment &line) {
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

  void publish_shapes(
      const nav_msgs::msg::OccupancyGrid &grid,
      const static_map_line_extractor::ExtractionResult &result) {
    visualization_msgs::msg::Marker walls;
    walls.header = grid.header;
    walls.ns = "static_environment_walls";
    walls.id = 0;
    walls.type = visualization_msgs::msg::Marker::LINE_LIST;
    walls.action = visualization_msgs::msg::Marker::ADD;
    walls.pose.orientation.w = 1.0;
    walls.scale.x = wall_line_width_;
    walls.color.r = 0.05F;
    walls.color.g = 0.48F;
    walls.color.b = 0.57F;
    walls.color.a = 1.00F;
    walls.points.reserve(result.wall_lines.size() * 2U);
    for (const auto &line : result.wall_lines) {
      append_line(walls, line);
    }

    visualization_msgs::msg::Marker blocks;
    blocks.header = grid.header;
    blocks.ns = "static_environment_blocks";
    blocks.id = 0;
    blocks.type = visualization_msgs::msg::Marker::LINE_LIST;
    blocks.action = visualization_msgs::msg::Marker::ADD;
    blocks.pose.orientation.w = 1.0;
    blocks.scale.x = block_line_width_;
    blocks.color.r = 0.80F;
    blocks.color.g = 0.48F;
    blocks.color.b = 0.12F;
    blocks.color.a = 1.00F;
    blocks.points.reserve(result.blocks.size() * 8U);
    for (const auto &block : result.blocks) {
      for (std::size_t index = 0; index < block.corners.size(); ++index) {
        const auto next = (index + 1U) % block.corners.size();
        append_line(blocks, {block.corners[index], block.corners[next]});
      }
    }

    visualization_msgs::msg::MarkerArray output;
    output.markers.push_back(std::move(walls));
    output.markers.push_back(std::move(blocks));
    line_publisher_->publish(output);
  }

  double wall_line_width_{0.12};
  double block_line_width_{0.07};
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
