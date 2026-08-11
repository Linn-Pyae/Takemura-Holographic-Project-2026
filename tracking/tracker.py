"""外部I/Oや描画に依存しないMulti-Object Tracking本体。"""

from __future__ import annotations

from dataclasses import dataclass
from math import hypot
from typing import Protocol, Sequence

from .models import Detection, Position2D, Track
from .motion import MotionModel


DEFAULT_NAMES = ("HARRY", "RON", "HERMIONE", "DRACO", "LUNA")


class AssociationStrategy(Protocol):
    """対応付けアルゴリズムの差し替え口（将来のHungarian法向け）。"""

    def associate(
        self,
        tracks: Sequence[Track],
        detections: Sequence[Detection],
        max_distance: float,
        predicted_positions: Sequence[Position2D] | None = None,
    ) -> tuple[list[tuple[int, int]], set[int], set[int]]:
        """(track index, detection index)と未対応index群を返す。"""


@dataclass(frozen=True)
class GreedyNearestNeighborMatcher:
    """距離の短い候補ペアから重複なしで採用する簡易対応付け。"""

    use_velocity_prediction: bool = True

    @staticmethod
    def _distance(a: Position2D, b: Position2D) -> float:
        return hypot(a.x - b.x, a.y - b.y)

    def associate(
        self,
        tracks: Sequence[Track],
        detections: Sequence[Detection],
        max_distance: float,
        predicted_positions: Sequence[Position2D] | None = None,
    ) -> tuple[list[tuple[int, int]], set[int], set[int]]:
        candidates: list[tuple[float, int, int]] = []
        for track_index, track in enumerate(tracks):
            if predicted_positions is not None:
                reference = predicted_positions[track_index]
            else:
                reference = (
                    track.predicted_position()
                    if self.use_velocity_prediction
                    else track.current_position
                )
            for detection_index, detection in enumerate(detections):
                distance = self._distance(reference, detection.position)
                if distance <= max_distance:
                    candidates.append((distance, track_index, detection_index))

        matches: list[tuple[int, int]] = []
        matched_tracks: set[int] = set()
        matched_detections: set[int] = set()
        for _, track_index, detection_index in sorted(candidates):
            if track_index in matched_tracks or detection_index in matched_detections:
                continue
            matches.append((track_index, detection_index))
            matched_tracks.add(track_index)
            matched_detections.add(detection_index)

        return (
            matches,
            set(range(len(tracks))) - matched_tracks,
            set(range(len(detections))) - matched_detections,
        )


@dataclass(frozen=True)
class HungarianMatcher:
    """全体の距離コストを最小化するHungarian Algorithm対応付け。

    距離ゲート外の組み合わせは無効とし、Track/Detectionの未対応を表す
    ダミー行・列を加えた正方コスト行列を解く。
    """

    use_velocity_prediction: bool = True

    @staticmethod
    def _distance(a: Position2D, b: Position2D) -> float:
        return hypot(a.x - b.x, a.y - b.y)

    @staticmethod
    def _solve(cost: Sequence[Sequence[float]]) -> list[int]:
        """Kuhn-Munkres法で各行に割り当てる列indexを返す。"""
        size = len(cost)
        if size == 0:
            return []
        if any(len(row) != size for row in cost):
            raise ValueError("Hungarian cost matrix must be square")

        # 1-indexed potentials implementation。O(n^3)。
        u = [0.0] * (size + 1)
        v = [0.0] * (size + 1)
        p = [0] * (size + 1)
        way = [0] * (size + 1)
        infinity = float("inf")

        for row in range(1, size + 1):
            p[0] = row
            column0 = 0
            minimum = [infinity] * (size + 1)
            used = [False] * (size + 1)
            while True:
                used[column0] = True
                current_row = p[column0]
                delta = infinity
                column1 = 0
                for column in range(1, size + 1):
                    if used[column]:
                        continue
                    reduced = (
                        cost[current_row - 1][column - 1]
                        - u[current_row]
                        - v[column]
                    )
                    if reduced < minimum[column]:
                        minimum[column] = reduced
                        way[column] = column0
                    if minimum[column] < delta:
                        delta = minimum[column]
                        column1 = column
                for column in range(size + 1):
                    if used[column]:
                        u[p[column]] += delta
                        v[column] -= delta
                    else:
                        minimum[column] -= delta
                column0 = column1
                if p[column0] == 0:
                    break
            while True:
                column1 = way[column0]
                p[column0] = p[column1]
                column0 = column1
                if column0 == 0:
                    break

        assignment = [-1] * size
        for column in range(1, size + 1):
            if p[column] != 0:
                assignment[p[column] - 1] = column - 1
        return assignment

    def associate(
        self,
        tracks: Sequence[Track],
        detections: Sequence[Detection],
        max_distance: float,
        predicted_positions: Sequence[Position2D] | None = None,
    ) -> tuple[list[tuple[int, int]], set[int], set[int]]:
        track_count = len(tracks)
        detection_count = len(detections)
        if track_count == 0 or detection_count == 0:
            return (
                [],
                set(range(track_count)),
                set(range(detection_count)),
            )
        if predicted_positions is not None and len(predicted_positions) != track_count:
            raise ValueError("predicted_positions length must equal tracks length")

        distances: list[list[float]] = []
        for track_index, track in enumerate(tracks):
            if predicted_positions is not None:
                reference = predicted_positions[track_index]
            else:
                reference = (
                    track.predicted_position()
                    if self.use_velocity_prediction
                    else track.current_position
                )
            distances.append([
                self._distance(reference, detection.position)
                for detection in detections
            ])

        # 実Trackと実Detectionに加え、双方の未対応を選べるダミー領域を作る。
        size = track_count + detection_count
        unmatched_cost = max_distance + 1.0
        forbidden_cost = unmatched_cost * (size + 1) * 10.0
        cost = [[0.0] * size for _ in range(size)]
        for row in range(track_count):
            for column in range(detection_count):
                distance = distances[row][column]
                cost[row][column] = (
                    distance if distance <= max_distance else forbidden_cost
                )
            for column in range(detection_count, size):
                cost[row][column] = unmatched_cost
        for row in range(track_count, size):
            for column in range(detection_count):
                cost[row][column] = unmatched_cost
            # dummy-to-dummyは0のまま。

        assignment = self._solve(cost)
        matches: list[tuple[int, int]] = []
        matched_tracks: set[int] = set()
        matched_detections: set[int] = set()
        for track_index in range(track_count):
            detection_index = assignment[track_index]
            if (
                0 <= detection_index < detection_count
                and distances[track_index][detection_index] <= max_distance
            ):
                matches.append((track_index, detection_index))
                matched_tracks.add(track_index)
                matched_detections.add(detection_index)
        return (
            matches,
            set(range(track_count)) - matched_tracks,
            set(range(detection_count)) - matched_detections,
        )


@dataclass(frozen=True)
class TrackerConfig:
    max_association_distance: float = 2.0
    max_missed_frames: int = 3

    def __post_init__(self) -> None:
        if self.max_association_distance <= 0:
            raise ValueError("max_association_distance must be positive")
        if self.max_missed_frames < 0:
            raise ValueError("max_missed_frames must be non-negative")


class MultiObjectTracker:
    """Detection列を受け取り、現在有効なTrack列を返す状態ful tracker。"""

    def __init__(
        self,
        config: TrackerConfig | None = None,
        association_strategy: AssociationStrategy | None = None,
        names: Sequence[str] = DEFAULT_NAMES,
        motion_model: MotionModel | None = None,
    ) -> None:
        if not names:
            raise ValueError("names must not be empty")
        self.config = config or TrackerConfig()
        self.association_strategy = association_strategy or GreedyNearestNeighborMatcher()
        self.motion_model = motion_model
        self.names = tuple(names)
        self._tracks: list[Track] = []
        self._next_id = 1

    @property
    def tracks(self) -> tuple[Track, ...]:
        """呼び出し側がTrack一覧そのものを置換しないようtupleで公開する。"""
        return tuple(self._tracks)

    def _new_name(self, track_id: int) -> str:
        index = track_id - 1
        base = self.names[index % len(self.names)]
        cycle = index // len(self.names)
        return base if cycle == 0 else f"{base}_{cycle + 1}"

    def _create_track(self, detection: Detection) -> Track:
        track_id = self._next_id
        self._next_id += 1
        track = Track(
            id=track_id,
            name=self._new_name(track_id),
            current_position=detection.position,
            last_timestamp=detection.timestamp,
            metadata=dict(detection.metadata),
        )
        if self.motion_model is not None:
            self.motion_model.initialize(track)
        return track

    def update(self, detections: Sequence[Detection]) -> tuple[Track, ...]:
        """1フレーム分を処理する。入力順は新規Trackの命名順にのみ影響する。"""
        detections = tuple(detections)
        if not all(isinstance(item, Detection) for item in detections):
            raise TypeError("all items must be Detection instances; use an adapter first")

        predicted_positions = (
            [self.motion_model.predict(track) for track in self._tracks]
            if self.motion_model is not None
            else None
        )
        if predicted_positions is None:
            association_result = self.association_strategy.associate(
                self._tracks,
                detections,
                self.config.max_association_distance,
            )
        else:
            association_result = self.association_strategy.associate(
                self._tracks,
                detections,
                self.config.max_association_distance,
                predicted_positions,
            )
        matches, unmatched_tracks, unmatched_detections = association_result
        for track_index, detection_index in matches:
            if self.motion_model is not None:
                self.motion_model.correct(
                    self._tracks[track_index], detections[detection_index]
                )
            self._tracks[track_index].update(detections[detection_index])
        for track_index in unmatched_tracks:
            self._tracks[track_index].mark_missed()

        removed_ids = {
            track.id
            for track in self._tracks
            if track.missed_frames > self.config.max_missed_frames
        }
        self._tracks = [
            track
            for track in self._tracks
            if track.missed_frames <= self.config.max_missed_frames
        ]
        if self.motion_model is not None:
            for track_id in removed_ids:
                self.motion_model.remove(track_id)
        for detection_index in sorted(unmatched_detections):
            self._tracks.append(self._create_track(detections[detection_index]))

        return self.tracks
