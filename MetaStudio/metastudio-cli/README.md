# MetaStudio CLI

`metastudio-cli` manages MetaStudio crop settings on Linux over the controller's CFG serial connection. Changes apply to the current device session and are not saved permanently.

## Build

CMake 3.16 or later and a C11 compiler are required.

```console
$ make
```

The static Linux binary is created at `dist/metastudio-cli`.

## Usage

```text
Usage: metastudio-cli [options] <command> [arguments]

Commands:
  status                    Show the input resolution and current crop
  center                    Center the largest square crop
  set X Y SIZE              Set the crop position and size
  move DX DY                Move the current crop
  test x|y DELTA [SECONDS]  Temporarily move the crop
  test size SIZE [SECONDS]  Temporarily test a centered crop size
  probe-vdbox               Read known Vdbox registers without changing them
  scan-vdbox START END      Read an inclusive Vdbox address range
  test-brightness VALUE [S] Temporarily change Vdbox brightness, then restore
  help [COMMAND]            Show help

Options:
  -d, --device PORT         Serial port (auto-detected by default)
  -i, --id ID               Controller ID (default: 1)
  -t, --timeout MS          Response timeout (default: 1000)
  -h, --help                Show help
  -V, --version             Show version
```

Examples:

```console
$ ./dist/metastudio-cli status
$ ./dist/metastudio-cli center
$ ./dist/metastudio-cli set 420 0 1080
$ ./dist/metastudio-cli move -100 0
$ ./dist/metastudio-cli test x 200 5
```

Use `--device /dev/ttyUSB0` if automatic detection does not select the correct serial port.

## Read-only Vdbox probe

Run `scripts/probe-vdbox.sh` to record the known Vdbox registers. It only sends
register-read requests and does not change the device configuration.

For a comparison-ready scan, run `scripts/capture-vdbox-snapshot.sh`. It scans
`0x00` through `0x5f` by default and writes a timestamped file to
`observations/`. Change one setting at a time, take another snapshot, then run
`scripts/diff-vdbox-snapshots.sh BEFORE AFTER`.

To visibly verify the known brightness register without persisting a change,
run `scripts/test-vdbox-brightness.sh VALUE [SECONDS]`. The original value is
read first and restored when the test ends or is interrupted with Ctrl+C.
