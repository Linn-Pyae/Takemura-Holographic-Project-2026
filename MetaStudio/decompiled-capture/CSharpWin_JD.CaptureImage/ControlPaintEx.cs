using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CSharpWin_JD.CaptureImage;

public sealed class ControlPaintEx
{
	public static void DrawCheckedFlag(Graphics graphics, Rectangle rect, Color color)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		PointF[] array = new PointF[3]
		{
			new PointF((float)rect.X + (float)rect.Width / 4.5f, (float)rect.Y + (float)rect.Height / 2.5f),
			new PointF((float)rect.X + (float)rect.Width / 2.5f, (float)rect.Bottom - (float)rect.Height / 3f),
			new PointF((float)rect.Right - (float)rect.Width / 4f, (float)rect.Y + (float)rect.Height / 4.5f)
		};
		Pen val = new Pen(color, 2f);
		try
		{
			graphics.DrawLines(val, array);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void DrawGlass(Graphics g, RectangleF glassRect, int alphaCenter, int alphaSurround)
	{
		DrawGlass(g, glassRect, Color.White, alphaCenter, alphaSurround);
	}

	public static void DrawGlass(Graphics g, RectangleF glassRect, Color glassColor, int alphaCenter, int alphaSurround)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		try
		{
			val.AddEllipse(glassRect);
			PathGradientBrush val2 = new PathGradientBrush(val);
			try
			{
				val2.CenterColor = Color.FromArgb(alphaCenter, glassColor);
				val2.SurroundColors = new Color[1] { Color.FromArgb(alphaSurround, glassColor) };
				val2.CenterPoint = new PointF(glassRect.X + glassRect.Width / 2f, glassRect.Y + glassRect.Height / 2f);
				g.FillPath((Brush)(object)val2, val);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		DrawBackgroundImage(g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, Point.Empty, (RightToLeft)0);
	}

	public static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		DrawBackgroundImage(g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, (RightToLeft)0);
	}

	public static void DrawBackgroundImage(Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Invalid comparison between Unknown and I4
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Invalid comparison between Unknown and I4
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Invalid comparison between Unknown and I4
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Invalid comparison between Unknown and I4
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if ((int)backgroundImageLayout == 1)
		{
			TextureBrush val = new TextureBrush(backgroundImage, (WrapMode)0);
			try
			{
				if (scrollOffset != Point.Empty)
				{
					Matrix transform = val.Transform;
					transform.Translate((float)scrollOffset.X, (float)scrollOffset.Y);
					val.Transform = transform;
				}
				g.FillRectangle((Brush)(object)val, clipRect);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		Rectangle rectangle = CalculateBackgroundImageRectangle(bounds, backgroundImage, backgroundImageLayout);
		if ((int)rightToLeft == 1 && (int)backgroundImageLayout == 0)
		{
			rectangle.X += clipRect.Width - rectangle.Width;
		}
		SolidBrush val2 = new SolidBrush(backColor);
		try
		{
			g.FillRectangle((Brush)(object)val2, clipRect);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		if (!clipRect.Contains(rectangle))
		{
			if ((int)backgroundImageLayout == 3 || (int)backgroundImageLayout == 4)
			{
				rectangle.Intersect(clipRect);
				g.DrawImage(backgroundImage, rectangle);
			}
			else if ((int)backgroundImageLayout == 0)
			{
				rectangle.Offset(clipRect.Location);
				Rectangle rectangle2 = rectangle;
				rectangle2.Intersect(clipRect);
				Rectangle rectangle3 = new Rectangle(Point.Empty, rectangle2.Size);
				g.DrawImage(backgroundImage, rectangle2, rectangle3.X, rectangle3.Y, rectangle3.Width, rectangle3.Height, (GraphicsUnit)2);
			}
			else
			{
				Rectangle rectangle4 = rectangle;
				rectangle4.Intersect(clipRect);
				Rectangle rectangle5 = new Rectangle(new Point(rectangle4.X - rectangle.X, rectangle4.Y - rectangle.Y), rectangle4.Size);
				g.DrawImage(backgroundImage, rectangle4, rectangle5.X, rectangle5.Y, rectangle5.Width, rectangle5.Height, (GraphicsUnit)2);
			}
		}
		else
		{
			ImageAttributes val3 = new ImageAttributes();
			val3.SetWrapMode((WrapMode)3);
			g.DrawImage(backgroundImage, rectangle, 0, 0, backgroundImage.Width, backgroundImage.Height, (GraphicsUnit)2, val3);
			val3.Dispose();
		}
	}

	internal static Rectangle CalculateBackgroundImageRectangle(Rectangle bounds, Image backgroundImage, ImageLayout imageLayout)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected I4, but got Unknown
		Rectangle result = bounds;
		if (backgroundImage != null)
		{
			switch ((int)imageLayout)
			{
			case 0:
				result.Size = backgroundImage.Size;
				return result;
			case 1:
				return result;
			case 2:
			{
				result.Size = backgroundImage.Size;
				Size size2 = bounds.Size;
				if (size2.Width > result.Width)
				{
					result.X = (size2.Width - result.Width) / 2;
				}
				if (size2.Height > result.Height)
				{
					result.Y = (size2.Height - result.Height) / 2;
				}
				return result;
			}
			case 3:
				result.Size = bounds.Size;
				return result;
			case 4:
			{
				Size size = backgroundImage.Size;
				float num = (float)bounds.Width / (float)size.Width;
				float num2 = (float)bounds.Height / (float)size.Height;
				if (num >= num2)
				{
					result.Height = bounds.Height;
					result.Width = (int)((double)((float)size.Width * num2) + 0.5);
					if (bounds.X >= 0)
					{
						result.X = (bounds.Width - result.Width) / 2;
					}
					return result;
				}
				result.Width = bounds.Width;
				result.Height = (int)((double)((float)size.Height * num) + 0.5);
				if (bounds.Y >= 0)
				{
					result.Y = (bounds.Height - result.Height) / 2;
				}
				return result;
			}
			}
		}
		return result;
	}
}
