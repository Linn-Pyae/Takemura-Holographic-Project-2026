"""シナリオ生成・評価器の回帰テスト。追跡精度の良否自体は合否にしない。"""

from __future__ import annotations

import unittest

from .evaluation import (
    GREEDY_METHOD,
    HUNGARIAN_KALMAN_METHOD,
    evaluate_scenario,
)
from .simulator import SimulationConfig, create_standard_scenarios
from .parameter_sweep import (
    DEFAULT_MINIMUM_DISTANCES,
    DEFAULT_NOISE_AMPLITUDES,
    run_parameter_sweep,
)
from .statistical_sweep import run_statistical_sweep


EXPECTED_SCENARIOS = {
    "two_person_overlap",
    "three_person_crossing",
    "four_person_crossing",
    "noise_0_1m",
    "noise_0_2m",
    "noise_0_3m",
    "sudden_direction_change",
    "stationary_near_pass",
    "short_dropout_1_2_frames",
    "dropout_near_max_missed",
    "dropout_over_max_missed",
    "detection_order_shuffle",
    "new_person_enters",
    "multiple_people_exit",
    "four_people_different_speeds",
    "sudden_acceleration_deceleration",
    "combined_noise_crossing_dropout",
}


class SimulationEvaluationTests(unittest.TestCase):
    def setUp(self) -> None:
        self.config = SimulationConfig(random_seed=20260811)
        self.scenarios = create_standard_scenarios(self.config)

    def test_all_required_scenarios_exist(self):
        self.assertEqual(set(self.scenarios), EXPECTED_SCENARIOS)

    def test_fixed_seed_is_reproducible(self):
        second = create_standard_scenarios(self.config)
        for name in self.scenarios:
            first_points = [
                (item.ground_truth_person_id,
                 item.detection.position.x,
                 item.detection.position.y)
                for frame in self.scenarios[name].frames
                for item in frame.detections
            ]
            second_points = [
                (item.ground_truth_person_id,
                 item.detection.position.x,
                 item.detection.position.y)
                for frame in second[name].frames
                for item in frame.detections
            ]
            self.assertEqual(first_points, second_points, name)

    def test_every_scenario_runs_with_both_methods(self):
        for name, scenario in self.scenarios.items():
            expected_records = sum(len(frame.ground_truth) for frame in scenario.frames)
            for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
                with self.subTest(scenario=name, method=method):
                    result = evaluate_scenario(scenario, method, self.config)
                    self.assertEqual(result.total_frames, len(scenario.frames))
                    self.assertEqual(len(result.records), expected_records)
                    self.assertGreaterEqual(result.new_track_count, 1)
                    self.assertGreaterEqual(result.id_switch_count, 0)
                    self.assertGreaterEqual(result.average_processing_time_ms, 0.0)
                    self.assertLessEqual(result.tracking_success_rate, 100.0)

    def test_short_dropout_recovery_is_counted(self):
        scenario = self.scenarios["short_dropout_1_2_frames"]
        for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
            result = evaluate_scenario(scenario, method, self.config)
            self.assertEqual(result.successful_id_recoveries, 2)
            self.assertEqual(result.lost_track_count, 0)

    def test_over_max_dropout_is_counted_as_lost_and_new_track(self):
        scenario = self.scenarios["dropout_over_max_missed"]
        for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD):
            result = evaluate_scenario(scenario, method, self.config)
            self.assertGreaterEqual(result.lost_track_count, 1)
            self.assertGreaterEqual(result.new_track_count, 2)
            self.assertGreaterEqual(result.id_switch_count, 1)

    def test_noise_position_error_increases_by_amplitude(self):
        errors = []
        for name in ("noise_0_1m", "noise_0_2m", "noise_0_3m"):
            result = evaluate_scenario(
                self.scenarios[name], GREEDY_METHOD, self.config
            )
            errors.append(result.mean_position_error)
        self.assertLess(errors[0], errors[1])
        self.assertLess(errors[1], errors[2])

    def test_assignment_record_contains_ground_truth_and_tracker_fields(self):
        result = evaluate_scenario(
            self.scenarios["new_person_enters"], GREEDY_METHOD, self.config
        )
        record = result.records[0]
        self.assertEqual(record.ground_truth_person_id, "A")
        self.assertIsNotNone(record.detection_x)
        self.assertIsNotNone(record.assigned_track_id)
        self.assertIsNotNone(record.assigned_track_name)

    def test_proximity_noise_parameter_sweep_has_all_48_conditions(self):
        results = run_parameter_sweep(self.config)
        self.assertEqual(len(results), 6 * 4 * 2)
        combinations = {
            (result.minimum_distance_m, result.noise_amplitude_m, result.method)
            for result in results
        }
        expected = {
            (distance, noise, method)
            for distance in DEFAULT_MINIMUM_DISTANCES
            for noise in DEFAULT_NOISE_AMPLITUDES
            for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD)
        }
        self.assertEqual(combinations, expected)
        for result in results:
            self.assertAlmostEqual(
                result.measured_minimum_distance_m,
                result.minimum_distance_m,
                places=12,
            )
            self.assertEqual(
                result.unstable,
                result.id_switch_count > 0
                or result.tracking_success_rate < 100.0,
            )

    def test_multiseed_sweep_is_reproducible_and_uses_same_seeds(self):
        seeds = [101, 102, 103]
        kwargs = {
            "seeds": seeds,
            "minimum_distances": [1.0, 0.0],
            "noise_amplitudes": [0.0, 0.2],
        }
        trials, summaries = run_statistical_sweep(self.config, **kwargs)
        repeated_trials, repeated_summaries = run_statistical_sweep(
            self.config, **kwargs
        )
        self.assertEqual(trials, repeated_trials)
        self.assertEqual(summaries, repeated_summaries)
        self.assertEqual(len(trials), 2 * 2 * 3 * 2)
        self.assertEqual(len(summaries), 2 * 2 * 2)
        for distance in (1.0, 0.0):
            for noise in (0.0, 0.2):
                method_seeds = {
                    method: {
                        trial.random_seed
                        for trial in trials
                        if trial.minimum_distance_m == distance
                        and trial.noise_amplitude_m == noise
                        and trial.method == method
                    }
                    for method in (GREEDY_METHOD, HUNGARIAN_KALMAN_METHOD)
                }
                self.assertEqual(method_seeds[GREEDY_METHOD], set(seeds))
                self.assertEqual(
                    method_seeds[GREEDY_METHOD],
                    method_seeds[HUNGARIAN_KALMAN_METHOD],
                )
        for summary in summaries:
            self.assertEqual(summary.seed_count, 3)
            self.assertAlmostEqual(
                summary.failure_rate,
                summary.failure_trial_count / 3,
            )


if __name__ == "__main__":
    unittest.main(verbosity=2)
