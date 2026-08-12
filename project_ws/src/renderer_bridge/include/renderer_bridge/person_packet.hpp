#pragma once

#include <array>
#include <cstddef>
#include <cstdint>
#include <optional>
#include <string>

namespace mapipc {

constexpr std::size_t kPersonNameBytes = 16;

struct PersonUpdate {
  std::int32_t id = 0;
  float x = 0.0F;
  float y = 0.0F;
  std::uint64_t sequence = 0;
  std::int64_t timestamp = 0;
  std::string name;
};

// Wire layout (big endian):
//   0..3 id, 4..7 x, 8..11 y, 12..19 sequence, 20..27 timestamp,
//   28..43 name (16 ASCII bytes, null-padded)
constexpr std::size_t kPersonPacketSize = 28 + kPersonNameBytes;
using PersonPacketBytes = std::array<std::uint8_t, kPersonPacketSize>;

std::optional<PersonUpdate> parsePersonPacket(const std::uint8_t *data,
                                              std::size_t size,
                                              std::string *error = nullptr);

PersonPacketBytes serializePersonPacket(const PersonUpdate &update);

} // namespace mapipc
