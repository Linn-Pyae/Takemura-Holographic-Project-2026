"""MinDist×Noiseを複数seedで反復する統計評価。"""

from __future__ import annotations

import argparse
import csv
from dataclasses import asdict, dataclass, replace
import os
from pathlib import Path
from statistics import mean, pstdev
import tempfile
from typing import Sequence

from .evaluation import (
    GREEDY_METHOD,
    HUNGARIAN_KALMAN_METHOD,
    evaluate_scenario,
)
from .parameter_sweep import (
    DEFAULT_MINIMUM_DISTANCES,
    DEFAULT_NOISE_AMPLITUDES,
    create_proximity_scenario,
)
from .simulator import SimulationConfig


@dataclass(frozen=True)
class SeedTrialResult:
    minimum_distance_m: float
    noise_amplitude_m: float
    method: str
    random_seed: int
    id_switch_count: int
    success_rate_percent: float
    false_reassignment_count: int
    failed: bool


@dataclass(frozen=True)
class StatisticalSweepResult:
    minimum_distance_m: float
    noise_amplitude_m: float
    method: str
    seed_start: int
    seed_count: int
    failure_trial_count: int
    failure_rate: float
    failure_rate_percent: float
    mean_id_switch_count: float
    max_id_switch_count: int
    mean_success_rate_percent: float
    success_rate_stddev_percent: float
    mean_false_reassignment_count: float


def run_statistical_sweep(
    config: SimulationConfig | None = None,
    *,
    seeds: Sequence[int],
    minimum_distances: Sequence[float] = DEFAULT_MINIMUM_DISTANCES,
    noise_amplitudes: Sequence[float] = DEFAULT_NOISE_AMPLITUDES,
    frame_count: int = 21,
) -> tuple[list[SeedTrialResult], list[StatisticalSweepResult]]:
    """同じseed群を両方式へ適用し、試行結果と条件別統計を返す。"""
    if not seeds:
        raise ValueError("seeds must not be empty")
    if len(set(seeds)) != len(seeds):
        raise ValueError("seeds must be unique")
    config = config or SimulationConfig()
    trial_results: list[SeedTrialResult] = []

    for minimum_distance in minimum_distances:
        for noise_amplitude in noise_amplitudes:
            for seed in seeds:
                seeded_config = replace(config, random_seed=int(seed))
                # 1つのScenarioを両方式で共有するためDetection列も完全に同一。
                scenario = create_proximity_scenario(
                    minimum_distance,
                    noise_amplitude,
                    seeded_config,
                    frame_count=frame_count,
                )
                for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
                    evaluation = evaluate_scenario(scenario, method, seeded_config)
                    trial_results.append(SeedTrialResult(
                        minimum_distance_m=float(minimum_distance),
                        noise_amplitude_m=float(noise_amplitude),
                        method=method,
                        random_seed=int(seed),
                        id_switch_count=evaluation.id_switch_count,
                        success_rate_percent=evaluation.tracking_success_rate,
                        false_reassignment_count=evaluation.false_reassignment_count,
                        failed=evaluation.id_switch_count > 0,
                    ))

    grouped: dict[tuple[float, float, str], list[SeedTrialResult]] = {}
    for trial in trial_results:
        key = (trial.minimum_distance_m, trial.noise_amplitude_m, trial.method)
        grouped.setdefault(key, []).append(trial)

    summary_results: list[StatisticalSweepResult] = []
    for minimum_distance in minimum_distances:
        for noise_amplitude in noise_amplitudes:
            for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
                trials = grouped[(float(minimum_distance), float(noise_amplitude), method)]
                failures = sum(trial.failed for trial in trials)
                success_rates = [trial.success_rate_percent for trial in trials]
                summary_results.append(StatisticalSweepResult(
                    minimum_distance_m=float(minimum_distance),
                    noise_amplitude_m=float(noise_amplitude),
                    method=method,
                    seed_start=min(seeds),
                    seed_count=len(seeds),
                    failure_trial_count=failures,
                    failure_rate=failures / len(trials),
                    failure_rate_percent=100.0 * failures / len(trials),
                    mean_id_switch_count=mean(
                        trial.id_switch_count for trial in trials
                    ),
                    max_id_switch_count=max(
                        trial.id_switch_count for trial in trials
                    ),
                    mean_success_rate_percent=mean(success_rates),
                    # 母集団標準偏差。指定seed群そのもののばらつきを表す。
                    success_rate_stddev_percent=pstdev(success_rates),
                    mean_false_reassignment_count=mean(
                        trial.false_reassignment_count for trial in trials
                    ),
                ))
    return trial_results, summary_results


def format_failure_rate_tables(
    summaries: Sequence[StatisticalSweepResult],
    minimum_distances: Sequence[float],
    noise_amplitudes: Sequence[float],
) -> str:
    lookup = {
        (result.method, result.minimum_distance_m, result.noise_amplitude_m): result
        for result in summaries
    }
    lines = [
        "Failure rate = trials with IDSW >= 1 / all seed trials",
        "Cell format: failure_rate% (mean IDSW / mean success%)",
    ]
    for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
        lines.extend(["", f"=== {method} ==="])
        header = f"{'MinDist[m]':>10}" + "".join(
            f"  noise±{noise:.1f}m".rjust(30) for noise in noise_amplitudes
        )
        lines.append(header)
        lines.append("-" * len(header))
        for distance in minimum_distances:
            row = [f"{distance:>10.1f}"]
            for noise in noise_amplitudes:
                result = lookup[(method, float(distance), float(noise))]
                cell = (
                    f"{result.failure_rate_percent:>5.1f}% "
                    f"({result.mean_id_switch_count:.2f} / "
                    f"{result.mean_success_rate_percent:.1f}%)"
                )
                row.append(cell.rjust(30))
            lines.append("".join(row))
    return "\n".join(lines) + "\n"


def save_csv(
    rows: Sequence[SeedTrialResult] | Sequence[StatisticalSweepResult],
    path: Path,
) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    fieldnames = list(asdict(rows[0]).keys()) if rows else []
    with path.open("w", newline="", encoding="utf-8-sig") as stream:
        writer = csv.DictWriter(stream, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(asdict(row) for row in rows)


def save_failure_rate_heatmap(
    summaries: Sequence[StatisticalSweepResult],
    minimum_distances: Sequence[float],
    noise_amplitudes: Sequence[float],
    output_path: Path,
) -> None:
    """両方式を共通0〜100%スケールで比較するヒートマップ。"""
    cache_dir = Path(tempfile.gettempdir()) / "tracking-matplotlib-cache"
    cache_dir.mkdir(parents=True, exist_ok=True)
    os.environ.setdefault("MPLCONFIGDIR", str(cache_dir))
    try:
        import matplotlib
        matplotlib.use("Agg")
        import matplotlib.pyplot as plt
    except ModuleNotFoundError as error:
        raise RuntimeError(
            "Matplotlib is required for heatmap. "
            "Install: pip install -r tracking/requirements.txt"
        ) from error

    lookup = {
        (result.method, result.minimum_distance_m, result.noise_amplitude_m): result
        for result in summaries
    }
    methods = (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD)
    fig, axes = plt.subplots(1, 2, figsize=(13, 6), constrained_layout=True)
    image = None
    for ax, method in zip(axes, methods):
        matrix = [
            [
                lookup[(method, float(distance), float(noise))].failure_rate_percent
                for noise in noise_amplitudes
            ]
            for distance in minimum_distances
        ]
        image = ax.imshow(matrix, cmap="RdYlGn_r", vmin=0.0, vmax=100.0, aspect="auto")
        ax.set_title(method)
        ax.set_xlabel("Detection noise amplitude [m]")
        ax.set_ylabel("Minimum person distance [m]")
        ax.set_xticks(range(len(noise_amplitudes)), [f"±{value:.1f}" for value in noise_amplitudes])
        ax.set_yticks(range(len(minimum_distances)), [f"{value:.1f}" for value in minimum_distances])
        for row, distance in enumerate(minimum_distances):
            for column, noise in enumerate(noise_amplitudes):
                value = lookup[(method, float(distance), float(noise))].failure_rate_percent
                color = "white" if value >= 55.0 else "black"
                ax.text(column, row, f"{value:.0f}%", ha="center", va="center", color=color)
    if image is not None:
        fig.colorbar(image, ax=axes, label="Failure rate [%]", shrink=0.85)
    fig.suptitle("ID Switch failure rate across random seeds")
    output_path.parent.mkdir(parents=True, exist_ok=True)
    fig.savefig(output_path, dpi=170)
    plt.close(fig)


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Multi-seed MinDist x Noise statistical MOT evaluation"
    )
    parser.add_argument("--seed-start", type=int, default=20260811)
    parser.add_argument("--seed-count", type=int, default=100)
    parser.add_argument(
        "--distances", type=float, nargs="+", default=list(DEFAULT_MINIMUM_DISTANCES)
    )
    parser.add_argument(
        "--noise-levels", type=float, nargs="+", default=list(DEFAULT_NOISE_AMPLITUDES)
    )
    parser.add_argument("--frames", type=int, default=21)
    parser.add_argument("--max-association-distance", type=float, default=1.5)
    parser.add_argument("--max-missed-frames", type=int, default=3)
    parser.add_argument("--kalman-process-noise", type=float, default=0.1)
    parser.add_argument("--kalman-measurement-noise", type=float, default=0.1)
    parser.add_argument("--initial-position-covariance", type=float, default=1.0)
    parser.add_argument("--initial-velocity-covariance", type=float, default=100.0)
    parser.add_argument("--trials-csv", default="proximity_noise_multiseed_trials.csv")
    parser.add_argument("--summary-csv", default="proximity_noise_multiseed_summary.csv")
    parser.add_argument("--table", default="proximity_noise_failure_rate_table.txt")
    parser.add_argument("--heatmap", default="proximity_noise_failure_rate_heatmap.png")
    parser.add_argument("--no-heatmap", action="store_true")
    return parser


def main(argv: Sequence[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    if args.seed_count <= 0:
        raise SystemExit("--seed-count must be positive")
    seeds = list(range(args.seed_start, args.seed_start + args.seed_count))
    config = SimulationConfig(
        random_seed=args.seed_start,
        max_association_distance=args.max_association_distance,
        max_missed_frames=args.max_missed_frames,
        kalman_process_noise=args.kalman_process_noise,
        kalman_measurement_noise=args.kalman_measurement_noise,
        initial_position_covariance=args.initial_position_covariance,
        initial_velocity_covariance=args.initial_velocity_covariance,
    )
    trials, summaries = run_statistical_sweep(
        config,
        seeds=seeds,
        minimum_distances=args.distances,
        noise_amplitudes=args.noise_levels,
        frame_count=args.frames,
    )
    table_text = format_failure_rate_tables(
        summaries, args.distances, args.noise_levels
    )
    print(table_text, end="")
    save_csv(trials, Path(args.trials_csv))
    save_csv(summaries, Path(args.summary_csv))
    Path(args.table).write_text(table_text, encoding="utf-8")
    if not args.no_heatmap:
        save_failure_rate_heatmap(
            summaries,
            args.distances,
            args.noise_levels,
            Path(args.heatmap),
        )
        print(f"saved heatmap: {Path(args.heatmap).resolve()}")
    print(f"saved trials CSV: {Path(args.trials_csv).resolve()}")
    print(f"saved summary CSV: {Path(args.summary_csv).resolve()}")
    print(f"saved table: {Path(args.table).resolve()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
