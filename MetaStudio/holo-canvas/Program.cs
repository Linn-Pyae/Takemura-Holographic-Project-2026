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

var display = GetDisplayArgument(args);

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

                    DrawTestPattern(renderer, width, height);
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

static void DrawTestPattern(IntPtr renderer, int width, int height)
{
    var side = Math.Min(width, height);
    var left = (width - side) / 2;
    var top = (height - side) / 2;
    var centerX = left + side / 2;
    var centerY = top + side / 2;
    var radius = side * 9 / 20;

    SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
    SDL_RenderClear(renderer);

    SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
    foreach (var fraction in new[] { 1.0, 2.0 / 3.0, 1.0 / 3.0 })
    {
        var ringRadius = (int)(radius * fraction);
        for (var angle = 0; angle < 360; angle++)
        {
            var radians = angle * Math.PI / 180.0;
            var x = centerX + (int)(Math.Cos(radians) * ringRadius);
            var y = centerY + (int)(Math.Sin(radians) * ringRadius);
            SDL_RenderDrawPoint(renderer, x, y);
        }
    }

    SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);
    SDL_RenderDrawLine(renderer, centerX, centerY, centerX, centerY - radius);
    SDL_SetRenderDrawColor(renderer, 0, 255, 0, 255);
    SDL_RenderDrawLine(renderer, centerX, centerY, centerX + radius, centerY);
    SDL_SetRenderDrawColor(renderer, 0, 0, 255, 255);
    SDL_RenderDrawLine(renderer, centerX, centerY, centerX, centerY + radius);
    SDL_SetRenderDrawColor(renderer, 255, 255, 0, 255);
    SDL_RenderDrawLine(renderer, centerX, centerY, centerX - radius, centerY);
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
static int SDL_SetRenderDrawColor(IntPtr renderer, int r, int g, int b, int a) => Native.SDL_SetRenderDrawColor(renderer, (byte)r, (byte)g, (byte)b, (byte)a);
static int SDL_RenderClear(IntPtr renderer) => Native.SDL_RenderClear(renderer);
static int SDL_RenderFillRect(IntPtr renderer, ref SDL_Rect rect) => Native.SDL_RenderFillRect(renderer, ref rect);
static int SDL_RenderDrawLine(IntPtr renderer, int x1, int y1, int x2, int y2) => Native.SDL_RenderDrawLine(renderer, x1, y1, x2, y2);
static int SDL_RenderDrawPoint(IntPtr renderer, int x, int y) => Native.SDL_RenderDrawPoint(renderer, x, y);
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

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_SetRenderDrawColor(IntPtr renderer, byte r, byte g, byte b, byte a);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_RenderClear(IntPtr renderer);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_RenderFillRect(IntPtr renderer, ref SDL_Rect rect);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_RenderDrawLine(IntPtr renderer, int x1, int y1, int x2, int y2);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_RenderDrawPoint(IntPtr renderer, int x, int y);

    [LibraryImport("SDL2-2.0")]
    internal static partial void SDL_RenderPresent(IntPtr renderer);

    [LibraryImport("SDL2-2.0")]
    internal static partial int SDL_PollEvent(IntPtr sdlEvent);

    [LibraryImport("SDL2-2.0")]
    internal static partial IntPtr SDL_GetError();
}
