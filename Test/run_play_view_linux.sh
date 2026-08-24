#!/usr/bin/env bash
# Replay ONE recorded bag and open the Open3D viewer.
#
# Usage:
#   ./Test/run_play_view_linux.sh                 # plays ./lidar_sample
#   ./Test/run_play_view_linux.sh lidar_sample_2  # plays ./lidar_sample_2
#   BAG=/path/to/bag ./Test/run_play_view_linux.sh
#
# Isolation: uses ROS_DOMAIN_ID=42 and remaps the cloud topic so live Pi
# data on domain 0 /velodyne_points_wifi cannot mix in.
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd -P)"
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
  BAG="${BAG:-lidar_sample}"
fi

# A relative BAG environment variable should mean relative to the repository,
# not relative to whichever directory the launcher was called from.
if [[ "$BAG" != /* ]]; then
  BAG="$ROOT/$BAG"
fi

if [[ ! -d "$BAG" || ! -f "$BAG/metadata.yaml" ]]; then
  echo "error: bag not found at $BAG" >&2
  echo "       usage: $0 [lidar_sample|lidar_sample_2|/path/to/bag]" >&2
  exit 1
fi

# Linux setup: ROS is installed system-wide and the viewer's Python packages
# live in a normal virtual environment.  Both locations can be overridden for
# a different machine without editing this script.
ROS_SETUP="${ROS_SETUP:-/opt/ros/lyrical/setup.bash}"
ROS_VIEW_VENV="${ROS_VIEW_VENV:-$HOME/.venvs/ros_view}"
OPEN3D_ROOT="${OPEN3D_ROOT:-$HOME/.local/share/takemura-holographic/open3d/usr}"
OPEN3D_LIB_DIR="${OPEN3D_LIB_DIR:-$OPEN3D_ROOT/lib/x86_64-linux-gnu}"

if [[ ! -r "$ROS_SETUP" ]]; then
  echo "error: ROS setup file not found at $ROS_SETUP" >&2
  echo "       set ROS_SETUP=/opt/ros/<distro>/setup.bash and try again" >&2
  exit 1
fi
if [[ ! -f "$ROS_VIEW_VENV/bin/activate" ]]; then
  echo "error: viewer virtual environment not found at $ROS_VIEW_VENV" >&2
  echo "       set ROS_VIEW_VENV=/path/to/ros_view and try again" >&2
  exit 1
fi
if [[ ! -d "$OPEN3D_ROOT/lib/python3/dist-packages/open3d" || ! -d "$OPEN3D_LIB_DIR" ]]; then
  echo "error: Open3D bundle not found under $OPEN3D_ROOT" >&2
  echo "       set OPEN3D_ROOT=/path/to/open3d/usr and try again" >&2
  exit 1
fi

# ROS's generated setup scripts reference a few optional variables before
# initializing them, so temporarily disable nounset while loading the file.
set +u
# shellcheck disable=SC1090
source "$ROS_SETUP"
set -u
# shellcheck disable=SC1090
source "$ROS_VIEW_VENV/bin/activate"

# Open3D was installed from a Linux package into a user-local bundle rather
# than the virtual environment.  Make both its Python module and shared
# libraries visible to the viewer.
export PYTHONPATH="$OPEN3D_ROOT/lib/python3/dist-packages${PYTHONPATH:+:$PYTHONPATH}"
export LD_LIBRARY_PATH="$OPEN3D_LIB_DIR${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"

if ! python -c 'import open3d, rclpy' >/dev/null 2>&1; then
  echo "error: the selected Python environment cannot import Open3D and ROS 2" >&2
  exit 1
fi

# This packaged Open3D build fails to initialize GLEW through GLFW's Wayland
# backend, while its X11 backend works.  A normal Linux desktop exposes DISPLAY
# alongside Wayland, so prefer X11 here; set OPEN3D_FORCE_X11=0 to opt out.
if [[ "${OPEN3D_FORCE_X11:-1}" == "1" && -n "${DISPLAY:-}" ]]; then
  unset WAYLAND_DISPLAY
  export XDG_SESSION_TYPE=x11
fi

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

cd "$SCRIPT_DIR"
# Do NOT exec — shell must stay alive so cleanup kills the bag player
python -u view.py
