# Person cluster pipeline

`person_cluster_node` turns a Velodyne `PointCloud2` into person centroids on `/person_detections` (`geometry_msgs/PoseArray`). It subscribes to `/velodyne_points_bag` (remapped to the live cloud in the exhibit launch).

### 1. Floor estimation

Before anything else, the node builds a histogram of point heights (`z`) over the first `floor_frames_` frames (bin size 5 cm). It skips a small noise budget of the lowest returns, then finds the densest bin within `floor_search_span_` above that lowest slab; that peak is the floor. This makes the height band relative to the floor rather than the sensor mount, so it still works whether the sensor is mounted low or high. If `auto_floor` is false, `floor_z` is used as-is.

### 2. Preprocessing

- **Height band:** keep only points between `floor_z_ + z_offset_min_` and `floor_z_ + z_offset_max_`.
- **Range filter:** drop points whose horizontal distance \sqrt{x^2+y^2} exceeds `max_range_`.
- **Voxel downsampling:** PCL's `VoxelGrid` averages points inside each `leaf_size_` cube into one representative point, cutting density while keeping overall shape.

### 3. Background subtraction (voxel occupancy EMA)

Each downsampled point is snapped to an integer grid cell (`VoxelKey`, cell size `bg_voxel_size_`) and stored in a hash map. Every frame, cells touched by the current cloud have their occupancy score nudged toward 1.0; untouched cells decay toward 0.0 (and are erased when the score falls below 0.01). Cells with score ≥ `static_threshold_` are treated as static background and filtered out. A cell that is touched constantly climbs high; a cell touched briefly (someone walking through) stays low.

An initial `warmup_frames_` period runs with a faster learning rate (`bg_warm_alpha_`) so the background model converges quickly before detection starts; afterward it uses the slower rate (`bg_alpha_`) for stability.

### 4. Clustering

Surviving (non-static) points are grouped with PCL's `EuclideanClusterExtraction` on a KD-tree: points within `cluster_tolerance_` of each other (directly or through a chain of neighbors) join the same cluster. Clusters outside `[min_cluster_size_, max_cluster_size_]` points are discarded by PCL.

This is *not* DBSCAN; there is no core/border/noise distinction or minimum density; so a thin bridge of points can merge two objects. That risk is reduced upstream by downsampling and background subtraction, and downstream by the shape gate.

### 5. Shape measurement and human-shape gate

For each cluster, `measure()` computes:

- centroid (`cx, cy, cz`); mean position of the cluster points
- bounding-box width (horizontal diagonal of the XY extents) and height (vertical extent)
- range (horizontal distance of the centroid from the sensor)
- verticality; PCA of the cluster covariance; the eigenvector for the largest eigenvalue is the principal axis, and `|axis · z|` measures how upright the blob is. If the cluster has fewer than `verticality_min_points_` points, verticality is treated as 1.0 (orientation is not used to reject sparse blobs)

`looks_human()` then rejects clusters that are:

- too sparse for their distance (fewer points than `max(min_points_floor_, round(points_at_one_meter_ / range))`)
- too short/tall (`min_height_m_` / `max_height_m_`)
- too narrow/wide (`min_width_m_` / `max_width_m_`)
- too squat (`height / width` below `min_aspect_ratio_`)
- insufficiently vertical (`min_verticality_`)

### 6. Motion gate (candidate memory)

When `require_motion_` is true, surviving human-shaped centroids are matched frame-to-frame to a list of `Candidate` objects with greedy nearest-neighbor association in XY within `assoc_radius_` (one detection per candidate). Each candidate keeps a short XY history (`history_frames_`).

A candidate is published only if:

1. it was matched this frame (`unseen == 0`), and
2. it has moved at least `move_distance_m_` within its history window (max distance from the oldest history point to any later point), **or** it did so recently enough that `frame_index_ - last_move_frame ≤ hold_frames_`

So a person who pauses briefly can keep publishing for `hold_frames_` while still being detected; unmatched candidates are not published, and are deleted after more than `drop_frames_` consecutive misses.

This is a prediction-free motion filter; no Kalman filter and no Hungarian assignment; only greedy nearest-neighbor matching plus a displacement-over-window check. Stable multi-person IDs are handled later by `person_tracker`.

## Parameters


| Parameter                               | Default         | Description                                                                         |
| --------------------------------------- | --------------- | ----------------------------------------------------------------------------------- |
| `auto_floor`                            | `true`          | Estimate floor height automatically from the point histogram                        |
| `floor_z`                               | `-0.85`         | Fallback/initial floor height (m) if not auto-estimated                             |
| `floor_search_span`                     | `0.60`          | Height range searched for the floor above the lowest returns (m)                    |
| `z_offset_min` / `z_offset_max`         | `0.30` / `2.00` | Height band above the floor to keep (m)                                             |
| `max_range`                             | `15.0`          | Max horizontal distance from sensor to consider (m)                                 |
| `leaf_size`                             | `0.07`          | Voxel downsampling cube size (m)                                                    |
| `bg_voxel_size`                         | `0.30`          | Background model cell size (m)                                                      |
| `floor_frames`                          | `15`            | Frames used to estimate the floor                                                   |
| `warmup_frames`                         | `20`            | Frames used to seed the background model at a faster rate                           |
| `bg_alpha`                              | `0.05`          | Background EMA rate after warmup                                                    |
| `bg_warm_alpha`                         | `0.30`          | Background EMA rate during warmup                                                   |
| `static_threshold`                      | `0.35`          | EMA score above which a cell is considered static background                        |
| `cluster_tolerance`                     | `0.45`          | Max distance between points to be in the same cluster (m)                           |
| `min_cluster_size` / `max_cluster_size` | `4` / `1200`    | Cluster point-count bounds                                                          |
| `min_height_m` / `max_height_m`         | `0.45` / `2.10` | Human height gate (m)                                                               |
| `min_width_m` / `max_width_m`           | `0.08` / `1.00` | Human footprint width gate (m)                                                      |
| `min_aspect_ratio`                      | `0.90`          | Minimum height/width ratio                                                          |
| `min_verticality`                       | `0.45`          | Minimum alignment of principal axis with vertical                                   |
| `verticality_min_points`                | `12`            | Minimum points needed to trust the verticality estimate                             |
| `points_at_one_meter`                   | `45.0`          | Expected point density at 1 m range; scales the minimum point count with distance   |
| `min_points_floor`                      | `5`             | Absolute floor on required point count regardless of distance                       |
| `assoc_radius`                          | `0.70`          | Max distance to match a new detection to an existing candidate (m)                  |
| `history_frames`                        | `10`            | Length of position history kept per candidate                                       |
| `move_distance_m`                       | `0.35`          | Minimum displacement within the history window to count as movement (m)             |
| `hold_frames`                           | `15`            | Frames a candidate keeps publishing after its last confirmed movement               |
| `drop_frames`                           | `5`             | Consecutive unmatched frames before a candidate is deleted (`unseen > drop_frames`) |
| `require_motion`                        | `true`          | If false, publish all human-shaped clusters regardless of movement                  |
| `debug_stats`                           | `true`          | Log per-frame pipeline counts                                                       |
| `process_period`                        | `0.12`          | Minimum seconds between processed frames (throttles input rate)                     |


## Notes

- Floor estimation and background warmup must both finish before detections are published; until then the node publishes empty `PoseArray` messages each frame.
- Setting `require_motion` to `false` skips the candidate motion gate and publishes raw human-shaped cluster centroids every frame; useful for debugging the shape gate alone.

