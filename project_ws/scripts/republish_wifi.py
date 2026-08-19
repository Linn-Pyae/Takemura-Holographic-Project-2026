#!/usr/bin/env python3
"""Republish /velodyne_points as BEST_EFFORT /velodyne_points_wifi.

Newest scan only. XYZ is read from the raw buffer (no slow point iterator),
NaNs are dropped, then the remaining returns are evenly thinned so people
keep enough points to cluster.
"""
import time

import numpy as np
import rclpy
from rclpy.node import Node
from rclpy.qos import (
    DurabilityPolicy,
    HistoryPolicy,
    QoSProfile,
    ReliabilityPolicy,
    qos_profile_sensor_data,
)
from sensor_msgs.msg import PointCloud2, PointField
import sensor_msgs_py.point_cloud2 as pc2

SUB_QOS = QoSProfile(
    reliability=ReliabilityPolicy.RELIABLE,
    durability=DurabilityPolicy.VOLATILE,
    history=HistoryPolicy.KEEP_LAST,
    depth=1,
)

MAX_HZ = 8.0
MAX_POINTS = 3500


def field_offset(msg: PointCloud2, name: str, default: int) -> int:
    for field in msg.fields:
        if field.name == name:
            return int(field.offset)
    return default


def finite_xyz(msg: PointCloud2) -> np.ndarray:
    n = int(msg.width) * int(msg.height)
    if n <= 0 or msg.point_step < 12:
        return np.zeros((0, 3), dtype=np.float32)
    ox = field_offset(msg, 'x', 0)
    oy = field_offset(msg, 'y', 4)
    oz = field_offset(msg, 'z', 8)
    raw = np.frombuffer(msg.data, dtype=np.uint8)
    usable = n * msg.point_step
    if len(raw) < usable:
        n = len(raw) // msg.point_step
        usable = n * msg.point_step
    pts = raw[:usable].reshape(n, msg.point_step)
    x = pts[:, ox:ox + 4].view(np.float32).reshape(-1)
    y = pts[:, oy:oy + 4].view(np.float32).reshape(-1)
    z = pts[:, oz:oz + 4].view(np.float32).reshape(-1)
    xyz = np.column_stack((x, y, z))
    return xyz[np.isfinite(xyz).all(axis=1)]


def thin(xyz: np.ndarray, max_points: int) -> np.ndarray:
    if len(xyz) <= max_points:
        return np.ascontiguousarray(xyz, dtype=np.float32)
    idx = np.linspace(0, len(xyz) - 1, max_points, dtype=np.int64)
    return np.ascontiguousarray(xyz[idx], dtype=np.float32)


def xyz_to_cloud(xyz, header):
    xyz = np.ascontiguousarray(xyz, dtype=np.float32)
    fields = [
        PointField(name='x', offset=0, datatype=PointField.FLOAT32, count=1),
        PointField(name='y', offset=4, datatype=PointField.FLOAT32, count=1),
        PointField(name='z', offset=8, datatype=PointField.FLOAT32, count=1),
    ]
    return pc2.create_cloud(header, fields, xyz)


class WifiRepublisher(Node):
    def __init__(self):
        super().__init__('velodyne_wifi_republisher')
        self.pub = self.create_publisher(
            PointCloud2, '/velodyne_points_wifi', qos_profile_sensor_data)
        self.sub = self.create_subscription(
            PointCloud2, '/velodyne_points', self.cb, SUB_QOS)
        self._last_pub = 0.0
        self._n = 0
        self.get_logger().info(
            f'Republishing -> /velodyne_points_wifi '
            f'(BEST_EFFORT, max {MAX_HZ} Hz, max {MAX_POINTS} pts)')

    def cb(self, msg: PointCloud2):
        now = time.monotonic()
        if now - self._last_pub < (1.0 / MAX_HZ):
            return
        try:
            xyz = thin(finite_xyz(msg), MAX_POINTS)
            if len(xyz) == 0:
                return
            self.pub.publish(xyz_to_cloud(xyz, msg.header))
            self._last_pub = now
            self._n += 1
            if self._n % 40 == 1:
                self.get_logger().info(f'wifi frame {self._n}: {len(xyz)} pts')
        except Exception as e:
            self.get_logger().error(f'republish failed: {e}')


def main():
    rclpy.init()
    node = WifiRepublisher()
    try:
        rclpy.spin(node)
    except KeyboardInterrupt:
        pass
    finally:
        node.destroy_node()
        rclpy.shutdown()


if __name__ == '__main__':
    main()
