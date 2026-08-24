#!/usr/bin/env bash

# Capture Vdbox register values without writing to the device.  Change exactly
# one setting in MetaStudio between snapshots, then compare the output files.
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cli_dir="$(cd -- "$script_dir/.." && pwd)"
cli="$cli_dir/dist/metastudio-cli"
serial_device="${1:-/dev/ttyUSB0}"
label="${2:-snapshot}"
first_address="${3:-0x00}"
last_address="${4:-0x5f}"
timeout_ms="${METASTUDIO_TIMEOUT_MS:-100}"
safe_label="$(printf '%s' "$label" | tr -cs 'A-Za-z0-9._-' '_')"
snapshot_dir="$cli_dir/observations"
timestamp="$(date +%Y%m%d-%H%M%S)"
snapshot="$snapshot_dir/${timestamp}-${safe_label}.txt"

if [[ ! -e "$serial_device" ]]; then
  echo "error: serial device is unavailable: $serial_device" >&2
  exit 1
fi

make -C "$cli_dir" -s
mkdir -p "$snapshot_dir"

{
  echo "# Read-only Vdbox snapshot"
  echo "# captured: $(date --iso-8601=seconds)"
  echo "# device: $serial_device"
  echo "# range: $first_address-$last_address"
  echo
  "$cli" --device "$serial_device" --timeout "$timeout_ms" status
  echo
  "$cli" --device "$serial_device" --timeout "$timeout_ms" \
    scan-vdbox "$first_address" "$last_address"
} > "$snapshot"

echo "$snapshot"
