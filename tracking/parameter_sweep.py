"""人物間最接近距離×Detectionノイズ量の総当たり評価。"""

from __future__ import annotations

import argparse
import csv
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Sequence

from .evaluation import (
    GREEDY_METHOD,
    HUNGARIAN_KALMAN_METHOD,
    evaluate_scenario,
)
from .simulator import (
    Scenario,
    SimulationConfig,
    build_scenario,
    linear_trajectory,
)


DEFAULT_MINIMUM_DISTANCES = (2.0, 1.0, 0.5, 0.3, 0.1, 0.0)
DEFAULT_NOISE_AMPLITUDES = (0.0, 0.1, 0.2, 0.3)


@dataclass(frozen=True)
class SweepResult:
    minimum_distance_m: float
    measured_minimum_distance_m: float
    noise_amplitude_m: float
    method: str
    random_seed: int
    total_frames: int
    total_detections: int
    id_switch_count: int
    tracking_success_rate: float
    false_reassignment_count: int
    track_fragmentation_count: int
    new_track_count: int
    mean_position_error_m: float
    max_position_error_m: float
    average_processing_time_ms: float
    max_processing_time_ms: float
    unstable: bool


def create_proximity_scenario(
    minimum_distance: float,
    noise_amplitude: float,
    config: SimulationConfig,
    *,
    frame_count: int = 21,
) -> Scenario:
    """中央フレームで人物間距離が指定値になる、2人の平行すれ違い。"""
    if minimum_distance < 0:
        raise ValueError("minimum_distance must be non-negative")
    if noise_amplitude < 0:
        raise ValueError("noise_amplitude must be non-negative")
    if frame_count < 3 or frame_count % 2 == 0:
        raise ValueError("frame_count must be an odd integer >= 3")
    velocity_x = 10.0 / (frame_count - 1)
    safe_distance = str(minimum_distance).replace(".", "_")
    safe_noise = str(noise_amplitude).replace(".", "_")
    name = f"proximity_d{safe_distance}_noise{safe_noise}"
    return build_scenario(
        name,
        f"minimum distance={minimum_distance:.3f}m, noise=±{noise_amplitude:.3f}m",
        {
            "A": linear_trajectory(
                frame_count,
                (-5.0, -minimum_distance / 2.0),
                (velocity_x, 0.0),
            ),
            "B": linear_trajectory(
                frame_count,
                (5.0, minimum_distance / 2.0),
                (-velocity_x, 0.0),
            ),
        },
        config,
        noise_amplitude=noise_amplitude,
        shuffle_detections=True,
    )


def _measured_minimum_distance(scenario: Scenario) -> float:
    distances: list[float] = []
    for frame in scenario.frames:
        if "A" not in frame.ground_truth or "B" not in frame.ground_truth:
            continue
        a = frame.ground_truth["A"]
        b = frame.ground_truth["B"]
        distances.append(((a.x - b.x) ** 2 + (a.y - b.y) ** 2) ** 0.5)
    return min(distances)


def run_parameter_sweep(
    config: SimulationConfig | None = None,
    *,
    minimum_distances: Sequence[float] = DEFAULT_MINIMUM_DISTANCES,
    noise_amplitudes: Sequence[float] = DEFAULT_NOISE_AMPLITUDES,
    frame_count: int = 21,
) -> list[SweepResult]:
    """全条件へ同一シナリオを2方式で適用する。"""
    config = config or SimulationConfig()
    results: list[SweepResult] = []
    for minimum_distance in minimum_distances:
        for noise_amplitude in noise_amplitudes:
            scenario = create_proximity_scenario(
                minimum_distance,
                noise_amplitude,
                config,
                frame_count=frame_count,
            )
            measured = _measured_minimum_distance(scenario)
            for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
                evaluation = evaluate_scenario(scenario, method, config)
                unstable = (
                    evaluation.id_switch_count > 0
                    or evaluation.tracking_success_rate < 100.0
                )
                results.append(SweepResult(
                    minimum_distance_m=float(minimum_distance),
                    measured_minimum_distance_m=measured,
                    noise_amplitude_m=float(noise_amplitude),
                    method=method,
                    random_seed=config.random_seed,
                    total_frames=evaluation.total_frames,
                    total_detections=evaluation.total_detections,
                    id_switch_count=evaluation.id_switch_count,
                    tracking_success_rate=evaluation.tracking_success_rate,
                    false_reassignment_count=evaluation.false_reassignment_count,
                    track_fragmentation_count=evaluation.track_fragmentation_count,
                    new_track_count=evaluation.new_track_count,
                    mean_position_error_m=evaluation.mean_position_error,
                    max_position_error_m=evaluation.max_position_error,
                    average_processing_time_ms=evaluation.average_processing_time_ms,
                    max_processing_time_ms=evaluation.max_processing_time_ms,
                    unstable=unstable,
                ))
    return results


def format_sweep_tables(
    results: Sequence[SweepResult],
    minimum_distances: Sequence[float],
    noise_amplitudes: Sequence[float],
) -> str:
    """方式別matrixと不安定条件一覧をテキスト化する。"""
    lookup = {
        (result.method, result.minimum_distance_m, result.noise_amplitude_m): result
        for result in results
    }
    lines = [
        "Cell format: IDSW / success_rate%  (* = unstable)",
        "Unstable definition: IDSW > 0 or success_rate < 100%",
    ]
    for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
        lines.extend(["", f"=== {method} ==="])
        header = f"{'MinDist[m]':>10}" + "".join(
            f"  noise±{noise:.1f}m".rjust(19) for noise in noise_amplitudes
        )
        lines.append(header)
        lines.append("-" * len(header))
        for distance in minimum_distances:
            row = [f"{distance:>10.1f}"]
            for noise in noise_amplitudes:
                result = lookup[(method, float(distance), float(noise))]
                marker = "*" if result.unstable else " "
                row.append(
                    f"{result.id_switch_count:>3} / "
                    f"{result.tracking_success_rate:>6.1f}%{marker}".rjust(19)
                )
            lines.append("".join(row))

    unstable_results = [result for result in results if result.unstable]
    lines.extend([
        "",
        "=== Unstable conditions ===",
        f"{'Method':<19} {'MinDist[m]':>10} {'Noise[m]':>9} "
        f"{'IDSW':>5} {'Success%':>9} {'False':>5}",
        "-" * 64,
    ])
    if not unstable_results:
        lines.append("none")
    else:
        for result in unstable_results:
            lines.append(
                f"{result.method:<19} "
                f"{result.minimum_distance_m:>10.1f} "
                f"{result.noise_amplitude_m:>9.1f} "
                f"{result.id_switch_count:>5} "
                f"{result.tracking_success_rate:>9.1f} "
                f"{result.false_reassignment_count:>5}"
            )
    return "\n".join(lines) + "\n"


def save_sweep_outputs(
    results: Sequence[SweepResult],
    table_text: str,
    *,
    csv_path: Path,
    table_path: Path,
) -> None:
    csv_path.parent.mkdir(parents=True, exist_ok=True)
    table_path.parent.mkdir(parents=True, exist_ok=True)
    fieldnames = list(asdict(results[0]).keys()) if results else []
    with csv_path.open("w", newline="", encoding="utf-8-sig") as stream:
        writer = csv.DictWriter(stream, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(asdict(result) for result in results)
    table_path.write_text(table_text, encoding="utf-8")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Sweep minimum person distance and Detection noise"
    )
    parser.add_argument(
        "--distances",
        type=float,
        nargs="+",
        default=list(DEFAULT_MINIMUM_DISTANCES),
    )
    parser.add_argument(
        "--noise-levels",
        type=float,
        nargs="+",
        default=list(DEFAULT_NOISE_AMPLITUDES),
    )
    parser.add_argument("--frames", type=int, default=21)
    parser.add_argument("--seed", type=int, default=20260811)
    parser.add_argument("--max-association-distance", type=float, default=1.5)
    parser.add_argument("--max-missed-frames", type=int, default=3)
    parser.add_argument("--kalman-process-noise", type=float, default=0.1)
    parser.add_argument("--kalman-measurement-noise", type=float, default=0.1)
    parser.add_argument("--initial-position-covariance", type=float, default=1.0)
    parser.add_argument("--initial-velocity-covariance", type=float, default=100.0)
    parser.add_argument("--csv", default="proximity_noise_sweep.csv")
    parser.add_argument("--table", default="proximity_noise_sweep_table.txt")
    return parser


def main(argv: Sequence[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    config = SimulationConfig(
        random_seed=args.seed,
        max_association_distance=args.max_association_distance,
        max_missed_frames=args.max_missed_frames,
        kalman_process_noise=args.kalman_process_noise,
        kalman_measurement_noise=args.kalman_measurement_noise,
        initial_position_covariance=args.initial_position_covariance,
        initial_velocity_covariance=args.initial_velocity_covariance,
    )
    results = run_parameter_sweep(
        config,
        minimum_distances=args.distances,
        noise_amplitudes=args.noise_levels,
        frame_count=args.frames,
    )
    table_text = format_sweep_tables(results, args.distances, args.noise_levels)
    print(table_text, end="")
    save_sweep_outputs(
        results,
        table_text,
        csv_path=Path(args.csv),
        table_path=Path(args.table),
    )
    print(f"saved CSV: {Path(args.csv).resolve()}")
    print(f"saved table: {Path(args.table).resolve()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
