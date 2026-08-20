#pragma once

#include <cstdint>
#include <utility>
#include <vector>

#include <opencv2/core.hpp>

namespace static_map_line_extractor {

/// OccupancyGrid geometry without any ROS message dependency.
struct GridGeometry {
  int width{0};
  int height{0};
  double resolution{0.0};
  double origin_x{0.0};
  double origin_y{0.0};
  double origin_yaw{0.0};
};

/// Parameters used by the ROS-independent extraction algorithm.
struct ExtractionParameters {
  int occupied_threshold{100};
  double min_contour_area{0.0};       // square metres
  double contour_epsilon{0.10};       // metres
  double minimum_line_length{0.30};   // metres
  double maximum_line_gap{0.20};      // metres, used for binary closing
  int minimum_component_cells{3};
};

struct LineSegment {
  cv::Point2d start;
  cv::Point2d end;

  double length() const;
};

struct ExtractionResult {
  cv::Mat cleaned_binary;
  std::vector<std::vector<cv::Point>> simplified_contours;
  std::vector<LineSegment> lines;
};

/**
 * Convert occupied cells into simplified contour edges.
 *
 * This class has no rclcpp or ROS message dependency. The ROS node is only an
 * adapter around it, which keeps image processing independently testable.
 */
class LineExtractor {
 public:
  explicit LineExtractor(ExtractionParameters parameters);

  ExtractionResult extract(const std::vector<std::int8_t> &occupancy,
                           const GridGeometry &geometry) const;

  static cv::Point2d grid_cell_to_world(const cv::Point &cell,
                                        const GridGeometry &geometry);

 private:
  cv::Mat make_binary(const std::vector<std::int8_t> &occupancy,
                      const GridGeometry &geometry) const;
  cv::Mat close_small_gaps(const cv::Mat &binary, double resolution) const;
  cv::Mat remove_small_components(const cv::Mat &binary) const;
  std::vector<std::vector<cv::Point>> find_simplified_contours(
      const cv::Mat &binary, double resolution) const;

  ExtractionParameters parameters_;
};

}  // namespace static_map_line_extractor
