using System;
using System.Drawing;

namespace MetaStudio;

public class ConstData
{
	public static int Ori_Width = 1920;

	public static int Ori_Height = 1080;

	public static double Diameter = 1024.0;

	public static double Radical_sign = Math.Sqrt(2.0);

	public static string jsonfilename = "data\\data.json";

	public static bool isSmall = false;

	public static bool canUpdate = false;

	public static int curOperID = 0;

	public static int CurStartX = 0;

	public static int CurStartY = 0;

	public static int CurAngle = 0;

	public static int CurSpeed = 0;

	public static int CurLight = 0;

	public static int CurScale = 0;

	public static int CurAutoStart = 0;

	public static int CurImageWidth = 0;

	public static int CurImageHeight = 0;

	public static int R_reg_1 = 0;

	public static int G_reg_1 = 0;

	public static int B_reg_1 = 0;

	public static int fan_ctrl = 0;

	public static Point frontPoint = new Point(0, 0);

	public static int frontWidth = 1920;

	public static int frontHeigth = 1080;

	public static string versionType = "1";

	public static int DeviceCount = 1;

	public static bool Importing = false;

	public static bool isShowMsg = false;

	public static string selKey = string.Empty;

	public static object o = new object();
}
