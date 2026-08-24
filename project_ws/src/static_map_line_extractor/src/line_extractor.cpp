#include "static_map_line_extractor/line_extractor.hpp"

#include <algorithm>
#include <cmath>
#include <cstddef>
#include <limits>
#include <stdexcept>

#include <opencv2/imgproc.hpp>

namespace static_map_line_extractor {

namespace {

constexpr double kPi = 3.14159265358979323846;

cv::Point2d normalized(const cv::Point2d &value) {
  const double length = std::hypot(value.x, value.y);
  if (length <= std::numeric_limits<double>::epsilon()) {
    return {1.0, 0.0};
  }
  return {value.x / length, value.y / length};
}

double dot(const cv::Point2d &left, const cv::Point2d &right) {
  return left.x * right.x + left.y * right.y;
}

cv::Point2d grid_point_to_world(const cv::Point2d &point,
                                const GridGeometry &geometry) {
  const double local_x = (point.x + 0.5) * geometry.resolution;
  const double local_y = (point.y + 0.5) * geometry.resolution;
  const double cosine = std::cos(geometry.origin_yaw);
  const double sine = std::sin(geometry.origin_yaw);
  return {
      geometry.origin_x + cosine * local_x - sine * local_y,
      geometry.origin_y + sine * local_x + cosine * local_y,
  };
}

void validate_parameters(const ExtractionParameters &parameters) {
  if (parameters.occupied_threshold < 1 ||
      parameters.occupied_threshold > 100) {
    throw std::invalid_argument("occupied_threshold must be between 1 and 100");
  }
  if (!std::isfinite(parameters.min_contour_area) ||
      parameters.min_contour_area < 0.0) {
    throw std::invalid_argument("min_contour_area must be non-negative");
  }
  if (!std::isfinite(parameters.contour_epsilon) ||
      parameters.contour_epsilon <= 0.0) {
    throw std::invalid_argument("contour_epsilon must be greater than zero");
  }
  if (!std::isfinite(parameters.minimum_line_length) ||
      parameters.minimum_line_length < 0.0) {
    throw std::invalid_argument("minimum_line_length must be non-negative");
  }
  if (!std::isfinite(parameters.maximum_line_gap) ||
      parameters.maximum_line_gap < 0.0) {
    throw std::invalid_argument("maximum_line_gap must be non-negative");
  }
  if (parameters.minimum_component_cells < 1) {
    throw std::invalid_argument("minimum_component_cells must be at least 1");
  }
  if (!std::isfinite(parameters.wall_min_length) ||
      parameters.wall_min_length <= 0.0) {
    throw std::invalid_argument("wall_min_length must be positive and finite");
  }
  if (!std::isfinite(parameters.wall_min_aspect_ratio) ||
      parameters.wall_min_aspect_ratio <= 1.0) {
    throw std::invalid_argument(
        "wall_min_aspect_ratio must be finite and greater than 1");
  }
  if (!std::isfinite(parameters.minimum_block_size) ||
      parameters.minimum_block_size <= 0.0) {
    throw std::invalid_argument(
        "minimum_block_size must be positive and finite");
  }
  if (!std::isfinite(parameters.wall_merge_angle_degrees) ||
      parameters.wall_merge_angle_degrees < 0.0 ||
      parameters.wall_merge_angle_degrees > 90.0) {
    throw std::invalid_argument(
        "wall_merge_angle_degrees must be between 0 and 90");
  }
  if (!std::isfinite(parameters.wall_merge_distance) ||
      parameters.wall_merge_distance < 0.0 ||
      !std::isfinite(parameters.wall_merge_gap) ||
      parameters.wall_merge_gap < 0.0) {
    throw std::invalid_argument(
        "wall merge distance and gap must be non-negative and finite");
  }
}

void validate_geometry(const GridGeometry &geometry,
                       std::size_t occupancy_size) {
  if (geometry.width <= 0 || geometry.height <= 0) {
    throw std::invalid_argument("grid width and height must be positive");
  }
  if (!std::isfinite(geometry.resolution) || geometry.resolution <= 0.0) {
    throw std::invalid_argument("grid resolution must be positive and finite");
  }
  if (!std::isfinite(geometry.origin_x) ||
      !std::isfinite(geometry.origin_y) ||
      !std::isfinite(geometry.origin_yaw)) {
    throw std::invalid_argument("grid origin must be finite");
  }
  const auto expected = static_cast<std::size_t>(geometry.width) *
                        static_cast<std::size_t>(geometry.height);
  if (occupancy_size != expected) {
    throw std::invalid_argument("occupancy data size does not match grid geometry");
  }
}

}  // namespace

double LineSegment::length() const {
  return std::hypot(end.x - start.x, end.y - start.y);
}

LineExtractor::LineExtractor(ExtractionParameters parameters)
    : parameters_(parameters) {
  validate_parameters(parameters_);
}

cv::Mat LineExtractor::make_binary(
    const std::vector<std::int8_t> &occupancy,
    const GridGeometry &geometry) const {
  cv::Mat binary(geometry.height, geometry.width, CV_8UC1, cv::Scalar(0));
  for (int row = 0; row < geometry.height; ++row) {
    auto *output = binary.ptr<std::uint8_t>(row);
    for (int column = 0; column < geometry.width; ++column) {
      const auto index = static_cast<std::size_t>(row) * geometry.width +
                         static_cast<std::size_t>(column);
      // Unknown (-1) and unconfirmed (0) cells remain zero. They are not
      // interpreted as free space; they are simply excluded from contours.
      if (static_cast<int>(occupancy[index]) >=
          parameters_.occupied_threshold) {
        output[column] = 255U;
      }
    }
  }
  return binary;
}

cv::Mat LineExtractor::close_small_gaps(const cv::Mat &binary,
                                        double resolution) const {
  if (parameters_.maximum_line_gap <= 0.0) {
    return binary.clone();
  }

  // A radius of ceil(gap / (2*resolution)) produces an odd kernel whose
  // diameter is approximately the allowed full gap in metres.
  const int radius = std::max(
      1, static_cast<int>(
             std::ceil(parameters_.maximum_line_gap / (2.0 * resolution))));
  const int kernel_size = 2 * radius + 1;
  const cv::Mat kernel = cv::getStructuringElement(
      cv::MORPH_RECT, cv::Size(kernel_size, kernel_size));
  cv::Mat closed;
  cv::morphologyEx(binary, closed, cv::MORPH_CLOSE, kernel);
  return closed;
}

cv::Mat LineExtractor::remove_small_components(const cv::Mat &binary) const {
  if (parameters_.minimum_component_cells <= 1) {
    return binary.clone();
  }

  cv::Mat labels;
  cv::Mat statistics;
  cv::Mat centroids;
  const int label_count = cv::connectedComponentsWithStats(
      binary, labels, statistics, centroids, 8, CV_32S);

  cv::Mat cleaned(binary.size(), CV_8UC1, cv::Scalar(0));
  for (int label = 1; label < label_count; ++label) {
    const int area = statistics.at<int>(label, cv::CC_STAT_AREA);
    if (area >= parameters_.minimum_component_cells) {
      cleaned.setTo(255U, labels == label);
    }
  }
  return cleaned;
}

std::vector<std::vector<cv::Point>>
LineExtractor::find_simplified_contours(const cv::Mat &binary,
                                        double resolution) const {
  std::vector<std::vector<cv::Point>> contours;
  cv::Mat contour_input = binary.clone();
  cv::findContours(contour_input, contours, cv::RETR_EXTERNAL,
                   cv::CHAIN_APPROX_SIMPLE);

  std::vector<std::vector<cv::Point>> simplified;
  const double epsilon_pixels = parameters_.contour_epsilon / resolution;
  for (const auto &contour : contours) {
    const double area_square_metres =
        std::abs(cv::contourArea(contour)) * resolution * resolution;
    if (area_square_metres < parameters_.min_contour_area) {
      continue;
    }

    std::vector<cv::Point> approximation;
    cv::approxPolyDP(contour, approximation, epsilon_pixels, true);
    if (approximation.size() >= 2U) {
      simplified.push_back(std::move(approximation));
    }
  }
  return simplified;
}

cv::Point2d LineExtractor::grid_cell_to_world(
    const cv::Point &cell, const GridGeometry &geometry) {
  return grid_point_to_world(
      {static_cast<double>(cell.x), static_cast<double>(cell.y)}, geometry);
}

void LineExtractor::classify_components(const cv::Mat &binary,
                                        const GridGeometry &geometry,
                                        ExtractionResult &result) const {
  cv::Mat labels;
  cv::Mat statistics;
  cv::Mat centroids;
  const int label_count = cv::connectedComponentsWithStats(
      binary, labels, statistics, centroids, 8, CV_32S);

  const double minimum_wall_length =
      std::max(parameters_.wall_min_length, parameters_.minimum_line_length);
  for (int label = 1; label < label_count; ++label) {
    const int cell_count = statistics.at<int>(label, cv::CC_STAT_AREA);
    const double area = static_cast<double>(cell_count) *
                        geometry.resolution * geometry.resolution;
    if (area < parameters_.min_contour_area) {
      continue;
    }

    std::vector<cv::Point> points;
    cv::findNonZero(labels == label, points);
    if (points.empty()) {
      continue;
    }

    const cv::RotatedRect rectangle = cv::minAreaRect(points);
    const double width_pixels = rectangle.size.width;
    const double height_pixels = rectangle.size.height;
    const double angle = static_cast<double>(rectangle.angle) * kPi / 180.0;
    const cv::Point2d width_axis{std::cos(angle), std::sin(angle)};
    const cv::Point2d height_axis{-std::sin(angle), std::cos(angle)};
    const bool width_is_long = width_pixels >= height_pixels;
    const cv::Point2d major_axis =
        width_is_long ? width_axis : height_axis;
    const cv::Point2d minor_axis =
        width_is_long ? height_axis : width_axis;
    const double long_pixels = std::max(width_pixels, height_pixels) + 1.0;
    const double short_pixels = std::min(width_pixels, height_pixels) + 1.0;
    const double long_metres = long_pixels * geometry.resolution;
    const double short_metres = short_pixels * geometry.resolution;
    const double aspect_ratio = long_metres / std::max(short_metres, 1.0e-9);
    const cv::Point2d center{rectangle.center.x, rectangle.center.y};

    if (long_metres >= minimum_wall_length &&
        aspect_ratio >= parameters_.wall_min_aspect_ratio) {
      const cv::Point2d half = major_axis * (long_pixels * 0.5);
      result.wall_lines.push_back(
          {grid_point_to_world(center - half, geometry),
           grid_point_to_world(center + half, geometry)});
      continue;
    }

    const double block_long_pixels =
        std::max(long_metres, parameters_.minimum_block_size) /
        geometry.resolution;
    const double block_short_pixels =
        std::max(short_metres, parameters_.minimum_block_size) /
        geometry.resolution;
    const cv::Point2d major_half = major_axis * (block_long_pixels * 0.5);
    const cv::Point2d minor_half = minor_axis * (block_short_pixels * 0.5);
    OrientedBlock block;
    block.corners = {
        grid_point_to_world(center - major_half - minor_half, geometry),
        grid_point_to_world(center + major_half - minor_half, geometry),
        grid_point_to_world(center + major_half + minor_half, geometry),
        grid_point_to_world(center - major_half + minor_half, geometry),
    };
    result.blocks.push_back(block);
  }
}

std::vector<LineSegment> LineExtractor::merge_wall_lines(
    std::vector<LineSegment> walls) const {
  bool merged_any = true;
  const double maximum_angle =
      parameters_.wall_merge_angle_degrees * kPi / 180.0;

  while (merged_any) {
    merged_any = false;
    for (std::size_t first = 0; first < walls.size() && !merged_any; ++first) {
      for (std::size_t second = first + 1; second < walls.size(); ++second) {
        cv::Point2d first_direction =
            normalized(walls[first].end - walls[first].start);
        cv::Point2d second_direction =
            normalized(walls[second].end - walls[second].start);
        double alignment = dot(first_direction, second_direction);
        if (alignment < 0.0) {
          second_direction *= -1.0;
          alignment *= -1.0;
        }
        alignment = std::clamp(alignment, -1.0, 1.0);
        if (std::acos(alignment) > maximum_angle) {
          continue;
        }

        const cv::Point2d axis = normalized(first_direction + second_direction);
        const cv::Point2d normal{-axis.y, axis.x};
        const cv::Point2d first_center =
            (walls[first].start + walls[first].end) * 0.5;
        const cv::Point2d second_center =
            (walls[second].start + walls[second].end) * 0.5;
        if (std::abs(dot(second_center - first_center, normal)) >
            parameters_.wall_merge_distance) {
          continue;
        }

        const double first_min =
            std::min(dot(walls[first].start, axis), dot(walls[first].end, axis));
        const double first_max =
            std::max(dot(walls[first].start, axis), dot(walls[first].end, axis));
        const double second_min = std::min(dot(walls[second].start, axis),
                                           dot(walls[second].end, axis));
        const double second_max = std::max(dot(walls[second].start, axis),
                                           dot(walls[second].end, axis));
        double gap = 0.0;
        if (first_max < second_min) {
          gap = second_min - first_max;
        } else if (second_max < first_min) {
          gap = first_min - second_max;
        }
        if (gap > parameters_.wall_merge_gap) {
          continue;
        }

        const double minimum_projection = std::min(first_min, second_min);
        const double maximum_projection = std::max(first_max, second_max);
        const double perpendicular =
            (dot(first_center, normal) + dot(second_center, normal)) * 0.5;
        walls[first] = {
            axis * minimum_projection + normal * perpendicular,
            axis * maximum_projection + normal * perpendicular,
        };
        walls.erase(walls.begin() + static_cast<std::ptrdiff_t>(second));
        merged_any = true;
        break;
      }
    }
  }
  return walls;
}

ExtractionResult LineExtractor::extract(
    const std::vector<std::int8_t> &occupancy,
    const GridGeometry &geometry) const {
  validate_geometry(geometry, occupancy.size());

  const cv::Mat binary = make_binary(occupancy, geometry);
  const cv::Mat closed = close_small_gaps(binary, geometry.resolution);
  ExtractionResult result;
  result.cleaned_binary = remove_small_components(closed);
  result.simplified_contours =
      find_simplified_contours(result.cleaned_binary, geometry.resolution);
  classify_components(result.cleaned_binary, geometry, result);
  result.wall_lines = merge_wall_lines(std::move(result.wall_lines));
  result.lines = result.wall_lines;
  for (const auto &block : result.blocks) {
    for (std::size_t index = 0; index < block.corners.size(); ++index) {
      const auto next = (index + 1U) % block.corners.size();
      result.lines.push_back({block.corners[index], block.corners[next]});
    }
  }
  return result;
}

}  // namespace static_map_line_extractor
