#!/usr/bin/env zsh
# Replay a LiDAR bag and open the Open3D (OpenGL) 3D point-cloud viewer.
# Use this to inspect raw cloud noise before / while tuning clustering.
#
# Usage:
#   ./scripts/run_bag_view3d.sh
#   ./scripts/run_bag_view3d.sh lidar_sample_2
#   BAG=/path/to/bag ./scripts/run_bag_view3d.sh
#
# Isolation: ROS_DOMAIN_ID=42, remapped topic /velodyne_points_bag
# Do not use `set -u` with micromamba activate (CONDA_BACKUP_* vars).
set -eo pipefail

WS="$(cd "$(dirname "$0")/.." && pwd)"
REPO="$(cd "$WS/.." && pwd)"
VIEWER="$WS/scripts/view3d.py"
SRC_TOPIC="/velodyne_points_wifi"
PLAY_TOPIC="/velodyne_points_bag"
PLAY_DOMAIN="${ROS_DOMAIN_ID:-42}"

if [[ -n "${1:-}" ]]; then
  if [[ "$1" = /* ]]; then
    BAG="$1"
  else
    BAG="$REPO/$1"
  fi
else
  BAG="${BAG:-$REPO/lidar_sample}"
fi

if [[ ! -d "$BAG" || ! -f "$BAG/metadata.yaml" ]]; then
  echo "error: bag not found at $BAG" >&2
  echo "       usage: $0 [lidar_sample|lidar_sample_2|/path/to/bag]" >&2
  exit 1
fi

if [[ ! -f "$VIEWER" ]]; then
  echo "error: Open3D viewer not found at $VIEWER" >&2
  exit 1
fi

export MAMBA_ROOT_PREFIX="${MAMBA_ROOT_PREFIX:-$HOME/micromamba}"
eval "$("$MAMBA_ROOT_PREFIX/bin/micromamba" shell hook -s zsh)"
micromamba activate ros_view

export ROS_DOMAIN_ID="$PLAY_DOMAIN"
export RMW_IMPLEMENTATION="${RMW_IMPLEMENTATION:-rmw_fastrtps_cpp}"
export ROS_AUTOMATIC_DISCOVERY_RANGE=LOCALHOST
export LIDAR_TOPIC="$PLAY_TOPIC"
unset ROS_LOCALHOST_ONLY
unset FASTRTPS_DEFAULT_PROFILES_FILE
unset FASTDDS_DEFAULT_PROFILES_FILE
unset ROS_DISCOVERY_SERVER
unset CYCLONEDDS_URI
unset ROS_STATIC_PEERS

ros2 daemon stop >/dev/null 2>&1 || true
pkill -9 -f 'ros2 bag play' 2>/dev/null || true
pkill -9 -f rosbag2_player 2>/dev/null || true
sleep 0.3

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
echo "domain=$ROS_DOMAIN_ID  topic=$PLAY_TOPIC"
echo "Opening Open3D (OpenGL) 3D viewer — close window or Ctrl+C to stop."
echo

ros2 bag play "$BAG" --loop --remap "${SRC_TOPIC}:=${PLAY_TOPIC}" &
PLAY_PID=$!
sleep 1

if ! kill -0 "$PLAY_PID" 2>/dev/null; then
  echo "error: bag player failed to start" >&2
  exit 1
fi

python -u "$VIEWER"
