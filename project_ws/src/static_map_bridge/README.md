# static_map_bridge

An independent ROS 2 bridge for static-environment line maps. It subscribes to
the `visualization_msgs/msg/MarkerArray` produced on `/static_map/lines`,
extracts `Marker.LINE_LIST` pairs, and sends them to a dedicated Unix domain
datagram socket:

```text
/static_map/lines -> static_map_bridge -> /tmp/takemura-static-map.sock
```

It never uses `/tmp/takemura-renderer.sock` and does not perform the renderer's
`(-y, -x)` conversion. All coordinates stay in the LiDAR frame.

## Package structure

```text
static_map_bridge/
├── CMakeLists.txt
├── package.xml
├── include/static_map_bridge/packet_protocol.hpp
├── src/packet_protocol.cpp
├── src/static_map_bridge_node.cpp
├── test/test_packet_protocol.cpp
└── validation/
```

The packet protocol library only uses the C++ standard library. ROS message
handling and Unix-socket transmission are isolated in the node.

## Packet protocol version 1

All fields are little-endian. The header is 28 bytes and each line segment is
16 bytes.

| Offset | Size | Type | Meaning |
|---:|---:|---|---|
| 0 | 4 | bytes | magic ASCII `TSMP` |
| 4 | 2 | uint16 | protocol version (`1`) |
| 6 | 2 | uint16 | header size (`28`) |
| 8 | 8 | uint64 | map sequence number |
| 16 | 4 | uint32 | packet index, zero-based |
| 20 | 4 | uint32 | total packet count |
| 24 | 4 | uint32 | segment count in this packet |
| 28 | 16 × N | float32 | repeated `x1, y1, x2, y2` |

With the default `max_datagram_bytes=8192`, one packet contains at most 510
segments. A full packet is 8188 bytes. Segment order is preserved when chunks
are produced. An empty map is one 28-byte packet with segment count zero, so a
future receiver can clear a previously displayed map.

A receiver must group packets by `map_sequence`, verify a consistent
`packet_count`, collect every index from `0` through `packet_count - 1`, and
concatenate segments in index order. Only after all packets arrive should it
atomically replace the displayed map. Incomplete or older sequences must not be
shown. Receiver implementation is deliberately outside this package/version.

## Parameters

| Parameter | Default | Meaning |
|---|---:|---|
| `socket_path` | `/tmp/takemura-static-map.sock` | dedicated receiver socket path |
| `max_datagram_bytes` | `8192` | packet byte limit; accepted range 44–65536 |

The bridge is a datagram sender and does not create, bind, unlink, or modify the
destination socket. Until a future receiver binds the socket path, the node
prints a throttled warning and continues running.

## Build and test

From `project_ws` in a sourced ROS 2 environment:

```bash
colcon build --packages-select static_map_bridge
source install/setup.bash
colcon test --packages-select static_map_bridge
colcon test-result --verbose
```

## Run

Start the future socket receiver first, then run:

```bash
ros2 run static_map_bridge static_map_bridge_node
```

Optional overrides:

```bash
ros2 run static_map_bridge static_map_bridge_node --ros-args \
  -p socket_path:=/tmp/takemura-static-map.sock \
  -p max_datagram_bytes:=8192
```

The subscription uses reliable, transient-local, keep-last(1) QoS.

## Run with the provisional Mild line preset

The extractor defaults are not changed. Apply Mild only as runtime overrides:

Terminal 1:

```bash
ros2 run static_map_line_extractor static_map_line_extractor_node --ros-args \
  -p minimum_line_length:=0.50 \
  -p minimum_component_cells:=5 \
  -p contour_epsilon:=0.15
```

Terminal 2:

```bash
ros2 run static_map_bridge static_map_bridge_node
```

Together with the separately started projector, the independent path is:

```text
PointCloud -> static_map_projector -> /static_map/debug_grid
           -> static_map_line_extractor -> /static_map/lines
           -> static_map_bridge -> /tmp/takemura-static-map.sock
```

`map_renderer` reception and drawing, person-data composition, HDMI/fan output,
and presentation effects are intentionally not implemented here.
