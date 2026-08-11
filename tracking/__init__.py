"""入出力形式に依存しない、軽量なMulti-Object Trackingパッケージ。"""

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
    "GreedyNearestNeighborMatcher",
    "HungarianMatcher",
    "KalmanFilter2D",
    "MotionModel",
    "MultiObjectTracker",
    "TrackerConfig",
]
