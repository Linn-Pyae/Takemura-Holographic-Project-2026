#define _DEFAULT_SOURCE

#include "serial.h"

#include <errno.h>
#include <fcntl.h>
#include <glob.h>
#include <poll.h>
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/ioctl.h>
#include <termios.h>
#include <time.h>
#include <unistd.h>

struct ms_serial_port {
    int fd;
};

static char last_error[256] = "unknown serial error";

static void set_error(const char *format, ...)
{
    va_list arguments;
    va_start(arguments, format);
    vsnprintf(last_error, sizeof(last_error), format, arguments);
    va_end(arguments);
}

static void set_system_error(const char *operation)
{
    set_error("%s: %s", operation, strerror(errno));
}

const char *ms_serial_error(void)
{
    return last_error;
}

uint64_t ms_monotonic_ms(void)
{
    struct timespec now;
    clock_gettime(CLOCK_MONOTONIC, &now);
    return (uint64_t)now.tv_sec * 1000u + (uint64_t)now.tv_nsec / 1000000u;
}

void ms_sleep_ms(unsigned long milliseconds)
{
    usleep(milliseconds * 1000u);
}

int ms_serial_detect(char *path, size_t path_size)
{
    const char *patterns[] = { "/dev/ttyUSB*", "/dev/ttyACM*" };

    for (size_t index = 0; index < sizeof(patterns) / sizeof(patterns[0]); index++) {
        glob_t matches;
        memset(&matches, 0, sizeof(matches));
        if (glob(patterns[index], 0, NULL, &matches) == 0 &&
            matches.gl_pathc > 0) {
            snprintf(path, path_size, "%s", matches.gl_pathv[0]);
            globfree(&matches);
            return 0;
        }
        globfree(&matches);
    }

    set_error("no compatible serial ports were found");
    return -1;
}

int ms_serial_open(ms_serial_port **port, const char *path, unsigned int baud_rate)
{
    ms_serial_port *opened = calloc(1, sizeof(*opened));
    if (opened == NULL) {
        set_error("out of memory");
        return -1;
    }
    opened->fd = open(path, O_RDWR | O_NOCTTY | O_NONBLOCK);
    if (opened->fd < 0) {
        set_system_error("could not open serial port");
        free(opened);
        return -1;
    }

    struct termios options;
    if (tcgetattr(opened->fd, &options) != 0) {
        set_system_error("could not read serial port settings");
        ms_serial_close(opened);
        return -1;
    }
    cfmakeraw(&options);
    options.c_cflag |= CLOCAL | CREAD;
    options.c_cflag &= ~(PARENB | CSTOPB | CSIZE);
    options.c_cflag |= CS8;
#ifdef CRTSCTS
    options.c_cflag &= ~CRTSCTS;
#endif
    options.c_cc[VMIN] = 0;
    options.c_cc[VTIME] = 1;

#ifdef B2000000
    if (baud_rate != 2000000u ||
        cfsetispeed(&options, B2000000) != 0 ||
        cfsetospeed(&options, B2000000) != 0) {
        set_error("unsupported baud rate: %u", baud_rate);
        ms_serial_close(opened);
        return -1;
    }
#else
    set_error("this platform does not support 2000000 baud");
    ms_serial_close(opened);
    return -1;
#endif

    if (tcsetattr(opened->fd, TCSANOW, &options) != 0) {
        set_system_error("could not configure serial port");
        ms_serial_close(opened);
        return -1;
    }

    int modem_bits;
    if (ioctl(opened->fd, TIOCMGET, &modem_bits) == 0) {
        modem_bits |= TIOCM_DTR | TIOCM_RTS;
        (void)ioctl(opened->fd, TIOCMSET, &modem_bits);
    }
    if (tcflush(opened->fd, TCIOFLUSH) != 0) {
        set_system_error("could not flush serial port");
        ms_serial_close(opened);
        return -1;
    }

    *port = opened;
    return 0;
}

void ms_serial_close(ms_serial_port *port)
{
    if (port == NULL) {
        return;
    }
    if (port->fd >= 0) {
        close(port->fd);
    }
    free(port);
}

int ms_serial_write_all(ms_serial_port *port, const uint8_t *data, size_t length)
{
    size_t written = 0;
    while (written < length) {
        ssize_t count = write(port->fd, data + written, length - written);
        if (count < 0 && errno == EINTR) {
            continue;
        }
        if (count <= 0) {
            set_system_error("could not write to serial port");
            return -1;
        }
        written += (size_t)count;
    }
    return 0;
}

int ms_serial_read(ms_serial_port *port, uint8_t *data, size_t length,
                   int timeout_ms)
{
    struct pollfd descriptor = { .fd = port->fd, .events = POLLIN };
    int result;
    do {
        result = poll(&descriptor, 1, timeout_ms);
    } while (result < 0 && errno == EINTR);

    if (result < 0) {
        set_system_error("could not poll serial port");
        return -1;
    }
    if (result == 0) {
        return 0;
    }

    ssize_t count;
    do {
        count = read(port->fd, data, length);
    } while (count < 0 && errno == EINTR);

    if (count < 0 && (errno == EAGAIN || errno == EWOULDBLOCK)) {
        return 0;
    }
    if (count < 0) {
        set_system_error("could not read from serial port");
        return -1;
    }
    return (int)count;
}

int ms_serial_drain(ms_serial_port *port)
{
    if (tcdrain(port->fd) != 0) {
        set_system_error("could not flush serial output");
        return -1;
    }
    return 0;
}

int ms_serial_flush_input(ms_serial_port *port)
{
    if (tcflush(port->fd, TCIFLUSH) != 0) {
        set_system_error("could not flush serial input");
        return -1;
    }
    return 0;
}
