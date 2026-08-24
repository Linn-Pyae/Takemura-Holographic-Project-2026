using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MetaStudio;

public class ScreenHelper
{
	public static void GetFullScreenShot()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		Screen primaryScreen = Screen.PrimaryScreen;
		Bitmap val = new Bitmap(primaryScreen.Bounds.Width, primaryScreen.Bounds.Height);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		val2.SmoothingMode = (SmoothingMode)2;
		val2.CopyFromScreen(new Point(0, 0), new Point(0, 0), primaryScreen.Bounds.Size);
		((Image)val).Save(".//SavedImage.png", ImageFormat.Png);
		val2.Dispose();
	}

	public static int CutScreenTOStator(int id, Point point, int _width, int _height, int row, int col, bool iscenter)
	{
		try
		{
			double num = (double)_height / (ConstData.Radical_sign * (double)(row - 1) + 2.0);
			double num2 = (double)_width / (ConstData.Radical_sign * (double)(col - 1) + 2.0);
			if (num > num2)
			{
				num = num2;
			}
			ConstData.Diameter = num * 2.0;
			int num3 = (int)(ConstData.Radical_sign * num * (double)(col - 1));
			int num4 = point.X;
			int y = point.Y;
			if (iscenter)
			{
				num4 = (int)(((double)_width - (num * 2.0 + (double)num3)) / 2.0);
			}
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < col; j++)
				{
					MetaTool.CaptureScreenTOStator(id, new Point(num4 + (int)((double)j * (ConstData.Radical_sign * num)), y + (int)((double)i * (ConstData.Radical_sign * num))), (int)ConstData.Diameter, (int)ConstData.Diameter);
				}
			}
			return num4;
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
		return 0;
	}

	public static void ConfigAngle(int deviceId, int value)
	{
		int data = (int)((double)value * 2.8444444444444446);
		SPHelper.SendTORotor(deviceId, 2, 16, data);
	}
}
