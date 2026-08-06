using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CSharpWin_JD.CaptureImage.Properties;

namespace CSharpWin_JD.CaptureImage;

public class DrawToolsControl : UserControl
{
	private CaptureImageToolColorTable _colorTable;

	private DrawStyle _drawStyle;

	private ToolStripButton _checkButton;

	private DrawToolsDockStyle _drawToolsDockStyle;

	private static readonly object EventButtonRedoClick = new object();

	private static readonly object EventButtonSaveClick = new object();

	private static readonly object EventButtonExitClick = new object();

	private static readonly object EventButtonAcceptClick = new object();

	private static readonly object EventButtonDrawStyleClick = new object();

	private IContainer components = null;

	private ToolStrip toolStrip;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton toolStripButtonExit;

	private ToolStripButton toolStripButtonAccept;

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
			toolStrip.Renderer = (ToolStripRenderer)(object)new ToolStripRendererEx(value);
		}
	}

	[Browsable(false)]
	public DrawStyle DrawStyle => _drawStyle;

	[DefaultValue(typeof(DrawToolsDockStyle), "0")]
	[Browsable(false)]
	public DrawToolsDockStyle DrawToolsDockStyle
	{
		get
		{
			return _drawToolsDockStyle;
		}
		set
		{
			_drawToolsDockStyle = value;
		}
	}

	private ToolStripButton CheckButton
	{
		get
		{
			return _checkButton;
		}
		set
		{
			if (_checkButton != null && _checkButton != value)
			{
				_checkButton.Checked = false;
			}
			_checkButton = value;
			if (_checkButton != null)
			{
				_checkButton.Checked = true;
			}
		}
	}

	protected override Size DefaultSize => new Size(224, 29);

	public event EventHandler ButtonDrawStyleClick
	{
		add
		{
			((Component)this).Events.AddHandler(EventButtonDrawStyleClick, value);
		}
		remove
		{
			((Component)this).Events.RemoveHandler(EventButtonDrawStyleClick, value);
		}
	}

	public event EventHandler ButtonRedoClick
	{
		add
		{
			((Component)this).Events.AddHandler(EventButtonRedoClick, value);
		}
		remove
		{
			((Component)this).Events.RemoveHandler(EventButtonRedoClick, value);
		}
	}

	public event EventHandler ButtonSaveClick
	{
		add
		{
			((Component)this).Events.AddHandler(EventButtonSaveClick, value);
		}
		remove
		{
			((Component)this).Events.RemoveHandler(EventButtonSaveClick, value);
		}
	}

	public event EventHandler ButtonExitClick
	{
		add
		{
			((Component)this).Events.AddHandler(EventButtonExitClick, value);
		}
		remove
		{
			((Component)this).Events.RemoveHandler(EventButtonExitClick, value);
		}
	}

	public event EventHandler ButtonAcceptClick
	{
		add
		{
			((Component)this).Events.AddHandler(EventButtonAcceptClick, value);
		}
		remove
		{
			((Component)this).Events.RemoveHandler(EventButtonAcceptClick, value);
		}
	}

	public DrawToolsControl()
	{
		InitializeComponent();
		((Control)this).DoubleBuffered = true;
		((Control)this).ResizeRedraw = true;
		InitEvents();
		toolStrip.Renderer = (ToolStripRenderer)(object)new ToolStripRendererEx();
	}

	public void ResetItemState()
	{
		switch (_drawStyle)
		{
		case DrawStyle.Rectangle:
			break;
		case DrawStyle.Ellipse:
			break;
		case DrawStyle.Arrow:
			break;
		case DrawStyle.Text:
			break;
		case DrawStyle.Line:
			break;
		}
	}

	public void ResetDrawStyle()
	{
		ResetItemState();
		_drawStyle = DrawStyle.None;
	}

	protected virtual void OnButtonDrawStyleClick(EventArgs e)
	{
		if (((Component)this).Events[EventButtonDrawStyleClick] is EventHandler eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnButtonRedoClick(EventArgs e)
	{
		if (((Component)this).Events[EventButtonRedoClick] is EventHandler eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnButtonSaveClick(EventArgs e)
	{
		if (((Component)this).Events[EventButtonSaveClick] is EventHandler eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnButtonExitClick(EventArgs e)
	{
		if (((Component)this).Events[EventButtonExitClick] is EventHandler eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnButtonAcceptClick(EventArgs e)
	{
		if (((Component)this).Events[EventButtonAcceptClick] is EventHandler eventHandler)
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

	protected override void OnMouseEnter(EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
		((Control)this).Cursor = Cursors.Default;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		((Control)this).OnPaint(e);
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = (SmoothingMode)4;
		GraphicsPath val = GraphicsPathHelper.CreatePath(((Control)this).ClientRectangle, 8, RoundStyle.All, correction: false);
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
				GraphicsPath val4 = GraphicsPathHelper.CreatePath(((Control)this).ClientRectangle, 8, RoundStyle.All, correction: true);
				try
				{
					graphics.DrawPath(val3, val4);
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

	private void InitEvents()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		toolStrip.ItemClicked += new ToolStripItemClickedEventHandler(ToolStripItemClicked);
	}

	private void ToolStripItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		switch (e.ClickedItem.Name)
		{
		case "toolStripButtonRectangular":
			if (_drawStyle != DrawStyle.Rectangle)
			{
				_drawStyle = DrawStyle.Rectangle;
			}
			else
			{
				_drawStyle = DrawStyle.None;
				CheckButton = null;
			}
			OnButtonDrawStyleClick((EventArgs)(object)e);
			break;
		case "toolStripButtonEllipse":
			ResetItemState();
			if (_drawStyle != DrawStyle.Ellipse)
			{
				_drawStyle = DrawStyle.Ellipse;
			}
			else
			{
				_drawStyle = DrawStyle.None;
				CheckButton = null;
			}
			OnButtonDrawStyleClick((EventArgs)(object)e);
			break;
		case "toolStripButtonText":
			ResetItemState();
			if (_drawStyle != DrawStyle.Text)
			{
				_drawStyle = DrawStyle.Text;
			}
			else
			{
				_drawStyle = DrawStyle.None;
				CheckButton = null;
			}
			OnButtonDrawStyleClick((EventArgs)(object)e);
			break;
		case "toolStripButtonArrow":
			ResetItemState();
			if (_drawStyle != DrawStyle.Arrow)
			{
				_drawStyle = DrawStyle.Arrow;
			}
			else
			{
				_drawStyle = DrawStyle.None;
				CheckButton = null;
			}
			OnButtonDrawStyleClick((EventArgs)(object)e);
			break;
		case "toolStripButtonLine":
			ResetItemState();
			if (_drawStyle != DrawStyle.Line)
			{
				_drawStyle = DrawStyle.Line;
			}
			else
			{
				_drawStyle = DrawStyle.None;
				CheckButton = null;
			}
			OnButtonDrawStyleClick((EventArgs)(object)e);
			break;
		case "toolStripButtonRedo":
			OnButtonRedoClick((EventArgs)(object)e);
			break;
		case "toolStripButtonSave":
			OnButtonSaveClick((EventArgs)(object)e);
			break;
		case "toolStripButtonExit":
			OnButtonExitClick((EventArgs)(object)e);
			break;
		case "toolStripButtonAccept":
			OnButtonAcceptClick((EventArgs)(object)e);
			break;
		}
	}

	private void toolStripButtonSave_Click(object sender, EventArgs e)
	{
	}

	private void toolStripButtonAccept_Click(object sender, EventArgs e)
	{
	}

	private void toolStripButtonExit_Click(object sender, EventArgs e)
	{
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
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		toolStrip = new ToolStrip();
		toolStripSeparator2 = new ToolStripSeparator();
		toolStripButtonExit = new ToolStripButton();
		toolStripButtonAccept = new ToolStripButton();
		((Control)toolStrip).SuspendLayout();
		((Control)this).SuspendLayout();
		toolStrip.GripStyle = (ToolStripGripStyle)0;
		toolStrip.ImageScalingSize = new Size(20, 20);
		toolStrip.Items.AddRange((ToolStripItem[])(object)new ToolStripItem[3]
		{
			(ToolStripItem)toolStripSeparator2,
			(ToolStripItem)toolStripButtonExit,
			(ToolStripItem)toolStripButtonAccept
		});
		((Control)toolStrip).Location = new Point(2, 2);
		((Control)toolStrip).Name = "toolStrip";
		((Control)toolStrip).Size = new Size(62, 27);
		((Control)toolStrip).TabIndex = 0;
		((Control)toolStrip).Text = "toolStrip1";
		((ToolStripItem)toolStripSeparator2).Name = "toolStripSeparator2";
		((ToolStripItem)toolStripSeparator2).Size = new Size(6, 27);
		((ToolStripItem)toolStripButtonExit).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)toolStripButtonExit).Image = (Image)(object)Resources.Exit;
		((ToolStripItem)toolStripButtonExit).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)toolStripButtonExit).Name = "toolStripButtonExit";
		((ToolStripItem)toolStripButtonExit).Size = new Size(24, 24);
		((ToolStripItem)toolStripButtonExit).Text = "退出截图";
		((ToolStripItem)toolStripButtonExit).Click += toolStripButtonExit_Click;
		((ToolStripItem)toolStripButtonAccept).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)toolStripButtonAccept).Image = (Image)(object)Resources.Accept;
		((ToolStripItem)toolStripButtonAccept).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)toolStripButtonAccept).Name = "toolStripButtonAccept";
		((ToolStripItem)toolStripButtonAccept).Size = new Size(24, 24);
		((ToolStripItem)toolStripButtonAccept).Text = "完成截图";
		((ToolStripItem)toolStripButtonAccept).Click += toolStripButtonAccept_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 12f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).Controls.Add((Control)(object)toolStrip);
		((Control)this).Name = "DrawToolsControl";
		((Control)this).Padding = new Padding(2, 2, 2, 2);
		((Control)this).Size = new Size(66, 29);
		((Control)toolStrip).ResumeLayout(false);
		((Control)toolStrip).PerformLayout();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
