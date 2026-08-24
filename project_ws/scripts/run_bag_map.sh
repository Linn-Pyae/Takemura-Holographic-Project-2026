#!/usr/bin/env zsh
# Cluster + track + renderer_bridge + map_renderer footprint GUI.
#
# Usage:
#   ./scripts/run_bag_map.sh                     # Mac + LIVE /velodyne_points (default)
#   ./scripts/run_bag_map.sh mac                 # same
#   ./scripts/run_bag_map.sh mac lidar_sample    # Mac + recorded bag
#   ./scripts/run_bag_map.sh lidar_sample_2      # Mac + bag (shorthand)
#   ./scripts/run_bag_map.sh pi                  # Pi/HDMI + LIVE /velodyne_points_wifi
#   ./scripts/run_bag_map.sh pi lidar_sample     # Pi + recorded bag (bench)
#
# Live cloud topic default: /velodyne_points (Mac) or /velodyne_points_wifi (Pi)
# Override with: LIVE_LIDAR_TOPIC=/your/topic ./scripts/run_bag_map.sh
#
# Mac uses micromamba env ros_view. Pi sources the ROS underlay (see ROS_UNDERLAY).
# Close the map window or Ctrl+C to stop everything.
set -eo pipefail

WS="$(cd "$(dirname "$0")/.." && pwd)"
REPO="$(cd "$WS/.." && pwd)"
RENDERER_BIN="$REPO/map_renderer/build/renderer"
BAG_SRC_TOPIC="/velodyne_points_wifi"
CLUSTER_TOPIC="/velodyne_points_bag"
LIVE_LIDAR_TOPIC="${LIVE_LIDAR_TOPIC:-}"
PLAY_DOMAIN="${ROS_DOMAIN_ID:-42}"
# setup.bash cannot be sourced from zsh: BASH_SOURCE is empty, so the prefix
# becomes $PWD and Jazzy looks for project_ws/setup.sh.
if [[ -n "${ZSH_VERSION:-}" ]]; then
  ROS_UNDERLAY="${ROS_UNDERLAY:-/opt/ros/jazzy/setup.zsh}"
else
  ROS_UNDERLAY="${ROS_UNDERLAY:-/opt/ros/jazzy/setup.bash}"
fi
TAKEMURA_RENDERER_SOCKET="${TAKEMURA_RENDERER_SOCKET:-/tmp/takemura-renderer.sock}"
TAKEMURA_VIEW_FILE="${TAKEMURA_VIEW_FILE:-/tmp/takemura-view}"
TAKEMURA_VIEW_SAVED="${TAKEMURA_VIEW_SAVED:-$HOME/.config/takemura-view}"
# pan_x pan_y zoom rotation, as tuned on the holographic fan.
TAKEMURA_VIEW_DEFAULT="${TAKEMURA_VIEW_DEFAULT:-130 -50 0.8 180}"

TARGET="mac"
BAG=""

# Parse: [mac|pi] [bag_name_or_path]
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

# Default is LIVE (no bag). Bag is only used when explicitly passed.
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

if [[ -z "$LIVE_LIDAR_TOPIC" ]]; then
  if [[ "$TARGET" == "pi" ]]; then
    # On the Pi the sensor is on ethernet, so use the full local cloud.
    # The thinned Wi-Fi topic only exists for remote viewers.
    LIVE_LIDAR_TOPIC="/velodyne_points"
  else
    LIVE_LIDAR_TOPIC="/velodyne_points_wifi"
  fi
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

setup_mac_live_dds() {
  PI_HOST="${PI_HOST:-linn.local}"
  XML="$HOME/fastdds_takemura.xml"

  if [[ -z "$MAC_IP" ]]; then
    IFACE=$(route -n get default 2>/dev/null | awk '/interface:/{print $2}')
    MAC_IP=$(ipconfig getifaddr "${IFACE:-en0}" 2>/dev/null)
  fi
  if [[ -z "$MAC_IP" ]]; then
    echo "error: this Mac has no IPv4 address - is WiFi connected?" >&2
    exit 1
  fi

  if [[ -z "$PI_IP" ]]; then
    PI_IP=$(dscacheutil -q host -a name "$PI_HOST" 2>/dev/null | awk '/^ip_address:/{print $2; exit}')
  fi
  if [[ -z "$PI_IP" ]]; then
    PI_IP=$(ping -c1 -W 2000 "$PI_HOST" 2>/dev/null | head -1 | sed -n 's/.*(\([0-9.]*\)).*/\1/p')
  fi
  if [[ -z "$PI_IP" ]]; then
    echo "error: could not resolve $PI_HOST - is the Pi powered and on the same WiFi?" >&2
    echo "       you can bypass mDNS with: PI_IP=192.168.0.11 $0" >&2
    exit 1
  fi

  echo "dds mac=$MAC_IP  pi=$PI_IP"
  cat > "$XML" << EOF
<?xml version="1.0" encoding="UTF-8" ?>
<dds xmlns="http://www.eprosima.com/XMLSchemas/fastRTPS_Profiles">
  <profiles>
    <transport_descriptors>
      <transport_descriptor>
        <transport_id>udp_transport</transport_id>
        <type>UDPv4</type>
        <interfaceWhiteList>
          <address>$MAC_IP</address>
          <address>127.0.0.1</address>
        </interfaceWhiteList>
      </transport_descriptor>
    </transport_descriptors>
    <participant profile_name="takemura_client" is_default_profile="true">
      <rtps>
        <userTransports>
          <transport_id>udp_transport</transport_id>
        </userTransports>
        <useBuiltinTransports>false</useBuiltinTransports>
        <builtin>
          <metatrafficUnicastLocatorList>
            <locator><udpv4><address>$MAC_IP</address></udpv4></locator>
          </metatrafficUnicastLocatorList>
          <initialPeersList>
            <locator><udpv4><address>$MAC_IP</address></udpv4></locator>
            <locator><udpv4><address>127.0.0.1</address></udpv4></locator>
            <locator><udpv4><address>$PI_IP</address></udpv4></locator>
          </initialPeersList>
        </builtin>
        <defaultUnicastLocatorList>
          <locator><udpv4><address>$MAC_IP</address></udpv4></locator>
        </defaultUnicastLocatorList>
      </rtps>
    </participant>
  </profiles>
</dds>
EOF
  export FASTRTPS_DEFAULT_PROFILES_FILE="$XML"
  export FASTDDS_DEFAULT_PROFILES_FILE="$XML"
  unset ROS_DISCOVERY_SERVER
  unset ROS_LOCALHOST_ONLY
  unset CYCLONEDDS_URI
}

setup_pi_env() {
  if [[ -n "${ZSH_VERSION:-}" && "$ROS_UNDERLAY" == *.bash ]]; then
    ROS_UNDERLAY="${ROS_UNDERLAY%.bash}.zsh"
  fi
  if [[ ! -f "$ROS_UNDERLAY" ]]; then
    echo "error: ROS underlay not found at $ROS_UNDERLAY" >&2
    echo "       set ROS_UNDERLAY=/path/to/setup.zsh (or setup.bash) and retry" >&2
    exit 1
  fi
  unset COLCON_CURRENT_PREFIX
  # shellcheck disable=SC1090
  source "$ROS_UNDERLAY"
  cd "$WS"
  local ws_setup
  if [[ -n "${ZSH_VERSION:-}" && -f install/setup.zsh ]]; then
    ws_setup="$WS/install/setup.zsh"
  elif [[ -f install/setup.bash ]]; then
    ws_setup="$WS/install/setup.bash"
  else
    echo "error: workspace not built. Run:" >&2
    echo "  cd $WS && colcon build && source install/setup.bash" >&2
    exit 1
  fi
  source "$ws_setup"
  # zsh nices background jobs by default, which throttles the renderer and the
  # ROS nodes started below.
  unsetopt bgnice 2>/dev/null || true
  export DISPLAY="${DISPLAY:-:0}"
  export TAKEMURA_RENDERER_FULLSCREEN="${TAKEMURA_RENDERER_FULLSCREEN:-1}"
  # Footprints linger long enough to walk over and look at the fan.
  export TAKEMURA_TRAIL_SECONDS="${TAKEMURA_TRAIL_SECONDS:-2}"
  # Prefer the fan's native 1080p. HDMI-2 is a firmware-forced dummy.
  if command -v xrandr >/dev/null 2>&1 && [[ -n "${DISPLAY:-}" ]]; then
    xrandr --output HDMI-1 --mode 1920x1080 --primary 2>/dev/null || \
      xrandr --output HDMI-1 --mode 1280x720 --primary 2>/dev/null || true
    xrandr --output HDMI-2 --off 2>/dev/null || true
  fi
  # xrandr can re-enable DPMS. Keep HDMI awake or the fan freezes on the last frame.
  xset s off 2>/dev/null || true
  xset s noblank 2>/dev/null || true
  xset -dpms 2>/dev/null || true
}

# A hand-typed run belongs to the SSH session, so closing the terminal kills
# it. Worse, it first kills the service's nodes, so the two keep restarting
# each other. Point the user at systemctl instead.
if [[ "$TARGET" == "pi" && "${TAKEMURA_SERVICE:-0}" != "1" && -z "$BAG" ]]; then
  if command -v systemctl >/dev/null 2>&1 &&
     systemctl is-active --quiet takemura-map 2>/dev/null; then
    echo "takemura-map.service is already running the live map on this Pi."
    echo
    echo "It survives SSH logouts and reboots, so there is nothing to start."
    echo "  watch:   journalctl -u takemura-map -f"
    echo "  restart: sudo systemctl restart takemura-map"
    echo "  stop:    sudo systemctl stop takemura-map"
    echo
    echo "To run by hand anyway (dies when this terminal closes):"
    echo "  sudo systemctl stop takemura-map && $0 pi"
    exit 0
  fi
fi

if [[ "$TARGET" == "pi" ]]; then
  setup_pi_env
else
  setup_mac_env
  if [[ -z "$BAG" ]]; then
    setup_mac_live_dds
  fi
fi

export ROS_DOMAIN_ID="$PLAY_DOMAIN"
export RMW_IMPLEMENTATION="${RMW_IMPLEMENTATION:-rmw_fastrtps_cpp}"
if [[ "$TARGET" == "pi" || -n "$BAG" ]]; then
  # Sensor and pipeline share this machine, so use the default local
  # transports. The Wi-Fi profile forces unicast-only discovery through a
  # short initial-peers list, which silently loses nodes once more than a
  # handful of participants run at once, and it skips shared memory.
  export ROS_AUTOMATIC_DISCOVERY_RANGE="${ROS_AUTOMATIC_DISCOVERY_RANGE:-LOCALHOST}"
  unset FASTRTPS_DEFAULT_PROFILES_FILE
  unset FASTDDS_DEFAULT_PROFILES_FILE
  unset ROS_DISCOVERY_SERVER
else
  export ROS_AUTOMATIC_DISCOVERY_RANGE="${ROS_AUTOMATIC_DISCOVERY_RANGE:-SUBNET}"
fi
export DETECTION_TOPIC="${DETECTION_TOPIC:-/person_detections}"
export TRACK_TOPIC="${TRACK_TOPIC:-/person_tracks}"
export TRACK_INFO_TOPIC="${TRACK_INFO_TOPIC:-/person_tracks_info}"
if [[ -z "$BAG" ]]; then
  export TRACK_DT="${TRACK_DT:-0.12}"
fi
export TAKEMURA_RENDERER_SOCKET

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
  sleep 0.4
}

echo "Stopping any leftover bag/cluster/track/bridge/renderer processes…"
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
echo "flow: cloud -> cluster -> tracker -> bridge -> map_renderer"
echo "GUI: raylib footprint map (close window or Ctrl+C to stop)"
echo "Wait ~7 s for cluster floor/warmup before footprints appear."
echo

# Renderer must bind the Unix socket BEFORE the bridge starts sending.
echo "Opening holographic footprint map (binds $TAKEMURA_RENDERER_SOCKET)…"
export TAKEMURA_RENDERER_SOCKET
unset TAKEMURA_RENDERER_DEMO
# /tmp is wiped on reboot, so a tuned view is kept in $TAKEMURA_VIEW_SAVED and
# copied back on every start.
export TAKEMURA_VIEW_FILE
if [[ ! -f "$TAKEMURA_VIEW_FILE" ]]; then
  if [[ -f "$TAKEMURA_VIEW_SAVED" ]]; then
    cp "$TAKEMURA_VIEW_SAVED" "$TAKEMURA_VIEW_FILE"
  else
    echo "$TAKEMURA_VIEW_DEFAULT" > "$TAKEMURA_VIEW_FILE"
  fi
fi
echo "view=$(cat "$TAKEMURA_VIEW_FILE")  (edit: echo \"PANX PANY ZOOM ROT\" > $TAKEMURA_VIEW_FILE)"
"$RENDERER_BIN" &
RENDERER_PID=$!
PIDS+=($RENDERER_PID)
sleep 2
if ! kill -0 "$RENDERER_PID" 2>/dev/null; then
  echo "error: map_renderer failed to start" >&2
  echo "       on Pi: DISPLAY=:0  MESA_GL_VERSION_OVERRIDE=3.3" >&2
  exit 1
fi

# Shared shape gate. Recorded clouds are already thinned, so the bag keeps the
# defaults for everything else.
CLUSTER_ROS_ARGS=(
  -p 'process_period:=0.10'
  -p 'min_cluster_size:=3'
  -p 'points_at_one_meter:=15.0'
  -p 'min_height_m:=0.35'
  -p 'min_aspect_ratio:=0.55'
  -p 'min_verticality:=0.25'
  -p 'require_motion:=false'
  -p 'static_threshold:=0.55'
  -p 'warmup_frames:=8'
  -p 'move_distance_m:=0.18'
)

if [[ -z "$BAG" ]]; then
  # The live sensor sends ~21k points per scan from a high mount, so the
  # background has to fade slowly or someone standing still is erased, and
  # range/leaf are trimmed to keep the Pi at real time.
  CLUSTER_ROS_ARGS+=(
    -p 'max_range:=12.0'
    -p 'leaf_size:=0.08'
    -p 'bg_alpha:=0.01'
    -p 'static_threshold:=0.60'
    -p 'warmup_frames:=15'
  )
fi

if [[ -n "$BAG" ]]; then
  ros2 bag play "$BAG" --loop --remap "${BAG_SRC_TOPIC}:=${CLUSTER_TOPIC}" &
  PIDS+=($!)
  sleep 1
  ros2 run person_cluster person_cluster_node --ros-args "${CLUSTER_ROS_ARGS[@]}" &
else
  # Subscribe straight to the sensor topic: an extra relay node would add a
  # copy of every 21k-point scan and show up as lag on the fan.
  ros2 run person_cluster person_cluster_node --ros-args \
    -r "${CLUSTER_TOPIC}:=${LIVE_LIDAR_TOPIC}" \
    "${CLUSTER_ROS_ARGS[@]}" &
fi
PIDS+=($!)
sleep 0.5

ros2 run person_tracker track_node &
PIDS+=($!)
sleep 0.5

ros2 run renderer_bridge renderer_bridge_node &
PIDS+=($!)

echo
echo "Pipeline running. Watch the map window for LIVE IPC status."
echo "Close the map window or press Ctrl+C to stop."
echo

# GLFW/xrandr can turn screensaver back on when the window opens.
xset s off 2>/dev/null || true
xset s noblank 2>/dev/null || true
xset -dpms 2>/dev/null || true

# Keep going until the renderer dies (systemd then restarts the whole unit).
# If a ROS node exits, start it again so the fan is not left on a frozen frame.
typeset -i watchdog=0
while kill -0 "$RENDERER_PID" 2>/dev/null; do
  if ! pgrep -f 'person_cluster_node' >/dev/null 2>&1; then
    echo "person_cluster_node exited; restarting"
    if [[ -n "$BAG" ]]; then
      ros2 run person_cluster person_cluster_node --ros-args "${CLUSTER_ROS_ARGS[@]}" &
    else
      ros2 run person_cluster person_cluster_node --ros-args \
        -r "${CLUSTER_TOPIC}:=${LIVE_LIDAR_TOPIC}" \
        "${CLUSTER_ROS_ARGS[@]}" &
    fi
    PIDS+=($!)
  fi
  if ! pgrep -f 'person_tracker.track_node|person_tracker/track_node' >/dev/null 2>&1; then
    echo "track_node exited; restarting"
    ros2 run person_tracker track_node &
    PIDS+=($!)
  fi
  if ! pgrep -f 'renderer_bridge_node' >/dev/null 2>&1; then
    echo "renderer_bridge_node exited; restarting"
    ros2 run renderer_bridge renderer_bridge_node &
    PIDS+=($!)
  fi
  watchdog+=1
  if (( watchdog == 1 || watchdog % 30 == 0 )); then
    xset s off 2>/dev/null || true
    xset s noblank 2>/dev/null || true
    xset -dpms 2>/dev/null || true
  fi
  sleep 1
done
