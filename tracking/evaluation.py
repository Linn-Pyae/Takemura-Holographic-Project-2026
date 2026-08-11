"""シミュレーション結果とGround Truthを照合する定量評価。"""

from __future__ import annotations

from dataclasses import asdict, dataclass, field
from math import hypot
from statistics import mean
from time import perf_counter
from typing import Any, Iterable

from .motion import KalmanFilter2D
from .simulator import Scenario, SimulationConfig
from .tracker import (
    GreedyNearestNeighborMatcher,
    HungarianMatcher,
    MultiObjectTracker,
    TrackerConfig,
)


GREEDY_METHOD = "greedy"
HUNGARIAN_KALMAN_METHOD = "hungarian_kalman"


@dataclass(frozen=True)
class AssignmentRecord:
    """1人物・1フレームについてGround Truthと割当結果を保持する。"""

    frame_number: int
    ground_truth_person_id: str
    true_x: float
    true_y: float
    detection_x: float | None
    detection_y: float | None
    assigned_track_id: int | None
    assigned_track_name: str | None
    track_x: float | None
    track_y: float | None
    missed_frames: int | None
    id_switch: bool = False


@dataclass
class EvaluationResult:
    scenario: str
    method: str
    total_frames: int
    total_detections: int
    id_switch_count: int
    track_fragmentation_count: int
    lost_track_count: int
    new_track_count: int
    false_reassignment_count: int
    mean_position_error: float
    max_position_error: float
    successful_id_recoveries: int
    id_switch_frames: list[int] = field(default_factory=list)
    track_id_history: dict[str, list[int | None]] = field(default_factory=dict)
    tracking_success_rate: float = 0.0
    average_processing_time_ms: float = 0.0
    max_processing_time_ms: float = 0.0
    records: list[AssignmentRecord] = field(default_factory=list)

    def to_dict(self, *, include_records: bool = True) -> dict[str, Any]:
        data = asdict(self)
        if not include_records:
            data.pop("records", None)
        return data


def create_tracker(method: str, config: SimulationConfig) -> MultiObjectTracker:
    """比較条件を一か所で定義し、全シナリオへ同じ設定を適用する。"""
    tracker_config = TrackerConfig(
        max_association_distance=config.max_association_distance,
        max_missed_frames=config.max_missed_frames,
    )
    if method == GREEDY_METHOD:
        return MultiObjectTracker(
            tracker_config,
            association_strategy=GreedyNearestNeighborMatcher(),
        )
    if method == HUNGARIAN_KALMAN_METHOD:
        return MultiObjectTracker(
            tracker_config,
            association_strategy=HungarianMatcher(),
            motion_model=KalmanFilter2D(
                process_variance=config.kalman_process_noise,
                measurement_variance=config.kalman_measurement_noise,
                initial_position_variance=config.initial_position_covariance,
                initial_velocity_variance=config.initial_velocity_covariance,
            ),
        )
    raise ValueError(f"unknown tracking method: {method}")


def evaluate_scenario(
    scenario: Scenario,
    method: str,
    config: SimulationConfig,
) -> EvaluationResult:
    """1シナリオを実行し、人物ID単位で割当品質を集計する。"""
    tracker = create_tracker(method, config)
    total_detections = 0
    id_switch_count = 0
    fragmentation_count = 0
    lost_track_count = 0
    false_reassignment_count = 0
    successful_recoveries = 0
    identity_correct_detections = 0
    switch_frames: list[int] = []
    errors: list[float] = []
    timings_ms: list[float] = []
    records: list[AssignmentRecord] = []

    first_track_by_person: dict[str, int] = {}
    last_track_by_person: dict[str, int] = {}
    last_detection_frame: dict[str, int] = {}
    initial_owner_by_track: dict[int, str] = {}
    track_id_history: dict[str, list[int | None]] = {}
    seen_track_ids: set[int] = set()
    lost_people: set[str] = set()

    for frame in scenario.frames:
        detections = [item.detection for item in frame.detections]
        start = perf_counter()
        tracks = tracker.update(detections)
        timings_ms.append((perf_counter() - start) * 1000.0)
        total_detections += len(frame.detections)

        active_track_ids = {track.id for track in tracks}
        new_ids = active_track_ids - seen_track_ids
        seen_track_ids.update(new_ids)

        # 以前のTrackが消滅し、人物自体はまだシーンにいる瞬間をlostとする。
        for person_id in frame.ground_truth:
            previous_id = last_track_by_person.get(person_id)
            if previous_id is not None and previous_id not in active_track_ids:
                if person_id not in lost_people:
                    lost_track_count += 1
                    lost_people.add(person_id)

        # 同一人物ラベルのTrackが複数ある場合、現フレームで観測更新された方を優先。
        tracks_by_person: dict[str, Any] = {}
        for track in sorted(tracks, key=lambda item: (item.missed_frames, -item.id)):
            person_id = track.metadata.get("ground_truth_person_id")
            if person_id is not None and person_id not in tracks_by_person:
                tracks_by_person[person_id] = track
        detections_by_person = {
            item.ground_truth_person_id: item for item in frame.detections
        }

        for person_id, true_position in frame.ground_truth.items():
            simulated_detection = detections_by_person.get(person_id)
            track = tracks_by_person.get(person_id)
            assigned_id = None if track is None else track.id
            history = track_id_history.setdefault(person_id, [])
            history.append(assigned_id)
            switched = False

            if simulated_detection is not None and track is not None:
                previous_id = last_track_by_person.get(person_id)
                previous_detection_frame = last_detection_frame.get(person_id)
                if previous_id is None:
                    first_track_by_person[person_id] = track.id
                elif track.id != previous_id:
                    id_switch_count += 1
                    switched = True
                    if frame.frame_number not in switch_frames:
                        switch_frames.append(frame.frame_number)

                if previous_detection_frame is not None:
                    detection_gap = frame.frame_number - previous_detection_frame - 1
                    if detection_gap > 0:
                        fragmentation_count += 1
                        if (
                            detection_gap <= config.max_missed_frames
                            and track.id == previous_id
                        ):
                            successful_recoveries += 1

                owner = initial_owner_by_track.get(track.id)
                if owner is None:
                    initial_owner_by_track[track.id] = person_id
                elif owner != person_id:
                    false_reassignment_count += 1

                if track.id == first_track_by_person[person_id]:
                    identity_correct_detections += 1
                last_track_by_person[person_id] = track.id
                last_detection_frame[person_id] = frame.frame_number
                lost_people.discard(person_id)

                errors.append(hypot(
                    track.current_position.x - true_position.x,
                    track.current_position.y - true_position.y,
                ))

            detection_position = (
                None if simulated_detection is None
                else simulated_detection.detection.position
            )
            records.append(AssignmentRecord(
                frame_number=frame.frame_number,
                ground_truth_person_id=person_id,
                true_x=true_position.x,
                true_y=true_position.y,
                detection_x=None if detection_position is None else detection_position.x,
                detection_y=None if detection_position is None else detection_position.y,
                assigned_track_id=assigned_id,
                assigned_track_name=None if track is None else track.name,
                track_x=None if track is None else track.current_position.x,
                track_y=None if track is None else track.current_position.y,
                missed_frames=None if track is None else track.missed_frames,
                id_switch=switched,
            ))

    success_rate = (
        100.0 * identity_correct_detections / total_detections
        if total_detections else 100.0
    )
    return EvaluationResult(
        scenario=scenario.name,
        method=method,
        total_frames=len(scenario.frames),
        total_detections=total_detections,
        id_switch_count=id_switch_count,
        track_fragmentation_count=fragmentation_count,
        lost_track_count=lost_track_count,
        new_track_count=len(seen_track_ids),
        false_reassignment_count=false_reassignment_count,
        mean_position_error=mean(errors) if errors else 0.0,
        max_position_error=max(errors, default=0.0),
        successful_id_recoveries=successful_recoveries,
        id_switch_frames=switch_frames,
        track_id_history=track_id_history,
        tracking_success_rate=success_rate,
        average_processing_time_ms=mean(timings_ms) if timings_ms else 0.0,
        max_processing_time_ms=max(timings_ms, default=0.0),
        records=records,
    )


def evaluate_methods(
    scenarios: Iterable[Scenario],
    config: SimulationConfig,
) -> list[EvaluationResult]:
    results: list[EvaluationResult] = []
    for scenario in scenarios:
        for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
            results.append(evaluate_scenario(scenario, method, config))
    return results
