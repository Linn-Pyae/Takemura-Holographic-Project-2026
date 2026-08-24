using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CSharpWin_JD.CaptureImage;

public class ToolStripRendererEx : ToolStripRenderer
{
	private const string MenuLogoString = "";

	private CaptureImageToolColorTable _colorTable;

	private static readonly int OffsetMargin = 24;

	protected virtual CaptureImageToolColorTable ColorTable
	{
		get
		{
			if (_colorTable == null)
			{
				_colorTable = new CaptureImageToolColorTable();
			}
			return _colorTable;
		}
	}

	public ToolStripRendererEx()
	{
	}

	public ToolStripRendererEx(CaptureImageToolColorTable colorTable)
	{
		_colorTable = colorTable;
	}

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		Color backColorNormal = ColorTable.BackColorNormal;
		ToolStrip toolStrip = e.ToolStrip;
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = (SmoothingMode)4;
		if (toolStrip is ToolStripDropDown)
		{
			RegionHelper.CreateRegion((Control)(object)e.ToolStrip, e.AffectedBounds);
			Rectangle affectedBounds = e.AffectedBounds;
			GraphicsPath val = GraphicsPathHelper.CreatePath(affectedBounds, 8, RoundStyle.All, correction: false);
			try
			{
				SolidBrush val2 = new SolidBrush(ColorTable.BackColorNormal);
				try
				{
					graphics.FillPath((Brush)(object)val2, val);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				Pen val3 = new Pen(ColorTable.BorderColor);
				try
				{
					graphics.DrawPath(val3, val);
					GraphicsPath val4 = GraphicsPathHelper.CreatePath(affectedBounds, 8, RoundStyle.All, correction: true);
					try
					{
						graphics.DrawPath(val3, val4);
						return;
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		LinearGradientMode mode = (LinearGradientMode)((int)e.ToolStrip.Orientation == 0);
		RenderBackgroundInternal(graphics, e.AffectedBounds, ColorTable.BackColorHover, ColorTable.BorderColor, ColorTable.BackColorNormal, RoundStyle.All, drawBorder: false, drawGlass: true, mode);
	}

	protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		ToolStripItem item = e.Item;
		ToolStripButton val = (ToolStripButton)(object)((item is ToolStripButton) ? item : null);
		if (val != null)
		{
			LinearGradientMode mode = (LinearGradientMode)((int)e.ToolStrip.Orientation == 0);
			Graphics graphics = e.Graphics;
			graphics.SmoothingMode = (SmoothingMode)4;
			Rectangle rectangle = new Rectangle(Point.Empty, ((ToolStripItem)val).Size);
			if (((ToolStripItem)val).BackgroundImage != null)
			{
				Rectangle clipRect = (((ToolStripItem)val).Selected ? ((ToolStripItem)val).ContentRectangle : rectangle);
				ControlPaintEx.DrawBackgroundImage(graphics, ((ToolStripItem)val).BackgroundImage, ColorTable.BackColorNormal, ((ToolStripItem)val).BackgroundImageLayout, rectangle, clipRect);
			}
			if ((int)val.CheckState != 0)
			{
				Color baseColor = ControlPaint.Light(ColorTable.BackColorHover);
				if (((ToolStripItem)val).Selected)
				{
					baseColor = ColorTable.BackColorHover;
				}
				if (((ToolStripItem)val).Pressed)
				{
					baseColor = ColorTable.BackColorPressed;
				}
				RenderBackgroundInternal(e.Graphics, rectangle, baseColor, ColorTable.BorderColor, ColorTable.BackColorNormal, RoundStyle.All, drawBorder: true, drawGlass: true, mode);
				return;
			}
			if (((ToolStripItem)val).Selected)
			{
				Color baseColor = ColorTable.BackColorHover;
				if (((ToolStripItem)val).Pressed)
				{
					baseColor = ColorTable.BackColorPressed;
				}
				RenderBackgroundInternal(graphics, rectangle, baseColor, ColorTable.BorderColor, ColorTable.BackColorNormal, RoundStyle.All, drawBorder: true, drawGlass: true, mode);
				return;
			}
			if (e.ToolStrip is ToolStripOverflow)
			{
				Brush val2 = (Brush)new SolidBrush(ColorTable.BackColorNormal);
				try
				{
					graphics.FillRectangle(val2, rectangle);
					return;
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
		}
		((ToolStripRenderer)this).OnRenderButtonBackground(e);
	}

	protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		Rectangle contentRectangle = ((ToolStripItemRenderEventArgs)e).Item.ContentRectangle;
		if (((ToolStripItemRenderEventArgs)e).ToolStrip is ToolStripDropDown)
		{
			if ((int)((ToolStripItemRenderEventArgs)e).Item.RightToLeft != 1)
			{
				contentRectangle.X += OffsetMargin + 4;
			}
			contentRectangle.Width -= OffsetMargin + 8;
		}
		RenderSeparatorLine(((ToolStripItemRenderEventArgs)e).Graphics, contentRectangle, ColorTable.BackColorPressed, ColorTable.BackColorNormal, SystemColors.ControlLightLight, e.Vertical);
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		if (e.Item.Enabled)
		{
			Graphics graphics = e.Graphics;
			Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
			graphics.SmoothingMode = (SmoothingMode)4;
			if ((int)e.Item.RightToLeft == 1)
			{
				rect.X += 4;
			}
			else
			{
				rect.X += OffsetMargin + 4;
			}
			rect.Width -= OffsetMargin + 8;
			rect.Height--;
			if (e.Item.Selected)
			{
				RenderBackgroundInternal(graphics, rect, ColorTable.BackColorHover, ColorTable.BorderColor, ColorTable.BackColorNormal, RoundStyle.All, drawBorder: true, drawGlass: true, (LinearGradientMode)1);
			}
			else
			{
				((ToolStripRenderer)this).OnRenderMenuItemBackground(e);
			}
		}
	}

	protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Expected O, but got Unknown
		if (e.ToolStrip is ToolStripDropDownMenu)
		{
			Rectangle affectedBounds = e.AffectedBounds;
			Graphics graphics = e.Graphics;
			affectedBounds.Width = OffsetMargin;
			if ((int)((Control)e.ToolStrip).RightToLeft == 1)
			{
				affectedBounds.X -= 2;
			}
			else
			{
				affectedBounds.X += 2;
			}
			affectedBounds.Y++;
			affectedBounds.Height -= 2;
			graphics.SmoothingMode = (SmoothingMode)4;
			LinearGradientBrush val = new LinearGradientBrush(affectedBounds, ColorTable.BackColorHover, Color.White, 90f);
			try
			{
				Blend val2 = new Blend();
				val2.Positions = new float[3] { 0f, 0.2f, 1f };
				val2.Factors = new float[3] { 0f, 0.1f, 0.9f };
				val.Blend = val2;
				affectedBounds.Y++;
				affectedBounds.Height -= 2;
				GraphicsPath val3 = GraphicsPathHelper.CreatePath(affectedBounds, 8, RoundStyle.All, correction: false);
				try
				{
					graphics.FillPath((Brush)(object)val, val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			graphics.TextRenderingHint = (TextRenderingHint)4;
			StringFormat val4 = new StringFormat((StringFormatFlags)4096);
			Font val5 = new Font(((Control)e.ToolStrip).Font.FontFamily, 11f, (FontStyle)1);
			val4.Alignment = (StringAlignment)0;
			val4.LineAlignment = (StringAlignment)1;
			val4.Trimming = (StringTrimming)3;
			graphics.TranslateTransform((float)affectedBounds.X, (float)affectedBounds.Bottom);
			graphics.RotateTransform(270f);
			if (!string.IsNullOrEmpty(""))
			{
				Rectangle rectangle = new Rectangle(affectedBounds.X, affectedBounds.Y, affectedBounds.Height, affectedBounds.Width);
				Brush val6 = (Brush)new SolidBrush(ColorTable.ForeColor);
				try
				{
					graphics.DrawString("", val5, val6, (RectangleF)rectangle, val4);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
			graphics.ResetTransform();
		}
		else
		{
			((ToolStripRenderer)this).OnRenderImageMargin(e);
		}
	}

	protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		Graphics graphics = ((ToolStripItemRenderEventArgs)e).Graphics;
		graphics.InterpolationMode = (InterpolationMode)6;
		if (((ToolStripItemRenderEventArgs)e).Item is ToolStripMenuItem)
		{
			ToolStripMenuItem val = (ToolStripMenuItem)((ToolStripItemRenderEventArgs)e).Item;
			if (!val.Checked)
			{
				Rectangle imageRectangle = e.ImageRectangle;
				if ((int)((ToolStripItemRenderEventArgs)e).Item.RightToLeft == 1)
				{
					imageRectangle.X -= OffsetMargin + 2;
				}
				else
				{
					imageRectangle.X += OffsetMargin + 2;
				}
				ToolStripItemImageRenderEventArgs e2 = new ToolStripItemImageRenderEventArgs(((ToolStripItemRenderEventArgs)e).Graphics, ((ToolStripItemRenderEventArgs)e).Item, e.Image, imageRectangle);
				((ToolStripRenderer)this).OnRenderItemImage(e2);
			}
		}
		else
		{
			((ToolStripRenderer)this).OnRenderItemImage(e);
		}
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		e.TextColor = ColorTable.ForeColor;
		if (!(((ToolStripItemRenderEventArgs)e).ToolStrip is MenuStrip) && ((ToolStripItemRenderEventArgs)e).Item is ToolStripMenuItem)
		{
			Rectangle textRectangle = e.TextRectangle;
			if ((int)((ToolStripItemRenderEventArgs)e).Item.RightToLeft == 1)
			{
				textRectangle.X -= 16;
			}
			else
			{
				textRectangle.X += 16;
			}
			e.TextRectangle = textRectangle;
		}
		((ToolStripRenderer)this).OnRenderItemText(e);
	}

	internal void RenderBackgroundInternal(Graphics g, Rectangle rect, Color baseColor, Color borderColor, Color innerBorderColor, RoundStyle style, bool drawBorder, bool drawGlass, LinearGradientMode mode)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		RenderBackgroundInternal(g, rect, baseColor, borderColor, innerBorderColor, style, 8, drawBorder, drawGlass, mode);
	}

	internal void RenderBackgroundInternal(Graphics g, Rectangle rect, Color baseColor, Color borderColor, Color innerBorderColor, RoundStyle style, int roundWidth, bool drawBorder, bool drawGlass, LinearGradientMode mode)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		RenderBackgroundInternal(g, rect, baseColor, borderColor, innerBorderColor, style, 8, 0.45f, drawBorder, drawGlass, mode);
	}

	internal void RenderBackgroundInternal(Graphics g, Rectangle rect, Color baseColor, Color borderColor, Color innerBorderColor, RoundStyle style, int roundWidth, float basePosition, bool drawBorder, bool drawGlass, LinearGradientMode mode)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Invalid comparison between Unknown and I4
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Invalid comparison between Unknown and I4
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Expected O, but got Unknown
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Invalid comparison between Unknown and I4
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Invalid comparison between Unknown and I4
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Expected O, but got Unknown
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Expected O, but got Unknown
		if (drawBorder)
		{
			rect.Width--;
			rect.Height--;
		}
		LinearGradientBrush val = new LinearGradientBrush(rect, Color.Transparent, Color.Transparent, mode);
		try
		{
			Color[] colors = new Color[4]
			{
				GetColor(baseColor, 0, 35, 24, 9),
				GetColor(baseColor, 0, 13, 8, 3),
				baseColor,
				GetColor(baseColor, 0, 68, 69, 54)
			};
			ColorBlend val2 = new ColorBlend();
			val2.Positions = new float[4]
			{
				0f,
				basePosition,
				basePosition + 0.05f,
				1f
			};
			val2.Colors = colors;
			val.InterpolationColors = val2;
			Pen val6;
			if (style != RoundStyle.None)
			{
				GraphicsPath val3 = GraphicsPathHelper.CreatePath(rect, roundWidth, style, correction: false);
				try
				{
					g.FillPath((Brush)(object)val, val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				if (baseColor.A > 80)
				{
					Rectangle rect2 = rect;
					if ((int)mode == 1)
					{
						rect2.Height = (int)((float)rect2.Height * basePosition);
					}
					else
					{
						rect2.Width = (int)((float)rect.Width * basePosition);
					}
					GraphicsPath val4 = GraphicsPathHelper.CreatePath(rect2, roundWidth, RoundStyle.Top, correction: false);
					try
					{
						SolidBrush val5 = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
						try
						{
							g.FillPath((Brush)(object)val5, val4);
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				if (drawGlass)
				{
					RectangleF glassRect = rect;
					if ((int)mode == 1)
					{
						glassRect.Y = (float)rect.Y + (float)rect.Height * basePosition;
						glassRect.Height = ((float)rect.Height - (float)rect.Height * basePosition) * 2f;
					}
					else
					{
						glassRect.X = (float)rect.X + (float)rect.Width * basePosition;
						glassRect.Width = ((float)rect.Width - (float)rect.Width * basePosition) * 2f;
					}
					ControlPaintEx.DrawGlass(g, glassRect, 170, 0);
				}
				if (!drawBorder)
				{
					return;
				}
				val3 = GraphicsPathHelper.CreatePath(rect, roundWidth, style, correction: false);
				try
				{
					val6 = new Pen(borderColor);
					try
					{
						g.DrawPath(val6, val3);
					}
					finally
					{
						((IDisposable)val6)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				rect.Inflate(-1, -1);
				val3 = GraphicsPathHelper.CreatePath(rect, roundWidth, style, correction: false);
				try
				{
					val6 = new Pen(innerBorderColor);
					try
					{
						g.DrawPath(val6, val3);
						return;
					}
					finally
					{
						((IDisposable)val6)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			g.FillRectangle((Brush)(object)val, rect);
			if (baseColor.A > 80)
			{
				Rectangle rect2 = rect;
				if ((int)mode == 1)
				{
					rect2.Height = (int)((float)rect2.Height * basePosition);
				}
				else
				{
					rect2.Width = (int)((float)rect.Width * basePosition);
				}
				SolidBrush val5 = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
				try
				{
					g.FillRectangle((Brush)(object)val5, rect2);
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
			}
			if (drawGlass)
			{
				RectangleF glassRect = rect;
				if ((int)mode == 1)
				{
					glassRect.Y = (float)rect.Y + (float)rect.Height * basePosition;
					glassRect.Height = ((float)rect.Height - (float)rect.Height * basePosition) * 2f;
				}
				else
				{
					glassRect.X = (float)rect.X + (float)rect.Width * basePosition;
					glassRect.Width = ((float)rect.Width - (float)rect.Width * basePosition) * 2f;
				}
				ControlPaintEx.DrawGlass(g, glassRect, 200, 0);
			}
			if (!drawBorder)
			{
				return;
			}
			val6 = new Pen(borderColor);
			try
			{
				g.DrawRectangle(val6, rect);
			}
			finally
			{
				((IDisposable)val6)?.Dispose();
			}
			rect.Inflate(-1, -1);
			val6 = new Pen(innerBorderColor);
			try
			{
				g.DrawRectangle(val6, rect);
			}
			finally
			{
				((IDisposable)val6)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal void RenderSeparatorLine(Graphics g, Rectangle rect, Color baseColor, Color backColor, Color shadowColor, bool vertical)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		float num = ((!vertical) ? 180f : 90f);
		LinearGradientBrush val = new LinearGradientBrush(rect, baseColor, backColor, num);
		try
		{
			Blend val2 = new Blend();
			val2.Positions = new float[5] { 0f, 0.3f, 0.5f, 0.7f, 1f };
			val2.Factors = new float[5] { 1f, 0.3f, 0f, 0.3f, 1f };
			val.Blend = val2;
			Pen val3 = new Pen((Brush)(object)val);
			try
			{
				if (vertical)
				{
					g.DrawLine(val3, rect.X, rect.Y, rect.X, rect.Bottom);
				}
				else
				{
					g.DrawLine(val3, rect.X, rect.Y, rect.Right, rect.Y);
				}
				val.LinearColors = new Color[2] { shadowColor, backColor };
				val3.Brush = (Brush)(object)val;
				if (vertical)
				{
					g.DrawLine(val3, rect.X + 1, rect.Y, rect.X + 1, rect.Bottom);
				}
				else
				{
					g.DrawLine(val3, rect.X, rect.Y + 1, rect.Right, rect.Y + 1);
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private Color GetColor(Color colorBase, int a, int r, int g, int b)
	{
		int a2 = colorBase.A;
		int r2 = colorBase.R;
		int g2 = colorBase.G;
		int b2 = colorBase.B;
		a = ((a + a2 <= 255) ? Math.Max(0, a + a2) : 255);
		r = ((r + r2 <= 255) ? Math.Max(0, r + r2) : 255);
		g = ((g + g2 <= 255) ? Math.Max(0, g + g2) : 255);
		b = ((b + b2 <= 255) ? Math.Max(0, b + b2) : 255);
		return Color.FromArgb(a, r, g, b);
	}
}
