"""評価シナリオのGround Truth、Detection、Track、ID switchを可視化する。"""

from __future__ import annotations

import argparse
from collections import defaultdict
import os
from pathlib import Path
import tempfile
from typing import Sequence

from .evaluation import (
    GREEDY_METHOD,
    HUNGARIAN_KALMAN_METHOD,
    AssignmentRecord,
    evaluate_scenario,
)
from .simulator import SimulationConfig, create_standard_scenarios


def _draw_records(ax, records: Sequence[AssignmentRecord], title: str) -> None:
    by_person: dict[str, list[AssignmentRecord]] = defaultdict(list)
    by_track: dict[int, list[AssignmentRecord]] = defaultdict(list)
    for record in records:
        by_person[record.ground_truth_person_id].append(record)
        if record.assigned_track_id is not None:
            by_track[record.assigned_track_id].append(record)

    for person_id, person_records in sorted(by_person.items()):
        ax.plot(
            [record.true_x for record in person_records],
            [record.true_y for record in person_records],
            linestyle="--",
            linewidth=1.5,
            label=f"GT Person {person_id}",
        )
    detection_records = [record for record in records if record.detection_x is not None]
    ax.scatter(
        [record.detection_x for record in detection_records],
        [record.detection_y for record in detection_records],
        s=16,
        marker=".",
        color="gray",
        alpha=0.45,
        label="Detection",
    )
    for track_id, track_records in sorted(by_track.items()):
        visible = [record for record in track_records if record.track_x is not None]
        if not visible:
            continue
        name = visible[-1].assigned_track_name or "-"
        ax.plot(
            [record.track_x for record in visible],
            [record.track_y for record in visible],
            marker="o",
            markersize=2.5,
            linewidth=1.2,
            label=f"Track {track_id}: {name}",
        )
        ax.annotate(
            f"{name} (ID {track_id})",
            (visible[-1].track_x, visible[-1].track_y),
            xytext=(5, 5),
            textcoords="offset points",
            fontsize=8,
        )
    switches = [record for record in records if record.id_switch]
    if switches:
        ax.scatter(
            [record.true_x for record in switches],
            [record.true_y for record in switches],
            color="red",
            marker="X",
            s=90,
            label="ID Switch",
            zorder=10,
        )
        for record in switches:
            ax.annotate(
                f"F{record.frame_number}",
                (record.true_x, record.true_y),
                color="red",
                fontsize=8,
            )
    ax.set(title=title, xlabel="X [m]", ylabel="Y [m]")
    ax.axis("equal")
    ax.grid(True, alpha=0.3)
    ax.legend(fontsize=7, loc="best")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Visualize MOT benchmark scenario")
    parser.add_argument("--scenario", default="combined_noise_crossing_dropout")
    parser.add_argument(
        "--method",
        choices=(GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD),
        default=HUNGARIAN_KALMAN_METHOD,
    )
    parser.add_argument("--save", help="静止画PNGまたはアニメーションGIFの保存先")
    parser.add_argument("--animate", action="store_true")
    parser.add_argument("--interval", type=int, default=300, help="animation間隔ms")
    parser.add_argument("--seed", type=int, default=20260811)
    parser.add_argument("--noise-amplitude", type=float, default=0.2)
    parser.add_argument("--max-association-distance", type=float, default=1.5)
    parser.add_argument("--max-missed-frames", type=int, default=3)
    parser.add_argument("--kalman-process-noise", type=float, default=0.1)
    parser.add_argument("--kalman-measurement-noise", type=float, default=0.1)
    parser.add_argument("--initial-position-covariance", type=float, default=1.0)
    parser.add_argument("--initial-velocity-covariance", type=float, default=100.0)
    return parser


def main(argv: Sequence[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    try:
        # 同梱PythonにTcl/Tkがない環境でもPNG/GIF保存できるようにする。
        cache_dir = Path(tempfile.gettempdir()) / "tracking-matplotlib-cache"
        cache_dir.mkdir(parents=True, exist_ok=True)
        os.environ.setdefault("MPLCONFIGDIR", str(cache_dir))
        if args.save:
            import matplotlib
            matplotlib.use("Agg")
        import matplotlib.pyplot as plt
        from matplotlib.animation import FuncAnimation
    except ModuleNotFoundError as error:
        raise SystemExit(
            "Matplotlib is required. Install: pip install -r tracking/requirements.txt"
        ) from error

    config = SimulationConfig(
        random_seed=args.seed,
        noise_amplitude=args.noise_amplitude,
        max_association_distance=args.max_association_distance,
        max_missed_frames=args.max_missed_frames,
        kalman_process_noise=args.kalman_process_noise,
        kalman_measurement_noise=args.kalman_measurement_noise,
        initial_position_covariance=args.initial_position_covariance,
        initial_velocity_covariance=args.initial_velocity_covariance,
    )
    scenarios = create_standard_scenarios(config)
    if args.scenario not in scenarios:
        raise SystemExit(
            f"Unknown scenario: {args.scenario}\nAvailable: {', '.join(scenarios)}"
        )
    scenario = scenarios[args.scenario]
    result = evaluate_scenario(scenario, args.method, config)
    title = (
        f"{scenario.name} / {args.method} "
        f"(IDSW={result.id_switch_count}, success={result.tracking_success_rate:.1f}%)"
    )
    fig, ax = plt.subplots(figsize=(10, 7))

    if args.animate:
        def update(frame_number: int):
            ax.clear()
            visible = [
                record for record in result.records
                if record.frame_number <= frame_number
            ]
            _draw_records(ax, visible, f"{title} / Frame {frame_number}")
            return ax.lines + ax.collections

        animation = FuncAnimation(
            fig,
            update,
            frames=range(result.total_frames),
            interval=args.interval,
            repeat=False,
        )
        if args.save:
            output = Path(args.save)
            if output.suffix.lower() != ".gif":
                raise SystemExit("Animation save path must end with .gif")
            animation.save(output, writer="pillow")
            print(f"saved animation: {output.resolve()}")
        else:
            plt.show()
    else:
        _draw_records(ax, result.records, title)
        fig.tight_layout()
        if args.save:
            output = Path(args.save)
            fig.savefig(output, dpi=160)
            print(f"saved image: {output.resolve()}")
        else:
            plt.show()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
