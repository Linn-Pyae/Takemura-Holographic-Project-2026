#include <algorithm>
#include <cmath>
#include <cstdint>
#include <functional>
#include <limits>
#include <memory>
#include <stdexcept>
#include <string>
#include <vector>

#include "nav_msgs/msg/occupancy_grid.hpp"
#include "rclcpp/rclcpp.hpp"
#include "sensor_msgs/msg/point_cloud2.hpp"
#include "sensor_msgs/msg/point_field.hpp"
#include "sensor_msgs/point_cloud2_iterator.hpp"
#include "std_msgs/msg/header.hpp"

using std::placeholders::_1;

namespace {

// These constants intentionally follow the floor-calibration scale used by
// person_cluster, but remain private to this independent package.
constexpr double kFloorBinSize = 0.05;
constexpr double kFloorRangeMin = -5.0;
constexpr double kFloorRangeMax = 5.0;
constexpr double kFloorSearchSpan = 0.60;
constexpr int kFloorCalibrationFrames = 15;
constexpr std::size_t kMaximumGridCells = 25'000'000;

bool has_float32_field(const sensor_msgs::msg::PointCloud2 &cloud,
                       const std::string &name) {
  for (const auto &field : cloud.fields) {
    if (field.name == name) {
      return field.datatype == sensor_msgs::msg::PointField::FLOAT32;
    }
  }
  return false;
}

}  // namespace

/**
 * Convert a height-filtered PointCloud2 stream into a persistent XY grid.
 *
 * A cell is published as occupied only after it has been observed in multiple
 * processed frames. Missed observations decrement the cell score, so old
 * objects eventually disappear instead of remaining in the debug map forever.
 * No person-detection, tracking, line extraction, socket, or rendering logic is
 * used here; this node is deliberately independent of the existing pipeline.
 */
class StaticMapProjectorNode : public rclcpp::Node {
 public:
  StaticMapProjectorNode() : Node("static_map_projector_node") {
    auto_floor_ = declare_parameter<bool>("auto_floor", true);
    floor_z_ = declare_parameter<double>("floor_z", -0.85);
    min_height_above_floor_ =
        declare_parameter<double>("min_height_above_floor", 0.30);
    max_height_above_floor_ =
        declare_parameter<double>("max_height_above_floor", 2.00);
    max_range_ = declare_parameter<double>("max_range", 15.0);
    voxel_size_xy_ = declare_parameter<double>("voxel_size_xy", 0.10);
    static_observation_threshold_ =
        declare_parameter<int>("static_observation_threshold", 5);
    map_update_period_ = declare_parameter<double>("map_update_period", 0.20);

    validate_parameters_and_create_grid();

    floor_histogram_.assign(
        static_cast<std::size_t>((kFloorRangeMax - kFloorRangeMin) /
                                 kFloorBinSize),
        0U);
    floor_calibrated_ = !auto_floor_;

    auto input_qos = rclcpp::SensorDataQoS().keep_last(1);
    cloud_subscription_ = create_subscription<sensor_msgs::msg::PointCloud2>(
        "/velodyne_points_bag", input_qos,
        std::bind(&StaticMapProjectorNode::cloud_callback, this, _1));

    // Transient-local QoS lets RViz receive the latest grid after it connects.
    auto output_qos = rclcpp::QoS(rclcpp::KeepLast(1)).reliable().transient_local();
    grid_publisher_ = create_publisher<nav_msgs::msg::OccupancyGrid>(
        "/static_map/debug_grid", output_qos);

    RCLCPP_INFO(
        get_logger(),
        "static map projector ready: range=%.2f m, resolution=%.3f m, "
        "threshold=%d, floor=%s (z=%.2f)",
        max_range_, voxel_size_xy_, static_observation_threshold_,
        auto_floor_ ? "auto" : "manual", floor_z_);
  }

 private:
  void validate_parameters_and_create_grid() {
    if (!std::isfinite(floor_z_)) {
      throw std::invalid_argument("floor_z must be finite");
    }
    if (!std::isfinite(min_height_above_floor_) ||
        !std::isfinite(max_height_above_floor_) ||
        min_height_above_floor_ < 0.0 ||
        max_height_above_floor_ <= min_height_above_floor_) {
      throw std::invalid_argument(
          "height limits must satisfy 0 <= min_height_above_floor < "
          "max_height_above_floor");
    }
    if (!std::isfinite(max_range_) || max_range_ <= 0.0) {
      throw std::invalid_argument("max_range must be finite and greater than 0");
    }
    if (!std::isfinite(voxel_size_xy_) || voxel_size_xy_ <= 0.0) {
      throw std::invalid_argument(
          "voxel_size_xy must be finite and greater than 0");
    }
    if (static_observation_threshold_ < 1 ||
        static_observation_threshold_ >
            static_cast<int>(std::numeric_limits<std::uint16_t>::max())) {
      throw std::invalid_argument(
          "static_observation_threshold must be between 1 and 65535");
    }
    if (!std::isfinite(map_update_period_) || map_update_period_ < 0.0) {
      throw std::invalid_argument(
          "map_update_period must be finite and non-negative");
    }

    const auto dimension = static_cast<std::uint64_t>(
        std::ceil((2.0 * max_range_) / voxel_size_xy_));
    if (dimension == 0U ||
        dimension > std::numeric_limits<std::uint32_t>::max() ||
        dimension * dimension > kMaximumGridCells) {
      throw std::invalid_argument(
          "max_range and voxel_size_xy create an unsupported grid size");
    }

    grid_width_ = static_cast<std::uint32_t>(dimension);
    grid_height_ = static_cast<std::uint32_t>(dimension);
    observation_scores_.assign(
        static_cast<std::size_t>(grid_width_) * grid_height_, 0U);
  }

  bool should_process(const rclcpp::Time &now) {
    if (!has_processed_time_) {
      last_processed_time_ = now;
      has_processed_time_ = true;
      return true;
    }

    const double elapsed = (now - last_processed_time_).seconds();
    if (elapsed >= 0.0 && elapsed < map_update_period_) {
      return false;
    }
    // A negative interval means simulated time restarted; process immediately.
    last_processed_time_ = now;
    return true;
  }

  void accumulate_floor_histogram(
      const sensor_msgs::msg::PointCloud2 &cloud) {
    sensor_msgs::PointCloud2ConstIterator<float> x_it(cloud, "x");
    sensor_msgs::PointCloud2ConstIterator<float> y_it(cloud, "y");
    sensor_msgs::PointCloud2ConstIterator<float> z_it(cloud, "z");

    const double max_range_squared = max_range_ * max_range_;
    for (; x_it != x_it.end(); ++x_it, ++y_it, ++z_it) {
      const double x = *x_it;
      const double y = *y_it;
      const double z = *z_it;
      if (!std::isfinite(x) || !std::isfinite(y) || !std::isfinite(z) ||
          x * x + y * y > max_range_squared || z < kFloorRangeMin ||
          z >= kFloorRangeMax) {
        continue;
      }

      const auto bin = static_cast<std::size_t>(
          (z - kFloorRangeMin) / kFloorBinSize);
      if (bin < floor_histogram_.size()) {
        ++floor_histogram_[bin];
      }
    }
  }

  void finish_floor_calibration() {
    std::uint64_t total = 0U;
    for (const auto count : floor_histogram_) {
      total += count;
    }
    if (total == 0U) {
      RCLCPP_WARN(get_logger(),
                  "automatic floor calibration had no valid points; using "
                  "floor_z=%.3f",
                  floor_z_);
      floor_calibrated_ = true;
      return;
    }

    // Ignore the lowest 0.5 percent as possible outliers, then find the
    // densest z slab in the following 0.60 m.
    const std::uint64_t noise_budget = std::max<std::uint64_t>(1U, total / 200U);
    std::size_t lowest_bin = 0U;
    std::uint64_t cumulative = 0U;
    for (std::size_t i = 0; i < floor_histogram_.size(); ++i) {
      cumulative += floor_histogram_[i];
      if (cumulative > noise_budget) {
        lowest_bin = i;
        break;
      }
    }

    const auto span_bins =
        static_cast<std::size_t>(kFloorSearchSpan / kFloorBinSize);
    const auto last_bin =
        std::min(floor_histogram_.size() - 1U, lowest_bin + span_bins);
    std::size_t best_bin = lowest_bin;
    std::uint64_t best_count = 0U;
    for (std::size_t i = lowest_bin; i <= last_bin; ++i) {
      if (floor_histogram_[i] > best_count) {
        best_count = floor_histogram_[i];
        best_bin = i;
      }
    }

    if (best_count > 0U) {
      floor_z_ = kFloorRangeMin +
                 (static_cast<double>(best_bin) + 0.5) * kFloorBinSize;
    }
    floor_calibrated_ = true;
    RCLCPP_INFO(get_logger(),
                "automatic floor calibration complete: floor_z=%.3f from "
                "%llu points",
                floor_z_, static_cast<unsigned long long>(total));
  }

  void update_grid(const sensor_msgs::msg::PointCloud2 &cloud) {
    std::vector<std::uint8_t> observed_this_frame(
        observation_scores_.size(), 0U);

    sensor_msgs::PointCloud2ConstIterator<float> x_it(cloud, "x");
    sensor_msgs::PointCloud2ConstIterator<float> y_it(cloud, "y");
    sensor_msgs::PointCloud2ConstIterator<float> z_it(cloud, "z");
    const double max_range_squared = max_range_ * max_range_;

    for (; x_it != x_it.end(); ++x_it, ++y_it, ++z_it) {
      const double x = *x_it;
      const double y = *y_it;
      const double z = *z_it;
      if (!std::isfinite(x) || !std::isfinite(y) || !std::isfinite(z) ||
          x * x + y * y > max_range_squared) {
        continue;
      }

      const double height = z - floor_z_;
      if (height < min_height_above_floor_ ||
          height > max_height_above_floor_) {
        continue;
      }

      const auto grid_x = static_cast<std::int64_t>(
          std::floor((x + max_range_) / voxel_size_xy_));
      const auto grid_y = static_cast<std::int64_t>(
          std::floor((y + max_range_) / voxel_size_xy_));
      if (grid_x < 0 || grid_y < 0 ||
          grid_x >= static_cast<std::int64_t>(grid_width_) ||
          grid_y >= static_cast<std::int64_t>(grid_height_)) {
        continue;
      }

      const auto index = static_cast<std::size_t>(grid_y) * grid_width_ +
                         static_cast<std::size_t>(grid_x);
      observed_this_frame[index] = 1U;
    }

    for (std::size_t i = 0; i < observation_scores_.size(); ++i) {
      auto &score = observation_scores_[i];
      if (observed_this_frame[i] != 0U) {
        score = static_cast<std::uint16_t>(std::min<int>(
            static_observation_threshold_, static_cast<int>(score) + 1));
      } else if (score > 0U) {
        --score;
      }
    }
  }

  void publish_grid(const std_msgs::msg::Header &input_header) {
    nav_msgs::msg::OccupancyGrid output;
    output.header = input_header;
    output.info.map_load_time = input_header.stamp;
    output.info.resolution = static_cast<float>(voxel_size_xy_);
    output.info.width = grid_width_;
    output.info.height = grid_height_;
    output.info.origin.position.x = -max_range_;
    output.info.origin.position.y = -max_range_;
    output.info.origin.position.z = floor_z_;
    output.info.origin.orientation.w = 1.0;

    output.data.resize(observation_scores_.size(), 0);
    std::size_t static_cells = 0U;
    for (std::size_t i = 0; i < observation_scores_.size(); ++i) {
      if (observation_scores_[i] >= static_observation_threshold_) {
        output.data[i] = 100;
        ++static_cells;
      }
    }
    grid_publisher_->publish(output);

    ++published_grid_count_;
    if (published_grid_count_ == 1U || published_grid_count_ % 25U == 0U) {
      RCLCPP_INFO(get_logger(),
                  "published debug grid: static_cells=%zu, floor_z=%.3f, "
                  "frame=%s",
                  static_cells, floor_z_, input_header.frame_id.c_str());
    }
  }

  void cloud_callback(const sensor_msgs::msg::PointCloud2::SharedPtr cloud) {
    if (!has_float32_field(*cloud, "x") ||
        !has_float32_field(*cloud, "y") ||
        !has_float32_field(*cloud, "z")) {
      if (!reported_bad_fields_) {
        RCLCPP_ERROR(
            get_logger(),
            "PointCloud2 must contain FLOAT32 fields named x, y, and z");
        reported_bad_fields_ = true;
      }
      return;
    }

    if (!should_process(this->now())) {
      return;
    }

    try {
      if (!floor_calibrated_) {
        accumulate_floor_histogram(*cloud);
        ++floor_calibration_frame_count_;
        if (floor_calibration_frame_count_ < kFloorCalibrationFrames) {
          return;
        }
        finish_floor_calibration();
      }

      update_grid(*cloud);
      publish_grid(cloud->header);
    } catch (const std::runtime_error &error) {
      RCLCPP_ERROR(get_logger(), "failed to read PointCloud2: %s", error.what());
    }
  }

  bool auto_floor_{true};
  double floor_z_{-0.85};
  double min_height_above_floor_{0.30};
  double max_height_above_floor_{2.00};
  double max_range_{15.0};
  double voxel_size_xy_{0.10};
  int static_observation_threshold_{5};
  double map_update_period_{0.20};

  std::uint32_t grid_width_{0U};
  std::uint32_t grid_height_{0U};
  std::vector<std::uint16_t> observation_scores_;
  std::vector<std::uint64_t> floor_histogram_;
  int floor_calibration_frame_count_{0};
  bool floor_calibrated_{false};
  bool reported_bad_fields_{false};
  bool has_processed_time_{false};
  rclcpp::Time last_processed_time_{0, 0, RCL_ROS_TIME};
  std::uint64_t published_grid_count_{0U};

  rclcpp::Subscription<sensor_msgs::msg::PointCloud2>::SharedPtr
      cloud_subscription_;
  rclcpp::Publisher<nav_msgs::msg::OccupancyGrid>::SharedPtr grid_publisher_;
};

int main(int argc, char *argv[]) {
  rclcpp::init(argc, argv);
  try {
    rclcpp::spin(std::make_shared<StaticMapProjectorNode>());
  } catch (const std::exception &error) {
    RCLCPP_FATAL(rclcpp::get_logger("static_map_projector"), "%s",
                 error.what());
    rclcpp::shutdown();
    return 1;
  }
  rclcpp::shutdown();
  return 0;
}
