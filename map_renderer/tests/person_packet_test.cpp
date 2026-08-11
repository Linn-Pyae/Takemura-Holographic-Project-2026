#include "person_packet.hpp"

#include <cassert>
#include <cmath>
#include <cstdint>
#include <limits>
#include <string>

int main() {
    const mapipc::PersonUpdate original{
        42, 123.5F, -9.25F, 0x0102'0304'0506'0708ULL,
        1'700'000'000'123'456'789LL};
    const mapipc::PersonPacketBytes bytes =
        mapipc::serializePersonPacket(original);

    // Sequence is serialized in big-endian order at offsets 12..19.
    assert(bytes[12] == 0x01);
    assert(bytes[13] == 0x02);
    assert(bytes[18] == 0x07);
    assert(bytes[19] == 0x08);

    std::string error;
    const auto parsed =
        mapipc::parsePersonPacket(bytes.data(), bytes.size(), &error);
    assert(parsed.has_value());
    assert(error.empty());
    assert(parsed->id == original.id);
    assert(parsed->x == original.x);
    assert(parsed->y == original.y);
    assert(parsed->sequence == original.sequence);
    assert(parsed->timestamp == original.timestamp);

    assert(!mapipc::parsePersonPacket(bytes.data(), bytes.size() - 1, &error));
    assert(!error.empty());

    const mapipc::PersonUpdate invalid{
        1, std::numeric_limits<float>::quiet_NaN(), 2.0F, 3, 4};
    const auto invalid_bytes = mapipc::serializePersonPacket(invalid);
    assert(!mapipc::parsePersonPacket(
        invalid_bytes.data(), invalid_bytes.size(), &error));

    return 0;
}
