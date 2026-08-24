#ifndef METASTUDIO_SERIAL_H
#define METASTUDIO_SERIAL_H

#include <stddef.h>
#include <stdint.h>

typedef struct ms_serial_port ms_serial_port;

int ms_serial_detect(char *path, size_t path_size);
int ms_serial_open(ms_serial_port **port, const char *path, unsigned int baud_rate);
void ms_serial_close(ms_serial_port *port);

int ms_serial_write_all(ms_serial_port *port, const uint8_t *data, size_t length);
int ms_serial_read(ms_serial_port *port, uint8_t *data, size_t length,
                   int timeout_ms);
int ms_serial_drain(ms_serial_port *port);
int ms_serial_flush_input(ms_serial_port *port);

uint64_t ms_monotonic_ms(void);
void ms_sleep_ms(unsigned long milliseconds);
const char *ms_serial_error(void);

#endif
