"""実機LiDARなしで再現可能なGround TruthとDetectionを生成する。"""

from __future__ import annotations

from dataclasses import dataclass
import random
from typing import Iterable, Iterator, Mapping, Sequence

from .models import Detection, Position2D


@dataclass(frozen=True)
class SimulationConfig:
    """シミュレーション・Tracker生成の共通設定。"""

    max_association_distance: float = 1.5
    max_missed_frames: int = 3
    kalman_process_noise: float = 0.1
    kalman_measurement_noise: float = 0.1
    initial_position_covariance: float = 1.0
    initial_velocity_covariance: float = 100.0
    noise_amplitude: float = 0.2
    random_seed: int = 20260811
    number_of_people: int = 4

    def __post_init__(self) -> None:
        if self.max_association_distance <= 0:
            raise ValueError("max_association_distance must be positive")
        if self.max_missed_frames < 0:
            raise ValueError("max_missed_frames must be non-negative")
        if self.noise_amplitude < 0:
            raise ValueError("noise_amplitude must be non-negative")
        if self.number_of_people <= 0:
            raise ValueError("number_of_people must be positive")


@dataclass(frozen=True)
class SimulatedDetection:
    ground_truth_person_id: str
    true_position: Position2D
    detection: Detection


@dataclass(frozen=True)
class SimulationFrame:
    frame_number: int
    ground_truth: Mapping[str, Position2D]
    detections: tuple[SimulatedDetection, ...]


@dataclass(frozen=True)
class Scenario:
    name: str
    description: str
    frames: tuple[SimulationFrame, ...]


Trajectory = Sequence[Position2D | None]


def linear_trajectory(
    frame_count: int,
    start: tuple[float, float],
    velocity: tuple[float, float],
    *,
    active_start: int = 0,
    active_end: int | None = None,
) -> list[Position2D | None]:
    """一定速度の軌道。active範囲外はシーンに存在しない。"""
    end = frame_count if active_end is None else active_end
    result: list[Position2D | None] = []
    for frame in range(frame_count):
        if frame < active_start or frame >= end:
            result.append(None)
            continue
        elapsed = frame - active_start
        result.append(Position2D(
            start[0] + velocity[0] * elapsed,
            start[1] + velocity[1] * elapsed,
        ))
    return result


def _scenario_seed(config: SimulationConfig, name: str) -> int:
    # Pythonのhash()はプロセスごとに変化するため使用しない。
    return config.random_seed + sum((index + 1) * ord(char) for index, char in enumerate(name))


def build_scenario(
    name: str,
    description: str,
    trajectories: Mapping[str, Trajectory],
    config: SimulationConfig,
    *,
    dropout_frames: Mapping[str, Iterable[int]] | None = None,
    noise_amplitude: float = 0.0,
    shuffle_detections: bool = False,
) -> Scenario:
    """人物軌道からGround Truthとノイズ付きDetectionを生成する。"""
    frame_count = max((len(path) for path in trajectories.values()), default=0)
    dropouts = {
        person_id: set(frames)
        for person_id, frames in (dropout_frames or {}).items()
    }
    rng = random.Random(_scenario_seed(config, name))
    frames: list[SimulationFrame] = []

    for frame_number in range(frame_count):
        ground_truth: dict[str, Position2D] = {}
        detections: list[SimulatedDetection] = []
        for person_id, trajectory in trajectories.items():
            if frame_number >= len(trajectory):
                continue
            true_position = trajectory[frame_number]
            if true_position is None:
                continue
            ground_truth[person_id] = true_position
            if frame_number in dropouts.get(person_id, set()):
                continue
            detected_x = true_position.x + rng.uniform(-noise_amplitude, noise_amplitude)
            detected_y = true_position.y + rng.uniform(-noise_amplitude, noise_amplitude)
            detection = Detection.from_xy(
                detected_x,
                detected_y,
                timestamp=float(frame_number),
                metadata={"ground_truth_person_id": person_id},
            )
            detections.append(SimulatedDetection(person_id, true_position, detection))
        if shuffle_detections:
            rng.shuffle(detections)
        frames.append(SimulationFrame(frame_number, ground_truth, tuple(detections)))
    return Scenario(name, description, tuple(frames))


def _positions(values: Sequence[tuple[float, float] | None]) -> list[Position2D | None]:
    return [None if value is None else Position2D(*value) for value in values]


def create_standard_scenarios(config: SimulationConfig | None = None) -> dict[str, Scenario]:
    """要求されたストレス条件を網羅する標準シナリオ群。"""
    config = config or SimulationConfig()
    scenarios: dict[str, Scenario] = {}

    scenarios["two_person_overlap"] = build_scenario(
        "two_person_overlap",
        "2人が接近し、中央で同一座標へ重なる",
        {
            "A": linear_trajectory(13, (-3.0, 0.0), (0.5, 0.0)),
            "B": linear_trajectory(13, (3.0, 0.0), (-0.5, 0.0)),
        },
        config,
        shuffle_detections=True,
    )

    three_paths = {
        "A": linear_trajectory(9, (-4.0, 0.0), (1.0, 0.0)),
        "B": linear_trajectory(9, (4.0, 0.0), (-1.0, 0.0)),
        "C": linear_trajectory(9, (0.0, -4.0), (0.0, 1.0)),
    }
    scenarios["three_person_crossing"] = build_scenario(
        "three_person_crossing", "3人が同時に原点で交差", three_paths, config,
        shuffle_detections=True,
    )
    scenarios["four_person_crossing"] = build_scenario(
        "four_person_crossing",
        "4人が4方向から同時に原点で交差",
        {
            **three_paths,
            "D": linear_trajectory(9, (0.0, 4.0), (0.0, -1.0)),
        },
        config,
        shuffle_detections=True,
    )

    noise_paths = {
        "A": linear_trajectory(20, (-6.0, -3.0), (0.35, 0.0)),
        "B": linear_trajectory(20, (6.0, 3.0), (-0.30, 0.0)),
        "C": linear_trajectory(20, (-5.0, 4.0), (0.25, -0.05)),
        "D": linear_trajectory(20, (5.0, -4.0), (-0.20, 0.05)),
    }
    for amplitude in (0.1, 0.2, 0.3):
        suffix = str(amplitude).replace(".", "_")
        name = f"noise_{suffix}m"
        scenarios[name] = build_scenario(
            name,
            f"4人のDetectionへ±{amplitude:.1f}mノイズ",
            noise_paths,
            config,
            noise_amplitude=amplitude,
            shuffle_detections=True,
        )

    scenarios["sudden_direction_change"] = build_scenario(
        "sudden_direction_change",
        "近接中に人物Aが急反転する",
        {
            "A": _positions([(-2, 0), (-1, 0), (0, 0), (-1, 0), (-2, 0), (-3, 0)]),
            "B": _positions([(1, 0)] * 6),
        },
        config,
    )

    scenarios["stationary_near_pass"] = build_scenario(
        "stationary_near_pass",
        "停止人物Aの0.25m横を人物Bが通過",
        {
            "A": _positions([(0.0, 0.0)] * 13),
            "B": linear_trajectory(13, (-3.0, 0.25), (0.5, 0.0)),
        },
        config,
        shuffle_detections=True,
    )

    dropout_paths = {
        "A": linear_trajectory(10, (0.0, 0.0), (0.5, 0.0)),
        "B": linear_trajectory(10, (0.0, 4.0), (0.4, 0.0)),
    }
    scenarios["short_dropout_1_2_frames"] = build_scenario(
        "short_dropout_1_2_frames",
        "人物Aは1フレーム、人物Bは2フレームDetectionが消失",
        dropout_paths,
        config,
        dropout_frames={"A": {4}, "B": {5, 6}},
        shuffle_detections=True,
    )
    near_gap = set(range(3, 3 + config.max_missed_frames))
    scenarios["dropout_near_max_missed"] = build_scenario(
        "dropout_near_max_missed",
        "max_missed_framesと同数のDetection欠落",
        {"A": linear_trajectory(10 + config.max_missed_frames, (0, 0), (0.4, 0))},
        config,
        dropout_frames={"A": near_gap},
    )
    over_gap = set(range(3, 4 + config.max_missed_frames))
    scenarios["dropout_over_max_missed"] = build_scenario(
        "dropout_over_max_missed",
        "max_missed_framesを超えて欠落後に再登場",
        {"A": linear_trajectory(11 + config.max_missed_frames, (0, 0), (0.4, 0))},
        config,
        dropout_frames={"A": over_gap},
    )

    scenarios["detection_order_shuffle"] = build_scenario(
        "detection_order_shuffle",
        "十分離れた4人のDetection順を毎フレームshuffle",
        noise_paths,
        config,
        shuffle_detections=True,
    )
    scenarios["new_person_enters"] = build_scenario(
        "new_person_enters",
        "人物BとCが途中からシーンへ入る",
        {
            "A": linear_trajectory(15, (-5, -2), (0.4, 0)),
            "B": linear_trajectory(15, (5, 2), (-0.3, 0), active_start=5),
            "C": linear_trajectory(15, (0, 5), (0, -0.4), active_start=9),
        },
        config,
        shuffle_detections=True,
    )
    scenarios["multiple_people_exit"] = build_scenario(
        "multiple_people_exit",
        "4人中3人が異なるフレームで範囲外へ出る",
        {
            "A": linear_trajectory(16, (-5, -3), (0.3, 0)),
            "B": linear_trajectory(16, (5, 3), (-0.3, 0), active_end=7),
            "C": linear_trajectory(16, (-4, 4), (0.2, 0), active_end=10),
            "D": linear_trajectory(16, (4, -4), (-0.2, 0), active_end=13),
        },
        config,
        shuffle_detections=True,
    )
    scenarios["four_people_different_speeds"] = build_scenario(
        "four_people_different_speeds",
        "4人が互いに離れたレーンを異なる速度で移動",
        {
            "A": linear_trajectory(20, (-7, -3), (0.20, 0)),
            "B": linear_trajectory(20, (-7, -1), (0.35, 0)),
            "C": linear_trajectory(20, (-7, 1), (0.50, 0)),
            "D": linear_trajectory(20, (-7, 3), (0.70, 0)),
        },
        config,
        shuffle_detections=True,
    )

    accelerated: list[Position2D] = []
    x = -4.0
    velocities = [0.2, 0.3, 0.5, 0.8, 1.2, 1.6, 1.0, 0.5, 0.2, 0.1, 0.6, 1.0]
    for velocity in velocities:
        accelerated.append(Position2D(x, 0.0))
        x += velocity
    scenarios["sudden_acceleration_deceleration"] = build_scenario(
        "sudden_acceleration_deceleration",
        "人物Aだけが急加速・急減速する",
        {
            "A": accelerated,
            "B": linear_trajectory(len(accelerated), (-4, 3), (0.4, 0)),
        },
        config,
        shuffle_detections=True,
    )

    complex_count = 21
    scenarios["combined_noise_crossing_dropout"] = build_scenario(
        "combined_noise_crossing_dropout",
        "±設定ノイズ、4人交差、Detection欠落、shuffleを同時発生",
        {
            "A": linear_trajectory(complex_count, (-5, -0.3), (0.5, 0)),
            "B": linear_trajectory(complex_count, (5, 0.3), (-0.5, 0)),
            "C": linear_trajectory(complex_count, (-0.3, -5), (0, 0.5)),
            "D": linear_trajectory(complex_count, (0.3, 5), (0, -0.5)),
        },
        config,
        dropout_frames={"A": {9}, "B": {10, 11}, "C": {8, 9}},
        noise_amplitude=config.noise_amplitude,
        shuffle_detections=True,
    )
    return scenarios


def iter_long_run_frames(
    frame_count: int,
    config: SimulationConfig | None = None,
) -> Iterator[SimulationFrame]:
    """長時間テスト向けに全フレームを保持せず逐次生成する。"""
    config = config or SimulationConfig()
    rng = random.Random(config.random_seed)
    labels = [chr(ord("A") + index) for index in range(config.number_of_people)]
    positions = {
        label: [rng.uniform(-8, 8), rng.uniform(-5, 5)] for label in labels
    }
    velocities = {
        label: [rng.uniform(0.08, 0.35), rng.uniform(-0.12, 0.12)]
        for label in labels
    }

    for frame_number in range(frame_count):
        ground_truth: dict[str, Position2D] = {}
        detections: list[SimulatedDetection] = []
        for label in labels:
            position = positions[label]
            velocity = velocities[label]
            for axis, limit in ((0, 10.0), (1, 6.0)):
                position[axis] += velocity[axis]
                if abs(position[axis]) > limit:
                    position[axis] = max(-limit, min(limit, position[axis]))
                    velocity[axis] *= -1.0
            true_position = Position2D(position[0], position[1])
            ground_truth[label] = true_position
            # 短いランダム欠落。seed固定なので再現可能。
            if rng.random() < 0.02:
                continue
            detection = Detection.from_xy(
                true_position.x + rng.uniform(-config.noise_amplitude, config.noise_amplitude),
                true_position.y + rng.uniform(-config.noise_amplitude, config.noise_amplitude),
                timestamp=float(frame_number),
                metadata={"ground_truth_person_id": label},
            )
            detections.append(SimulatedDetection(label, true_position, detection))
        rng.shuffle(detections)
        yield SimulationFrame(frame_number, ground_truth, tuple(detections))
