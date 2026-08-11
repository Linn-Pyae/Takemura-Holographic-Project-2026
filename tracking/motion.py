"""Trackの位置予測を追跡本体から分離したモーションモデル。"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol

from .models import Detection, Position2D, Track


class MotionModel(Protocol):
    """Kalman Filter等の予測器を差し替えるためのインターフェース。"""

    def initialize(self, track: Track) -> None: ...

    def predict(self, track: Track) -> Position2D: ...

    def correct(self, track: Track, detection: Detection) -> None: ...

    def remove(self, track_id: int) -> None: ...


@dataclass
class _KalmanState:
    # 状態ベクトルは[x, y, vx, vy]、Pは4x4共分散行列。
    x: list[float]
    p: list[list[float]]


class KalmanFilter2D:
    """XY位置を観測する、定速度モデルの軽量なKalman Filter。

    NumPy等には依存せず、4次元状態に必要な行列演算だけを実装している。
    predict()は1フレームにつき1度呼ばれることを前提とする。
    """

    def __init__(
        self,
        *,
        dt: float = 1.0,
        process_variance: float = 0.1,
        measurement_variance: float = 0.1,
        initial_position_variance: float = 1.0,
        initial_velocity_variance: float = 100.0,
    ) -> None:
        if dt <= 0:
            raise ValueError("dt must be positive")
        for name, value in (
            ("process_variance", process_variance),
            ("measurement_variance", measurement_variance),
            ("initial_position_variance", initial_position_variance),
            ("initial_velocity_variance", initial_velocity_variance),
        ):
            if value <= 0:
                raise ValueError(f"{name} must be positive")
        self.dt = float(dt)
        self.process_variance = float(process_variance)
        self.measurement_variance = float(measurement_variance)
        self.initial_position_variance = float(initial_position_variance)
        self.initial_velocity_variance = float(initial_velocity_variance)
        self._states: dict[int, _KalmanState] = {}

    @staticmethod
    def _transpose(matrix: list[list[float]]) -> list[list[float]]:
        return [list(column) for column in zip(*matrix)]

    @staticmethod
    def _multiply(
        left: list[list[float]], right: list[list[float]]
    ) -> list[list[float]]:
        right_t = KalmanFilter2D._transpose(right)
        return [
            [sum(a * b for a, b in zip(row, column)) for column in right_t]
            for row in left
        ]

    @staticmethod
    def _add(
        left: list[list[float]], right: list[list[float]]
    ) -> list[list[float]]:
        return [
            [a + b for a, b in zip(left_row, right_row)]
            for left_row, right_row in zip(left, right)
        ]

    @staticmethod
    def _identity(size: int) -> list[list[float]]:
        return [
            [1.0 if row == column else 0.0 for column in range(size)]
            for row in range(size)
        ]

    def initialize(self, track: Track) -> None:
        position = track.current_position
        self._states[track.id] = _KalmanState(
            x=[position.x, position.y, 0.0, 0.0],
            p=[
                [self.initial_position_variance, 0.0, 0.0, 0.0],
                [0.0, self.initial_position_variance, 0.0, 0.0],
                [0.0, 0.0, self.initial_velocity_variance, 0.0],
                [0.0, 0.0, 0.0, self.initial_velocity_variance],
            ],
        )

    def predict(self, track: Track) -> Position2D:
        if track.id not in self._states:
            self.initialize(track)
        state = self._states[track.id]
        dt = self.dt
        transition = [
            [1.0, 0.0, dt, 0.0],
            [0.0, 1.0, 0.0, dt],
            [0.0, 0.0, 1.0, 0.0],
            [0.0, 0.0, 0.0, 1.0],
        ]
        dt2 = dt * dt
        dt3 = dt2 * dt
        dt4 = dt2 * dt2
        q = self.process_variance
        process_noise = [
            [q * dt4 / 4.0, 0.0, q * dt3 / 2.0, 0.0],
            [0.0, q * dt4 / 4.0, 0.0, q * dt3 / 2.0],
            [q * dt3 / 2.0, 0.0, q * dt2, 0.0],
            [0.0, q * dt3 / 2.0, 0.0, q * dt2],
        ]

        state.x = [
            state.x[0] + dt * state.x[2],
            state.x[1] + dt * state.x[3],
            state.x[2],
            state.x[3],
        ]
        state.p = self._add(
            self._multiply(
                self._multiply(transition, state.p),
                self._transpose(transition),
            ),
            process_noise,
        )
        return Position2D(state.x[0], state.x[1])

    def correct(self, track: Track, detection: Detection) -> None:
        if track.id not in self._states:
            self.initialize(track)
        state = self._states[track.id]
        measurement = detection.position
        innovation = [measurement.x - state.x[0], measurement.y - state.x[1]]

        # H=[I2 0]なので、SはP左上2x2に観測ノイズを加えたもの。
        s00 = state.p[0][0] + self.measurement_variance
        s01 = state.p[0][1]
        s10 = state.p[1][0]
        s11 = state.p[1][1] + self.measurement_variance
        determinant = s00 * s11 - s01 * s10
        if abs(determinant) < 1e-12:
            raise RuntimeError("Kalman innovation covariance is singular")
        inverse_s = [
            [s11 / determinant, -s01 / determinant],
            [-s10 / determinant, s00 / determinant],
        ]

        # K=P H^T S^-1。Pの先頭2列だけがP H^Tとなる。
        pht = [[row[0], row[1]] for row in state.p]
        kalman_gain = self._multiply(pht, inverse_s)
        for index in range(4):
            state.x[index] += (
                kalman_gain[index][0] * innovation[0]
                + kalman_gain[index][1] * innovation[1]
            )

        observation = [
            [1.0, 0.0, 0.0, 0.0],
            [0.0, 1.0, 0.0, 0.0],
        ]
        kh = self._multiply(kalman_gain, observation)
        i_minus_kh = [
            [identity - value for identity, value in zip(identity_row, kh_row)]
            for identity_row, kh_row in zip(self._identity(4), kh)
        ]
        state.p = self._multiply(i_minus_kh, state.p)

    def remove(self, track_id: int) -> None:
        self._states.pop(track_id, None)
