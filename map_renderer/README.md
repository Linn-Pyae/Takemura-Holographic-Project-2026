# Renderer IPC layer

This C++17 project contains a minimal raylib renderer and receives person
positions through a non-blocking Unix domain datagram socket. The IPC receiver
is intended to be drained once per render frame. It currently targets
Linux/POSIX.

## Run the renderer

The CMake build uses an installed raylib 6.0 when available and otherwise
fetches the official raylib 6.0 source during configuration.

```sh
cmake -S . -B build
cmake --build build --parallel
./build/renderer
```

The current renderer moves two temporary subjects around a closed track in
opposite directions. Footprints are placed by distance travelled rather than
elapsed time, so their spacing stays constant when movement speed changes.
The step distance is configured by `kFootprintStepDistance` in `src/main.cpp`
and defaults to 0.42 m. Each subject owns an independent three-second trail.
Its opacity uses a smoothstep curve: it holds near full strength at first,
fades rapidly through the middle, then reaches zero smoothly. The supplied
right-foot PNG is mirrored at draw time for left steps and rotated to the
direction of travel.
The parchment `assets/textures/bg.png` is drawn behind the scene with its
aspect ratio preserved and cropped as needed to cover the whole window.
Press Space to pause, R to reset, and Escape to quit. IPC integration will
replace the temporary track data later.
The renderer intentionally has no top-left debug overlay; only the subject
labels and a temporary `Paused` indicator are drawn over the map.

The UI uses IM FELL English Pro from `assets/fonts/`. Its SIL Open Font License
is distributed alongside the regular and italic font files. CMake copies the
assets next to the renderer executable after each build.

## Packet format

Every datagram must be exactly 44 bytes. Multi-byte values use big-endian byte
order, even though the transport is local, so the wire format does not depend
on compiler layout or host endianness.

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | signed 32-bit integer | person ID |
| 4 | IEEE-754 32-bit float | x coordinate |
| 8 | IEEE-754 32-bit float | y coordinate |
| 12 | unsigned 64-bit integer | monotonically increasing sequence |
| 20 | signed 64-bit integer | sender-defined observation timestamp |
| 28 | 16 ASCII bytes | display label (null-padded), e.g. HARRY |

The logical value is:

```cpp
struct PersonUpdate {
    std::int32_t id;
    float x;
    float y;
    std::uint64_t sequence;
    std::int64_t timestamp;
    std::string name;
};
```

Use `serializePersonPacket()` instead of sending this struct's memory directly.
C++ object padding and host byte order are not part of the protocol.

`sequence` determines freshness. `timestamp` is metadata for interpolation,
latency measurement, or logging and must not be used as the primary ordering
key. The sender should increment sequence for every update it emits.

## Build and test

```sh
cmake -S . -B build
cmake --build build
ctest --test-dir build --output-on-failure
./build/unix_receive_example /tmp/takemura-renderer.sock
```

## Future render-loop integration

Drain the non-blocking socket until `would_block` once per frame. Keep the
greatest sequence per person; retaining only the final datagram would
accidentally discard updates for other people.

```cpp
mapipc::UnixDatagramReceiver receiver("/tmp/takemura-renderer.sock");
std::unordered_map<std::int32_t, mapipc::PersonUpdate> people;

std::string error;
if (!receiver.open(&error)) {
    // Report error.
}

while (!WindowShouldClose()) {
    for (;;) {
        mapipc::ReceiveResult result = receiver.receive();
        if (result.status == mapipc::ReceiveStatus::would_block) {
            break;
        }
        if (result.status != mapipc::ReceiveStatus::packet) {
            continue;
        }

        const mapipc::PersonUpdate& incoming = *result.update;
        auto current = people.find(incoming.id);
        if (current == people.end() ||
            incoming.sequence > current->second.sequence) {
            people[incoming.id] = incoming;
        }
    }

    BeginDrawing();
    // Draw from people.
    EndDrawing();
}
```

The receiver owns its socket path while open. On startup it removes a stale
socket node left by a crashed process, but refuses to remove an existing
non-socket file or a socket owned by a live receiver. Only one receiver can use
a given path.
