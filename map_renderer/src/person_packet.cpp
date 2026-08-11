#include "person_packet.hpp"

#include <cmath>
#include <cstring>
#include <type_traits>

namespace mapipc {
namespace {

static_assert(sizeof(float) == 4, "This protocol requires 32-bit floats");
static_assert(std::is_trivially_copyable<float>::value,
              "Float must be trivially copyable");

std::uint32_t readU32BigEndian(const std::uint8_t* bytes) {
    return (static_cast<std::uint32_t>(bytes[0]) << 24U) |
           (static_cast<std::uint32_t>(bytes[1]) << 16U) |
           (static_cast<std::uint32_t>(bytes[2]) << 8U) |
           static_cast<std::uint32_t>(bytes[3]);
}

void writeU32BigEndian(std::uint8_t* bytes, std::uint32_t value) {
    bytes[0] = static_cast<std::uint8_t>(value >> 24U);
    bytes[1] = static_cast<std::uint8_t>(value >> 16U);
    bytes[2] = static_cast<std::uint8_t>(value >> 8U);
    bytes[3] = static_cast<std::uint8_t>(value);
}

std::uint64_t readU64BigEndian(const std::uint8_t* bytes) {
    return (static_cast<std::uint64_t>(readU32BigEndian(bytes)) << 32U) |
           static_cast<std::uint64_t>(readU32BigEndian(bytes + 4));
}

void writeU64BigEndian(std::uint8_t* bytes, std::uint64_t value) {
    writeU32BigEndian(bytes, static_cast<std::uint32_t>(value >> 32U));
    writeU32BigEndian(bytes + 4, static_cast<std::uint32_t>(value));
}

std::int32_t decodeI32(const std::uint8_t* bytes) {
    const std::uint32_t bits = readU32BigEndian(bytes);
    std::int32_t value = 0;
    std::memcpy(&value, &bits, sizeof(value));
    return value;
}

float decodeFloat(const std::uint8_t* bytes) {
    const std::uint32_t bits = readU32BigEndian(bytes);
    float value = 0.0F;
    std::memcpy(&value, &bits, sizeof(value));
    return value;
}

std::int64_t decodeI64(const std::uint8_t* bytes) {
    const std::uint64_t bits = readU64BigEndian(bytes);
    std::int64_t value = 0;
    std::memcpy(&value, &bits, sizeof(value));
    return value;
}

std::uint32_t bitsOf(std::int32_t value) {
    std::uint32_t bits = 0;
    std::memcpy(&bits, &value, sizeof(bits));
    return bits;
}

std::uint32_t bitsOf(float value) {
    std::uint32_t bits = 0;
    std::memcpy(&bits, &value, sizeof(bits));
    return bits;
}

std::uint64_t bitsOf(std::int64_t value) {
    std::uint64_t bits = 0;
    std::memcpy(&bits, &value, sizeof(bits));
    return bits;
}

void setError(std::string* error, const std::string& message) {
    if (error != nullptr) {
        *error = message;
    }
}

}  // namespace

std::optional<PersonUpdate> parsePersonPacket(
    const std::uint8_t* data,
    std::size_t size,
    std::string* error) {
    if (data == nullptr) {
        setError(error, "packet data is null");
        return std::nullopt;
    }
    if (size != kPersonPacketSize) {
        setError(error, "packet must contain exactly 28 bytes");
        return std::nullopt;
    }

    PersonUpdate update;
    update.id = decodeI32(data);
    update.x = decodeFloat(data + 4);
    update.y = decodeFloat(data + 8);
    update.sequence = readU64BigEndian(data + 12);
    update.timestamp = decodeI64(data + 20);

    if (!std::isfinite(update.x) || !std::isfinite(update.y)) {
        setError(error, "x and y must be finite numbers");
        return std::nullopt;
    }

    if (error != nullptr) {
        error->clear();
    }
    return update;
}

PersonPacketBytes serializePersonPacket(const PersonUpdate& update) {
    PersonPacketBytes bytes{};
    writeU32BigEndian(bytes.data(), bitsOf(update.id));
    writeU32BigEndian(bytes.data() + 4, bitsOf(update.x));
    writeU32BigEndian(bytes.data() + 8, bitsOf(update.y));
    writeU64BigEndian(bytes.data() + 12, update.sequence);
    writeU64BigEndian(bytes.data() + 20, bitsOf(update.timestamp));
    return bytes;
}

}  // namespace mapipc
