"""Hungarian AlgorithmとKalman Filterを組み合わせたコンソールデモ。"""

from __future__ import annotations

from tracking.models import Detection
from tracking.motion import KalmanFilter2D
from tracking.tracker import HungarianMatcher, MultiObjectTracker, TrackerConfig


# AとBがすれ違い、Cはframe 3, 4で一時的に欠落する。
FRAMES = [
    [("A", -3.0, -0.3), ("B", 3.0, 0.3), ("C", 0.0, 4.0)],
    [("B", 2.0, 0.2), ("C", 0.0, 3.0), ("A", -2.0, -0.2)],
    [("C", 0.0, 2.0), ("A", -0.8, -0.1), ("B", 0.8, 0.1)],
    [("B", -0.5, -0.1), ("A", 0.5, 0.1)],
    [("A", 1.7, 0.2), ("B", -1.7, -0.2)],
    [("C", 0.0, -1.0), ("B", -2.9, -0.3), ("A", 2.9, 0.3)],
]


def main() -> None:
    tracker = MultiObjectTracker(
        TrackerConfig(max_association_distance=1.6, max_missed_frames=3),
        association_strategy=HungarianMatcher(),
        motion_model=KalmanFilter2D(
            process_variance=0.1,
            measurement_variance=0.05,
        ),
    )

    print("Association: Hungarian Algorithm")
    print("Motion model: Kalman Filter [x, y, vx, vy]")
    for frame_number, observations in enumerate(FRAMES):
        detections = [
            Detection.from_xy(x, y, metadata={"person": label})
            for label, x, y in observations
        ]
        tracks = tracker.update(detections)

        print(f"\n=== Frame {frame_number} ===")
        print("ID  NAME        PERSON      X      Y   MISSED  HISTORY")
        for track in tracks:
            position = track.current_position
            person = track.metadata.get("person", "-")
            print(
                f"{track.id:<3} {track.name:<10} {person:<7} "
                f"{position.x:>6.1f} {position.y:>6.1f} "
                f"{track.missed_frames:>7} {len(track.history):>8}"
            )


if __name__ == "__main__":
    main()
