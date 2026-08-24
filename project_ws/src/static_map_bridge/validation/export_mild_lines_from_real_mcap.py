#!/usr/bin/env python3
"""Export real-MCAP Mild-preset line segments for protocol round-trip tests.

This is an offline validation helper only. It imports the already-added offline
projector and line-extractor equivalents without changing either ROS package.
"""

from __future__ import annotations

import argparse
import csv
import json
from pathlib import Path


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mcap", nargs="+", type=Path)
    parser.add_argument("--topic", default="/velodyne_points_wifi")
    parser.add_argument(
        "--output-root",
        type=Path,
        default=Path(__file__).resolve().parent / "output",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    bridge_dir = Path(__file__).resolve().parents[1]
    source_dir = bridge_dir.parent
    comparison_path = (
        source_dir
        / "static_map_line_extractor"
        / "validation"
        / "preset_comparison"
        / "compare_line_presets.py"
    )

    # load_module is part of the comparison helper, so use importlib once to
    # load that helper before reusing its tested pipeline functions.
    import importlib.util
    import sys

    spec = importlib.util.spec_from_file_location("bridge_preset_comparison", comparison_path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot load {comparison_path}")
    comparison = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = comparison
    spec.loader.exec_module(comparison)

    projector = comparison.load_module(
        "bridge_projector_offline",
        source_dir / "static_map_projector" / "offline" / "offline_static_map.py",
    )
    line_module = comparison.load_module(
        "bridge_line_extractor_offline",
        source_dir
        / "static_map_line_extractor"
        / "offline"
        / "test_line_extractor_offline.py",
    )

    args.output_root.mkdir(parents=True, exist_ok=True)
    summaries = []
    for mcap_path in args.mcap:
        static_mask, bag_metadata = comparison.build_static_grid(
            mcap_path, args.topic, projector
        )
        _, lines, metrics = comparison.extract_preset(
            static_mask, "Mild", comparison.PRESETS["Mild"], line_module
        )
        csv_path = args.output_root / f"{mcap_path.stem}_mild_lines.csv"
        with csv_path.open("w", newline="", encoding="utf-8") as output:
            writer = csv.writer(output)
            writer.writerow(["x1", "y1", "x2", "y2"])
            for first, second in lines:
                writer.writerow([first[0], first[1], second[0], second[1]])

        summary = {
            **bag_metadata,
            **metrics,
            "topic": args.topic,
            "line_csv": str(csv_path.resolve()),
        }
        summaries.append(summary)
        print(
            f"{mcap_path.name}: Mild segments={len(lines)} -> {csv_path.resolve()}"
        )

    summary_path = args.output_root / "real_mcap_export_summary.json"
    summary_path.write_text(json.dumps(summaries, indent=2), encoding="utf-8")
    print(f"summary -> {summary_path.resolve()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
