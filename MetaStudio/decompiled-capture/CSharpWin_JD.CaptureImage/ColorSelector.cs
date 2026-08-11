using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CSharpWin_JD.CaptureImage.Properties;

namespace CSharpWin_JD.CaptureImage;

public class ColorSelector : UserControl
{
	private CaptureImageToolColorTable _colorTable;

	private static readonly Color InnerBorderColor = Color.FromArgb(200, 255, 255, 255);

	private static readonly object EventColorChanged = new object();

	private static readonly object EventFontSizeChanged = new object();

	private IContainer components = null;

	private Panel panelLeft;

	private ComboBox comboBoxFontSize;

	private Label labelFont;

	private Panel panelFill;

	private ColorLabel colorLabel16;

	private ColorLabel colorLabel8;

	private ColorLabel colorLabel15;

	private ColorLabel colorLabel7;

	private ColorLabel colorLabel14;

	private ColorLabel colorLabel6;

	private ColorLabel colorLabel13;

	private ColorLabel colorLabel5;

	private ColorLabel colorLabel12;

	private ColorLabel colorLabel4;

	private ColorLabel colorLabel11;

	private ColorLabel colorLabel3;

	private ColorLabel colorLabel10;

	private ColorLabel colorLabel2;

	private ColorLabel colorLabel9;

	private ColorLabel colorLabelSelected;

	private ColorLabel colorLabel1;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public CaptureImageToolColorTable ColorTable
	{
		get
		{
			if (_colorTable == null)
			{
				_colorTable = new CaptureImageToolColorTable();
			}
			return _colorTable;
		}
		set
		{
			_colorTable = value;
			((Control)this).Invalidate();
			SetColorLabelBorderColor(ColorTable.BorderColor);
		}
	}

	[Browsable(false)]
	public Color SelectedColor => ((Control)colorLabelSelected).BackColor;

	[Browsable(false)]
	public int FontSize => int.Parse(((Control)comboBoxFontSize).Text);

	public event EventHandler ColorChanged
	{
		add
		{
			((Component)this).Events.AddHandler(EventColorChanged, value);
		}
		remove
		{
			((Component)this).Events.RemoveHandler(EventColorChanged, value);
		}
	}

	public event EventHandler FontSizeChanged
	{
		add
		{
			((Component)this).Events.AddHandler(EventFontSizeChanged, value);
		}
		remove
		{
			((Component)this).Events.RemoveHandler(EventFontSizeChanged, value);
		}
	}

	public ColorSelector()
	{
		InitializeComponent();
		Init();
		((Control)this).DoubleBuffered = true;
		((Control)this).ResizeRedraw = true;
	}

	public void Reset()
	{
		((Control)colorLabelSelected).BackColor = Color.Red;
		((Control)comboBoxFontSize).Text = "12";
		((Control)panelLeft).Visible = false;
		((Control)this).Width = 189;
	}

	public void ChangeToFontStyle()
	{
		((Control)colorLabelSelected).BackColor = Color.Red;
		((Control)comboBoxFontSize).Text = "12";
		((Control)panelLeft).Visible = true;
		((Control)this).Width = 268;
	}

	protected virtual void OnColorChanged(EventArgs e)
	{
		if (((Component)this).Events[EventColorChanged] is EventHandler eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnFontSizeChanged(EventArgs e)
	{
		if (((Component)this).Events[EventFontSizeChanged] is EventHandler eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected override void OnCreateControl()
	{
		((UserControl)this).OnCreateControl();
		SetRegion();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		((Control)this).OnSizeChanged(e);
		SetRegion();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		((Control)this).OnPaint(e);
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = (SmoothingMode)4;
		RenderBackgroundInternal(graphics, ((Control)this).ClientRectangle, ColorTable.BackColorHover, ColorTable.BorderColor, InnerBorderColor, RoundStyle.All, drawBorder: true, drawGlass: true, (LinearGradientMode)1);
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

	private void SetRegion()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		GraphicsPath val = GraphicsPathHelper.CreatePath(((Control)this).ClientRectangle, 8, RoundStyle.All, correction: false);
		try
		{
			if (((Control)this).Region != null)
			{
				((Control)this).Region.Dispose();
			}
			((Control)this).Region = new Region(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void Init()
	{
		((Control)panelLeft).Visible = false;
		((Control)this).Width = 189;
		((Control)comboBoxFontSize).Text = "12";
		((Control)colorLabelSelected).BackColor = Color.Red;
		((Control)colorLabel1).BackColor = Color.Black;
		((Control)colorLabel2).BackColor = Color.FromArgb(153, 153, 153);
		((Control)colorLabel3).BackColor = Color.FromArgb(128, 0, 0);
		((Control)colorLabel4).BackColor = Color.FromArgb(128, 128, 0);
		((Control)colorLabel5).BackColor = Color.FromArgb(0, 128, 0);
		((Control)colorLabel6).BackColor = Color.FromArgb(0, 0, 128);
		((Control)colorLabel7).BackColor = Color.FromArgb(128, 0, 128);
		((Control)colorLabel8).BackColor = Color.FromArgb(0, 128, 128);
		((Control)colorLabel9).BackColor = Color.White;
		((Control)colorLabel10).BackColor = Color.FromArgb(192, 192, 192);
		((Control)colorLabel11).BackColor = Color.FromArgb(255, 0, 0);
		((Control)colorLabel12).BackColor = Color.FromArgb(255, 255, 0);
		((Control)colorLabel13).BackColor = Color.FromArgb(0, 255, 0);
		((Control)colorLabel14).BackColor = Color.FromArgb(0, 0, 255);
		((Control)colorLabel15).BackColor = Color.FromArgb(255, 0, 255);
		((Control)colorLabel16).BackColor = Color.FromArgb(0, 255, 255);
		((Control)colorLabel1).Click += ColorLabelClick;
		((Control)colorLabel2).Click += ColorLabelClick;
		((Control)colorLabel3).Click += ColorLabelClick;
		((Control)colorLabel4).Click += ColorLabelClick;
		((Control)colorLabel5).Click += ColorLabelClick;
		((Control)colorLabel6).Click += ColorLabelClick;
		((Control)colorLabel7).Click += ColorLabelClick;
		((Control)colorLabel8).Click += ColorLabelClick;
		((Control)colorLabel9).Click += ColorLabelClick;
		((Control)colorLabel10).Click += ColorLabelClick;
		((Control)colorLabel11).Click += ColorLabelClick;
		((Control)colorLabel12).Click += ColorLabelClick;
		((Control)colorLabel13).Click += ColorLabelClick;
		((Control)colorLabel14).Click += ColorLabelClick;
		((Control)colorLabel15).Click += ColorLabelClick;
		((Control)colorLabel16).Click += ColorLabelClick;
		comboBoxFontSize.SelectedIndexChanged += ComboBoxFontSizeSelectedIndexChanged;
	}

	private void ComboBoxFontSizeSelectedIndexChanged(object sender, EventArgs e)
	{
		OnFontSizeChanged(e);
	}

	private void ColorLabelClick(object sender, EventArgs e)
	{
		Control val = (Control)((sender is Control) ? sender : null);
		((Control)colorLabelSelected).BackColor = val.BackColor;
		OnColorChanged(e);
	}

	private void SetColorLabelBorderColor(Color borderColor)
	{
		colorLabel1.BorderColor = borderColor;
		colorLabel2.BorderColor = borderColor;
		colorLabel3.BorderColor = borderColor;
		colorLabel4.BorderColor = borderColor;
		colorLabel5.BorderColor = borderColor;
		colorLabel6.BorderColor = borderColor;
		colorLabel7.BorderColor = borderColor;
		colorLabel8.BorderColor = borderColor;
		colorLabel9.BorderColor = borderColor;
		colorLabel10.BorderColor = borderColor;
		colorLabel11.BorderColor = borderColor;
		colorLabel12.BorderColor = borderColor;
		colorLabel13.BorderColor = borderColor;
		colorLabel14.BorderColor = borderColor;
		colorLabel15.BorderColor = borderColor;
		colorLabel16.BorderColor = borderColor;
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

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((ContainerControl)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b3e: Unknown result type (might be due to invalid IL or missing references)
		panelLeft = new Panel();
		comboBoxFontSize = new ComboBox();
		labelFont = new Label();
		panelFill = new Panel();
		colorLabel16 = new ColorLabel();
		colorLabel8 = new ColorLabel();
		colorLabel15 = new ColorLabel();
		colorLabel7 = new ColorLabel();
		colorLabel14 = new ColorLabel();
		colorLabel6 = new ColorLabel();
		colorLabel13 = new ColorLabel();
		colorLabel5 = new ColorLabel();
		colorLabel12 = new ColorLabel();
		colorLabel4 = new ColorLabel();
		colorLabel11 = new ColorLabel();
		colorLabel3 = new ColorLabel();
		colorLabel10 = new ColorLabel();
		colorLabel2 = new ColorLabel();
		colorLabel9 = new ColorLabel();
		colorLabelSelected = new ColorLabel();
		colorLabel1 = new ColorLabel();
		((Control)panelLeft).SuspendLayout();
		((Control)panelFill).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)panelLeft).BackColor = Color.Transparent;
		((Control)panelLeft).Controls.Add((Control)(object)comboBoxFontSize);
		((Control)panelLeft).Controls.Add((Control)(object)labelFont);
		((Control)panelLeft).Dock = (DockStyle)3;
		((Control)panelLeft).Location = new Point(2, 2);
		((Control)panelLeft).Name = "panelLeft";
		((Control)panelLeft).Size = new Size(79, 34);
		((Control)panelLeft).TabIndex = 0;
		((Control)panelLeft).Visible = false;
		comboBoxFontSize.DropDownStyle = (ComboBoxStyle)2;
		((ListControl)comboBoxFontSize).FormattingEnabled = true;
		comboBoxFontSize.Items.AddRange(new object[9] { "8", "9", "10", "11", "12", "14", "16", "18", "20" });
		((Control)comboBoxFontSize).Location = new Point(23, 6);
		((Control)comboBoxFontSize).Name = "comboBoxFontSize";
		((Control)comboBoxFontSize).Size = new Size(53, 20);
		((Control)comboBoxFontSize).TabIndex = 2;
		((Control)labelFont).Dock = (DockStyle)3;
		labelFont.Image = (Image)(object)Resources.Text;
		labelFont.ImageAlign = (ContentAlignment)16;
		((Control)labelFont).Location = new Point(0, 0);
		((Control)labelFont).Name = "labelFont";
		((Control)labelFont).Size = new Size(20, 34);
		((Control)labelFont).TabIndex = 1;
		((Control)panelFill).BackColor = Color.Transparent;
		((Control)panelFill).Controls.Add((Control)(object)colorLabel16);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel8);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel15);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel7);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel14);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel6);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel13);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel5);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel12);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel4);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel11);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel3);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel10);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel2);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel9);
		((Control)panelFill).Controls.Add((Control)(object)colorLabelSelected);
		((Control)panelFill).Controls.Add((Control)(object)colorLabel1);
		((Control)panelFill).Dock = (DockStyle)5;
		((Control)panelFill).Location = new Point(81, 2);
		((Control)panelFill).Name = "panelFill";
		((Control)panelFill).Padding = new Padding(3, 0, 0, 0);
		((Control)panelFill).Size = new Size(185, 34);
		((Control)panelFill).TabIndex = 1;
		((Control)colorLabel16).Location = new Point(166, 18);
		((Control)colorLabel16).Name = "colorLabel16";
		((Control)colorLabel16).Size = new Size(16, 16);
		((Control)colorLabel16).TabIndex = 34;
		((Control)colorLabel16).Text = "colorLabel10";
		((Control)colorLabel8).Location = new Point(166, 0);
		((Control)colorLabel8).Name = "colorLabel8";
		((Control)colorLabel8).Size = new Size(16, 16);
		((Control)colorLabel8).TabIndex = 33;
		((Control)colorLabel8).Text = "colorLabel11";
		((Control)colorLabel15).Location = new Point(148, 18);
		((Control)colorLabel15).Name = "colorLabel15";
		((Control)colorLabel15).Size = new Size(16, 16);
		((Control)colorLabel15).TabIndex = 32;
		((Control)colorLabel15).Text = "colorLabel12";
		((Control)colorLabel7).Location = new Point(148, 0);
		((Control)colorLabel7).Name = "colorLabel7";
		((Control)colorLabel7).Size = new Size(16, 16);
		((Control)colorLabel7).TabIndex = 31;
		((Control)colorLabel7).Text = "colorLabel13";
		((Control)colorLabel14).Location = new Point(130, 18);
		((Control)colorLabel14).Name = "colorLabel14";
		((Control)colorLabel14).Size = new Size(16, 16);
		((Control)colorLabel14).TabIndex = 30;
		((Control)colorLabel14).Text = "colorLabel14";
		((Control)colorLabel6).Location = new Point(130, 0);
		((Control)colorLabel6).Name = "colorLabel6";
		((Control)colorLabel6).Size = new Size(16, 16);
		((Control)colorLabel6).TabIndex = 29;
		((Control)colorLabel6).Text = "colorLabel15";
		((Control)colorLabel13).Location = new Point(112, 18);
		((Control)colorLabel13).Name = "colorLabel13";
		((Control)colorLabel13).Size = new Size(16, 16);
		((Control)colorLabel13).TabIndex = 28;
		((Control)colorLabel13).Text = "colorLabel16";
		((Control)colorLabel5).Location = new Point(112, 0);
		((Control)colorLabel5).Name = "colorLabel5";
		((Control)colorLabel5).Size = new Size(16, 16);
		((Control)colorLabel5).TabIndex = 27;
		((Control)colorLabel5).Text = "colorLabel17";
		((Control)colorLabel12).Location = new Point(94, 18);
		((Control)colorLabel12).Name = "colorLabel12";
		((Control)colorLabel12).Size = new Size(16, 16);
		((Control)colorLabel12).TabIndex = 26;
		((Control)colorLabel12).Text = "colorLabel6";
		((Control)colorLabel4).Location = new Point(94, 0);
		((Control)colorLabel4).Name = "colorLabel4";
		((Control)colorLabel4).Size = new Size(16, 16);
		((Control)colorLabel4).TabIndex = 25;
		((Control)colorLabel4).Text = "colorLabel7";
		((Control)colorLabel11).Location = new Point(76, 18);
		((Control)colorLabel11).Name = "colorLabel11";
		((Control)colorLabel11).Size = new Size(16, 16);
		((Control)colorLabel11).TabIndex = 24;
		((Control)colorLabel11).Text = "colorLabel8";
		((Control)colorLabel3).Location = new Point(76, 0);
		((Control)colorLabel3).Name = "colorLabel3";
		((Control)colorLabel3).Size = new Size(16, 16);
		((Control)colorLabel3).TabIndex = 23;
		((Control)colorLabel3).Text = "colorLabel9";
		((Control)colorLabel10).Location = new Point(58, 18);
		((Control)colorLabel10).Name = "colorLabel10";
		((Control)colorLabel10).Size = new Size(16, 16);
		((Control)colorLabel10).TabIndex = 22;
		((Control)colorLabel10).Text = "colorLabel4";
		((Control)colorLabel2).Location = new Point(58, 0);
		((Control)colorLabel2).Name = "colorLabel2";
		((Control)colorLabel2).Size = new Size(16, 16);
		((Control)colorLabel2).TabIndex = 21;
		((Control)colorLabel2).Text = "colorLabel5";
		((Control)colorLabel9).Location = new Point(40, 18);
		((Control)colorLabel9).Name = "colorLabel9";
		((Control)colorLabel9).Size = new Size(16, 16);
		((Control)colorLabel9).TabIndex = 20;
		((Control)colorLabel9).Text = "colorLabel3";
		((Control)colorLabelSelected).Dock = (DockStyle)3;
		((Control)colorLabelSelected).Location = new Point(3, 0);
		((Control)colorLabelSelected).Name = "colorLabelSelected";
		((Control)colorLabelSelected).Size = new Size(34, 34);
		((Control)colorLabelSelected).TabIndex = 19;
		((Control)colorLabelSelected).Text = "colorLabel2";
		((Control)colorLabel1).Location = new Point(40, 0);
		((Control)colorLabel1).Name = "colorLabel1";
		((Control)colorLabel1).Size = new Size(16, 16);
		((Control)colorLabel1).TabIndex = 18;
		((Control)colorLabel1).Text = "colorLabel1";
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 12f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).Controls.Add((Control)(object)panelFill);
		((Control)this).Controls.Add((Control)(object)panelLeft);
		((Control)this).Name = "ColorSelector";
		((Control)this).Padding = new Padding(2);
		((Control)this).Size = new Size(268, 38);
		((Control)panelLeft).ResumeLayout(false);
		((Control)panelFill).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
	}
}
