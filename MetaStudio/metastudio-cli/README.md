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
