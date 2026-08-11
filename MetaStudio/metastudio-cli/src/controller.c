#include "controller.h"

#include <stdio.h>

static char last_error[256] = "unknown controller error";

static int read_crop(ms_controller *controller, ms_crop *crop)
{
    uint32_t x;
    uint32_t y;
    uint32_t size;

    if (ms_protocol_read(&controller->protocol, 0x81, controller->device_id,
                         32, &x, controller->timeout_ms) != 0 ||
        ms_protocol_read(&controller->protocol, 0x81, controller->device_id,
                         33, &y, controller->timeout_ms) != 0 ||
        ms_protocol_read(&controller->protocol, 0x81, controller->device_id,
                         34, &size, controller->timeout_ms) != 0) {
        snprintf(last_error, sizeof(last_error),
                 "could not read crop settings: %s", ms_protocol_error());
        return -1;
    }

    crop->x = (int32_t)x;
    crop->y = (int32_t)y;
    crop->width = (uint16_t)(size >> 16);
    crop->height = (uint16_t)(size & 0xffffu);
    return 0;
}

static int write_registers(ms_controller *controller, uint8_t device_type,
                           const uint8_t *addresses, const uint32_t *values,
                           size_t count)
{
    for (size_t index = 0; index < count; index++) {
        if (ms_protocol_write(&controller->protocol, device_type, 0,
                              addresses[index], values[index]) != 0) {
            snprintf(last_error, sizeof(last_error),
                     "could not update device settings: %s",
                     ms_protocol_error());
            return -1;
        }
    }
    return 0;
}

static int reset_input_stage(ms_controller *controller,
                             uint16_t width, uint16_t height)
{
    uint16_t scale_x = (uint16_t)(((uint32_t)width + 1u) * 2048u / 1025u);
    uint16_t scale_y = (uint16_t)(((uint32_t)height + 1u) * 2048u / 1025u);
    const uint8_t addresses[] = { 5, 6, 7, 11, 17, 17, 17, 16 };
    const uint32_t values[] = {
        0,
        0,
        ((uint32_t)width << 16) | height,
        ((uint32_t)scale_x << 16) | scale_y,
        0, 1, 0, 0
    };

    if (write_registers(controller, 0x01, addresses, values,
                        sizeof(addresses) / sizeof(addresses[0])) != 0) {
        return -1;
    }

    (void)ms_serial_flush_input(controller->protocol.serial);
    uint32_t x;
    uint32_t y;
    uint32_t size;
    if (ms_protocol_read(&controller->protocol, 0x01, 0, 5, &x,
                         controller->timeout_ms) != 0 ||
        ms_protocol_read(&controller->protocol, 0x01, 0, 6, &y,
                         controller->timeout_ms) != 0 ||
        ms_protocol_read(&controller->protocol, 0x01, 0, 7, &size,
                         controller->timeout_ms) != 0 ||
        x != 0 || y != 0 ||
        (uint16_t)(size >> 16) != width ||
        (uint16_t)(size & 0xffffu) != height) {
        snprintf(last_error, sizeof(last_error),
                 "device did not apply the input settings");
        return -1;
    }
    return 0;
}

static int write_crop(ms_controller *controller, const ms_crop *crop)
{
    uint16_t scale_x = (uint16_t)((uint32_t)crop->width * 2048u / 1024u);
    uint16_t scale_y = (uint16_t)((uint32_t)crop->height * 2048u / 1024u);
    const uint8_t addresses[] = { 32, 33, 239, 34, 37, 43, 43, 43, 42 };
    const uint32_t values[] = {
        (uint32_t)crop->x,
        (uint32_t)crop->y,
        ((uint32_t)crop->x << 16) | (uint16_t)crop->y,
        ((uint32_t)crop->width << 16) | crop->height,
        ((uint32_t)scale_x << 16) | scale_y,
        0, 1, 0, 0
    };

    return write_registers(controller, 0x81, addresses, values,
                           sizeof(addresses) / sizeof(addresses[0]));
}

void ms_controller_init(ms_controller *controller, ms_serial_port *serial,
                        uint8_t device_id, int timeout_ms)
{
    ms_protocol_init(&controller->protocol, serial);
    controller->device_id = device_id;
    controller->timeout_ms = timeout_ms;
}

int ms_controller_read_state(ms_controller *controller,
                             ms_controller_state *state)
{
    uint32_t resolution;
    if (ms_protocol_read(&controller->protocol, 0x01, 0, 10, &resolution,
                         controller->timeout_ms) != 0) {
        snprintf(last_error, sizeof(last_error),
                 "could not read input resolution: %s", ms_protocol_error());
        return -1;
    }

    state->input_width = (uint16_t)(resolution >> 16);
    state->input_height = (uint16_t)(resolution & 0xffffu);
    if (state->input_width == 0) {
        state->input_width = 1920;
    }
    if (state->input_height == 0) {
        state->input_height = 1080;
    }
    return read_crop(controller, &state->crop);
}

int ms_crop_is_valid(const ms_crop *crop,
                     uint16_t input_width, uint16_t input_height)
{
    return crop->x >= 0 && crop->y >= 0 &&
           crop->width >= 32 && crop->height >= 32 &&
           (uint32_t)crop->x + crop->width <= input_width &&
           (uint32_t)crop->y + crop->height <= input_height;
}

int ms_controller_apply_crop(ms_controller *controller,
                             uint16_t input_width, uint16_t input_height,
                             const ms_crop *crop, int reset_stage)
{
    if (!ms_crop_is_valid(crop, input_width, input_height)) {
        snprintf(last_error, sizeof(last_error),
                 "crop is outside the input bounds");
        return -1;
    }
    if (reset_stage &&
        reset_input_stage(controller, input_width, input_height) != 0) {
        return -1;
    }
    if (write_crop(controller, crop) != 0) {
        return -1;
    }

    (void)ms_serial_flush_input(controller->protocol.serial);
    ms_crop actual;
    if (read_crop(controller, &actual) != 0) {
        return -1;
    }
    if (actual.x != crop->x || actual.y != crop->y ||
        actual.width != crop->width || actual.height != crop->height) {
        snprintf(last_error, sizeof(last_error),
                 "device did not apply the requested crop");
        return -1;
    }
    return 0;
}

const char *ms_controller_error(void)
{
    return last_error;
}
