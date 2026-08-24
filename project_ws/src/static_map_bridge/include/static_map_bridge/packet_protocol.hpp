#pragma once

#include <array>
#include <cstddef>
#include <cstdint>
#include <stdexcept>
#include <vector>

namespace static_map_bridge
{

// The wire representation is explicitly little-endian and does not depend on
// ROS types or compiler struct packing.
inline constexpr std::array<std::uint8_t, 4> kMagic{{'T', 'S', 'M', 'P'}};
inline constexpr std::uint16_t kProtocolVersion = 1;
inline constexpr std::uint16_t kHeaderBytes = 28;
inline constexpr std::size_t kSegmentBytes = 16;
inline constexpr std::size_t kDefaultMaxDatagramBytes = 8192;

struct LineSegment
{
  float x1{};
  float y1{};
  float x2{};
  float y2{};
};

struct DecodedPacket
{
  std::uint64_t map_sequence{};
  std::uint32_t packet_index{};
  std::uint32_t packet_count{};
  std::vector<LineSegment> segments;
};

class ProtocolError : public std::runtime_error
{
public:
  using std::runtime_error::runtime_error;
};

std::size_t max_segments_per_packet(std::size_t max_datagram_bytes);

std::vector<std::uint8_t> serialize_packet(
  std::uint64_t map_sequence,
  std::uint32_t packet_index,
  std::uint32_t packet_count,
  const std::vector<LineSegment> & segments);

DecodedPacket deserialize_packet(const std::vector<std::uint8_t> & bytes);

// A map with no segments is represented by one header-only packet. This lets a
// future receiver atomically clear an old map after receiving the new sequence.
std::vector<std::vector<std::uint8_t>> packetize_segments(
  const std::vector<LineSegment> & segments,
  std::uint64_t map_sequence,
  std::size_t max_datagram_bytes = kDefaultMaxDatagramBytes);

}  // namespace static_map_bridge
