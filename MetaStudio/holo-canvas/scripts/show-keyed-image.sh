#!/usr/bin/env bash
# Display an image after replacing low-intensity pixels with RGB(0,0,0).
# This is intentionally PC-side: the Vdbox receives a normal HDMI image.
set -eu

if [ "$#" -lt 1 ] || [ "$#" -gt 3 ]; then
  echo "Usage: $0 IMAGE [CUTOFF] [DISPLAY]" >&2
  echo "  CUTOFF: 0..255, defaults to 64" >&2
  echo "  DISPLAY: SDL display index, defaults to 1" >&2
  exit 2
fi

image=$1
cutoff=${2:-64}
display=${3:-1}

case $cutoff in
  ''|*[!0-9]*) echo "CUTOFF must be an integer from 0 to 255." >&2; exit 2 ;;
esac
if [ "$cutoff" -gt 255 ]; then
  echo "CUTOFF must be an integer from 0 to 255." >&2
  exit 2
fi

case $display in
  ''|*[!0-9]*) echo "DISPLAY must be a non-negative integer." >&2; exit 2 ;;
esac
if [ ! -f "$image" ]; then
  echo "Image file not found: $image" >&2
  exit 2
fi

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
project_dir=$(CDPATH= cd -- "$script_dir/.." && pwd)
output_bmp="${XDG_RUNTIME_DIR:-/tmp}/holo-canvas-keyed-${UID}.bmp"

# Peak-RGB threshold keeps saturated primary colours visible while turning
# dim pixels into black.  RGB(0,0,0) is the LED-off value for this display.
filter="format=rgb24,geq=r='if(gte(max(r(X,Y),max(g(X,Y),b(X,Y))),${cutoff}),r(X,Y),0)':g='if(gte(max(r(X,Y),max(g(X,Y),b(X,Y))),${cutoff}),g(X,Y),0)':b='if(gte(max(r(X,Y),max(g(X,Y),b(X,Y))),${cutoff}),b(X,Y),0)'"

ffmpeg -hide_banner -loglevel error -y -i "$image" -vf "$filter" -frames:v 1 "$output_bmp"
dotnet run --project "$project_dir/HoloCanvas.csproj" -- --display "$display" --cutoff "$cutoff" --image "$output_bmp"
