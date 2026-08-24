#define DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CSharpWin_JD.CaptureImage.Properties;

namespace CSharpWin_JD.CaptureImage;

public class CaptureImageTool : Form
{
	private enum ProcessDPIAwareness
	{
		ProcessDPIUnaware,
		ProcessSystemDPIAware,
		ProcessPerMonitorDPIAware
	}

	public const int DESKTOPVERTRES = 117;

	public const int DESKTOPHORZRES = 118;

	public const int HORZRES = 8;

	public const int VERTRES = 10;

	private static SolidBrush mask = new SolidBrush(Color.FromArgb(100, 0, 0, 0));

	private Image ScreenImage;

	private Image _image;

	private CaptureImageToolColorTable _colorTable;

	private Cursor _selectCursor = Cursors.Default;

	private Cursor _drawCursor = Cursors.Cross;

	private Point _mouseDownPoint;

	public Point StartPoint;

	private Point _endPoint;

	private bool _mouseDown;

	private Rectangle _selectImageRect;

	private Rectangle _selectImageBounds;

	private bool _selectedImage;

	private SizeGrip _sizeGrip;

	private Dictionary<SizeGrip, Rectangle> _sizeGripRectList;

	private OperateManager _operateManager;

	private List<Point> _linePointList;

	private static readonly Font TextFont = new Font("Times New Roman", 12f, (FontStyle)1, (GraphicsUnit)3, (byte)0);

	private static readonly string ToolTipStartCapture = "按住左键不放选择截图区域";

	private bool contextMenuStripVisible = false;

	private IContainer components = null;

	private ToolTip toolTip;

	private DrawToolsControl drawToolsControl;

	private SaveFileDialog saveFileDialog;

	private ColorSelector colorSelector;

	private TextBox textBox;

	private ContextMenuStrip contextMenuStrip;

	private ToolStripMenuItem menuItemRedo;

	private ToolStripMenuItem menuItemReselect;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem menuItemAccept;

	private ToolStripMenuItem menuItemSave;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripMenuItem menuItemExit;

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
			SetControlColorTable();
		}
	}

	public Image Image => _image;

	public Cursor SelectCursor
	{
		get
		{
			return _selectCursor;
		}
		set
		{
			_selectCursor = value;
		}
	}

	public Cursor DrawCursor
	{
		get
		{
			return _drawCursor;
		}
		set
		{
			_drawCursor = value;
		}
	}

	internal bool SelectedImage
	{
		get
		{
			return _selectedImage;
		}
		set
		{
			_selectedImage = value;
		}
	}

	internal Rectangle SelectImageRect
	{
		get
		{
			return _selectImageRect;
		}
		set
		{
			_selectImageRect = value;
			if (!_selectImageRect.IsEmpty)
			{
				CalCulateSizeGripRect();
				((Control)this).Invalidate();
			}
		}
	}

	internal SizeGrip SizeGrip
	{
		get
		{
			return _sizeGrip;
		}
		set
		{
			_sizeGrip = value;
		}
	}

	internal Dictionary<SizeGrip, Rectangle> SizeGripRectList
	{
		get
		{
			if (_sizeGripRectList == null)
			{
				_sizeGripRectList = new Dictionary<SizeGrip, Rectangle>();
			}
			return _sizeGripRectList;
		}
	}

	internal OperateManager OperateManager
	{
		get
		{
			if (_operateManager == null)
			{
				_operateManager = new OperateManager();
			}
			return _operateManager;
		}
	}

	private DrawStyle DrawStyle => drawToolsControl.DrawStyle;

	private Color SelectedColor => colorSelector.SelectedColor;

	private int FontSize => colorSelector.FontSize;

	private List<Point> LinePointList
	{
		get
		{
			if (_linePointList == null)
			{
				_linePointList = new List<Point>(100);
			}
			return _linePointList;
		}
	}

	[DllImport("user32.dll")]
	private static extern IntPtr GetDC(IntPtr ptr);

	[DllImport("gdi32.dll")]
	private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

	public CaptureImageTool()
	{
		InitializeComponent();
		Init();
	}

	private void SetControlColorTable()
	{
		CaptureImageToolColorTable colorTable = ColorTable;
		ToolStripRendererEx renderer = new ToolStripRendererEx(colorTable);
		((ToolStrip)contextMenuStrip).Renderer = (ToolStripRenderer)(object)renderer;
		drawToolsControl.ColorTable = colorTable;
		colorSelector.ColorTable = colorTable;
	}

	protected override void OnLoad(EventArgs e)
	{
		((Form)this).OnLoad(e);
		toolTip.SetToolTip((Control)(object)this, ToolTipStartCapture);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
		((Control)this).Cursor = SelectCursor;
		if (!SelectedImage)
		{
			((Control)this).Invalidate(true);
			((Control)this).Update();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		((Control)this).OnMouseDown(e);
		if (((Control)textBox).Visible)
		{
			if (SelectImageRect.Contains(e.Location) || (int)e.Button == 1048576)
			{
				string text = ((Control)textBox).Text;
				Font font = ((Control)textBox).Font;
				Color foreColor = ((Control)textBox).ForeColor;
				HideTextBox();
				if (OperateManager.OperateCount > 0)
				{
					OperateObject operateObject = OperateManager.OperateList[OperateManager.OperateCount - 1];
					if (operateObject.OperateType == OperateType.DrawText)
					{
						DrawTextData drawTextData = operateObject.Data as DrawTextData;
						if (!drawTextData.Completed)
						{
							if (string.IsNullOrEmpty(text))
							{
								OperateManager.RedoOperate();
							}
							else
							{
								operateObject.Color = foreColor;
								drawTextData.Font = font;
								drawTextData.Text = text;
								drawTextData.Completed = true;
							}
						}
					}
				}
			}
			((Control)this).Invalidate();
		}
		else
		{
			if ((int)e.Button != 1048576)
			{
				return;
			}
			if (SelectedImage)
			{
				if (SizeGrip != SizeGrip.None)
				{
					_mouseDown = true;
					StartPoint = (_mouseDownPoint = e.Location);
					HideDrawToolsControl();
					((Control)this).Invalidate();
				}
				if (DrawStyle != DrawStyle.None && SelectImageRect.Contains(e.Location))
				{
					_mouseDown = true;
					StartPoint = (_mouseDownPoint = e.Location);
					if (DrawStyle == DrawStyle.Line)
					{
						LinePointList.Add(_mouseDownPoint);
					}
					ClipCursor(reset: false);
				}
			}
			else
			{
				_mouseDown = true;
				StartPoint = (_mouseDownPoint = e.Location);
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (_mouseDown)
		{
			if (!SelectedImage)
			{
				SelectImageRect = GetSelectImageRect(e.Location);
				IntPtr dC = GetDC(IntPtr.Zero);
				int deviceCaps = GetDeviceCaps(dC, 118);
				int deviceCaps2 = GetDeviceCaps(dC, 117);
				Debug.WriteLine("PrimaryScreen MaxW:" + deviceCaps + "MaxH:" + deviceCaps2);
				Rectangle selectImageRect = SelectImageRect;
				if (selectImageRect.X < 0)
				{
					selectImageRect.X = 0;
				}
				if (selectImageRect.Y < 0)
				{
					selectImageRect.Y = 0;
				}
				if (selectImageRect.Right > deviceCaps)
				{
					selectImageRect.X = deviceCaps - selectImageRect.Width;
				}
				if (selectImageRect.Bottom > deviceCaps2)
				{
					selectImageRect.Y = deviceCaps2 - selectImageRect.Height;
				}
				SelectImageRect = selectImageRect;
				Debug.WriteLine("OnMouseMove SelectedImage X:" + SelectImageRect.X + " Y:" + SelectImageRect.Y + " width:" + SelectImageRect.Width + " height:" + SelectImageRect.Height);
			}
			else if (DrawStyle != DrawStyle.None)
			{
				_endPoint = e.Location;
				if (DrawStyle == DrawStyle.Line)
				{
					LinePointList.Add(_endPoint);
				}
				((Control)this).Invalidate();
			}
			else if (SizeGrip != SizeGrip.None)
			{
				ChangeSelctImageRect(e.Location);
			}
		}
		else if (!SelectedImage)
		{
			toolTip.SetToolTip((Control)(object)this, ToolTipStartCapture);
		}
		else if (DrawStyle == DrawStyle.None)
		{
			if (OperateManager.OperateCount == 0)
			{
				SetSizeGrip(e.Location);
			}
		}
		else if (SelectImageRect.Contains(e.Location))
		{
			((Control)this).Cursor = DrawCursor;
		}
		else
		{
			((Control)this).Cursor = SelectCursor;
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Invalid comparison between Unknown and I4
		((Control)this).OnMouseUp(e);
		if ((int)e.Button == 1048576)
		{
			if (!SelectedImage)
			{
				SelectImageRect = GetSelectImageRect(e.Location);
				IntPtr dC = GetDC(IntPtr.Zero);
				int deviceCaps = GetDeviceCaps(dC, 118);
				int deviceCaps2 = GetDeviceCaps(dC, 117);
				Debug.WriteLine("PrimaryScreen MaxW:" + deviceCaps + "MaxH:" + deviceCaps2);
				Rectangle selectImageRect = SelectImageRect;
				if (selectImageRect.X < 0)
				{
					selectImageRect.X = 0;
				}
				if (selectImageRect.Y < 0)
				{
					selectImageRect.Y = 0;
				}
				if (selectImageRect.Right > deviceCaps)
				{
					selectImageRect.X = deviceCaps - selectImageRect.Width;
				}
				if (selectImageRect.Bottom > deviceCaps2)
				{
					selectImageRect.Y = deviceCaps2 - selectImageRect.Height;
				}
				SelectImageRect = selectImageRect;
				Debug.WriteLine("mouse up SelectedImage X:" + SelectImageRect.X + " Y:" + SelectImageRect.Y + " width:" + SelectImageRect.Width + " height:" + SelectImageRect.Height);
				if (!SelectImageRect.IsEmpty)
				{
					SelectedImage = true;
					ShowDrawToolsControl();
				}
			}
			else
			{
				_endPoint = e.Location;
				((Control)this).Invalidate();
				if (DrawStyle != DrawStyle.None)
				{
					ClipCursor(reset: true);
				}
				else if (SizeGrip != SizeGrip.None)
				{
					StartPoint = new Point(_selectImageBounds.X, _selectImageBounds.Y);
					ShowDrawToolsControl();
					SizeGrip = SizeGrip.None;
				}
			}
			_mouseDown = false;
			_mouseDownPoint = Point.Empty;
		}
		else
		{
			if ((int)e.Button != 2097152)
			{
				return;
			}
			if (SelectedImage)
			{
				if (SelectImageRect.Contains(e.Location))
				{
					((ToolStripDropDown)contextMenuStrip).Show((Control)(object)this, e.Location);
					contextMenuStripVisible = true;
				}
				else
				{
					ResetSelectImage();
				}
			}
			else
			{
				((Form)this).DialogResult = (DialogResult)2;
			}
		}
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		((Control)this).OnMouseDoubleClick(e);
		bool flag = SelectImageRect.Contains(e.Location);
		if ((int)e.Button == 1048576)
		{
			if (flag)
			{
				DrawLastImage();
				((Form)this).DialogResult = (DialogResult)1;
			}
		}
		else if ((int)e.Button == 2097152 && !flag)
		{
			((Form)this).DialogResult = (DialogResult)2;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Expected O, but got Unknown
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Expected O, but got Unknown
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Expected O, but got Unknown
		((Form)this).OnPaint(e);
		Graphics graphics = e.Graphics;
		graphics.DrawImage(ScreenImage, SelectImageRect, SelectImageRect, (GraphicsUnit)2);
		Graphics graphics2 = e.Graphics;
		graphics2.SmoothingMode = (SmoothingMode)4;
		Rectangle bounds = Screen.PrimaryScreen.Bounds;
		int width = bounds.Width;
		int height = bounds.Height;
		Rectangle selectImageRect = SelectImageRect;
		Rectangle selectImageRect2 = SelectImageRect;
		if (selectImageRect.X < 0)
		{
			selectImageRect.X = 0;
		}
		if (selectImageRect.Y < 0)
		{
			selectImageRect.Y = 0;
		}
		if (selectImageRect.Right > width)
		{
			selectImageRect.X = width - selectImageRect.Width;
		}
		if (selectImageRect.Bottom > height)
		{
			selectImageRect.Y = height - selectImageRect.Height;
		}
		Debug.WriteLine("on paint SelectedImage before  X:" + SelectImageRect.X + " Y:" + SelectImageRect.Y + " width:" + SelectImageRect.Width + " height:" + SelectImageRect.Height);
		Debug.WriteLine("on paint SelectedImage after  X:" + SelectImageRect.X + " Y:" + SelectImageRect.Y + " width:" + SelectImageRect.Width + " height:" + SelectImageRect.Height);
		if (SelectImageRect.Width == 0 || SelectImageRect.Height == 0)
		{
			return;
		}
		CaptureImageToolColorTable colorTable = ColorTable;
		if (_mouseDown && (!SelectedImage || SizeGrip != SizeGrip.None))
		{
			SolidBrush val = new SolidBrush(Color.FromArgb(0, colorTable.BackColorNormal));
			try
			{
				graphics2.FillRectangle((Brush)(object)val, selectImageRect2);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			DrawImageSizeInfo(graphics2, selectImageRect2);
		}
		Pen val2 = new Pen(colorTable.BorderColor);
		try
		{
			graphics2.DrawRectangle(val2, selectImageRect2);
			SolidBrush val = new SolidBrush(colorTable.BackColorPressed);
			try
			{
				foreach (Rectangle value in SizeGripRectList.Values)
				{
					graphics2.FillRectangle((Brush)(object)val, value);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		DrawOperate(graphics2);
		if (DrawStyle != DrawStyle.None)
		{
			DrawTools(graphics2, _endPoint);
		}
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		((Form)this).OnClosing(e);
		if (_sizeGripRectList != null)
		{
			_sizeGripRectList.Clear();
			_sizeGripRectList = null;
		}
		if (_operateManager != null)
		{
			_operateManager.Dispose();
			_operateManager = null;
		}
		if (_linePointList != null)
		{
			_linePointList.Clear();
			_linePointList = null;
		}
		_selectCursor = null;
		_drawCursor = null;
	}

	private void DrawImageSizeInfo(Graphics g, Rectangle rect)
	{
		string text = $"{rect.Width}x{rect.Height}";
		Size size = TextRenderer.MeasureText(text, TextFont);
		Rectangle bounds = Screen.GetBounds((Control)(object)this);
		int num = 0;
		int num2 = 0;
		num = ((rect.X + size.Width <= bounds.Right - 3) ? (rect.X + 2) : (bounds.Right - size.Width - 3));
		num2 = ((rect.Y - size.Width >= bounds.Y + 3) ? (rect.Y - size.Height - 2) : (rect.Y + 2));
		Rectangle rectangle = new Rectangle(num, num2, size.Width, size.Height);
		g.FillRectangle(Brushes.Black, rectangle);
		TextRenderer.DrawText((IDeviceContext)(object)g, text, TextFont, rectangle, Color.White);
	}

	private void DrawTools(Graphics g, Point point)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Expected O, but got Unknown
		if (!SelectImageRect.Contains(_mouseDownPoint))
		{
			return;
		}
		Color selectedColor = SelectedColor;
		switch (DrawStyle)
		{
		case DrawStyle.Rectangle:
		{
			Pen val = new Pen(selectedColor);
			try
			{
				g.DrawRectangle(val, ImageBoundsToRect(Rectangle.FromLTRB(_mouseDownPoint.X, _mouseDownPoint.Y, point.X, point.Y)));
				break;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		case DrawStyle.Ellipse:
		{
			Pen val = new Pen(selectedColor);
			try
			{
				g.DrawEllipse(val, ImageBoundsToRect(Rectangle.FromLTRB(_mouseDownPoint.X, _mouseDownPoint.Y, point.X, point.Y)));
				break;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		case DrawStyle.Arrow:
		{
			Pen val = new Pen(selectedColor);
			try
			{
				val.EndCap = (LineCap)20;
				val.EndCap = (LineCap)255;
				val.CustomEndCap = (CustomLineCap)new AdjustableArrowCap(4f, 4f, true);
				g.DrawLine(val, _mouseDownPoint, point);
				break;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		case DrawStyle.Text:
		{
			Pen val = new Pen(selectedColor);
			try
			{
				val.DashStyle = (DashStyle)3;
				val.DashCap = (DashCap)2;
				val.DashPattern = new float[4] { 9f, 3f, 3f, 3f };
				g.DrawRectangle(val, ImageBoundsToRect(Rectangle.FromLTRB(_mouseDownPoint.X, _mouseDownPoint.Y, point.X, point.Y)));
				break;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		case DrawStyle.Line:
		{
			if (LinePointList.Count < 2)
			{
				break;
			}
			Point[] array = LinePointList.ToArray();
			Pen val = new Pen(selectedColor);
			try
			{
				g.DrawLines(val, array);
				break;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		}
	}

	private void DrawOperate(Graphics g)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		foreach (OperateObject operate in OperateManager.OperateList)
		{
			switch (operate.OperateType)
			{
			case OperateType.DrawRectangle:
			{
				Pen val = new Pen(operate.Color);
				try
				{
					g.DrawRectangle(val, (Rectangle)operate.Data);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				break;
			}
			case OperateType.DrawEllipse:
			{
				Pen val = new Pen(operate.Color);
				try
				{
					g.DrawEllipse(val, (Rectangle)operate.Data);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				break;
			}
			case OperateType.DrawArrow:
			{
				Point[] array = operate.Data as Point[];
				Pen val = new Pen(operate.Color);
				try
				{
					val.EndCap = (LineCap)255;
					val.CustomEndCap = (CustomLineCap)new AdjustableArrowCap(4f, 4f, true);
					g.DrawLine(val, array[0], array[1]);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				break;
			}
			case OperateType.DrawText:
			{
				DrawTextData drawTextData = operate.Data as DrawTextData;
				if (string.IsNullOrEmpty(drawTextData.Text))
				{
					Pen val = new Pen(operate.Color);
					try
					{
						val.DashStyle = (DashStyle)3;
						val.DashCap = (DashCap)2;
						val.DashPattern = new float[4] { 9f, 3f, 3f, 3f };
						g.DrawRectangle(val, drawTextData.TextRect);
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
				}
				else
				{
					SolidBrush val2 = new SolidBrush(operate.Color);
					try
					{
						g.DrawString(drawTextData.Text, drawTextData.Font, (Brush)(object)val2, (RectangleF)drawTextData.TextRect);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				break;
			}
			case OperateType.DrawLine:
			{
				Pen val = new Pen(operate.Color);
				try
				{
					g.DrawLines(val, operate.Data as Point[]);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				break;
			}
			}
		}
	}

	private void DrawLastImage()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		Bitmap val = new Bitmap(((Control)this).Width, ((Control)this).Height, (PixelFormat)2498570);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				val2.InterpolationMode = (InterpolationMode)7;
				val2.SmoothingMode = (SmoothingMode)4;
				val2.DrawImage(ScreenImage, Point.Empty);
				DrawOperate(val2);
				val2.Flush();
				Bitmap val3 = new Bitmap(SelectImageRect.Width, SelectImageRect.Height, (PixelFormat)2498570);
				Graphics val4 = Graphics.FromImage((Image)(object)val3);
				val4.DrawImage((Image)(object)val, 0, 0, SelectImageRect, (GraphicsUnit)2);
				Debug.WriteLine("DrawLastImage SelectedImage X:" + SelectImageRect.X + " Y:" + SelectImageRect.Y + " width:" + SelectImageRect.Width + " height:" + SelectImageRect.Height);
				val4.Flush();
				val4.Dispose();
				_image = (Image)(object)val3;
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
		Clipboard.SetDataObject((object)Image, true);
	}

	private void ColorSelectorColorChanged(object sender, EventArgs e)
	{
		if (DrawStyle == DrawStyle.Text && ((Control)textBox).Visible)
		{
			((Control)textBox).ForeColor = SelectedColor;
		}
	}

	private void DrawToolsControlButtonDrawStyleClick(object sender, EventArgs e)
	{
		switch (DrawStyle)
		{
		case DrawStyle.Rectangle:
		case DrawStyle.Ellipse:
		case DrawStyle.Arrow:
		case DrawStyle.Line:
			colorSelector.Reset();
			ShowColorSelector();
			if (SizeGrip != SizeGrip.None)
			{
				SizeGrip = SizeGrip.None;
			}
			break;
		case DrawStyle.Text:
			colorSelector.ChangeToFontStyle();
			ShowColorSelector();
			if (SizeGrip != SizeGrip.None)
			{
				SizeGrip = SizeGrip.None;
			}
			break;
		case DrawStyle.None:
			HideColorSelector();
			break;
		}
	}

	private void DrawToolsControlButtonRedoClick(object sender, EventArgs e)
	{
		if (OperateManager.OperateCount > 0)
		{
			OperateManager.RedoOperate();
			((Control)this).Invalidate();
		}
		else if (SelectedImage)
		{
			ResetSelectImage();
			((Control)this).Invalidate();
		}
	}

	private void DrawToolsControlButtonSaveClick(object sender, EventArgs e)
	{
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		if (SelectedImage)
		{
			((FileDialog)saveFileDialog).FileName = "JD" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + ((FileDialog)saveFileDialog).DefaultExt;
			if ((int)((CommonDialog)saveFileDialog).ShowDialog() == 1)
			{
				DrawLastImage();
				string fileName = ((FileDialog)saveFileDialog).FileName;
				int num = fileName.LastIndexOf('.');
				string text = fileName.Substring(num + 1, fileName.Length - num - 1);
				text = text.ToLower();
				ImageFormat val = ImageFormat.Bmp;
				switch (text)
				{
				case "jpg":
				case "jpeg":
					val = ImageFormat.Jpeg;
					break;
				case "png":
					val = ImageFormat.Png;
					break;
				case "gif":
					val = ImageFormat.Gif;
					break;
				}
				Image.Save(((FileDialog)saveFileDialog).FileName, val);
				((Form)this).DialogResult = (DialogResult)2;
				((Form)this).Close();
			}
		}
		else
		{
			MessageBox.Show("请先选择图像。", "截图", (MessageBoxButtons)0);
		}
	}

	private void DrawToolsControlButtonAcceptClick(object sender, EventArgs e)
	{
		if (SelectedImage)
		{
			DrawLastImage();
			((Form)this).DialogResult = (DialogResult)1;
		}
		else
		{
			((Form)this).DialogResult = (DialogResult)2;
		}
	}

	private void DrawToolsControlButtonExitClick(object sender, EventArgs e)
	{
		((Form)this).DialogResult = (DialogResult)2;
	}

	private void MenuItemReselectClick(object sender, EventArgs e)
	{
		if (SelectedImage)
		{
			ResetSelectImage();
		}
	}

	private void TextBoxExLostFocus(object sender, EventArgs e)
	{
		if (!((Control)textBox).Visible)
		{
			return;
		}
		string text = ((Control)textBox).Text;
		Font font = ((Control)textBox).Font;
		Color foreColor = ((Control)textBox).ForeColor;
		HideTextBox();
		if (OperateManager.OperateCount > 0)
		{
			OperateObject operateObject = OperateManager.OperateList[OperateManager.OperateCount - 1];
			if (operateObject.OperateType == OperateType.DrawText)
			{
				DrawTextData drawTextData = operateObject.Data as DrawTextData;
				if (!drawTextData.Completed)
				{
					if (string.IsNullOrEmpty(text))
					{
						OperateManager.RedoOperate();
					}
					else
					{
						operateObject.Color = foreColor;
						drawTextData.Font = font;
						drawTextData.Text = text;
						drawTextData.Completed = true;
					}
				}
			}
		}
		((Control)this).Invalidate();
	}

	[DllImport("shcore.dll")]
	private static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);

	private static void SetDpiAwareness()
	{
		try
		{
			if (Environment.OSVersion.Version.Major >= 6)
			{
				SetProcessDpiAwareness(ProcessDPIAwareness.ProcessPerMonitorDPIAware);
			}
		}
		catch (EntryPointNotFoundException)
		{
		}
	}

	private void Init()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		((Control)this).SetStyle((ControlStyles)139266, true);
		((Control)drawToolsControl).Visible = false;
		((Control)colorSelector).Visible = false;
		((Control)textBox).Visible = false;
		SetDpiAwareness();
		((Form)this).TopMost = true;
		((Form)this).ShowInTaskbar = false;
		((Form)this).FormBorderStyle = (FormBorderStyle)0;
		IntPtr dC = GetDC(IntPtr.Zero);
		int deviceCaps = GetDeviceCaps(dC, 118);
		int deviceCaps2 = GetDeviceCaps(dC, 117);
		((Control)this).Bounds = new Rectangle(0, 0, deviceCaps, deviceCaps2);
		ScreenImage = GetDestopImage();
		Image val = (Image)new Bitmap(ScreenImage);
		Graphics val2 = Graphics.FromImage(val);
		val2.FillRectangle((Brush)(object)mask, 0, 0, val.Width, val.Height);
		val2.Dispose();
		((Control)this).BackgroundImage = val;
		((Control)this).BackgroundImageLayout = (ImageLayout)3;
		try
		{
			_selectCursor = new Cursor(Resources.Arrow_M.Handle);
		}
		catch
		{
		}
		((Control)this).Cursor = SelectCursor;
		((ToolStrip)contextMenuStrip).Renderer = (ToolStripRenderer)(object)new ToolStripRendererEx();
		((Control)textBox).LostFocus += TextBoxExLostFocus;
		colorSelector.ColorChanged += ColorSelectorColorChanged;
		drawToolsControl.ButtonExitClick += DrawToolsControlButtonExitClick;
		drawToolsControl.ButtonAcceptClick += DrawToolsControlButtonAcceptClick;
		drawToolsControl.ButtonSaveClick += DrawToolsControlButtonSaveClick;
		drawToolsControl.ButtonRedoClick += DrawToolsControlButtonRedoClick;
		drawToolsControl.ButtonDrawStyleClick += DrawToolsControlButtonDrawStyleClick;
		((ToolStripItem)menuItemExit).Click += DrawToolsControlButtonExitClick;
		((ToolStripItem)menuItemAccept).Click += DrawToolsControlButtonAcceptClick;
		((ToolStripItem)menuItemSave).Click += DrawToolsControlButtonSaveClick;
		((ToolStripItem)menuItemRedo).Click += DrawToolsControlButtonRedoClick;
		((ToolStripItem)menuItemReselect).Click += MenuItemReselectClick;
	}

	private Image GetDestopImage()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		IntPtr dC = GetDC(IntPtr.Zero);
		int deviceCaps = GetDeviceCaps(dC, 118);
		int deviceCaps2 = GetDeviceCaps(dC, 117);
		int deviceCaps3 = GetDeviceCaps(dC, 8);
		int deviceCaps4 = GetDeviceCaps(dC, 10);
		Bitmap val = new Bitmap(deviceCaps, deviceCaps2);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		val2.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(deviceCaps, deviceCaps2));
		return (Image)(object)val;
	}

	private Image GetDestopImage2()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		IntPtr dC = GetDC(IntPtr.Zero);
		int deviceCaps = GetDeviceCaps(dC, 118);
		int deviceCaps2 = GetDeviceCaps(dC, 117);
		int deviceCaps3 = GetDeviceCaps(dC, 8);
		int deviceCaps4 = GetDeviceCaps(dC, 10);
		Rectangle bounds = Screen.GetBounds((Control)(object)this);
		Bitmap val = new Bitmap(bounds.Width, bounds.Height, (PixelFormat)2498570);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		IntPtr hdc = val2.GetHdc();
		IntPtr desktopWindow = NativeMethods.GetDesktopWindow();
		IntPtr dC2 = NativeMethods.GetDC(desktopWindow);
		NativeMethods.BitBlt(hdc, 0, 0, ((Control)this).Width, ((Control)this).Height, dC2, 0, 0, NativeMethods.TernaryRasterOperations.SRCCOPY);
		NativeMethods.ReleaseDC(desktopWindow, dC2);
		val2.ReleaseHdc(hdc);
		return (Image)(object)val;
	}

	private Rectangle GetSelectImageRect(Point endPoint)
	{
		_selectImageBounds = Rectangle.FromLTRB(_mouseDownPoint.X, _mouseDownPoint.Y, endPoint.X, endPoint.Y);
		Debug.WriteLine("endPoint.X:" + endPoint.X + " endPoint.Y:" + endPoint.Y);
		if (_selectImageBounds.Width > _selectImageBounds.Height)
		{
			if (_mouseDownPoint.Y + _selectImageBounds.Width > 1080)
			{
				_selectImageBounds = new Rectangle(_selectImageBounds.X, _selectImageBounds.Y, _selectImageBounds.Height, _selectImageBounds.Height);
				Debug.WriteLine("wdith>height >1080: x:" + _selectImageBounds.X + " y:" + _selectImageBounds.Y + " height:" + _selectImageBounds.Height);
			}
			else
			{
				_selectImageBounds = new Rectangle(_selectImageBounds.X, _selectImageBounds.Y, _selectImageBounds.Width, _selectImageBounds.Width);
				Debug.WriteLine("wdith>height <=1080: x:" + _selectImageBounds.X + " y:" + _selectImageBounds.Y + " widht:" + _selectImageBounds.Width);
			}
		}
		else
		{
			_selectImageBounds = new Rectangle(_selectImageBounds.X, _selectImageBounds.Y, _selectImageBounds.Height, _selectImageBounds.Height);
			Debug.WriteLine("wdith<height: x:" + _selectImageBounds.X + " y:" + _selectImageBounds.Y + " Height:" + _selectImageBounds.Height);
		}
		return ImageBoundsToRect(_selectImageBounds);
	}

	private void CalCulateSizeGripRect()
	{
		Rectangle selectImageRect = SelectImageRect;
		int x = selectImageRect.X;
		int y = selectImageRect.Y;
		int num = x + selectImageRect.Width / 2;
		int num2 = y + selectImageRect.Height / 2;
		Dictionary<SizeGrip, Rectangle> sizeGripRectList = SizeGripRectList;
		sizeGripRectList.Clear();
		sizeGripRectList.Add(SizeGrip.TopLeft, new Rectangle(x - 2, y - 2, 5, 5));
		sizeGripRectList.Add(SizeGrip.TopRight, new Rectangle(selectImageRect.Right - 2, y - 2, 5, 5));
		sizeGripRectList.Add(SizeGrip.BottomLeft, new Rectangle(x - 2, selectImageRect.Bottom - 2, 5, 5));
		sizeGripRectList.Add(SizeGrip.BottomRight, new Rectangle(selectImageRect.Right - 2, selectImageRect.Bottom - 2, 5, 5));
		sizeGripRectList.Add(SizeGrip.Top, new Rectangle(num - 2, y - 2, 5, 5));
		sizeGripRectList.Add(SizeGrip.Bottom, new Rectangle(num - 2, selectImageRect.Bottom - 2, 5, 5));
		sizeGripRectList.Add(SizeGrip.Left, new Rectangle(x - 2, num2 - 2, 5, 5));
		sizeGripRectList.Add(SizeGrip.Right, new Rectangle(selectImageRect.Right - 2, num2 - 2, 5, 5));
	}

	private void SetSizeGrip(Point point)
	{
		SizeGrip = SizeGrip.None;
		foreach (SizeGrip key in SizeGripRectList.Keys)
		{
			if (SizeGripRectList[key].Contains(point))
			{
				SizeGrip = key;
				break;
			}
		}
		if (SizeGrip == SizeGrip.None && SelectImageRect.Contains(point))
		{
			SizeGrip = SizeGrip.All;
		}
		switch (SizeGrip)
		{
		case SizeGrip.TopLeft:
		case SizeGrip.BottomRight:
			((Control)this).Cursor = Cursors.SizeNWSE;
			break;
		case SizeGrip.TopRight:
		case SizeGrip.BottomLeft:
			((Control)this).Cursor = Cursors.SizeNESW;
			break;
		case SizeGrip.Top:
		case SizeGrip.Bottom:
			((Control)this).Cursor = Cursors.SizeNS;
			break;
		case SizeGrip.Left:
		case SizeGrip.Right:
			((Control)this).Cursor = Cursors.SizeWE;
			break;
		case SizeGrip.All:
			((Control)this).Cursor = Cursors.SizeAll;
			break;
		default:
			((Control)this).Cursor = SelectCursor;
			break;
		}
	}

	private void ChangeSelctImageRect(Point point)
	{
		Rectangle selectImageBounds = _selectImageBounds;
		int num = selectImageBounds.Left;
		int num2 = selectImageBounds.Top;
		int num3 = selectImageBounds.Right;
		int num4 = selectImageBounds.Bottom;
		bool flag = false;
		switch (SizeGrip)
		{
		case SizeGrip.All:
			selectImageBounds.Offset(point.X - _mouseDownPoint.X, point.Y - _mouseDownPoint.Y);
			flag = true;
			break;
		case SizeGrip.TopLeft:
			num = point.X;
			num2 = point.Y;
			break;
		case SizeGrip.TopRight:
			num3 = point.X;
			num2 = point.Y;
			break;
		case SizeGrip.BottomLeft:
			num = point.X;
			num4 = point.Y;
			break;
		case SizeGrip.BottomRight:
			num3 = point.X;
			num4 = point.Y;
			break;
		case SizeGrip.Top:
			num2 = point.Y;
			break;
		case SizeGrip.Bottom:
			num4 = point.Y;
			break;
		case SizeGrip.Left:
			num = point.X;
			break;
		case SizeGrip.Right:
			num3 = point.X;
			break;
		}
		if (!flag)
		{
			selectImageBounds.X = num;
			selectImageBounds.Y = num2;
			selectImageBounds.Width = num3 - num;
			selectImageBounds.Height = num4 - num2;
		}
		_mouseDownPoint = point;
		_selectImageBounds = selectImageBounds;
		SelectImageRect = ImageBoundsToRect(selectImageBounds);
		IntPtr dC = GetDC(IntPtr.Zero);
		int deviceCaps = GetDeviceCaps(dC, 118);
		int deviceCaps2 = GetDeviceCaps(dC, 117);
		Debug.WriteLine("PrimaryScreen MaxW:" + deviceCaps + "MaxH:" + deviceCaps2);
		Rectangle selectImageRect = SelectImageRect;
		if (selectImageRect.X < 0)
		{
			selectImageRect.X = 0;
		}
		if (selectImageRect.Y < 0)
		{
			selectImageRect.Y = 0;
		}
		if (selectImageRect.Right > deviceCaps)
		{
			selectImageRect.X = deviceCaps - selectImageRect.Width;
		}
		if (selectImageRect.Bottom > deviceCaps2)
		{
			selectImageRect.Y = deviceCaps2 - selectImageRect.Height;
		}
		SelectImageRect = selectImageRect;
		Debug.WriteLine("ChangeSelctImageRect SelectedImage X:" + SelectImageRect.X + " Y:" + SelectImageRect.Y + " width:" + SelectImageRect.Width + " height:" + SelectImageRect.Height);
	}

	private Rectangle ImageBoundsToRect(Rectangle bounds)
	{
		Rectangle result = bounds;
		int num = 0;
		int num2 = 0;
		Rectangle bounds2 = Screen.PrimaryScreen.Bounds;
		int width = bounds2.Width;
		int height = bounds2.Height;
		num = Math.Min(result.X, result.Right);
		num2 = Math.Min(result.Y, result.Bottom);
		result.X = num;
		result.Y = num2;
		result.Width = Math.Max(1, Math.Abs(result.Width));
		result.Height = Math.Max(1, Math.Abs(result.Height));
		return result;
	}

	private void ResetSelectImage()
	{
		SelectedImage = false;
		_selectImageBounds = Rectangle.Empty;
		SelectImageRect = Rectangle.Empty;
		SizeGrip = SizeGrip.None;
		HideDrawToolsControl();
		Debug.WriteLine("HideDrawToolsControl");
		if (((Control)textBox).Visible)
		{
			HideTextBox();
		}
		OperateManager.Clear();
		((Control)this).Invalidate();
	}

	private void ShowDrawToolsControl()
	{
		Rectangle selectImageRect = SelectImageRect;
		Debug.WriteLine("ShowDrawToolsControl rect X:" + selectImageRect.X + " Y:" + selectImageRect.Y + " width:" + selectImageRect.Width + " height:" + selectImageRect.Height);
		Rectangle bounds = Screen.GetBounds((Control)(object)this);
		int num = selectImageRect.Right - ((Control)drawToolsControl).Width - 2;
		int num2 = 0;
		DrawToolsDockStyle drawToolsDockStyle = DrawToolsDockStyle.None;
		if (selectImageRect.Bottom + ((Control)drawToolsControl).Height + 2 <= bounds.Bottom)
		{
			num2 = selectImageRect.Bottom + 2;
			drawToolsDockStyle = DrawToolsDockStyle.Bottom;
			Debug.WriteLine("Bottom ");
		}
		else if (selectImageRect.Y - ((Control)drawToolsControl).Height - 2 >= bounds.Top)
		{
			num2 = selectImageRect.Y - ((Control)drawToolsControl).Height - 2;
			drawToolsDockStyle = DrawToolsDockStyle.Top;
			Debug.WriteLine("Top ");
		}
		else
		{
			num2 = selectImageRect.Bottom - ((Control)drawToolsControl).Height - 2;
			drawToolsDockStyle = DrawToolsDockStyle.BottomUp;
			Debug.WriteLine("BottomUp ");
		}
		drawToolsControl.DrawToolsDockStyle = drawToolsDockStyle;
		((Control)drawToolsControl).Location = new Point(num, num2);
		Debug.WriteLine("Point(x, y):" + num + " " + num2);
		((Control)drawToolsControl).Visible = true;
	}

	private void HideDrawToolsControl()
	{
		((Control)drawToolsControl).Visible = false;
		drawToolsControl.ResetDrawStyle();
		HideColorSelector();
	}

	private void ShowColorSelector()
	{
		int x = 0;
		int y = 0;
		Rectangle bounds = ((Control)drawToolsControl).Bounds;
		Rectangle bounds2 = Screen.GetBounds((Control)(object)this);
		switch (drawToolsControl.DrawToolsDockStyle)
		{
		case DrawToolsDockStyle.Top:
		case DrawToolsDockStyle.BottomUp:
			x = bounds.X;
			y = bounds.Y - ((Control)colorSelector).Height - 2;
			break;
		case DrawToolsDockStyle.Bottom:
			x = bounds.X;
			y = bounds.Bottom + 2;
			break;
		}
		((Control)colorSelector).Location = new Point(x, y);
		((Control)colorSelector).Visible = true;
	}

	private void HideColorSelector()
	{
		if (((Control)colorSelector).Visible)
		{
			((Control)colorSelector).Visible = false;
			colorSelector.Reset();
		}
	}

	private void ShowTextBox()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		if (SelectImageRect.Contains(_mouseDownPoint))
		{
			Rectangle bounds = ImageBoundsToRect(Rectangle.FromLTRB(_mouseDownPoint.X, _mouseDownPoint.Y, _endPoint.X, _endPoint.Y));
			bounds.Inflate(-1, -1);
			((Control)textBox).Bounds = bounds;
			((Control)textBox).Text = "";
			((Control)textBox).ForeColor = SelectedColor;
			((Control)textBox).Font = new Font(((Control)textBox).Font.FontFamily, (float)FontSize);
			((Control)textBox).Visible = true;
			((Control)textBox).Focus();
		}
	}

	private void HideTextBox()
	{
		((Control)textBox).Visible = false;
		((Control)textBox).Text = string.Empty;
	}

	private void AddOperate(Point point)
	{
		if (!SelectImageRect.Contains(_mouseDownPoint))
		{
			return;
		}
		Color selectedColor = SelectedColor;
		switch (DrawStyle)
		{
		case DrawStyle.Rectangle:
			OperateManager.AddOperate(OperateType.DrawRectangle, selectedColor, ImageBoundsToRect(Rectangle.FromLTRB(_mouseDownPoint.X, _mouseDownPoint.Y, point.X, point.Y)));
			break;
		case DrawStyle.Ellipse:
			OperateManager.AddOperate(OperateType.DrawEllipse, selectedColor, ImageBoundsToRect(Rectangle.FromLTRB(_mouseDownPoint.X, _mouseDownPoint.Y, point.X, point.Y)));
			break;
		case DrawStyle.Arrow:
		{
			Point[] data2 = new Point[2] { _mouseDownPoint, point };
			OperateManager.AddOperate(OperateType.DrawArrow, selectedColor, data2);
			break;
		}
		case DrawStyle.Text:
		{
			ShowTextBox();
			Rectangle textRect = ImageBoundsToRect(Rectangle.FromLTRB(_mouseDownPoint.X, _mouseDownPoint.Y, point.X, point.Y));
			DrawTextData data = new DrawTextData(string.Empty, ((Control)this).Font, textRect);
			OperateManager.AddOperate(OperateType.DrawText, selectedColor, data);
			break;
		}
		case DrawStyle.Line:
			if (LinePointList.Count >= 2)
			{
				OperateManager.AddOperate(OperateType.DrawLine, selectedColor, LinePointList.ToArray());
				LinePointList.Clear();
			}
			break;
		}
	}

	private void ClipCursor(bool reset)
	{
		Rectangle rect = ((!reset) ? SelectImageRect : Screen.GetBounds((Control)(object)this));
		NativeMethods.RECT lpRect = new NativeMethods.RECT(rect);
		NativeMethods.ClipCursor(ref lpRect);
	}

	private void CaptureImageTool_KeyUp(object sender, KeyEventArgs e)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		if ((int)e.KeyCode == 27)
		{
			if (contextMenuStripVisible)
			{
				((Control)contextMenuStrip).Hide();
				contextMenuStripVisible = false;
			}
			else
			{
				((Form)this).DialogResult = (DialogResult)2;
				((Form)this).Close();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Expected O, but got Unknown
		components = new Container();
		toolTip = new ToolTip(components);
		saveFileDialog = new SaveFileDialog();
		textBox = new TextBox();
		contextMenuStrip = new ContextMenuStrip(components);
		menuItemRedo = new ToolStripMenuItem();
		menuItemReselect = new ToolStripMenuItem();
		toolStripSeparator1 = new ToolStripSeparator();
		menuItemAccept = new ToolStripMenuItem();
		menuItemSave = new ToolStripMenuItem();
		toolStripSeparator2 = new ToolStripSeparator();
		menuItemExit = new ToolStripMenuItem();
		colorSelector = new ColorSelector();
		drawToolsControl = new DrawToolsControl();
		((Control)contextMenuStrip).SuspendLayout();
		((Control)this).SuspendLayout();
		((FileDialog)saveFileDialog).DefaultExt = "bmp";
		((FileDialog)saveFileDialog).Filter = "BMP 文件(*.bmp)|*.bmp|JPEG 文件(*.jpg,*.jpeg)|*.jpg,*.jpeg|PNG 文件(*.png)|*.png|GIF 文件(*.gif)|*.gif";
		((TextBoxBase)textBox).BorderStyle = (BorderStyle)0;
		((Control)textBox).ImeMode = (ImeMode)1;
		((Control)textBox).Location = new Point(3, 291);
		((Control)textBox).Margin = new Padding(4, 4, 4, 4);
		((TextBoxBase)textBox).Multiline = true;
		((Control)textBox).Name = "textBox";
		((Control)textBox).Size = new Size(133, 26);
		((Control)textBox).TabIndex = 4;
		((ToolStrip)contextMenuStrip).ImageScalingSize = new Size(20, 20);
		((ToolStrip)contextMenuStrip).Items.AddRange((ToolStripItem[])(object)new ToolStripItem[7]
		{
			(ToolStripItem)menuItemRedo,
			(ToolStripItem)menuItemReselect,
			(ToolStripItem)toolStripSeparator1,
			(ToolStripItem)menuItemAccept,
			(ToolStripItem)menuItemSave,
			(ToolStripItem)toolStripSeparator2,
			(ToolStripItem)menuItemExit
		});
		((Control)contextMenuStrip).Name = "contextMenuStrip";
		((Control)contextMenuStrip).Size = new Size(203, 146);
		((ToolStripItem)menuItemRedo).Image = (Image)(object)Resources.Redo;
		((ToolStripItem)menuItemRedo).Name = "menuItemRedo";
		((ToolStripItem)menuItemRedo).Size = new Size(202, 26);
		((ToolStripItem)menuItemRedo).Text = "撤销编辑";
		((ToolStripItem)menuItemReselect).Name = "menuItemReselect";
		((ToolStripItem)menuItemReselect).Size = new Size(202, 26);
		((ToolStripItem)menuItemReselect).Text = "重新选择截图区域";
		((ToolStripItem)toolStripSeparator1).Name = "toolStripSeparator1";
		((ToolStripItem)toolStripSeparator1).Size = new Size(199, 6);
		((ToolStripItem)menuItemAccept).Image = (Image)(object)Resources.Accept;
		((ToolStripItem)menuItemAccept).Name = "menuItemAccept";
		((ToolStripItem)menuItemAccept).Size = new Size(202, 26);
		((ToolStripItem)menuItemAccept).Text = "复制并退出截图";
		((ToolStripItem)menuItemSave).Image = (Image)(object)Resources.Save;
		((ToolStripItem)menuItemSave).Name = "menuItemSave";
		((ToolStripItem)menuItemSave).Size = new Size(202, 26);
		((ToolStripItem)menuItemSave).Text = "另存为...";
		((ToolStripItem)toolStripSeparator2).Name = "toolStripSeparator2";
		((ToolStripItem)toolStripSeparator2).Size = new Size(199, 6);
		((ToolStripItem)menuItemExit).Image = (Image)(object)Resources.Exit;
		((ToolStripItem)menuItemExit).Name = "menuItemExit";
		((ToolStripItem)menuItemExit).Size = new Size(202, 26);
		((ToolStripItem)menuItemExit).Text = "退出截图";
		((Control)colorSelector).Location = new Point(3, 236);
		((Control)colorSelector).Margin = new Padding(5);
		((Control)colorSelector).Name = "colorSelector";
		((Control)colorSelector).Padding = new Padding(3, 2, 3, 2);
		((Control)colorSelector).Size = new Size(252, 48);
		((Control)colorSelector).TabIndex = 3;
		((Control)drawToolsControl).Location = new Point(3, 192);
		((Control)drawToolsControl).Margin = new Padding(5);
		((Control)drawToolsControl).Name = "drawToolsControl";
		((Control)drawToolsControl).Padding = new Padding(3, 2, 3, 2);
		((Control)drawToolsControl).Size = new Size(85, 36);
		((Control)drawToolsControl).TabIndex = 0;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 15f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = SystemColors.Control;
		((Form)this).ClientSize = new Size(425, 332);
		((Control)this).Controls.Add((Control)(object)textBox);
		((Control)this).Controls.Add((Control)(object)colorSelector);
		((Control)this).Controls.Add((Control)(object)drawToolsControl);
		((Form)this).Margin = new Padding(4, 4, 4, 4);
		((Control)this).Name = "CaptureImageTool";
		((Control)this).Text = "CaptureImageTool";
		((Control)this).KeyUp += new KeyEventHandler(CaptureImageTool_KeyUp);
		((Control)contextMenuStrip).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
