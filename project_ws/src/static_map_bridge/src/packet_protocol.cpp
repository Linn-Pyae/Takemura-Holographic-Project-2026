#include "static_map_bridge/packet_protocol.hpp"

#include <algorithm>
#include <cstring>
#include <limits>
#include <string>
#include <utility>

namespace static_map_bridge
{
namespace
{

static_assert(sizeof(float) == 4, "Protocol requires 32-bit float");
static_assert(std::numeric_limits<float>::is_iec559, "Protocol requires IEEE-754 float");

void append_u16(std::vector<std::uint8_t> & output, std::uint16_t value)
{
  output.push_back(static_cast<std::uint8_t>(value & 0xffU));
  output.push_back(static_cast<std::uint8_t>((value >> 8U) & 0xffU));
}

void append_u32(std::vector<std::uint8_t> & output, std::uint32_t value)
{
  for (unsigned shift = 0; shift < 32; shift += 8) {
    output.push_back(static_cast<std::uint8_t>((value >> shift) & 0xffU));
  }
}

void append_u64(std::vector<std::uint8_t> & output, std::uint64_t value)
{
  for (unsigned shift = 0; shift < 64; shift += 8) {
    output.push_back(static_cast<std::uint8_t>((value >> shift) & 0xffU));
  }
}

void append_f32(std::vector<std::uint8_t> & output, float value)
{
  std::uint32_t bits = 0;
  std::memcpy(&bits, &value, sizeof(bits));
  append_u32(output, bits);
}

std::uint16_t read_u16(const std::vector<std::uint8_t> & input, std::size_t offset)
{
  return static_cast<std::uint16_t>(input[offset]) |
         static_cast<std::uint16_t>(input[offset + 1]) << 8U;
}

std::uint32_t read_u32(const std::vector<std::uint8_t> & input, std::size_t offset)
{
  std::uint32_t value = 0;
  for (unsigned byte = 0; byte < 4; ++byte) {
    value |= static_cast<std::uint32_t>(input[offset + byte]) << (byte * 8U);
  }
  return value;
}

std::uint64_t read_u64(const std::vector<std::uint8_t> & input, std::size_t offset)
{
  std::uint64_t value = 0;
  for (unsigned byte = 0; byte < 8; ++byte) {
    value |= static_cast<std::uint64_t>(input[offset + byte]) << (byte * 8U);
  }
  return value;
}

float read_f32(const std::vector<std::uint8_t> & input, std::size_t offset)
{
  const std::uint32_t bits = read_u32(input, offset);
  float value = 0.0F;
  std::memcpy(&value, &bits, sizeof(value));
  return value;
}

}  // namespace

std::size_t max_segments_per_packet(std::size_t max_datagram_bytes)
{
  if (max_datagram_bytes < kHeaderBytes + kSegmentBytes) {
    throw ProtocolError("max_datagram_bytes must fit the header and at least one segment");
  }
  return (max_datagram_bytes - kHeaderBytes) / kSegmentBytes;
}

std::vector<std::uint8_t> serialize_packet(
  std::uint64_t map_sequence,
  std::uint32_t packet_index,
  std::uint32_t packet_count,
  const std::vector<LineSegment> & segments)
{
  if (packet_count == 0U) {
    throw ProtocolError("packet_count must be greater than zero");
  }
  if (packet_index >= packet_count) {
    throw ProtocolError("packet_index must be smaller than packet_count");
  }
  if (segments.size() > std::numeric_limits<std::uint32_t>::max()) {
    throw ProtocolError("segment count exceeds protocol range");
  }

  std::vector<std::uint8_t> output;
  output.reserve(kHeaderBytes + segments.size() * kSegmentBytes);
  output.insert(output.end(), kMagic.begin(), kMagic.end());
  append_u16(output, kProtocolVersion);
  append_u16(output, kHeaderBytes);
  append_u64(output, map_sequence);
  append_u32(output, packet_index);
  append_u32(output, packet_count);
  append_u32(output, static_cast<std::uint32_t>(segments.size()));

  for (const auto & segment : segments) {
    append_f32(output, segment.x1);
    append_f32(output, segment.y1);
    append_f32(output, segment.x2);
    append_f32(output, segment.y2);
  }
  return output;
}

DecodedPacket deserialize_packet(const std::vector<std::uint8_t> & bytes)
{
  if (bytes.size() < kHeaderBytes) {
    throw ProtocolError("truncated packet header");
  }
  if (!std::equal(kMagic.begin(), kMagic.end(), bytes.begin())) {
    throw ProtocolError("invalid magic number");
  }
  if (read_u16(bytes, 4) != kProtocolVersion) {
    throw ProtocolError("unsupported protocol version");
  }
  if (read_u16(bytes, 6) != kHeaderBytes) {
    throw ProtocolError("invalid header size");
  }

  DecodedPacket decoded;
  decoded.map_sequence = read_u64(bytes, 8);
  decoded.packet_index = read_u32(bytes, 16);
  decoded.packet_count = read_u32(bytes, 20);
  const std::uint32_t segment_count = read_u32(bytes, 24);

  if (decoded.packet_count == 0U) {
    throw ProtocolError("packet_count must be greater than zero");
  }
  if (decoded.packet_index >= decoded.packet_count) {
    throw ProtocolError("packet_index is outside packet_count");
  }
  if constexpr (sizeof(std::size_t) <= sizeof(std::uint32_t)) {
    const auto maximum_segments =
      (std::numeric_limits<std::size_t>::max() - kHeaderBytes) / kSegmentBytes;
    if (static_cast<std::size_t>(segment_count) > maximum_segments) {
      throw ProtocolError("segment count overflows packet length");
    }
  }
  const std::size_t expected_size =
    kHeaderBytes + static_cast<std::size_t>(segment_count) * kSegmentBytes;
  if (bytes.size() != expected_size) {
    throw ProtocolError(
            bytes.size() < expected_size ? "truncated segment data" : "unexpected trailing data");
  }

  decoded.segments.reserve(segment_count);
  std::size_t offset = kHeaderBytes;
  for (std::uint32_t i = 0; i < segment_count; ++i) {
    decoded.segments.push_back(LineSegment{
      read_f32(bytes, offset),
      read_f32(bytes, offset + 4),
      read_f32(bytes, offset + 8),
      read_f32(bytes, offset + 12)});
    offset += kSegmentBytes;
  }
  return decoded;
}

std::vector<std::vector<std::uint8_t>> packetize_segments(
  const std::vector<LineSegment> & segments,
  std::uint64_t map_sequence,
  std::size_t max_datagram_bytes)
{
  const std::size_t capacity = max_segments_per_packet(max_datagram_bytes);
  const std::size_t packet_count_size =
    segments.empty() ? 1U : (segments.size() + capacity - 1U) / capacity;
  if (packet_count_size > std::numeric_limits<std::uint32_t>::max()) {
    throw ProtocolError("packet count exceeds protocol range");
  }
  const auto packet_count = static_cast<std::uint32_t>(packet_count_size);

  std::vector<std::vector<std::uint8_t>> packets;
  packets.reserve(packet_count_size);
  for (std::uint32_t index = 0; index < packet_count; ++index) {
    const std::size_t begin = static_cast<std::size_t>(index) * capacity;
    const std::size_t end = std::min(begin + capacity, segments.size());
    std::vector<LineSegment> chunk;
    if (begin < end) {
      chunk.assign(segments.begin() + static_cast<std::ptrdiff_t>(begin),
        segments.begin() + static_cast<std::ptrdiff_t>(end));
    }
    auto packet = serialize_packet(map_sequence, index, packet_count, chunk);
    if (packet.size() > max_datagram_bytes) {
      throw ProtocolError("serialized packet exceeds max_datagram_bytes");
    }
    packets.push_back(std::move(packet));
  }
  return packets;
}

}  // namespace static_map_bridge
