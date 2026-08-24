#!/usr/bin/env bash

# Read-only Vdbox register snapshot.  This script never writes, saves, resets,
# or changes the crop configuration.
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cli_dir="$(cd -- "$script_dir/.." && pwd)"
cli="$cli_dir/dist/metastudio-cli"
serial_device="${1:-/dev/ttyUSB0}"

if [[ ! -e "$serial_device" ]]; then
  echo "error: serial device is unavailable: $serial_device" >&2
  exit 1
fi

if [[ ! -x "$cli" ]]; then
  make -C "$cli_dir"
fi

exec "$cli" --device "$serial_device" probe-vdbox
