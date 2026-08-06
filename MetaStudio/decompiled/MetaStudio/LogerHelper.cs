using System;
using System.IO;
using System.Windows.Forms;

namespace MetaStudio;

public class LogerHelper
{
	private static StreamWriter sw = null;

	private static StreamWriter sw2 = null;

	public static void Info(string info)
	{
		try
		{
			string text = "yyyy_MMdd_HHmmss";
			DateTime now = DateTime.Now;
			string text2 = "Log_Info";
			DateTime now2 = DateTime.Now;
			string value = string.Concat(new object[5] { "[", now2, "] ", info, "\r\n" });
			string path = Path.Combine(Application.StartupPath, "log") + "\\" + text2 + "_" + now.ToString(text) + ".log";
			if (sw == null)
			{
				sw = new StreamWriter(path, append: true);
			}
			sw.Write(value);
			sw.Flush();
		}
		catch (Exception)
		{
		}
	}

	public static void Error(string info)
	{
		try
		{
			string text = "yyyy_MMdd_HHmmss";
			DateTime now = DateTime.Now;
			string text2 = "Log_Error";
			DateTime now2 = DateTime.Now;
			string value = string.Concat(new object[5] { "[", now2, "] ", info, "\r\n" });
			string path = Path.Combine(Application.StartupPath, "log") + "\\" + text2 + "_" + now.ToString(text) + ".log";
			if (sw2 == null)
			{
				sw2 = new StreamWriter(path, append: true);
			}
			sw2.Write(value);
			sw2.Flush();
		}
		catch (Exception)
		{
		}
	}

	public void AddLogToFile(byte[] buf)
	{
		try
		{
			string text = DateTime.Now.ToString() + " ->Send:";
			for (int i = 0; i < buf.Length; i++)
			{
				text = text + " " + buf[i].ToString("X");
			}
			Info(text);
		}
		catch (Exception ex)
		{
			Error(ex.Message);
		}
	}
}
