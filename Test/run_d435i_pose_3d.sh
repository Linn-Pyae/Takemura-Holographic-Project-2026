#!/bin/zsh
# Capture 10 seconds of D435i calibration and MediaPipe Pose 2D/Depth/3D data.
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PYTHON="/Users/sakuya/micromamba/envs/d435i_pose/bin/python"

if [[ ! -x "$PYTHON" ]]; then
  echo "error: d435i_pose Python environment is missing" >&2
  exit 1
fi

echo "macOS requires administrator access to claim the D435i USB interface."
echo "Output will be written under: $SCRIPT_DIR/captures"
exec sudo "$PYTHON" "$SCRIPT_DIR/d435i_pose_3d.py" --duration 10 "$@"
