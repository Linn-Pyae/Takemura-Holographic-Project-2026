# Real-MCAP protocol validation

This helper exports the line coordinates produced by the existing offline
`static_map_projector` + `static_map_line_extractor` validation pipeline using
the **Mild** preset. It does not modify either implementation.

```bash
python -m pip install -r project_ws/src/static_map_bridge/validation/requirements.txt
python project_ws/src/static_map_bridge/validation/export_mild_lines_from_real_mcap.py \
  lidar_sample/lidar_sample_0.mcap \
  lidar_sample_2/lidar_sample_2_0.mcap \
  lidar_sample_3/lidar_sample_3_0.mcap
```

The generated CSV files can be passed as optional arguments to the standalone
`test_packet_protocol` executable. The test serializes, deserializes, and checks
every coordinate after float32 conversion.
