#include "cli.h"
#include "controller.h"
#include "serial.h"

#include <signal.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>

#define SERIAL_PATH_SIZE 256

static volatile sig_atomic_t interrupted = 0;

static void handle_signal(int signal_number)
{
    (void)signal_number;
    interrupted = 1;
}

static void print_crop(const char *label, const ms_crop *crop)
{
    printf("%s: %ux%u at (%d, %d)\n", label,
           crop->width, crop->height, crop->x, crop->y);
}

static int probe_vdbox(ms_controller *controller)
{
    /*
     * These addresses are either used by the original application or its
     * Vdbox initialization file.  0x1f is deliberately excluded because it
     * is used as the configuration apply/save trigger.
     */
    static const uint8_t addresses[] = {
        0x05, 0x06, 0x07, 0x08, 0x0b, 0x0e, 0x0f,
        0x20, 0x22, 0x23, 0x5a, 0x5c
    };

    puts("Vdbox register snapshot (read-only):");
    for (size_t index = 0; index < sizeof(addresses) / sizeof(addresses[0]);
         index++) {
        uint32_t value;
        uint8_t address = addresses[index];
        if (ms_protocol_read(&controller->protocol, 0x01,
                             controller->device_id, address, &value,
                             controller->timeout_ms) != 0) {
            fprintf(stderr, "  0x%02x: no response (%s)\n", address,
                    ms_protocol_error());
            continue;
        }
        printf("  0x%02x = 0x%08x (%u)\n", address, value, value);
    }
    return 0;
}

static int scan_vdbox(ms_controller *controller, uint8_t first, uint8_t last)
{
    unsigned int responses = 0;

    printf("Vdbox read-only scan: 0x%02x-0x%02x\n", first, last);
    for (unsigned int address = first; address <= last; address++) {
        uint32_t value;
        if (ms_protocol_read(&controller->protocol, 0x01,
                             controller->device_id, (uint8_t)address, &value,
                             controller->timeout_ms) == 0) {
            printf("  0x%02x = 0x%08x (%u)\n", address, value, value);
            responses++;
        }
    }
    printf("Responsive addresses: %u\n", responses);
    return 0;
}

static int test_vdbox_brightness(ms_controller *controller,
                                 uint8_t requested, long duration)
{
    uint32_t original;
    uint32_t actual;

    if (ms_protocol_read(&controller->protocol, 0x01,
                         controller->device_id, 0x22, &original,
                         controller->timeout_ms) != 0) {
        fprintf(stderr, "error: could not read Vdbox brightness: %s\n",
                ms_protocol_error());
        return 1;
    }
    if (original > 255u) {
        fprintf(stderr, "error: Vdbox brightness is outside the expected range: %u\n",
                original);
        return 1;
    }
    if (ms_protocol_write(&controller->protocol, 0x01,
                          controller->device_id, 0x22, requested) != 0 ||
        ms_protocol_read(&controller->protocol, 0x01,
                         controller->device_id, 0x22, &actual,
                         controller->timeout_ms) != 0 || actual != requested) {
        fprintf(stderr, "error: could not apply temporary Vdbox brightness: %s\n",
                ms_protocol_error());
        (void)ms_protocol_write(&controller->protocol, 0x01,
                                controller->device_id, 0x22, original);
        return 1;
    }

    printf("Temporary Vdbox brightness: %u (original: %u) for %ld seconds.\n",
           requested, original, duration);
    interrupted = 0;
    signal(SIGINT, handle_signal);
    signal(SIGTERM, handle_signal);
    for (long elapsed = 0; elapsed < duration && !interrupted; elapsed++) {
        ms_sleep_ms(1000);
    }

    if (ms_protocol_write(&controller->protocol, 0x01,
                          controller->device_id, 0x22, original) != 0 ||
        ms_protocol_read(&controller->protocol, 0x01,
                         controller->device_id, 0x22, &actual,
                         controller->timeout_ms) != 0 || actual != original) {
        fprintf(stderr, "error: could not restore Vdbox brightness: %s\n",
                ms_protocol_error());
        return 1;
    }
    printf("Original Vdbox brightness restored: %u\n", original);
    return 0;
}

static ms_crop target_crop(const ms_action *action,
                           const ms_controller_state *state)
{
    ms_crop target = state->crop;
    uint16_t side;

    switch (action->type) {
    case MS_ACTION_CENTER:
        side = state->input_width < state->input_height
                   ? state->input_width : state->input_height;
        target.x = (state->input_width - side) / 2;
        target.y = (state->input_height - side) / 2;
        target.width = side;
        target.height = side;
        break;
    case MS_ACTION_SET:
        target.x = (int32_t)action->x;
        target.y = (int32_t)action->y;
        target.width = (uint16_t)action->size;
        target.height = (uint16_t)action->size;
        break;
    case MS_ACTION_MOVE:
        target.x += (int32_t)action->x;
        target.y += (int32_t)action->y;
        break;
    case MS_ACTION_TEST_X:
        target.x += (int32_t)action->delta;
        break;
    case MS_ACTION_TEST_Y:
        target.y += (int32_t)action->delta;
        break;
    case MS_ACTION_TEST_SIZE:
        side = (uint16_t)action->size;
        target.x = (state->input_width - side) / 2;
        target.y = (state->input_height - side) / 2;
        target.width = side;
        target.height = side;
        break;
    case MS_ACTION_STATUS:
    case MS_ACTION_PROBE_VDBOX:
    case MS_ACTION_SCAN_VDBOX:
    case MS_ACTION_TEST_BRIGHTNESS:
        break;
    }
    return target;
}

static int handle_help_command(const ms_cli_arguments *arguments)
{
    if (arguments->operand_count == 0) {
        ms_cli_print_help();
        return 0;
    }
    if (arguments->operand_count == 1) {
        return ms_cli_print_command_help(arguments->operands[0]) == 0 ? 0 : 2;
    }
    fprintf(stderr, "error: usage: metastudio-cli help [COMMAND]\n");
    return 2;
}

int main(int argc, char **argv)
{
    ms_cli_arguments arguments;
    int parse_result = ms_cli_parse(argc, argv, &arguments);
    if (parse_result != 0) {
        return parse_result > 0 ? 0 : 2;
    }
    if (arguments.command == NULL) {
        ms_cli_print_help();
        return 0;
    }
    if (strcmp(arguments.command, "help") == 0) {
        return handle_help_command(&arguments);
    }

    ms_action action;
    if (ms_cli_prepare_action(&arguments, &action) != 0) {
        fprintf(stderr, "Try 'metastudio-cli --help' for more information.\n");
        return 2;
    }

    char detected_device[SERIAL_PATH_SIZE];
    const char *device = arguments.device;
    if (device == NULL) {
        if (ms_serial_detect(detected_device, sizeof(detected_device)) != 0) {
            fprintf(stderr,
                    "error: no serial port found; specify one with --device PORT\n");
            return 1;
        }
        device = detected_device;
    }

    ms_serial_port *serial = NULL;
    if (ms_serial_open(&serial, device, 2000000) != 0) {
        fprintf(stderr, "error: %s\n", ms_serial_error());
        return 1;
    }

    ms_controller controller;
    ms_controller_init(&controller, serial, (uint8_t)arguments.device_id,
                       (int)arguments.timeout_ms);

    ms_controller_state state;
    if (ms_controller_read_state(&controller, &state) != 0) {
        fprintf(stderr, "error: %s\n", ms_controller_error());
        ms_serial_close(serial);
        return 1;
    }

    if (action.type == MS_ACTION_STATUS) {
        printf("Input: %ux%u\n", state.input_width, state.input_height);
        print_crop("Crop", &state.crop);
        ms_serial_close(serial);
        return 0;
    }

    if (action.type == MS_ACTION_PROBE_VDBOX) {
        int result = probe_vdbox(&controller);
        ms_serial_close(serial);
        return result;
    }

    if (action.type == MS_ACTION_SCAN_VDBOX) {
        int result = scan_vdbox(&controller, (uint8_t)action.x,
                                (uint8_t)action.y);
        ms_serial_close(serial);
        return result;
    }

    if (action.type == MS_ACTION_TEST_BRIGHTNESS) {
        int result = test_vdbox_brightness(&controller, (uint8_t)action.size,
                                           action.duration);
        ms_serial_close(serial);
        return result;
    }

    ms_crop target = target_crop(&action, &state);
    if (!ms_crop_is_valid(&target, state.input_width, state.input_height)) {
        fprintf(stderr, "error: crop is outside the %ux%u input bounds\n",
                state.input_width, state.input_height);
        ms_serial_close(serial);
        return 2;
    }

    int temporary = action.type == MS_ACTION_TEST_X ||
                    action.type == MS_ACTION_TEST_Y ||
                    action.type == MS_ACTION_TEST_SIZE;
    int reset_input = action.type == MS_ACTION_CENTER ||
                      action.type == MS_ACTION_SET;
    if (ms_controller_apply_crop(&controller, state.input_width,
                                 state.input_height, &target,
                                 reset_input) != 0) {
        fprintf(stderr, "error: %s\n", ms_controller_error());
        ms_serial_close(serial);
        return 1;
    }

    if (!temporary) {
        print_crop("Crop updated", &target);
        ms_serial_close(serial);
        return 0;
    }

    printf("Temporary crop applied for %ld seconds. Press Ctrl+C to restore.\n",
           action.duration);
    signal(SIGINT, handle_signal);
    signal(SIGTERM, handle_signal);
    for (long elapsed = 0; elapsed < action.duration && !interrupted; elapsed++) {
        ms_sleep_ms(1000);
    }

    if (ms_controller_apply_crop(&controller, state.input_width,
                                 state.input_height, &state.crop, 0) != 0) {
        fprintf(stderr, "error: could not restore crop: %s\n",
                ms_controller_error());
        ms_serial_close(serial);
        return 1;
    }
    puts("Original crop restored.");
    ms_serial_close(serial);
    return 0;
}
