using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Sunny.UI;

namespace MetaStudio;

public class UserControl4 : UserControl
{
	private System.Timers.Timer timer = new System.Timers.Timer();

	private int R_reg_0;

	private int R_reg_1;

	private int R_reg_2;

	private int R_reg_3;

	private int G_reg_0;

	private int G_reg_1;

	private int G_reg_2;

	private int G_reg_3;

	private int B_reg_0;

	private int B_reg_1;

	private int B_reg_2;

	private int B_reg_3;

	private Form1 frm = null;

	private List<int> lst = new List<int>();

	private int index2 = 0;

	private int count = 0;

	private List<RegInfo> lstReg = new List<RegInfo>();

	private StringBuilder strRes = new StringBuilder();

	private IContainer components = null;

	private UIPanel uiPanel7;

	private UIGroupBox uiGroupBox1;

	private UITextBox txtB3;

	private UITextBox txtG3;

	private UITextBox txtR3;

	private UILabel uiLabel21;

	private UILabel uiLabel17;

	private UILabel uiLabel13;

	private UITextBox txtB2;

	private UITextBox txtG2;

	private UITextBox txtR2;

	private UILabel uiLabel20;

	private UILabel uiLabel16;

	private UILabel uiLabel6;

	private UITextBox txtB1;

	private UITextBox txtG1;

	private UITextBox txtR1;

	private UILabel uiLabel19;

	private UILabel uiLabel15;

	private UILabel uiLabel5;

	private UITextBox txtB0;

	private UILabel uiLabel18;

	private UITextBox txtG0;

	private UILabel uiLabel14;

	private UITextBox txtR0;

	private UILabel uiLabel4;

	private UIButton btnReadAll;

	private UIButton btnWriteAll;

	private UIButton btnRead_B;

	private UIButton btnWrite_B;

	private UIButton btnRead_G;

	private UIButton btnRead_R;

	private UIButton btnWrite_G;

	private UIButton btnWrite_R;

	private UILabel lbl_B;

	private UILabel lbl_G;

	private UILabel lbl_R;

	private UIButton uiButton16;

	private UIButton uiButton17;

	private UIButton uiButton4;

	private UILabel uiLabel7;

	private UIButton btnView;

	private UITextBox txtPath;

	private UITextBox txtLog;

	private UIButton btnWriteData;

	private UIButton btnClear;

	private UIButton btnTestSpeed;

	private UITextBox txtID;

	private UILabel uiLabel1;

	private UIButton btnTimerWrite;

	private UIButton btnReadConfig;

	private UITrackBar hsb_B;

	private UITrackBar hsb_G;

	private UITrackBar hsb_R;

	private UIButton btnJianR;

	private UIButton btnJiaR;

	private UIButton btnJiaB;

	private UIButton btnJiaG;

	private UIButton btnJianB;

	private UIButton btnJianG;

	private UIComboBox cbxColor;

	private UIComboBox cbxType;

	private UIButton btnResetRotor;

	private UIButton btnResetVdbox;

	private UIButton btnResetStator;

	private UIButton uiButton1;

	private UIButton btnUpdateDebug;

	public UserControl4(Form1 frm)
	{
		InitializeComponent();
		this.frm = frm;
		((Control)btnWrite_R).Click += btnWrite_R_Click;
		((Control)btnRead_R).Click += btnRead_R_Click;
		((Control)btnWrite_G).Click += btnWrite_G_Click;
		((Control)btnRead_G).Click += btnRead_G_Click;
		((Control)btnWrite_B).Click += btnWrite_B_Click;
		((Control)btnRead_B).Click += btnRead_B_Click;
		((Control)btnReadAll).Click += btnReadAll_Click;
		((Control)btnWriteAll).Click += btnWriteAll_Click;
		hsb_B.ValueChanged += hsb_B_ValueChanged;
		hsb_G.ValueChanged += hsb_G_ValueChanged;
		hsb_R.ValueChanged += hsb_R_ValueChanged;
		timer.Elapsed += timer_Elapsed;
		timer.Enabled = false;
		timer.Interval = 20.0;
		cbxType.SelectedIndex = 0;
	}

	private void timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		byte id = 1;
		if (!string.IsNullOrEmpty(((Control)txtID).Text))
		{
			id = (byte)Convert.ToInt32(((Control)txtID).Text, 16);
		}
		SPHelper.SendTORotor(id, 1, 240, 0);
		Thread.Sleep(1);
		SPHelper.SendTORotor(id, 1, 241, 0);
	}

	private void ShowRegOnControl(int reg, UITextBox tb)
	{
		((Control)this).Invoke((Delegate)(EventHandler)delegate
		{
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (reg.ToString("X").Length == 4)
				{
					((Control)tb).Text = "0x" + reg.ToString("X");
				}
				else
				{
					((Control)tb).Text = "0x00" + reg.ToString("X");
				}
				if (((Control)tb).Name == "txtR1")
				{
					hsb_R.Value = (reg & 0x1FE) >> 1;
					((Control)lbl_R).Text = hsb_R.Value.ToString();
				}
				else if (((Control)tb).Name == "txtG1")
				{
					hsb_G.Value = (reg & 0x1FE) >> 1;
					((Control)lbl_G).Text = hsb_G.Value.ToString();
				}
				else if (((Control)tb).Name == "txtB1")
				{
					hsb_B.Value = (reg & 0x1FE) >> 1;
					((Control)lbl_B).Text = hsb_B.Value.ToString();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
			}
		});
	}

	public void GetSerRegData(byte[] receivedData)
	{
		if (!SPHelper.CheckHead(receivedData) || receivedData.Length != 26)
		{
			return;
		}
		((Control)this).Invoke((Delegate)(EventHandler)delegate
		{
			string text = "0x" + receivedData[16].ToString("X") + "=0x" + SPHelper.ConvetInt(receivedData, 20).ToString("X");
			AddLog("Processing.....");
			strRes.Append(text + "+");
		});
		if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 33)
		{
			R_reg_0 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(R_reg_0, txtR0);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 34)
		{
			G_reg_0 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(G_reg_0, txtG0);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 35)
		{
			B_reg_0 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(B_reg_0, txtB0);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 36)
		{
			R_reg_1 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(R_reg_1, txtR1);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 37)
		{
			G_reg_1 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(G_reg_1, txtG1);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 38)
		{
			B_reg_1 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(B_reg_1, txtB1);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 39)
		{
			R_reg_2 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(R_reg_2, txtR2);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 40)
		{
			G_reg_2 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(G_reg_2, txtG2);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 41)
		{
			B_reg_2 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(B_reg_2, txtB2);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 42)
		{
			R_reg_3 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(R_reg_3, txtR3);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 43)
		{
			G_reg_3 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(G_reg_3, txtG3);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 44)
		{
			B_reg_3 = BitConverter.ToInt16(new byte[2]
			{
				receivedData[20],
				receivedData[21]
			}, 0);
			ShowRegOnControl(B_reg_3, txtB3);
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] == 244)
		{
			uint num = SPHelper.ConvetUInt(receivedData, 20);
			string value = index2 + ":0x" + num.ToString("X");
			index2++;
			Console.WriteLine(value);
			if (index2 == 1024)
			{
				index2 = 0;
				Console.WriteLine("-----------------分割线---------------------");
			}
		}
		else if (receivedData[4] == 129 && receivedData[7] == 128 && receivedData[16] != 241)
		{
		}
	}

	private void hsb_R_ValueChanged(object sender, EventArgs e)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			((Control)lbl_R).Text = hsb_R.Value.ToString();
			if (!string.IsNullOrEmpty(((Control)txtR1).Text))
			{
				short num = (short)Convert.ToInt32(((Control)txtR1).Text, 16);
				short num2 = (short)hsb_R.Value;
				short num3 = (short)(num & 0xFE01);
				short num4 = (short)(num2 << 1);
				((Control)txtR1).Text = "0x" + ((short)(num3 | num4)).ToString("X");
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		}
	}

	private void hsb_G_ValueChanged(object sender, EventArgs e)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			((Control)lbl_G).Text = hsb_G.Value.ToString();
			if (!string.IsNullOrEmpty(((Control)txtG1).Text))
			{
				short num = (short)Convert.ToInt32(((Control)txtG1).Text, 16);
				short num2 = (short)hsb_G.Value;
				short num3 = (short)(num & 0xFE01);
				short num4 = (short)(num2 << 1);
				((Control)txtG1).Text = "0x" + ((short)(num3 | num4)).ToString("X");
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		}
	}

	private void hsb_B_ValueChanged(object sender, EventArgs e)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			((Control)lbl_B).Text = hsb_B.Value.ToString();
			if (!string.IsNullOrEmpty(((Control)txtB1).Text))
			{
				short num = (short)Convert.ToInt32(((Control)txtB1).Text, 16);
				short num2 = (short)hsb_B.Value;
				short num3 = (short)(num & 0xFE01);
				short num4 = (short)(num2 << 1);
				((Control)txtB1).Text = "0x" + ((short)(num3 | num4)).ToString("X");
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		}
	}

	private void btnWrite_R_Click(object sender, EventArgs e)
	{
		byte statorID = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(statorID, 1, 36, 0);
		Thread.Sleep(300);
		short num = (short)hsb_R.Value;
		short num2 = (short)R_reg_1;
		short num3 = (short)(num2 & 0xFE01);
		short num4 = (short)(num << 1);
		short data = (short)(num3 | num4);
		SPHelper.SendTORotor(statorID, 2, 36, data);
		EnableReg();
	}

	private void btnRead_R_Click(object sender, EventArgs e)
	{
		byte statorID = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(statorID, 1, 36, 0);
		Thread.Sleep(300);
		((Control)this).Invoke((Delegate)(EventHandler)delegate
		{
			((Control)txtR1).Text = "0x" + R_reg_1.ToString("X");
		});
	}

	private void btnWrite_G_Click(object sender, EventArgs e)
	{
		byte id = Utils.GetStatorID(((Control)txtID).Text);
		Task task = new Task(delegate
		{
			SPHelper.SendTORotor(id, 1, 37, 0);
			Thread.Sleep(300);
			short num = (short)hsb_G.Value;
			short num2 = (short)G_reg_1;
			short num3 = (short)(num2 & 0xFE01);
			short num4 = (short)(num << 1);
			short data = (short)(num3 | num4);
			SPHelper.SendTORotor(id, 2, 37, data);
			EnableReg();
		});
		task.Start();
	}

	private void btnRead_G_Click(object sender, EventArgs e)
	{
		byte statorID = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(statorID, 1, 37, 0);
		Thread.Sleep(300);
		((Control)this).Invoke((Delegate)(EventHandler)delegate
		{
			((Control)txtG1).Text = "0x" + G_reg_1.ToString("X");
		});
	}

	private void btnWrite_B_Click(object sender, EventArgs e)
	{
		byte id = Utils.GetStatorID(((Control)txtID).Text);
		Task task = new Task(delegate
		{
			SPHelper.SendTORotor(id, 1, 38, 0);
			Thread.Sleep(300);
			short num = (short)hsb_B.Value;
			short num2 = (short)B_reg_1;
			short num3 = (short)(num2 & 0xFE01);
			short num4 = (short)(num << 1);
			short data = (short)(num3 | num4);
			SPHelper.SendTORotor(id, 2, 38, data);
			EnableReg();
		});
		task.Start();
	}

	private void btnRead_B_Click(object sender, EventArgs e)
	{
		byte statorID = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(statorID, 1, 38, 0);
		Thread.Sleep(300);
		((Control)this).Invoke((Delegate)(EventHandler)delegate
		{
			((Control)txtB1).Text = "0x" + B_reg_1.ToString("X");
		});
	}

	private void EnableReg()
	{
		byte statorID = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(statorID, 2, 31, 0);
		Thread.Sleep(300);
		SPHelper.SendTORotor(statorID, 2, 31, 1);
		Thread.Sleep(300);
		SPHelper.SendTORotor(statorID, 2, 31, 0);
	}

	private void ReadReg(byte addr)
	{
		byte statorID = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(statorID, 1, addr, 0);
	}

	private void WriteReg(byte addr, int data)
	{
		byte statorID = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(statorID, 2, addr, data);
	}

	private void WriteReg(byte addr, uint data)
	{
		byte statorID = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(statorID, 2, addr, data);
	}

	private void btnReadAll_Click(object sender, EventArgs e)
	{
		Task task = new Task(delegate
		{
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			ReadReg(33);
			Thread.Sleep(100);
			ReadReg(34);
			Thread.Sleep(100);
			ReadReg(35);
			Thread.Sleep(100);
			ReadReg(36);
			Thread.Sleep(100);
			ReadReg(37);
			Thread.Sleep(100);
			ReadReg(38);
			Thread.Sleep(100);
			ReadReg(39);
			Thread.Sleep(100);
			ReadReg(40);
			Thread.Sleep(100);
			ReadReg(41);
			Thread.Sleep(100);
			ReadReg(42);
			Thread.Sleep(100);
			ReadReg(43);
			Thread.Sleep(100);
			ReadReg(44);
			MessageBox.Show("寄存器回读成功！", "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		});
		task.Start();
	}

	private void btnWriteAll_Click(object sender, EventArgs e)
	{
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!string.IsNullOrEmpty(((Control)txtR0).Text))
			{
				int data = Convert.ToInt32(((Control)txtR0).Text, 16);
				WriteReg(33, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtG0).Text))
			{
				int data = Convert.ToInt32(((Control)txtG0).Text, 16);
				WriteReg(34, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtB0).Text))
			{
				int data = Convert.ToInt32(((Control)txtB0).Text, 16);
				WriteReg(35, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtR1).Text))
			{
				int data = Convert.ToInt32(((Control)txtR1).Text, 16);
				WriteReg(36, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtG1).Text))
			{
				int data = Convert.ToInt32(((Control)txtG1).Text, 16);
				WriteReg(37, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtB1).Text))
			{
				int data = Convert.ToInt32(((Control)txtB1).Text, 16);
				WriteReg(38, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtR2).Text))
			{
				int data = Convert.ToInt32(((Control)txtR2).Text, 16);
				WriteReg(39, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtG2).Text))
			{
				int data = Convert.ToInt32(((Control)txtG2).Text, 16);
				WriteReg(40, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtB2).Text))
			{
				int data = Convert.ToInt32(((Control)txtB2).Text, 16);
				WriteReg(41, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtR3).Text))
			{
				int data = Convert.ToInt32(((Control)txtR3).Text, 16);
				WriteReg(42, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtG3).Text))
			{
				int data = Convert.ToInt32(((Control)txtG3).Text, 16);
				WriteReg(43, data);
				Thread.Sleep(100);
			}
			if (!string.IsNullOrEmpty(((Control)txtB3).Text))
			{
				int data = Convert.ToInt32(((Control)txtB3).Text, 16);
				WriteReg(44, data);
			}
			EnableReg();
			MessageBox.Show("寄存器写入成功！", "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		}
	}

	private void btnView_Click(object sender, EventArgs e)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		try
		{
			OpenFileDialog val = new OpenFileDialog();
			((FileDialog)val).Filter = "文件类型(*.txt)|*.txt";
			((FileDialog)val).Title = "请选择一个txt格式文件";
			((FileDialog)val).InitialDirectory = "C:";
			val.ShowReadOnly = true;
			val.ReadOnlyChecked = true;
			((FileDialog)val).ShowHelp = true;
			if (!(((int)((CommonDialog)val).ShowDialog() == 1) & (((FileDialog)val).FileNames.Length > 0)))
			{
				return;
			}
			txtLog.Clear();
			string text = ((FileDialog)val).FileNames[0].Substring(((FileDialog)val).FileNames[0].LastIndexOf("\\") + 1);
			string text2 = ((FileDialog)val).FileNames[0].Substring(0, ((FileDialog)val).FileNames[0].LastIndexOf("\\"));
			string fileName = ((FileDialog)val).FileName;
			((Control)txtPath).Text = ((FileDialog)val).FileName;
			lst = new List<int>();
			lstReg = new List<RegInfo>();
			IEnumerable<string> enumerable = File.ReadLines(fileName);
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string item in enumerable)
			{
				if (num > 6)
				{
					lst.Add(Convert.ToInt32(item, 16));
					Console.WriteLine(item);
				}
				num++;
			}
			((Control)txtLog).Text = stringBuilder.ToString();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void btnWriteData_Click(object sender, EventArgs e)
	{
		txtLog.Clear();
		int temp = 1073741824;
		if (cbxColor.SelectedIndex == 0)
		{
			temp = 1073741824;
		}
		else if (cbxColor.SelectedIndex == 1)
		{
			temp = 1342177280;
		}
		else if (cbxColor.SelectedIndex == 2)
		{
			temp = 1610612736;
		}
		Task task = new Task(delegate
		{
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			WriteReg(54, 2147483648u);
			AddLog("开始写入：");
			foreach (int item in lst)
			{
				int data = temp | item;
				WriteReg(54, data);
				Thread.Sleep(10);
				AddLog(data.ToString("X"));
				int data2 = 0;
				WriteReg(54, data2);
				Thread.Sleep(10);
				AddLog(data2.ToString("00000000"));
			}
			MessageBox.Show("寄存器写入成功！", "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		});
		task.Start();
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		((Control)txtLog).Text = "";
	}

	public void AddLog(string msg)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		MethodInvoker val = null;
		try
		{
			if (!((Control)this).IsHandleCreated)
			{
				return;
			}
			if (val == null)
			{
				val = (MethodInvoker)delegate
				{
					txtLog.AppendText(DateTime.Now.ToString() + ":" + msg);
					txtLog.AppendText("\r\n");
				};
			}
			((Control)this).BeginInvoke((Delegate)(object)val);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void btnTestSpeed_Click_1(object sender, EventArgs e)
	{
		byte id = Utils.GetStatorID(((Control)txtID).Text);
		SPHelper.SendTORotor(id, 2, 242, 0);
		SPHelper.SendTORotor(id, 2, 242, 1);
		SPHelper.SendTORotor(id, 2, 242, 0);
		Task task = new Task(delegate
		{
			Thread.Sleep(60000);
			int num = 0;
			do
			{
				bool flag = true;
				Thread.Sleep(10);
				SPHelper.SendTORotor(id, 2, 243, num);
				SPHelper.SendTORotor(id, 1, 244, 0);
				num++;
				if (num > 1023)
				{
					num = 0;
					count++;
				}
			}
			while (count != 1);
		});
		task.Start();
	}

	private void btnTimerWrite_Click(object sender, EventArgs e)
	{
		Task task = new Task(delegate
		{
			while (true)
			{
				EnableReg();
				Thread.Sleep(10);
			}
		});
		task.Start();
	}

	private void btnReadConfig_Click(object sender, EventArgs e)
	{
		strRes = new StringBuilder();
		string str_id = ((Control)txtID).Text;
		int index = cbxType.SelectedIndex;
		Task task = new Task(delegate
		{
			for (int i = 0; i <= 255; i++)
			{
				byte statorID = Utils.GetStatorID(str_id);
				if (index == 0)
				{
					SPHelper.SendTORotor(statorID, 1, i, 0);
				}
				else if (index == 1)
				{
					SPHelper.SendTOStator(statorID, 1, i, 0);
				}
				else if (index == 2)
				{
					SPHelper.SendTOVdbox(statorID, 1, i, 0);
				}
				Thread.Sleep(50);
			}
			Thread.Sleep(50);
			WriteToLogFile(DES3Helper.Encrypt(strRes.ToString()));
			AddLog("Success!");
		});
		task.Start();
	}

	private void WriteToLogFile(string str)
	{
		DateTime now = DateTime.Now;
		string path = Path.Combine(Application.StartupPath, "config") + "\\" + ((Control)cbxType).Text + "_" + ((Control)txtID).Text + "_" + now.ToString("yyyyMMddHHmmss") + ".txt";
		using StreamWriter streamWriter = new StreamWriter(path, append: true);
		streamWriter.WriteLine(str);
		streamWriter.Flush();
	}

	private void btnJiaR_Click(object sender, EventArgs e)
	{
		hsb_R.Value += 1;
	}

	private void btnJianR_Click(object sender, EventArgs e)
	{
		hsb_R.Value -= 1;
	}

	private void btnJiaG_Click(object sender, EventArgs e)
	{
		hsb_G.Value += 1;
	}

	private void btnJianG_Click(object sender, EventArgs e)
	{
		hsb_G.Value -= 1;
	}

	private void btnJiaB_Click(object sender, EventArgs e)
	{
		hsb_B.Value += 1;
	}

	private void btnJianB_Click(object sender, EventArgs e)
	{
		hsb_B.Value -= 1;
	}

	public void Translate(int type)
	{
		switch (type)
		{
		case 0:
			((Control)btnView).Text = "浏览";
			((Control)btnRead_R).Text = "回读";
			((Control)btnRead_G).Text = "回读";
			((Control)btnRead_B).Text = "回读";
			((Control)uiLabel7).Text = "文件路径:";
			((Control)btnWriteData).Text = "写入";
			((Control)btnClear).Text = "清除";
			((Control)btnReadConfig).Text = "回读";
			((Control)btnWriteAll).Text = "写入";
			((Control)btnReadAll).Text = "回读";
			((Control)uiGroupBox1).Text = "电流增益";
			break;
		case 1:
			((Control)btnView).Text = "Browse";
			((Control)btnRead_R).Text = "Read";
			((Control)btnRead_G).Text = "Read";
			((Control)btnRead_B).Text = "Read";
			((Control)uiLabel7).Text = "Path:";
			((Control)btnWriteData).Text = "Write";
			((Control)btnClear).Text = "Clear";
			((Control)btnReadConfig).Text = "Read";
			((Control)btnWriteAll).Text = "Write";
			((Control)btnReadAll).Text = "Read";
			((Control)uiGroupBox1).Text = "Current Gain";
			break;
		}
	}

	private void btnResetRotor_Click(object sender, EventArgs e)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if ((int)MessageBox.Show("确认执行恢复出厂设置(Y/N)？", "系统提示", (MessageBoxButtons)4, (MessageBoxIcon)64) != 6)
		{
			return;
		}
		MetaTool.Stop(0);
		try
		{
			Task task = new Task(delegate
			{
				string path = Path.Combine(Application.StartupPath, "sysConfig") + "\\Rotor_Init.txt";
				string text = File.ReadAllText(path);
				string s = DES3Helper.Decrypt(text);
				string[] array = StringEx.Split(s, "+");
				int id = Convert.ToInt32(((Control)txtID).Text, 16);
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
						AddLog("Processing.....");
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
				AddLog("Success!");
			});
			task.Start();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.ToString());
		}
	}

	private void btnResetStator_Click(object sender, EventArgs e)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if ((int)MessageBox.Show("确认执行恢复出厂设置(Y/N)？", "系统提示", (MessageBoxButtons)4, (MessageBoxIcon)64) != 6)
		{
			return;
		}
		MetaTool.Stop(0);
		try
		{
			Task task = new Task(delegate
			{
				string path = Path.Combine(Application.StartupPath, "sysConfig") + "\\Stator_Init.txt";
				string text = File.ReadAllText(path);
				string s = DES3Helper.Decrypt(text);
				string[] array = StringEx.Split(s, "+");
				int id = Convert.ToInt32(((Control)txtID).Text, 16);
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
						AddLog("Processing.....");
					}
				}
				SPHelper.SendTOStator(id, 2, 63, 0);
				SPHelper.SendTOStator(id, 2, 63, 1);
				Thread.Sleep(1000);
				SPHelper.SendTOStator(id, 2, 63, 0);
				Thread.Sleep(2000);
				SPHelper.SendTOStator(id, 2, 16, 10);
				AddLog("Success!");
			});
			task.Start();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.ToString());
		}
	}

	private void uiButton1_Click(object sender, EventArgs e)
	{
		for (int i = 0; i <= 255; i++)
		{
			SPHelper.SendTORotor(1, 2, i, 0);
			Thread.Sleep(50);
		}
	}

	private void btnResetVdbox_Click(object sender, EventArgs e)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if ((int)MessageBox.Show("确认执行恢复出厂设置(Y/N)？", "系统提示", (MessageBoxButtons)4, (MessageBoxIcon)64) != 6)
		{
			return;
		}
		MetaTool.Stop(0);
		try
		{
			Task task = new Task(delegate
			{
				string path = Path.Combine(Application.StartupPath, "sysConfig") + "\\Vdbox_Init.txt";
				string text = File.ReadAllText(path);
				string s = DES3Helper.Decrypt(text);
				string[] array = StringEx.Split(s, "+");
				int id = Convert.ToInt32(((Control)txtID).Text, 16);
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
						AddLog("Processing.....");
					}
				}
				SPHelper.SendTOVdbox(id, 2, 31, 0);
				SPHelper.SendTOVdbox(id, 2, 31, 1);
				Thread.Sleep(1000);
				SPHelper.SendTOVdbox(id, 2, 31, 0);
				AddLog("Success!");
			});
			task.Start();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.ToString());
		}
	}

	private void btnUpdateDebug_Click(object sender, EventArgs e)
	{
		Task task = new Task(delegate
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			MetaTool.SetDebugImage(0, 822);
			RegisterHelper.SaveConfig(0);
			Thread.Sleep(3000);
			MessageBox.Show("修复成功！", "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
		});
		task.Start();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((ContainerControl)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Expected O, but got Unknown
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Expected O, but got Unknown
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cd: Expected O, but got Unknown
		//IL_0b07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b11: Expected O, but got Unknown
		//IL_0d4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d55: Expected O, but got Unknown
		//IL_0f0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f17: Expected O, but got Unknown
		//IL_0f6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa6: Unknown result type (might be due to invalid IL or missing references)
		//IL_104e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1058: Expected O, but got Unknown
		//IL_10af: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_1224: Unknown result type (might be due to invalid IL or missing references)
		//IL_122e: Expected O, but got Unknown
		//IL_13ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f4: Expected O, but got Unknown
		//IL_14b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_14c1: Expected O, but got Unknown
		//IL_14e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_165f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1669: Expected O, but got Unknown
		//IL_18ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_18b7: Expected O, but got Unknown
		//IL_1afb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b05: Expected O, but got Unknown
		//IL_1b88: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d58: Expected O, but got Unknown
		//IL_1ddb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f37: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f41: Expected O, but got Unknown
		//IL_1f64: Unknown result type (might be due to invalid IL or missing references)
		//IL_2070: Unknown result type (might be due to invalid IL or missing references)
		//IL_207a: Expected O, but got Unknown
		//IL_21b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_21c1: Expected O, but got Unknown
		//IL_2244: Unknown result type (might be due to invalid IL or missing references)
		//IL_23a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_23ae: Expected O, but got Unknown
		//IL_23d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_28f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_28ff: Expected O, but got Unknown
		//IL_2930: Unknown result type (might be due to invalid IL or missing references)
		//IL_296a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a99: Unknown result type (might be due to invalid IL or missing references)
		//IL_2aa3: Expected O, but got Unknown
		//IL_2b23: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ce9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2cf3: Expected O, but got Unknown
		//IL_2d73: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f39: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f43: Expected O, but got Unknown
		//IL_2fc3: Unknown result type (might be due to invalid IL or missing references)
		//IL_3189: Unknown result type (might be due to invalid IL or missing references)
		//IL_3193: Expected O, but got Unknown
		//IL_3210: Unknown result type (might be due to invalid IL or missing references)
		//IL_33d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_33e0: Expected O, but got Unknown
		//IL_345d: Unknown result type (might be due to invalid IL or missing references)
		//IL_3623: Unknown result type (might be due to invalid IL or missing references)
		//IL_362d: Expected O, but got Unknown
		//IL_36aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_37f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_3802: Expected O, but got Unknown
		//IL_38c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_38d3: Expected O, but got Unknown
		//IL_399a: Unknown result type (might be due to invalid IL or missing references)
		//IL_39a4: Expected O, but got Unknown
		//IL_3a65: Unknown result type (might be due to invalid IL or missing references)
		//IL_3a6f: Expected O, but got Unknown
		//IL_3a95: Unknown result type (might be due to invalid IL or missing references)
		//IL_3b7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_3b88: Expected O, but got Unknown
		//IL_3bae: Unknown result type (might be due to invalid IL or missing references)
		//IL_3c97: Unknown result type (might be due to invalid IL or missing references)
		//IL_3ca1: Expected O, but got Unknown
		//IL_3cc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_3d9f: Unknown result type (might be due to invalid IL or missing references)
		//IL_3da9: Expected O, but got Unknown
		//IL_3e57: Unknown result type (might be due to invalid IL or missing references)
		//IL_3e61: Expected O, but got Unknown
		//IL_3f0f: Unknown result type (might be due to invalid IL or missing references)
		//IL_3f19: Expected O, but got Unknown
		//IL_3fd8: Unknown result type (might be due to invalid IL or missing references)
		//IL_3fe2: Expected O, but got Unknown
		//IL_4008: Unknown result type (might be due to invalid IL or missing references)
		//IL_40f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_40fb: Expected O, but got Unknown
		//IL_4121: Unknown result type (might be due to invalid IL or missing references)
		//IL_420a: Unknown result type (might be due to invalid IL or missing references)
		//IL_4214: Expected O, but got Unknown
		//IL_423a: Unknown result type (might be due to invalid IL or missing references)
		//IL_4312: Unknown result type (might be due to invalid IL or missing references)
		//IL_431c: Expected O, but got Unknown
		//IL_43ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_43d4: Expected O, but got Unknown
		//IL_4482: Unknown result type (might be due to invalid IL or missing references)
		//IL_448c: Expected O, but got Unknown
		//IL_454b: Unknown result type (might be due to invalid IL or missing references)
		//IL_4555: Expected O, but got Unknown
		//IL_457b: Unknown result type (might be due to invalid IL or missing references)
		//IL_4664: Unknown result type (might be due to invalid IL or missing references)
		//IL_466e: Expected O, but got Unknown
		//IL_4694: Unknown result type (might be due to invalid IL or missing references)
		//IL_477d: Unknown result type (might be due to invalid IL or missing references)
		//IL_4787: Expected O, but got Unknown
		//IL_47ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_4885: Unknown result type (might be due to invalid IL or missing references)
		//IL_488f: Expected O, but got Unknown
		//IL_493d: Unknown result type (might be due to invalid IL or missing references)
		//IL_4947: Expected O, but got Unknown
		//IL_49f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_49ff: Expected O, but got Unknown
		//IL_4abe: Unknown result type (might be due to invalid IL or missing references)
		//IL_4ac8: Expected O, but got Unknown
		//IL_4aeb: Unknown result type (might be due to invalid IL or missing references)
		//IL_4bc3: Unknown result type (might be due to invalid IL or missing references)
		//IL_4bcd: Expected O, but got Unknown
		//IL_4c88: Unknown result type (might be due to invalid IL or missing references)
		//IL_4c92: Expected O, but got Unknown
		//IL_4cb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_4d8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_4d97: Expected O, but got Unknown
		//IL_4e52: Unknown result type (might be due to invalid IL or missing references)
		//IL_4e5c: Expected O, but got Unknown
		//IL_4e7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_4f57: Unknown result type (might be due to invalid IL or missing references)
		//IL_4f61: Expected O, but got Unknown
		//IL_509a: Unknown result type (might be due to invalid IL or missing references)
		//IL_50a4: Expected O, but got Unknown
		//IL_5127: Unknown result type (might be due to invalid IL or missing references)
		//IL_52d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_52df: Expected O, but got Unknown
		//IL_5362: Unknown result type (might be due to invalid IL or missing references)
		//IL_5510: Unknown result type (might be due to invalid IL or missing references)
		//IL_551a: Expected O, but got Unknown
		//IL_559a: Unknown result type (might be due to invalid IL or missing references)
		//IL_5748: Unknown result type (might be due to invalid IL or missing references)
		//IL_5752: Expected O, but got Unknown
		//IL_57d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_598d: Unknown result type (might be due to invalid IL or missing references)
		//IL_5997: Expected O, but got Unknown
		//IL_5a17: Unknown result type (might be due to invalid IL or missing references)
		//IL_5bc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_5bcf: Expected O, but got Unknown
		//IL_5c4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_5dfd: Unknown result type (might be due to invalid IL or missing references)
		//IL_5e07: Expected O, but got Unknown
		//IL_5e87: Unknown result type (might be due to invalid IL or missing references)
		//IL_6042: Unknown result type (might be due to invalid IL or missing references)
		//IL_604c: Expected O, but got Unknown
		//IL_60cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_61f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_6202: Expected O, but got Unknown
		//IL_62ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_62b7: Expected O, but got Unknown
		//IL_6362: Unknown result type (might be due to invalid IL or missing references)
		//IL_636c: Expected O, but got Unknown
		//IL_6482: Unknown result type (might be due to invalid IL or missing references)
		//IL_648c: Expected O, but got Unknown
		//IL_65ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_65b7: Expected O, but got Unknown
		//IL_66ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_66f7: Expected O, but got Unknown
		//IL_68a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_68b1: Expected O, but got Unknown
		//IL_6934: Unknown result type (might be due to invalid IL or missing references)
		uiPanel7 = new UIPanel();
		btnUpdateDebug = new UIButton();
		btnResetVdbox = new UIButton();
		btnResetStator = new UIButton();
		btnResetRotor = new UIButton();
		cbxType = new UIComboBox();
		cbxColor = new UIComboBox();
		btnTimerWrite = new UIButton();
		uiLabel1 = new UILabel();
		txtID = new UITextBox();
		uiButton1 = new UIButton();
		btnTestSpeed = new UIButton();
		btnClear = new UIButton();
		btnWriteData = new UIButton();
		txtLog = new UITextBox();
		uiLabel7 = new UILabel();
		btnView = new UIButton();
		txtPath = new UITextBox();
		uiGroupBox1 = new UIGroupBox();
		btnJiaB = new UIButton();
		btnJiaG = new UIButton();
		btnJiaR = new UIButton();
		btnJianB = new UIButton();
		btnJianG = new UIButton();
		btnJianR = new UIButton();
		hsb_B = new UITrackBar();
		hsb_G = new UITrackBar();
		hsb_R = new UITrackBar();
		txtB3 = new UITextBox();
		txtG3 = new UITextBox();
		txtR3 = new UITextBox();
		uiLabel21 = new UILabel();
		uiLabel17 = new UILabel();
		uiLabel13 = new UILabel();
		txtB2 = new UITextBox();
		txtG2 = new UITextBox();
		txtR2 = new UITextBox();
		uiLabel20 = new UILabel();
		uiLabel16 = new UILabel();
		uiLabel6 = new UILabel();
		txtB1 = new UITextBox();
		txtG1 = new UITextBox();
		txtR1 = new UITextBox();
		uiLabel19 = new UILabel();
		uiLabel15 = new UILabel();
		uiLabel5 = new UILabel();
		txtB0 = new UITextBox();
		uiLabel18 = new UILabel();
		txtG0 = new UITextBox();
		uiLabel14 = new UILabel();
		txtR0 = new UITextBox();
		uiLabel4 = new UILabel();
		btnReadAll = new UIButton();
		btnWriteAll = new UIButton();
		btnRead_B = new UIButton();
		btnWrite_B = new UIButton();
		btnRead_G = new UIButton();
		btnRead_R = new UIButton();
		btnWrite_G = new UIButton();
		btnWrite_R = new UIButton();
		lbl_B = new UILabel();
		lbl_G = new UILabel();
		lbl_R = new UILabel();
		uiButton16 = new UIButton();
		uiButton17 = new UIButton();
		uiButton4 = new UIButton();
		btnReadConfig = new UIButton();
		((Control)uiPanel7).SuspendLayout();
		((Control)uiGroupBox1).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)uiPanel7).Controls.Add((Control)(object)btnUpdateDebug);
		((Control)uiPanel7).Controls.Add((Control)(object)btnResetVdbox);
		((Control)uiPanel7).Controls.Add((Control)(object)btnResetStator);
		((Control)uiPanel7).Controls.Add((Control)(object)btnResetRotor);
		((Control)uiPanel7).Controls.Add((Control)(object)cbxType);
		((Control)uiPanel7).Controls.Add((Control)(object)cbxColor);
		((Control)uiPanel7).Controls.Add((Control)(object)btnTimerWrite);
		((Control)uiPanel7).Controls.Add((Control)(object)uiLabel1);
		((Control)uiPanel7).Controls.Add((Control)(object)txtID);
		((Control)uiPanel7).Controls.Add((Control)(object)uiButton1);
		((Control)uiPanel7).Controls.Add((Control)(object)btnTestSpeed);
		((Control)uiPanel7).Controls.Add((Control)(object)btnClear);
		((Control)uiPanel7).Controls.Add((Control)(object)btnWriteData);
		((Control)uiPanel7).Controls.Add((Control)(object)txtLog);
		((Control)uiPanel7).Controls.Add((Control)(object)uiLabel7);
		((Control)uiPanel7).Controls.Add((Control)(object)btnView);
		((Control)uiPanel7).Controls.Add((Control)(object)txtPath);
		((Control)uiPanel7).Controls.Add((Control)(object)uiGroupBox1);
		((Control)uiPanel7).Controls.Add((Control)(object)btnReadConfig);
		((Control)uiPanel7).Dock = (DockStyle)5;
		uiPanel7.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel7).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel7).ForeColor = Color.Silver;
		((Control)uiPanel7).Location = new Point(0, 0);
		((Control)uiPanel7).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel7).MinimumSize = new Size(1, 1);
		((Control)uiPanel7).Name = "uiPanel7";
		uiPanel7.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel7).Size = new Size(1186, 755);
		uiPanel7.Style = UIStyle.Black;
		((Control)uiPanel7).TabIndex = 1;
		((Control)uiPanel7).Text = null;
		uiPanel7.TextAlignment = (ContentAlignment)32;
		((Control)btnUpdateDebug).BackColor = Color.Transparent;
		((Control)btnUpdateDebug).Cursor = Cursors.Hand;
		btnUpdateDebug.FillColor = Color.FromArgb(15, 40, 70);
		btnUpdateDebug.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnUpdateDebug.FillPressColor = Color.FromArgb(235, 243, 255);
		btnUpdateDebug.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnUpdateDebug).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnUpdateDebug.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnUpdateDebug.ForePressColor = Color.FromArgb(130, 130, 130);
		btnUpdateDebug.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnUpdateDebug).Location = new Point(721, 598);
		((Control)btnUpdateDebug).MinimumSize = new Size(1, 1);
		((Control)btnUpdateDebug).Name = "btnUpdateDebug";
		btnUpdateDebug.Radius = 25;
		btnUpdateDebug.RectColor = Color.FromArgb(130, 130, 130);
		btnUpdateDebug.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnUpdateDebug.RectPressColor = Color.FromArgb(130, 130, 130);
		btnUpdateDebug.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnUpdateDebug).Size = new Size(131, 31);
		btnUpdateDebug.Style = UIStyle.Black;
		((Control)btnUpdateDebug).TabIndex = 88;
		((Control)btnUpdateDebug).Text = "修复图像溢出";
		((Control)btnUpdateDebug).Visible = false;
		((Control)btnUpdateDebug).Click += btnUpdateDebug_Click;
		((Control)btnResetVdbox).BackColor = Color.Transparent;
		((Control)btnResetVdbox).Cursor = Cursors.Hand;
		btnResetVdbox.FillColor = Color.FromArgb(15, 40, 70);
		btnResetVdbox.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnResetVdbox.FillPressColor = Color.FromArgb(235, 243, 255);
		btnResetVdbox.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnResetVdbox).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnResetVdbox.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnResetVdbox.ForePressColor = Color.FromArgb(130, 130, 130);
		btnResetVdbox.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnResetVdbox).Location = new Point(721, 558);
		((Control)btnResetVdbox).MinimumSize = new Size(1, 1);
		((Control)btnResetVdbox).Name = "btnResetVdbox";
		btnResetVdbox.Radius = 25;
		btnResetVdbox.RectColor = Color.FromArgb(130, 130, 130);
		btnResetVdbox.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnResetVdbox.RectPressColor = Color.FromArgb(130, 130, 130);
		btnResetVdbox.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnResetVdbox).Size = new Size(131, 31);
		btnResetVdbox.Style = UIStyle.Black;
		((Control)btnResetVdbox).TabIndex = 87;
		((Control)btnResetVdbox).Text = "恢复盒子出厂设置";
		((Control)btnResetVdbox).Click += btnResetVdbox_Click;
		((Control)btnResetStator).BackColor = Color.Transparent;
		((Control)btnResetStator).Cursor = Cursors.Hand;
		btnResetStator.FillColor = Color.FromArgb(15, 40, 70);
		btnResetStator.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnResetStator.FillPressColor = Color.FromArgb(235, 243, 255);
		btnResetStator.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnResetStator).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnResetStator.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnResetStator.ForePressColor = Color.FromArgb(130, 130, 130);
		btnResetStator.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnResetStator).Location = new Point(721, 510);
		((Control)btnResetStator).MinimumSize = new Size(1, 1);
		((Control)btnResetStator).Name = "btnResetStator";
		btnResetStator.Radius = 25;
		btnResetStator.RectColor = Color.FromArgb(130, 130, 130);
		btnResetStator.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnResetStator.RectPressColor = Color.FromArgb(130, 130, 130);
		btnResetStator.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnResetStator).Size = new Size(131, 31);
		btnResetStator.Style = UIStyle.Black;
		((Control)btnResetStator).TabIndex = 87;
		((Control)btnResetStator).Text = "恢复定子出厂设置";
		((Control)btnResetStator).Click += btnResetStator_Click;
		((Control)btnResetRotor).BackColor = Color.Transparent;
		((Control)btnResetRotor).Cursor = Cursors.Hand;
		btnResetRotor.FillColor = Color.FromArgb(15, 40, 70);
		btnResetRotor.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnResetRotor.FillPressColor = Color.FromArgb(235, 243, 255);
		btnResetRotor.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnResetRotor).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnResetRotor.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnResetRotor.ForePressColor = Color.FromArgb(130, 130, 130);
		btnResetRotor.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnResetRotor).Location = new Point(721, 463);
		((Control)btnResetRotor).MinimumSize = new Size(1, 1);
		((Control)btnResetRotor).Name = "btnResetRotor";
		btnResetRotor.Radius = 25;
		btnResetRotor.RectColor = Color.FromArgb(130, 130, 130);
		btnResetRotor.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnResetRotor.RectPressColor = Color.FromArgb(130, 130, 130);
		btnResetRotor.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnResetRotor).Size = new Size(131, 31);
		btnResetRotor.Style = UIStyle.Black;
		((Control)btnResetRotor).TabIndex = 87;
		((Control)btnResetRotor).Text = "恢复转子出厂设置";
		((Control)btnResetRotor).Click += btnResetRotor_Click;
		cbxType.DataSource = null;
		cbxType.FillColor = Color.White;
		((Control)cbxType).Font = new Font("微软雅黑", 12f);
		cbxType.Items.AddRange(new object[3] { "Rotor", "Stator", "Vdbox" });
		((Control)cbxType).Location = new Point(29, 394);
		((Control)cbxType).Margin = new Padding(4, 5, 4, 5);
		((Control)cbxType).MinimumSize = new Size(63, 0);
		((Control)cbxType).Name = "cbxType";
		((Control)cbxType).Padding = new Padding(0, 0, 30, 2);
		cbxType.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxType).Size = new Size(84, 29);
		cbxType.Style = UIStyle.Black;
		((Control)cbxType).TabIndex = 86;
		((Control)cbxType).Text = "Rotor";
		cbxType.TextAlignment = (ContentAlignment)16;
		cbxColor.DataSource = null;
		cbxColor.FillColor = Color.White;
		((Control)cbxColor).Font = new Font("微软雅黑", 12f);
		cbxColor.Items.AddRange(new object[3] { "Red", "Green", "Blue" });
		((Control)cbxColor).Location = new Point(721, 706);
		((Control)cbxColor).Margin = new Padding(4, 5, 4, 5);
		((Control)cbxColor).MinimumSize = new Size(63, 0);
		((Control)cbxColor).Name = "cbxColor";
		((Control)cbxColor).Padding = new Padding(0, 0, 30, 2);
		cbxColor.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxColor).Size = new Size(150, 29);
		cbxColor.Style = UIStyle.Black;
		((Control)cbxColor).TabIndex = 85;
		((Control)cbxColor).Text = "Red";
		cbxColor.TextAlignment = (ContentAlignment)16;
		((Control)cbxColor).Visible = false;
		((Control)btnTimerWrite).BackColor = Color.Transparent;
		((Control)btnTimerWrite).Cursor = Cursors.Hand;
		btnTimerWrite.FillColor = Color.FromArgb(15, 40, 70);
		btnTimerWrite.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnTimerWrite.FillPressColor = Color.FromArgb(235, 243, 255);
		btnTimerWrite.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnTimerWrite).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnTimerWrite.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnTimerWrite.ForePressColor = Color.FromArgb(130, 130, 130);
		btnTimerWrite.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTimerWrite).Location = new Point(808, 649);
		((Control)btnTimerWrite).MinimumSize = new Size(1, 1);
		((Control)btnTimerWrite).Name = "btnTimerWrite";
		btnTimerWrite.Radius = 25;
		btnTimerWrite.RectColor = Color.FromArgb(130, 130, 130);
		btnTimerWrite.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnTimerWrite.RectPressColor = Color.FromArgb(130, 130, 130);
		btnTimerWrite.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTimerWrite).Size = new Size(89, 35);
		btnTimerWrite.Style = UIStyle.Black;
		((Control)btnTimerWrite).TabIndex = 84;
		((Control)btnTimerWrite).Text = "定时写入";
		((Control)btnTimerWrite).Visible = false;
		((Control)btnTimerWrite).Click += btnTimerWrite_Click;
		((Control)uiLabel1).BackColor = Color.Transparent;
		((Control)uiLabel1).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel1).ForeColor = Color.Silver;
		((Control)uiLabel1).Location = new Point(118, 397);
		((Control)uiLabel1).Name = "uiLabel1";
		((Control)uiLabel1).Size = new Size(39, 23);
		uiLabel1.Style = UIStyle.Black;
		((Control)uiLabel1).TabIndex = 83;
		((Control)uiLabel1).Text = "ID";
		((Label)uiLabel1).TextAlign = (ContentAlignment)16;
		((Control)txtID).Cursor = Cursors.IBeam;
		txtID.FillColor = Color.White;
		((Control)txtID).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtID).Location = new Point(159, 391);
		((Control)txtID).Margin = new Padding(4, 5, 4, 5);
		txtID.Maximum = 2147483647.0;
		txtID.Minimum = -2147483648.0;
		((Control)txtID).MinimumSize = new Size(1, 1);
		((Control)txtID).Name = "txtID";
		txtID.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtID).Size = new Size(77, 34);
		txtID.Style = UIStyle.Black;
		((Control)txtID).TabIndex = 82;
		((Control)txtID).Text = "0x02";
		txtID.TextAlignment = (ContentAlignment)16;
		((Control)uiButton1).BackColor = Color.Transparent;
		((Control)uiButton1).Cursor = Cursors.Hand;
		uiButton1.FillColor = Color.FromArgb(15, 40, 70);
		uiButton1.FillHoverColor = Color.FromArgb(216, 233, 255);
		uiButton1.FillPressColor = Color.FromArgb(235, 243, 255);
		uiButton1.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)uiButton1).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		uiButton1.ForeHoverColor = Color.FromArgb(130, 130, 130);
		uiButton1.ForePressColor = Color.FromArgb(130, 130, 130);
		uiButton1.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton1).Location = new Point(712, 658);
		((Control)uiButton1).MinimumSize = new Size(1, 1);
		((Control)uiButton1).Name = "uiButton1";
		uiButton1.Radius = 25;
		uiButton1.RectColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectHoverColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectPressColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton1).Size = new Size(81, 31);
		uiButton1.Style = UIStyle.Black;
		((Control)uiButton1).TabIndex = 81;
		((Control)uiButton1).Text = "Test";
		((Control)uiButton1).Visible = false;
		((Control)uiButton1).Click += uiButton1_Click;
		((Control)btnTestSpeed).BackColor = Color.Transparent;
		((Control)btnTestSpeed).Cursor = Cursors.Hand;
		btnTestSpeed.FillColor = Color.FromArgb(15, 40, 70);
		btnTestSpeed.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnTestSpeed.FillPressColor = Color.FromArgb(235, 243, 255);
		btnTestSpeed.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnTestSpeed).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnTestSpeed.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnTestSpeed.ForePressColor = Color.FromArgb(130, 130, 130);
		btnTestSpeed.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTestSpeed).Location = new Point(712, 647);
		((Control)btnTestSpeed).MinimumSize = new Size(1, 1);
		((Control)btnTestSpeed).Name = "btnTestSpeed";
		btnTestSpeed.Radius = 25;
		btnTestSpeed.RectColor = Color.FromArgb(130, 130, 130);
		btnTestSpeed.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnTestSpeed.RectPressColor = Color.FromArgb(130, 130, 130);
		btnTestSpeed.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTestSpeed).Size = new Size(81, 31);
		btnTestSpeed.Style = UIStyle.Black;
		((Control)btnTestSpeed).TabIndex = 81;
		((Control)btnTestSpeed).Text = "测转速";
		((Control)btnTestSpeed).Visible = false;
		((Control)btnTestSpeed).Click += btnTestSpeed_Click_1;
		((Control)btnClear).BackColor = Color.Transparent;
		((Control)btnClear).Cursor = Cursors.Hand;
		btnClear.FillColor = Color.FromArgb(15, 40, 70);
		btnClear.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnClear.FillPressColor = Color.FromArgb(235, 243, 255);
		btnClear.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnClear).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnClear.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnClear.ForePressColor = Color.FromArgb(130, 130, 130);
		btnClear.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnClear).Location = new Point(847, 394);
		((Control)btnClear).Margin = new Padding(2);
		((Control)btnClear).MinimumSize = new Size(1, 1);
		((Control)btnClear).Name = "btnClear";
		btnClear.Radius = 25;
		btnClear.RectColor = Color.FromArgb(130, 130, 130);
		btnClear.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnClear.RectPressColor = Color.FromArgb(130, 130, 130);
		btnClear.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnClear).Size = new Size(74, 29);
		btnClear.Style = UIStyle.Black;
		((Control)btnClear).TabIndex = 80;
		((Control)btnClear).Text = "清除";
		((Control)btnClear).Click += btnClear_Click;
		((Control)btnWriteData).BackColor = Color.Transparent;
		((Control)btnWriteData).Cursor = Cursors.Hand;
		btnWriteData.FillColor = Color.FromArgb(15, 40, 70);
		btnWriteData.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnWriteData.FillPressColor = Color.FromArgb(235, 243, 255);
		btnWriteData.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnWriteData).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnWriteData.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnWriteData.ForePressColor = Color.FromArgb(130, 130, 130);
		btnWriteData.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWriteData).Location = new Point(707, 397);
		((Control)btnWriteData).Margin = new Padding(2);
		((Control)btnWriteData).MinimumSize = new Size(1, 1);
		((Control)btnWriteData).Name = "btnWriteData";
		btnWriteData.Radius = 26;
		btnWriteData.RectColor = Color.FromArgb(130, 130, 130);
		btnWriteData.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnWriteData.RectPressColor = Color.FromArgb(130, 130, 130);
		btnWriteData.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWriteData).Size = new Size(57, 23);
		btnWriteData.Style = UIStyle.Black;
		((Control)btnWriteData).TabIndex = 79;
		((Control)btnWriteData).Text = "写入";
		((Control)btnWriteData).Click += btnWriteData_Click;
		((ScrollableControl)txtLog).AutoScroll = true;
		((Control)txtLog).Cursor = Cursors.IBeam;
		txtLog.FillColor = Color.White;
		((Control)txtLog).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtLog).Location = new Point(24, 443);
		((Control)txtLog).Margin = new Padding(4, 5, 4, 5);
		txtLog.Maximum = 2147483647.0;
		txtLog.Minimum = -2147483648.0;
		((Control)txtLog).MinimumSize = new Size(1, 1);
		txtLog.Multiline = true;
		((Control)txtLog).Name = "txtLog";
		txtLog.ReadOnly = true;
		txtLog.RectColor = Color.FromArgb(130, 130, 130);
		txtLog.ShowScrollBar = true;
		((Control)txtLog).Size = new Size(676, 278);
		txtLog.Style = UIStyle.Black;
		((Control)txtLog).TabIndex = 26;
		txtLog.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel7).BackColor = Color.Transparent;
		((Control)uiLabel7).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel7).ForeColor = Color.Silver;
		((Control)uiLabel7).Location = new Point(241, 395);
		((Control)uiLabel7).Name = "uiLabel7";
		((Control)uiLabel7).Size = new Size(87, 29);
		uiLabel7.Style = UIStyle.Black;
		((Control)uiLabel7).TabIndex = 25;
		((Control)uiLabel7).Text = "文件路径:";
		((Label)uiLabel7).TextAlign = (ContentAlignment)16;
		((Control)btnView).BackColor = Color.Transparent;
		((Control)btnView).Cursor = Cursors.Hand;
		btnView.FillColor = Color.FromArgb(15, 40, 70);
		btnView.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnView.FillPressColor = Color.FromArgb(235, 243, 255);
		btnView.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnView).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnView.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnView.ForePressColor = Color.FromArgb(130, 130, 130);
		btnView.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnView).Location = new Point(616, 396);
		((Control)btnView).Margin = new Padding(2);
		((Control)btnView).MinimumSize = new Size(1, 1);
		((Control)btnView).Name = "btnView";
		btnView.Radius = 25;
		btnView.RectColor = Color.FromArgb(130, 130, 130);
		btnView.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnView.RectPressColor = Color.FromArgb(130, 130, 130);
		btnView.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnView).Size = new Size(74, 29);
		btnView.Style = UIStyle.Black;
		((Control)btnView).TabIndex = 24;
		((Control)btnView).Text = "浏览";
		((Control)btnView).Click += btnView_Click;
		((Control)txtPath).BackColor = Color.Transparent;
		((Control)txtPath).Cursor = Cursors.IBeam;
		txtPath.FillColor = Color.White;
		((Control)txtPath).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtPath).Location = new Point(330, 394);
		((Control)txtPath).Margin = new Padding(4, 5, 4, 5);
		txtPath.Maximum = 2147483647.0;
		txtPath.Minimum = -2147483648.0;
		((Control)txtPath).MinimumSize = new Size(1, 1);
		((Control)txtPath).Name = "txtPath";
		txtPath.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtPath).Size = new Size(277, 34);
		txtPath.Style = UIStyle.Black;
		((Control)txtPath).TabIndex = 23;
		txtPath.TextAlignment = (ContentAlignment)16;
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnJiaB);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnJiaG);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnJiaR);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnJianB);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnJianG);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnJianR);
		((Control)uiGroupBox1).Controls.Add((Control)(object)hsb_B);
		((Control)uiGroupBox1).Controls.Add((Control)(object)hsb_G);
		((Control)uiGroupBox1).Controls.Add((Control)(object)hsb_R);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtB3);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtG3);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtR3);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel21);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel17);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel13);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtB2);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtG2);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtR2);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel20);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel16);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel6);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtB1);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtG1);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtR1);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel19);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel15);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel5);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtB0);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel18);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtG0);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel14);
		((Control)uiGroupBox1).Controls.Add((Control)(object)txtR0);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiLabel4);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnReadAll);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnWriteAll);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnRead_B);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnWrite_B);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnRead_G);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnRead_R);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnWrite_G);
		((Control)uiGroupBox1).Controls.Add((Control)(object)btnWrite_R);
		((Control)uiGroupBox1).Controls.Add((Control)(object)lbl_B);
		((Control)uiGroupBox1).Controls.Add((Control)(object)lbl_G);
		((Control)uiGroupBox1).Controls.Add((Control)(object)lbl_R);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiButton16);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiButton17);
		((Control)uiGroupBox1).Controls.Add((Control)(object)uiButton4);
		uiGroupBox1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiGroupBox1).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiGroupBox1).ForeColor = Color.Silver;
		((Control)uiGroupBox1).Location = new Point(17, 22);
		((Control)uiGroupBox1).Margin = new Padding(4, 5, 4, 5);
		((Control)uiGroupBox1).MinimumSize = new Size(1, 1);
		((Control)uiGroupBox1).Name = "uiGroupBox1";
		((Control)uiGroupBox1).Padding = new Padding(0, 32, 0, 0);
		uiGroupBox1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiGroupBox1).Size = new Size(944, 357);
		uiGroupBox1.Style = UIStyle.Black;
		((Control)uiGroupBox1).TabIndex = 2;
		((Control)uiGroupBox1).Text = "电流增益";
		uiGroupBox1.TextAlignment = (ContentAlignment)32;
		((Control)btnJiaB).BackColor = Color.Transparent;
		((Control)btnJiaB).Cursor = Cursors.Hand;
		btnJiaB.FillColor = Color.FromArgb(15, 40, 70);
		btnJiaB.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJiaB.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJiaB.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJiaB).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJiaB.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaB.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJiaB.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaB).Location = new Point(445, 118);
		((Control)btnJiaB).Margin = new Padding(2);
		((Control)btnJiaB).MinimumSize = new Size(1, 1);
		((Control)btnJiaB).Name = "btnJiaB";
		btnJiaB.Radius = 26;
		btnJiaB.RectColor = Color.FromArgb(130, 130, 130);
		btnJiaB.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaB.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJiaB.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaB).Size = new Size(29, 23);
		btnJiaB.Style = UIStyle.Black;
		((Control)btnJiaB).TabIndex = 83;
		((Control)btnJiaB).Text = "+";
		((Control)btnJiaB).Click += btnJiaB_Click;
		((Control)btnJiaG).BackColor = Color.Transparent;
		((Control)btnJiaG).Cursor = Cursors.Hand;
		btnJiaG.FillColor = Color.FromArgb(15, 40, 70);
		btnJiaG.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJiaG.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJiaG.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJiaG).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJiaG.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaG.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJiaG.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaG).Location = new Point(445, 75);
		((Control)btnJiaG).Margin = new Padding(2);
		((Control)btnJiaG).MinimumSize = new Size(1, 1);
		((Control)btnJiaG).Name = "btnJiaG";
		btnJiaG.Radius = 26;
		btnJiaG.RectColor = Color.FromArgb(130, 130, 130);
		btnJiaG.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaG.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJiaG.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaG).Size = new Size(29, 23);
		btnJiaG.Style = UIStyle.Black;
		((Control)btnJiaG).TabIndex = 83;
		((Control)btnJiaG).Text = "+";
		((Control)btnJiaG).Click += btnJiaG_Click;
		((Control)btnJiaR).BackColor = Color.Transparent;
		((Control)btnJiaR).Cursor = Cursors.Hand;
		btnJiaR.FillColor = Color.FromArgb(15, 40, 70);
		btnJiaR.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJiaR.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJiaR.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJiaR).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJiaR.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaR.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJiaR.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaR).Location = new Point(445, 37);
		((Control)btnJiaR).Margin = new Padding(2);
		((Control)btnJiaR).MinimumSize = new Size(1, 1);
		((Control)btnJiaR).Name = "btnJiaR";
		btnJiaR.Radius = 26;
		btnJiaR.RectColor = Color.FromArgb(130, 130, 130);
		btnJiaR.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaR.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJiaR.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaR).Size = new Size(29, 23);
		btnJiaR.Style = UIStyle.Black;
		((Control)btnJiaR).TabIndex = 83;
		((Control)btnJiaR).Text = "+";
		((Control)btnJiaR).Click += btnJiaR_Click;
		((Control)btnJianB).BackColor = Color.Transparent;
		((Control)btnJianB).Cursor = Cursors.Hand;
		btnJianB.FillColor = Color.FromArgb(15, 40, 70);
		btnJianB.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJianB.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJianB.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJianB).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJianB.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJianB.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJianB.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJianB).Location = new Point(84, 118);
		((Control)btnJianB).Margin = new Padding(2);
		((Control)btnJianB).MinimumSize = new Size(1, 1);
		((Control)btnJianB).Name = "btnJianB";
		btnJianB.Radius = 26;
		btnJianB.RectColor = Color.FromArgb(130, 130, 130);
		btnJianB.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJianB.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJianB.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJianB).Size = new Size(29, 23);
		btnJianB.Style = UIStyle.Black;
		((Control)btnJianB).TabIndex = 82;
		((Control)btnJianB).Text = "-";
		((Control)btnJianB).Click += btnJianB_Click;
		((Control)btnJianG).BackColor = Color.Transparent;
		((Control)btnJianG).Cursor = Cursors.Hand;
		btnJianG.FillColor = Color.FromArgb(15, 40, 70);
		btnJianG.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJianG.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJianG.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJianG).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJianG.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJianG.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJianG.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJianG).Location = new Point(83, 75);
		((Control)btnJianG).Margin = new Padding(2);
		((Control)btnJianG).MinimumSize = new Size(1, 1);
		((Control)btnJianG).Name = "btnJianG";
		btnJianG.Radius = 26;
		btnJianG.RectColor = Color.FromArgb(130, 130, 130);
		btnJianG.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJianG.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJianG.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJianG).Size = new Size(29, 23);
		btnJianG.Style = UIStyle.Black;
		((Control)btnJianG).TabIndex = 82;
		((Control)btnJianG).Text = "-";
		((Control)btnJianG).Click += btnJianG_Click;
		((Control)btnJianR).BackColor = Color.Transparent;
		((Control)btnJianR).Cursor = Cursors.Hand;
		btnJianR.FillColor = Color.FromArgb(15, 40, 70);
		btnJianR.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJianR.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJianR.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJianR).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJianR.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJianR.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJianR.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJianR).Location = new Point(83, 37);
		((Control)btnJianR).Margin = new Padding(2);
		((Control)btnJianR).MinimumSize = new Size(1, 1);
		((Control)btnJianR).Name = "btnJianR";
		btnJianR.Radius = 26;
		btnJianR.RectColor = Color.FromArgb(130, 130, 130);
		btnJianR.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJianR.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJianR.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJianR).Size = new Size(29, 23);
		btnJianR.Style = UIStyle.Black;
		((Control)btnJianR).TabIndex = 82;
		((Control)btnJianR).Text = "-";
		((Control)btnJianR).Click += btnJianR_Click;
		hsb_B.DisableColor = Color.Silver;
		hsb_B.FillColor = Color.FromArgb(24, 24, 24);
		((Control)hsb_B).Font = new Font("微软雅黑", 12f);
		((Control)hsb_B).Location = new Point(118, 118);
		hsb_B.Maximum = 255;
		((Control)hsb_B).MinimumSize = new Size(1, 1);
		((Control)hsb_B).Name = "hsb_B";
		((Control)hsb_B).Size = new Size(319, 25);
		hsb_B.Style = UIStyle.Black;
		((Control)hsb_B).TabIndex = 81;
		((Control)hsb_B).Text = "uiTrackBar6";
		hsb_G.DisableColor = Color.Silver;
		hsb_G.FillColor = Color.FromArgb(24, 24, 24);
		((Control)hsb_G).Font = new Font("微软雅黑", 12f);
		((Control)hsb_G).Location = new Point(118, 75);
		hsb_G.Maximum = 255;
		((Control)hsb_G).MinimumSize = new Size(1, 1);
		((Control)hsb_G).Name = "hsb_G";
		((Control)hsb_G).Size = new Size(319, 25);
		hsb_G.Style = UIStyle.Black;
		((Control)hsb_G).TabIndex = 81;
		((Control)hsb_G).Text = "uiTrackBar6";
		hsb_R.DisableColor = Color.Silver;
		hsb_R.FillColor = Color.FromArgb(24, 24, 24);
		((Control)hsb_R).Font = new Font("微软雅黑", 12f);
		((Control)hsb_R).Location = new Point(118, 35);
		hsb_R.Maximum = 255;
		((Control)hsb_R).MinimumSize = new Size(1, 1);
		((Control)hsb_R).Name = "hsb_R";
		((Control)hsb_R).Size = new Size(319, 25);
		hsb_R.Style = UIStyle.Black;
		((Control)hsb_R).TabIndex = 81;
		((Control)hsb_R).Text = "uiTrackBar6";
		((Control)txtB3).Cursor = Cursors.IBeam;
		txtB3.FillColor = Color.White;
		((Control)txtB3).Font = new Font("微软雅黑", 12f);
		((Control)txtB3).Location = new Point(661, 256);
		((Control)txtB3).Margin = new Padding(4, 5, 4, 5);
		txtB3.Maximum = 2147483647.0;
		txtB3.Minimum = -2147483648.0;
		((Control)txtB3).MinimumSize = new Size(1, 1);
		((Control)txtB3).Name = "txtB3";
		txtB3.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtB3).Size = new Size(76, 34);
		txtB3.Style = UIStyle.Black;
		((Control)txtB3).TabIndex = 80;
		txtB3.TextAlignment = (ContentAlignment)16;
		((Control)txtG3).Cursor = Cursors.IBeam;
		txtG3.FillColor = Color.White;
		((Control)txtG3).Font = new Font("微软雅黑", 12f);
		((Control)txtG3).Location = new Point(661, 217);
		((Control)txtG3).Margin = new Padding(4, 5, 4, 5);
		txtG3.Maximum = 2147483647.0;
		txtG3.Minimum = -2147483648.0;
		((Control)txtG3).MinimumSize = new Size(1, 1);
		((Control)txtG3).Name = "txtG3";
		txtG3.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtG3).Size = new Size(76, 34);
		txtG3.Style = UIStyle.Black;
		((Control)txtG3).TabIndex = 80;
		txtG3.TextAlignment = (ContentAlignment)16;
		((Control)txtR3).Cursor = Cursors.IBeam;
		txtR3.FillColor = Color.White;
		((Control)txtR3).Font = new Font("微软雅黑", 12f);
		((Control)txtR3).Location = new Point(661, 178);
		((Control)txtR3).Margin = new Padding(4, 5, 4, 5);
		txtR3.Maximum = 2147483647.0;
		txtR3.Minimum = -2147483648.0;
		((Control)txtR3).MinimumSize = new Size(1, 1);
		((Control)txtR3).Name = "txtR3";
		txtR3.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtR3).Size = new Size(76, 34);
		txtR3.Style = UIStyle.Black;
		((Control)txtR3).TabIndex = 80;
		txtR3.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel21).BackColor = Color.Transparent;
		((Control)uiLabel21).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel21).ForeColor = Color.Silver;
		((Control)uiLabel21).Location = new Point(550, 256);
		((Control)uiLabel21).Name = "uiLabel21";
		((Control)uiLabel21).Size = new Size(90, 34);
		uiLabel21.Style = UIStyle.Black;
		((Control)uiLabel21).TabIndex = 79;
		((Control)uiLabel21).Text = "B_reg_3";
		((Label)uiLabel21).TextAlign = (ContentAlignment)16;
		((Control)uiLabel17).BackColor = Color.Transparent;
		((Control)uiLabel17).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel17).ForeColor = Color.Silver;
		((Control)uiLabel17).Location = new Point(550, 217);
		((Control)uiLabel17).Name = "uiLabel17";
		((Control)uiLabel17).Size = new Size(90, 34);
		uiLabel17.Style = UIStyle.Black;
		((Control)uiLabel17).TabIndex = 79;
		((Control)uiLabel17).Text = "G_reg_3";
		((Label)uiLabel17).TextAlign = (ContentAlignment)16;
		((Control)uiLabel13).BackColor = Color.Transparent;
		((Control)uiLabel13).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel13).ForeColor = Color.Silver;
		((Control)uiLabel13).Location = new Point(550, 178);
		((Control)uiLabel13).Name = "uiLabel13";
		((Control)uiLabel13).Size = new Size(90, 39);
		uiLabel13.Style = UIStyle.Black;
		((Control)uiLabel13).TabIndex = 79;
		((Control)uiLabel13).Text = "R_reg_3";
		((Label)uiLabel13).TextAlign = (ContentAlignment)16;
		((Control)txtB2).Cursor = Cursors.IBeam;
		txtB2.FillColor = Color.White;
		((Control)txtB2).Font = new Font("微软雅黑", 12f);
		((Control)txtB2).Location = new Point(464, 256);
		((Control)txtB2).Margin = new Padding(4, 5, 4, 5);
		txtB2.Maximum = 2147483647.0;
		txtB2.Minimum = -2147483648.0;
		((Control)txtB2).MinimumSize = new Size(1, 1);
		((Control)txtB2).Name = "txtB2";
		txtB2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtB2).Size = new Size(76, 34);
		txtB2.Style = UIStyle.Black;
		((Control)txtB2).TabIndex = 80;
		txtB2.TextAlignment = (ContentAlignment)16;
		((Control)txtG2).Cursor = Cursors.IBeam;
		txtG2.FillColor = Color.White;
		((Control)txtG2).Font = new Font("微软雅黑", 12f);
		((Control)txtG2).Location = new Point(464, 217);
		((Control)txtG2).Margin = new Padding(4, 5, 4, 5);
		txtG2.Maximum = 2147483647.0;
		txtG2.Minimum = -2147483648.0;
		((Control)txtG2).MinimumSize = new Size(1, 1);
		((Control)txtG2).Name = "txtG2";
		txtG2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtG2).Size = new Size(76, 34);
		txtG2.Style = UIStyle.Black;
		((Control)txtG2).TabIndex = 80;
		txtG2.TextAlignment = (ContentAlignment)16;
		((Control)txtR2).Cursor = Cursors.IBeam;
		txtR2.FillColor = Color.White;
		((Control)txtR2).Font = new Font("微软雅黑", 12f);
		((Control)txtR2).Location = new Point(464, 178);
		((Control)txtR2).Margin = new Padding(4, 5, 4, 5);
		txtR2.Maximum = 2147483647.0;
		txtR2.Minimum = -2147483648.0;
		((Control)txtR2).MinimumSize = new Size(1, 1);
		((Control)txtR2).Name = "txtR2";
		txtR2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtR2).Size = new Size(76, 34);
		txtR2.Style = UIStyle.Black;
		((Control)txtR2).TabIndex = 80;
		txtR2.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel20).BackColor = Color.Transparent;
		((Control)uiLabel20).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel20).ForeColor = Color.Silver;
		((Control)uiLabel20).Location = new Point(369, 256);
		((Control)uiLabel20).Name = "uiLabel20";
		((Control)uiLabel20).Size = new Size(88, 34);
		uiLabel20.Style = UIStyle.Black;
		((Control)uiLabel20).TabIndex = 79;
		((Control)uiLabel20).Text = "B_reg_2";
		((Label)uiLabel20).TextAlign = (ContentAlignment)16;
		((Control)uiLabel16).BackColor = Color.Transparent;
		((Control)uiLabel16).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel16).ForeColor = Color.Silver;
		((Control)uiLabel16).Location = new Point(365, 217);
		((Control)uiLabel16).Name = "uiLabel16";
		((Control)uiLabel16).Size = new Size(92, 34);
		uiLabel16.Style = UIStyle.Black;
		((Control)uiLabel16).TabIndex = 79;
		((Control)uiLabel16).Text = "G_reg_2";
		((Label)uiLabel16).TextAlign = (ContentAlignment)16;
		((Control)uiLabel6).BackColor = Color.Transparent;
		((Control)uiLabel6).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel6).ForeColor = Color.Silver;
		((Control)uiLabel6).Location = new Point(369, 178);
		((Control)uiLabel6).Name = "uiLabel6";
		((Control)uiLabel6).Size = new Size(88, 34);
		uiLabel6.Style = UIStyle.Black;
		((Control)uiLabel6).TabIndex = 79;
		((Control)uiLabel6).Text = "R_reg_2";
		((Label)uiLabel6).TextAlign = (ContentAlignment)16;
		((Control)txtB1).Cursor = Cursors.IBeam;
		txtB1.FillColor = Color.White;
		((Control)txtB1).Font = new Font("微软雅黑", 12f);
		((Control)txtB1).Location = new Point(286, 256);
		((Control)txtB1).Margin = new Padding(4, 5, 4, 5);
		txtB1.Maximum = 2147483647.0;
		txtB1.Minimum = -2147483648.0;
		((Control)txtB1).MinimumSize = new Size(1, 1);
		((Control)txtB1).Name = "txtB1";
		txtB1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtB1).Size = new Size(76, 34);
		txtB1.Style = UIStyle.Black;
		((Control)txtB1).TabIndex = 80;
		txtB1.TextAlignment = (ContentAlignment)16;
		((Control)txtG1).Cursor = Cursors.IBeam;
		txtG1.FillColor = Color.White;
		((Control)txtG1).Font = new Font("微软雅黑", 12f);
		((Control)txtG1).Location = new Point(286, 217);
		((Control)txtG1).Margin = new Padding(4, 5, 4, 5);
		txtG1.Maximum = 2147483647.0;
		txtG1.Minimum = -2147483648.0;
		((Control)txtG1).MinimumSize = new Size(1, 1);
		((Control)txtG1).Name = "txtG1";
		txtG1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtG1).Size = new Size(76, 34);
		txtG1.Style = UIStyle.Black;
		((Control)txtG1).TabIndex = 80;
		txtG1.TextAlignment = (ContentAlignment)16;
		((Control)txtR1).Cursor = Cursors.IBeam;
		txtR1.FillColor = Color.White;
		((Control)txtR1).Font = new Font("微软雅黑", 12f);
		((Control)txtR1).Location = new Point(286, 178);
		((Control)txtR1).Margin = new Padding(4, 5, 4, 5);
		txtR1.Maximum = 2147483647.0;
		txtR1.Minimum = -2147483648.0;
		((Control)txtR1).MinimumSize = new Size(1, 1);
		((Control)txtR1).Name = "txtR1";
		txtR1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtR1).Size = new Size(76, 34);
		txtR1.Style = UIStyle.Black;
		((Control)txtR1).TabIndex = 80;
		txtR1.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel19).BackColor = Color.Transparent;
		((Control)uiLabel19).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel19).ForeColor = Color.Silver;
		((Control)uiLabel19).Location = new Point(192, 256);
		((Control)uiLabel19).Name = "uiLabel19";
		((Control)uiLabel19).Size = new Size(87, 34);
		uiLabel19.Style = UIStyle.Black;
		((Control)uiLabel19).TabIndex = 79;
		((Control)uiLabel19).Text = "B_reg_1";
		((Label)uiLabel19).TextAlign = (ContentAlignment)16;
		((Control)uiLabel15).BackColor = Color.Transparent;
		((Control)uiLabel15).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel15).ForeColor = Color.Silver;
		((Control)uiLabel15).Location = new Point(191, 217);
		((Control)uiLabel15).Name = "uiLabel15";
		((Control)uiLabel15).Size = new Size(96, 34);
		uiLabel15.Style = UIStyle.Black;
		((Control)uiLabel15).TabIndex = 79;
		((Control)uiLabel15).Text = "G_reg_1";
		((Label)uiLabel15).TextAlign = (ContentAlignment)16;
		((Control)uiLabel5).BackColor = Color.Transparent;
		((Control)uiLabel5).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel5).ForeColor = Color.Silver;
		((Control)uiLabel5).Location = new Point(192, 178);
		((Control)uiLabel5).Name = "uiLabel5";
		((Control)uiLabel5).Size = new Size(87, 34);
		uiLabel5.Style = UIStyle.Black;
		((Control)uiLabel5).TabIndex = 79;
		((Control)uiLabel5).Text = "R_reg_1";
		((Label)uiLabel5).TextAlign = (ContentAlignment)16;
		((Control)txtB0).Cursor = Cursors.IBeam;
		txtB0.FillColor = Color.White;
		((Control)txtB0).Font = new Font("微软雅黑", 12f);
		((Control)txtB0).Location = new Point(109, 256);
		((Control)txtB0).Margin = new Padding(4, 5, 4, 5);
		txtB0.Maximum = 2147483647.0;
		txtB0.Minimum = -2147483648.0;
		((Control)txtB0).MinimumSize = new Size(1, 1);
		((Control)txtB0).Name = "txtB0";
		txtB0.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtB0).Size = new Size(76, 34);
		txtB0.Style = UIStyle.Black;
		((Control)txtB0).TabIndex = 80;
		txtB0.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel18).BackColor = Color.Transparent;
		((Control)uiLabel18).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel18).ForeColor = Color.Silver;
		((Control)uiLabel18).Location = new Point(7, 256);
		((Control)uiLabel18).Name = "uiLabel18";
		((Control)uiLabel18).Size = new Size(93, 34);
		uiLabel18.Style = UIStyle.Black;
		((Control)uiLabel18).TabIndex = 79;
		((Control)uiLabel18).Text = "B_reg_0";
		((Label)uiLabel18).TextAlign = (ContentAlignment)16;
		((Control)txtG0).Cursor = Cursors.IBeam;
		txtG0.FillColor = Color.White;
		((Control)txtG0).Font = new Font("微软雅黑", 12f);
		((Control)txtG0).Location = new Point(109, 217);
		((Control)txtG0).Margin = new Padding(4, 5, 4, 5);
		txtG0.Maximum = 2147483647.0;
		txtG0.Minimum = -2147483648.0;
		((Control)txtG0).MinimumSize = new Size(1, 1);
		((Control)txtG0).Name = "txtG0";
		txtG0.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtG0).Size = new Size(76, 34);
		txtG0.Style = UIStyle.Black;
		((Control)txtG0).TabIndex = 80;
		txtG0.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel14).BackColor = Color.Transparent;
		((Control)uiLabel14).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel14).ForeColor = Color.Silver;
		((Control)uiLabel14).Location = new Point(7, 217);
		((Control)uiLabel14).Name = "uiLabel14";
		((Control)uiLabel14).Size = new Size(98, 34);
		uiLabel14.Style = UIStyle.Black;
		((Control)uiLabel14).TabIndex = 79;
		((Control)uiLabel14).Text = "G_reg_0";
		((Label)uiLabel14).TextAlign = (ContentAlignment)16;
		((Control)txtR0).Cursor = Cursors.IBeam;
		txtR0.FillColor = Color.White;
		((Control)txtR0).Font = new Font("微软雅黑", 12f);
		((Control)txtR0).Location = new Point(109, 178);
		((Control)txtR0).Margin = new Padding(4, 5, 4, 5);
		txtR0.Maximum = 2147483647.0;
		txtR0.Minimum = -2147483648.0;
		((Control)txtR0).MinimumSize = new Size(1, 1);
		((Control)txtR0).Name = "txtR0";
		txtR0.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtR0).Size = new Size(76, 34);
		txtR0.Style = UIStyle.Black;
		((Control)txtR0).TabIndex = 80;
		txtR0.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel4).BackColor = Color.Transparent;
		((Control)uiLabel4).Font = new Font("微软雅黑", 12f);
		((Control)uiLabel4).ForeColor = Color.Silver;
		((Control)uiLabel4).Location = new Point(7, 178);
		((Control)uiLabel4).Name = "uiLabel4";
		((Control)uiLabel4).Size = new Size(98, 34);
		uiLabel4.Style = UIStyle.Black;
		((Control)uiLabel4).TabIndex = 79;
		((Control)uiLabel4).Text = "R_reg_0";
		((Label)uiLabel4).TextAlign = (ContentAlignment)16;
		((Control)btnReadAll).BackColor = Color.Transparent;
		((Control)btnReadAll).Cursor = Cursors.Hand;
		btnReadAll.FillColor = Color.FromArgb(15, 40, 70);
		btnReadAll.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnReadAll.FillPressColor = Color.FromArgb(235, 243, 255);
		btnReadAll.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnReadAll).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnReadAll.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnReadAll.ForePressColor = Color.FromArgb(130, 130, 130);
		btnReadAll.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReadAll).Location = new Point(645, 312);
		((Control)btnReadAll).Margin = new Padding(2);
		((Control)btnReadAll).MinimumSize = new Size(1, 1);
		((Control)btnReadAll).Name = "btnReadAll";
		btnReadAll.Radius = 26;
		btnReadAll.RectColor = Color.FromArgb(130, 130, 130);
		btnReadAll.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnReadAll.RectPressColor = Color.FromArgb(130, 130, 130);
		btnReadAll.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReadAll).Size = new Size(57, 23);
		btnReadAll.Style = UIStyle.Black;
		((Control)btnReadAll).TabIndex = 78;
		((Control)btnReadAll).Text = "回读";
		((Control)btnWriteAll).BackColor = Color.Transparent;
		((Control)btnWriteAll).Cursor = Cursors.Hand;
		btnWriteAll.FillColor = Color.FromArgb(15, 40, 70);
		btnWriteAll.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnWriteAll.FillPressColor = Color.FromArgb(235, 243, 255);
		btnWriteAll.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnWriteAll).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnWriteAll.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnWriteAll.ForePressColor = Color.FromArgb(130, 130, 130);
		btnWriteAll.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWriteAll).Location = new Point(571, 312);
		((Control)btnWriteAll).Margin = new Padding(2);
		((Control)btnWriteAll).MinimumSize = new Size(1, 1);
		((Control)btnWriteAll).Name = "btnWriteAll";
		btnWriteAll.Radius = 26;
		btnWriteAll.RectColor = Color.FromArgb(130, 130, 130);
		btnWriteAll.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnWriteAll.RectPressColor = Color.FromArgb(130, 130, 130);
		btnWriteAll.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWriteAll).Size = new Size(57, 23);
		btnWriteAll.Style = UIStyle.Black;
		((Control)btnWriteAll).TabIndex = 78;
		((Control)btnWriteAll).Text = "写入";
		((Control)btnRead_B).BackColor = Color.Transparent;
		((Control)btnRead_B).Cursor = Cursors.Hand;
		btnRead_B.FillColor = Color.FromArgb(15, 40, 70);
		btnRead_B.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnRead_B.FillPressColor = Color.FromArgb(235, 243, 255);
		btnRead_B.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnRead_B).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnRead_B.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnRead_B.ForePressColor = Color.FromArgb(130, 130, 130);
		btnRead_B.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRead_B).Location = new Point(562, 120);
		((Control)btnRead_B).Margin = new Padding(2);
		((Control)btnRead_B).MinimumSize = new Size(1, 1);
		((Control)btnRead_B).Name = "btnRead_B";
		btnRead_B.Radius = 26;
		btnRead_B.RectColor = Color.FromArgb(130, 130, 130);
		btnRead_B.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnRead_B.RectPressColor = Color.FromArgb(130, 130, 130);
		btnRead_B.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRead_B).Size = new Size(57, 23);
		btnRead_B.Style = UIStyle.Black;
		((Control)btnRead_B).TabIndex = 78;
		((Control)btnRead_B).Text = "回读";
		((Control)btnWrite_B).BackColor = Color.Black;
		((Control)btnWrite_B).Cursor = Cursors.Hand;
		btnWrite_B.FillColor = Color.FromArgb(15, 40, 70);
		btnWrite_B.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnWrite_B.FillPressColor = Color.FromArgb(235, 243, 255);
		btnWrite_B.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnWrite_B).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnWrite_B.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnWrite_B.ForePressColor = Color.FromArgb(130, 130, 130);
		btnWrite_B.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWrite_B).Location = new Point(635, 120);
		((Control)btnWrite_B).Margin = new Padding(2);
		((Control)btnWrite_B).MinimumSize = new Size(1, 1);
		((Control)btnWrite_B).Name = "btnWrite_B";
		btnWrite_B.Radius = 26;
		btnWrite_B.RectColor = Color.FromArgb(130, 130, 130);
		btnWrite_B.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnWrite_B.RectPressColor = Color.FromArgb(130, 130, 130);
		btnWrite_B.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWrite_B).Size = new Size(57, 23);
		btnWrite_B.Style = UIStyle.Black;
		((Control)btnWrite_B).TabIndex = 78;
		((Control)btnWrite_B).Text = "写入";
		((Control)btnWrite_B).Visible = false;
		((Control)btnRead_G).BackColor = Color.Transparent;
		((Control)btnRead_G).Cursor = Cursors.Hand;
		btnRead_G.FillColor = Color.FromArgb(15, 40, 70);
		btnRead_G.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnRead_G.FillPressColor = Color.FromArgb(235, 243, 255);
		btnRead_G.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnRead_G).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnRead_G.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnRead_G.ForePressColor = Color.FromArgb(130, 130, 130);
		btnRead_G.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRead_G).Location = new Point(562, 75);
		((Control)btnRead_G).Margin = new Padding(2);
		((Control)btnRead_G).MinimumSize = new Size(1, 1);
		((Control)btnRead_G).Name = "btnRead_G";
		btnRead_G.Radius = 26;
		btnRead_G.RectColor = Color.FromArgb(130, 130, 130);
		btnRead_G.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnRead_G.RectPressColor = Color.FromArgb(130, 130, 130);
		btnRead_G.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRead_G).Size = new Size(57, 23);
		btnRead_G.Style = UIStyle.Black;
		((Control)btnRead_G).TabIndex = 78;
		((Control)btnRead_G).Text = "回读";
		((Control)btnRead_R).BackColor = Color.Transparent;
		((Control)btnRead_R).Cursor = Cursors.Hand;
		btnRead_R.FillColor = Color.FromArgb(15, 40, 70);
		btnRead_R.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnRead_R.FillPressColor = Color.FromArgb(235, 243, 255);
		btnRead_R.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnRead_R).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnRead_R.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnRead_R.ForePressColor = Color.FromArgb(130, 130, 130);
		btnRead_R.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRead_R).Location = new Point(562, 35);
		((Control)btnRead_R).Margin = new Padding(2);
		((Control)btnRead_R).MinimumSize = new Size(1, 1);
		((Control)btnRead_R).Name = "btnRead_R";
		btnRead_R.Radius = 26;
		btnRead_R.RectColor = Color.FromArgb(130, 130, 130);
		btnRead_R.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnRead_R.RectPressColor = Color.FromArgb(130, 130, 130);
		btnRead_R.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRead_R).Size = new Size(57, 23);
		btnRead_R.Style = UIStyle.Black;
		((Control)btnRead_R).TabIndex = 78;
		((Control)btnRead_R).Text = "回读";
		((Control)btnWrite_G).BackColor = Color.Black;
		((Control)btnWrite_G).Cursor = Cursors.Hand;
		btnWrite_G.FillColor = Color.FromArgb(15, 40, 70);
		btnWrite_G.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnWrite_G.FillPressColor = Color.FromArgb(235, 243, 255);
		btnWrite_G.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnWrite_G).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnWrite_G.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnWrite_G.ForePressColor = Color.FromArgb(130, 130, 130);
		btnWrite_G.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWrite_G).Location = new Point(635, 78);
		((Control)btnWrite_G).Margin = new Padding(2);
		((Control)btnWrite_G).MinimumSize = new Size(1, 1);
		((Control)btnWrite_G).Name = "btnWrite_G";
		btnWrite_G.Radius = 26;
		btnWrite_G.RectColor = Color.FromArgb(130, 130, 130);
		btnWrite_G.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnWrite_G.RectPressColor = Color.FromArgb(130, 130, 130);
		btnWrite_G.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWrite_G).Size = new Size(57, 23);
		btnWrite_G.Style = UIStyle.Black;
		((Control)btnWrite_G).TabIndex = 78;
		((Control)btnWrite_G).Text = "写入";
		((Control)btnWrite_G).Visible = false;
		((Control)btnWrite_R).BackColor = Color.Black;
		((Control)btnWrite_R).Cursor = Cursors.Hand;
		btnWrite_R.FillColor = Color.FromArgb(15, 40, 70);
		btnWrite_R.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnWrite_R.FillPressColor = Color.FromArgb(235, 243, 255);
		btnWrite_R.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnWrite_R).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnWrite_R.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnWrite_R.ForePressColor = Color.FromArgb(130, 130, 130);
		btnWrite_R.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWrite_R).Location = new Point(635, 34);
		((Control)btnWrite_R).Margin = new Padding(2);
		((Control)btnWrite_R).MinimumSize = new Size(1, 1);
		((Control)btnWrite_R).Name = "btnWrite_R";
		btnWrite_R.Radius = 26;
		btnWrite_R.RectColor = Color.FromArgb(130, 130, 130);
		btnWrite_R.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnWrite_R.RectPressColor = Color.FromArgb(130, 130, 130);
		btnWrite_R.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWrite_R).Size = new Size(57, 23);
		btnWrite_R.Style = UIStyle.Black;
		((Control)btnWrite_R).TabIndex = 78;
		((Control)btnWrite_R).Text = "写入";
		((Control)btnWrite_R).Visible = false;
		((Control)lbl_B).BackColor = Color.Transparent;
		((Control)lbl_B).Font = new Font("微软雅黑", 12f);
		((Control)lbl_B).ForeColor = Color.Silver;
		((Control)lbl_B).Location = new Point(493, 120);
		((Control)lbl_B).Name = "lbl_B";
		((Control)lbl_B).Size = new Size(63, 23);
		lbl_B.Style = UIStyle.Black;
		((Control)lbl_B).TabIndex = 77;
		((Control)lbl_B).Text = "0";
		((Label)lbl_B).TextAlign = (ContentAlignment)16;
		((Control)lbl_G).BackColor = Color.Transparent;
		((Control)lbl_G).Font = new Font("微软雅黑", 12f);
		((Control)lbl_G).ForeColor = Color.Silver;
		((Control)lbl_G).Location = new Point(493, 75);
		((Control)lbl_G).Name = "lbl_G";
		((Control)lbl_G).Size = new Size(63, 23);
		lbl_G.Style = UIStyle.Black;
		((Control)lbl_G).TabIndex = 77;
		((Control)lbl_G).Text = "0";
		((Label)lbl_G).TextAlign = (ContentAlignment)16;
		((Control)lbl_R).BackColor = Color.Transparent;
		((Control)lbl_R).Font = new Font("微软雅黑", 12f);
		((Control)lbl_R).ForeColor = Color.Silver;
		((Control)lbl_R).Location = new Point(493, 35);
		((Control)lbl_R).Name = "lbl_R";
		((Control)lbl_R).Size = new Size(63, 23);
		lbl_R.Style = UIStyle.Black;
		((Control)lbl_R).TabIndex = 77;
		((Control)lbl_R).Text = "0";
		((Label)lbl_R).TextAlign = (ContentAlignment)16;
		((Control)uiButton16).Cursor = Cursors.Hand;
		uiButton16.FillColor = Color.FromArgb(110, 190, 40);
		uiButton16.FillHoverColor = Color.FromArgb(136, 202, 81);
		uiButton16.FillPressColor = Color.FromArgb(100, 168, 35);
		uiButton16.FillSelectedColor = Color.FromArgb(100, 168, 35);
		((Control)uiButton16).Font = new Font("微软雅黑", 12f);
		((Control)uiButton16).Location = new Point(18, 75);
		((Control)uiButton16).MinimumSize = new Size(1, 1);
		((Control)uiButton16).Name = "uiButton16";
		uiButton16.Radius = 0;
		uiButton16.RectColor = Color.FromArgb(110, 190, 40);
		uiButton16.RectHoverColor = Color.FromArgb(136, 202, 81);
		uiButton16.RectPressColor = Color.FromArgb(100, 168, 35);
		uiButton16.RectSelectedColor = Color.FromArgb(100, 168, 35);
		((Control)uiButton16).Size = new Size(48, 26);
		uiButton16.Style = UIStyle.Green;
		uiButton16.StyleCustomMode = true;
		((Control)uiButton16).TabIndex = 76;
		((Control)uiButton16).Text = "G";
		((Control)uiButton17).Cursor = Cursors.Hand;
		((Control)uiButton17).Font = new Font("微软雅黑", 12f);
		uiButton17.ForeSelectedColor = Color.Empty;
		((Control)uiButton17).Location = new Point(18, 120);
		((Control)uiButton17).MinimumSize = new Size(1, 1);
		((Control)uiButton17).Name = "uiButton17";
		uiButton17.Radius = 0;
		uiButton17.RectSelectedColor = Color.Empty;
		((Control)uiButton17).Size = new Size(48, 23);
		uiButton17.StyleCustomMode = true;
		((Control)uiButton17).TabIndex = 75;
		((Control)uiButton17).Text = "B";
		((Control)uiButton4).Cursor = Cursors.Hand;
		uiButton4.FillColor = Color.FromArgb(230, 80, 80);
		uiButton4.FillHoverColor = Color.FromArgb(232, 127, 128);
		uiButton4.FillPressColor = Color.FromArgb(202, 87, 89);
		uiButton4.FillSelectedColor = Color.FromArgb(202, 87, 89);
		((Control)uiButton4).Font = new Font("微软雅黑", 12f);
		((Control)uiButton4).Location = new Point(18, 35);
		((Control)uiButton4).MinimumSize = new Size(1, 1);
		((Control)uiButton4).Name = "uiButton4";
		uiButton4.RectColor = Color.FromArgb(230, 80, 80);
		uiButton4.RectHoverColor = Color.FromArgb(232, 127, 128);
		uiButton4.RectPressColor = Color.FromArgb(202, 87, 89);
		uiButton4.RectSelectedColor = Color.FromArgb(202, 87, 89);
		uiButton4.ShowFocusLine = true;
		((Control)uiButton4).Size = new Size(48, 23);
		uiButton4.Style = UIStyle.Red;
		uiButton4.StyleCustomMode = true;
		((Control)uiButton4).TabIndex = 74;
		((Control)uiButton4).Text = "R";
		((Control)btnReadConfig).BackColor = Color.Transparent;
		((Control)btnReadConfig).Cursor = Cursors.Hand;
		btnReadConfig.FillColor = Color.FromArgb(15, 40, 70);
		btnReadConfig.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnReadConfig.FillPressColor = Color.FromArgb(235, 243, 255);
		btnReadConfig.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnReadConfig).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnReadConfig.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnReadConfig.ForePressColor = Color.FromArgb(130, 130, 130);
		btnReadConfig.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReadConfig).Location = new Point(779, 397);
		((Control)btnReadConfig).Margin = new Padding(2);
		((Control)btnReadConfig).MinimumSize = new Size(1, 1);
		((Control)btnReadConfig).Name = "btnReadConfig";
		btnReadConfig.Radius = 26;
		btnReadConfig.RectColor = Color.FromArgb(130, 130, 130);
		btnReadConfig.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnReadConfig.RectPressColor = Color.FromArgb(130, 130, 130);
		btnReadConfig.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReadConfig).Size = new Size(57, 23);
		btnReadConfig.Style = UIStyle.Black;
		((Control)btnReadConfig).TabIndex = 78;
		((Control)btnReadConfig).Text = "回读";
		((Control)btnReadConfig).Click += btnReadConfig_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 15f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).Controls.Add((Control)(object)uiPanel7);
		((Control)this).Name = "UserControl4";
		((Control)this).Size = new Size(1186, 755);
		((Control)uiPanel7).ResumeLayout(false);
		((Control)uiGroupBox1).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
	}
}
