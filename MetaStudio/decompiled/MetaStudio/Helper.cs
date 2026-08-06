using System;
using System.Collections.Generic;
using System.Drawing;
using System.Management;
using System.Windows.Forms;

namespace MetaStudio;

public class Helper
{
	public static bool IsNumeric(string input)
	{
		int result;
		return int.TryParse(input, out result);
	}

	public static List<string> GetPortDeviceName()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		List<string> list = new List<string>();
		ManagementObjectSearcher val = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM%'");
		try
		{
			ManagementObjectCollection val2 = val.Get();
			ManagementObjectEnumerator enumerator = val2.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ManagementBaseObject current = enumerator.Current;
					if (current.Properties["Name"].Value != null)
					{
						string text = current.Properties["Name"].Value.ToString();
						if (text.Contains("CH340"))
						{
							list.Add(text);
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return list;
	}

	public static bool ContainerCom(string serialName, List<string> lst)
	{
		foreach (string item in lst)
		{
			if (item.Contains(serialName))
			{
				return true;
			}
		}
		return false;
	}

	private static string getcom(string str)
	{
		return str.Substring(str.Length - 6, 5);
	}

	public static bool CheckStringIsDigit(string str, out int val)
	{
		if (!int.TryParse(str, out val))
		{
			return false;
		}
		return true;
	}

	public static ushort GetUShort(byte[] buf, int offset)
	{
		ushort num = 0;
		num |= GetByte(buf, offset);
		return (ushort)(num | (ushort)(GetByte(buf, offset + 1) << 8));
	}

	public static byte GetByte(byte[] buf, int offset)
	{
		if (offset >= buf.Length)
		{
			return 0;
		}
		return buf[offset];
	}

	public static byte[] CheckCRC(byte[] buf)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		if (buf == null || buf.Length == 0)
		{
			return buf;
		}
		byte[] array = new byte[buf.Length];
		for (int i = 0; i < buf.Length; i++)
		{
			array[i] = buf[i];
		}
		try
		{
			int num = 0;
			for (int i = 4; i <= buf.Length - 2; i += 2)
			{
				int uShort = GetUShort(buf, i);
				if (i == 14 || i == buf.Length - 2)
				{
					byte[] bytes = BitConverter.GetBytes(num);
					array[i] = bytes[0];
					array[i + 1] = bytes[1];
					num = 0;
				}
				else
				{
					num ^= uShort;
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		}
		return array;
	}

	public static void DrawEllipse(Graphics g, int row, int col, int offsetX, int offsetY, bool isbottom)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		Brush greenBrush = (Brush)new SolidBrush(Color.Green);
		Brush greenBrush2 = (Brush)new SolidBrush(Color.Gray);
		if (isbottom)
		{
			DrawGreenImage(g, row, col, offsetX, offsetY, greenBrush, greenBrush2);
			DrawOrangeImage(g, row, col, offsetX, offsetY, greenBrush, greenBrush2);
		}
		else
		{
			DrawOrangeImage(g, row, col, offsetX, offsetY, greenBrush, greenBrush2);
			DrawGreenImage(g, row, col, offsetX, offsetY, greenBrush, greenBrush2);
		}
	}

	public static void DrawOrangeImage(Graphics g, int row, int col, int offsetX, int offsetY, Brush greenBrush, Brush greenBrush2)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		int num = 100;
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < col; j++)
			{
				if (i % 2 == 0)
				{
					if (j % 2 == 0)
					{
						g.FillEllipse(greenBrush, offsetX + j * 90, offsetY + i * 90, num, num);
						if (i + "-" + j == ConstData.selKey)
						{
							g.FillEllipse((Brush)new SolidBrush(Color.Red), offsetX + j * 90, offsetY + i * 90, num, num);
						}
					}
				}
				else if (j % 2 != 0)
				{
					g.FillEllipse(greenBrush, offsetX + j * 90, offsetY + i * 90, num, num);
					if (i + "-" + j == ConstData.selKey)
					{
						g.FillEllipse((Brush)new SolidBrush(Color.Red), offsetX + j * 90, offsetY + i * 90, num, num);
					}
				}
			}
		}
	}

	public static void DrawGreenImage(Graphics g, int row, int col, int offsetX, int offsetY, Brush greenBrush, Brush greenBrush2)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		int num = 100;
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < col; j++)
			{
				if (i % 2 == 0)
				{
					if (j % 2 != 0)
					{
						g.FillEllipse(greenBrush2, offsetX + j * 90, offsetY + i * 90, num, num);
						if (i + "-" + j == ConstData.selKey)
						{
							g.FillEllipse((Brush)new SolidBrush(Color.Red), offsetX + j * 90, offsetY + i * 90, num, num);
						}
					}
				}
				else if (j % 2 == 0)
				{
					g.FillEllipse(greenBrush2, offsetX + j * 90, offsetY + i * 90, num, num);
					if (i + "-" + j == ConstData.selKey)
					{
						g.FillEllipse((Brush)new SolidBrush(Color.Red), offsetX + j * 90, offsetY + i * 90, num, num);
					}
				}
			}
		}
	}

	public static void DrawString6(Graphics g, Point p, string drawString, int ori_offsetX, int ori_offsetY)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		if (string.IsNullOrEmpty(drawString))
		{
			return;
		}
		Brush val = (Brush)new SolidBrush(Color.White);
		Pen val2 = new Pen(Color.Blue, 1f);
		Font val3 = new Font("微软雅黑", 14f, (FontStyle)1, (GraphicsUnit)3);
		try
		{
			if (!string.IsNullOrEmpty(drawString))
			{
				g.DrawString(drawString, val3, val, (PointF)p);
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	public static string DrawString(Graphics g, Point p, string drawString, int ori_offsetX, int ori_offsetY)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if (string.IsNullOrEmpty(drawString))
		{
			return string.Empty;
		}
		Brush val = (Brush)new SolidBrush(Color.White);
		Pen val2 = new Pen(Color.Blue, 1f);
		string result = "";
		Font val3 = new Font("微软雅黑", 14f, (FontStyle)1, (GraphicsUnit)3);
		try
		{
			if (!string.IsNullOrEmpty(drawString))
			{
				g.DrawString(drawString, val3, val, (PointF)p);
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		return result;
	}

	public static string GetKey2(Dictionary<string, Point> dicPoint, Point p)
	{
		string empty = string.Empty;
		foreach (KeyValuePair<string, Point> item in dicPoint)
		{
			if (Math.Abs(p.X - item.Value.X) <= 40 && Math.Abs(p.Y - item.Value.Y) <= 40)
			{
				return item.Key;
			}
		}
		return empty;
	}

	public static void DrawString3(Graphics g, int _col, int _row, string drawString, int ori_offsetX, int ori_offsetY)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		if (string.IsNullOrEmpty(drawString))
		{
			return;
		}
		Brush val = (Brush)new SolidBrush(Color.White);
		Pen val2 = new Pen(Color.Blue, 1f);
		Point point = new Point(ori_offsetX + _col * 90, ori_offsetY + _row * 90);
		Font val3 = new Font("微软雅黑", 14f, (FontStyle)1, (GraphicsUnit)3);
		try
		{
			if (!string.IsNullOrEmpty(drawString))
			{
				g.DrawString(drawString, val3, val, (PointF)point);
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	public static void DrawString2(Graphics g, Point point, string drawString)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		Brush val = (Brush)new SolidBrush(Color.White);
		Pen val2 = new Pen(Color.Blue, 1f);
		Font val3 = new Font("微软雅黑", 14f, (FontStyle)1, (GraphicsUnit)3);
		try
		{
			if (!string.IsNullOrEmpty(drawString))
			{
				g.DrawString(drawString, val3, val, (PointF)point);
			}
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	public static int Encryption(int value)
	{
		return value << 2;
	}

	public static int Decryption(int value)
	{
		return value >> 2;
	}
}
