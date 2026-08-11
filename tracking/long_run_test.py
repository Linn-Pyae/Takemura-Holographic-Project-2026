"""数千〜数万フレームで速度・メモリ・history・ID管理を監視する。"""

from __future__ import annotations

import argparse
from dataclasses import asdict, dataclass
import json
from statistics import mean
from time import perf_counter
import tracemalloc
from typing import Sequence

from .evaluation import (
    GREEDY_METHOD,
    HUNGARIAN_KALMAN_METHOD,
    create_tracker,
)
from .simulator import SimulationConfig, iter_long_run_frames


@dataclass
class LongRunResult:
    method: str
    total_frames: int
    total_detections: int
    total_track_ids_created: int
    max_active_tracks: int
    max_history_length: int
    stale_id_reuse_count: int
    average_processing_time_ms: float
    max_processing_time_ms: float
    first_10_percent_average_ms: float
    last_10_percent_average_ms: float
    processing_slowdown_ratio: float
    current_traced_memory_mb: float
    peak_traced_memory_mb: float
    sampled_memory_growth_mb: float
    history_is_unbounded_risk: bool


def run_long_test(
    frame_count: int,
    method: str,
    config: SimulationConfig,
    *,
    memory_sample_interval: int = 500,
) -> LongRunResult:
    tracker = create_tracker(method, config)
    timings: list[float] = []
    memory_samples: list[int] = []
    seen_ids: set[int] = set()
    previous_active_ids: set[int] = set()
    stale_id_reuse_count = 0
    max_active_tracks = 0
    max_history_length = 0
    total_detections = 0

    tracemalloc.start()
    try:
        for frame in iter_long_run_frames(frame_count, config):
            total_detections += len(frame.detections)
            start = perf_counter()
            tracks = tracker.update([item.detection for item in frame.detections])
            timings.append((perf_counter() - start) * 1000.0)

            active_ids = {track.id for track in tracks}
            if len(active_ids) != len(tracks):
                raise AssertionError("duplicate active Track ID detected")
            newly_active = active_ids - previous_active_ids
            stale_id_reuse_count += len(newly_active & seen_ids)
            seen_ids.update(active_ids)
            previous_active_ids = active_ids
            max_active_tracks = max(max_active_tracks, len(tracks))
            max_history_length = max(
                max_history_length,
                max((len(track.history) for track in tracks), default=0),
            )
            if frame.frame_number % max(1, memory_sample_interval) == 0:
                memory_samples.append(tracemalloc.get_traced_memory()[0])
        current_memory, peak_memory = tracemalloc.get_traced_memory()
    finally:
        tracemalloc.stop()

    window = max(1, frame_count // 10)
    first_average = mean(timings[:window]) if timings else 0.0
    last_average = mean(timings[-window:]) if timings else 0.0
    slowdown = last_average / first_average if first_average > 0 else 0.0
    memory_growth = (
        memory_samples[-1] - memory_samples[0] if len(memory_samples) > 1 else 0
    )
    return LongRunResult(
        method=method,
        total_frames=frame_count,
        total_detections=total_detections,
        total_track_ids_created=len(seen_ids),
        max_active_tracks=max_active_tracks,
        max_history_length=max_history_length,
        stale_id_reuse_count=stale_id_reuse_count,
        average_processing_time_ms=mean(timings) if timings else 0.0,
        max_processing_time_ms=max(timings, default=0.0),
        first_10_percent_average_ms=first_average,
        last_10_percent_average_ms=last_average,
        processing_slowdown_ratio=slowdown,
        current_traced_memory_mb=current_memory / (1024 * 1024),
        peak_traced_memory_mb=peak_memory / (1024 * 1024),
        sampled_memory_growth_mb=memory_growth / (1024 * 1024),
        history_is_unbounded_risk=max_history_length > max(100, frame_count // 2),
    )


def print_long_results(results: Sequence[LongRunResult]) -> None:
    for result in results:
        print(f"\n=== Long run: {result.method} ===")
        for key, value in asdict(result).items():
            print(f"{key}: {value}")
        if result.history_is_unbounded_risk:
            print("WARNING: history grows with frames; a configurable cap is recommended.")


def main(argv: Sequence[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Long-running LiDAR MOT simulation")
    parser.add_argument("--frames", type=int, default=10000)
    parser.add_argument(
        "--method",
        choices=("both", GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD),
        default="both",
    )
    parser.add_argument("--people", type=int, default=4)
    parser.add_argument("--seed", type=int, default=20260811)
    parser.add_argument("--noise-amplitude", type=float, default=0.2)
    parser.add_argument("--max-association-distance", type=float, default=1.5)
    parser.add_argument("--max-missed-frames", type=int, default=3)
    parser.add_argument("--output-json")
    args = parser.parse_args(argv)
    if args.frames <= 0:
        parser.error("--frames must be positive")
    config = SimulationConfig(
        number_of_people=args.people,
        random_seed=args.seed,
        noise_amplitude=args.noise_amplitude,
        max_association_distance=args.max_association_distance,
        max_missed_frames=args.max_missed_frames,
    )
    methods = (
        (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD)
        if args.method == "both" else (args.method,)
    )
    results = [run_long_test(args.frames, method, config) for method in methods]
    print_long_results(results)
    if args.output_json:
        with open(args.output_json, "w", encoding="utf-8") as stream:
            json.dump([asdict(result) for result in results], stream, indent=2)
        print(f"saved: {args.output_json}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
