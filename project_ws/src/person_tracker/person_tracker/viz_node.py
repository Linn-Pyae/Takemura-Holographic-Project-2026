#!/usr/bin/env python3
"""Live 2D map: detections + tracked people with trail history."""

from __future__ import annotations

import json
import os
from collections import defaultdict, deque

import rclpy
from geometry_msgs.msg import PoseArray
from rclpy.node import Node
from std_msgs.msg import String

DETECTION_TOPIC = os.environ.get("DETECTION_TOPIC", "/person_detections")
TRACK_TOPIC = os.environ.get("TRACK_TOPIC", "/person_tracks")
TRACK_INFO_TOPIC = os.environ.get("TRACK_INFO_TOPIC", "/person_tracks_info")
HISTORY_LEN = int(os.environ.get("TRACK_VIZ_HISTORY", "80"))
REFRESH_MS = int(os.environ.get("TRACK_VIZ_REFRESH_MS", "100"))


class MapVizNode(Node):
    def __init__(self) -> None:
        super().__init__("person_map_viz")
        self.detections: list[tuple[float, float]] = []
        self.tracks: list[dict] = []
        self.histories: dict[int, deque[tuple[float, float]]] = defaultdict(
            lambda: deque(maxlen=HISTORY_LEN)
        )
        self._dirty = False

        self.create_subscription(PoseArray, DETECTION_TOPIC, self._on_detections, 10)
        self.create_subscription(PoseArray, TRACK_TOPIC, self._on_tracks, 10)
        self.create_subscription(String, TRACK_INFO_TOPIC, self._on_track_info, 10)
        self.get_logger().info(
            f"2D map: dets={DETECTION_TOPIC} tracks={TRACK_TOPIC} "
            f"(history={HISTORY_LEN})"
        )

    def _on_detections(self, msg: PoseArray) -> None:
        self.detections = [
            (float(p.position.x), float(p.position.y)) for p in msg.poses
        ]
        self._dirty = True

    def _on_tracks(self, msg: PoseArray) -> None:
        if not self.tracks or len(self.tracks) != len(msg.poses):
            self.tracks = [
                {
                    "id": i + 1,
                    "name": f"T{i + 1}",
                    "x": float(p.position.x),
                    "y": float(p.position.y),
                }
                for i, p in enumerate(msg.poses)
            ]
        else:
            for i, p in enumerate(msg.poses):
                self.tracks[i]["x"] = float(p.position.x)
                self.tracks[i]["y"] = float(p.position.y)
        self._record_histories()
        self._dirty = True

    def _on_track_info(self, msg: String) -> None:
        try:
            payload = json.loads(msg.data)
        except json.JSONDecodeError:
            return
        if not isinstance(payload, list):
            return
        self.tracks = payload
        self._record_histories()
        self._dirty = True

    def _record_histories(self) -> None:
        active_ids = set()
        for track in self.tracks:
            track_id = int(track["id"])
            active_ids.add(track_id)
            self.histories[track_id].append(
                (float(track["x"]), float(track["y"]))
            )
        # Drop trails for tracks that disappeared
        for track_id in list(self.histories):
            if track_id not in active_ids:
                del self.histories[track_id]


def _palette(index: int) -> str:
    colors = ["#d62728", "#2ca02c", "#1f77b4", "#ff7f0e", "#9467bd", "#17becf"]
    return colors[index % len(colors)]


def main(args: list[str] | None = None) -> None:
    # Matplotlib UI (not Agg) — need an interactive backend window
    cache_dir = os.path.join(
        os.environ.get("TMPDIR", "/tmp"), "person-tracker-matplotlib"
    )
    os.makedirs(cache_dir, exist_ok=True)
    os.environ.setdefault("MPLCONFIGDIR", cache_dir)

    import matplotlib.pyplot as plt

    rclpy.init(args=args)
    node = MapVizNode()

    plt.ion()
    fig, ax = plt.subplots(figsize=(9, 8))
    fig.canvas.manager.set_window_title("Person tracks (2D map)")
    ax.set_title("Yellow dots = detections · colored lines = track trails")
    ax.set_xlabel("X [m]")
    ax.set_ylabel("Y [m]")
    ax.set_aspect("equal", adjustable="datalim")
    ax.grid(True, alpha=0.3)

    det_scatter = ax.scatter([], [], s=36, c="#c9a227", marker="o", label="Detection", zorder=3)
    track_lines: dict[int, object] = {}
    track_heads: dict[int, object] = {}
    track_labels: dict[int, object] = {}

    # Seed empty legend entry
    ax.legend(loc="upper right", fontsize=8)

    node.get_logger().info("Matplotlib 2D map open — close the window to quit.")

    try:
        while plt.fignum_exists(fig.number):
            rclpy.spin_once(node, timeout_sec=0.0)
            for _ in range(10):
                rclpy.spin_once(node, timeout_sec=0.0)

            if node._dirty:
                node._dirty = False

                if node.detections:
                    det_scatter.set_offsets(node.detections)
                else:
                    det_scatter.set_offsets([[float("nan"), float("nan")]])

                active = {int(t["id"]) for t in node.tracks}
                for track_id in list(track_lines):
                    if track_id not in active:
                        track_lines.pop(track_id).remove()
                        if track_id in track_heads:
                            track_heads.pop(track_id).remove()
                        if track_id in track_labels:
                            track_labels.pop(track_id).remove()

                for track in node.tracks:
                    track_id = int(track["id"])
                    color = _palette(track_id - 1)
                    hist = list(node.histories.get(track_id, []))
                    xs = [p[0] for p in hist] or [float(track["x"])]
                    ys = [p[1] for p in hist] or [float(track["y"])]

                    if track_id not in track_lines:
                        (line,) = ax.plot(
                            xs,
                            ys,
                            "-",
                            color=color,
                            linewidth=2.0,
                            label=f"{track.get('name', track_id)} ({track_id})",
                        )
                        (head,) = ax.plot(
                            [xs[-1]],
                            [ys[-1]],
                            "o",
                            color=color,
                            markersize=8,
                            zorder=4,
                        )
                        label = ax.annotate(
                            f"{track.get('name', '?')} ({track_id})",
                            (xs[-1], ys[-1]),
                            xytext=(6, 6),
                            textcoords="offset points",
                            fontsize=9,
                            color=color,
                        )
                        track_lines[track_id] = line
                        track_heads[track_id] = head
                        track_labels[track_id] = label
                        ax.legend(loc="upper right", fontsize=8)
                    else:
                        track_lines[track_id].set_data(xs, ys)
                        track_heads[track_id].set_data([xs[-1]], [ys[-1]])
                        track_labels[track_id].xy = (xs[-1], ys[-1])
                        track_labels[track_id].set_text(
                            f"{track.get('name', '?')} ({track_id})"
                        )

                all_x = [x for x, _ in node.detections]
                all_y = [y for _, y in node.detections]
                for track in node.tracks:
                    all_x.append(float(track["x"]))
                    all_y.append(float(track["y"]))
                    all_x.extend(p[0] for p in node.histories.get(int(track["id"]), []))
                    all_y.extend(p[1] for p in node.histories.get(int(track["id"]), []))
                if all_x and all_y:
                    pad = 1.0
                    ax.set_xlim(min(all_x) - pad, max(all_x) + pad)
                    ax.set_ylim(min(all_y) - pad, max(all_y) + pad)

            fig.canvas.draw_idle()
            fig.canvas.flush_events()
            plt.pause(REFRESH_MS / 1000.0)
    except KeyboardInterrupt:
        pass
    finally:
        plt.close(fig)
        node.destroy_node()
        if rclpy.ok():
            rclpy.shutdown()


if __name__ == "__main__":
    main()
