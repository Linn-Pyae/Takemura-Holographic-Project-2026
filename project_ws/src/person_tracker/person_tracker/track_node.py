#!/usr/bin/env python3
"""Subscribe clustered person centroids and publish tracked poses."""

from __future__ import annotations

import json
import os

import rclpy
from geometry_msgs.msg import Point, Pose, PoseArray, Quaternion
from rclpy.node import Node
from std_msgs.msg import String

from person_tracker.mot import (
    HungarianMatcher,
    KalmanFilter2D,
    MappingDetectionAdapter,
    MappingTrackAdapter,
    MultiObjectTracker,
    TrackerConfig,
)


class PersonTrackNode(Node):
    def __init__(self) -> None:
        super().__init__("person_track_node")

        detection_topic = os.environ.get("DETECTION_TOPIC", "/person_detections")
        track_topic = os.environ.get("TRACK_TOPIC", "/person_tracks")
        track_info_topic = os.environ.get("TRACK_INFO_TOPIC", "/person_tracks_info")
        max_distance = float(os.environ.get("TRACK_MAX_DISTANCE", "1.5"))
        max_missed = int(os.environ.get("TRACK_MAX_MISSED", "5"))
        # The Velodyne bags publish at ~5 Hz, so the filter steps 0.2 s.
        dt = float(os.environ.get("TRACK_DT", "0.2"))

        self._adapter_in = MappingDetectionAdapter()
        self._adapter_out = MappingTrackAdapter()
        self._tracker = MultiObjectTracker(
            TrackerConfig(
                max_association_distance=max_distance,
                max_missed_frames=max_missed,
            ),
            association_strategy=HungarianMatcher(),
            motion_model=KalmanFilter2D(
                dt=dt,
                process_variance=0.1,
                measurement_variance=0.05,
            ),
        )

        from rclpy.qos import HistoryPolicy, QoSProfile, ReliabilityPolicy

        qos = QoSProfile(
            reliability=ReliabilityPolicy.RELIABLE,
            history=HistoryPolicy.KEEP_LAST,
            depth=1,
        )
        self._pub = self.create_publisher(PoseArray, track_topic, qos)
        self._info_pub = self.create_publisher(String, track_info_topic, qos)
        self.create_subscription(
            PoseArray, detection_topic, self._on_detections, qos
        )
        self.get_logger().info(
            f"{detection_topic} -> tracker -> {track_topic} (+ {track_info_topic}) "
            f"(max_distance={max_distance}, max_missed={max_missed}, dt={dt})"
        )

    def _on_detections(self, msg: PoseArray) -> None:
        stamp = msg.header.stamp
        timestamp = float(stamp.sec) + float(stamp.nanosec) * 1e-9
        raw = [
            {
                "x": float(pose.position.x),
                "y": float(pose.position.y),
                "timestamp": timestamp,
                "z": float(pose.position.z),
            }
            for pose in msg.poses
        ]
        detections = self._adapter_in.convert_frame(raw)
        tracks = self._tracker.update(detections)

        out = PoseArray()
        out.header = msg.header
        info = []
        for track in tracks:
            pose = Pose()
            pose.position = Point(
                x=track.current_position.x,
                y=track.current_position.y,
                z=0.0,
            )
            pose.orientation = Quaternion(w=1.0)
            out.poses.append(pose)
            info.append(
                {
                    "id": track.id,
                    "name": track.name,
                    "x": track.current_position.x,
                    "y": track.current_position.y,
                    "missed_frames": track.missed_frames,
                }
            )

        self._pub.publish(out)
        self._info_pub.publish(String(data=json.dumps(info)))

        summary = ", ".join(
            f"{t.name}({t.id}) x={t.current_position.x:.2f} y={t.current_position.y:.2f}"
            for t in tracks
        )
        self.get_logger().info(
            f"tracks={len(tracks)}" + (f" [{summary}]" if summary else ""),
            throttle_duration_sec=1.0,
        )


def main(args: list[str] | None = None) -> None:
    rclpy.init(args=args)
    node = PersonTrackNode()
    try:
        rclpy.spin(node)
    except KeyboardInterrupt:
        pass
    finally:
        node.destroy_node()
        if rclpy.ok():
            rclpy.shutdown()


if __name__ == "__main__":
    main()
