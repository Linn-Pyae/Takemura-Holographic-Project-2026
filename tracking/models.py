"""追跡処理の内部だけで使う、外部フォーマット非依存のデータモデル。"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Mapping, Optional


@dataclass(frozen=True)
class Position2D:
    """XY平面上の位置。将来Position3D等へ置換しやすい独立型。"""

    x: float
    y: float


@dataclass(frozen=True)
class Detection:
    """前段の検出結果を正規化した内部表現。

    metadataにはz、クラスタサイズ、点数、速度など任意情報を保持できる。
    timestampはフレーム全体の時刻と検出個別の時刻の双方に対応できる。
    """

    position: Position2D
    timestamp: Optional[float] = None
    metadata: Mapping[str, Any] = field(default_factory=dict)

    @classmethod
    def from_xy(
        cls,
        x: float,
        y: float,
        *,
        timestamp: Optional[float] = None,
        metadata: Optional[Mapping[str, Any]] = None,
    ) -> "Detection":
        return cls(Position2D(float(x), float(y)), timestamp, metadata or {})


@dataclass
class Track:
    """同一物体についてフレームをまたいで保持する状態。"""

    id: int
    name: str
    current_position: Position2D
    previous_position: Optional[Position2D] = None
    history: list[Position2D] = field(default_factory=list)
    missed_frames: int = 0
    last_timestamp: Optional[float] = None
    metadata: dict[str, Any] = field(default_factory=dict)

    def __post_init__(self) -> None:
        if not self.history:
            self.history.append(self.current_position)

    def update(self, detection: Detection) -> None:
        """Detectionとの対応が取れたときだけ位置と履歴を更新する。"""
        self.previous_position = self.current_position
        self.current_position = detection.position
        self.history.append(detection.position)
        self.missed_frames = 0
        self.last_timestamp = detection.timestamp
        self.metadata = dict(detection.metadata)

    def mark_missed(self) -> None:
        self.missed_frames += 1

    def predicted_position(self) -> Position2D:
        """直近の移動量を1フレーム外挿する簡易予測。"""
        if self.previous_position is None:
            return self.current_position
        return Position2D(
            2.0 * self.current_position.x - self.previous_position.x,
            2.0 * self.current_position.y - self.previous_position.y,
        )
