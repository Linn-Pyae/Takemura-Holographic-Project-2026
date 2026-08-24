#pragma once

#include <array>
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
  int minimum_component_cells{8};
  double wall_min_length{1.20};        // metres
  double wall_min_aspect_ratio{3.50};
  double minimum_block_size{0.40};     // metres, visual minimum
  double wall_merge_angle_degrees{12.0};
  double wall_merge_distance{0.30};    // perpendicular metres
  double wall_merge_gap{0.50};         // longitudinal metres
};

struct LineSegment {
  cv::Point2d start;
  cv::Point2d end;

  double length() const;
};

/// Coarse oriented rectangle used for compact static clusters.
struct OrientedBlock {
  std::array<cv::Point2d, 4> corners;
};

struct ExtractionResult {
  cv::Mat cleaned_binary;
  std::vector<std::vector<cv::Point>> simplified_contours;
  std::vector<LineSegment> wall_lines;
  std::vector<OrientedBlock> blocks;
  /// Compatibility output: wall lines followed by four edges per block.
  std::vector<LineSegment> lines;
};

/**
 * Convert occupied cells into coarse, presentation-friendly map shapes.
 *
 * Long, narrow components become one wall centreline. Compact components
 * become oriented rectangles. Nearby collinear wall fragments are merged.
 * The class has no rclcpp or ROS message dependency.
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
  void classify_components(const cv::Mat &binary,
                           const GridGeometry &geometry,
                           ExtractionResult &result) const;
  std::vector<LineSegment> merge_wall_lines(
      std::vector<LineSegment> walls) const;

  ExtractionParameters parameters_;
};

}  // namespace static_map_line_extractor
