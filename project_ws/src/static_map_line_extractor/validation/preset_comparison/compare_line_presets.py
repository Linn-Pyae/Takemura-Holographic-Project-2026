#!/usr/bin/env python3
"""Compare line-extraction parameter presets on identical real-MCAP grids."""

from __future__ import annotations

import argparse
import csv
import importlib.util
import json
import math
from pathlib import Path
import sys
from types import ModuleType
from typing import Any

try:
    import cv2
    import matplotlib

    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    import numpy as np
    from matplotlib.collections import LineCollection
    from mcap_ros2.reader import read_ros2_messages
except ImportError as error:  # pragma: no cover
    raise SystemExit(
        "Comparison dependencies are missing. Install them with:\n"
        "  python -m pip install -r preset_comparison/requirements.txt"
    ) from error


PROJECTOR_DEFAULTS = {
    "auto_floor": True,
    "floor_z_fallback": -0.85,
    "min_height_above_floor": 0.30,
    "max_height_above_floor": 2.00,
    "max_range": 15.0,
    "voxel_size_xy": 0.10,
    "static_observation_threshold": 5,
    "map_update_period": 0.20,
}

COMMON_LINE_DEFAULTS = {
    "occupied_threshold": 100,
    "min_contour_area": 0.0,
    "maximum_line_gap": 0.20,
    "line_width": 0.04,
    "update_period": 0.20,
}

PRESETS = {
    "Current": {
        "minimum_line_length": 0.30,
        "minimum_component_cells": 3,
        "contour_epsilon": 0.10,
    },
    "Mild": {
        "minimum_line_length": 0.50,
        "minimum_component_cells": 5,
        "contour_epsilon": 0.15,
    },
    "Clean": {
        "minimum_line_length": 0.50,
        "minimum_component_cells": 8,
        "contour_epsilon": 0.20,
    },
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
        description="Compare Current, Mild, and Clean on real LiDAR MCAP files."
    )
    parser.add_argument("mcap", nargs="+", type=Path)
    parser.add_argument("--topic", default="/velodyne_points_wifi")
    parser.add_argument(
        "--output-root",
        type=Path,
        default=Path(__file__).resolve().parent / "output",
    )
    return parser.parse_args()


def build_static_grid(
    mcap_path: Path, topic: str, projector: ModuleType
) -> tuple[np.ndarray, dict[str, Any]]:
    maximum_range = PROJECTOR_DEFAULTS["max_range"]
    resolution = PROJECTOR_DEFAULTS["voxel_size_xy"]
    dimension = math.ceil((2.0 * maximum_range) / resolution)
    scores = np.zeros((dimension, dimension), dtype=np.uint16)
    histogram_bins = int(
        (projector.FLOOR_RANGE_MAX - projector.FLOOR_RANGE_MIN)
        / projector.FLOOR_BIN_SIZE
    )
    floor_histogram = np.zeros(histogram_bins, dtype=np.uint64)

    floor_z = PROJECTOR_DEFAULTS["floor_z_fallback"]
    floor_calibrated = False
    floor_frames = 0
    messages = 0
    projector_frames = 0
    mapping_frames = 0
    total_points = 0
    filtered_points = 0
    last_time = None

    for record in read_ros2_messages(mcap_path, topics=[topic]):
        messages += 1
        xyz = projector.pointcloud_xyz(record.ros_msg)
        total_points += len(xyz)
        current_time = record.log_time
        if last_time is not None:
            elapsed = (current_time - last_time).total_seconds()
            if 0.0 <= elapsed < PROJECTOR_DEFAULTS["map_update_period"]:
                continue
        last_time = current_time
        projector_frames += 1

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
            PROJECTOR_DEFAULTS["min_height_above_floor"],
            PROJECTOR_DEFAULTS["max_height_above_floor"],
            maximum_range,
        )
        mapping_frames += 1
        filtered_points += len(xy)

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
            PROJECTOR_DEFAULTS["static_observation_threshold"],
        ).astype(np.uint16)
        scores[(~observed) & (scores > 0)] -= 1

    if messages == 0:
        raise RuntimeError(f"No PointCloud2 messages found on {topic}")
    if not floor_calibrated:
        raise RuntimeError("Not enough frames for automatic floor calibration")

    static_mask = scores >= PROJECTOR_DEFAULTS["static_observation_threshold"]
    metadata = {
        "bag_file": str(mcap_path.resolve()),
        "messages": messages,
        "projector_frames": projector_frames,
        "mapping_frames": mapping_frames,
        "total_points": total_points,
        "filtered_points": filtered_points,
        "estimated_floor_z": floor_z,
        "grid_resolution": resolution,
        "static_cells": int(static_mask.sum()),
    }
    return static_mask, metadata


def removed_small_component_count(
    occupancy: np.ndarray, minimum_cells: int
) -> int:
    binary = np.where(
        occupancy >= COMMON_LINE_DEFAULTS["occupied_threshold"], 255, 0
    ).astype(np.uint8)
    resolution = PROJECTOR_DEFAULTS["voxel_size_xy"]
    gap = COMMON_LINE_DEFAULTS["maximum_line_gap"]
    if gap > 0.0:
        radius = max(1, math.ceil(gap / (2.0 * resolution)))
        size = 2 * radius + 1
        kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (size, size))
        binary = cv2.morphologyEx(binary, cv2.MORPH_CLOSE, kernel)

    component_count, _, statistics, _ = cv2.connectedComponentsWithStats(
        binary, connectivity=8, ltype=cv2.CV_32S
    )
    return sum(
        1
        for label in range(1, component_count)
        if statistics[label, cv2.CC_STAT_AREA] < minimum_cells
    )


def extract_preset(
    static_mask: np.ndarray,
    preset_name: str,
    preset: dict[str, Any],
    line_module: ModuleType,
) -> tuple[list[Any], list[Any], dict[str, Any]]:
    occupancy = np.where(static_mask, 100, 0).astype(np.int8)
    parameters = line_module.Parameters(
        occupied_threshold=COMMON_LINE_DEFAULTS["occupied_threshold"],
        min_contour_area=COMMON_LINE_DEFAULTS["min_contour_area"],
        contour_epsilon=preset["contour_epsilon"],
        minimum_line_length=preset["minimum_line_length"],
        maximum_line_gap=COMMON_LINE_DEFAULTS["maximum_line_gap"],
        minimum_component_cells=preset["minimum_component_cells"],
    )
    geometry = line_module.Geometry(
        resolution=PROJECTOR_DEFAULTS["voxel_size_xy"],
        origin_x=-PROJECTOR_DEFAULTS["max_range"],
        origin_y=-PROJECTOR_DEFAULTS["max_range"],
        origin_yaw=0.0,
    )
    contours, lines = line_module.extract_lines(occupancy, geometry, parameters)
    lengths = [math.dist(line[0], line[1]) for line in lines]
    metrics = {
        "preset": preset_name,
        **COMMON_LINE_DEFAULTS,
        **preset,
        "contours": len(contours),
        "line_segments": len(lines),
        "removed_small_components": removed_small_component_count(
            occupancy, preset["minimum_component_cells"]
        ),
        "mean_line_length": float(np.mean(lengths)) if lengths else 0.0,
        "max_line_length": max(lengths, default=0.0),
    }
    return contours, lines, metrics


def draw_sensor(ax: Any) -> None:
    ax.scatter([0.0], [0.0], marker="*", s=65, color="#d62728", zorder=8)
    ax.annotate(
        "LiDAR (0, 0)",
        xy=(0.0, 0.0),
        xytext=(-65, 9),
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
        xytext=(0, -17),
        textcoords="offset points",
        ha="center",
        fontsize=8,
        color="#d62728",
    )


def style_axes(ax: Any, title: str) -> None:
    maximum = PROJECTOR_DEFAULTS["max_range"]
    ax.set_title(title)
    ax.set_xlabel("X — LiDAR forward (m)")
    ax.set_ylabel("Y — LiDAR left (m)")
    ax.set_xlim(-maximum, maximum)
    ax.set_ylim(-maximum, maximum)
    ax.set_aspect("equal", adjustable="box")
    ax.grid(True, linewidth=0.35, alpha=0.30)
    draw_sensor(ax)


def plot_grid(ax: Any, static_mask: np.ndarray, title: str) -> None:
    rows, columns = np.nonzero(static_mask)
    resolution = PROJECTOR_DEFAULTS["voxel_size_xy"]
    origin = -PROJECTOR_DEFAULTS["max_range"]
    x = origin + (columns + 0.5) * resolution
    y = origin + (rows + 0.5) * resolution
    if len(x):
        ax.scatter(
            x,
            y,
            marker="s",
            s=3.2,
            c="#303030",
            linewidths=0,
            rasterized=True,
        )
    style_axes(ax, title)


def add_lines(ax: Any, lines: list[Any], color: str) -> None:
    if lines:
        ax.add_collection(
            LineCollection(
                [[line[0], line[1]] for line in lines],
                colors=color,
                linewidths=1.5,
                zorder=6,
            )
        )


def plot_lines(
    ax: Any,
    lines: list[Any],
    title: str,
    *,
    color: str = "#007c91",
) -> None:
    add_lines(ax, lines, color)
    style_axes(ax, title)


def save_preset_images(
    output_dir: Path,
    static_mask: np.ndarray,
    lines: list[Any],
    preset_name: str,
    metrics: dict[str, Any],
) -> dict[str, str]:
    output_dir.mkdir(parents=True, exist_ok=True)
    paths = {
        "static_occupancy_grid": output_dir / "static_occupancy_grid.png",
        "extracted_lines": output_dir / "extracted_lines.png",
        "grid_lines_overlay": output_dir / "grid_lines_overlay.png",
    }
    subtitle = (
        f"{preset_name}: contours={metrics['contours']}, "
        f"lines={metrics['line_segments']}, "
        f"removed={metrics['removed_small_components']}"
    )

    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_grid(ax, static_mask, "Static occupancy grid")
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(paths["static_occupancy_grid"], dpi=200)
    plt.close(figure)

    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_lines(ax, lines, "Extracted lines")
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(paths["extracted_lines"], dpi=200)
    plt.close(figure)

    figure, ax = plt.subplots(figsize=(9, 8), constrained_layout=True)
    plot_grid(ax, static_mask, "Static grid + extracted lines")
    add_lines(ax, lines, "#d62728")
    figure.suptitle(subtitle, fontsize=10)
    figure.savefig(paths["grid_lines_overlay"], dpi=200)
    plt.close(figure)
    return {name: str(path.resolve()) for name, path in paths.items()}


def save_comparison(
    path: Path,
    static_mask: np.ndarray,
    results: list[tuple[str, list[Any], dict[str, Any]]],
    bag_name: str,
) -> None:
    colors = {"Current": "#007c91", "Mild": "#e17c05", "Clean": "#2a7f3e"}
    figure, axes = plt.subplots(1, 3, figsize=(22, 7.2), constrained_layout=True)
    for ax, (name, lines, metrics) in zip(axes, results):
        plot_grid(ax, static_mask, name)
        add_lines(ax, lines, colors[name])
        ax.set_title(
            f"{name}\ncontours={metrics['contours']}  "
            f"lines={metrics['line_segments']}  "
            f"removed={metrics['removed_small_components']}\n"
            f"mean={metrics['mean_line_length']:.2f} m  "
            f"max={metrics['max_line_length']:.2f} m"
        )
    figure.suptitle(f"{bag_name}: Current / Mild / Clean", fontsize=11)
    path.parent.mkdir(parents=True, exist_ok=True)
    figure.savefig(path, dpi=180)
    plt.close(figure)


def write_metrics(output_root: Path, rows: list[dict[str, Any]]) -> None:
    json_path = output_root / "preset_metrics.json"
    json_path.write_text(json.dumps(rows, indent=2), encoding="utf-8")
    csv_path = output_root / "preset_metrics.csv"
    if rows:
        with csv_path.open("w", newline="", encoding="utf-8") as file:
            writer = csv.DictWriter(file, fieldnames=list(rows[0].keys()))
            writer.writeheader()
            writer.writerows(rows)


def main() -> int:
    args = parse_args()
    comparison_dir = Path(__file__).resolve().parent
    validation_dir = comparison_dir.parent
    line_package_dir = validation_dir.parent
    source_dir = line_package_dir.parent
    projector = load_module(
        "preset_projector_offline",
        source_dir / "static_map_projector" / "offline" / "offline_static_map.py",
    )
    line_module = load_module(
        "preset_line_extractor_offline",
        line_package_dir / "offline" / "test_line_extractor_offline.py",
    )

    all_rows: list[dict[str, Any]] = []
    for mcap_path in args.mcap:
        static_mask, bag_metadata = build_static_grid(mcap_path, args.topic, projector)
        bag_dir = args.output_root / mcap_path.stem
        comparison_results = []
        for preset_name, preset in PRESETS.items():
            _, lines, metrics = extract_preset(
                static_mask, preset_name, preset, line_module
            )
            images = save_preset_images(
                bag_dir / preset_name.lower(),
                static_mask,
                lines,
                preset_name,
                metrics,
            )
            row = {
                "bag": mcap_path.name,
                "static_cells": bag_metadata["static_cells"],
                **metrics,
            }
            all_rows.append(row)
            comparison_results.append((preset_name, lines, metrics))
            (bag_dir / preset_name.lower() / "metrics.json").write_text(
                json.dumps({**bag_metadata, **row, "images": images}, indent=2),
                encoding="utf-8",
            )

        save_comparison(
            bag_dir / "preset_comparison.png",
            static_mask,
            comparison_results,
            mcap_path.name,
        )

    args.output_root.mkdir(parents=True, exist_ok=True)
    write_metrics(args.output_root, all_rows)
    print(json.dumps(all_rows, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
