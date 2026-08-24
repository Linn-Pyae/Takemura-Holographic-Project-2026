#!/bin/zsh
# Replay ONE recorded bag and open the Open3D viewer.
#
# Usage:
#   ./run_play_view.sh                 # plays ../lidar_sample
#   ./run_play_view.sh lidar_sample_2  # plays ../lidar_sample_2
#   BAG=/path/to/bag ./run_play_view.sh
#
# Isolation: uses ROS_DOMAIN_ID=42 and remaps the cloud topic so live Pi
# data on domain 0 /velodyne_points_wifi cannot mix in.
set -e

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC_TOPIC="/velodyne_points_wifi"
PLAY_TOPIC="/velodyne_points_bag"
PLAY_DOMAIN=42

if [[ -n "${1:-}" ]]; then
  if [[ "$1" = /* ]]; then
    BAG="$1"
  else
    BAG="$ROOT/$1"
  fi
else
  BAG="${BAG:-$ROOT/lidar_sample}"
fi

if [[ ! -d "$BAG" || ! -f "$BAG/metadata.yaml" ]]; then
  echo "error: bag not found at $BAG" >&2
  echo "       usage: $0 [lidar_sample|lidar_sample_2|/path/to/bag]" >&2
  exit 1
fi

export MAMBA_ROOT_PREFIX="$HOME/micromamba"
eval "$("$HOME/micromamba/bin/micromamba" shell hook -s zsh)"
micromamba activate ros_view

# Separate DDS domain from live Pi (domain 0) — hard isolation
export ROS_DOMAIN_ID="$PLAY_DOMAIN"
export RMW_IMPLEMENTATION=rmw_fastrtps_cpp
export ROS_AUTOMATIC_DISCOVERY_RANGE=LOCALHOST
export LIDAR_TOPIC="$PLAY_TOPIC"
unset ROS_LOCALHOST_ONLY
unset FASTRTPS_DEFAULT_PROFILES_FILE
unset FASTDDS_DEFAULT_PROFILES_FILE
unset ROS_DISCOVERY_SERVER
unset CYCLONEDDS_URI
unset ROS_STATIC_PEERS

ros2 daemon stop > /dev/null 2>&1 || true

# Hard-stop every leftover player (previous runs left orphans)
pkill -9 -f 'ros2 bag play' 2>/dev/null || true
pkill -9 -f rosbag2_player 2>/dev/null || true
sleep 0.5

PLAY_PID=""
cleanup() {
  if [[ -n "$PLAY_PID" ]] && kill -0 "$PLAY_PID" 2>/dev/null; then
    kill -TERM "$PLAY_PID" 2>/dev/null || true
    sleep 0.2
    kill -9 "$PLAY_PID" 2>/dev/null || true
    wait "$PLAY_PID" 2>/dev/null || true
  fi
  pkill -9 -f 'ros2 bag play' 2>/dev/null || true
  pkill -9 -f rosbag2_player 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "bag=$BAG"
echo "domain=$PLAY_DOMAIN  topic=$PLAY_TOPIC  (isolated from live Pi)"
# Remap so even on the same machine, bag never shares the live topic name
ros2 bag play "$BAG" --loop --remap "${SRC_TOPIC}:=${PLAY_TOPIC}" &
PLAY_PID=$!
sleep 1

if ! kill -0 "$PLAY_PID" 2>/dev/null; then
  echo "error: bag player failed to start" >&2
  exit 1
fi

cd "$(dirname "$0")"
# Do NOT exec — shell must stay alive so cleanup kills the bag player
python -u view.py
