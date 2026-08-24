using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CSharpWin_JD.CaptureImage;

internal class NativeMethods
{
	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;

		public Rectangle Rect => new Rectangle(Left, Top, Right - Left, Bottom - Top);

		public Size Size => new Size(Right - Left, Bottom - Top);

		public RECT(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public RECT(Rectangle rect)
		{
			Left = rect.Left;
			Top = rect.Top;
			Right = rect.Right;
			Bottom = rect.Bottom;
		}

		public static RECT FromXYWH(int x, int y, int width, int height)
		{
			return new RECT(x, y, x + width, y + height);
		}

		public static RECT FromRectangle(Rectangle rect)
		{
			return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}
	}

	public enum TernaryRasterOperations
	{
		SRCCOPY = 13369376,
		SRCPAINT = 15597702,
		SRCAND = 8913094,
		SRCINVERT = 6684742,
		SRCERASE = 4457256,
		NOTSRCCOPY = 3342344,
		NOTSRCERASE = 1114278,
		MERGECOPY = 12583114,
		MERGEPAINT = 12255782,
		PATCOPY = 15728673,
		PATPAINT = 16452105,
		PATINVERT = 5898313,
		DSTINVERT = 5570569,
		BLACKNESS = 66,
		WHITENESS = 16711778
	}

	public const int WS_EX_TRANSPARENT = 32;

	[DllImport("user32.dll")]
	public static extern bool ClipCursor(ref RECT lpRect);

	[DllImport("user32.dll")]
	public static extern IntPtr GetDesktopWindow();

	[DllImport("user32.dll")]
	public static extern IntPtr GetDC(IntPtr ptr);

	[DllImport("user32.dll")]
	public static extern int ReleaseDC(IntPtr hwnd, IntPtr hDC);

	[DllImport("gdi32.dll")]
	public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr LoadLibrary(string lpFileName);
}
