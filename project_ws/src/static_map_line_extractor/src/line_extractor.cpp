#include "static_map_line_extractor/line_extractor.hpp"

#include <algorithm>
#include <cmath>
#include <limits>
#include <stdexcept>

#include <opencv2/imgproc.hpp>

namespace static_map_line_extractor {

namespace {

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
  const double local_x =
      (static_cast<double>(cell.x) + 0.5) * geometry.resolution;
  const double local_y =
      (static_cast<double>(cell.y) + 0.5) * geometry.resolution;
  const double cosine = std::cos(geometry.origin_yaw);
  const double sine = std::sin(geometry.origin_yaw);
  return cv::Point2d(
      geometry.origin_x + cosine * local_x - sine * local_y,
      geometry.origin_y + sine * local_x + cosine * local_y);
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

  for (const auto &contour : result.simplified_contours) {
    if (contour.size() == 2U) {
      LineSegment line{grid_cell_to_world(contour[0], geometry),
                       grid_cell_to_world(contour[1], geometry)};
      if (line.length() >= parameters_.minimum_line_length) {
        result.lines.push_back(line);
      }
      continue;
    }

    for (std::size_t index = 0; index < contour.size(); ++index) {
      const auto next = (index + 1U) % contour.size();
      LineSegment line{grid_cell_to_world(contour[index], geometry),
                       grid_cell_to_world(contour[next], geometry)};
      if (line.length() >= parameters_.minimum_line_length) {
        result.lines.push_back(line);
      }
    }
  }
  return result;
}

}  // namespace static_map_line_extractor
