#!/usr/bin/env bash

# Temporarily set the Vdbox brightness and restore its original value.
set -euo pipefail

if [[ $# -lt 1 || $# -gt 2 ]]; then
  echo "usage: $0 VALUE [SECONDS]" >&2
  exit 2
fi

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cli_dir="$(cd -- "$script_dir/.." && pwd)"
serial_device="${METASTUDIO_DEVICE:-/dev/ttyUSB0}"

if [[ ! -e "$serial_device" ]]; then
  echo "error: serial device is unavailable: $serial_device" >&2
  exit 1
fi

make -C "$cli_dir" -s
exec "$cli_dir/dist/metastudio-cli" --device "$serial_device" \
  test-brightness "$@"
