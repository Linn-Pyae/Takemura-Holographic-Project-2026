#include "protocol.h"

#include <stdio.h>
#include <string.h>

#define FRAME_SIZE 26

static char last_error[256] = "unknown protocol error";

static uint16_t read_le16(const uint8_t *data)
{
    return (uint16_t)data[0] | ((uint16_t)data[1] << 8);
}

static uint32_t read_le32(const uint8_t *data)
{
    return (uint32_t)data[0] |
           ((uint32_t)data[1] << 8) |
           ((uint32_t)data[2] << 16) |
           ((uint32_t)data[3] << 24);
}

static void write_le16(uint8_t *data, uint16_t value)
{
    data[0] = (uint8_t)(value & 0xffu);
    data[1] = (uint8_t)(value >> 8);
}

static void write_le32(uint8_t *data, uint32_t value)
{
    data[0] = (uint8_t)(value & 0xffu);
    data[1] = (uint8_t)((value >> 8) & 0xffu);
    data[2] = (uint8_t)((value >> 16) & 0xffu);
    data[3] = (uint8_t)((value >> 24) & 0xffu);
}

static uint16_t xor_words(const uint8_t *frame, int first, int last)
{
    uint16_t value = 0;
    for (int offset = first; offset <= last; offset += 2) {
        value ^= read_le16(frame + offset);
    }
    return value;
}

static void set_checksums(uint8_t *frame)
{
    write_le16(frame + 14, xor_words(frame, 4, 12));
    write_le16(frame + 24, xor_words(frame, 16, 22));
}

static int response_is_valid(const uint8_t *frame, uint8_t device_type,
                             uint8_t address)
{
    return read_le16(frame + 24) == xor_words(frame, 16, 22) &&
           frame[4] == device_type && frame[7] == 0x00 &&
           frame[16] == address;
}

static void make_request(uint8_t *frame, uint8_t device_type,
                         uint8_t device_id, uint8_t address)
{
    static const uint8_t template_frame[FRAME_SIZE] = {
        0xF0, 0xA5, 0x5A, 0x0F,
        0x81, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x08, 0x00,
        0x00, 0x00, 0xCC, 0xCC,
        0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0xCC, 0xCC
    };

    memcpy(frame, template_frame, sizeof(template_frame));
    frame[4] = device_type;
    frame[6] = device_id;
    write_le32(frame + 16, address);
    set_checksums(frame);
}

static int read_response(ms_protocol *protocol, uint8_t device_type,
                         uint8_t address, uint8_t *frame, int timeout_ms)
{
    size_t received = 0;
    uint64_t deadline = ms_monotonic_ms() + (uint64_t)timeout_ms;

    while (ms_monotonic_ms() < deadline) {
        uint64_t now = ms_monotonic_ms();
        int remaining = now < deadline ? (int)(deadline - now) : 0;
        uint8_t chunk[128];
        int count = ms_serial_read(protocol->serial, chunk, sizeof(chunk),
                                   remaining > 0 ? remaining : 1);
        if (count < 0) {
            snprintf(last_error, sizeof(last_error), "%s", ms_serial_error());
            return -1;
        }
        if (count == 0) {
            break;
        }

        for (int index = 0; index < count; index++) {
            uint8_t byte = chunk[index];
            if (received == 0 && byte != 0xF0) {
                continue;
            }
            frame[received++] = byte;

            if ((received == 2 && frame[1] != 0xA5) ||
                (received == 3 && frame[2] != 0x5A) ||
                (received == 4 && frame[3] != 0x0F)) {
                received = byte == 0xF0 ? 1u : 0u;
                if (received == 1u) {
                    frame[0] = byte;
                }
                continue;
            }

            if (received != FRAME_SIZE) {
                continue;
            }
            if (response_is_valid(frame, device_type, address)) {
                return 0;
            }
            received = 0;
        }
    }

    snprintf(last_error, sizeof(last_error),
             "device did not respond before the timeout");
    return -1;
}

void ms_protocol_init(ms_protocol *protocol, ms_serial_port *serial)
{
    protocol->serial = serial;
}

int ms_protocol_read(ms_protocol *protocol, uint8_t device_type,
                     uint8_t device_id, uint8_t address,
                     uint32_t *value, int timeout_ms)
{
    uint8_t request[FRAME_SIZE];
    uint8_t response[FRAME_SIZE];

    make_request(request, device_type, device_id, address);
    if (ms_serial_write_all(protocol->serial, request, sizeof(request)) != 0 ||
        ms_serial_drain(protocol->serial) != 0) {
        snprintf(last_error, sizeof(last_error), "%s", ms_serial_error());
        return -1;
    }
    if (read_response(protocol, device_type, address, response, timeout_ms) != 0) {
        return -1;
    }
    *value = read_le32(response + 20);
    return 0;
}

int ms_protocol_write(ms_protocol *protocol, uint8_t device_type,
                      uint8_t device_id, uint8_t address, uint32_t value)
{
    uint8_t frame[FRAME_SIZE];
    make_request(frame, device_type, device_id, address);
    frame[8] = 2;
    write_le32(frame + 20, value);
    set_checksums(frame);

    if (ms_serial_write_all(protocol->serial, frame, sizeof(frame)) != 0 ||
        ms_serial_drain(protocol->serial) != 0) {
        snprintf(last_error, sizeof(last_error), "%s", ms_serial_error());
        return -1;
    }
    ms_sleep_ms(20);
    return 0;
}

const char *ms_protocol_error(void)
{
    return last_error;
}
