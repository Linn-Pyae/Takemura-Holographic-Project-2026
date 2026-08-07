#!/usr/bin/env bash

# Compare two files made by capture-vdbox-snapshot.sh.
set -euo pipefail

if [[ $# -ne 2 ]]; then
  echo "usage: $0 BEFORE AFTER" >&2
  exit 2
fi

diff -u "$1" "$2" || true
