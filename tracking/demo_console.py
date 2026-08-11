"""外部ライブラリ不要で追跡動作を確認するコンソールデモ。"""

from __future__ import annotations

from tracking.models import Detection
from tracking.tracker import MultiObjectTracker, TrackerConfig


# 2人がすれ違い、途中からLUNA役ではなく3番目（HERMIONE）が参加する。
# frame 5では3番目のDetectionを意図的に欠落させている。
FRAMES = [
    [(-4.0, -1.0), (4.0, 1.0)],
    [(-3.0, -0.8), (3.0, 0.8)],
    [(-2.0, -0.5), (2.0, 0.5)],
    [(-1.0, -0.2), (1.0, 0.2), (0.0, 4.0)],
    [(0.2, 0.0), (-0.2, 0.0), (0.0, 3.0)],
    [(1.2, 0.2), (-1.2, -0.2)],
    [(2.2, 0.5), (-2.2, -0.5), (0.0, 1.0)],
]


def main() -> None:
    tracker = MultiObjectTracker(
        TrackerConfig(max_association_distance=1.7, max_missed_frames=2)
    )

    for frame_number, points in enumerate(FRAMES):
        frame = [Detection.from_xy(x, y) for x, y in points]
        tracks = tracker.update(frame)

        print(f"\n=== Frame {frame_number} ===")
        print("ID  NAME        X      Y   MISSED  HISTORY")
        for track in tracks:
            position = track.current_position
            print(
                f"{track.id:<3} {track.name:<10} "
                f"{position.x:>5.1f}  {position.y:>5.1f}  "
                f"{track.missed_frames:>6}  {len(track.history):>7}"
            )


if __name__ == "__main__":
    main()
