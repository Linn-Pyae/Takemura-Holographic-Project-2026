#!/usr/bin/env python3
"""Validate the current projector -> line extractor pipeline on real MCAP data.

This is an offline adapter for environments without ROS 2. It imports the
existing offline reference functions without modifying either ROS package and
uses their current default parameters.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
import math
from pathlib import Path
import sys
from types import ModuleType
from typing import Any

try:
    import matplotlib

    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    import numpy as np
    from matplotlib.collections import LineCollection
    from mcap_ros2.reader import read_ros2_messages
except ImportError as error:  # pragma: no cover
    raise SystemExit(
        "Validation dependencies are missing. Install them with:\n"
        "  python -m pip install -r validation/requirements.txt"
    ) from error


PROJECTOR_PARAMETERS = {
    "auto_floor": True,
    "floor_z_fallback": -0.85,
    "min_height_above_floor": 0.30,
    "max_height_above_floor": 2.00,
    "max_range": 15.0,
    "voxel_size_xy": 0.10,
    "static_observation_threshold": 5,
    "map_update_period": 0.20,
}

LINE_PARAMETERS = {
    "occupied_threshold": 100,
    "min_contour_area": 0.0,
    "contour_epsilon": 0.10,
    "minimum_line_length": 0.30,
    "maximum_line_gap": 0.20,
    "minimum_component_cells": 3,
    "line_width": 0.04,
    "update_period": 0.20,
}


def load_module(name: str, path: Path) -> ModuleType:
    specification = importlib.util.spec_from_file_location(name, path)
    if specification is None or specification.loader is None:
        raise RuntimeError(f"Cannot load module from {path}")
    module = importlib.util.module_from_spec(specification)
    sys.modules[name] = module
    specification.loader.exec_module(module)
    return module


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Run the default static-map pipeline directly on real MCAP files."
    )
    parser.add_argument("mcap", type=Path, nargs="+", help="Input MCAP file(s)")
    parser.add_argument("--topic", default="/velodyne_points_wifi")
    parser.add_argument(
        "--output-root",
        type=Path,
        default=Path(__file__).resolve().parent / "output",
    )
    return parser.parse_args()


def draw_sensor(ax: Any) -> None:
    ax.scatter([0.0], [0.0], marker="*", s=70, color="#d62728", zorder=8)
    ax.annotate(
        "LiDAR (0, 0)",
        xy=(0.0, 0.0),
        xytext=(-68, 10),
        textcoords="offset points",
        fontsize=8,
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
        fontsize=8,
        color="#d62728",
    )


def style_axes(ax: Any, title: str) -> None:
    maximum = PROJECTOR_PARAMETERS["max_range"]
    ax.set_title(title)
    ax.set_xlabel("X — LiDAR forward (m)")
    ax.set_ylabel("Y — LiDAR left (m)")
    ax.set_xlim(-maximum, maximum)
    ax.set_ylim(-maximum, maximum)
    ax.set_aspect("equal", adjustable="box")
    ax.grid(True, linewidth=0.35, alpha=0.30)
    draw_sensor(ax)


def plot_raw(ax: Any, xy: np.ndarray) -> None:
    if xy.size:
        ax.scatter(
            xy[:, 0],
            xy[:, 1],
            s=0.35,
            c="#1f77b4",
            alpha=0.10,
            linewidths=0,
            rasterized=True,
        )
    style_axes(ax, "Raw XY projection")


def static_cell_coordinates(static_mask: np.ndarray) -> tuple[np.ndarray, np.ndarray]:
    rows, columns = np.nonzero(static_mask)
    origin = -PROJECTOR_PARAMETERS["max_range"]
    resolution = PROJECTOR_PARAMETERS["voxel_size_xy"]
    return (
        origin + (columns + 0.5) * resolution,
        origin + (rows + 0.5) * resolution,
    )


def plot_grid(ax: Any, static_mask: np.ndarray) -> None:
    x, y = static_cell_coordinates(static_mask)
    if x.size:
        ax.scatter(
            x,
            y,
            marker="s",
            s=3.2,
            c="#303030",
            linewidths=0,
            rasterized=True,
        )
    style_axes(ax, "Static occupancy grid")


def plot_lines(ax: Any, lines: list[Any], *, overlay: bool = False) -> None:
    if lines:
        collection = LineCollection(
            [[line[0], line[1]] for line in lines],
            colors="#d62728" if overlay else "#007c91",
            linewidths=1.3 if overlay else 1.6,
            zorder=6,
        )
        ax.add_collection(collection)
    style_axes(ax, "Static grid + extracted lines" if overlay else "Extracted lines")


def save_images(
    output_dir: Path,
    raw_xy: np.ndarray,
    static_mask: np.ndarray,
    lines: list[Any],
    floor_z: float,
) -> dict[str, str]:
    output_dir.mkdir(parents=True, exist_ok=True)
    subtitle = f"default parameters, floor_z={floor_z:.3f} m"

    paths = {
        "raw_xy_projection": output_dir / "raw_xy_projection.png",
        "static_occupancy_grid": output_dir / "static_occupancy_grid.png",
        "static_map_lines": output_dir / "static_map_lines.png",
        "static_map_comparison": output_dir / "static_map_comparison.png",
        "static_grid_with_lines": output_dir / "static_grid_with_lines.png",
    }

    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_raw(ax, raw_xy)
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(paths["raw_xy_projection"], dpi=200)
    plt.close(figure)

    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_grid(ax, static_mask)
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(paths["static_occupancy_grid"], dpi=200)
    plt.close(figure)

    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_lines(ax, lines)
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(paths["static_map_lines"], dpi=200)
    plt.close(figure)

    figure, axes = plt.subplots(1, 3, figsize=(22, 7.2), constrained_layout=True)
    plot_raw(axes[0], raw_xy)
    plot_grid(axes[1], static_mask)
    plot_lines(axes[2], lines)
    figure.suptitle(subtitle, fontsize=11)
    figure.savefig(paths["static_map_comparison"], dpi=180)
    plt.close(figure)

    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_grid(ax, static_mask)
    plot_lines(ax, lines, overlay=True)
    ax.set_title("Static occupancy grid with extracted lines")
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(paths["static_grid_with_lines"], dpi=200)
    plt.close(figure)

    return {name: str(path.resolve()) for name, path in paths.items()}


def validate_one(
    mcap_path: Path,
    topic: str,
    output_dir: Path,
    projector: ModuleType,
    line_extractor: ModuleType,
) -> dict[str, Any]:
    if not mcap_path.is_file():
        raise FileNotFoundError(mcap_path)

    maximum_range = PROJECTOR_PARAMETERS["max_range"]
    resolution = PROJECTOR_PARAMETERS["voxel_size_xy"]
    dimension = math.ceil((2.0 * maximum_range) / resolution)
    scores = np.zeros((dimension, dimension), dtype=np.uint16)
    histogram_bins = int(
        (projector.FLOOR_RANGE_MAX - projector.FLOOR_RANGE_MIN)
        / projector.FLOOR_BIN_SIZE
    )
    floor_histogram = np.zeros(histogram_bins, dtype=np.uint64)

    floor_z = PROJECTOR_PARAMETERS["floor_z_fallback"]
    floor_frames = 0
    floor_calibrated = False
    messages_decoded = 0
    projector_frames = 0
    mapping_frames = 0
    line_update_frames = 0
    total_points = 0
    selected_points = 0
    invalid_points = 0
    filtered_points = 0
    raw_frames: list[np.ndarray] = []
    last_projector_time = None
    last_line_time = None
    frame_id = ""
    latest_contours: list[Any] = []
    latest_lines: list[Any] = []

    line_parameters = line_extractor.Parameters(
        occupied_threshold=LINE_PARAMETERS["occupied_threshold"],
        min_contour_area=LINE_PARAMETERS["min_contour_area"],
        contour_epsilon=LINE_PARAMETERS["contour_epsilon"],
        minimum_line_length=LINE_PARAMETERS["minimum_line_length"],
        maximum_line_gap=LINE_PARAMETERS["maximum_line_gap"],
        minimum_component_cells=LINE_PARAMETERS["minimum_component_cells"],
    )
    geometry = line_extractor.Geometry(
        resolution=resolution,
        origin_x=-maximum_range,
        origin_y=-maximum_range,
        origin_yaw=0.0,
    )

    for record in read_ros2_messages(mcap_path, topics=[topic]):
        messages_decoded += 1
        xyz = projector.pointcloud_xyz(record.ros_msg)
        total_points += len(xyz)

        current_time = record.log_time
        if last_projector_time is not None:
            elapsed = (current_time - last_projector_time).total_seconds()
            if 0.0 <= elapsed < PROJECTOR_PARAMETERS["map_update_period"]:
                continue
        last_projector_time = current_time

        projector_frames += 1
        selected_points += len(xyz)
        invalid_points += int((~np.isfinite(xyz).all(axis=1)).sum())
        frame_id = record.ros_msg.header.frame_id

        if not floor_calibrated:
            finite = np.isfinite(xyz).all(axis=1)
            range_ok = xyz[:, 0] ** 2 + xyz[:, 1] ** 2 <= maximum_range**2
            z_ok = (xyz[:, 2] >= projector.FLOOR_RANGE_MIN) & (
                xyz[:, 2] < projector.FLOOR_RANGE_MAX
            )
            z_values = xyz[finite & range_ok & z_ok, 2]
            frame_histogram, _ = np.histogram(
                z_values,
                bins=histogram_bins,
                range=(projector.FLOOR_RANGE_MIN, projector.FLOOR_RANGE_MAX),
            )
            floor_histogram += frame_histogram.astype(np.uint64)
            floor_frames += 1
            if floor_frames < projector.FLOOR_CALIBRATION_FRAMES:
                continue
            floor_z = projector.estimate_floor(floor_histogram, floor_z)
            floor_calibrated = True

        xy = projector.filtered_xy(
            xyz,
            floor_z,
            PROJECTOR_PARAMETERS["min_height_above_floor"],
            PROJECTOR_PARAMETERS["max_height_above_floor"],
            maximum_range,
        )
        mapping_frames += 1
        filtered_points += len(xy)
        if len(xy):
            raw_frames.append(xy)

        observed = np.zeros_like(scores, dtype=bool)
        if len(xy):
            grid_x = np.floor((xy[:, 0] + maximum_range) / resolution).astype(
                np.int64
            )
            grid_y = np.floor((xy[:, 1] + maximum_range) / resolution).astype(
                np.int64
            )
            inside = (
                (grid_x >= 0)
                & (grid_x < dimension)
                & (grid_y >= 0)
                & (grid_y < dimension)
            )
            observed[grid_y[inside], grid_x[inside]] = True

        scores[observed] = np.minimum(
            scores[observed].astype(np.uint32) + 1,
            PROJECTOR_PARAMETERS["static_observation_threshold"],
        ).astype(np.uint16)
        scores[(~observed) & (scores > 0)] -= 1

        if last_line_time is not None:
            line_elapsed = (current_time - last_line_time).total_seconds()
            if 0.0 <= line_elapsed < LINE_PARAMETERS["update_period"]:
                continue
        last_line_time = current_time
        line_update_frames += 1
        occupancy = np.where(
            scores >= PROJECTOR_PARAMETERS["static_observation_threshold"], 100, 0
        ).astype(np.int8)
        latest_contours, latest_lines = line_extractor.extract_lines(
            occupancy, geometry, line_parameters
        )

    if messages_decoded == 0:
        raise RuntimeError(f"No PointCloud2 messages found on {topic}")
    if not floor_calibrated:
        raise RuntimeError("Not enough frames for automatic floor calibration")

    static_mask = scores >= PROJECTOR_PARAMETERS["static_observation_threshold"]
    raw_xy = (
        np.concatenate(raw_frames, axis=0)
        if raw_frames
        else np.empty((0, 2), dtype=np.float32)
    )
    images = save_images(output_dir, raw_xy, static_mask, latest_lines, floor_z)

    summary = {
        "bag_file": str(mcap_path.resolve()),
        "topic": topic,
        "message_type": "sensor_msgs/msg/PointCloud2",
        "frame_id": frame_id,
        "messages_decoded": messages_decoded,
        "projector_frames_used": projector_frames,
        "mapping_frames_after_floor_warmup": mapping_frames,
        "line_extractor_updates": line_update_frames,
        "total_points": total_points,
        "points_in_projector_frames": selected_points,
        "invalid_points_in_projector_frames": invalid_points,
        "estimated_floor_z": floor_z,
        "filtered_points": filtered_points,
        "grid_resolution": resolution,
        "static_cells": int(static_mask.sum()),
        "contours": len(latest_contours),
        "line_segments": len(latest_lines),
        "projector_parameters": PROJECTOR_PARAMETERS,
        "line_extractor_parameters": LINE_PARAMETERS,
        "images": images,
    }
    summary_path = output_dir / "validation_summary.json"
    summary_path.write_text(json.dumps(summary, indent=2), encoding="utf-8")
    summary["summary_json"] = str(summary_path.resolve())
    return summary


def main() -> int:
    args = parse_args()
    package_dir = Path(__file__).resolve().parents[1]
    source_dir = package_dir.parent
    projector = load_module(
        "static_map_projector_offline",
        source_dir / "static_map_projector" / "offline" / "offline_static_map.py",
    )
    line_extractor = load_module(
        "static_map_line_extractor_offline",
        package_dir / "offline" / "test_line_extractor_offline.py",
    )

    summaries = []
    for mcap_path in args.mcap:
        output_dir = args.output_root / mcap_path.stem
        summary = validate_one(
            mcap_path, args.topic, output_dir, projector, line_extractor
        )
        summaries.append(summary)
        print(json.dumps(summary, indent=2))

    combined_path = args.output_root / "all_bags_summary.json"
    combined_path.parent.mkdir(parents=True, exist_ok=True)
    combined_path.write_text(json.dumps(summaries, indent=2), encoding="utf-8")
    print(f"Combined summary: {combined_path.resolve()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
