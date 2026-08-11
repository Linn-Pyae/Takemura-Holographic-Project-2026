"""LiDARなしで主要シナリオを検証し、各フレームの状態を表示するテスト。"""

from __future__ import annotations

from copy import deepcopy
import random
import unittest

from tracking.models import Detection
from tracking.motion import KalmanFilter2D
from tracking.tracker import (
    GreedyNearestNeighborMatcher,
    HungarianMatcher,
    MultiObjectTracker,
    TrackerConfig,
)


def detections(*points: tuple[float, float]) -> list[Detection]:
    return [Detection.from_xy(x, y) for x, y in points]


def print_tracks(frame: int, tracks: tuple) -> None:
    print(f"\nframe={frame}")
    print("ID  name       x      y      missed  history")
    for track in tracks:
        p = track.current_position
        print(f"{track.id:<3} {track.name:<10} {p.x:>5.1f}  {p.y:>5.1f}  "
              f"{track.missed_frames:>6}  {len(track.history):>7}")


def run_frames(frames, *, max_distance=2.0, max_missed=2):
    tracker = MultiObjectTracker(
        TrackerConfig(max_distance, max_missed)
    )
    snapshots = []
    for frame_number, frame in enumerate(frames):
        tracks = tracker.update(detections(*frame))
        print_tracks(frame_number, tracks)
        # Trackは次フレームで更新される可変オブジェクトなので、検証用には固定する。
        snapshots.append(deepcopy(tracks))
    return tracker, snapshots


class MultiObjectTrackerTests(unittest.TestCase):
    def test_one_person_moves_in_straight_line(self):
        tracker, _ = run_frames([[(0, 0)], [(1, 0)], [(2, 0)]])
        self.assertEqual(tracker.tracks[0].id, 1)
        self.assertEqual(tracker.tracks[0].name, "HARRY")
        self.assertEqual(len(tracker.tracks[0].history), 3)

    def test_two_people_move_simultaneously(self):
        tracker, _ = run_frames([
            [(0, 0), (10, 0)], [(1, 0), (9, 0)], [(2, 0), (8, 0)]
        ])
        self.assertEqual([track.id for track in tracker.tracks], [1, 2])
        self.assertEqual([track.name for track in tracker.tracks], ["HARRY", "RON"])

    def test_new_person_enters(self):
        tracker, _ = run_frames([[(0, 0)], [(1, 0)], [(2, 0), (10, 0)]])
        self.assertEqual({track.id for track in tracker.tracks}, {1, 2})

    def test_person_leaves_range_and_is_deleted(self):
        tracker, snapshots = run_frames([[(0, 0)], [], [], []], max_missed=2)
        self.assertEqual(snapshots[1][0].missed_frames, 1)
        self.assertEqual(snapshots[2][0].missed_frames, 2)
        self.assertEqual(tracker.tracks, ())

    def test_short_detection_dropout_keeps_id(self):
        tracker, _ = run_frames([[(0, 0)], [(1, 0)], [], [], [(4, 0)]], max_missed=2)
        self.assertEqual(len(tracker.tracks), 1)
        self.assertEqual(tracker.tracks[0].id, 1)
        self.assertEqual(tracker.tracks[0].missed_frames, 0)
        self.assertEqual(len(tracker.tracks[0].history), 3)

    def test_two_people_approach_and_cross(self):
        # 定速度予測により、交差後も左右へ進むIDを維持する。
        frames = [
            [(-3, 0), (3, 0)],
            [(-2, 0), (2, 0)],
            [(-1, 0), (1, 0)],
            [(0.2, 0), (-0.2, 0)],
            [(1.2, 0), (-1.2, 0)],
        ]
        tracker, _ = run_frames(frames, max_distance=1.6)
        by_id = {track.id: track for track in tracker.tracks}
        self.assertGreater(by_id[1].current_position.x, 0)
        self.assertLess(by_id[2].current_position.x, 0)


def labeled_detection(label: str, x: float, y: float) -> Detection:
    """テスト上の人物ラベルをmetadataへ埋め込んだDetectionを作る。"""
    return Detection.from_xy(x, y, metadata={"ground_truth": label})


def run_labeled_frames(
    frames,
    *,
    max_distance=2.0,
    max_missed=2,
    association_strategy=None,
    motion_model=None,
):
    """正解人物ラベルごとのID変化を検出して表示する。

    tracker本体はground_truthを対応付けに使用しない。対応後のTrackがどの
    Detectionを受け取ったかをテスト側で確認するためだけにmetadataを使う。
    """
    tracker = MultiObjectTracker(
        TrackerConfig(max_distance, max_missed),
        association_strategy=association_strategy,
        motion_model=motion_model,
    )
    initial_ids: dict[str, int] = {}
    switch_events: list[tuple[int, str, int, int]] = []

    for frame_number, frame in enumerate(frames):
        detections_for_frame = [
            labeled_detection(label, x, y) for label, x, y in frame
        ]
        tracks = tracker.update(detections_for_frame)
        print_tracks(frame_number, tracks)

        # missed中のTrackは新しい観測を受け取っていないため判定対象外。
        observed_tracks = [track for track in tracks if track.missed_frames == 0]
        for track in observed_tracks:
            label = track.metadata.get("ground_truth")
            if label is None:
                continue
            if label not in initial_ids:
                initial_ids[label] = track.id
            elif track.id != initial_ids[label]:
                event = (frame_number, label, initial_ids[label], track.id)
                if event not in switch_events:
                    switch_events.append(event)

    if switch_events:
        print("ID switch: DETECTED")
        for frame_number, label, expected_id, actual_id in switch_events:
            print(
                f"  frame={frame_number} person={label} "
                f"initial_id={expected_id} actual_id={actual_id}"
            )
    else:
        print("ID switch: NONE")
    return tracker, switch_events


class TrackerStressTests(unittest.TestCase):
    """現行アルゴリズムを変更せず、安定範囲と既知の限界を記録する。"""

    def assertNoIdSwitch(self, events) -> None:
        self.assertEqual(events, [], f"unexpected ID switch: {events}")

    def assertIdSwitchDetected(self, events) -> None:
        self.assertTrue(events, "expected an ID switch, but none was detected")

    def test_detection_order_shuffle(self):
        """十分離れた3人では、Detection順を毎フレーム変えてもIDを維持する。"""
        rng = random.Random(20260811)
        frames = []
        for step in range(8):
            frame = [
                ("A", -5.0 + 0.6 * step, -3.0),
                ("B", 5.0 - 0.5 * step, 3.0),
                ("C", 4.0, -4.0 + 0.4 * step),
            ]
            rng.shuffle(frame)
            frames.append(frame)
        _, events = run_labeled_frames(frames, max_distance=1.2)
        self.assertNoIdSwitch(events)

    def test_random_position_noise_with_fixed_seed(self):
        """±0.1〜0.3m相当の再現可能なノイズ下でIDを維持する。"""
        rng = random.Random(314159)

        def signed_noise() -> float:
            return rng.choice((-1.0, 1.0)) * rng.uniform(0.1, 0.3)

        frames = []
        for step in range(10):
            frame = []
            for label, base_x, base_y, vx in [
                ("A", -5.0, -2.0, 0.55),
                ("B", 5.0, 2.0, -0.50),
                ("C", -3.0, 4.0, 0.35),
            ]:
                # 各軸へ絶対値0.1〜0.3m、符号ランダムのノイズを加える。
                x = base_x + vx * step + signed_noise()
                y = base_y + signed_noise()
                frame.append((label, x, y))
            rng.shuffle(frame)
            frames.append(frame)
        _, events = run_labeled_frames(frames, max_distance=1.5)
        self.assertNoIdSwitch(events)

    def test_moving_person_passes_near_stationary_person(self):
        """停止人物の0.25m横を別人物が通過するケース。"""
        frames = [
            [("STILL", 0.0, 0.0), ("MOVE", -2.0, 0.25)],
            [("MOVE", -1.0, 0.25), ("STILL", 0.0, 0.0)],
            [("STILL", 0.0, 0.0), ("MOVE", -0.2, 0.25)],
            [("MOVE", 0.6, 0.25), ("STILL", 0.0, 0.0)],
            [("STILL", 0.0, 0.0), ("MOVE", 1.4, 0.25)],
        ]
        _, events = run_labeled_frames(frames, max_distance=1.2)
        self.assertNoIdSwitch(events)

    def test_two_people_approach_almost_same_coordinate(self):
        """ほぼ同一点ではDetection順の曖昧さによりID switchが起きる。"""
        frames = [
            [("A", -1.0, 0.0), ("B", 1.0, 0.0)],
            [("A", -0.5, 0.0), ("B", 0.5, 0.0)],
            # 予測位置が共に0付近となるフレームで順序をB, Aにする。
            [("B", 0.02, 0.0), ("A", -0.02, 0.0)],
            [("B", -0.5, 0.0), ("A", 0.5, 0.0)],
        ]
        _, events = run_labeled_frames(frames, max_distance=1.0)
        self.assertIdSwitchDetected(events)

    def test_sudden_direction_change_near_another_person(self):
        """近接中の急反転では定速度予測が外れ、ID switchが起きる。"""
        frames = [
            [("TURN", -2.0, 0.0), ("STILL", 1.0, 0.0)],
            [("TURN", -1.0, 0.0), ("STILL", 1.0, 0.0)],
            [("TURN", 0.0, 0.0), ("STILL", 1.0, 0.0)],
            # TURNが急に反転。TURNの予測位置はSTILLの位置と重なる。
            [("TURN", -1.0, 0.0), ("STILL", 1.0, 0.0)],
            [("TURN", -2.0, 0.0), ("STILL", 1.0, 0.0)],
        ]
        _, events = run_labeled_frames(frames, max_distance=2.1)
        self.assertIdSwitchDetected(events)

    def test_four_people_cross_simultaneously(self):
        """4方向から同一点へ入る場合、位置情報だけでは人物を区別できない。"""
        frames = [
            [("A", -2.0, 0.0), ("B", 2.0, 0.0),
             ("C", 0.0, -2.0), ("D", 0.0, 2.0)],
            [("A", -1.0, 0.0), ("B", 1.0, 0.0),
             ("C", 0.0, -1.0), ("D", 0.0, 1.0)],
            # 全員が同一点。入力順を変えて曖昧性を明示する。
            [("C", 0.0, 0.0), ("D", 0.0, 0.0),
             ("A", 0.0, 0.0), ("B", 0.0, 0.0)],
            [("A", 1.0, 0.0), ("B", -1.0, 0.0),
             ("C", 0.0, 1.0), ("D", 0.0, -1.0)],
        ]
        _, events = run_labeled_frames(frames, max_distance=1.5)
        self.assertIdSwitchDetected(events)

    def test_reappearance_after_exceeding_max_missed_frames(self):
        """Track削除後の再登場は新規IDとなることを確認する。"""
        frames = [
            [("A", 0.0, 0.0)],
            [("A", 0.5, 0.0)],
            [],
            [],
            [],  # max_missed_frames=2を超え、旧Trackが削除される。
            [("A", 2.0, 0.0)],
        ]
        tracker, events = run_labeled_frames(
            frames, max_distance=2.0, max_missed=2
        )
        self.assertIdSwitchDetected(events)
        self.assertEqual(len(tracker.tracks), 1)
        self.assertEqual(tracker.tracks[0].id, 2)


class AdvancedTrackingTests(unittest.TestCase):
    """Hungarian AlgorithmとKalman Filterの改善効果・限界を比較する。"""

    def test_hungarian_avoids_greedy_local_minimum(self):
        # Greedyは最短のA-track→B-detectionを先に確定し、全体では不利になる。
        frames = [
            [("A", 0.0, 0.0), ("B", 3.0, 0.0)],
            [("A", -2.0, 0.0), ("B", 1.0, 0.0)],
        ]
        _, greedy_events = run_labeled_frames(
            frames,
            max_distance=6.0,
            association_strategy=GreedyNearestNeighborMatcher(
                use_velocity_prediction=False
            ),
        )
        _, hungarian_events = run_labeled_frames(
            frames,
            max_distance=6.0,
            association_strategy=HungarianMatcher(use_velocity_prediction=False),
        )
        print(
            "comparison: greedy_switches="
            f"{len(greedy_events)} hungarian_switches={len(hungarian_events)}"
        )
        self.assertTrue(greedy_events)
        self.assertEqual(hungarian_events, [])

    def test_kalman_reacquires_moving_person_after_dropout(self):
        # 最終フレームのx=4まで、人物Aは毎フレーム1m進んでいる想定。
        frames = [
            [("A", 0.0, 0.0)],
            [("A", 1.0, 0.0)],
            [],
            [],
            [("A", 4.0, 0.0)],
        ]
        baseline_tracker, baseline_events = run_labeled_frames(
            frames,
            max_distance=1.5,
            max_missed=3,
            association_strategy=HungarianMatcher(),
        )
        kalman_tracker, kalman_events = run_labeled_frames(
            frames,
            max_distance=1.5,
            max_missed=3,
            association_strategy=HungarianMatcher(),
            motion_model=KalmanFilter2D(
                process_variance=0.1,
                measurement_variance=0.05,
                initial_velocity_variance=100.0,
            ),
        )
        print(
            "comparison: without_kalman_switches="
            f"{len(baseline_events)} with_kalman_switches={len(kalman_events)}"
        )
        self.assertTrue(baseline_events)
        self.assertGreater(len(baseline_tracker.tracks), 1)
        self.assertEqual(kalman_events, [])
        self.assertEqual(len(kalman_tracker.tracks), 1)
        self.assertEqual(kalman_tracker.tracks[0].id, 1)

    def test_exact_overlap_remains_ambiguous(self):
        # Hungarian+Kalmanでも同一座標・同一コストのDetectionは識別不能。
        frames = [
            [("A", -1.0, 0.0), ("B", 1.0, 0.0)],
            [("A", -0.5, 0.0), ("B", 0.5, 0.0)],
            [("B", 0.0, 0.0), ("A", 0.0, 0.0)],
        ]
        _, events = run_labeled_frames(
            frames,
            max_distance=1.0,
            association_strategy=HungarianMatcher(),
            motion_model=KalmanFilter2D(),
        )
        self.assertTrue(events)


if __name__ == "__main__":
    unittest.main(verbosity=2)
