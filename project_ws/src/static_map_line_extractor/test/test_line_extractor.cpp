#include <algorithm>
#include <cmath>
#include <cstdint>
#include <vector>

#include <gtest/gtest.h>

#include "static_map_line_extractor/line_extractor.hpp"

namespace {

using static_map_line_extractor::ExtractionParameters;
using static_map_line_extractor::GridGeometry;
using static_map_line_extractor::LineExtractor;

std::size_t index_of(int x, int y, const GridGeometry &geometry) {
  return static_cast<std::size_t>(y) * geometry.width +
         static_cast<std::size_t>(x);
}

std::vector<std::int8_t> empty_grid(const GridGeometry &geometry) {
  return std::vector<std::int8_t>(
      static_cast<std::size_t>(geometry.width) * geometry.height, 0);
}

ExtractionParameters test_parameters() {
  ExtractionParameters parameters;
  parameters.contour_epsilon = 0.05;
  parameters.minimum_line_length = 0.20;
  parameters.maximum_line_gap = 0.0;
  parameters.minimum_component_cells = 3;
  return parameters;
}

TEST(LineExtractorTest, LongWallProducesLongSegment) {
  const GridGeometry geometry{60, 60, 0.10, -3.0, -3.0, 0.0};
  auto occupancy = empty_grid(geometry);
  for (int x = 5; x <= 54; ++x) {
    occupancy[index_of(x, 30, geometry)] = 100;
    occupancy[index_of(x, 31, geometry)] = 100;
  }

  const auto result = LineExtractor(test_parameters()).extract(occupancy, geometry);
  ASSERT_EQ(result.wall_lines.size(), 1U);
  EXPECT_TRUE(result.blocks.empty());
  EXPECT_GE(result.wall_lines.front().length(), 4.5);
}

TEST(LineExtractorTest, SquareObstacleProducesClosedOutline) {
  const GridGeometry geometry{40, 40, 0.10, -2.0, -2.0, 0.0};
  auto occupancy = empty_grid(geometry);
  for (int y = 10; y < 20; ++y) {
    for (int x = 10; x < 20; ++x) {
      occupancy[index_of(x, y, geometry)] = 100;
    }
  }

  const auto result = LineExtractor(test_parameters()).extract(occupancy, geometry);
  ASSERT_EQ(result.simplified_contours.size(), 1U);
  EXPECT_TRUE(result.wall_lines.empty());
  EXPECT_EQ(result.blocks.size(), 1U);
  EXPECT_EQ(result.lines.size(), 4U);
}

TEST(LineExtractorTest, IsolatedNoiseCellsAreRemoved) {
  const GridGeometry geometry{30, 30, 0.10, 0.0, 0.0, 0.0};
  auto occupancy = empty_grid(geometry);
  occupancy[index_of(2, 2, geometry)] = 100;
  occupancy[index_of(10, 15, geometry)] = 100;
  occupancy[index_of(25, 20, geometry)] = 100;

  const auto result = LineExtractor(test_parameters()).extract(occupancy, geometry);
  EXPECT_TRUE(result.simplified_contours.empty());
  EXPECT_TRUE(result.lines.empty());
}

TEST(LineExtractorTest, MultipleObjectsRemainIndependent) {
  const GridGeometry geometry{50, 50, 0.10, -2.5, -2.5, 0.0};
  auto occupancy = empty_grid(geometry);
  for (int y = 5; y < 12; ++y) {
    for (int x = 5; x < 15; ++x) {
      occupancy[index_of(x, y, geometry)] = 100;
    }
  }
  for (int y = 30; y < 40; ++y) {
    for (int x = 32; x < 42; ++x) {
      occupancy[index_of(x, y, geometry)] = 100;
    }
  }

  const auto result = LineExtractor(test_parameters()).extract(occupancy, geometry);
  EXPECT_EQ(result.simplified_contours.size(), 2U);
  EXPECT_EQ(result.lines.size(), 8U);
}

TEST(LineExtractorTest, GridCoordinateUsesOriginResolutionAndYaw) {
  constexpr double half_pi = 1.5707963267948966;
  const GridGeometry geometry{10, 10, 0.50, 10.0, 20.0, half_pi};
  const auto world = LineExtractor::grid_cell_to_world(cv::Point(1, 2), geometry);
  EXPECT_NEAR(world.x, 8.75, 1e-9);
  EXPECT_NEAR(world.y, 20.75, 1e-9);
}

TEST(LineExtractorTest, CollinearWallFragmentsAreMerged) {
  const GridGeometry geometry{60, 30, 0.10, 0.0, 0.0, 0.0};
  auto occupancy = empty_grid(geometry);
  for (int x = 5; x < 25; ++x) {
    occupancy[index_of(x, 14, geometry)] = 100;
    occupancy[index_of(x, 15, geometry)] = 100;
  }
  for (int x = 28; x < 50; ++x) {
    occupancy[index_of(x, 14, geometry)] = 100;
    occupancy[index_of(x, 15, geometry)] = 100;
  }

  auto parameters = test_parameters();
  parameters.wall_merge_gap = 0.50;
  const auto result = LineExtractor(parameters).extract(occupancy, geometry);
  ASSERT_EQ(result.wall_lines.size(), 1U);
  EXPECT_GT(result.wall_lines.front().length(), 4.0);
}

}  // namespace
