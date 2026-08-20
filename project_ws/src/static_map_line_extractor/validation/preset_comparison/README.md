# Static-map coarse-shape preset comparison

実MCAPから同一のstatic Occupancy Gridを1回生成し、粗形状抽出の3 presetを
切り替えて比較するオフライン確認ツールです。

| Preset | 目的 | 主な違い |
| --- | --- | --- |
| `Detailed` | 小さな形も多く残す | 5セル以上、壁1.0 m以上、結合は控えめ |
| `Balanced` | 既定の見栄え | 8セル以上、壁1.2 m以上、通常の結合 |
| `Minimal` | 最も大まかに表示 | 12セル以上、壁1.5 m以上、結合を強める |

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
- `extracted_lines.png`（壁線と長方形モデル）
- `grid_lines_overlay.png`（Gridへの重ね合わせ）
- `metrics.json`

各bagには`preset_comparison.png`、全体には`preset_metrics.csv`と
`preset_metrics.json`を保存します。
