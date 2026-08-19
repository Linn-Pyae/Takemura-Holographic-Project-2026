#include <raylib.h>

#include "person_packet.hpp"
#include "unix_receiver.hpp"

#include <algorithm>
#include <array>
#include <cmath>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <deque>
#include <string>
#include <sys/stat.h>
#include <unordered_map>
#include <utility>
#include <vector>

namespace {

constexpr int kInitialWidth = 1280;
constexpr int kInitialHeight = 720;
constexpr float kPixelsPerMeter = 72.0F;
constexpr float kSecondsPerSegment = 1.15F;
// How long a footprint stays on the map, from TAKEMURA_TRAIL_SECONDS.
float g_trail_lifetime = 2.0F;
// Fraction of that life a footprint holds full opacity before it starts to
// fade, so prints read as fresh ink first and then dry out.
constexpr float kTrailHoldFraction = 0.3F;
// Distance travelled between footprints, in scene-space meters. Sized like a
// walking stride so left/right prints read as a Marauder's Map trail.
constexpr float kFootprintStepDistance = 0.42F;
constexpr float kLiveFootprintStepDistance = 0.42F;
// Drop a live person if no packet arrives for this long (tracker already
// drops missed tracks; this clears stale markers/IDs on the map).
constexpr float kLivePersonTimeoutSeconds = 1.5F;
constexpr int kFontAtlasSize = 64;
constexpr float kFootprintLateralOffset = 0.16F;
constexpr float kFootprintSize = 0.42F;
constexpr float kPersonLabelFontSize = 0.32F;

// Temporary closed track in scene-space meters. Replace this with accepted
// PersonUpdate positions once the IPC receiver is connected to the render loop.
constexpr std::array<Vector2, 12> kDemoTrack{{
    {-5.5F, 1.6F},
    {-4.4F, -1.5F},
    {-2.4F, -2.8F},
    {-0.2F, -2.1F},
    {1.2F, -0.5F},
    {3.5F, -1.7F},
    {5.2F, -0.3F},
    {4.5F, 2.2F},
    {2.1F, 2.8F},
    {0.2F, 1.5F},
    {-1.7F, 2.8F},
    {-3.8F, 2.6F},
}};

struct TrailPoint {
  Vector2 position{};
  float rotation = 0.0F;
  float age = 0.0F;
  bool right_foot = true;
};

struct LivePerson {
  std::int32_t id = 0;
  std::string label;
  Color ink{};

  Vector2 position{};
  float distance_since_last_footprint = 0.0F;
  Vector2 previous_position{};
  bool has_previous_position = false;
  bool next_is_right_foot = true;
  std::uint64_t last_sequence = 0;
  float seconds_since_update = 0.0F;
  std::deque<TrailPoint> trail;
};

Color colorForId(std::int32_t id) {
  static const Color palette[] = {
      {88, 38, 32, 255},  {42, 62, 73, 255}, {62, 92, 58, 255},
      {120, 72, 38, 255}, {78, 48, 96, 255},
  };
  const std::size_t index = static_cast<std::size_t>(id >= 0 ? id : -id) %
                            (sizeof(palette) / sizeof(palette[0]));
  return palette[index];
}

bool envFlagEnabled(const char *name) {
  const char *value = std::getenv(name);
  return value != nullptr && value[0] != '\0' && value[0] != '0';
}

float envFloat(const char *name, float fallback) {
  const char *value = std::getenv(name);
  if (value == nullptr || value[0] == '\0') {
    return fallback;
  }
  char *end = nullptr;
  const float parsed = std::strtof(value, &end);
  if (end == value) {
    return fallback;
  }
  return parsed;
}

void fillMonitorIfRequested() {
  if (!envFlagEnabled("TAKEMURA_RENDERER_FULLSCREEN")) {
    return;
  }
  const int monitor = GetCurrentMonitor();
  const int width = GetMonitorWidth(monitor);
  const int height = GetMonitorHeight(monitor);
  if (width <= 0 || height <= 0) {
    return;
  }
  if (GetScreenWidth() != width || GetScreenHeight() != height) {
    SetWindowSize(width, height);
    SetWindowPosition(0, 0);
  }
}

const char *viewFilePath() {
  const char *value = std::getenv("TAKEMURA_VIEW_FILE");
  if (value != nullptr && value[0] != '\0') {
    return value;
  }
  return "/tmp/takemura-view";
}

bool loadViewFile(float &pan_x, float &pan_y, float &zoom, float &rotation,
                  time_t &seen_mtime) {
  struct stat info{};
  if (stat(viewFilePath(), &info) != 0) {
    return false;
  }
  if (info.st_mtime == seen_mtime) {
    return false;
  }
  std::FILE *file = std::fopen(viewFilePath(), "r");
  if (file == nullptr) {
    return false;
  }
  float parsed_x = 0.0F;
  float parsed_y = 0.0F;
  float parsed_zoom = 1.0F;
  float parsed_rotation = 0.0F;
  const int count = std::fscanf(file, "%f %f %f %f", &parsed_x, &parsed_y,
                                &parsed_zoom, &parsed_rotation);
  std::fclose(file);
  if (count < 3) {
    return false;
  }
  pan_x = parsed_x;
  pan_y = parsed_y;
  zoom = parsed_zoom;
  if (count >= 4) {
    rotation = parsed_rotation;
  }
  seen_mtime = info.st_mtime;
  TraceLog(LOG_INFO, "View file: pan=(%.0f, %.0f) zoom=%.2f rot=%.1f", pan_x,
           pan_y, zoom, rotation);
  return true;
}

std::string rendererSocketPath() {
  const char *value = std::getenv("TAKEMURA_RENDERER_SOCKET");
  if (value != nullptr && value[0] != '\0') {
    return value;
  }
  return "/tmp/takemura-renderer.sock";
}

// LiDAR planar (x forward, y left) onto the parchment:
// -90 deg yaw then a horizontal mirror so room left/right match the screen.
// (x, y) -> (-y, -x). Forward stays screen-up; LiDAR +Y is screen-left.
Vector2 lidarPlanarToMap(float x, float y) { return {-y, -x}; }

void applyLiveUpdate(LivePerson &person, const mapipc::PersonUpdate &update) {
  person.position = lidarPlanarToMap(update.x, update.y);
  if (!update.name.empty()) {
    person.label = update.name;
  } else if (person.label.empty()) {
    person.label = "ID" + std::to_string(update.id);
  }
  person.ink = colorForId(update.id);
  person.last_sequence = update.sequence;
  person.seconds_since_update = 0.0F;
}

struct DemoPerson {
  DemoPerson(const char *person_name, float offset_seconds, float speed,
             Color color)
      : name(person_name), track_offset_seconds(offset_seconds),
        track_speed(speed), ink(color) {}

  const char *name = "";
  float track_offset_seconds = 0.0F;
  float track_speed = 1.0F;
  Color ink{};

  Vector2 position{};
  float distance_since_last_footprint = 0.0F;
  Vector2 previous_position{};
  bool has_previous_position = false;
  bool next_is_right_foot = true;
  std::deque<TrailPoint> trail;
};

std::string assetPath(const char *relative_path) {
  std::string directory = GetApplicationDirectory();
  if (!directory.empty() && directory.back() != '/') {
    directory.push_back('/');
  }
  return directory + "assets/" + relative_path;
}

std::size_t wrappedIndex(int index) {
  const int count = static_cast<int>(kDemoTrack.size());
  return static_cast<std::size_t>((index % count + count) % count);
}

Vector2 catmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
  const float t2 = t * t;
  const float t3 = t2 * t;
  return {
      0.5F * ((2.0F * p1.x) + (-p0.x + p2.x) * t +
              (2.0F * p0.x - 5.0F * p1.x + 4.0F * p2.x - p3.x) * t2 +
              (-p0.x + 3.0F * p1.x - 3.0F * p2.x + p3.x) * t3),
      0.5F * ((2.0F * p1.y) + (-p0.y + p2.y) * t +
              (2.0F * p0.y - 5.0F * p1.y + 4.0F * p2.y - p3.y) * t2 +
              (-p0.y + 3.0F * p1.y - 3.0F * p2.y + p3.y) * t3),
  };
}

Vector2 sampleDemoTrack(float elapsed_seconds) {
  const float segment = elapsed_seconds / kSecondsPerSegment;
  const int index = static_cast<int>(std::floor(segment));
  const float t = segment - std::floor(segment);

  return catmullRom(kDemoTrack[wrappedIndex(index - 1)],
                    kDemoTrack[wrappedIndex(index)],
                    kDemoTrack[wrappedIndex(index + 1)],
                    kDemoTrack[wrappedIndex(index + 2)], t);
}

float trailOpacity(float age) {
  const float progress = std::clamp(age / g_trail_lifetime, 0.0F, 1.0F);
  if (progress <= kTrailHoldFraction) {
    return 1.0F;
  }
  const float fade =
      (progress - kTrailHoldFraction) / (1.0F - kTrailHoldFraction);
  return 1.0F - fade * fade * (3.0F - 2.0F * fade);
}

// Ages a trail without adding to it. Prints left by someone the tracker has
// lost keep fading on their own instead of vanishing with the marker.
void ageTrail(std::deque<TrailPoint> &trail, float dt) {
  for (TrailPoint &point : trail) {
    point.age += dt;
  }
  while (!trail.empty() && trail.front().age >= g_trail_lifetime) {
    trail.pop_front();
  }
}

void updateFadingTrails(std::vector<std::deque<TrailPoint>> &trails, float dt) {
  for (auto it = trails.begin(); it != trails.end();) {
    ageTrail(*it, dt);
    if (it->empty()) {
      it = trails.erase(it);
    } else {
      ++it;
    }
  }
}

void updateTrail(std::deque<TrailPoint> &trail, Vector2 person_position,
                 float dt, float step_distance,
                 float &distance_since_last_footprint,
                 Vector2 &previous_position, bool &has_previous_position,
                 bool &next_is_right_foot) {
  ageTrail(trail, dt);

  if (!has_previous_position) {
    previous_position = person_position;
    has_previous_position = true;
    // Plant one print immediately so a newly detected person is visible
    // before they have walked a full step distance.
    trail.push_back({person_position, 0.0F, 0.0F, next_is_right_foot});
    next_is_right_foot = !next_is_right_foot;
    return;
  }

  Vector2 direction{
      person_position.x - previous_position.x,
      person_position.y - previous_position.y,
  };
  float remaining_distance =
      std::sqrt(direction.x * direction.x + direction.y * direction.y);
  if (remaining_distance <= 0.0001F) {
    previous_position = person_position;
    return;
  }

  direction.x /= remaining_distance;
  direction.y /= remaining_distance;
  Vector2 cursor = previous_position;
  float distance_to_next = step_distance - distance_since_last_footprint;

  while (remaining_distance + 0.0001F >= distance_to_next) {
    cursor.x += direction.x * distance_to_next;
    cursor.y += direction.y * distance_to_next;

    // In screen-style coordinates (+Y is down), this is the walker's
    // right-hand perpendicular to the direction of travel.
    const Vector2 right{-direction.y, direction.x};
    const float side = next_is_right_foot ? 1.0F : -1.0F;
    const Vector2 footprint_position{
        cursor.x + right.x * kFootprintLateralOffset * side,
        cursor.y + right.y * kFootprintLateralOffset * side,
    };
    const float rotation =
        std::atan2(direction.y, direction.x) * RAD2DEG + 90.0F;

    trail.push_back({footprint_position, rotation, 0.0F, next_is_right_foot});
    next_is_right_foot = !next_is_right_foot;
    remaining_distance -= distance_to_next;
    distance_since_last_footprint = 0.0F;
    distance_to_next = step_distance;
  }

  distance_since_last_footprint += remaining_distance;
  previous_position = person_position;
}

void drainLiveUpdates(mapipc::UnixDatagramReceiver &receiver,
                      std::unordered_map<std::int32_t, LivePerson> &people) {
  for (;;) {
    const mapipc::ReceiveResult result = receiver.receive();
    if (result.status == mapipc::ReceiveStatus::would_block) {
      break;
    }
    if (result.status != mapipc::ReceiveStatus::packet || !result.update) {
      continue;
    }

    const mapipc::PersonUpdate &incoming = *result.update;
    LivePerson &person = people[incoming.id];
    if (person.id == 0) {
      person.id = incoming.id;
    }
    if (incoming.sequence >= person.last_sequence) {
      applyLiveUpdate(person, incoming);
    }
  }
}

void updateLiveTrails(std::unordered_map<std::int32_t, LivePerson> &people,
                      std::vector<std::deque<TrailPoint>> &fading_trails,
                      float dt) {
  for (auto it = people.begin(); it != people.end();) {
    LivePerson &person = it->second;
    person.seconds_since_update += dt;
    if (person.seconds_since_update > kLivePersonTimeoutSeconds) {
      if (!person.trail.empty()) {
        fading_trails.push_back(std::move(person.trail));
      }
      it = people.erase(it);
      continue;
    }

    updateTrail(person.trail, person.position, dt, kLiveFootprintStepDistance,
                person.distance_since_last_footprint, person.previous_position,
                person.has_previous_position, person.next_is_right_foot);
    ++it;
  }
}

void resetLivePerson(LivePerson &person) {
  person.distance_since_last_footprint = 0.0F;
  person.trail.clear();
  person.has_previous_position = false;
  person.next_is_right_foot = true;
}

constexpr Color kHeadingInk{88, 38, 32, 200};
constexpr float kHeadingLength = 2.4F;

void drawLidarHeading() {
  // Forward = screen-up; LiDAR +Y (left) = screen-left after the mirror.
  const Vector2 origin{0.0F, 0.0F};
  const Vector2 forward = lidarPlanarToMap(kHeadingLength, 0.0F);
  const Vector2 left = lidarPlanarToMap(0.0F, kHeadingLength);
  DrawCircleV(origin, 0.08F, kHeadingInk);
  DrawLineEx(origin, forward, 0.045F, kHeadingInk);
  DrawLineEx(origin, left, 0.045F, Fade(kHeadingInk, 0.65F));
}

// Text is drawn in screen space after EndMode2D. Inside the camera a rotated
// map would turn every label upside down or mirror it.
void drawScreenLabel(const Camera2D &camera, Font font, Vector2 world,
                     const char *text, float world_size, Vector2 screen_offset,
                     bool centered, Color ink) {
  const Vector2 anchor = GetWorldToScreen2D(world, camera);
  const float size = std::max(14.0F, world_size * camera.zoom);
  const float spacing = size * 0.05F;
  Vector2 position{anchor.x + screen_offset.x, anchor.y + screen_offset.y};
  if (centered) {
    const Vector2 extent = MeasureTextEx(font, text, size, spacing);
    position.x -= extent.x * 0.5F;
    position.y -= extent.y * 0.5F;
  }
  DrawTextEx(font, text, position, size, spacing, ink);
}

void drawLidarHeadingLabels(const Camera2D &camera, Font font) {
  drawScreenLabel(camera, font, lidarPlanarToMap(kHeadingLength, 0.0F),
                  "LIDAR FWD", 0.22F, {0.0F, -14.0F}, true, kHeadingInk);
  drawScreenLabel(camera, font, lidarPlanarToMap(0.0F, kHeadingLength),
                  "LIDAR LEFT", 0.22F, {0.0F, -14.0F}, true, kHeadingInk);
}

void drawMapBackground(bool draw_demo_route) {
  constexpr Color grid_color{116, 95, 67, 55};
  constexpr Color route_color{95, 69, 45, 75};

  for (int coordinate = -20; coordinate <= 20; coordinate += 2) {
    const float value = static_cast<float>(coordinate);
    DrawLineEx({value, -12.0F}, {value, 12.0F}, 0.012F, grid_color);
    DrawLineEx({-20.0F, value}, {20.0F, value}, 0.012F, grid_color);
  }

  if (!draw_demo_route) {
    return;
  }

  DrawRectangleLinesEx({-3.1F, -1.4F, 2.2F, 1.8F}, 0.05F, route_color);
  DrawRectangleLinesEx({1.5F, 0.4F, 2.6F, 1.7F}, 0.05F, route_color);

  for (std::size_t index = 0; index < kDemoTrack.size(); ++index) {
    const Vector2 from = kDemoTrack[index];
    const Vector2 to = kDemoTrack[(index + 1) % kDemoTrack.size()];
    DrawLineEx(from, to, 0.025F, Fade(route_color, 0.45F));
  }
}

void drawCoverBackground(Texture2D texture) {
  if (!IsTextureValid(texture)) {
    return;
  }

  const float screen_width = static_cast<float>(GetScreenWidth());
  const float screen_height = static_cast<float>(GetScreenHeight());
  const float screen_aspect = screen_width / screen_height;
  const float texture_aspect =
      static_cast<float>(texture.width) / static_cast<float>(texture.height);

  Rectangle source{
      0.0F,
      0.0F,
      static_cast<float>(texture.width),
      static_cast<float>(texture.height),
  };
  if (screen_aspect > texture_aspect) {
    source.height = static_cast<float>(texture.width) / screen_aspect;
    source.y = (static_cast<float>(texture.height) - source.height) * 0.5F;
  } else {
    source.width = static_cast<float>(texture.height) * screen_aspect;
    source.x = (static_cast<float>(texture.width) - source.width) * 0.5F;
  }

  DrawTexturePro(texture, source, {0.0F, 0.0F, screen_width, screen_height},
                 {0.0F, 0.0F}, 0.0F, WHITE);
}

void drawFallbackTrail(const std::deque<TrailPoint> &trail) {
  constexpr Color ink{88, 38, 32, 255};

  for (std::size_t index = 1; index < trail.size(); ++index) {
    const TrailPoint &previous = trail[index - 1];
    const TrailPoint &current = trail[index];
    const float opacity = trailOpacity(current.age);

    DrawLineEx(previous.position, current.position, 0.07F,
               Fade(ink, opacity * 0.55F));
    DrawCircleV(current.position, 0.055F, Fade(ink, opacity * 0.8F));
  }
}

void drawFootprints(const std::deque<TrailPoint> &trail,
                    Texture2D footprint_texture) {
  if (!IsTextureValid(footprint_texture)) {
    drawFallbackTrail(trail);
    return;
  }

  for (const TrailPoint &footprint : trail) {
    const float opacity = trailOpacity(footprint.age);
    Rectangle source{
        0.0F,
        0.0F,
        footprint.right_foot ? static_cast<float>(footprint_texture.width)
                             : -static_cast<float>(footprint_texture.width),
        static_cast<float>(footprint_texture.height),
    };
    const Rectangle destination{
        footprint.position.x,
        footprint.position.y,
        kFootprintSize,
        kFootprintSize,
    };
    const Vector2 origin{kFootprintSize * 0.5F, kFootprintSize * 0.5F};

    // The source image is black ink, so tint controls alpha here. A
    // negative source width mirrors the right footprint into a left one.
    DrawTexturePro(footprint_texture, source, destination, origin,
                   footprint.rotation, Fade(WHITE, opacity * 0.72F));
  }
}

void drawPerson(Vector2 position, Color ink) {
  DrawCircleV(position, 0.24F, Fade(ink, 0.18F));
  DrawCircleLinesV(position, 0.18F, ink);
  DrawCircleV(position, 0.075F, ink);
}

void drawPersonLabel(const Camera2D &camera, Font font, Vector2 position,
                     const char *name, Color ink) {
  drawScreenLabel(camera, font, position, name, kPersonLabelFontSize,
                  {14.0F, -10.0F}, false, ink);
}

} // namespace

int main() {
  SetConfigFlags(FLAG_WINDOW_RESIZABLE | FLAG_VSYNC_HINT);
  InitWindow(kInitialWidth, kInitialHeight,
             "Takemura Holographic Renderer - Trail Demo");
  if (!IsWindowReady()) {
    TraceLog(LOG_ERROR, "InitWindow failed. On the Pi set DISPLAY=:0 and "
                        "MESA_GL_VERSION_OVERRIDE=3.3");
    return 1;
  }
  fillMonitorIfRequested();
  SetTargetFPS(30);
  g_trail_lifetime =
      std::max(1.0F, envFloat("TAKEMURA_TRAIL_SECONDS", g_trail_lifetime));

  const std::string regular_font_path = assetPath("fonts/FeENrm28C.otf");
  Font regular_font =
      LoadFontEx(regular_font_path.c_str(), kFontAtlasSize, nullptr, 0);
  const bool regular_font_loaded = IsFontValid(regular_font);
  if (!regular_font_loaded) {
    TraceLog(LOG_WARNING, "Could not load font: %s", regular_font_path.c_str());
    regular_font = GetFontDefault();
  } else {
    SetTextureFilter(regular_font.texture, TEXTURE_FILTER_BILINEAR);
  }

  const std::string italic_font_path = assetPath("fonts/FeENit27C.otf");
  Font italic_font =
      LoadFontEx(italic_font_path.c_str(), kFontAtlasSize, nullptr, 0);
  const bool italic_font_loaded = IsFontValid(italic_font);
  if (!italic_font_loaded) {
    TraceLog(LOG_WARNING, "Could not load font: %s", italic_font_path.c_str());
    italic_font = regular_font;
  } else {
    SetTextureFilter(italic_font.texture, TEXTURE_FILTER_BILINEAR);
  }

  const std::string footprint_path = assetPath("textures/footprint_right.png");
  Texture2D footprint_texture = LoadTexture(footprint_path.c_str());
  const bool footprint_texture_loaded = IsTextureValid(footprint_texture);
  if (!footprint_texture_loaded) {
    TraceLog(LOG_WARNING, "Could not load footprint: %s",
             footprint_path.c_str());
  } else {
    SetTextureFilter(footprint_texture, TEXTURE_FILTER_BILINEAR);
  }

  const std::string background_path = assetPath("textures/bg.png");
  Texture2D background_texture = LoadTexture(background_path.c_str());
  const bool background_texture_loaded = IsTextureValid(background_texture);
  if (!background_texture_loaded) {
    TraceLog(LOG_WARNING, "Could not load background: %s",
             background_path.c_str());
  } else {
    SetTextureFilter(background_texture, TEXTURE_FILTER_BILINEAR);
  }

  Camera2D camera{};
  camera.target = {0.0F, 0.0F};
  camera.rotation = envFloat("TAKEMURA_VIEW_ROTATION", 0.0F);
  camera.zoom = kPixelsPerMeter;
  float view_pan_x = envFloat("TAKEMURA_VIEW_PAN_X", 0.0F);
  float view_pan_y = envFloat("TAKEMURA_VIEW_PAN_Y", 0.0F);
  float view_zoom = envFloat("TAKEMURA_VIEW_ZOOM", 1.0F);
  if (view_zoom < 0.15F) {
    view_zoom = 0.15F;
  }
  bool circle_mask = envFlagEnabled("TAKEMURA_HOLO_MASK");
  // The fan is a display surface, so the tuning overlay is opt-in.
  bool show_view_help = envFlagEnabled("TAKEMURA_VIEW_HUD");
  time_t view_file_mtime = 0;

  const bool force_demo = envFlagEnabled("TAKEMURA_RENDERER_DEMO");
  mapipc::UnixDatagramReceiver receiver(rendererSocketPath());
  std::string receiver_error;
  const bool ipc_mode = !force_demo && receiver.open(&receiver_error);
  if (!ipc_mode) {
    if (force_demo) {
      TraceLog(LOG_INFO, "Renderer demo mode (TAKEMURA_RENDERER_DEMO=1).");
    } else {
      TraceLog(LOG_WARNING,
               "IPC receiver unavailable (%s); falling back to demo track.",
               receiver_error.c_str());
    }
  } else {
    TraceLog(LOG_INFO, "Listening for live tracks on %s",
             receiver.socketPath().c_str());
  }

  bool paused = false;
  float demo_time = 0.0F;
  std::array<DemoPerson, 2> demo_people{{
      {"Demo Person A", 0.0F, 1.0F, Color{88, 38, 32, 255}},
      {"Demo Person B", kSecondsPerSegment * 5.0F, -0.82F,
       Color{42, 62, 73, 255}},
  }};
  std::unordered_map<std::int32_t, LivePerson> live_people;
  // Trails of people the tracker has dropped, still fading out.
  std::vector<std::deque<TrailPoint>> fading_trails;

  while (!WindowShouldClose()) {
    if (IsKeyPressed(KEY_SPACE)) {
      paused = !paused;
    }
    if (IsKeyPressed(KEY_R)) {
      demo_time = 0.0F;
      for (DemoPerson &person : demo_people) {
        person.distance_since_last_footprint = 0.0F;
        person.trail.clear();
        person.has_previous_position = false;
        person.next_is_right_foot = true;
      }
      for (auto &entry : live_people) {
        resetLivePerson(entry.second);
      }
      fading_trails.clear();
    }

    fillMonitorIfRequested();
    loadViewFile(view_pan_x, view_pan_y, view_zoom, camera.rotation,
                 view_file_mtime);

    const float pan_speed = 180.0F * GetFrameTime();
    if (IsKeyDown(KEY_LEFT)) {
      view_pan_x -= pan_speed;
    }
    if (IsKeyDown(KEY_RIGHT)) {
      view_pan_x += pan_speed;
    }
    if (IsKeyDown(KEY_UP)) {
      view_pan_y -= pan_speed;
    }
    if (IsKeyDown(KEY_DOWN)) {
      view_pan_y += pan_speed;
    }
    if (IsKeyDown(KEY_EQUAL) || IsKeyDown(KEY_KP_ADD)) {
      view_zoom += 0.6F * GetFrameTime();
    }
    if (IsKeyDown(KEY_MINUS) || IsKeyDown(KEY_KP_SUBTRACT)) {
      view_zoom -= 0.6F * GetFrameTime();
    }
    view_zoom = std::clamp(view_zoom, 0.15F, 4.0F);
    if (IsKeyDown(KEY_LEFT_BRACKET)) {
      camera.rotation -= 45.0F * GetFrameTime();
    }
    if (IsKeyDown(KEY_RIGHT_BRACKET)) {
      camera.rotation += 45.0F * GetFrameTime();
    }
    if (IsKeyPressed(KEY_C)) {
      circle_mask = !circle_mask;
    }
    if (IsKeyPressed(KEY_H)) {
      show_view_help = !show_view_help;
    }
    if (IsKeyPressed(KEY_ZERO)) {
      view_pan_x = envFloat("TAKEMURA_VIEW_PAN_X", 0.0F);
      view_pan_y = envFloat("TAKEMURA_VIEW_PAN_Y", 0.0F);
      view_zoom = envFloat("TAKEMURA_VIEW_ZOOM", 1.0F);
      camera.rotation = envFloat("TAKEMURA_VIEW_ROTATION", 0.0F);
    }

    const float dt = paused ? 0.0F : GetFrameTime();
    if (ipc_mode) {
      drainLiveUpdates(receiver, live_people);
      updateLiveTrails(live_people, fading_trails, dt);
      updateFadingTrails(fading_trails, dt);
    } else {
      demo_time += dt;
      for (DemoPerson &person : demo_people) {
        person.position = sampleDemoTrack(person.track_offset_seconds +
                                          demo_time * person.track_speed);
        updateTrail(person.trail, person.position, dt, kFootprintStepDistance,
                    person.distance_since_last_footprint,
                    person.previous_position, person.has_previous_position,
                    person.next_is_right_foot);
      }
    }

    camera.offset = {
        GetScreenWidth() * 0.5F + view_pan_x,
        GetScreenHeight() * 0.5F + view_pan_y,
    };
    camera.target = {0.0F, 0.0F};
    camera.zoom = kPixelsPerMeter * view_zoom;

    BeginDrawing();
    ClearBackground(Color{222, 205, 165, 255});
    drawCoverBackground(background_texture);

    BeginMode2D(camera);
    drawMapBackground(!ipc_mode);
    drawLidarHeading();
    if (ipc_mode) {
      for (const auto &trail : fading_trails) {
        drawFootprints(trail, footprint_texture);
      }
      for (const auto &entry : live_people) {
        drawFootprints(entry.second.trail, footprint_texture);
      }
      for (const auto &entry : live_people) {
        drawPerson(entry.second.position, entry.second.ink);
      }
    } else {
      for (const DemoPerson &person : demo_people) {
        drawFootprints(person.trail, footprint_texture);
      }
      for (const DemoPerson &person : demo_people) {
        drawPerson(person.position, person.ink);
      }
    }
    EndMode2D();

    drawLidarHeadingLabels(camera, regular_font);
    if (ipc_mode) {
      for (const auto &entry : live_people) {
        const LivePerson &person = entry.second;
        drawPersonLabel(camera, italic_font, person.position,
                        person.label.c_str(), person.ink);
      }
    } else {
      for (const DemoPerson &person : demo_people) {
        drawPersonLabel(camera, italic_font, person.position, person.name,
                        person.ink);
      }
    }

    if (circle_mask) {
      const float screen_width = static_cast<float>(GetScreenWidth());
      const float screen_height = static_cast<float>(GetScreenHeight());
      const float radius = std::min(screen_width, screen_height) * 0.5F;
      const Vector2 center{screen_width * 0.5F, screen_height * 0.5F};
      const Color hide{0, 0, 0, 255};
      DrawRectangle(0, 0, GetScreenWidth(), static_cast<int>(center.y - radius),
                    hide);
      DrawRectangle(0, static_cast<int>(center.y + radius), GetScreenWidth(),
                    GetScreenHeight(), hide);
      DrawRectangle(0, 0, static_cast<int>(center.x - radius),
                    GetScreenHeight(), hide);
      DrawRectangle(static_cast<int>(center.x + radius), 0, GetScreenWidth(),
                    GetScreenHeight(), hide);
      const int rings = 48;
      for (int i = 0; i < rings; ++i) {
        const float inner = radius + static_cast<float>(i) * 6.0F;
        DrawRing(center, inner, inner + 7.0F, 0.0F, 360.0F, 64, hide);
      }
    }

    if (show_view_help) {
      if (ipc_mode) {
        const std::string status =
            live_people.empty()
                ? "LIVE IPC — waiting for tracked people..."
                : ("LIVE IPC — people: " + std::to_string(live_people.size()));
        DrawTextEx(regular_font, status.c_str(), {18.0F, 18.0F}, 20.0F, 0.8F,
                   Color{88, 38, 32, 255});
      } else {
        DrawTextEx(regular_font, "DEMO MODE (no live socket)", {18.0F, 18.0F},
                   20.0F, 0.8F, Color{88, 38, 32, 255});
      }
    }

    if (paused) {
      DrawTextEx(regular_font, "Paused",
                 {static_cast<float>(GetScreenWidth() - 110), 22.0F}, 22.0F,
                 0.8F, Color{88, 38, 32, 255});
    }

    if (show_view_help) {
      char view_status[128];
      std::snprintf(view_status, sizeof(view_status),
                    "pan %.0f,%.0f  zoom %.2f  rot %.0f", view_pan_x,
                    view_pan_y, view_zoom, camera.rotation);
      DrawTextEx(regular_font, view_status, {18.0F, 46.0F}, 18.0F, 0.7F,
                 Color{88, 38, 32, 220});
      DrawTextEx(regular_font,
                 "echo \"PANX PANY ZOOM ROT\" > /tmp/takemura-view",
                 {18.0F, 70.0F}, 18.0F, 0.7F, Color{88, 38, 32, 220});
    }

    EndDrawing();
  }

  if (ipc_mode) {
    receiver.close();
  }

  if (background_texture_loaded) {
    UnloadTexture(background_texture);
  }
  if (footprint_texture_loaded) {
    UnloadTexture(footprint_texture);
  }
  if (italic_font_loaded) {
    UnloadFont(italic_font);
  }
  if (regular_font_loaded) {
    UnloadFont(regular_font);
  }
  CloseWindow();
  return 0;
}
