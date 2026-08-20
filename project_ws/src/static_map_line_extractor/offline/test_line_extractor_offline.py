#!/usr/bin/env python3
"""ROS-free verification of the OpenCV contour-to-line algorithm."""

from __future__ import annotations

import math
import unittest
from dataclasses import dataclass

try:
    import cv2
    import numpy as np
except ImportError as error:  # pragma: no cover
    raise SystemExit(
        "Install offline dependencies with:\n"
        "  python -m pip install -r offline/requirements.txt"
    ) from error


@dataclass(frozen=True)
class Parameters:
    occupied_threshold: int = 100
    min_contour_area: float = 0.0
    contour_epsilon: float = 0.05
    minimum_line_length: float = 0.20
    maximum_line_gap: float = 0.0
    minimum_component_cells: int = 3


@dataclass(frozen=True)
class Geometry:
    resolution: float
    origin_x: float = 0.0
    origin_y: float = 0.0
    origin_yaw: float = 0.0


def grid_to_world(cell: tuple[int, int], geometry: Geometry) -> tuple[float, float]:
    column, row = cell
    local_x = (column + 0.5) * geometry.resolution
    local_y = (row + 0.5) * geometry.resolution
    cosine = math.cos(geometry.origin_yaw)
    sine = math.sin(geometry.origin_yaw)
    return (
        geometry.origin_x + cosine * local_x - sine * local_y,
        geometry.origin_y + sine * local_x + cosine * local_y,
    )


def extract_lines(
    occupancy: np.ndarray,
    geometry: Geometry,
    parameters: Parameters = Parameters(),
) -> tuple[list[np.ndarray], list[tuple[tuple[float, float], tuple[float, float]]]]:
    """Mirror the C++ core algorithm for offline unit verification."""
    binary = np.where(occupancy >= parameters.occupied_threshold, 255, 0).astype(
        np.uint8
    )

    if parameters.maximum_line_gap > 0.0:
        radius = max(
            1,
            math.ceil(
                parameters.maximum_line_gap / (2.0 * geometry.resolution)
            ),
        )
        kernel_size = 2 * radius + 1
        kernel = cv2.getStructuringElement(
            cv2.MORPH_RECT, (kernel_size, kernel_size)
        )
        binary = cv2.morphologyEx(binary, cv2.MORPH_CLOSE, kernel)

    if parameters.minimum_component_cells > 1:
        count, labels, statistics, _ = cv2.connectedComponentsWithStats(
            binary, connectivity=8, ltype=cv2.CV_32S
        )
        cleaned = np.zeros_like(binary)
        for label in range(1, count):
            if statistics[label, cv2.CC_STAT_AREA] >= parameters.minimum_component_cells:
                cleaned[labels == label] = 255
        binary = cleaned

    contours, _ = cv2.findContours(
        binary.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE
    )
    simplified: list[np.ndarray] = []
    lines: list[tuple[tuple[float, float], tuple[float, float]]] = []
    epsilon_pixels = parameters.contour_epsilon / geometry.resolution

    for contour in contours:
        area = abs(cv2.contourArea(contour)) * geometry.resolution**2
        if area < parameters.min_contour_area:
            continue
        approximation = cv2.approxPolyDP(contour, epsilon_pixels, True).reshape(-1, 2)
        if len(approximation) < 2:
            continue
        simplified.append(approximation)

        pairs = [(0, 1)] if len(approximation) == 2 else [
            (index, (index + 1) % len(approximation))
            for index in range(len(approximation))
        ]
        for start_index, end_index in pairs:
            start = grid_to_world(tuple(approximation[start_index]), geometry)
            end = grid_to_world(tuple(approximation[end_index]), geometry)
            if math.dist(start, end) >= parameters.minimum_line_length:
                lines.append((start, end))

    return simplified, lines


class OfflineLineExtractorTests(unittest.TestCase):
    def test_long_wall(self) -> None:
        grid = np.zeros((60, 60), dtype=np.int8)
        grid[30:32, 5:55] = 100
        _, lines = extract_lines(grid, Geometry(0.10, -3.0, -3.0))
        self.assertTrue(lines)
        self.assertGreaterEqual(max(math.dist(*line) for line in lines), 4.5)

    def test_square_obstacle(self) -> None:
        grid = np.zeros((40, 40), dtype=np.int8)
        grid[10:20, 10:20] = 100
        contours, lines = extract_lines(grid, Geometry(0.10, -2.0, -2.0))
        self.assertEqual(len(contours), 1)
        self.assertEqual(len(lines), 4)

    def test_isolated_noise_is_removed(self) -> None:
        grid = np.zeros((30, 30), dtype=np.int8)
        grid[2, 2] = grid[15, 10] = grid[20, 25] = 100
        contours, lines = extract_lines(grid, Geometry(0.10))
        self.assertEqual(contours, [])
        self.assertEqual(lines, [])

    def test_multiple_independent_objects(self) -> None:
        grid = np.zeros((50, 50), dtype=np.int8)
        grid[5:12, 5:15] = 100
        grid[30:40, 32:42] = 100
        contours, lines = extract_lines(grid, Geometry(0.10, -2.5, -2.5))
        self.assertEqual(len(contours), 2)
        self.assertEqual(len(lines), 8)

    def test_origin_resolution_and_yaw(self) -> None:
        world = grid_to_world(
            (1, 2), Geometry(0.50, 10.0, 20.0, math.pi / 2.0)
        )
        self.assertAlmostEqual(world[0], 8.75)
        self.assertAlmostEqual(world[1], 20.75)


if __name__ == "__main__":
    unittest.main(verbosity=2)
