# project_ws — LiDAR person cluster + track

Self-contained ROS 2 (Jazzy / RoboStack) workspace:

```text
PointCloud2 (/velodyne_points_bag)
        │
        ▼
person_cluster (C++)  →  /person_detections  (PoseArray centroids)
        │
        ▼
person_tracker (Python) →  /person_tracks     (PoseArray tracked poses)
        │                 →  /person_tracks_info (JSON labels)
        ▼
renderer_bridge (C++)  →  Unix socket (/tmp/takemura-renderer.sock)
        │
        ▼
map_renderer (raylib) →  footprint map GUI (Mac window or Pi HDMI)
```

Optional debug GUI: `viz_node` (matplotlib 2D) via `run_bag_pipeline.sh`.

Ship this folder (`project_ws`) plus a recorded bag (or live cloud on the remapped topic). Offline simulation benchmarks stay in the repo-root `tracking/` package; runtime MOT used by ROS lives in `person_tracker/person_tracker/mot/`.

## Layout

```text
project_ws/
  scripts/run_bag_map.sh          # live or bag + footprint map only
  scripts/run_bag_map_both.sh     # live or bag + Open3D 3D + footprint map
  scripts/run_bag_pipeline.sh   # bag + cluster + track + matplotlib 2D map
  scripts/run_bag_view3d.sh     # bag + Open3D (OpenGL) raw cloud view
  scripts/run_bag_both.sh       # bag + cluster + track + Open3D + 2D map
  src/
    person_cluster/             # C++ LiDAR clustering
    person_tracker/             # Python ROS node + MOT core + 2D viz
    renderer_bridge/            # C++ ROS -> map_renderer Unix socket
  build/ install/ log/          # created by colcon (gitignored)
```

## Prerequisites

- micromamba env `ros_view` (RoboStack Jazzy) with:
  - `rclcpp`, `rclpy`, `sensor_msgs`, `geometry_msgs`
  - `pcl`, `ros-jazzy-pcl-conversions`
  - `matplotlib`
  - `colcon`

Install PCL stack if missing:

```bash
micromamba activate ros_view
micromamba install -y -c robostack-jazzy -c conda-forge \
  ros-jazzy-pcl-conversions pcl
```

## Build

```bash
micromamba activate ros_view
cd project_ws
colcon build
source install/setup.zsh   # bash: install/setup.bash
```

Rebuild one package after edits:

```bash
colcon build --packages-select person_cluster person_tracker
source install/setup.zsh
```

## Run (recommended)

### Footprint map (Mac dev or Pi HDMI)

From `project_ws`:

```bash
./scripts/run_bag_map.sh                     # Mac + LIVE /velodyne_points (footprint map only)
./scripts/run_bag_map_both.sh                # Mac + LIVE + Open3D 3D + footprint map
./scripts/run_bag_map_both.sh mac lidar_sample   # Mac + bag + both GUIs
./scripts/run_bag_map.sh mac lidar_sample    # Mac + recorded bag (map only)
./scripts/run_bag_map_both.sh pi             # Pi + LIVE + both GUIs + HDMI
LIVE_LIDAR_TOPIC=/velodyne_points_wifi ./scripts/run_bag_map_both.sh
```

Default is **live LiDAR** on `/velodyne_points` (override with `LIVE_LIDAR_TOPIC`).
Pass a bag name only when you want recorded replay.

Mac uses `micromamba activate ros_view`. Pi sources `ROS_UNDERLAY` (default
`/opt/ros/jazzy/setup.bash`) and sets `DISPLAY=:0` for HDMI output.

Build `map_renderer` once before first run:

```bash
cd ../map_renderer
cmake -S . -B build && cmake --build build
```

Build `renderer_bridge` after adding it:

```bash
colcon build --packages-select renderer_bridge
source install/setup.zsh   # or setup.bash on Pi
```

### Matplotlib debug map

From `project_ws` (expects bag next to the parent repo, e.g. `../lidar_sample`):

```bash
./scripts/run_bag_pipeline.sh
./scripts/run_bag_pipeline.sh lidar_sample_2
BAG=/absolute/path/to/bag ./scripts/run_bag_pipeline.sh
```

This starts bag + cluster + tracker and opens a **matplotlib 2D map**:

- Yellow dots = current cluster detections
- Colored lines = track trails (history)
- Labels = track name + ID (e.g. HARRY (1))

Close the window or press Ctrl+C to stop everything.

The script uses `ROS_DOMAIN_ID=42` so live Pi traffic on domain 0 does not mix in.

## Inspect raw cloud (Open3D / OpenGL)

To check LiDAR noise in 3D (no clustering/tracking):

```bash
./scripts/run_bag_view3d.sh
./scripts/run_bag_view3d.sh lidar_sample_2
```

## Both GUIs together

Bag + cluster + track + Open3D cloud + matplotlib 2D map:

```bash
./scripts/run_bag_both.sh
./scripts/run_bag_both.sh lidar_sample_2
```

## Run (manual)

All terminals: `micromamba activate ros_view`, `export ROS_DOMAIN_ID=42`, and `source project_ws/install/setup.zsh`.

```bash
# T1 — bag
ros2 bag play /path/to/lidar_sample --loop \
  --remap /velodyne_points_wifi:=/velodyne_points_bag

# T2 — cluster
ros2 run person_cluster person_cluster_node

# T3 — track
ros2 run person_tracker track_node

# T4 — 2D map
ros2 run person_tracker viz_node
```

## Topics

| Topic | Type | Role |
|-------|------|------|
| `/velodyne_points_bag` | `sensor_msgs/PointCloud2` | input cloud (remapped from bag) |
| `/person_detections` | `geometry_msgs/PoseArray` | cluster centroids |
| `/person_tracks` | `geometry_msgs/PoseArray` | tracked people |
| `/person_tracks_info` | `std_msgs/String` (JSON) | track id/name for the GUI |

Override with env vars: `DETECTION_TOPIC`, `TRACK_TOPIC`, `TRACK_INFO_TOPIC`, `TRACK_MAX_DISTANCE`, `TRACK_MAX_MISSED`, `TRACK_DT`, `TRACK_VIZ_HISTORY`.

## Notes

- Every process must share the same `ROS_DOMAIN_ID`.
- **How a person is found.** `person_cluster_node` runs five stages, and the log line
  `cloud=… candidates=… clusters=… human_shaped=… published=…` shows how many points
  survive each one:
  1. *Floor lock* (`floor_frames`, ~15 frames): the floor is the densest horizontal
     slab in the z histogram. The sensor sits ~0.85 m above the floor, so the height
     band must be floor-relative — this is set automatically. Set `auto_floor:=false`
     plus `floor_z` to pin it manually.
  2. *Band + range + downsample*: keep `floor_z + z_offset_min` to `floor_z + z_offset_max`
     within `max_range`, then voxel-downsample at `leaf_size`.
  3. *Background model* (`warmup_frames`, ~20 frames): every 30 cm voxel carries an
     exponential moving average of how often it is occupied (`bg_alpha`, seeded faster
     with `bg_warm_alpha`). Anything above `static_threshold` is background. Because the
     average keeps updating, furniture that only flickers in and out still converges to
     static, unlike a one-shot frozen map.
  4. *Human-shape gate*: a person is a narrow, vertically elongated blob, so a cluster
     must satisfy `min_height_m`/`max_height_m`, `min_width_m`/`max_width_m`,
     `min_aspect_ratio` and `min_verticality` (PCA long axis vs vertical). Point count is
     range-aware via `points_at_one_meter`, since a cluster at 10 m is legitimately sparse.
  5. *Motion memory*: a blob is only published once it has travelled `move_distance_m`
     within the last `history_frames`, and it keeps being published for `hold_frames`
     after it stops. This is what removes stationary objects without deleting a walking
     person mid-stride the way per-point frame differencing did. Set `require_motion:=false`
     to publish every human-shaped cluster.
- The sample bags run at ~5 Hz, which is why `track_node` defaults to `TRACK_DT=0.2`.
- Rebuild after C++ edits: `colcon build --packages-select person_cluster && source install/setup.zsh`