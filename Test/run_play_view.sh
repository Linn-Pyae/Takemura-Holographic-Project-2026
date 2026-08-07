#!/bin/zsh
# Replay a recorded bag and open the Open3D live viewer.
#
# Default bag: ../lidar_sample (repo root). Override with BAG=/path/to/bag
# Playback is local-only — no Pi / FastDDS peer profile needed.
set -e

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
BAG="${BAG:-$ROOT/lidar_sample}"

if [[ ! -d "$BAG" || ! -f "$BAG/metadata.yaml" ]]; then
  echo "error: bag not found at $BAG" >&2
  echo "       record first, or set BAG=/path/to/bag $0" >&2
  exit 1
fi

export MAMBA_ROOT_PREFIX="$HOME/micromamba"
eval "$("$HOME/micromamba/bin/micromamba" shell hook -s zsh)"
micromamba activate ros_view

export ROS_DOMAIN_ID=0
export RMW_IMPLEMENTATION=rmw_fastrtps_cpp
export ROS_LOCALHOST_ONLY=1
unset FASTRTPS_DEFAULT_PROFILES_FILE
unset FASTDDS_DEFAULT_PROFILES_FILE
unset ROS_DISCOVERY_SERVER
unset CYCLONEDDS_URI

ros2 daemon stop > /dev/null 2>&1 || true

PLAY_PID=""
cleanup() {
  if [[ -n "$PLAY_PID" ]] && kill -0 "$PLAY_PID" 2>/dev/null; then
    kill "$PLAY_PID" 2>/dev/null || true
    wait "$PLAY_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

echo "bag=$BAG  (looping /velodyne_points_wifi)"
ros2 bag play "$BAG" --loop &
PLAY_PID=$!
sleep 1

cd "$(dirname "$0")"
exec python -u view.py
