using System;

namespace MetaStudio;

public class SPHelper
{
	public static Form1 mainFrm = null;

	public static void SendTORotor(int Id, int cmd, int addr, int data)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 129, 0, 0, 128, 2, 0,
			8, 0, 0, 0, 204, 204, 1, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		array[6] = (byte)Id;
		byte[] bytes = BitConverter.GetBytes(addr);
		array[16] = bytes[0];
		array[17] = bytes[1];
		array[18] = bytes[2];
		array[19] = bytes[3];
		array[8] = (byte)cmd;
		byte[] bytes2 = BitConverter.GetBytes(data);
		array[20] = bytes2[0];
		array[21] = bytes2[1];
		array[22] = bytes2[2];
		array[23] = bytes2[3];
		if (mainFrm != null)
		{
			mainFrm.Send(array);
			if (addr == 16)
			{
				PrintBuf(array);
			}
		}
	}

	public static void SendTORotor(int Id, int cmd, int addr, uint data)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 129, 0, 0, 128, 2, 0,
			8, 0, 0, 0, 204, 204, 1, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		array[6] = (byte)Id;
		byte[] bytes = BitConverter.GetBytes(addr);
		array[16] = bytes[0];
		array[17] = bytes[1];
		array[18] = bytes[2];
		array[19] = bytes[3];
		array[8] = (byte)cmd;
		byte[] bytes2 = BitConverter.GetBytes(data);
		array[20] = bytes2[0];
		array[21] = bytes2[1];
		array[22] = bytes2[2];
		array[23] = bytes2[3];
		if (mainFrm != null)
		{
			mainFrm.Send(array);
			if (addr == 16)
			{
				PrintBuf(array);
			}
		}
	}

	public static void SendTOVdbox(int Id, int cmd, int addr, int data)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 1, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 5, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		array[6] = (byte)Id;
		byte[] bytes = BitConverter.GetBytes(addr);
		array[16] = bytes[0];
		array[17] = bytes[1];
		array[18] = bytes[2];
		array[19] = bytes[3];
		array[8] = (byte)cmd;
		byte[] bytes2 = BitConverter.GetBytes(data);
		array[20] = bytes2[0];
		array[21] = bytes2[1];
		array[22] = bytes2[2];
		array[23] = bytes2[3];
		if (mainFrm != null)
		{
			mainFrm.Send(array);
		}
	}

	public static void SendTOStator(int Id, int cmd, int addr, int data)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 129, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 1, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		array[6] = (byte)Id;
		byte[] bytes = BitConverter.GetBytes(addr);
		array[16] = bytes[0];
		array[17] = bytes[1];
		array[18] = bytes[2];
		array[19] = bytes[3];
		array[8] = (byte)cmd;
		byte[] bytes2 = BitConverter.GetBytes(data);
		array[20] = bytes2[0];
		array[21] = bytes2[1];
		array[22] = bytes2[2];
		array[23] = bytes2[3];
		if (mainFrm != null)
		{
			mainFrm.Send(array);
		}
	}

	public static bool CheckHead(byte[] buf)
	{
		if (buf == null || buf.Length < 4)
		{
			return false;
		}
		if (buf[0] != 240)
		{
			return false;
		}
		if (buf[1] != 165)
		{
			return false;
		}
		if (buf[2] != 90)
		{
			return false;
		}
		if (buf[3] != 15)
		{
			return false;
		}
		return true;
	}

	public static void Factor_Sclr(int Id, int width_in, int heigth_in, int width_out, int heigth_out)
	{
		ushort num = (ushort)(width_in * 2048 / width_out);
		ushort num2 = (ushort)(heigth_in * 2048 / heigth_out);
		SendTOStator(Id, 2, 37, (num << 16) | num2);
	}

	public static uint ConvetUInt(byte[] buf, int offset)
	{
		return BitConverter.ToUInt32(new byte[4]
		{
			buf[offset],
			buf[offset + 1],
			buf[offset + 2],
			buf[offset + 3]
		}, 0);
	}

	public static int ConvetInt(byte[] buf, int offset)
	{
		return BitConverter.ToInt32(new byte[4]
		{
			buf[offset],
			buf[offset + 1],
			buf[offset + 2],
			buf[offset + 3]
		}, 0);
	}

	public static int ConvetShort(byte[] buf, int offset)
	{
		return BitConverter.ToInt16(new byte[2]
		{
			buf[offset],
			buf[offset + 1]
		}, 0);
	}

	public static string PrintBuf(byte[] buf)
	{
		string text = string.Empty;
		for (int i = 0; i < buf.Length; i++)
		{
			text = text + " " + buf[i].ToString("X");
		}
		return text;
	}
}
