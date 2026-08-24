#!/bin/zsh
# Live Open3D viewer for the Pi's Velodyne cloud over WiFi.
#
# Both machines get their addresses from DHCP, so nothing here is hardcoded:
# the Pi is found over mDNS and the Fast DDS profile is rebuilt on every run.
# Override either address with PI_IP=... / MAC_IP=... if mDNS is unavailable.
set -e

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
  echo "       you can bypass mDNS with: PI_IP=192.168.0.64 $0" >&2
  exit 1
fi

echo "mac=$MAC_IP  pi=$PI_IP"

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

export MAMBA_ROOT_PREFIX="$HOME/micromamba"
eval "$("$HOME/micromamba/bin/micromamba" shell hook -s zsh)"
micromamba activate ros_view

export ROS_DOMAIN_ID=42
export RMW_IMPLEMENTATION=rmw_fastrtps_cpp
# Both names needed (ros2cli vs rclpy / Fast DDS versions)
export FASTRTPS_DEFAULT_PROFILES_FILE="$XML"
export FASTDDS_DEFAULT_PROFILES_FILE="$XML"
unset ROS_DISCOVERY_SERVER
unset ROS_LOCALHOST_ONLY
unset CYCLONEDDS_URI

# The daemon caches the discovery graph from the previous profile and will hide
# the Pi's topics if the addresses changed since the last run.
ros2 daemon stop > /dev/null 2>&1 || true

cd "$(dirname "$0")"
exec python -u view.py
