#ifndef METASTUDIO_PROTOCOL_H
#define METASTUDIO_PROTOCOL_H

#include "serial.h"

#include <stdint.h>

typedef struct {
    ms_serial_port *serial;
} ms_protocol;

void ms_protocol_init(ms_protocol *protocol, ms_serial_port *serial);
int ms_protocol_read(ms_protocol *protocol, uint8_t device_type,
                     uint8_t device_id, uint8_t address,
                     uint32_t *value, int timeout_ms);
int ms_protocol_write(ms_protocol *protocol, uint8_t device_type,
                      uint8_t device_id, uint8_t address, uint32_t value);
const char *ms_protocol_error(void);

#endif
