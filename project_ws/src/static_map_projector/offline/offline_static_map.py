#!/usr/bin/env python3
"""Create static-map preview PNGs directly from a ROS 2 MCAP bag.

This tool deliberately does not import ROS 2.  It decodes PointCloud2 records
from MCAP and mirrors the filtering, floor calibration, grid quantization, and
per-cell observation score used by static_map_projector_node.cpp.
"""

from __future__ import annotations

import argparse
import json
import math
from pathlib import Path
from typing import Any, Iterable

try:
    import matplotlib

    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    import numpy as np
    from mcap_ros2.reader import read_ros2_messages
except ImportError as error:  # pragma: no cover - gives a useful CLI message
    raise SystemExit(
        "Offline dependencies are missing. Install them with:\n"
        "  python -m pip install -r offline/requirements.txt"
    ) from error


FLOOR_BIN_SIZE = 0.05
FLOOR_RANGE_MIN = -5.0
FLOOR_RANGE_MAX = 5.0
FLOOR_SEARCH_SPAN = 0.60
FLOOR_CALIBRATION_FRAMES = 15
POINT_FIELD_FLOAT32 = 7


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Generate raw and persistent XY maps from a real MCAP PointCloud2 topic."
    )
    parser.add_argument("mcap", type=Path, help="Input .mcap file")
    parser.add_argument(
        "--topic", default="/velodyne_points_wifi", help="PointCloud2 topic in MCAP"
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=Path(__file__).resolve().parent / "output",
    )
    parser.add_argument("--auto-floor", action=argparse.BooleanOptionalAction, default=True)
    parser.add_argument("--floor-z", type=float, default=-0.85)
    parser.add_argument("--min-height-above-floor", type=float, default=0.30)
    parser.add_argument("--max-height-above-floor", type=float, default=2.00)
    parser.add_argument("--max-range", type=float, default=15.0)
    parser.add_argument("--voxel-size-xy", type=float, default=0.10)
    parser.add_argument("--static-observation-threshold", type=int, default=5)
    parser.add_argument("--map-update-period", type=float, default=0.20)
    parser.add_argument(
        "--max-frames",
        type=int,
        default=0,
        help="Maximum processed frames; 0 processes the entire bag",
    )
    return parser.parse_args()


def validate_args(args: argparse.Namespace) -> None:
    if not args.mcap.is_file():
        raise SystemExit(f"MCAP file not found: {args.mcap}")
    if not math.isfinite(args.floor_z):
        raise SystemExit("floor-z must be finite")
    if not (
        math.isfinite(args.min_height_above_floor)
        and math.isfinite(args.max_height_above_floor)
        and 0.0 <= args.min_height_above_floor < args.max_height_above_floor
    ):
        raise SystemExit("height limits must satisfy 0 <= min < max")
    if not math.isfinite(args.max_range) or args.max_range <= 0.0:
        raise SystemExit("max-range must be finite and greater than zero")
    if not math.isfinite(args.voxel_size_xy) or args.voxel_size_xy <= 0.0:
        raise SystemExit("voxel-size-xy must be finite and greater than zero")
    if not 1 <= args.static_observation_threshold <= 65535:
        raise SystemExit("static-observation-threshold must be between 1 and 65535")
    if not math.isfinite(args.map_update_period) or args.map_update_period < 0.0:
        raise SystemExit("map-update-period must be finite and non-negative")
    if args.max_frames < 0:
        raise SystemExit("max-frames must be non-negative")


def pointcloud_xyz(message: Any) -> np.ndarray:
    """Decode FLOAT32 x/y/z fields, respecting point and row strides."""
    fields = {field.name: field for field in message.fields}
    for name in ("x", "y", "z"):
        field = fields.get(name)
        if field is None or field.datatype != POINT_FIELD_FLOAT32 or field.count != 1:
            raise ValueError(f"PointCloud2 field {name!r} must be one FLOAT32 value")

    endian = ">" if message.is_bigendian else "<"
    dtype = np.dtype(
        {
            "names": ["x", "y", "z"],
            "formats": [f"{endian}f4", f"{endian}f4", f"{endian}f4"],
            "offsets": [fields["x"].offset, fields["y"].offset, fields["z"].offset],
            "itemsize": message.point_step,
        }
    )
    data = memoryview(message.data)
    rows: list[np.ndarray] = []
    for row in range(message.height):
        row_points = np.ndarray(
            shape=(message.width,),
            dtype=dtype,
            buffer=data,
            offset=row * message.row_step,
            strides=(message.point_step,),
        )
        rows.append(
            np.column_stack((row_points["x"], row_points["y"], row_points["z"]))
        )
    if not rows:
        return np.empty((0, 3), dtype=np.float32)
    return np.concatenate(rows, axis=0).astype(np.float32, copy=False)


def estimate_floor(histogram: np.ndarray, fallback_floor_z: float) -> float:
    """Mirror StaticMapProjectorNode::finish_floor_calibration."""
    total = int(histogram.sum())
    if total == 0:
        return fallback_floor_z

    noise_budget = max(1, total // 200)
    cumulative = np.cumsum(histogram)
    candidates = np.flatnonzero(cumulative > noise_budget)
    if candidates.size == 0:
        return fallback_floor_z
    lowest_bin = int(candidates[0])
    span_bins = int(FLOOR_SEARCH_SPAN / FLOOR_BIN_SIZE)
    last_bin = min(len(histogram) - 1, lowest_bin + span_bins)
    local = histogram[lowest_bin : last_bin + 1]
    best_bin = lowest_bin + int(np.argmax(local))
    return FLOOR_RANGE_MIN + (best_bin + 0.5) * FLOOR_BIN_SIZE


def filtered_xy(
    xyz: np.ndarray,
    floor_z: float,
    min_height: float,
    max_height: float,
    max_range: float,
) -> np.ndarray:
    finite = np.isfinite(xyz).all(axis=1)
    range_ok = xyz[:, 0] ** 2 + xyz[:, 1] ** 2 <= max_range**2
    height = xyz[:, 2] - floor_z
    height_ok = (height >= min_height) & (height <= max_height)
    return xyz[finite & range_ok & height_ok, :2]


def draw_sensor(ax: Any) -> None:
    ax.scatter([0.0], [0.0], marker="*", s=80, color="#d62728", zorder=5)
    ax.annotate(
        "LiDAR (0, 0)",
        xy=(0.0, 0.0),
        xytext=(-68, 10),
        textcoords="offset points",
        fontsize=9,
    )
    ax.annotate(
        "",
        xy=(2.0, 0.0),
        xytext=(0.0, 0.0),
        arrowprops={"arrowstyle": "->", "color": "#d62728"},
    )
    ax.annotate(
        "+X forward",
        xy=(1.0, 0.0),
        xytext=(0, -18),
        textcoords="offset points",
        ha="center",
        fontsize=9,
        color="#d62728",
    )


def style_xy_axes(ax: Any, max_range: float, title: str) -> None:
    ax.set_title(title)
    ax.set_xlabel("X — LiDAR forward (m)")
    ax.set_ylabel("Y — LiDAR left (m)")
    ax.set_xlim(-max_range, max_range)
    ax.set_ylim(-max_range, max_range)
    ax.set_aspect("equal", adjustable="box")
    ax.grid(True, linewidth=0.35, alpha=0.35)
    draw_sensor(ax)


def plot_raw(ax: Any, all_xy: np.ndarray, max_range: float) -> None:
    if all_xy.size:
        ax.scatter(
            all_xy[:, 0],
            all_xy[:, 1],
            s=0.35,
            c="#1f77b4",
            alpha=0.10,
            linewidths=0,
            rasterized=True,
        )
    style_xy_axes(ax, max_range, "Raw XY projection after height filtering")


def plot_static(
    ax: Any,
    static_mask: np.ndarray,
    max_range: float,
    voxel_size: float,
) -> None:
    occupied_y, occupied_x = np.nonzero(static_mask)
    if occupied_x.size:
        x = -max_range + (occupied_x + 0.5) * voxel_size
        y = -max_range + (occupied_y + 0.5) * voxel_size
        ax.scatter(
            x,
            y,
            marker="s",
            s=3.0,
            c="#202020",
            linewidths=0,
            rasterized=True,
        )
    style_xy_axes(ax, max_range, "Persistent static occupancy cells")


def save_figures(
    output_dir: Path,
    all_xy: np.ndarray,
    static_mask: np.ndarray,
    max_range: float,
    voxel_size: float,
    floor_z: float,
    processed_frames: int,
) -> dict[str, str]:
    output_dir.mkdir(parents=True, exist_ok=True)
    subtitle = f"floor_z={floor_z:.3f} m, processed frames={processed_frames}"

    raw_path = output_dir / "raw_xy_projection.png"
    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_raw(ax, all_xy, max_range)
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(raw_path, dpi=200)
    plt.close(figure)

    static_path = output_dir / "static_occupancy_map.png"
    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_static(ax, static_mask, max_range, voxel_size)
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(static_path, dpi=200)
    plt.close(figure)

    comparison_path = output_dir / "static_map_comparison.png"
    figure, axes = plt.subplots(1, 2, figsize=(16, 7.5), constrained_layout=True)
    plot_raw(axes[0], all_xy, max_range)
    plot_static(axes[1], static_mask, max_range, voxel_size)
    figure.suptitle(subtitle, fontsize=11)
    figure.savefig(comparison_path, dpi=200)
    plt.close(figure)

    return {
        "raw_xy_projection": str(raw_path.resolve()),
        "static_occupancy_map": str(static_path.resolve()),
        "static_map_comparison": str(comparison_path.resolve()),
    }


def main() -> int:
    args = parse_args()
    validate_args(args)

    grid_dimension = math.ceil((2.0 * args.max_range) / args.voxel_size_xy)
    scores = np.zeros((grid_dimension, grid_dimension), dtype=np.uint16)
    floor_bins = int((FLOOR_RANGE_MAX - FLOOR_RANGE_MIN) / FLOOR_BIN_SIZE)
    floor_histogram = np.zeros(floor_bins, dtype=np.uint64)

    floor_z = args.floor_z
    floor_calibrated = not args.auto_floor
    floor_frames = 0
    messages_decoded = 0
    frames_used = 0
    mapping_frames = 0
    total_points_decoded = 0
    total_points_used = 0
    invalid_points_used = 0
    filtered_point_count = 0
    raw_xy_frames: list[np.ndarray] = []
    last_processed_time = None
    frame_id = ""

    messages: Iterable[Any] = read_ros2_messages(
        args.mcap, topics=[args.topic]
    )
    for record in messages:
        messages_decoded += 1
        xyz = pointcloud_xyz(record.ros_msg)
        total_points_decoded += len(xyz)

        current_time = record.log_time
        if last_processed_time is not None:
            elapsed = (current_time - last_processed_time).total_seconds()
            if 0.0 <= elapsed < args.map_update_period:
                continue
        last_processed_time = current_time

        frames_used += 1
        total_points_used += len(xyz)
        invalid_points_used += int((~np.isfinite(xyz).all(axis=1)).sum())
        frame_id = record.ros_msg.header.frame_id

        if not floor_calibrated:
            finite = np.isfinite(xyz).all(axis=1)
            range_ok = xyz[:, 0] ** 2 + xyz[:, 1] ** 2 <= args.max_range**2
            z_ok = (xyz[:, 2] >= FLOOR_RANGE_MIN) & (xyz[:, 2] < FLOOR_RANGE_MAX)
            z_values = xyz[finite & range_ok & z_ok, 2]
            frame_histogram, _ = np.histogram(
                z_values, bins=floor_bins, range=(FLOOR_RANGE_MIN, FLOOR_RANGE_MAX)
            )
            floor_histogram += frame_histogram.astype(np.uint64)
            floor_frames += 1
            if floor_frames < FLOOR_CALIBRATION_FRAMES:
                if args.max_frames and frames_used >= args.max_frames:
                    break
                continue
            floor_z = estimate_floor(floor_histogram, floor_z)
            floor_calibrated = True

        xy = filtered_xy(
            xyz,
            floor_z,
            args.min_height_above_floor,
            args.max_height_above_floor,
            args.max_range,
        )
        mapping_frames += 1
        filtered_point_count += len(xy)
        if len(xy):
            raw_xy_frames.append(xy)

        observed = np.zeros_like(scores, dtype=bool)
        if len(xy):
            grid_x = np.floor((xy[:, 0] + args.max_range) / args.voxel_size_xy).astype(
                np.int64
            )
            grid_y = np.floor((xy[:, 1] + args.max_range) / args.voxel_size_xy).astype(
                np.int64
            )
            inside = (
                (grid_x >= 0)
                & (grid_x < grid_dimension)
                & (grid_y >= 0)
                & (grid_y < grid_dimension)
            )
            observed[grid_y[inside], grid_x[inside]] = True

        scores[observed] = np.minimum(
            scores[observed].astype(np.uint32) + 1,
            args.static_observation_threshold,
        ).astype(np.uint16)
        scores[(~observed) & (scores > 0)] -= 1

        if args.max_frames and frames_used >= args.max_frames:
            break

    if messages_decoded == 0:
        raise SystemExit(f"No messages found for topic {args.topic!r}")
    if not floor_calibrated:
        raise SystemExit(
            f"Only {floor_frames} processed frames were available; "
            f"automatic floor estimation needs {FLOOR_CALIBRATION_FRAMES}"
        )

    all_xy = (
        np.concatenate(raw_xy_frames, axis=0)
        if raw_xy_frames
        else np.empty((0, 2), dtype=np.float32)
    )
    static_mask = scores >= args.static_observation_threshold
    static_cell_count = int(static_mask.sum())
    images = save_figures(
        args.output_dir,
        all_xy,
        static_mask,
        args.max_range,
        args.voxel_size_xy,
        floor_z,
        frames_used,
    )

    summary = {
        "bag_file": str(args.mcap.resolve()),
        "topic": args.topic,
        "message_type": "sensor_msgs/msg/PointCloud2",
        "frame_id": frame_id,
        "messages_decoded": messages_decoded,
        "frames_used": frames_used,
        "mapping_frames_after_floor_warmup": mapping_frames,
        "total_points_decoded": total_points_decoded,
        "total_points_in_used_frames": total_points_used,
        "invalid_points_in_used_frames": invalid_points_used,
        "estimated_floor_z": floor_z,
        "filtered_points": filtered_point_count,
        "static_cells": static_cell_count,
        "parameters": {
            "auto_floor": args.auto_floor,
            "floor_z_fallback": args.floor_z,
            "min_height_above_floor": args.min_height_above_floor,
            "max_height_above_floor": args.max_height_above_floor,
            "max_range": args.max_range,
            "voxel_size_xy": args.voxel_size_xy,
            "static_observation_threshold": args.static_observation_threshold,
            "map_update_period": args.map_update_period,
        },
        "images": images,
    }
    summary_path = args.output_dir / "offline_static_map_summary.json"
    summary_path.write_text(json.dumps(summary, indent=2), encoding="utf-8")
    summary["summary_json"] = str(summary_path.resolve())
    print(json.dumps(summary, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
