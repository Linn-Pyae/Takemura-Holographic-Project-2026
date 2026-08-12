#!/usr/bin/env zsh
# Full pipeline + BOTH GUIs:
#   - Open3D (OpenGL) raw point cloud
#   - map_renderer footprint map (Harry Potter names + trails)
#
# Usage:
#   ./scripts/run_bag_map_both.sh                     # Mac + LIVE /velodyne_points
#   ./scripts/run_bag_map_both.sh mac                 # same
#   ./scripts/run_bag_map_both.sh mac lidar_sample    # Mac + recorded bag
#   ./scripts/run_bag_map_both.sh lidar_sample_2      # Mac + bag (shorthand)
#   ./scripts/run_bag_map_both.sh pi                    # Pi + LIVE + HDMI map
#   ./scripts/run_bag_map_both.sh pi lidar_sample       # Pi + bag (bench)
#
# Live cloud topic default: /velodyne_points
# Override with: LIVE_LIDAR_TOPIC=/your/topic ./scripts/run_bag_map_both.sh
#
# Close either GUI window or Ctrl+C to stop everything.
set -eo pipefail

WS="$(cd "$(dirname "$0")/.." && pwd)"
REPO="$(cd "$WS/.." && pwd)"
RENDERER_BIN="$REPO/map_renderer/build/renderer"
VIEWER="$REPO/Test/view.py"
BAG_SRC_TOPIC="/velodyne_points_wifi"
CLUSTER_TOPIC="/velodyne_points_bag"
LIVE_LIDAR_TOPIC="${LIVE_LIDAR_TOPIC:-/velodyne_points}"
PLAY_DOMAIN="${ROS_DOMAIN_ID:-42}"
ROS_UNDERLAY="${ROS_UNDERLAY:-/opt/ros/jazzy/setup.bash}"
TAKEMURA_RENDERER_SOCKET="${TAKEMURA_RENDERER_SOCKET:-/tmp/takemura-renderer.sock}"

TARGET="mac"
BAG=""

if [[ "${1:-}" == "mac" || "${1:-}" == "pi" ]]; then
  TARGET="$1"
  shift
fi

if [[ -n "${1:-}" ]]; then
  if [[ "$1" = /* ]]; then
    BAG="$1"
  else
    BAG="$REPO/$1"
  fi
fi

if [[ -n "$BAG" ]]; then
  if [[ ! -d "$BAG" || ! -f "$BAG/metadata.yaml" ]]; then
    echo "error: bag not found at $BAG" >&2
    echo "       usage: $0 [mac|pi] [lidar_sample|/path/to/bag]" >&2
    echo "       default (no bag arg): live LiDAR on $LIVE_LIDAR_TOPIC" >&2
    exit 1
  fi
fi

if [[ ! -x "$RENDERER_BIN" ]]; then
  echo "error: map_renderer not built at $RENDERER_BIN" >&2
  echo "       build it with:" >&2
  echo "         cd $REPO/map_renderer && cmake -S . -B build && cmake --build build" >&2
  exit 1
fi

if [[ ! -f "$VIEWER" ]]; then
  echo "error: Open3D viewer not found at $VIEWER" >&2
  exit 1
fi

setup_mac_env() {
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
}

setup_pi_env() {
  if [[ ! -f "$ROS_UNDERLAY" ]]; then
    echo "error: ROS underlay not found at $ROS_UNDERLAY" >&2
    echo "       set ROS_UNDERLAY=/path/to/setup.bash and retry" >&2
    exit 1
  fi
  # shellcheck disable=SC1090
  source "$ROS_UNDERLAY"
  cd "$WS"
  if [[ ! -f install/setup.bash ]]; then
    echo "error: workspace not built. Run:" >&2
    echo "  cd $WS && colcon build && source install/setup.bash" >&2
    exit 1
  fi
  source "$WS/install/setup.bash"
  export DISPLAY="${DISPLAY:-:0}"
}

if [[ "$TARGET" == "pi" ]]; then
  setup_pi_env
else
  setup_mac_env
fi

export ROS_DOMAIN_ID="$PLAY_DOMAIN"
export RMW_IMPLEMENTATION="${RMW_IMPLEMENTATION:-rmw_fastrtps_cpp}"
if [[ -n "$BAG" ]]; then
  export ROS_AUTOMATIC_DISCOVERY_RANGE="${ROS_AUTOMATIC_DISCOVERY_RANGE:-LOCALHOST}"
else
  export ROS_AUTOMATIC_DISCOVERY_RANGE="${ROS_AUTOMATIC_DISCOVERY_RANGE:-SUBNET}"
fi
export DETECTION_TOPIC="${DETECTION_TOPIC:-/person_detections}"
export TRACK_TOPIC="${TRACK_TOPIC:-/person_tracks}"
export TRACK_INFO_TOPIC="${TRACK_INFO_TOPIC:-/person_tracks_info}"
export TAKEMURA_RENDERER_SOCKET

# Open3D viewer topic: bag uses remapped bag topic; live uses sensor topic.
if [[ -n "$BAG" ]]; then
  export LIDAR_TOPIC="${LIDAR_TOPIC:-$CLUSTER_TOPIC}"
else
  export LIDAR_TOPIC="${LIDAR_TOPIC:-$LIVE_LIDAR_TOPIC}"
fi

ros2 daemon stop >/dev/null 2>&1 || true

kill_pipeline_procs() {
  pkill -9 -f 'ros2 bag play' 2>/dev/null || true
  pkill -9 -f rosbag2_player 2>/dev/null || true
  pkill -9 -f 'person_cluster_node' 2>/dev/null || true
  pkill -9 -f 'person_tracker.track_node' 2>/dev/null || true
  pkill -9 -f 'person_tracker/track_node' 2>/dev/null || true
  pkill -9 -f 'renderer_bridge_node' 2>/dev/null || true
  pkill -9 -f 'ros2 run person_cluster' 2>/dev/null || true
  pkill -9 -f 'ros2 run person_tracker' 2>/dev/null || true
  pkill -9 -f 'ros2 run renderer_bridge' 2>/dev/null || true
  pkill -9 -f "$RENDERER_BIN" 2>/dev/null || true
  pkill -9 -f 'Test/view.py' 2>/dev/null || true
  pkill -9 -f 'python -u view.py' 2>/dev/null || true
  sleep 0.4
}

echo "Stopping any leftover bag/cluster/track/bridge/GUI processes…"
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

echo "target=$TARGET"
echo "domain=$ROS_DOMAIN_ID"
echo "renderer_socket=$TAKEMURA_RENDERER_SOCKET"
if [[ -n "$BAG" ]]; then
  echo "mode=bag"
  echo "bag=$BAG"
  echo "cloud=$BAG_SRC_TOPIC -> $CLUSTER_TOPIC"
else
  echo "mode=live"
  echo "cloud=$LIVE_LIDAR_TOPIC -> $CLUSTER_TOPIC (remap)"
fi
echo "open3d_topic=$LIDAR_TOPIC"
echo "flow: cloud -> cluster -> tracker -> bridge -> footprint map"
echo "GUI: Open3D (3D cloud) + map_renderer (footprint map)"
echo "Wait ~7 s for cluster warmup before footprints appear."
echo

# Footprint map binds the Unix socket first.
echo "Opening footprint map (binds $TAKEMURA_RENDERER_SOCKET)…"
export TAKEMURA_RENDERER_SOCKET
unset TAKEMURA_RENDERER_DEMO
"$RENDERER_BIN" &
RENDERER_PID=$!
PIDS+=($RENDERER_PID)
sleep 0.8
if ! kill -0 "$RENDERER_PID" 2>/dev/null; then
  echo "error: map_renderer failed to start" >&2
  exit 1
fi

if [[ -n "$BAG" ]]; then
  ros2 bag play "$BAG" --loop --remap "${BAG_SRC_TOPIC}:=${CLUSTER_TOPIC}" &
  PIDS+=($!)
  sleep 1
  ros2 run person_cluster person_cluster_node &
else
  ros2 run person_cluster person_cluster_node --ros-args \
    -r "${CLUSTER_TOPIC}:=${LIVE_LIDAR_TOPIC}" &
fi
PIDS+=($!)
sleep 0.5

ros2 run person_tracker track_node &
PIDS+=($!)
sleep 0.5

ros2 run renderer_bridge renderer_bridge_node &
PIDS+=($!)
sleep 0.5

echo "Opening Open3D (OpenGL) 3D viewer…"
(
  cd "$REPO/Test"
  python -u view.py
) &
OPEN3D_PID=$!
PIDS+=($OPEN3D_PID)

echo
echo "Both GUIs running. Close either window or press Ctrl+C to stop."
echo

while kill -0 "$RENDERER_PID" 2>/dev/null || kill -0 "$OPEN3D_PID" 2>/dev/null; do
  sleep 0.5
done
