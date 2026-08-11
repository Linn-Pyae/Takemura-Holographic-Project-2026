"""GreedyとHungarian+Kalmanを同じシナリオで比較するCLI。"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Sequence

from .evaluation import EvaluationResult, evaluate_methods
from .simulator import SimulationConfig, create_standard_scenarios


def print_results(
    results: Sequence[EvaluationResult],
    *,
    details: bool = False,
    show_records: bool = False,
) -> None:
    print("\n=== Quantitative metrics ===")
    header = (
        f"{'Scenario':<37} {'Method':<19} {'Frm':>4} {'Det':>4} "
        f"{'IDSW':>4} {'Frag':>4} {'Lost':>4} {'New':>4} {'False':>5} "
        f"{'MeanErr':>7} {'MaxErr':>7} {'Rec':>3} {'Succ%':>6} "
        f"{'AvgMs':>7} {'MaxMs':>7}"
    )
    print(header)
    print("-" * len(header))
    for result in results:
        print(
            f"{result.scenario:<37} {result.method:<19} "
            f"{result.total_frames:>4} {result.total_detections:>4} "
            f"{result.id_switch_count:>4} "
            f"{result.track_fragmentation_count:>4} "
            f"{result.lost_track_count:>4} {result.new_track_count:>4} "
            f"{result.false_reassignment_count:>5} "
            f"{result.mean_position_error:>7.3f} "
            f"{result.max_position_error:>7.3f} "
            f"{result.successful_id_recoveries:>3} "
            f"{result.tracking_success_rate:>6.1f} "
            f"{result.average_processing_time_ms:>7.3f} "
            f"{result.max_processing_time_ms:>7.3f}"
        )

    print("\n=== Greedy vs Hungarian + Kalman: ID Switch comparison ===")
    print(f"{'Scenario':<37} {'Greedy IDSW':>12} {'Hungarian+Kalman IDSW':>23}")
    print("-" * 76)
    by_scenario: dict[str, dict[str, EvaluationResult]] = {}
    for result in results:
        by_scenario.setdefault(result.scenario, {})[result.method] = result
    for scenario, method_results in by_scenario.items():
        greedy = method_results["greedy"].id_switch_count
        advanced = method_results["hungarian_kalman"].id_switch_count
        print(f"{scenario:<37} {greedy:>12} {advanced:>23}")

    if details:
        print("\n=== ID switch frames and per-person Track ID history ===")
        for result in results:
            print(f"\n[{result.scenario} / {result.method}]")
            print(f"switch frames: {result.id_switch_frames or 'none'}")
            for person_id, history in sorted(result.track_id_history.items()):
                print(f"  Person {person_id}: {history}")

    if show_records:
        print("\n=== Per-frame Ground Truth / Detection / Track assignment ===")
        for result in results:
            print(f"\n[{result.scenario} / {result.method}]")
            print(
                f"{'Frm':>4} {'GT':>3} {'TrueX':>7} {'TrueY':>7} "
                f"{'DetX':>7} {'DetY':>7} {'Track':>5} {'Name':<10} {'IDSW':>4}"
            )
            for record in result.records:
                detection_x = "-" if record.detection_x is None else f"{record.detection_x:.3f}"
                detection_y = "-" if record.detection_y is None else f"{record.detection_y:.3f}"
                track_id = "-" if record.assigned_track_id is None else str(record.assigned_track_id)
                track_name = record.assigned_track_name or "-"
                print(
                    f"{record.frame_number:>4} "
                    f"{record.ground_truth_person_id:>3} "
                    f"{record.true_x:>7.3f} {record.true_y:>7.3f} "
                    f"{detection_x:>7} {detection_y:>7} "
                    f"{track_id:>5} {track_name:<10} "
                    f"{'YES' if record.id_switch else '':>4}"
                )


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="LiDAR MOT simulation benchmark: Greedy vs Hungarian+Kalman"
    )
    parser.add_argument(
        "--scenario",
        action="append",
        default=[],
        help="シナリオ名（複数指定可）。省略またはallで全件",
    )
    parser.add_argument("--details", action="store_true", help="ID履歴も表示")
    parser.add_argument(
        "--show-records",
        action="store_true",
        help="全フレームのGT/Detection/Track割当を表示",
    )
    parser.add_argument("--output-json", help="全指標とフレーム詳細をJSON保存")
    parser.add_argument("--max-association-distance", type=float, default=1.5)
    parser.add_argument("--max-missed-frames", type=int, default=3)
    parser.add_argument("--kalman-process-noise", type=float, default=0.1)
    parser.add_argument("--kalman-measurement-noise", type=float, default=0.1)
    parser.add_argument("--initial-position-covariance", type=float, default=1.0)
    parser.add_argument("--initial-velocity-covariance", type=float, default=100.0)
    parser.add_argument("--noise-amplitude", type=float, default=0.2)
    parser.add_argument("--seed", type=int, default=20260811)
    parser.add_argument("--number-of-people", type=int, default=4)
    return parser


def main(argv: Sequence[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    config = SimulationConfig(
        max_association_distance=args.max_association_distance,
        max_missed_frames=args.max_missed_frames,
        kalman_process_noise=args.kalman_process_noise,
        kalman_measurement_noise=args.kalman_measurement_noise,
        initial_position_covariance=args.initial_position_covariance,
        initial_velocity_covariance=args.initial_velocity_covariance,
        noise_amplitude=args.noise_amplitude,
        random_seed=args.seed,
        number_of_people=args.number_of_people,
    )
    all_scenarios = create_standard_scenarios(config)
    requested = args.scenario
    if not requested or "all" in requested:
        selected = list(all_scenarios.values())
    else:
        unknown = sorted(set(requested) - set(all_scenarios))
        if unknown:
            print(f"Unknown scenario(s): {', '.join(unknown)}")
            print(f"Available: {', '.join(all_scenarios)}")
            return 2
        selected = [all_scenarios[name] for name in requested]

    results = evaluate_methods(selected, config)
    print_results(
        results,
        details=args.details,
        show_records=args.show_records,
    )
    if args.output_json:
        output = Path(args.output_json)
        output.write_text(
            json.dumps(
                {
                    "config": config.__dict__,
                    "results": [result.to_dict() for result in results],
                },
                ensure_ascii=False,
                indent=2,
            ),
            encoding="utf-8",
        )
        print(f"\nsaved: {output.resolve()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
