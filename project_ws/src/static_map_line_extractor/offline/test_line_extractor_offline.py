#!/usr/bin/env python3
"""ROS-free verification of coarse wall/block shape extraction."""

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
    minimum_component_cells: int = 8
    wall_min_length: float = 1.20
    wall_min_aspect_ratio: float = 3.50
    minimum_block_size: float = 0.40
    wall_merge_angle_degrees: float = 12.0
    wall_merge_distance: float = 0.30
    wall_merge_gap: float = 0.50


@dataclass(frozen=True)
class Geometry:
    resolution: float
    origin_x: float = 0.0
    origin_y: float = 0.0
    origin_yaw: float = 0.0


Wall = tuple[tuple[float, float], tuple[float, float]]
Block = tuple[
    tuple[float, float],
    tuple[float, float],
    tuple[float, float],
    tuple[float, float],
]


@dataclass(frozen=True)
class ShapeExtraction:
    contours: list[np.ndarray]
    wall_lines: list[Wall]
    blocks: list[Block]
    lines: list[Wall]


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


def _normalise(vector: np.ndarray) -> np.ndarray:
    length = float(np.linalg.norm(vector))
    if length <= np.finfo(float).eps:
        return np.array([1.0, 0.0], dtype=float)
    return vector / length


def _merge_walls(walls: list[Wall], parameters: Parameters) -> list[Wall]:
    merged = list(walls)
    maximum_angle = math.radians(parameters.wall_merge_angle_degrees)
    changed = True
    while changed:
        changed = False
        for first_index in range(len(merged)):
            if changed:
                break
            for second_index in range(first_index + 1, len(merged)):
                first = np.asarray(merged[first_index], dtype=float)
                second = np.asarray(merged[second_index], dtype=float)
                first_direction = _normalise(first[1] - first[0])
                second_direction = _normalise(second[1] - second[0])
                alignment = float(np.dot(first_direction, second_direction))
                if alignment < 0.0:
                    second_direction *= -1.0
                    alignment *= -1.0
                alignment = max(-1.0, min(1.0, alignment))
                if math.acos(alignment) > maximum_angle:
                    continue

                axis = _normalise(first_direction + second_direction)
                normal = np.array([-axis[1], axis[0]])
                first_center = first.mean(axis=0)
                second_center = second.mean(axis=0)
                perpendicular_distance = abs(
                    float(np.dot(second_center - first_center, normal))
                )
                if perpendicular_distance > parameters.wall_merge_distance:
                    continue

                first_projection = sorted(float(np.dot(point, axis)) for point in first)
                second_projection = sorted(
                    float(np.dot(point, axis)) for point in second
                )
                gap = max(
                    0.0,
                    second_projection[0] - first_projection[1],
                    first_projection[0] - second_projection[1],
                )
                if gap > parameters.wall_merge_gap:
                    continue

                minimum = min(first_projection[0], second_projection[0])
                maximum = max(first_projection[1], second_projection[1])
                perpendicular = (
                    float(np.dot(first_center, normal))
                    + float(np.dot(second_center, normal))
                ) * 0.5
                start = axis * minimum + normal * perpendicular
                end = axis * maximum + normal * perpendicular
                merged[first_index] = (tuple(start), tuple(end))
                del merged[second_index]
                changed = True
                break
    return merged


def extract_shapes(
    occupancy: np.ndarray,
    geometry: Geometry,
    parameters: Parameters = Parameters(),
) -> ShapeExtraction:
    """Mirror the C++ coarse-shape algorithm for offline verification."""
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
    epsilon_pixels = parameters.contour_epsilon / geometry.resolution

    for contour in contours:
        area = abs(cv2.contourArea(contour)) * geometry.resolution**2
        if area < parameters.min_contour_area:
            continue
        approximation = cv2.approxPolyDP(contour, epsilon_pixels, True).reshape(-1, 2)
        if len(approximation) < 2:
            continue
        simplified.append(approximation)

    component_count, labels, statistics, _ = cv2.connectedComponentsWithStats(
        binary, connectivity=8, ltype=cv2.CV_32S
    )
    walls: list[Wall] = []
    blocks: list[Block] = []
    minimum_wall_length = max(
        parameters.wall_min_length, parameters.minimum_line_length
    )
    for label in range(1, component_count):
        cell_count = int(statistics[label, cv2.CC_STAT_AREA])
        area = cell_count * geometry.resolution**2
        if area < parameters.min_contour_area:
            continue
        rows, columns = np.nonzero(labels == label)
        points = np.column_stack((columns, rows)).astype(np.float32)
        if len(points) == 0:
            continue
        center, size, angle_degrees = cv2.minAreaRect(points)
        width_pixels, height_pixels = size
        angle = math.radians(angle_degrees)
        width_axis = np.array([math.cos(angle), math.sin(angle)])
        height_axis = np.array([-math.sin(angle), math.cos(angle)])
        width_is_long = width_pixels >= height_pixels
        major_axis = width_axis if width_is_long else height_axis
        minor_axis = height_axis if width_is_long else width_axis
        long_pixels = max(width_pixels, height_pixels) + 1.0
        short_pixels = min(width_pixels, height_pixels) + 1.0
        long_metres = long_pixels * geometry.resolution
        short_metres = short_pixels * geometry.resolution
        aspect_ratio = long_metres / max(short_metres, 1.0e-9)
        center_point = np.asarray(center, dtype=float)

        if (
            long_metres >= minimum_wall_length
            and aspect_ratio >= parameters.wall_min_aspect_ratio
        ):
            half = major_axis * long_pixels * 0.5
            walls.append(
                (
                    grid_to_world(tuple(center_point - half), geometry),
                    grid_to_world(tuple(center_point + half), geometry),
                )
            )
            continue

        block_long_pixels = (
            max(long_metres, parameters.minimum_block_size) / geometry.resolution
        )
        block_short_pixels = (
            max(short_metres, parameters.minimum_block_size) / geometry.resolution
        )
        major_half = major_axis * block_long_pixels * 0.5
        minor_half = minor_axis * block_short_pixels * 0.5
        blocks.append(
            tuple(
                grid_to_world(tuple(point), geometry)
                for point in (
                    center_point - major_half - minor_half,
                    center_point + major_half - minor_half,
                    center_point + major_half + minor_half,
                    center_point - major_half + minor_half,
                )
            )
        )

    walls = _merge_walls(walls, parameters)
    lines = list(walls)
    for block in blocks:
        lines.extend(
            (block[index], block[(index + 1) % len(block)])
            for index in range(len(block))
        )
    return ShapeExtraction(simplified, walls, blocks, lines)


def extract_lines(
    occupancy: np.ndarray,
    geometry: Geometry,
    parameters: Parameters = Parameters(),
) -> tuple[list[np.ndarray], list[Wall]]:
    """Compatibility wrapper returning flattened wall and rectangle edges."""
    result = extract_shapes(occupancy, geometry, parameters)
    return result.contours, result.lines


class OfflineLineExtractorTests(unittest.TestCase):
    def test_long_wall(self) -> None:
        grid = np.zeros((60, 60), dtype=np.int8)
        grid[30:32, 5:55] = 100
        result = extract_shapes(grid, Geometry(0.10, -3.0, -3.0))
        self.assertEqual(len(result.wall_lines), 1)
        self.assertEqual(result.blocks, [])
        self.assertGreaterEqual(math.dist(*result.wall_lines[0]), 4.5)

    def test_square_obstacle(self) -> None:
        grid = np.zeros((40, 40), dtype=np.int8)
        grid[10:20, 10:20] = 100
        result = extract_shapes(grid, Geometry(0.10, -2.0, -2.0))
        self.assertEqual(len(result.contours), 1)
        self.assertEqual(result.wall_lines, [])
        self.assertEqual(len(result.blocks), 1)
        self.assertEqual(len(result.lines), 4)

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

    def test_collinear_wall_fragments_are_merged(self) -> None:
        grid = np.zeros((30, 60), dtype=np.int8)
        grid[14:16, 5:25] = 100
        grid[14:16, 28:50] = 100
        result = extract_shapes(
            grid,
            Geometry(0.10),
            Parameters(maximum_line_gap=0.0, wall_merge_gap=0.50),
        )
        self.assertEqual(len(result.wall_lines), 1)
        self.assertGreater(math.dist(*result.wall_lines[0]), 4.0)

    def test_origin_resolution_and_yaw(self) -> None:
        world = grid_to_world(
            (1, 2), Geometry(0.50, 10.0, 20.0, math.pi / 2.0)
        )
        self.assertAlmostEqual(world[0], 8.75)
        self.assertAlmostEqual(world[1], 20.75)


if __name__ == "__main__":
    unittest.main(verbosity=2)
