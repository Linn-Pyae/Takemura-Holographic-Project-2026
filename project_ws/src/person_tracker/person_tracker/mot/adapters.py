"""前段・後段の形式変更を追跡本体から隔離するアダプター例。"""

from __future__ import annotations

from typing import Any, Iterable, Mapping, Protocol, Sequence, TypeVar

from .models import Detection, Track

RawDetection = TypeVar("RawDetection")
TrackOutput = TypeVar("TrackOutput")


class DetectionAdapter(Protocol[RawDetection]):
    def to_detection(self, raw: RawDetection) -> Detection: ...


class TrackAdapter(Protocol[TrackOutput]):
    def from_track(self, track: Track) -> TrackOutput: ...


class MappingDetectionAdapter:
    """仮のMapping入力例。JSONやROS2への依存はここだけに閉じ込める。"""

    def to_detection(self, raw: Mapping[str, Any]) -> Detection:
        known = {"x", "y", "timestamp"}
        return Detection.from_xy(
            raw["x"],
            raw["y"],
            timestamp=raw.get("timestamp"),
            metadata={key: value for key, value in raw.items() if key not in known},
        )

    def convert_frame(self, items: Iterable[Mapping[str, Any]]) -> list[Detection]:
        return [self.to_detection(item) for item in items]


class MappingTrackAdapter:
    """描画側へ渡す仮の辞書出力。必要なら別Adapterへ交換する。"""

    def from_track(self, track: Track) -> dict[str, Any]:
        return {
            "id": track.id,
            "name": track.name,
            "x": track.current_position.x,
            "y": track.current_position.y,
            "previous_position": (
                None
                if track.previous_position is None
                else {
                    "x": track.previous_position.x,
                    "y": track.previous_position.y,
                }
            ),
            "history": [{"x": p.x, "y": p.y} for p in track.history],
            "missed_frames": track.missed_frames,
            "timestamp": track.last_timestamp,
            "metadata": dict(track.metadata),
        }

    def convert_tracks(self, tracks: Sequence[Track]) -> list[dict[str, Any]]:
        return [self.from_track(track) for track in tracks]
