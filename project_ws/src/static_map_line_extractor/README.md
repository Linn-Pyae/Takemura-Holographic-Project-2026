# static_map_line_extractor

`/static_map/debug_grid`のstatic候補セルを、簡略化した2D輪郭線へ変換する
独立ROS 2 C++ packageです。人物検出、tracking、renderer、bridge、
Unix socketには接続しません。

## Pipeline

```text
/velodyne_points_bag (PointCloud2)
  -> static_map_projector
  -> /static_map/debug_grid (OccupancyGrid)
  -> static_map_line_extractor
  -> /static_map/lines (MarkerArray / LINE_LIST)
```

入力Gridの値0は自由空間ではなく、static未確定として扱います。
`occupied_threshold`以上のセルだけが輪郭抽出対象です。

## Topics

| 種類 | Topic | Message type | QoS |
| --- | --- | --- | --- |
| Subscribe | `/static_map/debug_grid` | `nav_msgs/msg/OccupancyGrid` | reliable, transient local, depth 1 |
| Publish | `/static_map/lines` | `visualization_msgs/msg/MarkerArray` | reliable, transient local, depth 1 |

出力は1個の`Marker.LINE_LIST`に集約され、各線分につき2点
`(x1, y1, 0)`、`(x2, y2, 0)`を格納します。`header.frame_id`とstampは
入力OccupancyGridから継承します。

## Extraction algorithm

1. `occupied_threshold`以上のセルだけを255とする2値画像を作る
2. `maximum_line_gap`に対応するOpenCV morphology closeで小さな隙間を閉じる
3. connected componentsで小さな孤立領域を除く
4. OpenCV `findContours(RETR_EXTERNAL, CHAIN_APPROX_SIMPLE)`で外輪郭を抽出
5. `approxPolyDP`で輪郭を簡略化
6. 簡略化された隣接頂点を線分化し、短い線分を除く
7. Grid cell座標をメートル座標へ変換する

Hough Transform、RANSAC、壁分類は使用していません。

## Coordinate conversion

輪郭点`(column, row)`はセル中心として、まずGridローカル座標へ変換します。

```text
local_x = (column + 0.5) * resolution
local_y = (row    + 0.5) * resolution
```

その後、OccupancyGrid originの位置とyawを適用します。

```text
world_x = origin_x + cos(yaw)*local_x - sin(yaw)*local_y
world_y = origin_y + sin(yaw)*local_x + cos(yaw)*local_y
```

`X=LiDAR前方`、`Y=LiDAR左方向`を維持します。renderer用の`(-y, -x)`変換は
行いません。

## Parameters

| Parameter | Default | Unit / meaning |
| --- | ---: | --- |
| `occupied_threshold` | `100` | 輪郭対象になる最小Grid値 |
| `min_contour_area` | `0.0` | 最小輪郭面積 `[m^2]`。細い壁を残すため既定は0 |
| `contour_epsilon` | `0.10` | `approxPolyDP`の許容誤差 `[m]` |
| `minimum_line_length` | `0.30` | publishする最小線分長 `[m]` |
| `maximum_line_gap` | `0.20` | morphology closeで接続する隙間の目安 `[m]` |
| `minimum_component_cells` | `3` | 残すconnected componentの最小セル数 |
| `line_width` | `0.04` | RViz Marker線幅 `[m]` |
| `update_period` | `0.20` | 最短更新周期 `[s]`。0なら全Gridを処理 |

## Build

ROS 2 JazzyとOpenCV development packageが必要です。

```bash
sudo apt install libopencv-dev
source /opt/ros/jazzy/setup.bash
cd project_ws
colcon build --packages-select static_map_line_extractor
source install/setup.bash
```

## Run

```bash
ros2 run static_map_line_extractor static_map_line_extractor_node
```

parameter変更例:

```bash
ros2 run static_map_line_extractor static_map_line_extractor_node --ros-args \
  -p contour_epsilon:=0.15 \
  -p minimum_line_length:=0.50 \
  -p maximum_line_gap:=0.30 \
  -p minimum_component_cells:=5
```

RViz2ではFixed Frameを入力Gridのframe（サンプルでは`velodyne`）にし、
MarkerArray displayで`/static_map/lines`を選択します。

## Existing MCAP verification

3つのterminalを使用します。既存スクリプトの変更は不要です。

Terminal 1 — bag:

```bash
source /opt/ros/jazzy/setup.bash
cd project_ws
ros2 bag play ../lidar_sample --loop \
  --remap /velodyne_points_wifi:=/velodyne_points_bag
```

Terminal 2 — projector:

```bash
source /opt/ros/jazzy/setup.bash
cd project_ws
source install/setup.bash
ros2 run static_map_projector static_map_projector_node
```

Terminal 3 — line extractor:

```bash
source /opt/ros/jazzy/setup.bash
cd project_ws
source install/setup.bash
ros2 run static_map_line_extractor static_map_line_extractor_node
```

確認:

```bash
ros2 topic info /static_map/lines -v
ros2 topic echo /static_map/lines --once
```

## Tests

C++ unit testはROS messageを使用せず、人工Gridから長い壁、四角形、孤立ノイズ、
複数物体、origin/yaw座標変換を検証します。

```bash
colcon test --packages-select static_map_line_extractor
colcon test-result --verbose
```

ROS 2がないPCでは、`offline/test_line_extractor_offline.py`で同じOpenCV処理を
確認できます。

```bash
python -m pip install -r \
  project_ws/src/static_map_line_extractor/offline/requirements.txt
python project_ws/src/static_map_line_extractor/offline/test_line_extractor_offline.py
```

## Not implemented

- static_map_bridge / Unix socket
- map_renderer / HDMI / holographic fan
- 人物trackとの合成
- Hough Transform / RANSAC / 壁・物体分類
