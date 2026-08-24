# Map renderer (`main.cpp`)

Raylib map that draws fading footprint trails for tracked people. In exhibit mode it listens on a Unix domain datagram socket for `PersonUpdate` packets from `renderer_bridge`. If the socket cannot be opened (or `TAKEMURA_RENDERER_DEMO=1`), it falls back to a closed Catmull–Rom demo track with two walkers.

### 1. Live input (IPC)

- Socket path: `TAKEMURA_RENDERER_SOCKET` or `/tmp/takemura-renderer.sock`.
- Each frame, `drainLiveUpdates()` non-blockingly receives packets. A person is keyed by `id`; updates apply only when `sequence >= last_sequence`.
- Planar LiDAR coordinates are mapped onto the parchment with `lidarPlanarToMap`: `(x, y) -> (-y, -x)` (−90° yaw then a horizontal mirror so room left/right match the screen).
- Label comes from the packet name, or `ID{n}` if empty. Ink color is chosen from a small palette by `id`.

People with no packet for more than `kLivePersonTimeoutSeconds` (1.5 s) are removed; their trail is moved into `fading_trails` so footprints keep fading instead of vanishing with the marker.

### 2. Footprint placement (distance, not time)

`updateTrail()` drops prints by distance travelled in map meters, not by wall-clock stride timing:

- Step length: `kFootprintStepDistance` / `kLiveFootprintStepDistance` (0.42 m).
- On first sighting, one print is planted immediately so a new person is visible before a full step.
- While the person moves, leftover distance is accumulated; whenever enough distance remains, a print is placed along the segment, offset left/right by `kFootprintLateralOffset` (0.16 m) and alternating feet.
- Rotation faces the travel direction (`atan2` + 90°). The right-foot PNG is mirrored (negative source width) for the left foot.

Spacing stays constant when people speed up or slow down.

### 3. Trail lifetime and fade

- Lifetime: `TAKEMURA_TRAIL_SECONDS` (default 2 s, minimum 1 s), stored in `g_trail_lifetime`.
- `trailOpacity(age)`: full opacity for the first `kTrailHoldFraction` (0.3) of life, then a smoothstep fade to zero.
- `ageTrail()` advances ages every frame and pops prints older than the lifetime.
- Orphan trails in `fading_trails` age the same way after the person is gone.

### 4. Drawing

- Parchment `bg.png` is cover-cropped to the window; optional grid (and demo route) under the trails.
- Footprints draw with the texture when loaded; otherwise a simple ink line/circle fallback.
- Person marker: soft filled circle + outline. Labels are drawn in **screen space** after `EndMode2D` so map rotation does not flip text.
- Optional circular mask (`TAKEMURA_HOLO_MASK` or key `C`) blacks out outside a centered disk for the holographic fan.

### 5. View controls

Camera uses 72 px/m (`kPixelsPerMeter`) times `view_zoom`. Pan / zoom / rotation come from:

- Env: `TAKEMURA_VIEW_PAN_X`, `TAKEMURA_VIEW_PAN_Y`, `TAKEMURA_VIEW_ZOOM`, `TAKEMURA_VIEW_ROTATION`
- File: `TAKEMURA_VIEW_FILE` or `/tmp/takemura-view` (`pan_x pan_y zoom [rotation]`), reloaded when mtime changes
- Keys: arrows pan, `+/-` zoom, `[` `]` rotate, `0` reset to env defaults, `H` toggle HUD, Space pause, `R` reset trails

Fullscreen sizing: `TAKEMURA_RENDERER_FULLSCREEN`.

### 6. Demo mode

When IPC is off, two `DemoPerson`s sample `kDemoTrack` with Catmull–Rom (`sampleDemoTrack`) at opposite speeds. The same `updateTrail` / fade path is used so the visual behavior matches live mode.

## Parameters / constants

| Name | Default | Description |
| --- | --- | --- |
| `TAKEMURA_TRAIL_SECONDS` | `2.0` | Footprint lifetime (s) |
| `kTrailHoldFraction` | `0.3` | Fraction of life at full opacity before fade |
| `kFootprintStepDistance` | `0.42` | Meters between demo footprints |
| `kLiveFootprintStepDistance` | `0.42` | Meters between live footprints |
| `kFootprintLateralOffset` | `0.16` | Left/right offset from path (m) |
| `kFootprintSize` | `0.42` | Drawn footprint size (m) |
| `kLivePersonTimeoutSeconds` | `1.5` | Drop live person after silence (s) |
| `kPixelsPerMeter` | `72` | Base camera zoom scale |
| `TAKEMURA_RENDERER_SOCKET` | `/tmp/takemura-renderer.sock` | Unix datagram path |
| `TAKEMURA_RENDERER_DEMO` | off | Force demo track even if socket works |
| `TAKEMURA_HOLO_MASK` | off | Start with circular fan mask |
| `TAKEMURA_VIEW_HUD` | off | Start with on-screen view help |
| `TAKEMURA_VIEW_FILE` | `/tmp/takemura-view` | External pan/zoom/rotation file |

## Notes

- Packet layout (44-byte big-endian `PersonUpdate`) is documented in `map_renderer/README.md`; `main.cpp` only consumes decoded updates from `UnixDatagramReceiver`.
- Target FPS is 30. Pause sets `dt` to 0 so trails stop aging and demo time freezes.
