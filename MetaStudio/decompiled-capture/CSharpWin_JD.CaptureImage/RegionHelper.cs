using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CSharpWin_JD.CaptureImage;

internal static class RegionHelper
{
	public static void CreateRegion(Control control, Rectangle rect)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		GraphicsPath val = GraphicsPathHelper.CreatePath(rect, 8, RoundStyle.All, correction: false);
		try
		{
			if (control.Region != null)
			{
				control.Region.Dispose();
			}
			control.Region = new Region(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
