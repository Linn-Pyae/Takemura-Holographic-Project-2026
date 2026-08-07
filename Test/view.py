#!/usr/bin/env python3
"""Live 3D Velodyne PointCloud viewer (Open3D) for /velodyne_points_wifi."""

from __future__ import annotations

import os
import time
import traceback

import numpy as np
import open3d as o3d
import rclpy
import sensor_msgs_py.point_cloud2 as pc2
from rclpy.node import Node
from rclpy.qos import (
    DurabilityPolicy,
    HistoryPolicy,
    QoSProfile,
    ReliabilityPolicy,
)
from sensor_msgs.msg import PointCloud2

TOPIC = os.environ.get('LIDAR_TOPIC', '/velodyne_points_wifi')

# Match Pi WiFi republisher (BEST_EFFORT)
WIFI_QOS = QoSProfile(
    reliability=ReliabilityPolicy.BEST_EFFORT,
    durability=DurabilityPolicy.VOLATILE,
    history=HistoryPolicy.KEEP_LAST,
    depth=1,  # keep only newest cloud — drops backlog/lag
)

# UI throttle (~15 FPS max) so Open3D stays responsive
MIN_UI_INTERVAL = 1.0 / 15.0
# Extra safety downsample on Mac (Pi already caps at 4000)
MAX_DISPLAY_POINTS = 3000


def cloud_to_xyz(msg: PointCloud2) -> np.ndarray:
    """Convert PointCloud2 to contiguous float64 Nx3."""
    try:
        arr = pc2.read_points_numpy(msg, field_names=('x', 'y', 'z'), skip_nans=True)
    except Exception:
        pts = list(pc2.read_points(msg, field_names=('x', 'y', 'z'), skip_nans=True))
        if not pts:
            return np.zeros((0, 3), dtype=np.float64)
        return np.asarray([(float(p[0]), float(p[1]), float(p[2])) for p in pts], dtype=np.float64)

    if arr is None or len(arr) == 0:
        return np.zeros((0, 3), dtype=np.float64)

    if getattr(arr.dtype, 'names', None):
        xyz = np.column_stack([arr['x'], arr['y'], arr['z']])
    else:
        xyz = np.asarray(arr, dtype=np.float64)
        if xyz.ndim == 1:
            xyz = xyz.reshape(-1, 3)
        xyz = xyz[:, :3]

    xyz = np.ascontiguousarray(xyz, dtype=np.float64)
    mask = np.isfinite(xyz).all(axis=1)
    xyz = xyz[mask]
    if len(xyz) > MAX_DISPLAY_POINTS:
        idx = np.linspace(0, len(xyz) - 1, MAX_DISPLAY_POINTS, dtype=np.int64)
        xyz = xyz[idx]
    return xyz


def height_colors(xyz: np.ndarray) -> np.ndarray:
    """Fast height coloring (min/max, no percentiles)."""
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
        self.latest_xyz: np.ndarray | None = None
        self.frame_id = ''
        self.new_cloud = False
        self.callback_count = 0

        self.sub = self.create_subscription(PointCloud2, TOPIC, self.cb, WIFI_QOS)
        self.get_logger().info(f'Listening on {TOPIC} (Open3D live view, throttled)')

    def cb(self, msg: PointCloud2):
        try:
            xyz = cloud_to_xyz(msg)
            if len(xyz) == 0:
                return
            # Always keep only the newest cloud (drop backlog)
            self.latest_xyz = xyz
            self.frame_id = msg.header.frame_id
            self.new_cloud = True
            self.callback_count += 1
            if self.callback_count == 1 or self.callback_count % 30 == 0:
                self.get_logger().info(
                    f'cloud #{self.callback_count}: {len(xyz)} points  frame={self.frame_id}')
        except Exception:
            self.get_logger().error(f'Failed to parse cloud:\n{traceback.format_exc()}')


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

    axes = o3d.geometry.TriangleMesh.create_coordinate_frame(size=1.0)
    vis.add_geometry(axes)

    render = vis.get_render_option()
    render.background_color = np.asarray([0.05, 0.05, 0.08])
    render.point_size = 2.5

    fitted = False
    last_n = 1
    last_ui = 0.0
    idle_spins = 0

    node.get_logger().info('Open3D window open — waiting for point clouds…')

    try:
        while True:
            # Drain ROS callbacks; keep only newest cloud
            rclpy.spin_once(node, timeout_sec=0.0)
            while node.context.ok():
                # Non-blocking drain so we don't process a backlog of old clouds
                if not rclpy.ok():
                    break
                # spin_once returns quickly when queue empty
                had = node.new_cloud
                rclpy.spin_once(node, timeout_sec=0.0)
                if not node.new_cloud and not had:
                    break
                # if new_cloud stayed/got set, continue draining a few times max
                break

            now = time.monotonic()
            if node.new_cloud and node.latest_xyz is not None and (now - last_ui) >= MIN_UI_INTERVAL:
                xyz = node.latest_xyz
                node.new_cloud = False
                last_ui = now
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

            idle_spins += 1
            if idle_spins == 400 and node.callback_count == 0:
                node.get_logger().warn(
                    f'No messages on {TOPIC} yet. Check Pi + TakemuraLab + FastDDS XML.')

            if not vis.poll_events():
                break
            vis.update_renderer()
    except KeyboardInterrupt:
        pass
    finally:
        vis.destroy_window()
        node.destroy_node()
        rclpy.shutdown()


if __name__ == '__main__':
    main()
