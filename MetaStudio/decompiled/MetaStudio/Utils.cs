using System;

namespace MetaStudio;

public class Utils
{
	public static byte GetStatorID(string txtID)
	{
		byte result = 1;
		if (!string.IsNullOrEmpty(txtID))
		{
			result = (byte)Convert.ToInt32(txtID, 16);
		}
		return result;
	}

	public static short GetRGB(int ori_value, int value)
	{
		short num = (short)(ori_value & 0xFE01);
		short num2 = (short)(value << 1);
		return (short)(num | num2);
	}
}
