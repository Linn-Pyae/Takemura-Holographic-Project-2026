using System;
using System.Drawing;
using System.Threading;

namespace MetaStudio;

public class MetaTool
{
	private static int waitTime = 50;

	public static void Start(int deviceId)
	{
		SPHelper.SendTOStator(deviceId, 2, 1, 0);
	}

	public static void Stop(int deviceId)
	{
		SPHelper.SendTOStator(deviceId, 2, 1, 1);
	}

	public static int SetDeviceSpeed(int deviceID, int value)
	{
		try
		{
			switch (value)
			{
			case 750:
				SPHelper.SendTOStator(deviceID, 2, 3, 385);
				SPHelper.SendTOVdbox(deviceID, 2, 35, 385);
				break;
			case 900:
				SPHelper.SendTOStator(deviceID, 2, 3, 474);
				SPHelper.SendTOVdbox(deviceID, 2, 35, 474);
				break;
			}
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetDeviceSpeed(int deviceID)
	{
		SPHelper.SendTOStator(deviceID, 1, 3, 0);
		Thread.Sleep(waitTime);
		return ConstData.CurSpeed;
	}

	public static int MatchDevice(int deviceID, int matchType)
	{
		try
		{
			switch (matchType)
			{
			case 0:
				SPHelper.SendTOVdbox(deviceID, 2, 8, 3);
				break;
			case 1:
				SPHelper.SendTOStator(deviceID, 2, 19, 3);
				break;
			}
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetBrightness(int deviceID, int value)
	{
		try
		{
			SPHelper.SendTOVdbox(deviceID, 2, 34, value);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetBrightness(int deviceID)
	{
		try
		{
			SPHelper.SendTOVdbox(deviceID, 1, 34, 0);
			Thread.Sleep(waitTime);
			return ConstData.CurLight;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetAngle(int deviceID, int value)
	{
		try
		{
			int data = (int)((double)value * 2.8444444444444446);
			SPHelper.SendTORotor(deviceID, 2, 16, data);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetAngle(int deviceID)
	{
		try
		{
			int num = -1;
			ConstData.CurAngle = -1;
			SPHelper.SendTORotor(deviceID, 1, 16, 0);
			Thread.Sleep(waitTime);
			num = (int)((double)(ConstData.CurAngle * 360) / 1024.0) + 1;
			if (ConstData.CurAngle == -1)
			{
				return -1;
			}
			return num;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static void SetAdujst(int deviceID, int val)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 2, 12, val);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetScreenProjection(int deviceID, bool value)
	{
		try
		{
			SPHelper.SendTORotor(deviceID, 1, 26, 0);
			Thread.Sleep(30);
			int num = ConstData.fan_ctrl & 0xFF;
			if (!value)
			{
				SPHelper.SendTORotor(deviceID, 2, 26, num);
			}
			else
			{
				SPHelper.SendTORotor(deviceID, 2, 26, num | 0xFF00);
			}
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetDebugImage(int deviceID, int value)
	{
		try
		{
			SPHelper.SendTORotor(deviceID, 2, 50, 822);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetAutoStart(int deviceID, bool value)
	{
		try
		{
			if (value)
			{
				SPHelper.SendTOStator(deviceID, 2, 2, 0);
			}
			else
			{
				SPHelper.SendTOStator(deviceID, 2, 2, 1);
			}
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetAutoStart(int deviceID)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 1, 2, 0);
			Thread.Sleep(30);
			return ConstData.CurAutoStart;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetBreathingLight(int deviceID, bool value)
	{
		try
		{
			SPHelper.SendTORotor(deviceID, 1, 26, 0);
			Thread.Sleep(30);
			int num = ConstData.fan_ctrl & 0xFF00;
			if (value)
			{
				SPHelper.SendTORotor(deviceID, 2, 26, num | 4);
			}
			else
			{
				SPHelper.SendTORotor(deviceID, 2, 26, num);
			}
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetStartX(int deviceID, int value)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 2, 32, value);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetStartX(int deviceID)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 1, 32, 0);
			Thread.Sleep(30);
			return ConstData.CurStartX;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetImageWidth(int deviceID)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 1, 34, 0);
			Thread.Sleep(30);
			return ConstData.CurImageWidth;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetImageHeight(int deviceID)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 1, 34, 0);
			Thread.Sleep(30);
			return ConstData.CurImageHeight;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetStartY(int deviceID, int value)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 2, 33, value);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetStartY(int deviceID)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 1, 33, 0);
			Thread.Sleep(30);
			return ConstData.CurStartY;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetScale(int deviceID, int value)
	{
		try
		{
			int num = (int)((double)(1024 - value) / 2.0);
			ScaleStator(deviceID, new Point(num, num), value, value, value);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int GetScale(int deviceID)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 1, 44, 0);
			Thread.Sleep(30);
			return ConstData.CurScale;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static void Set2CReg(int id, int val)
	{
		SPHelper.SendTOStator(0, 2, 44, 67109888);
	}

	public static void ScaleStator(int Id, Point point, int m_width, int m_height, int scaleValue)
	{
		try
		{
			int data = (scaleValue << 16) | scaleValue;
			SPHelper.SendTOStator(Id, 2, 44, data);
			int num = m_width;
			int num2 = m_height;
			if (m_width < 40)
			{
				num = 40;
			}
			if (m_height < 40)
			{
				num2 = 40;
			}
			SPHelper.Factor_Sclr(Id, (ushort)ConstData.Diameter, (ushort)ConstData.Diameter, scaleValue, scaleValue);
			SPHelper.SendTOStator(Id, 2, 43, 0);
			SPHelper.SendTOStator(Id, 2, 43, 1);
			SPHelper.SendTOStator(Id, 2, 43, 0);
			SPHelper.SendTOStator(Id, 2, 42, 0);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static void SetVideoOutputEn(int id)
	{
		SPHelper.SendTOStator(id, 2, 41, 1);
	}

	public static void CaptureScreenTOStator(int Id, Point point, int m_width, int m_height)
	{
		SPHelper.SendTOStator(Id, 2, 32, point.X);
		SPHelper.SendTOStator(Id, 2, 33, point.Y);
		SPHelper.SendTOStator(Id, 2, 239, (point.X << 16) | point.Y);
		int num = m_width;
		if (num < 32)
		{
			num = 32;
		}
		int num2 = m_height;
		if (num2 < 32)
		{
			num2 = 32;
		}
		SPHelper.SendTOStator(Id, 2, 34, (num << 16) | num2);
		SPHelper.Factor_Sclr(Id, num, num2, 1024, 1024);
		SPHelper.SendTOStator(Id, 2, 43, 0);
		SPHelper.SendTOStator(Id, 2, 43, 1);
		SPHelper.SendTOStator(Id, 2, 43, 0);
		SPHelper.SendTOStator(Id, 2, 42, 0);
	}

	public static int SetBackground(int deviceID, int value)
	{
		try
		{
			switch (value)
			{
			case 0:
				SPHelper.SendTORotor(deviceID, 2, 125, 2);
				break;
			case 1:
				SPHelper.SendTORotor(deviceID, 2, 125, 3);
				break;
			case 2:
				SPHelper.SendTORotor(deviceID, 2, 125, 1);
				break;
			}
			EnableRGBReg(deviceID);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static void MotoDirct(int deviceID, int value)
	{
		try
		{
			switch (value)
			{
			case 1:
			{
				SPHelper.SendTORotor(deviceID, 2, 22, 1);
				int num = 420;
				SPHelper.SendTOStator(deviceID, 2, 15, 1);
				break;
			}
			case 0:
			{
				SPHelper.SendTORotor(deviceID, 2, 22, 0);
				int num = 167;
				SPHelper.SendTOStator(deviceID, 2, 15, 0);
				break;
			}
			}
			AppConfig.WriteConfig("MotoDirct" + (deviceID - 1), value.ToString());
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public static int EnableRGBReg(int deviceID)
	{
		try
		{
			SPHelper.SendTORotor(deviceID, 2, 31, 0);
			Thread.Sleep(300);
			SPHelper.SendTORotor(deviceID, 2, 31, 1);
			Thread.Sleep(300);
			SPHelper.SendTORotor(deviceID, 2, 31, 0);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int EnableFusion(int deviceID, int val)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 2, 242, 63);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int SetUpgrade(int deviceID, int val)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 2, 243, val);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int ResetID(int deviceID)
	{
		try
		{
			SPHelper.SendTOStator(deviceID, 2, 173, 1437204481);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int CloseFusion(int deviceID)
	{
		try
		{
			SPHelper.SendTORotor(deviceID, 2, 95, 0);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static int ResetFusionXY(int deviceID)
	{
		try
		{
			SPHelper.SendTORotor(deviceID, 2, 99, 1280);
			SPHelper.SendTORotor(deviceID, 2, 100, 512);
			SPHelper.SendTORotor(deviceID, 2, 105, 1280);
			SPHelper.SendTORotor(deviceID, 2, 106, 2048);
			SPHelper.SendTORotor(deviceID, 2, 111, 512);
			SPHelper.SendTORotor(deviceID, 2, 112, 1280);
			SPHelper.SendTORotor(deviceID, 2, 117, 2048);
			SPHelper.SendTORotor(deviceID, 2, 118, 1280);
			return 0;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
