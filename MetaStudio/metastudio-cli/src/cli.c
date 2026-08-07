#include "cli.h"

#include <ctype.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifndef METASTUDIO_CLI_VERSION
#define METASTUDIO_CLI_VERSION "0.0.0"
#endif

#define DEFAULT_TIMEOUT_MS 1000

void ms_cli_print_help(void)
{
    puts("Usage: metastudio-cli [options] <command> [arguments]\n"
         "\n"
         "Manage MetaStudio crop settings over the controller's serial port.\n"
         "\n"
         "Commands:\n"
         "  status                    Show the input resolution and current crop\n"
         "  center                    Center the largest square crop\n"
         "  set X Y SIZE              Set the crop position and size\n"
         "  move DX DY                Move the current crop\n"
         "  test x|y DELTA [SECONDS]  Temporarily move the crop\n"
         "  test size SIZE [SECONDS]  Temporarily test a centered crop size\n"
         "  probe-vdbox               Read known Vdbox registers without changing them\n"
         "  scan-vdbox START END      Read an inclusive Vdbox address range\n"
         "  test-brightness VALUE [S] Temporarily change Vdbox brightness, then restore\n"
         "  help [COMMAND]            Show help\n"
         "\n"
         "Options:\n"
         "  -d, --device PORT         Serial port (auto-detected by default)\n"
         "  -i, --id ID               Controller ID (default: 1)\n"
         "  -t, --timeout MS          Response timeout (default: 1000)\n"
         "  -h, --help                Show help\n"
         "  -V, --version             Show version");
}

int ms_cli_print_command_help(const char *command)
{
    if (strcmp(command, "status") == 0) {
        puts("Usage: metastudio-cli [options] status\n\n"
             "Show the input resolution and current crop.");
    } else if (strcmp(command, "center") == 0) {
        puts("Usage: metastudio-cli [options] center\n\n"
             "Center the largest square crop.");
    } else if (strcmp(command, "set") == 0) {
        puts("Usage: metastudio-cli [options] set X Y SIZE\n\n"
             "Set a square crop using pixel coordinates.");
    } else if (strcmp(command, "move") == 0) {
        puts("Usage: metastudio-cli [options] move DX DY\n\n"
             "Move the current crop by a pixel offset.");
    } else if (strcmp(command, "test") == 0) {
        puts("Usage: metastudio-cli [options] test x|y DELTA [SECONDS]\n"
             "       metastudio-cli [options] test size SIZE [SECONDS]\n\n"
             "Apply a crop temporarily, then restore the original crop.");
    } else if (strcmp(command, "probe-vdbox") == 0) {
        puts("Usage: metastudio-cli [options] probe-vdbox\n\n"
             "Read known Vdbox registers without changing device settings.");
    } else if (strcmp(command, "scan-vdbox") == 0) {
        puts("Usage: metastudio-cli [options] scan-vdbox START END\n\n"
             "Read the inclusive hexadecimal Vdbox address range without\n"
             "changing device settings.");
    } else if (strcmp(command, "test-brightness") == 0) {
        puts("Usage: metastudio-cli [options] test-brightness VALUE [SECONDS]\n\n"
             "Temporarily set Vdbox brightness (0 through 255), then restore\n"
             "the original value. The default duration is 10 seconds.");
    } else {
        fprintf(stderr, "error: unknown command '%s'\n", command);
        return -1;
    }
    return 0;
}

static int parse_long(const char *text, long minimum, long maximum, long *value)
{
    char *end = NULL;
    errno = 0;
    long parsed = strtol(text, &end, 0);
    if (errno != 0 || text[0] == '\0' || *end != '\0' ||
        parsed < minimum || parsed > maximum) {
        return -1;
    }
    *value = parsed;
    return 0;
}

static int add_operand(ms_cli_arguments *arguments, const char *operand)
{
    if (arguments->operand_count == MS_CLI_MAX_OPERANDS) {
        fprintf(stderr, "error: too many arguments for '%s'\n",
                arguments->command);
        return -1;
    }
    arguments->operands[arguments->operand_count++] = operand;
    return 0;
}

int ms_cli_parse(int argc, char **argv, ms_cli_arguments *arguments)
{
    memset(arguments, 0, sizeof(*arguments));
    arguments->device_id = 1;
    arguments->timeout_ms = DEFAULT_TIMEOUT_MS;

    for (int index = 1; index < argc; index++) {
        const char *item = argv[index];

        if (strcmp(item, "-h") == 0 || strcmp(item, "--help") == 0) {
            if (arguments->command == NULL) {
                ms_cli_print_help();
                return 1;
            }
            return ms_cli_print_command_help(arguments->command) == 0 ? 1 : -1;
        }
        if (strcmp(item, "-V") == 0 || strcmp(item, "--version") == 0) {
            printf("metastudio-cli %s\n", METASTUDIO_CLI_VERSION);
            return 1;
        }
        if (strcmp(item, "-d") == 0 || strcmp(item, "--device") == 0) {
            if (++index == argc) {
                fprintf(stderr, "error: option '%s' requires a value\n", item);
                return -1;
            }
            arguments->device = argv[index];
            continue;
        }
        if (strcmp(item, "-i") == 0 || strcmp(item, "--id") == 0) {
            if (++index == argc ||
                parse_long(argv[index], 0, 255, &arguments->device_id) != 0) {
                fprintf(stderr, "error: controller ID must be between 0 and 255\n");
                return -1;
            }
            continue;
        }
        if (strcmp(item, "-t") == 0 || strcmp(item, "--timeout") == 0) {
            if (++index == argc ||
                parse_long(argv[index], 1, 60000, &arguments->timeout_ms) != 0) {
                fprintf(stderr, "error: timeout must be between 1 and 60000 ms\n");
                return -1;
            }
            continue;
        }

        if (arguments->command == NULL) {
            if (item[0] == '-') {
                fprintf(stderr, "error: unknown option '%s'\n", item);
                return -1;
            }
            arguments->command = item;
        } else if (item[0] == '-' && item[1] != '\0' &&
                   !isdigit((unsigned char)item[1])) {
            fprintf(stderr, "error: unknown option '%s'\n", item);
            return -1;
        } else if (add_operand(arguments, item) != 0) {
            return -1;
        }
    }
    return 0;
}

int ms_cli_prepare_action(const ms_cli_arguments *arguments,
                          ms_action *prepared)
{
    const char *command = arguments->command;
    int count = arguments->operand_count;

    memset(prepared, 0, sizeof(*prepared));
    prepared->duration = 5;

    if (strcmp(command, "status") == 0) {
        if (count != 0) {
            fprintf(stderr, "error: 'status' does not accept arguments\n");
            return -1;
        }
        prepared->type = MS_ACTION_STATUS;
    } else if (strcmp(command, "probe-vdbox") == 0) {
        if (count != 0) {
            fprintf(stderr, "error: 'probe-vdbox' does not accept arguments\n");
            return -1;
        }
        prepared->type = MS_ACTION_PROBE_VDBOX;
    } else if (strcmp(command, "scan-vdbox") == 0) {
        if (count != 2 ||
            parse_long(arguments->operands[0], 0, 255, &prepared->x) != 0 ||
            parse_long(arguments->operands[1], 0, 255, &prepared->y) != 0 ||
            prepared->x > prepared->y) {
            fprintf(stderr,
                    "error: usage: scan-vdbox START END (0x00 through 0xff)\n");
            return -1;
        }
        prepared->type = MS_ACTION_SCAN_VDBOX;
    } else if (strcmp(command, "test-brightness") == 0) {
        if ((count != 1 && count != 2) ||
            parse_long(arguments->operands[0], 0, 255, &prepared->size) != 0 ||
            (count == 2 &&
             parse_long(arguments->operands[1], 1, 60, &prepared->duration) != 0)) {
            fprintf(stderr,
                    "error: usage: test-brightness VALUE [SECONDS]\n");
            return -1;
        }
        if (count == 1) {
            prepared->duration = 10;
        }
        prepared->type = MS_ACTION_TEST_BRIGHTNESS;
    } else if (strcmp(command, "center") == 0) {
        if (count != 0) {
            fprintf(stderr, "error: 'center' does not accept arguments\n");
            return -1;
        }
        prepared->type = MS_ACTION_CENTER;
    } else if (strcmp(command, "set") == 0) {
        if (count != 3 ||
            parse_long(arguments->operands[0], 0, 65535, &prepared->x) != 0 ||
            parse_long(arguments->operands[1], 0, 65535, &prepared->y) != 0 ||
            parse_long(arguments->operands[2], 32, 65535, &prepared->size) != 0) {
            fprintf(stderr, "error: usage: metastudio-cli set X Y SIZE\n");
            return -1;
        }
        prepared->type = MS_ACTION_SET;
    } else if (strcmp(command, "move") == 0) {
        if (count != 2 ||
            parse_long(arguments->operands[0], -65535, 65535, &prepared->x) != 0 ||
            parse_long(arguments->operands[1], -65535, 65535, &prepared->y) != 0) {
            fprintf(stderr, "error: usage: metastudio-cli move DX DY\n");
            return -1;
        }
        prepared->type = MS_ACTION_MOVE;
    } else if (strcmp(command, "test") == 0) {
        if (count != 2 && count != 3) {
            fprintf(stderr,
                    "error: usage: metastudio-cli test x|y DELTA [SECONDS]\n");
            return -1;
        }
        if (count == 3 &&
            parse_long(arguments->operands[2], 1, 60, &prepared->duration) != 0) {
            fprintf(stderr,
                    "error: test duration must be between 1 and 60 seconds\n");
            return -1;
        }
        if (strcmp(arguments->operands[0], "x") == 0 ||
            strcmp(arguments->operands[0], "y") == 0) {
            if (parse_long(arguments->operands[1], -65535, 65535,
                           &prepared->delta) != 0) {
                fprintf(stderr, "error: DELTA must be a valid pixel offset\n");
                return -1;
            }
            prepared->type = strcmp(arguments->operands[0], "x") == 0
                                 ? MS_ACTION_TEST_X : MS_ACTION_TEST_Y;
        } else if (strcmp(arguments->operands[0], "size") == 0) {
            if (parse_long(arguments->operands[1], 32, 65535,
                           &prepared->size) != 0) {
                fprintf(stderr, "error: SIZE must be at least 32 pixels\n");
                return -1;
            }
            prepared->type = MS_ACTION_TEST_SIZE;
        } else {
            fprintf(stderr, "error: test target must be 'x', 'y', or 'size'\n");
            return -1;
        }
    } else {
        fprintf(stderr, "error: unknown command '%s'\n", command);
        return -1;
    }
    return 0;
}
