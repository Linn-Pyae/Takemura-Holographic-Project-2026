# Static-map line preset comparison

実MCAPから同一のstatic Occupancy Gridを1回生成し、Current / Mild / Cleanの
3 presetだけを切り替えて比較するオフライン確認ツールです。

既存のprojector、line extractor、ROS pipelineは変更しません。

リポジトリrootから実行します。

```powershell
python -m pip install -r `
  project_ws/src/static_map_line_extractor/validation/preset_comparison/requirements.txt

python project_ws/src/static_map_line_extractor/validation/preset_comparison/compare_line_presets.py `
  lidar_sample/lidar_sample_0.mcap `
  lidar_sample_2/lidar_sample_2_0.mcap `
  lidar_sample_3/lidar_sample_3_0.mcap
```

各bag・presetに以下を保存します。

- `static_occupancy_grid.png`
- `extracted_lines.png`
- `grid_lines_overlay.png`
- `metrics.json`

各bagには`preset_comparison.png`、全体には`preset_metrics.csv`と
`preset_metrics.json`を保存します。
