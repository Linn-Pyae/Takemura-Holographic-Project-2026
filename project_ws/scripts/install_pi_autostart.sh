#!/usr/bin/env bash
# Install systemd units so the holographic map runs on the Pi by itself:
# started at boot, restarted if it dies, independent of any SSH session.
#
#   sudo ./scripts/install_pi_autostart.sh
#
# Stop it:      sudo systemctl stop takemura-map
# Start it:     sudo systemctl start takemura-map
# Boot on/off:  sudo systemctl enable|disable takemura-map takemura-x
# Watch logs:   journalctl -u takemura-map -f
set -euo pipefail

if [[ $EUID -ne 0 ]]; then
  echo "error: run with sudo" >&2
  exit 1
fi

MAP_USER="${MAP_USER:-linn}"
MAP_HOME="$(getent passwd "$MAP_USER" | cut -d: -f6)"
WS="$MAP_HOME/Takemura-Holographic-Project-2026/project_ws"
WRAPPER="$MAP_HOME/ros2_config/run-map.sh"

if [[ ! -x "$WS/scripts/run_bag_map.sh" ]]; then
  echo "error: $WS/scripts/run_bag_map.sh not found" >&2
  exit 1
fi

# logind defaults to RemoveIPC=yes: when the last session of $MAP_USER ends it
# deletes that user's POSIX shared memory, which is where Fast DDS keeps its
# local channels. The nodes stay alive but stop seeing each other, so the map
# freezes a few seconds after an SSH logout. Lingering keeps the user from ever
# counting as fully logged out.
install -d /etc/systemd/logind.conf.d
cat > /etc/systemd/logind.conf.d/10-takemura-keep-ipc.conf << 'EOF'
[Login]
RemoveIPC=no
EOF
loginctl enable-linger "$MAP_USER" || true
systemctl restart systemd-logind || true

install -d -o "$MAP_USER" -g "$MAP_USER" "$MAP_HOME/ros2_config"
cat > "$WRAPPER" << EOF
#!/usr/bin/env bash
# Wait for the X server, then hand over to the map pipeline.
export HOME="$MAP_HOME"
export DISPLAY=:0
export TAKEMURA_RENDERER_FULLSCREEN=1
# Lets the launcher tell a service start apart from a hand-typed one.
export TAKEMURA_SERVICE=1

for _ in \$(seq 1 60); do
  [[ -e /tmp/.X11-unix/X0 ]] && break
  sleep 1
done

# Wipe the previous HDMI frame so the fan does not keep an old image.
xsetroot -solid black 2>/dev/null || true
# No keyboard on the exhibit: default X blanks HDMI after 10 min and the
# fan then holds the last frame, which looks like a freeze.
xset s off 2>/dev/null || true
xset s noblank 2>/dev/null || true
xset -dpms 2>/dev/null || true

cd "$WS"
exec ./scripts/run_bag_map.sh pi
EOF
chmod 755 "$WRAPPER"
chown "$MAP_USER":"$MAP_USER" "$WRAPPER"

install -d /etc/X11/xorg.conf.d
cat > /etc/X11/xorg.conf.d/10-takemura-noblank.conf << 'EOF'
Section "ServerFlags"
    Option "BlankTime" "0"
    Option "StandbyTime" "0"
    Option "SuspendTime" "0"
    Option "OffTime" "0"
EndSection
EOF

cat > /etc/systemd/system/takemura-x.service << 'EOF'
[Unit]
Description=X server for the Takemura holographic map
After=systemd-user-sessions.service

[Service]
Type=simple
# -s 0 -dpms: never blank HDMI (the fan otherwise freezes on the last frame).
ExecStart=/usr/bin/Xorg :0 vt7 -nolisten tcp -ac -s 0 -dpms
Restart=always
RestartSec=2

[Install]
WantedBy=multi-user.target
EOF

cat > /etc/systemd/system/takemura-map.service << EOF
[Unit]
Description=Takemura holographic footprint map
Requires=takemura-x.service
After=takemura-x.service velodyne-driver.service velodyne-pointcloud.service
Wants=velodyne-driver.service velodyne-pointcloud.service

[Service]
Type=simple
User=$MAP_USER
Group=$MAP_USER
WorkingDirectory=$WS
Environment=HOME=$MAP_HOME
Environment=DISPLAY=:0
ExecStart=$WRAPPER
Restart=always
RestartSec=5
# The launcher traps SIGTERM and shuts its own ROS nodes down.
KillMode=mixed
TimeoutStopSec=15

[Install]
WantedBy=multi-user.target
EOF

# A hand-started Xorg or pipeline would fight the units for the display.
pkill -9 -f 'run_bag_map.sh' 2>/dev/null || true
pkill -9 -f 'map_renderer/build/renderer' 2>/dev/null || true
if ! systemctl is-active --quiet takemura-x; then
  pkill -9 Xorg 2>/dev/null || true
  sleep 1
fi

systemctl daemon-reload
systemctl enable takemura-x takemura-map
systemctl restart takemura-x
sleep 3
systemctl restart takemura-map

echo
systemctl --no-pager --lines=0 status takemura-x takemura-map || true
echo
echo "installed. map starts at boot and restarts on failure."
echo "  stop:  sudo systemctl stop takemura-map"
echo "  logs:  journalctl -u takemura-map -f"
