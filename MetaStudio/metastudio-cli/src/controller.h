#ifndef METASTUDIO_CONTROLLER_H
#define METASTUDIO_CONTROLLER_H

#include "protocol.h"

#include <stdint.h>

typedef struct {
    int32_t x;
    int32_t y;
    uint16_t width;
    uint16_t height;
} ms_crop;

typedef struct {
    uint16_t input_width;
    uint16_t input_height;
    ms_crop crop;
} ms_controller_state;

typedef struct {
    ms_protocol protocol;
    uint8_t device_id;
    int timeout_ms;
} ms_controller;

void ms_controller_init(ms_controller *controller, ms_serial_port *serial,
                        uint8_t device_id, int timeout_ms);
int ms_controller_read_state(ms_controller *controller,
                             ms_controller_state *state);
int ms_controller_apply_crop(ms_controller *controller,
                             uint16_t input_width, uint16_t input_height,
                             const ms_crop *crop, int reset_input_stage);
int ms_crop_is_valid(const ms_crop *crop,
                     uint16_t input_width, uint16_t input_height);
const char *ms_controller_error(void);

#endif
