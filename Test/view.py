#!/usr/bin/env python3
import rclpy
from rclpy.node import Node
from sensor_msgs.msg import PointCloud2
import sensor_msgs_py.point_cloud2 as pc2

class LidarViewer(Node):
    def __init__(self):
        super().__init__('lidar_viewer')
        self.sub = self.create_subscription(
            PointCloud2, '/velodyne_points', self.cb, 10)

    def cb(self, msg: PointCloud2):
        # xyz points as list of (x, y, z)
        points = list(pc2.read_points(msg, field_names=('x', 'y', 'z'), skip_nans=True))
        self.get_logger().info(f'{len(points)} points, frame={msg.header.frame_id}')
        # then: Open3D / Matplotlib / VisPy / save to file / etc.

def main():
    rclpy.init()
    node = LidarViewer()
    rclpy.spin(node)

if __name__ == '__main__':
    main()