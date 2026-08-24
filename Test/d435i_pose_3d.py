#!/usr/bin/env python3
"""Record MediaPipe Pose pixels and RealSense-backed 3D coordinates."""

from __future__ import annotations

import argparse
import csv
import json
import sys
import time
from datetime import datetime
from pathlib import Path

import cv2
import mediapipe as mp
import numpy as np
import pyrealsense2 as rs


LANDMARK_NAMES = (
    "nose", "left_eye_inner", "left_eye", "left_eye_outer",
    "right_eye_inner", "right_eye", "right_eye_outer", "left_ear",
    "right_ear", "mouth_left", "mouth_right", "left_shoulder",
    "right_shoulder", "left_elbow", "right_elbow", "left_wrist",
    "right_wrist", "left_pinky", "right_pinky", "left_index",
    "right_index", "left_thumb", "right_thumb", "left_hip",
    "right_hip", "left_knee", "right_knee", "left_ankle",
    "right_ankle", "left_heel", "right_heel", "left_foot_index",
    "right_foot_index",
)


def intrinsics_dict(intr: rs.intrinsics) -> dict:
    return {
        "width": intr.width,
        "height": intr.height,
        "fx": intr.fx,
        "fy": intr.fy,
        "ppx": intr.ppx,
        "ppy": intr.ppy,
        "model": str(intr.model),
        "coeffs": list(intr.coeffs),
    }


def extrinsics_dict(extr: rs.extrinsics) -> dict:
    return {
        "rotation": list(extr.rotation),
        "translation_m": list(extr.translation),
    }


def median_depth_m(depth_data: np.ndarray, u: int, v: int,
                   depth_scale: float, radius: int = 2) -> float:
    """Return a hole-resistant local median depth in metres."""
    height, width = depth_data.shape
    x0, x1 = max(0, u - radius), min(width, u + radius + 1)
    y0, y1 = max(0, v - radius), min(height, v + radius + 1)
    samples = depth_data[y0:y1, x0:x1]
    valid = samples[samples > 0]
    if valid.size == 0:
        return 0.0
    return float(np.median(valid)) * depth_scale


def make_calibration(profile: rs.pipeline_profile, duration: float) -> tuple[dict, object]:
    color_profile = profile.get_stream(rs.stream.color).as_video_stream_profile()
    depth_profile = profile.get_stream(rs.stream.depth).as_video_stream_profile()
    color_intr = color_profile.get_intrinsics()
    depth_intr = depth_profile.get_intrinsics()
    color_to_depth = color_profile.get_extrinsics_to(depth_profile)
    depth_to_color = depth_profile.get_extrinsics_to(color_profile)
    depth_scale = profile.get_device().first_depth_sensor().get_depth_scale()

    calibration = {
        "captured_at": datetime.now().astimezone().isoformat(),
        "duration_seconds": duration,
        "device": {
            "name": profile.get_device().get_info(rs.camera_info.name),
            "serial_number": profile.get_device().get_info(rs.camera_info.serial_number),
            "firmware_version": profile.get_device().get_info(rs.camera_info.firmware_version),
        },
        "color": {
            "resolution": [color_intr.width, color_intr.height],
            "fps": color_profile.fps(),
            "format": str(color_profile.format()),
            "intrinsics": intrinsics_dict(color_intr),
        },
        "depth": {
            "resolution": [depth_intr.width, depth_intr.height],
            "fps": depth_profile.fps(),
            "format": str(depth_profile.format()),
            "intrinsics": intrinsics_dict(depth_intr),
            "depth_scale_m_per_unit": depth_scale,
        },
        "color_to_depth": extrinsics_dict(color_to_depth),
        "depth_to_color": extrinsics_dict(depth_to_color),
        "coordinate_system": {
            "units": "metres",
            "x": "right",
            "y": "down",
            "z": "forward",
            "xyz_color": "RealSense color optical frame",
            "xyz_depth": "RealSense depth optical frame",
        },
    }
    return calibration, color_to_depth


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Capture D435i + MediaPipe Pose 2D/Depth/3D data")
    parser.add_argument("--duration", type=float, default=10.0)
    parser.add_argument("--output-dir", type=Path)
    parser.add_argument("--model", type=Path,
                        default=Path(__file__).parent / "models" / "pose_landmarker_full.task")
    parser.add_argument("--preview", action="store_true")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    if args.duration <= 0:
        raise SystemExit("--duration must be greater than zero")
    if not args.model.is_file():
        raise SystemExit(f"MediaPipe model not found: {args.model}")

    stamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    output_dir = args.output_dir or Path(__file__).parent / "captures" / stamp
    output_dir.mkdir(parents=True, exist_ok=True)
    calibration_path = output_dir / "calibration.json"
    landmarks_path = output_dir / "pose_3d.csv"

    pipeline = rs.pipeline()
    config = rs.config()
    # USB 2.1 was reported on this Mac, so use a bandwidth-safe matched mode.
    config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 15)
    config.enable_stream(rs.stream.depth, 640, 480, rs.format.z16, 15)

    profile = None
    landmarker = None
    csv_file = None
    frame_count = 0
    landmark_row_count = 0
    pose_frame_count = 0

    try:
        profile = pipeline.start(config)
        calibration, color_to_depth = make_calibration(profile, args.duration)
        calibration_path.write_text(
            json.dumps(calibration, ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8")

        align_to_color = rs.align(rs.stream.color)
        depth_scale = calibration["depth"]["depth_scale_m_per_unit"]

        base_options = mp.tasks.BaseOptions(model_asset_path=str(args.model))
        options = mp.tasks.vision.PoseLandmarkerOptions(
            base_options=base_options,
            running_mode=mp.tasks.vision.RunningMode.VIDEO,
            num_poses=1,
            min_pose_detection_confidence=0.5,
            min_pose_presence_confidence=0.5,
            min_tracking_confidence=0.5,
        )
        landmarker = mp.tasks.vision.PoseLandmarker.create_from_options(options)

        csv_file = landmarks_path.open("w", newline="", encoding="utf-8")
        writer = csv.writer(csv_file)
        writer.writerow([
            "elapsed_s", "device_timestamp_ms", "frame_number",
            "landmark_id", "landmark_name", "u", "v",
            "normalized_u", "normalized_v", "visibility", "presence",
            "depth_m", "X_color_m", "Y_color_m", "Z_color_m",
            "X_depth_m", "Y_depth_m", "Z_depth_m",
        ])

        print(json.dumps(calibration, ensure_ascii=False, indent=2), flush=True)
        print(f"\nCapturing for {args.duration:.1f} seconds...", flush=True)
        started = time.monotonic()
        last_mp_timestamp = -1

        while time.monotonic() - started < args.duration:
            frames = pipeline.wait_for_frames(5000)
            aligned = align_to_color.process(frames)
            color_frame = aligned.get_color_frame()
            depth_frame = aligned.get_depth_frame()
            if not color_frame or not depth_frame:
                continue

            frame_count += 1
            elapsed = time.monotonic() - started
            color_bgr = np.asanyarray(color_frame.get_data())
            depth_data = np.asanyarray(depth_frame.get_data())
            height, width = color_bgr.shape[:2]
            aligned_intr = depth_frame.profile.as_video_stream_profile().get_intrinsics()

            color_rgb = cv2.cvtColor(color_bgr, cv2.COLOR_BGR2RGB)
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=color_rgb)
            mp_timestamp = max(last_mp_timestamp + 1, int(elapsed * 1000))
            last_mp_timestamp = mp_timestamp
            result = landmarker.detect_for_video(mp_image, mp_timestamp)

            if result.pose_landmarks:
                pose_frame_count += 1
                for landmark_id, landmark in enumerate(result.pose_landmarks[0]):
                    u = int(round(landmark.x * (width - 1)))
                    v = int(round(landmark.y * (height - 1)))
                    if not (0 <= u < width and 0 <= v < height):
                        continue

                    depth_m = median_depth_m(depth_data, u, v, depth_scale)
                    if depth_m > 0:
                        xyz_color = rs.rs2_deproject_pixel_to_point(
                            aligned_intr, [float(u), float(v)], depth_m)
                        xyz_depth = rs.rs2_transform_point_to_point(
                            color_to_depth, xyz_color)
                    else:
                        xyz_color = [float("nan")] * 3
                        xyz_depth = [float("nan")] * 3

                    writer.writerow([
                        f"{elapsed:.6f}", f"{color_frame.get_timestamp():.3f}",
                        color_frame.get_frame_number(), landmark_id,
                        LANDMARK_NAMES[landmark_id], u, v,
                        f"{landmark.x:.8f}", f"{landmark.y:.8f}",
                        f"{(landmark.visibility or 0.0):.8f}",
                        f"{(landmark.presence or 0.0):.8f}", f"{depth_m:.6f}",
                        *(f"{value:.6f}" for value in xyz_color),
                        *(f"{value:.6f}" for value in xyz_depth),
                    ])
                    landmark_row_count += 1

                    if args.preview:
                        cv2.circle(color_bgr, (u, v), 3, (0, 255, 0), -1)

            if args.preview:
                cv2.putText(color_bgr, f"{elapsed:4.1f}/{args.duration:.1f}s",
                            (15, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.8,
                            (255, 255, 255), 2, cv2.LINE_AA)
                cv2.imshow("D435i MediaPipe Pose 3D", color_bgr)
                if cv2.waitKey(1) & 0xFF in (27, ord("q")):
                    break

        csv_file.flush()
        print(f"Captured frames: {frame_count}")
        print(f"Frames with pose: {pose_frame_count}")
        print(f"3D landmark rows: {landmark_row_count}")
        print(f"Calibration: {calibration_path}")
        print(f"Pose CSV: {landmarks_path}")
        if pose_frame_count == 0:
            print("WARNING: No person was detected; calibration is still valid.",
                  file=sys.stderr)
        return 0
    finally:
        if csv_file is not None:
            csv_file.close()
        if landmarker is not None:
            landmarker.close()
        if profile is not None:
            pipeline.stop()
        if args.preview:
            cv2.destroyAllWindows()


if __name__ == "__main__":
    raise SystemExit(main())
