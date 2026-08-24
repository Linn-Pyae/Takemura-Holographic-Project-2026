using System.Diagnostics;
using System.Runtime.InteropServices;

const uint SdlInitVideo = 0x00000020;
const uint SdlWindowFullscreen = 0x00000001;
const uint SdlWindowFullscreenDesktop = SdlWindowFullscreen | 0x00001000;
const uint SdlRendererAccelerated = 0x00000002;
const uint SdlRendererPresentVsync = 0x00000004;
const uint SdlQuitEvent = 0x100;
const uint SdlKeyDownEvent = 0x300;
const int SdlKeyEscape = 27;
const int SdlWindowPosUndefinedMask = 0x1FFF0000;
const uint SdlPixelFormatRgb24 = 386930691;
const int SdlTextureAccessStreaming = 1;
const int VideoWidth = 1280;
const int VideoHeight = 720;

var display = GetDisplayArgument(args);
var cutoff = GetCutoffArgument(args);
var imagePath = GetImageArgument(args);
var videoPath = GetVideoArgument(args);

if (imagePath is not null && videoPath is not null)
{
    throw new ArgumentException("Use either --image or --video, not both.");
}

if (SDL_Init(SdlInitVideo) != 0)
{
    throw new InvalidOperationException($"SDL video initialization failed: {GetSdlError()}");
}

try
{
    var displays = SDL_GetNumVideoDisplays();
    if (displays < 1)
    {
        throw new InvalidOperationException("No display was found.");
    }

    Console.WriteLine("Available displays:");
    for (var index = 0; index < displays; index++)
    {
        SDL_GetDisplayBounds(index, out var bounds);
        Console.WriteLine($"  {index}: {Marshal.PtrToStringUTF8(SDL_GetDisplayName(index))} ({bounds.w}x{bounds.h}+{bounds.x}+{bounds.y})");
    }

    if (display < 0 || display >= displays)
    {
        throw new ArgumentOutOfRangeException(nameof(display), $"Display {display} is unavailable.");
    }

    var position = SdlWindowPosUndefinedMask | display;
    var window = SDL_CreateWindow(
        "Holo Canvas — Esc to close",
        position,
        position,
        1920,
        1080,
        SdlWindowFullscreenDesktop);
    if (window == IntPtr.Zero)
    {
        throw new InvalidOperationException($"SDL window creation failed: {GetSdlError()}");
    }

    try
    {
        var renderer = SDL_CreateRenderer(window, -1, SdlRendererAccelerated | SdlRendererPresentVsync);
        if (renderer == IntPtr.Zero)
        {
            throw new InvalidOperationException($"SDL renderer creation failed: {GetSdlError()}");
        }

        try
        {
            SDL_GetRendererOutputSize(renderer, out var width, out var height);
            Console.WriteLine($"Showing a {Math.Min(width, height)}x{Math.Min(width, height)} square canvas on display {display} ({width}x{height}).");
            Console.WriteLine($"Pixels at or below brightness {cutoff} are rendered as black (LED off). Press Esc to close.");

            var imageTexture = imagePath is null ? IntPtr.Zero : LoadBmpTexture(renderer, imagePath);
            var videoTexture = IntPtr.Zero;
            Process? videoDecoder = null;
            byte[]? videoFrame = null;
            var videoFramePin = default(GCHandle);

            try
            {
                if (videoPath is not null)
                {
                    Console.WriteLine("Looping video on the selected display. Press Esc to close.");
                    videoDecoder = StartVideoDecoder(videoPath);
                    videoFrame = new byte[VideoWidth * VideoHeight * 3];
                    videoFramePin = GCHandle.Alloc(videoFrame, GCHandleType.Pinned);
                    videoTexture = SDL_CreateTexture(renderer, SdlPixelFormatRgb24, SdlTextureAccessStreaming, VideoWidth, VideoHeight);
                    if (videoTexture == IntPtr.Zero)
                    {
                        throw new InvalidOperationException($"Could not create video texture: {GetSdlError()}");
                    }
                }

                var running = true;
                var eventBuffer = Marshal.AllocHGlobal(56);
                try
                {
                    while (running)
                    {
                        while (SDL_PollEvent(eventBuffer) != 0)
                        {
                            var eventType = unchecked((uint)Marshal.ReadInt32(eventBuffer, 0));
                            var key = Marshal.ReadInt32(eventBuffer, 20);
                            if (eventType == SdlQuitEvent || (eventType == SdlKeyDownEvent && key == SdlKeyEscape))
                            {
                                running = false;
                            }
                        }

                        if (videoTexture != IntPtr.Zero)
                        {
                            if (!ReadNextVideoFrame(videoDecoder!.StandardOutput.BaseStream, videoFrame!))
                            {
                                running = false;
                                continue;
                            }

                            if (SDL_UpdateTexture(videoTexture, IntPtr.Zero, videoFramePin.AddrOfPinnedObject(), VideoWidth * 3) != 0)
                            {
                                throw new InvalidOperationException($"Could not update video texture: {GetSdlError()}");
                            }

                            DrawImage(renderer, videoTexture, width, height);
                        }
                        else if (imageTexture == IntPtr.Zero)
                        {
                            DrawTestPattern(renderer, width, height, cutoff);
                        }
                        else
                        {
                            DrawImage(renderer, imageTexture, width, height);
                        }

                        SDL_RenderPresent(renderer);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(eventBuffer);
                }
            }
            finally
            {
                if (imageTexture != IntPtr.Zero)
                {
                    SDL_DestroyTexture(imageTexture);
                }

                if (videoTexture != IntPtr.Zero)
                {
                    SDL_DestroyTexture(videoTexture);
                }

                if (videoFramePin.IsAllocated)
                {
                    videoFramePin.Free();
                }

                StopVideoDecoder(videoDecoder);
            }
        }
        finally
        {
            SDL_DestroyRenderer(renderer);
        }
    }
    finally
    {
        SDL_DestroyWindow(window);
    }
}
finally
{
    SDL_Quit();
}

static int GetDisplayArgument(string[] args)
{
    var display = 1;
    for (var index = 0; index < args.Length; index++)
    {
        if (args[index] == "--display" && index + 1 < args.Length && int.TryParse(args[index + 1], out var parsed))
        {
            display = parsed;
            index++;
        }
    }

    return display;
}

static byte GetCutoffArgument(string[] args)
{
    for (var index = 0; index < args.Length; index++)
    {
        if (args[index] == "--cutoff" && index + 1 < args.Length && int.TryParse(args[index + 1], out var parsed))
        {
            if (parsed is < 0 or > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(args), "--cutoff must be between 0 and 255.");
            }

            return (byte)parsed;
        }
    }

    return 0;
}

static string? GetImageArgument(string[] args)
{
    for (var index = 0; index < args.Length; index++)
    {
        if (args[index] == "--image" && index + 1 < args.Length)
        {
            var imagePath = Path.GetFullPath(args[index + 1]);
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Image file was not found.", imagePath);
            }

            if (!string.Equals(Path.GetExtension(imagePath), ".bmp", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("--image accepts BMP only. Use scripts/show-keyed-image.sh for PNG/JPEG/WebP input.");
            }

            return imagePath;
        }
    }

    return null;
}

static string? GetVideoArgument(string[] args)
{
    for (var index = 0; index < args.Length; index++)
    {
        if (args[index] == "--video" && index + 1 < args.Length)
        {
            var videoPath = Path.GetFullPath(args[index + 1]);
            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException("Video file was not found.", videoPath);
            }

            return videoPath;
        }
    }

    return null;
}

static Process StartVideoDecoder(string videoPath)
{
    var startInfo = new ProcessStartInfo("ffmpeg")
    {
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    startInfo.ArgumentList.Add("-hide_banner");
    startInfo.ArgumentList.Add("-loglevel");
    startInfo.ArgumentList.Add("error");
    startInfo.ArgumentList.Add("-stream_loop");
    startInfo.ArgumentList.Add("-1");
    startInfo.ArgumentList.Add("-re");
    startInfo.ArgumentList.Add("-i");
    startInfo.ArgumentList.Add(videoPath);
    startInfo.ArgumentList.Add("-an");
    startInfo.ArgumentList.Add("-vf");
    startInfo.ArgumentList.Add($"scale={VideoWidth}:{VideoHeight}:force_original_aspect_ratio=decrease,pad={VideoWidth}:{VideoHeight}:(ow-iw)/2:(oh-ih)/2:color=black");
    startInfo.ArgumentList.Add("-f");
    startInfo.ArgumentList.Add("rawvideo");
    startInfo.ArgumentList.Add("-pix_fmt");
    startInfo.ArgumentList.Add("rgb24");
    startInfo.ArgumentList.Add("pipe:1");

    return Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start ffmpeg video decoder.");
}

static bool ReadNextVideoFrame(Stream stream, byte[] buffer)
{
    var offset = 0;
    while (offset < buffer.Length)
    {
        var count = stream.Read(buffer, offset, buffer.Length - offset);
        if (count == 0)
        {
            return false;
        }

        offset += count;
    }

    return true;
}

static void StopVideoDecoder(Process? videoDecoder)
{
    if (videoDecoder is null)
    {
        return;
    }

    try
    {
        if (!videoDecoder.HasExited)
        {
            videoDecoder.Kill(entireProcessTree: true);
            videoDecoder.WaitForExit(1000);
        }
    }
    finally
    {
        videoDecoder.Dispose();
    }
}

static IntPtr LoadBmpTexture(IntPtr renderer, string imagePath)
{
    var surface = SDL_LoadBMP(imagePath);
    if (surface == IntPtr.Zero)
    {
        throw new InvalidOperationException($"Could not load image: {GetSdlError()}");
    }

    try
    {
        var texture = SDL_CreateTextureFromSurface(renderer, surface);
        if (texture == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Could not prepare image texture: {GetSdlError()}");
        }

        return texture;
    }
    finally
    {
        SDL_FreeSurface(surface);
    }
}

static void DrawImage(IntPtr renderer, IntPtr texture, int width, int height)
{
    SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
    SDL_RenderClear(renderer);
    var destination = new SDL_Rect { x = 0, y = 0, w = width, h = height };
    if (SDL_RenderCopy(renderer, texture, IntPtr.Zero, ref destination) != 0)
    {
        throw new InvalidOperationException($"Could not render image: {GetSdlError()}");
    }
}

static void DrawTestPattern(IntPtr renderer, int width, int height, byte cutoff)
{
    var side = Math.Min(width, height);
    var left = (width - side) / 2;
    var top = (height - side) / 2;
    var centerX = left + side / 2;
    var centerY = top + side / 2;
    var radius = side * 9 / 20;

    SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
    SDL_RenderClear(renderer);

    // Each ring is a fixed input brightness.  Rings at or below cutoff are
    // deliberately left black, so the corresponding LEDs receive RGB(0,0,0).
    foreach (var (fraction, brightness) in new[]
             {
                 (1.0, 255), (0.80, 192), (0.60, 128), (0.40, 64), (0.20, 32)
             })
    {
        if (brightness <= cutoff)
        {
            continue;
        }

        SDL_SetRenderDrawColor(renderer, brightness, brightness, brightness, 255);
        var ringRadius = (int)(radius * fraction);
        for (var angle = 0; angle < 360; angle++)
        {
            var radians = angle * Math.PI / 180.0;
            var x = centerX + (int)(Math.Cos(radians) * ringRadius);
            var y = centerY + (int)(Math.Sin(radians) * ringRadius);
            SDL_RenderDrawPoint(renderer, x, y);
        }
    }

    DrawLineIfAboveCutoff(renderer, 255, 0, 0, cutoff, centerX, centerY, centerX, centerY - radius);
    DrawLineIfAboveCutoff(renderer, 0, 255, 0, cutoff, centerX, centerY, centerX + radius, centerY);
    DrawLineIfAboveCutoff(renderer, 0, 0, 255, cutoff, centerX, centerY, centerX, centerY + radius);
    DrawLineIfAboveCutoff(renderer, 255, 255, 0, cutoff, centerX, centerY, centerX - radius, centerY);
}

static void DrawLineIfAboveCutoff(IntPtr renderer, byte red, byte green, byte blue, byte cutoff, int x1, int y1, int x2, int y2)
{
    var brightness = Math.Max(red, Math.Max(green, blue));
    if (brightness <= cutoff)
    {
        return;
    }

    SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
    SDL_RenderDrawLine(renderer, x1, y1, x2, y2);
}

static string GetSdlError() => Marshal.PtrToStringUTF8(SDL_GetError()) ?? "unknown SDL error";

static int SDL_Init(uint flags) => Native.SDL_Init(flags);
static void SDL_Quit() => Native.SDL_Quit();
static int SDL_GetNumVideoDisplays() => Native.SDL_GetNumVideoDisplays();
static IntPtr SDL_GetDisplayName(int index) => Native.SDL_GetDisplayName(index);
static int SDL_GetDisplayBounds(int index, out SDL_Rect rect) => Native.SDL_GetDisplayBounds(index, out rect);
static IntPtr SDL_CreateWindow(string title, int x, int y, int width, int height, uint flags) => Native.SDL_CreateWindow(title, x, y, width, height, flags);
static void SDL_DestroyWindow(IntPtr window) => Native.SDL_DestroyWindow(window);
static IntPtr SDL_CreateRenderer(IntPtr window, int index, uint flags) => Native.SDL_CreateRenderer(window, index, flags);
static void SDL_DestroyRenderer(IntPtr renderer) => Native.SDL_DestroyRenderer(renderer);
static int SDL_GetRendererOutputSize(IntPtr renderer, out int width, out int height) => Native.SDL_GetRendererOutputSize(renderer, out width, out height);
static IntPtr SDL_LoadBMP(string file) => Native.SDL_LoadBMP(file);
static void SDL_FreeSurface(IntPtr surface) => Native.SDL_FreeSurface(surface);
static IntPtr SDL_CreateTextureFromSurface(IntPtr renderer, IntPtr surface) => Native.SDL_CreateTextureFromSurface(renderer, surface);
static IntPtr SDL_CreateTexture(IntPtr renderer, uint format, int access, int width, int height) => Native.SDL_CreateTexture(renderer, format, access, width, height);
static void SDL_DestroyTexture(IntPtr texture) => Native.SDL_DestroyTexture(texture);
static int SDL_SetRenderDrawColor(IntPtr renderer, int r, int g, int b, int a) => Native.SDL_SetRenderDrawColor(renderer, (byte)r, (byte)g, (byte)b, (byte)a);
static int SDL_RenderClear(IntPtr renderer) => Native.SDL_RenderClear(renderer);
static int SDL_RenderDrawLine(IntPtr renderer, int x1, int y1, int x2, int y2) => Native.SDL_RenderDrawLine(renderer, x1, y1, x2, y2);
static int SDL_RenderDrawPoint(IntPtr renderer, int x, int y) => Native.SDL_RenderDrawPoint(renderer, x, y);
static int SDL_UpdateTexture(IntPtr texture, IntPtr rectangle, IntPtr pixels, int pitch) => Native.SDL_UpdateTexture(texture, rectangle, pixels, pitch);
static int SDL_RenderCopy(IntPtr renderer, IntPtr texture, IntPtr source, ref SDL_Rect destination) => Native.SDL_RenderCopy(renderer, texture, source, ref destination);
static void SDL_RenderPresent(IntPtr renderer) => Native.SDL_RenderPresent(renderer);
static int SDL_PollEvent(IntPtr sdlEvent) => Native.SDL_PollEvent(sdlEvent);
static IntPtr SDL_GetError() => Native.SDL_GetError();

[StructLayout(LayoutKind.Sequential)]
struct SDL_Rect
{
    public int x;
    public int y;
    public int w;
    public int h;
}

static partial class Native
{
    [LibraryImport("SDL2-2.0", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int SDL_Init(uint flags);

    [LibraryImport("SDL2-2.0")]
    internal static partial void SDL_Quit();

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_GetNumVideoDisplays();

    [LibraryImport("SDL2-2.0")]
    internal static partial IntPtr SDL_GetDisplayName(int displayIndex);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_GetDisplayBounds(int displayIndex, out SDL_Rect rect);

    [LibraryImport("SDL2-2.0", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr SDL_CreateWindow(string title, int x, int y, int w, int h, uint flags);

    [LibraryImport("SDL2-2.0")]
    internal static partial void SDL_DestroyWindow(IntPtr window);

    [LibraryImport("SDL2-2.0")]
    internal static partial IntPtr SDL_CreateRenderer(IntPtr window, int index, uint flags);

    [LibraryImport("SDL2-2.0")]
    internal static partial void SDL_DestroyRenderer(IntPtr renderer);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_GetRendererOutputSize(IntPtr renderer, out int width, out int height);

    [LibraryImport("SDL2-2.0", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr SDL_LoadBMP(string file);

    [LibraryImport("SDL2-2.0")]
    internal static partial void SDL_FreeSurface(IntPtr surface);

    [LibraryImport("SDL2-2.0")]
    internal static partial IntPtr SDL_CreateTextureFromSurface(IntPtr renderer, IntPtr surface);

    [LibraryImport("SDL2-2.0")]
    internal static partial IntPtr SDL_CreateTexture(IntPtr renderer, uint format, int access, int w, int h);

    [LibraryImport("SDL2-2.0")]
    internal static partial void SDL_DestroyTexture(IntPtr texture);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_SetRenderDrawColor(IntPtr renderer, byte r, byte g, byte b, byte a);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_RenderClear(IntPtr renderer);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_RenderDrawLine(IntPtr renderer, int x1, int y1, int x2, int y2);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_RenderDrawPoint(IntPtr renderer, int x, int y);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_UpdateTexture(IntPtr texture, IntPtr rect, IntPtr pixels, int pitch);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_RenderCopy(IntPtr renderer, IntPtr texture, IntPtr source, ref SDL_Rect destination);

    [LibraryImport("SDL2-2.0")]
    internal static partial void SDL_RenderPresent(IntPtr renderer);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_PollEvent(IntPtr sdlEvent);

    [LibraryImport("SDL2-2.0")]
    internal static partial IntPtr SDL_GetError();
}
