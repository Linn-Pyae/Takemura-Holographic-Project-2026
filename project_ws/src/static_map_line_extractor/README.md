# static_map_line_extractor

`/static_map/debug_grid`のstatic候補セルを、表示向けの大まかな2D形状へ変換する
独立ROS 2 C++ packageです。細かな輪郭をそのまま線分化せず、細長い塊は壁の
中心線、まとまった小さな塊は回転可能な長方形として表現します。

人物検出、tracking、renderer、Unix socketには直接接続しません。

## Pipeline

```text
/velodyne_points_bag (PointCloud2)
  -> static_map_projector
  -> /static_map/debug_grid (OccupancyGrid)
  -> static_map_line_extractor
  -> /static_map/lines (MarkerArray)
       ├─ static_environment_walls  : 太い壁中心線
       └─ static_environment_blocks : 長方形の外周
```

入力Gridの値0は自由空間ではなく、static未確定として扱います。
`occupied_threshold`以上のセルだけが抽出対象です。

## Topics

| 種類 | Topic | Message type | QoS |
| --- | --- | --- | --- |
| Subscribe | `/static_map/debug_grid` | `nav_msgs/msg/OccupancyGrid` | reliable, transient local, depth 1 |
| Publish | `/static_map/lines` | `visualization_msgs/msg/MarkerArray` | reliable, transient local, depth 1 |

出力MarkerArrayには2個の`Marker.LINE_LIST`を格納します。

- `static_environment_walls`: 壁1本につき2点。太い青緑色
- `static_environment_blocks`: 長方形1個につき4辺8点。細い橙色

`header.frame_id`とstampは入力OccupancyGridから継承します。既存bridgeとの互換性を
保つため、どちらも通常の`LINE_LIST`です。bridge側では壁と長方形の辺が線分として
平坦化されます。

## Extraction algorithm

1. `occupied_threshold`以上のセルから2値画像を作る
2. `maximum_line_gap`に対応するmorphology closeで小さな隙間を閉じる
3. 8近傍connected componentsを取り、8セル未満の小ノイズを除く
4. 各componentへOpenCV `minAreaRect`を当てはめる
5. 長さと縦横比が壁条件を満たす細長いcomponentを、1本の中心線へ置き換える
6. それ以外を、見やすい最小寸法を持つ回転長方形へ置き換える
7. 角度・横ずれ・端点間隔が近い壁中心線を1本へ結合する
8. Grid座標へoriginとyawを適用してメートル座標へ変換する

これは壁や机などを厳密に認識するsemantic modelではなく、点群の幾何形状を
「壁らしい線」と「その他の箱」に大まかに当てはめる表示用モデルです。

## Coordinate conversion

Grid点`(column, row)`はセル中心としてローカル座標へ変換した後、
OccupancyGrid originの位置とyawを適用します。

```text
local_x = (column + 0.5) * resolution
local_y = (row    + 0.5) * resolution

world_x = origin_x + cos(yaw)*local_x - sin(yaw)*local_y
world_y = origin_y + sin(yaw)*local_x + cos(yaw)*local_y
```

`X=LiDAR前方`、`Y=LiDAR左方向`を維持します。renderer用の`(-y, -x)`変換は
行いません。

## Parameters

| Parameter | Default | Unit / meaning |
| --- | ---: | --- |
| `occupied_threshold` | `100` | 抽出対象になる最小Grid値 |
| `maximum_line_gap` | `0.20` | close処理でつなぐ小さな隙間 `[m]` |
| `minimum_component_cells` | `8` | 残すcomponentの最小セル数 |
| `wall_min_length` | `1.20` | 壁とみなす最小長さ `[m]` |
| `wall_min_aspect_ratio` | `3.50` | 壁とみなす最小縦横比 |
| `minimum_block_size` | `0.40` | 長方形を見せる最小一辺 `[m]` |
| `wall_merge_angle_degrees` | `12.0` | 結合できる壁同士の最大角度差 `[deg]` |
| `wall_merge_distance` | `0.30` | 結合できる壁同士の最大横ずれ `[m]` |
| `wall_merge_gap` | `0.50` | 結合できる壁端点間の最大隙間 `[m]` |
| `wall_line_width` | `0.12` | RVizの壁線幅 `[m]` |
| `block_line_width` | `0.07` | RVizの長方形線幅 `[m]` |
| `update_period` | `0.20` | 最短更新周期 `[s]`。0なら全Gridを処理 |

互換用の`min_contour_area`、`contour_epsilon`、`minimum_line_length`も残して
あります。前2つはデバッグ輪郭、最後は壁の最小長さの下限に使われます。

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

より大まかな表示にする例:

```bash
ros2 run static_map_line_extractor static_map_line_extractor_node --ros-args \
  -p minimum_component_cells:=12 \
  -p wall_min_length:=1.50 \
  -p wall_min_aspect_ratio:=4.0 \
  -p minimum_block_size:=0.50 \
  -p wall_merge_gap:=0.70
```

RViz2ではFixed Frameを入力Gridのframe（サンプルでは`velodyne`）にし、
MarkerArray displayで`/static_map/lines`を選択します。

## Existing MCAP verification

3つのterminalを使用します。

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

Terminal 3 — shape extractor:

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

C++ unit testと同等のROS不要Python testで、人工Gridから長い壁、長方形、
孤立ノイズ、複数物体、壁の結合、origin/yaw座標変換を検証します。

```bash
colcon test --packages-select static_map_line_extractor

python project_ws/src/static_map_line_extractor/offline/test_line_extractor_offline.py
```

実MCAP 3件をROSなしで検証する方法は`validation/README.md`を参照してください。
