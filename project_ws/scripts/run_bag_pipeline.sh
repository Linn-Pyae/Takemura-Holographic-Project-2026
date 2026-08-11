#!/usr/bin/env zsh
# Run bag -> person_cluster -> person_tracker on an isolated ROS domain.
#
# Usage (from anywhere):
#   ./scripts/run_bag_pipeline.sh
#   ./scripts/run_bag_pipeline.sh lidar_sample_2
#   BAG=/path/to/bag ./scripts/run_bag_pipeline.sh
#
# Ctrl+C stops bag + both nodes.
# Do not use `set -u` around micromamba activate: conda backup vars
# (e.g. CONDA_BACKUP_CLANGXX) may be unset and abort the script.
set -eo pipefail

WS="$(cd "$(dirname "$0")/.." && pwd)"
REPO="$(cd "$WS/.." && pwd)"
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

export MAMBA_ROOT_PREFIX="${MAMBA_ROOT_PREFIX:-$HOME/micromamba}"
eval "$("$MAMBA_ROOT_PREFIX/bin/micromamba" shell hook -s zsh)"
micromamba activate ros_view

cd "$WS"
if [[ ! -f install/setup.zsh ]]; then
  echo "error: workspace not built. Run:" >&2
  echo "  cd $WS && colcon build && source install/setup.zsh" >&2
  exit 1
fi
source "$WS/install/setup.zsh"

export ROS_DOMAIN_ID="$PLAY_DOMAIN"
export RMW_IMPLEMENTATION="${RMW_IMPLEMENTATION:-rmw_fastrtps_cpp}"
export ROS_AUTOMATIC_DISCOVERY_RANGE=LOCALHOST
export DETECTION_TOPIC="${DETECTION_TOPIC:-/person_detections}"
export TRACK_TOPIC="${TRACK_TOPIC:-/person_tracks}"
export TRACK_INFO_TOPIC="${TRACK_INFO_TOPIC:-/person_tracks_info}"
export LIDAR_TOPIC="${LIDAR_TOPIC:-$PLAY_TOPIC}"

ros2 daemon stop >/dev/null 2>&1 || true

kill_pipeline_procs() {
  pkill -9 -f 'ros2 bag play' 2>/dev/null || true
  pkill -9 -f rosbag2_player 2>/dev/null || true
  pkill -9 -f 'person_cluster_node' 2>/dev/null || true
  pkill -9 -f 'person_tracker.track_node' 2>/dev/null || true
  pkill -9 -f 'person_tracker/track_node' 2>/dev/null || true
  pkill -9 -f 'person_tracker.viz_node' 2>/dev/null || true
  pkill -9 -f 'person_tracker/viz_node' 2>/dev/null || true
  # ros2 run wrappers
  pkill -9 -f 'ros2 run person_cluster' 2>/dev/null || true
  pkill -9 -f 'ros2 run person_tracker' 2>/dev/null || true
  sleep 0.4
}

# Previous runs often left orphan cluster/track nodes → duplicate noisy tracks
echo "Stopping any leftover bag/cluster/track/viz processes…"
kill_pipeline_procs

PIDS=()
cleanup() {
  for pid in "${PIDS[@]:-}"; do
    if kill -0 "$pid" 2>/dev/null; then
      kill -TERM "$pid" 2>/dev/null || true
    fi
  done
  sleep 0.3
  for pid in "${PIDS[@]:-}"; do
    if kill -0 "$pid" 2>/dev/null; then
      kill -9 "$pid" 2>/dev/null || true
    fi
  done
  kill_pipeline_procs
}
trap cleanup EXIT INT TERM

echo "bag=$BAG"
echo "domain=$ROS_DOMAIN_ID"
echo "cloud=$PLAY_TOPIC -> cluster -> $DETECTION_TOPIC -> tracker -> $TRACK_TOPIC"
echo "GUI: matplotlib 2D map (detections + track trails)"
echo "Wait for cluster warmup (~60 frames) before detections appear."
echo

ros2 bag play "$BAG" --loop --remap "${SRC_TOPIC}:=${PLAY_TOPIC}" &
PIDS+=($!)
sleep 1

ros2 run person_cluster person_cluster_node &
PIDS+=($!)
sleep 0.5

ros2 run person_tracker track_node &
PIDS+=($!)
sleep 0.5

# Foreground 2D map — closing the window ends the script via cleanup trap
echo "Opening 2D track map…"
ros2 run person_tracker viz_node
