using System.Drawing;
using System.Drawing.Drawing2D;

namespace CSharpWin_JD.CaptureImage;

public static class GraphicsPathHelper
{
	public static GraphicsPath CreatePath(Rectangle rect, int radius, RoundStyle style, bool correction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		int num = (correction ? 1 : 0);
		switch (style)
		{
		case RoundStyle.None:
			val.AddRectangle(rect);
			break;
		case RoundStyle.All:
			val.AddArc(rect.X, rect.Y, radius, radius, 180f, 90f);
			val.AddArc(rect.Right - radius - num, rect.Y, radius, radius, 270f, 90f);
			val.AddArc(rect.Right - radius - num, rect.Bottom - radius - num, radius, radius, 0f, 90f);
			val.AddArc(rect.X, rect.Bottom - radius - num, radius, radius, 90f, 90f);
			break;
		case RoundStyle.Left:
			val.AddArc(rect.X, rect.Y, radius, radius, 180f, 90f);
			val.AddLine(rect.Right - num, rect.Y, rect.Right - num, rect.Bottom - num);
			val.AddArc(rect.X, rect.Bottom - radius - num, radius, radius, 90f, 90f);
			break;
		case RoundStyle.Right:
			val.AddArc(rect.Right - radius - num, rect.Y, radius, radius, 270f, 90f);
			val.AddArc(rect.Right - radius - num, rect.Bottom - radius - num, radius, radius, 0f, 90f);
			val.AddLine(rect.X, rect.Bottom - num, rect.X, rect.Y);
			break;
		case RoundStyle.Top:
			val.AddArc(rect.X, rect.Y, radius, radius, 180f, 90f);
			val.AddArc(rect.Right - radius - num, rect.Y, radius, radius, 270f, 90f);
			val.AddLine(rect.Right - num, rect.Bottom - num, rect.X, rect.Bottom - num);
			break;
		case RoundStyle.Bottom:
			val.AddArc(rect.Right - radius - num, rect.Bottom - radius - num, radius, radius, 0f, 90f);
			val.AddArc(rect.X, rect.Bottom - radius - num, radius, radius, 90f, 90f);
			val.AddLine(rect.X, rect.Y, rect.Right - num, rect.Y);
			break;
		}
		val.CloseFigure();
		return val;
	}
}
