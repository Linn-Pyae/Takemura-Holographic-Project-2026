#pragma once

#include <array>
#include <cstddef>
#include <cstdint>
#include <optional>
#include <string>

namespace mapipc {

// The value used by the renderer after a Unix datagram has been parsed.
struct PersonUpdate {
    std::int32_t id = 0;
    float x = 0.0F;
    float y = 0.0F;
    // Monotonically increasing value assigned by the sender. The receiver uses
    // this, rather than arrival order, to reject stale updates.
    std::uint64_t sequence = 0;
    // Observation time supplied by the sender. This is metadata and is not
    // used to decide which update is newest.
    std::int64_t timestamp = 0;
};

// Wire layout, in network byte order (big endian):
//   bytes  0..3  id
//   bytes  4..7  x (IEEE-754 binary32 bits)
//   bytes  8..11 y (IEEE-754 binary32 bits)
//   bytes 12..19 sequence
//   bytes 20..27 timestamp
constexpr std::size_t kPersonPacketSize = 28;
using PersonPacketBytes = std::array<std::uint8_t, kPersonPacketSize>;

// Parses exactly one packet. Invalid size, NaN, and infinite coordinates fail.
std::optional<PersonUpdate> parsePersonPacket(
    const std::uint8_t* data,
    std::size_t size,
    std::string* error = nullptr);

// Useful for a C++ sender and for tests. It produces the format above.
PersonPacketBytes serializePersonPacket(const PersonUpdate& update);

}  // namespace mapipc
