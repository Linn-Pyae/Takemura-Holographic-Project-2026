# LiDAR Multi-Object Tracking / Simulation Benchmark

LiDAR動体検出と描画の間に置く、外部フォーマット非依存の追跡モジュールです。実機なしで性能と弱点を測るため、固定seedのシミュレーター、Ground Truth照合、Greedy対Hungarian＋Kalman比較、可視化、長時間テストも含みます。

現時点では実LiDARとの接続前です。READMEに記載する評価値を含め、入力はすべてシミュレーションで生成しており、実機環境での性能を保証するものではありません。

## 想定する入出力

LiDAR側には、1フレームごとに人物クラスタの代表位置を並べた入力を想定しています。仮の辞書形式は次のとおりです。`x`、`y`は必須のメートル単位座標、`timestamp`は任意の秒単位時刻です。それ以外の値は`metadata`として保持されます。

```python
raw_frame = [
    {"x": 1.25, "y": -0.40, "timestamp": 12.3, "z": 0.9, "point_count": 42},
    {"x": -0.80, "y": 0.15, "timestamp": 12.3},
]
```

内部の`Detection`は、`position: Position2D(x, y)`、任意の`timestamp`、任意の読み取り専用`metadata`で構成します。LiDAR固有形式は`DetectionAdapter`でこの型へ変換し、1フレーム分の`Sequence[Detection]`を`tracker.update()`へ渡します。

Trackerは現在有効な`list[Track]`を返します。`MappingTrackAdapter`を使った後段向け辞書出力は、`id`、`name`、`x`、`y`、`previous_position`、`history`、`missed_frames`、`timestamp`、`metadata`を含みます。

`Track`は追跡IDと表示名、現在／直前位置、観測位置履歴、連続未検出フレーム数、最終時刻、最新Detectionのmetadataを保持します。未対応のフレームでは位置と履歴を更新せず`missed_frames`を増やし、設定値を超えると削除されます。

## 追跡方式

- Greedy方式: 各TrackとDetectionの距離候補を近い順に確定するNearest Neighbor方式です。局所的に決めるため、密集時には全体として最適な対応にならない場合があります。
- Hungarian Algorithm + Kalman Filter: 全TrackとDetectionの距離コストをHungarian Algorithmで一括割り当てし、各Trackの定速度Kalman Filterで予測位置を作ります。Kalman状態は`[x, y, vx, vy]`です。

## 基本的な実行方法

リポジトリのルートから実行します。追跡本体とコンソールデモに外部依存はありません。

```powershell
# Greedy方式
python -m tracking.demo_console

# Hungarian Algorithm + Kalman Filter
python -m tracking.demo_advanced
```

## ファイル構成

- `models.py`: 内部型 `Position2D`、`Detection`、`Track`
- `tracker.py`: Track管理、Greedy Nearest Neighbor、Hungarian Algorithm
- `motion.py`: 状態 `[x, y, vx, vy]` の純Python `KalmanFilter2D`
- `adapters.py`: 外部入力からDetection、Trackから後段形式への仮変換
- `simulator.py`: Ground Truthとノイズ・欠落・shuffle付きDetection生成
- `evaluation.py`: ID switch等の評価指標とフレーム詳細の集計
- `benchmark.py`: 全シナリオで2方式を比較するCLI
- `stress_tests.py`: シミュレーターと評価器の回帰テスト
- `long_run_test.py`: 数千〜数万フレームの速度・メモリ・history・ID監視
- `demo_visualization.py`: Ground Truth、Detection、Track、switch地点の描画
- `test_tracker.py`: Tracker本体の通常・ストレス・方式比較テスト

## 標準シナリオ

`create_standard_scenarios()` は次の17シナリオを生成します。乱数seedは固定でき、同じ設定ならDetection座標と順序を再現できます。

1. 2人の接近・同一座標への重なり
2. 3人同時交差
3. 4人同時交差
4. Detectionノイズ ±0.1m
5. Detectionノイズ ±0.2m
6. Detectionノイズ ±0.3m
7. 近接中の急な方向転換
8. 停止人物の近くを別人物が通過
9. 1フレーム／2フレームのDetection欠落
10. `max_missed_frames`と同数の欠落
11. `max_missed_frames`を超える欠落と再登場
12. Detection順の毎フレームshuffle
13. 新しい人物の途中参加
14. 複数人物の途中退出
15. 4人が異なる速度で移動
16. 1人だけ急加速・急減速
17. ノイズ＋4人交差＋欠落＋shuffleの複合条件

Ground Truth人物ID（A、B、C、D）とTrackerの名前（HARRY、RON、HERMIONE、DRACO）は独立しています。Ground Truth IDはテスト専用metadataで、追跡アルゴリズムのコスト計算には使いません。

## 評価指標

- `total_frames`: シナリオの総フレーム数
- `total_detections`: Trackerへ渡したDetection総数
- `id_switch_count`: 同じGround Truth人物へ前回と異なるTrack IDが割り当てられた回数
- `track_fragmentation_count`: Detectionが1フレーム以上途切れた後、再び割り当てられた回数
- `lost_track_count`: 人物がシーンに存在する間に、それまでのTrackが削除された回数
- `new_track_count`: シナリオ中に生成された一意なTrack ID数
- `false_reassignment_count`: ある人物用に最初に生成されたTrackが別人物へ割り当てられた観測数
- `mean_position_error` / `max_position_error`: Track現在位置とGround Truthのユークリッド距離[m]
- `successful_id_recoveries`: `max_missed_frames`以内の欠落後に同じIDで復帰した回数
- `id_switch_frames`: ID switchが起きたフレーム番号
- `track_id_history`: Ground Truth人物ごとのフレーム別Track ID
- `tracking_success_rate`: Detectionのうち人物の初期Track IDを維持した割合
- `average_processing_time_ms` / `max_processing_time_ms`: `tracker.update()`の処理時間

現状の`Track.current_position`は観測Detection位置なので、位置誤差は主にDetectionノイズを表します。Kalman内部推定位置そのものの誤差ではありません。ID switch回数だけでなく、成功率、誤再割当、fragmentationも合わせて判断してください。

## Greedy vs Hungarian＋Kalman比較

`outputs`ディレクトリから実行します。

```powershell
python -m tracking.benchmark
```

1シナリオだけ詳しく確認する例です。

```powershell
python -m tracking.benchmark `
  --scenario combined_noise_crossing_dropout `
  --details `
  --show-records
```

JSONへ全指標と各フレームの以下の値を保存できます。

- `ground_truth_person_id`
- true X/Y
- detection X/Y
- assigned Track ID/name
- Track位置、missed_frames、ID switch判定

```powershell
python -m tracking.benchmark --output-json benchmark_results.json
```

現行設定の代表結果では、ノイズ±0.3mでGreedy 3回対Hungarian＋Kalman 2回、急反転で3回対1回でした。完全な同一点交差は両方式とも解消できません。複合条件ではID switch数がGreedy 2回対新方式5回でも、初期ID維持率は74.7%対96.2%となりました。このように単一指標だけでは優劣を決められません。

## 最接近距離×Detectionノイズ総当たり

2人が反対方向へ移動し、中央フレームで指定した最接近距離になる平行すれ違いを使います。標準では距離 `2.0, 1.0, 0.5, 0.3, 0.1, 0.0m`、ノイズ `0.0, 0.1, 0.2, 0.3m`、2方式の計48条件です。

```powershell
python -m tracking.parameter_sweep
```

次の2ファイルを保存します。

- `proximity_noise_sweep.csv`: 48条件の全指標。Excelで開けるUTF-8 BOM付きCSV
- `proximity_noise_sweep_table.txt`: セルを `IDSW / success%` で表した方式別matrixと不安定条件一覧

不安定条件は `ID switch > 0` または `success rate < 100%` と定義しています。距離、ノイズ、Tracker・Kalman設定、seed、フレーム数は変更できます。

```powershell
python -m tracking.parameter_sweep `
  --distances 2.0 1.0 0.5 0.3 0.1 0.0 `
  --noise-levels 0.0 0.1 0.2 0.3 `
  --seed 20260811 `
  --csv my_sweep.csv `
  --table my_sweep.txt
```

固定seedの単発評価なので、結果が距離・ノイズに対して必ず単調になるとは限りません。統計的な傾向は、次の100 seed統計評価で確認できます。

### 100 seed統計評価

標準ではseed `20260811`〜`20260910`の100個を使用し、6距離×4ノイズ×2方式、合計4800試行を実行します。同じseedのDetection列をGreedyとHungarian＋Kalmanへ入力します。

```powershell
python -m tracking.statistical_sweep --seed-count 100
```

現行設定（seed `20260811`〜`20260910`）の結果では、全24条件×100 seedの合計で、ID Switch総数はGreedyの1,971回に対してHungarian＋Kalmanは1,310回、ID Switchが1回以上発生した試行は503件に対して328件でした。多くの近接距離・noise条件でHungarian＋KalmanがID Switchを抑制する傾向を確認しました。ただし完全な同一点や一部の条件では同等または悪化する場合があり、シミュレーション結果は実LiDARでの性能を保証しません。

条件ごとに次を集計します。

- IDSWが1回以上発生した試行の割合（failure rate）
- 平均IDSW、最大IDSW
- 平均success rate
- success rateの母集団標準偏差
- 平均false reassignment

生成ファイル:

- `proximity_noise_multiseed_trials.csv`: 4800試行すべて。各行にrandom seedを保存
- `proximity_noise_multiseed_summary.csv`: 48条件の統計量
- `proximity_noise_failure_rate_table.txt`: 方式別failure rate matrix
- `proximity_noise_failure_rate_heatmap.png`: 両方式を共通0〜100%スケールで比較したヒートマップ

seed範囲や出力先も変更できます。

```powershell
python -m tracking.statistical_sweep `
  --seed-start 1000 `
  --seed-count 100 `
  --trials-csv trials.csv `
  --summary-csv summary.csv `
  --table failure_table.txt `
  --heatmap failure_heatmap.png
```

## 方式の切り替え

```python
from tracking.motion import KalmanFilter2D
from tracking.tracker import HungarianMatcher, MultiObjectTracker, TrackerConfig

tracker = MultiObjectTracker(
    TrackerConfig(max_association_distance=1.5, max_missed_frames=3),
    association_strategy=HungarianMatcher(),
    motion_model=KalmanFilter2D(
        process_variance=0.1,
        measurement_variance=0.1,
        initial_position_variance=1.0,
        initial_velocity_variance=100.0,
    ),
)
```

Greedyは `GreedyNearestNeighborMatcher()` を指定し、`motion_model`を省略します。

## 設定パラメータ

`SimulationConfig`に以下をまとめています。`benchmark.py`のコマンドライン引数からも変更できます。

- `max_association_distance`
- `max_missed_frames`
- `kalman_process_noise`
- `kalman_measurement_noise`
- `initial_position_covariance`
- `initial_velocity_covariance`
- `noise_amplitude`
- `random_seed`
- `number_of_people`（主に長時間テスト）

例:

```powershell
python -m tracking.benchmark `
  --max-association-distance 2.0 `
  --max-missed-frames 5 `
  --kalman-process-noise 0.2 `
  --kalman-measurement-noise 0.15 `
  --seed 12345
```

## テスト

```powershell
# 既存Trackerの通常・ストレス・比較テスト（16件）
python -m unittest -v tracking.test_tracker

# 17シナリオの生成・再現性・評価指標テスト
python -m unittest -v tracking.stress_tests

# 両方
python -m unittest -v tracking.test_tracker tracking.stress_tests
```

評価テストは「ID switchが0であること」を合格条件にしていません。性能が悪いシナリオでも、再現可能に実行され、指標が正しく集計されれば合格します。

## 可視化

最初に任意依存を導入します。

```powershell
python -m pip install -r tracking/requirements.txt
```

静止画をPNG保存:

```powershell
python -m tracking.demo_visualization `
  --scenario combined_noise_crossing_dropout `
  --method hungarian_kalman `
  --save benchmark_visualization.png
```

リアルタイムアニメーション:

```powershell
python -m tracking.demo_visualization --animate
```

GIF保存:

```powershell
python -m tracking.demo_visualization --animate --save tracking.gif
```

Ground Truth軌道、Detection、Track軌跡、名前、ID switch地点（赤いX）を表示します。

## 長時間テスト

```powershell
# 両方式を1万フレーム
python -m tracking.long_run_test --frames 10000 --method both

# 人数やseedを変更
python -m tracking.long_run_test `
  --frames 50000 `
  --method hungarian_kalman `
  --people 4 `
  --seed 12345 `
  --output-json long_run.json
```

次を報告します。

- traced memoryの現在値、peak、サンプル間増加量
- 最大history長
- Track ID作成数、同時Track数、削除済みIDの再利用異常
- 平均／最大処理時間
- 最初の10%と最後の10%の処理時間比
- 例外の有無

現在の`Track.history`には上限がなく、長時間稼働ではフレーム数に比例してメモリが増える可能性があります。今回は評価を優先して追跡ロジックを変更していません。長時間結果を確認後、`max_history_length`をTracker設定へ追加するか判断してください。

## VS Code

「実行とデバッグ」から次を選択できます。

- `MOT: Greedy vs Hungarian+Kalman評価`
- `MOT: 距離×ノイズ総当たり評価`
- `MOT: 距離×ノイズ 100-seed統計評価`
- `MOT: 評価機能テスト`
- `MOT: 長時間テスト (10000 frames)`
- `MOT: 評価結果PNG保存`
- 既存デモ・全テスト

新しい項目が見えない場合は `Ctrl+Shift+P` → `Developer: Reload Window` を実行してください。

## 現在分かっている限界

- XY位置だけでは完全に同じ座標へ重なった人物を識別できない
- 定速度Kalmanは急反転・急加速で予測を外すことがある
- Hungarianは全体距離を最小化するが、距離コスト自体が人物識別に十分とは限らない
- `max_missed_frames`を超えて削除された人物は新規IDになる
- historyは現在無制限

次の改善段階では、今回の結果を根拠に、クラスタサイズ、点数、高さ、進行方向、Re-ID、history上限などを検討します。
