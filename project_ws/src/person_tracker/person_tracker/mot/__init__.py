"""ROS-free multi-object tracking core (shipped inside person_tracker)."""

from .adapters import MappingDetectionAdapter, MappingTrackAdapter
from .models import Detection, Position2D, Track
from .motion import KalmanFilter2D, MotionModel
from .tracker import (
    GreedyNearestNeighborMatcher,
    HungarianMatcher,
    MultiObjectTracker,
    TrackerConfig,
)

__all__ = [
    "Detection",
    "Position2D",
    "Track",
    "MappingDetectionAdapter",
    "MappingTrackAdapter",
    "GreedyNearestNeighborMatcher",
    "HungarianMatcher",
    "KalmanFilter2D",
    "MotionModel",
    "MultiObjectTracker",
    "TrackerConfig",
]
