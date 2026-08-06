#!/bin/zsh
# Live Open3D viewer for Pi Velodyne cloud over TakemuraLab WiFi
export MAMBA_ROOT_PREFIX="$HOME/micromamba"
eval "$("$HOME/micromamba/bin/micromamba" shell hook -s zsh)"
micromamba activate ros_view

export ROS_DOMAIN_ID=0
export RMW_IMPLEMENTATION=rmw_fastrtps_cpp
# Both names needed (ros2cli vs rclpy / Fast DDS versions)
export FASTRTPS_DEFAULT_PROFILES_FILE="$HOME/fastdds_takemura.xml"
export FASTDDS_DEFAULT_PROFILES_FILE="$HOME/fastdds_takemura.xml"
unset ROS_DISCOVERY_SERVER
unset ROS_LOCALHOST_ONLY
unset CYCLONEDDS_URI

cd "$(dirname "$0")"
exec python -u view.py
