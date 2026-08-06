using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetaStudio;

public class DataParser
{
	public static void ResetRotor(TextBox tb, int id, string filepath)
	{
		try
		{
			Task task = new Task(delegate
			{
				//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
				//IL_01b2: Expected O, but got Unknown
				//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ef: Expected O, but got Unknown
				MethodInvoker val = null;
				MethodInvoker val2 = null;
				string path = Path.Combine(Application.StartupPath, "sysConfig") + "\\Rotor_Init2.txt";
				string text = File.ReadAllText(path);
				string[] array = File.ReadAllLines(path);
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (!string.IsNullOrEmpty(text2))
					{
						string[] array3 = text2.Split(new char[1] { '=' });
						int num = Convert.ToInt32(array3[0], 16);
						int data = Convert.ToInt32(array3[1], 16);
						if (num != 55 && num != 56 && num != 57)
						{
							SPHelper.SendTORotor(id, 2, num, data);
						}
						Thread.Sleep(50);
						if (((Control)tb).IsHandleCreated)
						{
							TextBox obj = tb;
							if (val == null)
							{
								val = (MethodInvoker)delegate
								{
									((TextBoxBase)tb).AppendText("Processing.....\r\n");
								};
							}
							((Control)obj).BeginInvoke((Delegate)(object)val);
						}
					}
				}
				SPHelper.SendTORotor(id, 2, 55, 0);
				SPHelper.SendTORotor(id, 2, 55, 1);
				Thread.Sleep(1000);
				SPHelper.SendTORotor(id, 2, 55, 0);
				Thread.Sleep(2000);
				SPHelper.SendTOStator(id, 2, 18, 90);
				Thread.Sleep(1000);
				SPHelper.SendTOStator(id, 2, 18, 0);
				if (((Control)tb).IsHandleCreated)
				{
					TextBox obj2 = tb;
					if (val2 == null)
					{
						val2 = (MethodInvoker)delegate
						{
							((TextBoxBase)tb).AppendText("Success.....\r\n");
						};
					}
					((Control)obj2).BeginInvoke((Delegate)(object)val2);
				}
			});
			task.Start();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.ToString());
		}
	}

	public static void ResetStator(TextBox tb, int id, string filepath)
	{
		try
		{
			Task task = new Task(delegate
			{
				//IL_016b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0172: Expected O, but got Unknown
				//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ca: Expected O, but got Unknown
				MethodInvoker val = null;
				MethodInvoker val2 = null;
				string path = Path.Combine(Application.StartupPath, "sysConfig") + "\\Stator_Init2.txt";
				string text = File.ReadAllText(path);
				string[] array = File.ReadAllLines(path);
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (!string.IsNullOrEmpty(text2))
					{
						string[] array3 = text2.Split(new char[1] { '=' });
						int addr = Convert.ToInt32(array3[0], 16);
						int data = Convert.ToInt32(array3[1], 16);
						SPHelper.SendTOStator(id, 2, addr, data);
						Thread.Sleep(50);
						if (((Control)tb).IsHandleCreated)
						{
							TextBox obj = tb;
							if (val == null)
							{
								val = (MethodInvoker)delegate
								{
									((TextBoxBase)tb).AppendText("Processing.....\r\n");
								};
							}
							((Control)obj).BeginInvoke((Delegate)(object)val);
						}
					}
				}
				SPHelper.SendTOStator(id, 2, 63, 0);
				SPHelper.SendTOStator(id, 2, 63, 1);
				Thread.Sleep(1000);
				SPHelper.SendTOStator(id, 2, 63, 0);
				Thread.Sleep(2000);
				SPHelper.SendTOStator(id, 2, 16, 10);
				if (((Control)tb).IsHandleCreated)
				{
					TextBox obj2 = tb;
					if (val2 == null)
					{
						val2 = (MethodInvoker)delegate
						{
							((TextBoxBase)tb).AppendText("Success.....\r\n");
						};
					}
					((Control)obj2).BeginInvoke((Delegate)(object)val2);
				}
			});
			task.Start();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.ToString());
		}
	}

	public static void ResetVdbox(TextBox tb, int id, string filepath)
	{
		try
		{
			Task task = new Task(delegate
			{
				//IL_014f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0156: Expected O, but got Unknown
				//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ca: Expected O, but got Unknown
				MethodInvoker val = null;
				MethodInvoker val2 = null;
				string path = Path.Combine(Application.StartupPath, "sysConfig") + "\\Vdbox_Init2.txt";
				string text = File.ReadAllText(path);
				string[] array = File.ReadAllLines(path);
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (!string.IsNullOrEmpty(text2))
					{
						string[] array3 = text2.Split(new char[1] { '=' });
						int addr = Convert.ToInt32(array3[0], 16);
						int data = Convert.ToInt32(array3[1], 16);
						SPHelper.SendTOVdbox(id, 2, addr, data);
						Thread.Sleep(50);
						if (((Control)tb).IsHandleCreated)
						{
							TextBox obj = tb;
							if (val == null)
							{
								val = (MethodInvoker)delegate
								{
									((TextBoxBase)tb).AppendText("Processing.....\r\n");
								};
							}
							((Control)obj).BeginInvoke((Delegate)(object)val);
						}
					}
				}
				SPHelper.SendTOVdbox(id, 2, 31, 0);
				SPHelper.SendTOVdbox(id, 2, 31, 1);
				Thread.Sleep(1000);
				SPHelper.SendTOVdbox(id, 2, 31, 0);
				if (((Control)tb).IsHandleCreated)
				{
					TextBox obj2 = tb;
					if (val2 == null)
					{
						val2 = (MethodInvoker)delegate
						{
							((TextBoxBase)tb).AppendText("Success.....\r\n");
						};
					}
					((Control)obj2).BeginInvoke((Delegate)(object)val2);
				}
			});
			task.Start();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.ToString());
		}
	}
}
