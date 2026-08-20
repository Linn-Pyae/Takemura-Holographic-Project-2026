# Real MCAP pipeline validation

ROS 2を使用せず、リポジトリ内の実MCAPを直接decodeして、現在のdefault設定の
`static_map_projector -> static_map_line_extractor`処理を再現します。

既存C++ nodeや既存オフラインコードは変更しません。

リポジトリrootから実行します。

```powershell
python -m pip install -r `
  project_ws/src/static_map_line_extractor/validation/requirements.txt

python project_ws/src/static_map_line_extractor/validation/validate_real_mcap_pipeline.py `
  lidar_sample/lidar_sample_0.mcap `
  lidar_sample_2/lidar_sample_2_0.mcap `
  lidar_sample_3/lidar_sample_3_0.mcap
```

各bagについて次を生成します。

- `raw_xy_projection.png`
- `static_occupancy_grid.png`
- `static_map_lines.png`
- `static_map_comparison.png`
- `static_grid_with_lines.png`
- `validation_summary.json`

出力先は既定で`validation/output/<mcap stem>/`です。
