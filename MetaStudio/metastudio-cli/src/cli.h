#ifndef METASTUDIO_CLI_H
#define METASTUDIO_CLI_H

#define MS_CLI_MAX_OPERANDS 4

typedef struct {
    const char *device;
    long device_id;
    long timeout_ms;
    const char *command;
    const char *operands[MS_CLI_MAX_OPERANDS];
    int operand_count;
} ms_cli_arguments;

typedef enum {
    MS_ACTION_STATUS,
    MS_ACTION_CENTER,
    MS_ACTION_SET,
    MS_ACTION_MOVE,
    MS_ACTION_TEST_X,
    MS_ACTION_TEST_Y,
    MS_ACTION_TEST_SIZE,
    MS_ACTION_PROBE_VDBOX,
    MS_ACTION_SCAN_VDBOX,
    MS_ACTION_TEST_BRIGHTNESS
} ms_action_type;

typedef struct {
    ms_action_type type;
    long x;
    long y;
    long size;
    long delta;
    long duration;
} ms_action;

void ms_cli_print_help(void);
int ms_cli_print_command_help(const char *command);
int ms_cli_parse(int argc, char **argv, ms_cli_arguments *arguments);
int ms_cli_prepare_action(const ms_cli_arguments *arguments,
                          ms_action *prepared);

#endif
