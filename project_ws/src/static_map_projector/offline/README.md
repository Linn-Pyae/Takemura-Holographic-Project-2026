# Offline static-map preview

ROS 2を使用せず、実際のROS 2 MCAPに記録された
`sensor_msgs/msg/PointCloud2`を直接decodeして、
`static_map_projector_node.cpp`と同じ基本条件の2D画像を生成します。

既存node、topic、renderer、Unix socketには接続しません。

リポジトリrootから実行してください。

```powershell
python -m pip install -r project_ws/src/static_map_projector/offline/requirements.txt

python project_ws/src/static_map_projector/offline/offline_static_map.py `
  lidar_sample/lidar_sample_0.mcap `
  --topic /velodyne_points_wifi `
  --output-dir project_ws/src/static_map_projector/offline/output
```

Linux/macOSの場合も引数は同じです。

```bash
python3 project_ws/src/static_map_projector/offline/offline_static_map.py \
  lidar_sample/lidar_sample_0.mcap \
  --topic /velodyne_points_wifi \
  --output-dir project_ws/src/static_map_projector/offline/output
```

生成物:

- `raw_xy_projection.png`
- `static_occupancy_map.png`
- `static_map_comparison.png`
- `offline_static_map_summary.json`

全parameterは`--help`で確認できます。既定値はROS 2 nodeと同じです。
