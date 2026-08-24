#include "static_map_bridge/packet_protocol.hpp"

#include <algorithm>
#include <cmath>
#include <cstddef>
#include <cstdint>
#include <fstream>
#include <iostream>
#include <sstream>
#include <stdexcept>
#include <string>
#include <vector>

namespace
{

using static_map_bridge::LineSegment;

void require(bool condition, const std::string & message)
{
  if (!condition) {
    throw std::runtime_error(message);
  }
}

bool nearly_equal(float left, float right)
{
  return std::fabs(left - right) <= 1.0e-6F;
}

void require_equal(const LineSegment & actual, const LineSegment & expected)
{
  require(nearly_equal(actual.x1, expected.x1), "x1 mismatch");
  require(nearly_equal(actual.y1, expected.y1), "y1 mismatch");
  require(nearly_equal(actual.x2, expected.x2), "x2 mismatch");
  require(nearly_equal(actual.y2, expected.y2), "y2 mismatch");
}

template<typename Function>
void require_protocol_error(Function function, const std::string & message)
{
  try {
    function();
  } catch (const static_map_bridge::ProtocolError &) {
    return;
  }
  throw std::runtime_error(message);
}

std::vector<LineSegment> reconstruct(
  const std::vector<std::vector<std::uint8_t>> & packets,
  std::uint64_t expected_sequence)
{
  std::vector<LineSegment> result;
  for (std::size_t index = 0; index < packets.size(); ++index) {
    const auto decoded = static_map_bridge::deserialize_packet(packets[index]);
    require(decoded.map_sequence == expected_sequence, "sequence mismatch");
    require(decoded.packet_index == index, "packet index mismatch");
    require(decoded.packet_count == packets.size(), "packet count mismatch");
    result.insert(result.end(), decoded.segments.begin(), decoded.segments.end());
  }
  return result;
}

void test_one_segment()
{
  const std::vector<LineSegment> expected{{1.25F, -2.5F, 3.75F, 4.5F}};
  const auto packets = static_map_bridge::packetize_segments(expected, 42);
  require(packets.size() == 1, "one segment should fit one packet");
  const auto actual = reconstruct(packets, 42);
  require(actual.size() == expected.size(), "one-segment size mismatch");
  require_equal(actual[0], expected[0]);
}

void test_multiple_segments()
{
  const std::vector<LineSegment> expected{
    {0.0F, 0.0F, 1.0F, 0.0F},
    {-1.0F, 2.0F, 3.0F, -4.0F},
    {9.25F, -8.5F, 7.75F, 6.125F}};
  const auto packets = static_map_bridge::packetize_segments(expected, 1001);
  const auto actual = reconstruct(packets, 1001);
  require(actual.size() == expected.size(), "multiple-segment size mismatch");
  for (std::size_t i = 0; i < expected.size(); ++i) {
    require_equal(actual[i], expected[i]);
  }
}

void test_large_split_and_headers()
{
  std::vector<LineSegment> expected;
  for (std::size_t i = 0; i < 1200; ++i) {
    const float value = static_cast<float>(i) * 0.01F;
    expected.push_back({value, -value, value + 0.5F, -value - 0.25F});
  }
  const auto packets = static_map_bridge::packetize_segments(expected, 987654321ULL);
  require(static_map_bridge::max_segments_per_packet(8192) == 510, "default capacity mismatch");
  require(packets.size() == 3, "1200 segments should be split into three packets");
  require(packets[0].size() == 8188, "full packet byte size mismatch");
  const auto actual = reconstruct(packets, 987654321ULL);
  require(actual.size() == expected.size(), "split reconstruction size mismatch");
  for (std::size_t i = 0; i < expected.size(); ++i) {
    require_equal(actual[i], expected[i]);
  }
}

void test_empty_map()
{
  const auto packets = static_map_bridge::packetize_segments({}, 77);
  require(packets.size() == 1, "empty map requires one clear packet");
  require(packets[0].size() == static_map_bridge::kHeaderBytes, "empty packet size mismatch");
  const auto decoded = static_map_bridge::deserialize_packet(packets[0]);
  require(decoded.map_sequence == 77 && decoded.segments.empty(), "empty map decode mismatch");
}

void test_invalid_inputs()
{
  const std::vector<LineSegment> segments{{1.0F, 2.0F, 3.0F, 4.0F}};
  const auto valid = static_map_bridge::packetize_segments(segments, 9)[0];

  auto bad_magic = valid;
  bad_magic[0] = 'X';
  require_protocol_error(
    [&]() {static_map_bridge::deserialize_packet(bad_magic);},
    "invalid magic was accepted");

  auto bad_version = valid;
  bad_version[4] = 2;
  bad_version[5] = 0;
  require_protocol_error(
    [&]() {static_map_bridge::deserialize_packet(bad_version);},
    "invalid protocol version was accepted");

  auto truncated = valid;
  truncated.pop_back();
  require_protocol_error(
    [&]() {static_map_bridge::deserialize_packet(truncated);},
    "truncated packet was accepted");
}

std::vector<LineSegment> read_csv(const std::string & path)
{
  std::ifstream input(path);
  require(input.good(), "cannot open real-data CSV: " + path);
  std::vector<LineSegment> segments;
  std::string line;
  bool first_line = true;
  while (std::getline(input, line)) {
    if (line.empty()) {
      continue;
    }
    if (first_line) {
      first_line = false;
      if (line.find("x1") != std::string::npos) {
        continue;
      }
    }
    std::replace(line.begin(), line.end(), ',', ' ');
    std::istringstream row(line);
    LineSegment segment;
    require(
      static_cast<bool>(row >> segment.x1 >> segment.y1 >> segment.x2 >> segment.y2),
      "invalid real-data CSV row in " + path);
    segments.push_back(segment);
  }
  return segments;
}

void test_real_data_csv(const std::string & path, std::uint64_t sequence)
{
  const auto expected = read_csv(path);
  const auto packets = static_map_bridge::packetize_segments(expected, sequence);
  const auto actual = reconstruct(packets, sequence);
  require(actual.size() == expected.size(), "real-data reconstruction size mismatch");
  for (std::size_t i = 0; i < expected.size(); ++i) {
    require_equal(actual[i], expected[i]);
  }
  std::size_t bytes = 0;
  for (const auto & packet : packets) {
    bytes += packet.size();
  }
  std::cout << "REAL_DATA " << path << " segments=" << expected.size()
            << " packets=" << packets.size() << " bytes=" << bytes
            << " roundtrip=OK\n";
}

}  // namespace

int main(int argc, char ** argv)
{
  try {
    test_one_segment();
    test_multiple_segments();
    test_large_split_and_headers();
    test_empty_map();
    test_invalid_inputs();
    std::cout << "UNIT_TESTS 1-line, multi-line, split, sequence/index/count, empty-map, "
                 "bad-magic, bad-version, truncated: OK\n";
    for (int index = 1; index < argc; ++index) {
      test_real_data_csv(argv[index], 2000U + static_cast<std::uint64_t>(index));
    }
    std::cout << "ALL_PACKET_PROTOCOL_TESTS: OK\n";
    return 0;
  } catch (const std::exception & error) {
    std::cerr << "TEST_FAILURE: " << error.what() << '\n';
    return 1;
  }
}
