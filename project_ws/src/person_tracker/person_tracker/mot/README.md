# Multi-object tracker

`MultiObjectTracker` in `tracker.py` assigns stable IDs and display names to per-frame person detections. It has no ROS or rendering dependency. The live exhibit wraps it in `track_node.py`, which feeds `/person_detections` and publishes `/person_tracks` plus `/person_tracks_info`.

Default live wiring uses `HungarianMatcher` plus `KalmanFilter2D` from `motion.py`. Without a motion model, association falls back to each track's last position (or a simple velocity extrapolation on the track itself).

### 1. Frame update (`update`)

Each call is one frame:

1. If a `motion_model` is set, `predict()` every active track and collect predicted XY positions.
2. Run the configured `AssociationStrategy` to match tracks to detections within `max_association_distance`.
3. For each match: optionally `correct()` the motion model, then `Track.update()` with the detection.
4. For unmatched tracks: `mark_missed()` (increment `missed_frames`).
5. Drop tracks with `missed_frames > max_missed_frames`, and `remove()` them from the motion model.
6. Create a new track for each unmatched detection (numeric `id`, random Harry Potter `name`).

Input detection order only affects the naming order of brand-new tracks.

### 2. Greedy nearest-neighbor association

`GreedyNearestNeighborMatcher` builds all (track, detection) pairs whose distance is ≤ `max_distance`, sorts them by distance ascending, and accepts pairs that do not reuse a track or detection already matched.

Reference position per track:

- `predicted_positions[i]` if the caller passed Kalman (or other) predictions, else
- `track.predicted_position()` when `use_velocity_prediction` is true, else
- `track.current_position`

This is fast and local; when people cross, a short-distance wrong pair can win before a globally better assignment is considered.

### 3. Hungarian association

`HungarianMatcher` builds a square cost matrix of size `n_tracks + n_detections`:

- Real track–detection cells: Euclidean distance if ≤ `max_distance`, otherwise a large forbidden cost.
- Dummy columns/rows: cost `max_distance + 1` so a track or detection may stay unmatched.
- Dummy–dummy: cost 0.

It solves the assignment with a Kuhn–Munkres (Hungarian) implementation (`_solve`, O(n³)), then keeps only real pairs that still pass the distance gate. This minimizes total association cost for the frame, which reduces ID swaps when paths cross compared with greedy matching.

### 4. Kalman motion model (`motion.py`)

`KalmanFilter2D` is a constant-velocity filter with state `[x, y, vx, vy]`. It is pure Python (no NumPy).

- `initialize`: place the track at the detection with zero velocity; large initial velocity variance.
- `predict`: advance state with `dt` and inflate covariance with process noise; return predicted XY used for association.
- `correct`: update state from the matched detection (measurement variance on XY).
- `remove`: drop filter state when the track is deleted.

`predict()` is assumed once per frame per track before association.

### 5. Track lifecycle and names

- New tracks get the next integer `id` and a random unused name from `DEFAULT_NAMES` (Harry Potter cast). If every base name is taken, names become `NAME_2`, `NAME_3`, …
- `Track.current_position` follows the matched detection (observation), not the internal Kalman estimate exposed to the renderer.
- Tracks survive up to `max_missed_frames` consecutive misses, then are deleted.

## Parameters

Defaults below are from `TrackerConfig` / `KalmanFilter2D`. The ROS node overrides some via environment variables (shown in parentheses).

| Parameter / env | Default | Description |
| --- | --- | --- |
| `max_association_distance` (`TRACK_MAX_DISTANCE`) | `2.0` (`1.5` in node) | Max XY distance to associate a track and detection (m) |
| `max_missed_frames` (`TRACK_MAX_MISSED`) | `3` (`5` in node) | Consecutive misses allowed before a track is deleted |
| `TRACK_DT` | `0.2` | Kalman time step (s); ~5 Hz Velodyne bags |
| Kalman `process_variance` | `0.1` | Process noise scale (node uses `0.1`) |
| Kalman `measurement_variance` | `0.1` | Measurement noise (node uses `0.05`) |
| Association strategy | Greedy in library default; **Hungarian** in `track_node` | Matcher used for the frame |
| Motion model | `None` in library default; **KalmanFilter2D** in `track_node` | Optional predictor for association |

## Notes

- Cluster-side `Candidate` motion gating is separate; this module owns stable multi-person IDs for the map labels.
- Offline simulation and benchmarks under `tracking/` exercise the same MOT ideas; the exhibit path uses this `person_tracker.mot` package.
- Adapters (`MappingDetectionAdapter` / `MappingTrackAdapter`) convert dict / PoseArray shapes without changing tracker math.
