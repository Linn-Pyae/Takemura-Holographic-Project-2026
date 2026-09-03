#!/usr/bin/env python3
"""Live 3D Velodyne PointCloud viewer (Open3D) for /velodyne_points_wifi."""

from __future__ import annotations

import os
import time
import traceback

import numpy as np
import open3d as o3d
import rclpy
from rclpy.node import Node
from rclpy.qos import (
    DurabilityPolicy,
    HistoryPolicy,
    QoSProfile,
    ReliabilityPolicy,
)
from sensor_msgs.msg import PointCloud2

TOPIC = os.environ.get('LIDAR_TOPIC', '/velodyne_points_wifi')

WIFI_QOS = QoSProfile(
    reliability=ReliabilityPolicy.BEST_EFFORT,
    durability=DurabilityPolicy.VOLATILE,
    history=HistoryPolicy.KEEP_LAST,
    depth=1,
)

MIN_UI_INTERVAL = 1.0 / 8.0
MAX_DISPLAY_POINTS = 800


def field_offset(msg: PointCloud2, name: str, default: int) -> int:
    for field in msg.fields:
        if field.name == name:
            return int(field.offset)
    return default


def cloud_to_xyz(msg: PointCloud2) -> np.ndarray:
    """Fast XYZ from the raw buffer; thin only for display."""
    n = int(msg.width) * int(msg.height)
    if n <= 0 or msg.point_step < 12:
        return np.zeros((0, 3), dtype=np.float64)
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
    xyz = xyz[np.isfinite(xyz).all(axis=1)]
    if len(xyz) > MAX_DISPLAY_POINTS:
        idx = np.linspace(0, len(xyz) - 1, MAX_DISPLAY_POINTS, dtype=np.int64)
        xyz = xyz[idx]
    return np.ascontiguousarray(xyz, dtype=np.float64)


def height_colors(xyz: np.ndarray) -> np.ndarray:
    z = xyz[:, 2]
    zmin = float(z.min())
    zmax = float(z.max())
    if zmax <= zmin:
        zmax = zmin + 1e-3
    t = np.clip((z - zmin) / (zmax - zmin), 0.0, 1.0)
    r = np.clip(1.5 * t - 0.2, 0.0, 1.0)
    g = np.clip(1.0 - np.abs(t - 0.5) * 2.0, 0.0, 1.0)
    b = np.clip(1.2 * (1.0 - t) - 0.1, 0.0, 1.0)
    return np.ascontiguousarray(np.column_stack([r, g, b]), dtype=np.float64)


class LidarViewer(Node):
    def __init__(self):
        super().__init__('lidar_viewer_3d')
        self.latest_msg: PointCloud2 | None = None
        self.frame_id = ''
        self.new_cloud = False
        self.callback_count = 0
        self.sub = self.create_subscription(PointCloud2, TOPIC, self.cb, WIFI_QOS)
        self.get_logger().info(f'Listening on {TOPIC} (Open3D live view, throttled)')

    def cb(self, msg: PointCloud2):
        # Do not convert here — converting every WiFi callback is what made the Mac hitch.
        self.latest_msg = msg
        self.frame_id = msg.header.frame_id
        self.new_cloud = True
        self.callback_count += 1


def main():
    if os.environ.get('FASTRTPS_DEFAULT_PROFILES_FILE') and not os.environ.get('FASTDDS_DEFAULT_PROFILES_FILE'):
        os.environ['FASTDDS_DEFAULT_PROFILES_FILE'] = os.environ['FASTRTPS_DEFAULT_PROFILES_FILE']

    rclpy.init()
    node = LidarViewer()

    vis = o3d.visualization.Visualizer()
    ok = vis.create_window(
        window_name=f'Velodyne Live — {TOPIC}', width=1280, height=800, visible=True)
    if not ok:
        node.get_logger().error('Failed to create Open3D window')
        node.destroy_node()
        rclpy.shutdown()
        return

    pcd = o3d.geometry.PointCloud()
    pcd.points = o3d.utility.Vector3dVector(np.zeros((1, 3)))
    pcd.colors = o3d.utility.Vector3dVector(np.array([[0.3, 0.3, 0.3]]))
    vis.add_geometry(pcd)
    vis.add_geometry(o3d.geometry.TriangleMesh.create_coordinate_frame(size=1.0))

    render = vis.get_render_option()
    render.background_color = np.asarray([0.05, 0.05, 0.08])
    render.point_size = 3.0

    fitted = False
    last_n = 1
    last_ui = 0.0
    last_log = 0

    node.get_logger().info('Open3D window open — waiting for point clouds…')

    try:
        while True:
            loop_start = time.monotonic()
            rclpy.spin_once(node, timeout_sec=0.0)

            now = time.monotonic()
            if node.new_cloud and node.latest_msg is not None and (now - last_ui) >= MIN_UI_INTERVAL:
                try:
                    xyz = cloud_to_xyz(node.latest_msg)
                except Exception:
                    node.get_logger().error(f'Failed to parse cloud:\n{traceback.format_exc()}')
                    xyz = np.zeros((0, 3), dtype=np.float64)
                node.new_cloud = False
                last_ui = now
                if len(xyz) == 0:
                    continue
                n = len(xyz)
                pcd.points = o3d.utility.Vector3dVector(xyz)
                pcd.colors = o3d.utility.Vector3dVector(height_colors(xyz))
                if n != last_n:
                    vis.remove_geometry(pcd, reset_bounding_box=False)
                    vis.add_geometry(pcd, reset_bounding_box=not fitted)
                    last_n = n
                else:
                    vis.update_geometry(pcd)
                if not fitted:
                    vis.reset_view_point(True)
                    fitted = True
                    node.get_logger().info('Camera fitted to first cloud')
                if node.callback_count == 1 or node.callback_count - last_log >= 40:
                    last_log = node.callback_count
                    node.get_logger().info(
                        f'cloud #{node.callback_count}: {n} draw pts  frame={node.frame_id}')

            if not vis.poll_events():
                break
            vis.update_renderer()
            leftover = MIN_UI_INTERVAL - (time.monotonic() - loop_start)
            if leftover > 0.0:
                time.sleep(leftover)
    except KeyboardInterrupt:
        pass
    finally:
        vis.destroy_window()
        node.destroy_node()
        rclpy.shutdown()


if __name__ == '__main__':
    main()
