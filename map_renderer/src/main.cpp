#include <raylib.h>

#include <array>
#include <cmath>
#include <cstddef>
#include <deque>
#include <string>

namespace {

constexpr int kInitialWidth = 1280;
constexpr int kInitialHeight = 720;
constexpr float kPixelsPerMeter = 72.0F;
constexpr float kSecondsPerSegment = 1.15F;
constexpr float kTrailSampleInterval = 0.24F;
constexpr float kTrailLifetime = 6.0F;
constexpr int kFontAtlasSize = 64;
constexpr float kFootprintLateralOffset = 0.11F;
constexpr float kFootprintSize = 0.42F;

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

std::string assetPath(const char* relative_path) {
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

Vector2 catmullRom(
    Vector2 p0,
    Vector2 p1,
    Vector2 p2,
    Vector2 p3,
    float t) {
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

    return catmullRom(
        kDemoTrack[wrappedIndex(index - 1)],
        kDemoTrack[wrappedIndex(index)],
        kDemoTrack[wrappedIndex(index + 1)],
        kDemoTrack[wrappedIndex(index + 2)],
        t);
}

void updateTrail(
    std::deque<TrailPoint>& trail,
    Vector2 person_position,
    float dt,
    float& sample_accumulator,
    Vector2& previous_sample_position,
    bool& has_previous_sample,
    bool& next_is_right_foot) {
    for (TrailPoint& point : trail) {
        point.age += dt;
    }
    while (!trail.empty() && trail.front().age >= kTrailLifetime) {
        trail.pop_front();
    }

    sample_accumulator += dt;
    while (sample_accumulator >= kTrailSampleInterval) {
        Vector2 direction{0.0F, -1.0F};
        if (has_previous_sample) {
            direction = {
                person_position.x - previous_sample_position.x,
                person_position.y - previous_sample_position.y,
            };
            const float length =
                std::sqrt(direction.x * direction.x + direction.y * direction.y);
            if (length > 0.0001F) {
                direction.x /= length;
                direction.y /= length;
            }
        }

        // In screen-style coordinates (+Y is down), this is the walker's
        // right-hand perpendicular to the direction of travel.
        const Vector2 right{-direction.y, direction.x};
        const float side = next_is_right_foot ? 1.0F : -1.0F;
        const Vector2 footprint_position{
            person_position.x + right.x * kFootprintLateralOffset * side,
            person_position.y + right.y * kFootprintLateralOffset * side,
        };
        const float rotation =
            std::atan2(direction.y, direction.x) * RAD2DEG + 90.0F;

        trail.push_back(
            {footprint_position, rotation, 0.0F, next_is_right_foot});
        previous_sample_position = person_position;
        has_previous_sample = true;
        next_is_right_foot = !next_is_right_foot;
        sample_accumulator -= kTrailSampleInterval;
    }
}

void drawMapBackground() {
    constexpr Color grid_color{116, 95, 67, 55};
    constexpr Color route_color{95, 69, 45, 75};

    for (int coordinate = -20; coordinate <= 20; ++coordinate) {
        const float value = static_cast<float>(coordinate);
        DrawLineEx({value, -12.0F}, {value, 12.0F}, 0.012F, grid_color);
        DrawLineEx({-20.0F, value}, {20.0F, value}, 0.012F, grid_color);
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

    DrawTexturePro(texture, source,
                   {0.0F, 0.0F, screen_width, screen_height},
                   {0.0F, 0.0F}, 0.0F, WHITE);
}

void drawFallbackTrail(const std::deque<TrailPoint>& trail) {
    constexpr Color ink{88, 38, 32, 255};

    for (std::size_t index = 1; index < trail.size(); ++index) {
        const TrailPoint& previous = trail[index - 1];
        const TrailPoint& current = trail[index];
        const float opacity = 1.0F - current.age / kTrailLifetime;

        DrawLineEx(previous.position, current.position, 0.07F,
                   Fade(ink, opacity * 0.55F));
        DrawCircleV(current.position, 0.055F, Fade(ink, opacity * 0.8F));
    }
}

void drawFootprints(
    const std::deque<TrailPoint>& trail,
    Texture2D footprint_texture) {
    if (!IsTextureValid(footprint_texture)) {
        drawFallbackTrail(trail);
        return;
    }

    for (const TrailPoint& footprint : trail) {
        const float opacity = 1.0F - footprint.age / kTrailLifetime;
        Rectangle source{
            0.0F,
            0.0F,
            footprint.right_foot
                ? static_cast<float>(footprint_texture.width)
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

void drawPerson(Vector2 position, Font font) {
    constexpr Color ink{88, 38, 32, 255};
    DrawCircleV(position, 0.24F, Fade(ink, 0.18F));
    DrawCircleLinesV(position, 0.18F, ink);
    DrawCircleV(position, 0.075F, ink);

    DrawTextEx(font, "Demo Person",
               {position.x + 0.28F, position.y - 0.28F},
               0.22F, 0.015F, ink);
}

}  // namespace

int main() {
    SetConfigFlags(FLAG_WINDOW_RESIZABLE | FLAG_VSYNC_HINT);
    InitWindow(kInitialWidth, kInitialHeight,
               "Takemura Holographic Renderer - Trail Demo");
    SetTargetFPS(60);

    const std::string regular_font_path = assetPath("fonts/FeENrm28C.otf");
    Font regular_font =
        LoadFontEx(regular_font_path.c_str(), kFontAtlasSize, nullptr, 0);
    const bool regular_font_loaded = IsFontValid(regular_font);
    if (!regular_font_loaded) {
        TraceLog(LOG_WARNING, "Could not load font: %s",
                 regular_font_path.c_str());
        regular_font = GetFontDefault();
    } else {
        SetTextureFilter(regular_font.texture, TEXTURE_FILTER_BILINEAR);
    }

    const std::string italic_font_path = assetPath("fonts/FeENit27C.otf");
    Font italic_font =
        LoadFontEx(italic_font_path.c_str(), kFontAtlasSize, nullptr, 0);
    const bool italic_font_loaded = IsFontValid(italic_font);
    if (!italic_font_loaded) {
        TraceLog(LOG_WARNING, "Could not load font: %s",
                 italic_font_path.c_str());
        italic_font = regular_font;
    } else {
        SetTextureFilter(italic_font.texture, TEXTURE_FILTER_BILINEAR);
    }

    const std::string footprint_path =
        assetPath("textures/footprint_right.png");
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
    camera.rotation = 0.0F;
    camera.zoom = kPixelsPerMeter;

    float demo_time = 0.0F;
    float trail_sample_accumulator = kTrailSampleInterval;
    bool paused = false;
    bool has_previous_sample = false;
    bool next_is_right_foot = true;
    Vector2 previous_sample_position{};
    std::deque<TrailPoint> trail;

    while (!WindowShouldClose()) {
        if (IsKeyPressed(KEY_SPACE)) {
            paused = !paused;
        }
        if (IsKeyPressed(KEY_R)) {
            demo_time = 0.0F;
            trail_sample_accumulator = kTrailSampleInterval;
            trail.clear();
            has_previous_sample = false;
            next_is_right_foot = true;
        }

        const float dt = paused ? 0.0F : GetFrameTime();
        demo_time += dt;
        const Vector2 person_position = sampleDemoTrack(demo_time);
        updateTrail(trail, person_position, dt, trail_sample_accumulator,
                    previous_sample_position, has_previous_sample,
                    next_is_right_foot);

        camera.offset = {
            GetScreenWidth() * 0.5F,
            GetScreenHeight() * 0.5F,
        };

        BeginDrawing();
        ClearBackground(Color{222, 205, 165, 255});
        drawCoverBackground(background_texture);

        BeginMode2D(camera);
        drawMapBackground();
        drawFootprints(trail, footprint_texture);
        drawPerson(person_position, italic_font);
        EndMode2D();

        DrawTextEx(regular_font, "Temporary Track + 6 Second Trail",
                   {24.0F, 18.0F}, 27.0F, 1.0F,
                   Color{68, 48, 35, 255});
        DrawTextEx(regular_font, "Space: pause    R: reset    Esc: quit",
                   {25.0F, 53.0F}, 19.0F, 0.7F,
                   Color{99, 76, 53, 255});
        DrawTextEx(regular_font,
                   TextFormat("Footprints: %d", static_cast<int>(trail.size())),
                   {25.0F, 78.0F}, 19.0F, 0.7F,
                   Color{99, 76, 53, 255});
        if (paused) {
            DrawTextEx(regular_font, "Paused",
                       {static_cast<float>(GetScreenWidth() - 110), 22.0F},
                       22.0F, 0.8F, Color{88, 38, 32, 255});
        }

        DrawTextEx(regular_font, TextFormat("%d FPS", GetFPS()),
                   {static_cast<float>(GetScreenWidth() - 82),
                    static_cast<float>(GetScreenHeight() - 34)},
                   18.0F, 0.6F, Color{70, 105, 55, 255});
        EndDrawing();
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
