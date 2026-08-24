# static_map_projector

既存の人物検出・追跡・描画パイプラインから独立して、3D LiDAR点群を
デバッグ用の2D Occupancy Gridへ投影するROS 2 C++ packageです。

この第1段階では、壁や家具などが複数フレームにわたって観測されるかを
Gridで確認することだけを目的にしています。人物検出、tracking、線抽出、
Unix socket、既存rendererとの接続は行いません。

## 入出力

| 種類 | Topic | Message type | QoS |
| --- | --- | --- | --- |
| Subscribe | `/velodyne_points_bag` | `sensor_msgs/msg/PointCloud2` | SensorDataQoS, depth 1 |
| Publish | `/static_map/debug_grid` | `nav_msgs/msg/OccupancyGrid` | reliable, transient local, depth 1 |

出力Gridの`header.frame_id`は入力PointCloudの値をそのまま使います。
座標変換（TF）はこのnode内では行いません。そのため、静的地図として蓄積する
場合はLiDARが固定されていること、または入力点群が既に固定座標系へ変換済みで
あることが前提です。

## 処理

1. PointCloud2の`x`, `y`, `z`（FLOAT32）を読み取る
2. NaN、Inf、範囲外の点を除外する
3. 起動直後の15処理フレームから床のZを推定する（`auto_floor:=true`の場合）
4. 床からの高さ範囲で点を絞る
5. XY平面へ投影し、`voxel_size_xy`単位のセルへ量子化する
6. 観測されたセルのscoreを加算し、未観測セルのscoreを減算する
7. scoreが`static_observation_threshold`以上のセルを100（occupied）としてpublishする

このGridはレイキャスティングを行っていないため、値0は「自由空間」ではなく
「まだstaticと確定していないセル」という意味です。

## Parameters

| Parameter | Default | 説明 |
| --- | ---: | --- |
| `auto_floor` | `true` | 最初の15処理フレームで床Zを自動推定する |
| `floor_z` | `-0.85` m | 自動推定を無効化した場合の床Z。推定失敗時のfallbackでも使用 |
| `min_height_above_floor` | `0.30` m | 残す点の床からの最小高さ |
| `max_height_above_floor` | `2.00` m | 残す点の床からの最大高さ |
| `max_range` | `15.0` m | LiDAR原点からのXY最大距離およびGrid半径 |
| `voxel_size_xy` | `0.10` m | Occupancy Gridのセルサイズ |
| `static_observation_threshold` | `5` | static候補になるために必要な観測score |
| `map_update_period` | `0.20` s | 点群を処理・publishする最短周期。0なら全メッセージ処理 |

値は起動時にROS parameterで指定してください。

## Build

リポジトリの`project_ws`で実行します（ROS 2 Jazzyの例）。

```bash
source /opt/ros/jazzy/setup.bash
cd project_ws
colcon build --packages-select static_map_projector
source install/setup.bash
```

既存packageをbuild対象から外したい場合にも、上の
`--packages-select static_map_projector`を使用できます。

## Run

既定のtopicを使う場合:

```bash
ros2 run static_map_projector static_map_projector_node
```

実機topic `/velodyne_points` を直接使う場合:

```bash
ros2 run static_map_projector static_map_projector_node --ros-args \
  -r /velodyne_points_bag:=/velodyne_points
```

parameterを変更する例:

```bash
ros2 run static_map_projector static_map_projector_node --ros-args \
  -p auto_floor:=false \
  -p floor_z:=-0.85 \
  -p min_height_above_floor:=0.20 \
  -p max_height_above_floor:=2.20 \
  -p max_range:=12.0 \
  -p voxel_size_xy:=0.10 \
  -p static_observation_threshold:=8 \
  -p map_update_period:=0.20
```

## rosbagで単体確認

既存のbagスクリプトを変更せず、2つのterminalで手動実行できます。
bag内topicが`/velodyne_points_wifi`の場合の例です。

Terminal 1:

```bash
source /opt/ros/jazzy/setup.bash
cd project_ws
ros2 bag play ../lidar_sample --loop \
  --remap /velodyne_points_wifi:=/velodyne_points_bag
```

Terminal 2:

```bash
source /opt/ros/jazzy/setup.bash
cd project_ws
source install/setup.bash
ros2 run static_map_projector static_map_projector_node
```

確認コマンド:

```bash
ros2 topic info /static_map/debug_grid -v
ros2 topic hz /static_map/debug_grid
ros2 topic echo /static_map/debug_grid --once
```

RViz2ではFixed FrameをPointCloudの`frame_id`に合わせ、Map displayへ
`/static_map/debug_grid`を指定してください。最初の床推定15処理フレーム中は
Gridがpublishされません。

## 対象外（第1段階）

- Hough Transform / RANSAC / 輪郭線抽出
- `/static_map_lines`
- static_map_bridge / Unix socket
- map_renderer / ホログラフィックファン統合

これらはGrid取得を確認した後の別段階として追加します。
