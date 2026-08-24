using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CSharpWin_JD.CaptureImage;

public class ColorLabel : Control
{
	private Color _borderColor = Color.FromArgb(65, 173, 236);

	[DefaultValue(typeof(Color), "65, 173, 236")]
	public Color BorderColor
	{
		get
		{
			return _borderColor;
		}
		set
		{
			_borderColor = value;
			((Control)this).Invalidate();
		}
	}

	protected override Size DefaultSize => new Size(16, 16);

	public ColorLabel()
	{
		SetStyles();
	}

	private void SetStyles()
	{
		((Control)this).SetStyle((ControlStyles)139282, true);
		((Control)this).UpdateStyles();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		((Control)this).OnPaint(e);
		Graphics graphics = e.Graphics;
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		SolidBrush val = new SolidBrush(((Control)this).BackColor);
		try
		{
			graphics.FillRectangle((Brush)(object)val, clientRectangle);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ControlPaint.DrawBorder(graphics, clientRectangle, _borderColor, (ButtonBorderStyle)3);
		clientRectangle.Inflate(-1, -1);
		ControlPaint.DrawBorder(graphics, clientRectangle, Color.White, (ButtonBorderStyle)3);
	}
}
