using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Sunny.UI;

namespace MetaStudio;

public class UserControl3 : UserControl
{
	private System.Timers.Timer t = new System.Timers.Timer();

	private System.Timers.Timer t_reg = new System.Timers.Timer();

	private Form1 frm = null;

	private byte type1 = 1;

	private byte type2 = 0;

	private int addr = 0;

	private int data = 0;

	private byte[] arr;

	private int Seg = 16;

	private bool AllStep = false;

	private byte type3 = 1;

	private byte type4 = 0;

	private Form2 f2 = null;

	private int step = 0;

	private bool canForChang = false;

	private bool canPrint = false;

	public bool canShow = false;

	private bool canColor = false;

	private bool canWriteSN = true;

	private string sn = string.Empty;

	private int maxtrycnt = 0;

	private int trycnt = 0;

	private StreamWriter flog = null;

	private string logfilename = null;

	private bool AllScreen = false;

	private bool AllVersion = false;

	private string upgradRes = string.Empty;

	private string gfilename;

	private int resolution = 1024;

	private double distanceN = 0.0;

	private int overlay_high = 150;

	private int overlay_low = 210;

	private int jishu = 0;

	private double beishuN = 0.5;

	private double tuoyuanN = 1.0;

	private IContainer components = null;

	private UIButton btnView;

	private UIButton btnUpgrade;

	private UIButton btnReadReg;

	private UIButton btnWriteReg;

	private UIButton btnClear;

	private UIButton btnVersion;

	private UILabel uiLabel7;

	private UILabel uiLabel2;

	private UILabel uiLabel1;

	private UILabel uiLabel3;

	private UIProcessBar uiProcessBar1;

	private UILine uiLine1;

	private UILabel uiLabel4;

	private UIButton btnHand;

	private UIButton btnClearData;

	private UIButton btnStepUpgate;

	private UIPanel uiPanel1;

	private UIComboBox cbxRegType;

	private UITextBox txtRegData;

	private UITextBox txtRegAddr;

	private UITextBox txtPath;

	private UIComboBox cbxUpgradeType;

	private UILabel uiLabel5;

	private UILabel uiLabel6;

	private UIComboBox ubxImageType;

	private UIButton btnSpem;

	private UITextBox txtID;

	private UILabel lblID;

	private TextBox txtLog1;

	private UIButton uiButton1;

	private UIPanel panelArgu;

	private UITextBox txtD4;

	private UITextBox txtA4;

	private UITextBox txtD3;

	private UITextBox txtA3;

	private UITextBox txtD2;

	private UITextBox txtA2;

	private UITextBox txtD1;

	private UITextBox txtA1;

	private UILabel uiLabel9;

	private UILabel uiLabel8;

	private UIButton btnWhite;

	private UIButton btnBlue;

	private UIButton btnStop;

	private UIButton btnAuto;

	private UIButton btnGreen;

	private UIButton btnRed;

	private UILabel uiLabel10;

	private UITextBox txtSN;

	private UIButton btnWriteSN;

	private UIButton btnReadSN;

	private UIButton btnAllScreen;

	private UITextBox txtTime;

	private UIButton btnStopReg;

	private UIButton btnAutoReg;

	private UILabel uiLabel11;

	private UIButton btnVersionAll;

	private UIButton btnResetDevice;

	private UIButton btnForChange;

	private UIButton btnUpdateDebug;

	private Panel panel1;

	private UIButton btnCreate;

	private UITextBox txtlA;

	private UITextBox txthA;

	private UITextBox txtOver;

	private UILabel uiLabel14;

	private UILabel uiLabel13;

	private UILabel uiLabel12;

	private UITextBox txtName;

	private UILabel uiLabel15;

	public UserControl3(Form1 frm)
	{
		InitializeComponent();
		t.Elapsed += t_Elapsed;
		t.Enabled = false;
		t.Interval = 2000.0;
		t_reg.Elapsed += t_reg_Elapsed;
		t.Enabled = false;
		t.Interval = 3000.0;
		this.frm = frm;
		cbxUpgradeType.SelectedIndex = 2;
		distanceN = (double)(resolution - 2) * Math.Sqrt(2.0) / 2.0;
	}

	private void t_reg_Elapsed(object sender, ElapsedEventArgs e)
	{
		btnReadReg_Click(null, null);
	}

	private void EnableTimer(int interval, ushort maxtry)
	{
		t.Enabled = true;
		t.Interval = interval;
		maxtrycnt = maxtry;
		trycnt = 0;
	}

	private void t_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (trycnt < maxtrycnt)
		{
			trycnt++;
			byte[] array = new byte[22]
			{
				240, 165, 90, 15, 129, 0, 0, 0, 5, 0,
				3, 0, 0, 0, 204, 204, 117, 87, 27, 0,
				204, 204
			};
			array[4] = type3;
			array[7] = type4;
			array[6] = GetStatorID();
			frm.Send(array);
			PrintLog("Send the query command for the " + trycnt + " time");
		}
		else
		{
			t.Enabled = false;
			trycnt = 0;
			PrintLog("Update Failed!");
			PrintLog("-----------------end-----------------");
		}
	}

	public void btnReadReg_Click(object sender, EventArgs e)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 0, 0, 0, 0, 1, 0,
			8, 0, 0, 0, 204, 204, 3, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		if (!string.IsNullOrEmpty(((Control)txtRegAddr).Text))
		{
			addr = Convert.ToInt32(((Control)txtRegAddr).Text, 16);
			array[6] = GetStatorID();
			array[4] = type1;
			array[7] = type2;
			byte[] bytes = BitConverter.GetBytes(addr);
			array[16] = bytes[0];
			array[17] = bytes[1];
			array[18] = bytes[2];
			array[19] = bytes[3];
			AddLog(array, 0);
			byte[] array2 = frm.Send(array);
		}
	}

	private void btnWriteReg_Click(object sender, EventArgs e)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 0, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 3, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		if (!string.IsNullOrEmpty(((Control)txtRegAddr).Text) && !string.IsNullOrEmpty(((Control)txtRegData).Text))
		{
			addr = Convert.ToInt32(((Control)txtRegAddr).Text, 16);
			data = Convert.ToInt32(((Control)txtRegData).Text, 16);
			byte[] bytes = BitConverter.GetBytes(data);
			array[4] = type1;
			array[6] = GetStatorID();
			array[7] = type2;
			byte[] bytes2 = BitConverter.GetBytes(addr);
			array[16] = bytes2[0];
			array[17] = bytes2[1];
			array[18] = bytes2[2];
			array[19] = bytes2[3];
			for (int i = 0; i < 4; i++)
			{
				array[20 + i] = bytes[i];
			}
			byte[] buf = frm.Send(array);
			AddLog(buf, 0);
		}
	}

	private void ParseSpem(byte[] buf)
	{
		if (buf != null && buf.Length == 26 && buf[4] == 129 && buf[7] == 0 && buf[16] == 17)
		{
			PrintLog("Temperature: " + buf[20] + "℃");
		}
	}

	private void ParseSN(byte[] buf)
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Expected O, but got Unknown
		MethodInvoker val = null;
		MethodInvoker val2 = null;
		if (!SPHelper.CheckHead(buf) || buf == null || buf.Length != 26)
		{
			return;
		}
		if (buf[4] == 1 && buf[7] == 0)
		{
			if (buf[16] == 250)
			{
				byte[] bytes = new byte[2]
				{
					buf[20],
					buf[21]
				};
				sn += Encoding.Default.GetString(bytes);
			}
			else if (buf[16] == 251)
			{
				byte[] bytes = new byte[4]
				{
					buf[20],
					buf[21],
					buf[22],
					buf[23]
				};
				sn += Encoding.Default.GetString(bytes);
			}
			else if (buf[16] == 252)
			{
				uint num = SPHelper.ConvetUInt(buf, 20);
				sn += num;
				if (((Control)this).IsHandleCreated)
				{
					if (val == null)
					{
						val = (MethodInvoker)delegate
						{
							((TextBoxBase)txtLog1).AppendText("Vdbox SN:" + sn);
							((TextBoxBase)txtLog1).AppendText("\r\n");
							sn = "";
						};
					}
					((Control)this).BeginInvoke((Delegate)(object)val);
				}
			}
		}
		if (buf[4] != 129 || buf[7] != 0)
		{
			return;
		}
		if (buf[16] == 250)
		{
			byte[] bytes = new byte[2]
			{
				buf[20],
				buf[21]
			};
			sn += Encoding.Default.GetString(bytes);
		}
		else if (buf[16] == 251)
		{
			byte[] bytes = new byte[4]
			{
				buf[20],
				buf[21],
				buf[22],
				buf[23]
			};
			sn += Encoding.Default.GetString(bytes);
		}
		else
		{
			if (buf[16] != 252)
			{
				return;
			}
			uint num = SPHelper.ConvetUInt(buf, 20);
			sn += num;
			if (!((Control)this).IsHandleCreated)
			{
				return;
			}
			if (val2 == null)
			{
				val2 = (MethodInvoker)delegate
				{
					((TextBoxBase)txtLog1).AppendText("Stator SN:" + sn);
					((TextBoxBase)txtLog1).AppendText("\r\n");
					sn = "";
				};
			}
			((Control)this).BeginInvoke((Delegate)(object)val2);
		}
	}

	private void ParseVersion(byte[] buf)
	{
		if (!SPHelper.CheckHead(buf) || buf == null || buf.Length != 26)
		{
			return;
		}
		if (buf[4] == 1 && buf[7] == 0 && buf[16] == 0)
		{
			string hexVer = GetHexVer(buf);
			PrintLog("Vdbox Version:" + hexVer);
		}
		else if (buf[4] == 1 && buf[7] == 0 && buf[16] == byte.MaxValue)
		{
			int num = buf[20] & 0xF;
			int num2 = (num & 0xC) >> 2;
			int num3 = num & 3;
			string hexVer = "PCBA Version:V" + num2 + "  PCB Version:V" + num3;
			PrintLog("Vdbox Hardware Version:" + hexVer);
			ConnectVersion(buf[6]);
		}
		else if (buf[4] == 1 && buf[7] == 0 && buf[16] == 253)
		{
			int num = buf[20] & 1;
			if (num == 1)
			{
				string hexVer = "Full-Featured Version";
				PrintLog(hexVer);
			}
			else
			{
				string hexVer = "HDMI Version";
				PrintLog(hexVer);
			}
		}
		else if (buf[4] == 129 && buf[7] == 0 && buf[16] == 0)
		{
			string hexVer = GetHexVer(buf);
			PrintLog("Stator Version:" + hexVer);
			ConnectVersion(buf[6]);
		}
		else if (buf[4] == 129 && buf[7] == 0 && buf[16] == byte.MaxValue)
		{
			int num = buf[20] & 0xF;
			int num2 = (num & 0xC) >> 2;
			int num3 = num & 3;
			string hexVer = "PCBA Version:V" + num2 + "  PCB Version:V" + num3;
			PrintLog("Stator Hardware Version:" + hexVer);
		}
		else if (buf[4] == 129 && buf[7] == 128 && buf[16] == 0)
		{
			string hexVer = GetHexVer(buf);
			PrintLog("Rotor Version:" + hexVer);
			ConnectVersion(buf[6]);
		}
		else if (buf[4] == 129 && buf[7] == 128 && buf[16] == byte.MaxValue)
		{
			int num = buf[20] & 0xF;
			int num2 = (num & 0xC) >> 2;
			int num3 = num & 3;
			string hexVer = "PCBA Version:V" + num2 + "  PCB Version:V" + num3;
			PrintLog("Rotor Hardware Version:" + hexVer);
		}
	}

	private void ConnectVersion(int currentID)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		MethodInvoker val = null;
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		if (val == null)
		{
			val = (MethodInvoker)delegate
			{
				if (AllVersion && cbxUpgradeType.SelectedIndex != 0)
				{
					int num = currentID - 1;
					if (num > 1)
					{
						((Control)txtID).Text = num.ToString();
						QueryVersion();
					}
				}
			};
		}
		((Control)this).BeginInvoke((Delegate)(object)val);
	}

	private string GetHexVer(byte[] buf)
	{
		return "v" + buf[23].ToString("X") + "." + buf[22].ToString("X") + "." + buf[21].ToString("X") + "." + buf[20].ToString("X");
	}

	private void ParseRegData(byte[] buf)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		MethodInvoker val = null;
		if (!SPHelper.CheckHead(buf) || buf == null || buf.Length != 26)
		{
			return;
		}
		int data = BitConverter.ToInt32(new byte[4]
		{
			buf[20],
			buf[21],
			buf[22],
			buf[23]
		}, 0);
		if (((Control)this).IsHandleCreated)
		{
			if (val == null)
			{
				val = (MethodInvoker)delegate
				{
					((Control)txtRegData).Text = "0x" + data.ToString("X");
				};
			}
			((Control)this).BeginInvoke((Delegate)(object)val);
		}
		if (f2 != null)
		{
			f2.UpdateQueueValue(data);
		}
	}

	private void ParseUpdateData(byte[] buf)
	{
		if (!SPHelper.CheckHead(buf) || buf == null || buf.Length == 0)
		{
			return;
		}
		if (step == 1 && buf.Length == 20 && buf[8] == 5 && buf[10] == 2 && buf[16] == 0 && buf[17] == 0)
		{
			if (step != 1)
			{
				return;
			}
			t.Enabled = false;
			PrintLog("ID:" + buf[6] + "-Handshakes Succeed!");
			if (AllStep)
			{
				ClearData(GetStatorID());
			}
		}
		if (step == 2)
		{
			if (buf.Length == 20 && buf[8] == 5 && buf[10] == 2 && buf[16] == 1 && buf[17] == 0)
			{
				if (step != 2)
				{
					return;
				}
				PrintLog("ID:" + buf[6] + "-Erasing!");
			}
			else if (buf.Length == 20 && buf[8] == 5 && buf[10] == 2 && buf[16] == 2 && buf[17] == 0)
			{
				if (step != 2)
				{
					return;
				}
				t.Enabled = false;
				PrintLog("ID:" + buf[6] + "-Erasing Complete!");
				if (AllStep)
				{
					StepUpgate(GetStatorID());
				}
			}
		}
		if (step == 4)
		{
			if (buf.Length == 20 && buf[8] == 5 && buf[10] == 2 && buf[16] == 20 && buf[17] == 0)
			{
				PrintLog("ID:" + buf[6] + "-Checking!");
			}
			else if (buf.Length == 20 && buf[8] == 5 && buf[10] == 2 && buf[16] == 24 && buf[17] == 0)
			{
				t.Enabled = false;
				PrintLog("ID:" + buf[6] + "-Update Successfully，Please Power Down and Restart!");
				PrintLog("----------------------end--------------------------");
				SeriesConnection(buf[6]);
				step = 5;
			}
			else if (buf.Length == 20 && buf[8] == 5 && buf[10] == 2 && buf[16] == 8 && buf[17] == 0)
			{
				t.Enabled = false;
				PrintLog("ID:" + buf[6] + "-Update Failed!");
				PrintLog("----------------------end--------------------------");
				SeriesConnection(buf[6]);
				upgradRes = upgradRes + "\\" + buf[6];
				step = 5;
			}
			else
			{
				t.Enabled = false;
				PrintLog("ID:" + buf[6] + "-Update Failed!");
				PrintLog("----------------------end--------------------------");
				SeriesConnection(buf[6]);
				upgradRes = upgradRes + "\\" + buf[6];
				step = 5;
			}
		}
	}

	private void SeriesConnection(int currentID)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		MethodInvoker val = null;
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		if (val == null)
		{
			val = (MethodInvoker)delegate
			{
				//IL_0105: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
				if (ConstData.DeviceCount == 1)
				{
					PowerDown(1);
				}
				else
				{
					int num = currentID - 1;
					if (num <= 1)
					{
						for (int num2 = ConstData.DeviceCount + 1; num2 > 1; num2--)
						{
							PowerDown(num2);
							PrintLog("DeviceID:" + num2 + " PowerDown!");
						}
						if (!string.IsNullOrEmpty(upgradRes))
						{
							PrintLog("DeviceID:" + upgradRes + " Update Failed");
							MessageBox.Show("DeviceID:" + upgradRes + " Update Failed", "System Prompt", (MessageBoxButtons)0, (MessageBoxIcon)64);
						}
						else
						{
							PrintLog("All Device Update Successfully!");
							MessageBox.Show("All Device Update Successfully!", "System Prompt", (MessageBoxButtons)0, (MessageBoxIcon)64);
						}
					}
					else
					{
						((Control)txtID).Text = num.ToString();
						Upgrade();
					}
				}
			};
		}
		((Control)this).BeginInvoke((Delegate)(object)val);
	}

	public void PowerDown(int deviceId)
	{
		SPHelper.SendTOStator(deviceId, 2, 18, 90);
		Thread.Sleep(1000);
		SPHelper.SendTOStator(deviceId, 2, 18, 0);
		SPHelper.SendTOStator(deviceId, 2, 16, 10);
	}

	public void GetSerData(byte[] buf)
	{
		try
		{
			WriteRegToFile(buf);
			AddLog(buf, 1);
			ParseSpem(buf);
			ParseVersion(buf);
			ParseRegData(buf);
			ParseUpdateData(buf);
			ParseSN(buf);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void WriteRegToFile(byte[] receivedData)
	{
		if (!string.IsNullOrEmpty(logfilename))
		{
			string value = "0x" + receivedData[16].ToString("X") + "=0x" + SPHelper.ConvetInt(receivedData, 20).ToString("X");
			if (flog != null)
			{
				flog.WriteLine(value);
				flog.Flush();
			}
		}
	}

	public void AddLog(byte[] buf, int direction)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		if (!canPrint)
		{
			return;
		}
		try
		{
			MethodInvoker val = null;
			string msg = DateTime.Now.ToString();
			switch (direction)
			{
			case 0:
				msg += " ->Send:";
				break;
			case 1:
				msg += " ->Recv:";
				break;
			}
			for (int i = 0; i < buf.Length; i++)
			{
				msg = msg + " " + buf[i].ToString("X");
			}
			if (!((Control)this).IsHandleCreated)
			{
				return;
			}
			if (val == null)
			{
				val = (MethodInvoker)delegate
				{
					((TextBoxBase)txtLog1).AppendText(msg);
					((TextBoxBase)txtLog1).AppendText("\r\n");
				};
			}
			((Control)this).BeginInvoke((Delegate)(object)val);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		ClearLog();
	}

	private void ClearLog()
	{
		((Control)txtLog1).Text = string.Empty;
	}

	private void cbxRegType_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (cbxRegType.SelectedIndex)
		{
		case 0:
			type1 = 1;
			type2 = 0;
			break;
		case 1:
			type1 = 129;
			type2 = 0;
			break;
		case 2:
			type1 = 129;
			type2 = 128;
			break;
		}
	}

	private void btnVersion_Click(object sender, EventArgs e)
	{
		AllVersion = false;
		QueryVersion();
	}

	private void QueryVersion()
	{
		try
		{
			PrintLog("-----------------DeviceID:" + ((Control)txtID).Text + " Version----------------------");
			byte[] array = new byte[26]
			{
				240, 165, 90, 15, 0, 0, 0, 0, 1, 0,
				8, 0, 0, 0, 204, 204, 0, 0, 0, 0,
				0, 0, 0, 0, 204, 204
			};
			array[4] = type3;
			array[7] = type4;
			array[6] = GetStatorID();
			frm.Send(array);
			AddLog(array, 0);
			Thread.Sleep(30);
			switch (cbxUpgradeType.SelectedIndex)
			{
			case 0:
				SPHelper.SendTOVdbox(GetStatorID(), 1, 255, 0);
				Thread.Sleep(30);
				SPHelper.SendTOVdbox(GetStatorID(), 1, 253, 0);
				break;
			case 1:
				SPHelper.SendTOStator(GetStatorID(), 1, 255, 0);
				break;
			case 2:
				SPHelper.SendTORotor(GetStatorID(), 1, 255, 0);
				break;
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
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
			((FileDialog)val).Filter = "文件类型(*.bit)|*.bit";
			((FileDialog)val).Title = "请选择一个bit格式文件";
			((FileDialog)val).InitialDirectory = "C:";
			val.ShowReadOnly = true;
			val.ReadOnlyChecked = true;
			((FileDialog)val).ShowHelp = true;
			if (!(((int)((CommonDialog)val).ShowDialog() == 1) & (((FileDialog)val).FileNames.Length > 0)))
			{
				return;
			}
			gfilename = ((FileDialog)val).FileNames[0].Substring(((FileDialog)val).FileNames[0].LastIndexOf("\\") + 1);
			string text = ((FileDialog)val).FileNames[0].Substring(0, ((FileDialog)val).FileNames[0].LastIndexOf("\\"));
			string fileName = ((FileDialog)val).FileName;
			((Control)txtPath).Text = ((FileDialog)val).FileName;
			arr = File.ReadAllBytes(fileName);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < arr.Length; i++)
			{
				stringBuilder.Append(arr[i].ToString("X2") + " ");
			}
			if (gfilename.Contains("Vdbox"))
			{
				cbxUpgradeType.SelectedIndex = 0;
			}
			else if (gfilename.Contains("Stator"))
			{
				cbxUpgradeType.SelectedIndex = 1;
				if (gfilename.Contains("Stator_mix_data"))
				{
					MetaTool.SetUpgrade(GetStatorID(), 1);
				}
				else
				{
					MetaTool.SetUpgrade(0, 0);
				}
			}
			else if (gfilename.Contains("Rotor"))
			{
				cbxUpgradeType.SelectedIndex = 2;
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private bool JudgeFileName(string _fileName)
	{
		if (string.IsNullOrEmpty(_fileName))
		{
			return false;
		}
		string fileName = Path.GetFileName(_fileName);
		int selectedIndex = cbxUpgradeType.SelectedIndex;
		if (fileName.Contains("Vdbox") && selectedIndex == 0)
		{
			return true;
		}
		if (fileName.Contains("Stator") && selectedIndex == 1)
		{
			return true;
		}
		if (fileName.Contains("Rotor") && selectedIndex == 2)
		{
			return true;
		}
		return false;
	}

	private void btnHand_Click(object sender, EventArgs e)
	{
		AllStep = false;
		HandShock(GetStatorID());
	}

	private void HandShock(int deviceId)
	{
		ResetFlash(deviceId);
		byte[] array = new byte[22]
		{
			240, 165, 90, 15, 129, 0, 0, 0, 5, 0,
			3, 0, 0, 0, 204, 204, 117, 87, 27, 0,
			204, 204
		};
		array[6] = (byte)deviceId;
		array[4] = type3;
		array[7] = type4;
		PrintLog("Send Handshake Command!");
		AddLog(array, 0);
		frm.Send(array);
		step = 1;
		EnableTimer(2000, 6);
	}

	private void ResetFlash(int deviceId)
	{
		switch (cbxUpgradeType.SelectedIndex)
		{
		case 0:
			SPHelper.SendTOVdbox(deviceId, 2, 92, 92);
			SPHelper.SendTOVdbox(deviceId, 2, 92, 0);
			break;
		case 1:
			SPHelper.SendTOStator(deviceId, 2, 92, 92);
			SPHelper.SendTOStator(deviceId, 2, 92, 0);
			break;
		case 2:
			SPHelper.SendTORotor(deviceId, 2, 57, 92);
			SPHelper.SendTORotor(deviceId, 2, 57, 0);
			break;
		}
	}

	private void btnClearData_Click(object sender, EventArgs e)
	{
		AllStep = false;
		ClearData(GetStatorID());
	}

	public byte GetStatorID()
	{
		try
		{
			byte result = 1;
			if (!string.IsNullOrEmpty(((Control)txtID).Text))
			{
				result = ((!((Control)txtID).Text.Contains("0x")) ? ((byte)Convert.ToInt32(((Control)txtID).Text, 10)) : ((byte)Convert.ToInt32(((Control)txtID).Text, 16)));
			}
			return result;
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
			return 1;
		}
	}

	private void ClearData(int deviceId)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 129, 0, 0, 0, 5, 0,
			8, 0, 0, 0, 204, 204, 118, 103, 32, 0,
			118, 103, 32, 4, 204, 204
		};
		array[6] = (byte)deviceId;
		array[4] = type3;
		array[7] = type4;
		frm.Send(array);
		PrintLog("Send Erase Command!");
		AddLog(array, 0);
		step = 2;
		EnableTimer(2000, 6);
	}

	private void StepUpgate(int deviceId)
	{
		Task task = new Task(delegate
		{
			try
			{
				step = 3;
				if (arr != null && arr.Length != 0)
				{
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					int num4 = arr.Length;
					num2 = num4 % Seg;
					num3 = ((num2 <= 0) ? (num4 / Seg) : (num4 / Seg + 1));
					uiProcessBar1.Maximum = num3;
					PrintLog("Total Packet：" + num3);
					byte[] array = new byte[22]
					{
						240, 165, 90, 15, 129, 0, 0, 0, 5, 0,
						4, 0, 0, 0, 204, 204, 118, 103, 32, 12,
						204, 204
					};
					array[6] = (byte)deviceId;
					array[4] = type3;
					array[7] = type4;
					frm.Send(array);
					for (int i = 16; i < array.Length; i++)
					{
						num ^= array[i];
					}
					Thread.Sleep(1);
					byte[] array2 = new byte[16]
					{
						240, 165, 90, 15, 129, 0, 0, 0, 5, 0,
						16, 0, 0, 0, 204, 204
					};
					array2[4] = type3;
					array2[7] = type4;
					array2[6] = (byte)deviceId;
					byte[] array3 = new byte[16 + Seg + 2];
					Array.Copy(array2, 0, array3, 0, 16);
					PrintLog("System is being upgraded, please wait...");
					for (int j = 0; j < num3; j++)
					{
						for (int i = 16; i < array3.Length; i++)
						{
							array3[i] = 0;
						}
						array3[32] = 204;
						array3[33] = 204;
						if (j == num3 - 1 && num4 % Seg != 0)
						{
							Array.Copy(arr, j * Seg, array3, 16, arr.Length - j * Seg);
							AddLog(array3, 0);
							byte[] array4 = frm.Send(array3);
						}
						else
						{
							Array.Copy(arr, j * Seg, array3, 16, Seg);
							byte[] array5 = frm.Send(array3);
							if (j == 0)
							{
								AddLog(array3, 0);
							}
						}
						uiProcessBar1.Value = j;
					}
					for (int i = 0; i < arr.Length; i++)
					{
						num ^= arr[i];
					}
					byte[] array6 = new byte[24]
					{
						240, 165, 90, 15, 129, 0, 0, 0, 5, 0,
						5, 0, 0, 0, 204, 204, 118, 103, 32, 0,
						255, 0, 204, 204
					};
					array6[4] = type3;
					array6[7] = type4;
					array6[6] = GetStatorID();
					for (int i = 16; i < array6.Length - 4; i++)
					{
						num ^= array6[i];
					}
					array6[20] = (byte)num;
					byte[] array7 = frm.Send(array6);
					PrintLog("CRC Check!");
					AddLog(array6, 0);
					PrintLog("Upgrade Completed...");
					EnableTimer(2000, 6);
					step = 4;
				}
			}
			catch (Exception ex)
			{
				LogerHelper.Error(ex.Message);
			}
		});
		task.Start();
	}

	private void btnStepUpgate_Click(object sender, EventArgs e)
	{
		AllStep = false;
		StepUpgate(GetStatorID());
	}

	private void PrintLog(string msg)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		MethodInvoker val = null;
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		if (val == null)
		{
			val = (MethodInvoker)delegate
			{
				((TextBoxBase)txtLog1).AppendText(DateTime.Now.ToString() + "->" + msg);
				((TextBoxBase)txtLog1).AppendText("\r\n");
				LogerHelper.Info(msg);
			};
		}
		((Control)this).BeginInvoke((Delegate)(object)val);
	}

	private void btnUpgrade_Click(object sender, EventArgs e)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if ((int)MessageBox.Show("确认是否执行过联屏初始化操作(Y/N)？", "系统提示", (MessageBoxButtons)4, (MessageBoxIcon)64) == 6 && gfilename.Contains("Stator_mix_data"))
		{
			MetaTool.SetUpgrade(0, 1);
			if (ConstData.DeviceCount == 1)
			{
				((Control)txtID).Text = 1.ToString();
			}
			else
			{
				((Control)txtID).Text = (ConstData.DeviceCount + 1).ToString();
			}
			AllScreen = true;
			Upgrade();
		}
	}

	private void Upgrade()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		MetaTool.Stop(0);
		if (!JudgeFileName(((Control)txtPath).Text))
		{
			MessageBox.Show("The upgrade file is invalid or the upgrade type does not match！", "System Prompt", (MessageBoxButtons)0, (MessageBoxIcon)64);
		}
		else if (!string.IsNullOrEmpty(((Control)txtID).Text) && Helper.IsNumeric(((Control)txtID).Text) && Convert.ToInt32(((Control)txtID).Text) >= 1)
		{
			uiProcessBar1.Value = 0;
			PrintLog("---------------Starting(DeviceID:" + ((Control)txtID).Text + ")---------------");
			AllStep = true;
			HandShock(GetStatorID());
		}
	}

	private void cbxUpgradeType_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (cbxUpgradeType.SelectedIndex)
		{
		case 0:
			type3 = 1;
			type4 = 0;
			break;
		case 1:
			type3 = 129;
			type4 = 0;
			break;
		case 2:
			type3 = 129;
			type4 = 128;
			break;
		}
	}

	private void ubxImageType_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = ubxImageType.SelectedIndex;
		SPHelper.SendTOStator(0, 2, 40, selectedIndex + 2);
	}

	private void ShowControl()
	{
		if (canShow)
		{
			((Control)btnHand).Visible = true;
			((Control)btnClearData).Visible = true;
			((Control)btnStepUpgate).Visible = true;
			((Control)uiLabel6).Visible = true;
			((Control)ubxImageType).Visible = true;
			((Control)uiLabel4).Visible = true;
			((Control)cbxRegType).Visible = true;
			((Control)uiLabel2).Visible = true;
			((Control)txtRegAddr).Visible = true;
			((Control)uiLabel3).Visible = true;
			((Control)txtRegData).Visible = true;
			((Control)btnReadReg).Visible = true;
			((Control)btnWriteReg).Visible = true;
			((Control)btnSpem).Visible = true;
			((Control)btnResetDevice).Visible = true;
			((Control)panelArgu).Visible = true;
		}
		else
		{
			((Control)btnHand).Visible = false;
			((Control)btnClearData).Visible = false;
			((Control)btnStepUpgate).Visible = false;
			((Control)uiLabel6).Visible = false;
			((Control)ubxImageType).Visible = false;
			((Control)uiLabel4).Visible = false;
			((Control)cbxRegType).Visible = false;
			((Control)uiLabel2).Visible = false;
			((Control)txtRegAddr).Visible = false;
			((Control)uiLabel3).Visible = false;
			((Control)txtRegData).Visible = false;
			((Control)btnReadReg).Visible = false;
			((Control)btnWriteReg).Visible = false;
			((Control)btnSpem).Visible = false;
			((Control)btnResetDevice).Visible = false;
			((Control)panelArgu).Visible = false;
		}
	}

	private void btnSpem_Click(object sender, EventArgs e)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 129, 0, 0, 0, 1, 0,
			8, 0, 0, 0, 204, 204, 17, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		int selectedIndex = ubxImageType.SelectedIndex;
		array[20] = (byte)(selectedIndex + 2);
		array[6] = GetStatorID();
		frm.Send(array);
		AddLog(array, 0);
	}

	private void txtLog1_KeyDown(object sender, KeyEventArgs e)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Invalid comparison between Unknown and I4
		if ((int)e.KeyCode == 80 && (int)e.Modifiers == 262144)
		{
			if (canShow)
			{
				canShow = false;
				((Form)frm).Size = new Size(900, 797);
			}
			else
			{
				canShow = true;
				((Form)frm).Size = new Size(1199, 797);
			}
			ShowControl();
		}
		if ((int)e.KeyCode == 87 && (int)e.Modifiers == 262144)
		{
			if (canPrint)
			{
				canPrint = false;
			}
			else
			{
				canPrint = true;
			}
		}
	}

	private void uiButton1_Click(object sender, EventArgs e)
	{
		f2 = new Form2(this);
		((Control)f2).Show();
	}

	private void txtD2_Leave(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(((Control)txtA2).Text) && !string.IsNullOrEmpty(((Control)txtD2).Text))
		{
			int num = Convert.ToInt32(((Control)txtA2).Text, 16);
			int num2 = Convert.ToInt32(((Control)txtD2).Text, 16);
			int statorID = GetStatorID();
			SPHelper.SendTOStator(statorID, 2, num, num2);
		}
	}

	private void txtD3_Leave(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(((Control)txtA3).Text) && !string.IsNullOrEmpty(((Control)txtD3).Text))
		{
			int num = Convert.ToInt32(((Control)txtA3).Text, 16);
			int num2 = Convert.ToInt32(((Control)txtD3).Text, 16);
			int statorID = GetStatorID();
			SPHelper.SendTOStator(statorID, 2, num, num2);
		}
	}

	private void txtD4_Leave(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(((Control)txtA4).Text) && !string.IsNullOrEmpty(((Control)txtD4).Text))
		{
			int num = Convert.ToInt32(((Control)txtA4).Text, 16);
			int num2 = Convert.ToInt32(((Control)txtD4).Text, 16);
			int statorID = GetStatorID();
			SPHelper.SendTOStator(statorID, 2, num, num2);
		}
	}

	private void txtD1_Leave(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(((Control)txtA1).Text) && !string.IsNullOrEmpty(((Control)txtD1).Text))
		{
			int num = Convert.ToInt32(((Control)txtA1).Text, 16);
			int num2 = Convert.ToInt32(((Control)txtD1).Text, 16);
			int statorID = GetStatorID();
			SPHelper.SendTOStator(statorID, 2, num, num2);
		}
	}

	private void btnRed_Click(object sender, EventArgs e)
	{
		SPHelper.SendTORotor(GetStatorID(), 2, 26, 65281);
	}

	private void btnGreen_Click(object sender, EventArgs e)
	{
		SPHelper.SendTORotor(GetStatorID(), 2, 26, 65282);
	}

	private void btnBlue_Click(object sender, EventArgs e)
	{
		SPHelper.SendTORotor(GetStatorID(), 2, 26, 65283);
	}

	private void btnWhite_Click(object sender, EventArgs e)
	{
		SPHelper.SendTORotor(GetStatorID(), 2, 26, 65284);
	}

	private void btnAuto_Click(object sender, EventArgs e)
	{
		canColor = true;
		Task task = new Task(delegate
		{
			while (canColor)
			{
				SPHelper.SendTORotor(GetStatorID(), 2, 26, 65281);
				Thread.Sleep(2000);
				SPHelper.SendTORotor(GetStatorID(), 2, 26, 65282);
				Thread.Sleep(2000);
				SPHelper.SendTORotor(GetStatorID(), 2, 26, 65283);
				Thread.Sleep(2000);
				SPHelper.SendTORotor(GetStatorID(), 2, 26, 65284);
				Thread.Sleep(2000);
			}
		});
		task.Start();
	}

	private void btnStop_Click(object sender, EventArgs e)
	{
		canColor = false;
	}

	private void btnReadSN_Click(object sender, EventArgs e)
	{
		Task task = new Task(delegate
		{
			if (canWriteSN)
			{
				sn = string.Empty;
				if (type3 == 1 && type4 == 0)
				{
					Thread.Sleep(100);
					SPHelper.SendTOVdbox(GetStatorID(), 1, 250, 0);
					Thread.Sleep(100);
					SPHelper.SendTOVdbox(GetStatorID(), 1, 251, 0);
					Thread.Sleep(100);
					SPHelper.SendTOVdbox(GetStatorID(), 1, 252, 0);
				}
				else if (type3 == 129 && type4 == 0)
				{
					Thread.Sleep(100);
					SPHelper.SendTOStator(GetStatorID(), 1, 250, 0);
					Thread.Sleep(100);
					SPHelper.SendTOStator(GetStatorID(), 1, 251, 0);
					Thread.Sleep(100);
					SPHelper.SendTOStator(GetStatorID(), 1, 252, 0);
				}
			}
		});
		task.Start();
	}

	private void btnWriteSN_Click(object sender, EventArgs e)
	{
		Task task = new Task(delegate
		{
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Expected O, but got Unknown
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Expected O, but got Unknown
			MethodInvoker val = null;
			MethodInvoker val2 = null;
			try
			{
				if (string.IsNullOrEmpty(((Control)txtSN).Text) || ((Control)txtSN).Text.Length != 14 || !canWriteSN)
				{
					return;
				}
				canWriteSN = false;
				string text = ((Control)txtSN).Text;
				if (type3 == 129 && type4 == 128)
				{
					return;
				}
				string str = text.Substring(0, 2);
				WriteSN(str, 250);
				Thread.Sleep(30);
				string str2 = text.Substring(2, 4);
				WriteSN(str2, 251);
				Thread.Sleep(30);
				string value = text.Substring(6, text.Length - 6);
				int num = Convert.ToInt32(value);
				if (type3 == 1 && type4 == 0)
				{
					SPHelper.SendTOVdbox(GetStatorID(), 2, 252, num);
					RegisterHelper.SaveConfig(GetStatorID());
					if (((Control)this).IsHandleCreated)
					{
						if (val == null)
						{
							val = (MethodInvoker)delegate
							{
								Thread.Sleep(2000);
								((TextBoxBase)txtLog1).AppendText("Vdbox SN Write Successful!");
								((TextBoxBase)txtLog1).AppendText("\r\n");
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val);
					}
				}
				else if (type3 == 129 && type4 == 0)
				{
					SPHelper.SendTOStator(GetStatorID(), 2, 252, num);
					RegisterHelper.SaveConfig(GetStatorID());
					if (((Control)this).IsHandleCreated)
					{
						if (val2 == null)
						{
							val2 = (MethodInvoker)delegate
							{
								Thread.Sleep(2000);
								((TextBoxBase)txtLog1).AppendText("Stator SN Write Successful!");
								((TextBoxBase)txtLog1).AppendText("\r\n");
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val2);
					}
				}
			}
			catch (Exception ex)
			{
				canWriteSN = true;
				MessageBox.Show(ex.Message, "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
			}
			canWriteSN = true;
		});
		task.Start();
	}

	private void WriteSN(string str, byte address)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 0, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 0, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		array[4] = type3;
		array[6] = GetStatorID();
		array[7] = type4;
		array[16] = address;
		byte[] bytes = Encoding.Default.GetBytes(str);
		if (bytes.Length <= 4)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				array[20 + i] = bytes[i];
			}
		}
		byte[] array2 = frm.Send(array);
	}

	public void Translate(int type)
	{
		switch (type)
		{
		case 0:
			((Control)btnView).Text = "浏览";
			((Control)btnUpgrade).Text = "融合升级";
			((Control)btnReadReg).Text = "读取";
			((Control)btnWriteReg).Text = "写入";
			((Control)btnClear).Text = "清除";
			((Control)btnVersion).Text = "单机版本";
			((Control)uiLabel2).Text = "寄存器地址:";
			((Control)uiLabel1).Text = "升级进度:";
			((Control)uiLabel3).Text = "寄存器数据:";
			((Control)uiLine1).Text = "日志信息";
			((Control)uiLabel4).Text = "寄存器类型:";
			((Control)btnHand).Text = "握手";
			((Control)btnClearData).Text = "擦除";
			((Control)btnStepUpgate).Text = "升级";
			((Control)uiLabel5).Text = "板卡类型:";
			((Control)uiLabel7).Text = "文件路径:";
			((Control)lblID).Text = "设备ID:";
			cbxUpgradeType.Items.Clear();
			((Control)cbxUpgradeType).Text = "视频盒";
			cbxUpgradeType.Items.AddRange(new object[3] { "视频盒", "定子", "转子" });
			cbxRegType.Items.Clear();
			((Control)cbxRegType).Text = "视频盒";
			cbxRegType.Items.AddRange(new object[3] { "视频盒", "定子", "转子" });
			((Control)uiLabel6).Text = "图像类型:";
			((Control)btnSpem).Text = "温度查询";
			((Control)btnRed).Text = "红色";
			((Control)btnGreen).Text = "绿色";
			((Control)btnAuto).Text = "自动";
			((Control)btnStop).Text = "停止";
			((Control)btnBlue).Text = "蓝色";
			((Control)btnWhite).Text = "白色";
			((Control)uiButton1).Text = "显示图表";
			((Control)btnReadSN).Text = "读取";
			((Control)btnWriteSN).Text = "写入";
			((Control)uiLabel10).Text = "序列号:";
			((Control)uiLabel8).Text = "寄存器地址:";
			((Control)uiLabel9).Text = "寄存器数据:";
			((Control)btnAllScreen).Text = "应用升级";
			((Control)btnVersionAll).Text = "联屏版本";
			((Control)btnResetDevice).Text = "恢复出厂设置";
			((Control)btnAutoReg).Text = "启动读";
			((Control)btnStopReg).Text = "停止读";
			((Control)uiLabel11).Text = "时间:";
			((Control)btnForChange).Text = "循环切换";
			break;
		case 1:
			((Control)btnView).Text = "Browse";
			((Control)btnUpgrade).Text = "Data Upgrade";
			((Control)btnReadReg).Text = "Read";
			((Control)btnWriteReg).Text = "Write";
			((Control)btnClear).Text = "Clear";
			((Control)btnVersion).Text = "Version";
			((Control)uiLabel2).Text = "Address:";
			((Control)uiLabel1).Text = "Progress:";
			((Control)uiLabel3).Text = "Data:";
			((Control)uiLine1).Text = "Log Infomation";
			((Control)uiLabel4).Text = "Device Type:";
			((Control)btnHand).Text = "Handshake";
			((Control)btnClearData).Text = "Erase";
			((Control)btnStepUpgate).Text = "Upgrade";
			((Control)uiLabel5).Text = "Device Type:";
			((Control)uiLabel7).Text = "Path:";
			((Control)lblID).Text = "Device ID:";
			cbxUpgradeType.Items.Clear();
			((Control)cbxUpgradeType).Text = "Vdbox";
			cbxUpgradeType.Items.AddRange(new object[3] { "Vdbox", "Stator", "Rotor" });
			cbxRegType.Items.Clear();
			((Control)cbxRegType).Text = "Vdbox";
			cbxRegType.Items.AddRange(new object[3] { "Vdbox", "Stator", "Rotor" });
			((Control)uiLabel6).Text = "Image:";
			((Control)btnSpem).Text = "Temperature";
			((Control)btnRed).Text = "Red";
			((Control)btnGreen).Text = "Green";
			((Control)btnAuto).Text = "Auto";
			((Control)btnStop).Text = "Stop";
			((Control)btnBlue).Text = "Blue";
			((Control)btnWhite).Text = "White";
			((Control)uiButton1).Text = "Show Chart";
			((Control)btnReadSN).Text = "Write";
			((Control)btnWriteSN).Text = "Read";
			((Control)uiLabel10).Text = "SN:";
			((Control)uiLabel8).Text = "Address:";
			((Control)uiLabel9).Text = "Data:";
			((Control)btnAllScreen).Text = "Upgrade-All";
			((Control)btnVersionAll).Text = "Version-All";
			((Control)btnResetDevice).Text = "Reset System";
			((Control)btnAutoReg).Text = "Read";
			((Control)btnStopReg).Text = "Stop";
			((Control)uiLabel11).Text = "Time:";
			((Control)btnForChange).Text = "Change";
			break;
		}
		cbxUpgradeType.SelectedIndex = 2;
		cbxRegType.SelectedIndex = 2;
	}

	private void btnAllScreen_Click(object sender, EventArgs e)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if ((int)MessageBox.Show("确认是否执行过联屏初始化操作(Y/N)？", "系统提示", (MessageBoxButtons)4, (MessageBoxIcon)64) != 6)
		{
			return;
		}
		AllScreen = true;
		upgradRes = string.Empty;
		if (cbxUpgradeType.SelectedIndex == 0 || ConstData.DeviceCount == 1)
		{
			((Control)txtID).Text = 1.ToString();
		}
		else
		{
			((Control)txtID).Text = (ConstData.DeviceCount + 1).ToString();
		}
		if (!gfilename.Contains("Stator_mix_data"))
		{
			if (cbxUpgradeType.SelectedIndex == 1)
			{
				MetaTool.SetUpgrade(0, 0);
			}
			Upgrade();
		}
	}

	private void btnStopReg_Click(object sender, EventArgs e)
	{
		t_reg.Enabled = false;
	}

	private void btnAutoReg_Click(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(((Control)txtTime).Text))
		{
			DateTime now = DateTime.Now;
			logfilename = Path.Combine(Application.StartupPath, "config") + "\\" + ((Control)cbxRegType).Text + "_" + ((Control)txtID).Text + "_" + now.ToString("yyyyMMddHHmmss") + ".txt";
			int num = Convert.ToInt32(((Control)txtTime).Text);
			flog = new StreamWriter(logfilename, append: true);
			t_reg.Interval = num;
			t_reg.Enabled = true;
		}
	}

	private void btnVersionAll_Click(object sender, EventArgs e)
	{
		AllVersion = true;
		if (cbxUpgradeType.SelectedIndex == 0 || ConstData.DeviceCount == 1)
		{
			((Control)txtID).Text = 1.ToString();
		}
		else
		{
			((Control)txtID).Text = (ConstData.DeviceCount + 1).ToString();
		}
		QueryVersion();
	}

	private void btnResetDevice_Click(object sender, EventArgs e)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if ((int)MessageBox.Show("确认执行恢复出厂设置(Y/N)？", "系统提示", (MessageBoxButtons)4, (MessageBoxIcon)64) == 6)
		{
			MetaTool.Stop(0);
			switch (cbxUpgradeType.SelectedIndex)
			{
			case 0:
				DataParser.ResetVdbox(txtLog1, GetStatorID(), null);
				break;
			case 1:
				DataParser.ResetStator(txtLog1, GetStatorID(), null);
				break;
			case 2:
				DataParser.ResetRotor(txtLog1, GetStatorID(), null);
				break;
			}
		}
	}

	private void btnForChange_Click(object sender, EventArgs e)
	{
		if (((Control)btnForChange).Text == "循环切换")
		{
			((Control)btnForChange).Text = "停止";
			int selIndex = 0;
			canForChang = true;
			Task task = new Task(delegate
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002f: Expected O, but got Unknown
				MethodInvoker val = null;
				while (canForChang)
				{
					if (((Control)this).IsHandleCreated)
					{
						UserControl3 userControl = this;
						if (val == null)
						{
							val = (MethodInvoker)delegate
							{
								ubxImageType.SelectedIndex = selIndex;
							};
						}
						((Control)userControl).BeginInvoke((Delegate)(object)val);
					}
					Thread.Sleep(Convert.ToInt32(((Control)txtTime).Text));
					selIndex++;
					if (selIndex > 13)
					{
						selIndex = 0;
					}
				}
			});
			task.Start();
		}
		else
		{
			canForChang = false;
			((Control)btnForChange).Text = "循环切换";
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

	private void button2_Click(object sender, EventArgs e)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		if (string.IsNullOrEmpty(((Control)txtName).Text))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = Path.Combine(Application.StartupPath, "ImageData\\images\\");
		Bitmap val = new Bitmap(text + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".png");
		try
		{
			for (int i = 0; i < ((Image)val).Width; i++)
			{
				for (int j = 0; j < ((Image)val).Height; j++)
				{
					Color pixel = val.GetPixel(j, i);
					stringBuilder.Append("x=" + i + "|y=" + j + "|A=" + pixel.A + "|R=" + pixel.R + "|G=" + pixel.G + "|B=" + pixel.B + " \r\n");
				}
			}
			string text2 = Path.Combine(Application.StartupPath, "ImageData\\txt\\Stator_mix_data");
			File.WriteAllText(text2 + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".txt", stringBuilder.ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void button3_Click(object sender, EventArgs e)
	{
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Expected O, but got Unknown
		string text = Path.Combine(Application.StartupPath, "ImageData\\txt\\Stator_mix_data");
		string[] array = File.ReadAllLines(text + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".txt");
		int num = 0;
		byte[,] array2 = new byte[1024, 1024];
		List<byte> list = new List<byte>();
		string[] array3 = array;
		foreach (string text2 in array3)
		{
			if (!string.IsNullOrEmpty(text2))
			{
				num++;
				string[] array4 = text2.Split(new char[1] { '|' });
				string[] array5 = array4[0].Split(new char[1] { '=' });
				string[] array6 = array4[1].Split(new char[1] { '=' });
				string[] array7 = array4[2].Split(new char[1] { '=' });
				int num2 = int.Parse(array5[1]);
				int num3 = int.Parse(array6[1]);
				byte b = byte.Parse(array7[1]);
				list.Add((byte)(255 - b));
				array2[num2, num3] = b;
			}
		}
		string text3 = Path.Combine(Application.StartupPath, "ImageData\\bit\\Stator_mix_data");
		using (FileStream fileStream = new FileStream(text3 + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".bit", FileMode.OpenOrCreate, FileAccess.Write))
		{
			fileStream.Write(list.ToArray(), 0, list.Count);
		}
		Bitmap val = new Bitmap(1024, 1024);
		for (int num2 = 0; num2 < 1024; num2++)
		{
			for (int num3 = 0; num3 < 1024; num3++)
			{
				val.SetPixel(num2, num3, Color.FromArgb(array2[num2, num3], 0, 0, 0));
			}
		}
		Console.WriteLine(list.Count);
		Console.WriteLine(num);
	}

	private void button4_Click(object sender, EventArgs e)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		string text = Path.Combine(Application.StartupPath, "ImageData\\bit\\Stator_mix_data");
		byte[] array = File.ReadAllBytes(text + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".bit");
		Bitmap val = new Bitmap(1024, 1024);
		for (int i = 0; i < 1024; i++)
		{
			for (int j = 0; j < 1024; j++)
			{
				val.SetPixel(i, j, Color.FromArgb(array[j + i * 1024], 0, 0, 0));
			}
		}
		string text2 = Path.Combine(Application.StartupPath, "ImageData\\rebuild\\Stator_mix_data");
		((Image)val).Save(text2 + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".png", ImageFormat.Png);
	}

	private void button1_Click(object sender, EventArgs e)
	{
		Task task = new Task(delegate
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			((Control)this).BeginInvoke((Delegate)(MethodInvoker)delegate
			{
				//IL_0283: Unknown result type (might be due to invalid IL or missing references)
				//IL_011a: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					string text = Path.Combine(Application.StartupPath, "ImageData\\images\\");
					string maskPath = text + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".png";
					int overlap = int.Parse(((Control)txtOver).Text);
					int hAlhpa = int.Parse(((Control)txthA).Text);
					int lAlhpa = int.Parse(((Control)txtlA).Text);
					DoDrawMask(1, 1, 2, 2, maskPath, overlap, 1024, hAlhpa, lAlhpa);
					Thread.Sleep(1000);
					button2_Click(null, null);
					Thread.Sleep(1000);
					button3_Click(null, null);
					Thread.Sleep(1000);
					button4_Click(null, null);
					MessageBox.Show("生成成功！", "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
					((Control)txtID).Text = "03";
					cbxUpgradeType.SelectedIndex = 1;
					MetaTool.SetUpgrade(GetStatorID(), 1);
					string text2 = Path.Combine(Application.StartupPath, "ImageData\\bit\\Stator_mix_data");
					gfilename = "Stator_mix_data" + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".bit";
					string text3 = text2 + ((Control)txtName).Text + "_" + ((Control)txthA).Text + "_" + ((Control)txtlA).Text + "_" + ((Control)txtOver).Text + ".bit";
					((Control)txtPath).Text = text3;
					arr = File.ReadAllBytes(text3);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
				}
			});
		});
		task.Start();
	}

	public void DoDrawMask(int positionW, int positionH, int gw, int gh, string maskPath, int overlap, int rs, int hAlhpa, int lAlhpa)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Expected O, but got Unknown
		int num = 150;
		int num2 = 255;
		if (overlap < 0)
		{
			overlap = 0;
		}
		if (overlap > 100)
		{
			overlap = 100;
		}
		int num3 = rs / 2;
		Color black = Color.Black;
		Point point = new Point(num3, num3);
		Bitmap val = new Bitmap(rs, rs, (PixelFormat)2498570);
		int height = ((Image)val).Height;
		int width = ((Image)val).Width;
		int num4 = (int)((2.0 - Math.Sqrt(2.0)) * (double)num3);
		if (num4 % 2 != 0)
		{
			num4++;
		}
		double num5 = (double)num4 * 1.0 / 2.0;
		int num6 = num4 / 2;
		int num7 = (int)(Math.Sqrt(2.0) * (double)num3);
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int d = CalcuateDistance(j, point.X, i, point.Y);
				if (IsOutOfCircle(d, num3))
				{
					val.SetPixel(j, i, black);
					continue;
				}
				long num8 = fun1(j, i, rs);
				long num9 = fun2(j, i);
				try
				{
					double num10 = Math.Sqrt(Math.Pow(j - point.X, 2.0) + Math.Pow(i - point.Y, 2.0));
					int num11 = (int)((double)(num4 / 2) * ((double)overlap * 1.0 / 100.0));
					if ((int)Math.Pow(num3, 2.0) < (int)Math.Pow(j - point.X, 2.0) + (int)Math.Pow(i - point.Y, 2.0) || (int)Math.Pow(num3 - num11, 2.0) > (int)Math.Pow(j - point.X, 2.0) + (int)Math.Pow(i - point.Y, 2.0))
					{
						continue;
					}
					for (int num12 = num11; num12 >= 0; num12--)
					{
						if ((int)Math.Pow(num3 - num12, 2.0) >= (int)Math.Pow(j - point.X, 2.0) + (int)Math.Pow(i - point.Y, 2.0))
						{
							double num13 = (double)hAlhpa / ((double)(num11 * num11) * 1.0);
							double num14 = num13 * (double)(num11 - num12) * (double)(num11 - num12);
							if (num14 <= 0.0)
							{
								num14 = 0.0;
							}
							Color color = Color.FromArgb((int)num14, black.R, black.G, black.B);
							val.SetPixel(j, i, color);
							break;
						}
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
		}
		Bitmap val2 = new Bitmap((Image)(object)val, new Size(rs, rs));
		((Image)val2).Save(maskPath, ImageFormat.Png);
		((Image)val2).Dispose();
	}

	private int CalcuateDistance(int x1, int x2, int y1, int y2)
	{
		return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
	}

	private bool IsOccur(int d1, int d2, int r)
	{
		int num = r * r;
		return d1 <= num && d2 <= num;
	}

	private bool IsOutOfCircle(int d, int r)
	{
		int num = r * r;
		return d > num;
	}

	private int fun1(int x, int y, int c)
	{
		return y + x - c;
	}

	private int fun2(int x, int y)
	{
		return y - x;
	}

	private void btnCreate_Click(object sender, EventArgs e)
	{
		button1_Click(null, null);
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
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Expected O, but got Unknown
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Expected O, but got Unknown
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Expected O, but got Unknown
		//IL_06bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0881: Unknown result type (might be due to invalid IL or missing references)
		//IL_088b: Expected O, but got Unknown
		//IL_090e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aeb: Expected O, but got Unknown
		//IL_0b6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d4b: Expected O, but got Unknown
		//IL_0dce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f94: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f9e: Expected O, but got Unknown
		//IL_1021: Unknown result type (might be due to invalid IL or missing references)
		//IL_116c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1176: Expected O, but got Unknown
		//IL_1228: Unknown result type (might be due to invalid IL or missing references)
		//IL_1232: Expected O, but got Unknown
		//IL_12f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_12fe: Expected O, but got Unknown
		//IL_13b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ba: Expected O, but got Unknown
		//IL_1499: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a3: Expected O, but got Unknown
		//IL_15a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_15ab: Expected O, but got Unknown
		//IL_1690: Unknown result type (might be due to invalid IL or missing references)
		//IL_169a: Expected O, but got Unknown
		//IL_17e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_17eb: Expected O, but got Unknown
		//IL_186b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a48: Expected O, but got Unknown
		//IL_1ac8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ca5: Expected O, but got Unknown
		//IL_1d28: Unknown result type (might be due to invalid IL or missing references)
		//IL_20f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2103: Expected O, but got Unknown
		//IL_2132: Unknown result type (might be due to invalid IL or missing references)
		//IL_2281: Unknown result type (might be due to invalid IL or missing references)
		//IL_228b: Expected O, but got Unknown
		//IL_24cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_24d9: Expected O, but got Unknown
		//IL_255c: Unknown result type (might be due to invalid IL or missing references)
		//IL_299d: Unknown result type (might be due to invalid IL or missing references)
		//IL_29a7: Expected O, but got Unknown
		//IL_29da: Unknown result type (might be due to invalid IL or missing references)
		//IL_2bff: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c09: Expected O, but got Unknown
		//IL_2c29: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d26: Expected O, but got Unknown
		//IL_2e0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e16: Expected O, but got Unknown
		//IL_2e4a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f74: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f7e: Expected O, but got Unknown
		//IL_2faf: Unknown result type (might be due to invalid IL or missing references)
		//IL_30d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_30e3: Expected O, but got Unknown
		//IL_3111: Unknown result type (might be due to invalid IL or missing references)
		//IL_3204: Unknown result type (might be due to invalid IL or missing references)
		//IL_320e: Expected O, but got Unknown
		//IL_32c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_32ca: Expected O, but got Unknown
		//IL_3379: Unknown result type (might be due to invalid IL or missing references)
		//IL_3383: Expected O, but got Unknown
		//IL_34ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_34c4: Expected O, but got Unknown
		//IL_36fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_3705: Expected O, but got Unknown
		//IL_393c: Unknown result type (might be due to invalid IL or missing references)
		//IL_3946: Expected O, but got Unknown
		//IL_39c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_3b26: Unknown result type (might be due to invalid IL or missing references)
		//IL_3b30: Expected O, but got Unknown
		//IL_3b64: Unknown result type (might be due to invalid IL or missing references)
		//IL_3cdc: Unknown result type (might be due to invalid IL or missing references)
		//IL_3ce6: Expected O, but got Unknown
		//IL_3d69: Unknown result type (might be due to invalid IL or missing references)
		//IL_3ec1: Unknown result type (might be due to invalid IL or missing references)
		//IL_3ecb: Expected O, but got Unknown
		//IL_3f7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_3fb8: Unknown result type (might be due to invalid IL or missing references)
		//IL_410a: Unknown result type (might be due to invalid IL or missing references)
		//IL_4114: Expected O, but got Unknown
		//IL_4194: Unknown result type (might be due to invalid IL or missing references)
		//IL_42d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_42dc: Expected O, but got Unknown
		//IL_439e: Unknown result type (might be due to invalid IL or missing references)
		//IL_43a8: Expected O, but got Unknown
		//IL_43cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_4546: Unknown result type (might be due to invalid IL or missing references)
		//IL_4550: Expected O, but got Unknown
		//IL_45d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_4796: Unknown result type (might be due to invalid IL or missing references)
		//IL_47a0: Expected O, but got Unknown
		//IL_4820: Unknown result type (might be due to invalid IL or missing references)
		//IL_49e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_49f0: Expected O, but got Unknown
		//IL_4a73: Unknown result type (might be due to invalid IL or missing references)
		//IL_4bb1: Unknown result type (might be due to invalid IL or missing references)
		//IL_4bbb: Expected O, but got Unknown
		//IL_4d02: Unknown result type (might be due to invalid IL or missing references)
		//IL_4d0c: Expected O, but got Unknown
		//IL_4d8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_4f55: Unknown result type (might be due to invalid IL or missing references)
		//IL_4f5f: Expected O, but got Unknown
		//IL_4fdf: Unknown result type (might be due to invalid IL or missing references)
		//IL_51a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_51af: Expected O, but got Unknown
		//IL_5232: Unknown result type (might be due to invalid IL or missing references)
		//IL_53f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_5402: Expected O, but got Unknown
		//IL_5482: Unknown result type (might be due to invalid IL or missing references)
		//IL_5648: Unknown result type (might be due to invalid IL or missing references)
		//IL_5652: Expected O, but got Unknown
		//IL_56d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_581a: Unknown result type (might be due to invalid IL or missing references)
		//IL_5824: Expected O, but got Unknown
		//IL_584a: Unknown result type (might be due to invalid IL or missing references)
		//IL_59c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_59d3: Expected O, but got Unknown
		//IL_5b89: Unknown result type (might be due to invalid IL or missing references)
		//IL_5b93: Expected O, but got Unknown
		//IL_5bb6: Unknown result type (might be due to invalid IL or missing references)
		//IL_5cb0: Unknown result type (might be due to invalid IL or missing references)
		//IL_5cba: Expected O, but got Unknown
		//IL_5ce0: Unknown result type (might be due to invalid IL or missing references)
		//IL_5de1: Unknown result type (might be due to invalid IL or missing references)
		//IL_5deb: Expected O, but got Unknown
		//IL_5e0e: Unknown result type (might be due to invalid IL or missing references)
		//IL_5f08: Unknown result type (might be due to invalid IL or missing references)
		//IL_5f12: Expected O, but got Unknown
		//IL_5f35: Unknown result type (might be due to invalid IL or missing references)
		//IL_6036: Unknown result type (might be due to invalid IL or missing references)
		//IL_6040: Expected O, but got Unknown
		//IL_6060: Unknown result type (might be due to invalid IL or missing references)
		//IL_615a: Unknown result type (might be due to invalid IL or missing references)
		//IL_6164: Expected O, but got Unknown
		//IL_6187: Unknown result type (might be due to invalid IL or missing references)
		//IL_6288: Unknown result type (might be due to invalid IL or missing references)
		//IL_6292: Expected O, but got Unknown
		//IL_62b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_63a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_63ac: Expected O, but got Unknown
		//IL_645e: Unknown result type (might be due to invalid IL or missing references)
		//IL_6468: Expected O, but got Unknown
		//IL_6519: Unknown result type (might be due to invalid IL or missing references)
		//IL_6523: Expected O, but got Unknown
		//IL_65ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_65f6: Expected O, but got Unknown
		//IL_664a: Unknown result type (might be due to invalid IL or missing references)
		//IL_6685: Unknown result type (might be due to invalid IL or missing references)
		//IL_6759: Unknown result type (might be due to invalid IL or missing references)
		//IL_6763: Expected O, but got Unknown
		//IL_67ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_67f4: Expected O, but got Unknown
		//IL_6854: Unknown result type (might be due to invalid IL or missing references)
		//IL_685e: Expected O, but got Unknown
		//IL_688e: Unknown result type (might be due to invalid IL or missing references)
		//IL_697e: Unknown result type (might be due to invalid IL or missing references)
		//IL_6988: Expected O, but got Unknown
		//IL_6ac2: Unknown result type (might be due to invalid IL or missing references)
		//IL_6acc: Expected O, but got Unknown
		//IL_6b4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_6d15: Unknown result type (might be due to invalid IL or missing references)
		//IL_6d1f: Expected O, but got Unknown
		//IL_6da2: Unknown result type (might be due to invalid IL or missing references)
		//IL_6f07: Unknown result type (might be due to invalid IL or missing references)
		//IL_6f11: Expected O, but got Unknown
		//IL_6f62: Unknown result type (might be due to invalid IL or missing references)
		//IL_6f9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_7057: Unknown result type (might be due to invalid IL or missing references)
		//IL_7061: Expected O, but got Unknown
		//IL_7131: Unknown result type (might be due to invalid IL or missing references)
		//IL_713b: Expected O, but got Unknown
		//IL_7161: Unknown result type (might be due to invalid IL or missing references)
		//IL_7272: Unknown result type (might be due to invalid IL or missing references)
		//IL_727c: Expected O, but got Unknown
		//IL_72a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_73b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_73bd: Expected O, but got Unknown
		//IL_73e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_74cc: Unknown result type (might be due to invalid IL or missing references)
		btnView = new UIButton();
		btnUpgrade = new UIButton();
		btnReadReg = new UIButton();
		btnWriteReg = new UIButton();
		btnClear = new UIButton();
		btnVersion = new UIButton();
		uiLabel7 = new UILabel();
		uiLabel2 = new UILabel();
		uiLabel1 = new UILabel();
		uiLabel3 = new UILabel();
		uiProcessBar1 = new UIProcessBar();
		uiLine1 = new UILine();
		uiLabel4 = new UILabel();
		btnHand = new UIButton();
		btnClearData = new UIButton();
		btnStepUpgate = new UIButton();
		uiPanel1 = new UIPanel();
		btnResetDevice = new UIButton();
		btnVersionAll = new UIButton();
		panelArgu = new UIPanel();
		panel1 = new Panel();
		txtName = new UITextBox();
		uiLabel15 = new UILabel();
		txtlA = new UITextBox();
		txthA = new UITextBox();
		txtOver = new UITextBox();
		uiLabel14 = new UILabel();
		uiLabel13 = new UILabel();
		uiLabel12 = new UILabel();
		btnCreate = new UIButton();
		btnUpdateDebug = new UIButton();
		btnForChange = new UIButton();
		txtTime = new UITextBox();
		btnWriteSN = new UIButton();
		ubxImageType = new UIComboBox();
		btnReadSN = new UIButton();
		uiLabel10 = new UILabel();
		txtSN = new UITextBox();
		btnWhite = new UIButton();
		btnBlue = new UIButton();
		btnStop = new UIButton();
		uiLabel6 = new UILabel();
		btnStopReg = new UIButton();
		btnAutoReg = new UIButton();
		btnAuto = new UIButton();
		btnGreen = new UIButton();
		btnRed = new UIButton();
		txtD4 = new UITextBox();
		uiButton1 = new UIButton();
		txtA4 = new UITextBox();
		txtD3 = new UITextBox();
		txtA3 = new UITextBox();
		txtD2 = new UITextBox();
		txtA2 = new UITextBox();
		txtD1 = new UITextBox();
		txtA1 = new UITextBox();
		uiLabel9 = new UILabel();
		uiLabel11 = new UILabel();
		uiLabel8 = new UILabel();
		cbxRegType = new UIComboBox();
		txtLog1 = new TextBox();
		txtID = new UITextBox();
		lblID = new UILabel();
		btnAllScreen = new UIButton();
		btnSpem = new UIButton();
		cbxUpgradeType = new UIComboBox();
		uiLabel5 = new UILabel();
		txtRegData = new UITextBox();
		txtRegAddr = new UITextBox();
		txtPath = new UITextBox();
		((Control)uiPanel1).SuspendLayout();
		((Control)panelArgu).SuspendLayout();
		((Control)panel1).SuspendLayout();
		((Control)this).SuspendLayout();
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
		((Control)btnView).Location = new Point(798, 28);
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
		((Control)btnView).TabIndex = 21;
		((Control)btnView).Text = "浏览";
		((Control)btnView).Click += btnView_Click;
		((Control)btnUpgrade).BackColor = Color.Transparent;
		((Control)btnUpgrade).Cursor = Cursors.Hand;
		btnUpgrade.FillColor = Color.FromArgb(15, 40, 70);
		btnUpgrade.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnUpgrade.FillPressColor = Color.FromArgb(235, 243, 255);
		btnUpgrade.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnUpgrade).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnUpgrade.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnUpgrade.ForePressColor = Color.FromArgb(130, 130, 130);
		btnUpgrade.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnUpgrade).Location = new Point(427, 632);
		((Control)btnUpgrade).Margin = new Padding(2);
		((Control)btnUpgrade).MinimumSize = new Size(1, 1);
		((Control)btnUpgrade).Name = "btnUpgrade";
		btnUpgrade.Radius = 25;
		btnUpgrade.RectColor = Color.FromArgb(130, 130, 130);
		btnUpgrade.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnUpgrade.RectPressColor = Color.FromArgb(130, 130, 130);
		btnUpgrade.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnUpgrade).Size = new Size(120, 29);
		btnUpgrade.Style = UIStyle.Black;
		((Control)btnUpgrade).TabIndex = 21;
		((Control)btnUpgrade).Text = " 融合升级";
		((Control)btnUpgrade).Click += btnUpgrade_Click;
		((Control)btnReadReg).BackColor = Color.Transparent;
		((Control)btnReadReg).Cursor = Cursors.Hand;
		btnReadReg.FillColor = Color.FromArgb(15, 40, 70);
		btnReadReg.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnReadReg.FillPressColor = Color.FromArgb(235, 243, 255);
		btnReadReg.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnReadReg).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnReadReg.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnReadReg.ForePressColor = Color.FromArgb(130, 130, 130);
		btnReadReg.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReadReg).Location = new Point(706, 679);
		((Control)btnReadReg).Margin = new Padding(2);
		((Control)btnReadReg).MinimumSize = new Size(1, 1);
		((Control)btnReadReg).Name = "btnReadReg";
		btnReadReg.Radius = 25;
		btnReadReg.RectColor = Color.FromArgb(130, 130, 130);
		btnReadReg.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnReadReg.RectPressColor = Color.FromArgb(130, 130, 130);
		btnReadReg.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReadReg).Size = new Size(74, 29);
		btnReadReg.Style = UIStyle.Black;
		((Control)btnReadReg).TabIndex = 21;
		((Control)btnReadReg).Text = "读取";
		((Control)btnReadReg).Visible = false;
		((Control)btnReadReg).Click += btnReadReg_Click;
		((Control)btnWriteReg).BackColor = Color.Transparent;
		((Control)btnWriteReg).Cursor = Cursors.Hand;
		btnWriteReg.FillColor = Color.FromArgb(15, 40, 70);
		btnWriteReg.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnWriteReg.FillPressColor = Color.FromArgb(235, 243, 255);
		btnWriteReg.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnWriteReg).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnWriteReg.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnWriteReg.ForePressColor = Color.FromArgb(130, 130, 130);
		btnWriteReg.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWriteReg).Location = new Point(784, 679);
		((Control)btnWriteReg).Margin = new Padding(2);
		((Control)btnWriteReg).MinimumSize = new Size(1, 1);
		((Control)btnWriteReg).Name = "btnWriteReg";
		btnWriteReg.Radius = 25;
		btnWriteReg.RectColor = Color.FromArgb(130, 130, 130);
		btnWriteReg.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnWriteReg.RectPressColor = Color.FromArgb(130, 130, 130);
		btnWriteReg.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWriteReg).Size = new Size(74, 29);
		btnWriteReg.Style = UIStyle.Black;
		((Control)btnWriteReg).TabIndex = 21;
		((Control)btnWriteReg).Text = "写入";
		((Control)btnWriteReg).Visible = false;
		((Control)btnWriteReg).Click += btnWriteReg_Click;
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
		((Control)btnClear).Location = new Point(784, 632);
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
		((Control)btnClear).TabIndex = 21;
		((Control)btnClear).Text = "清除";
		((Control)btnClear).Click += btnClear_Click;
		((Control)btnVersion).BackColor = Color.Transparent;
		((Control)btnVersion).Cursor = Cursors.Hand;
		btnVersion.FillColor = Color.FromArgb(15, 40, 70);
		btnVersion.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnVersion.FillPressColor = Color.FromArgb(235, 243, 255);
		btnVersion.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnVersion).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnVersion.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnVersion.ForePressColor = Color.FromArgb(130, 130, 130);
		btnVersion.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnVersion).Location = new Point(129, 630);
		((Control)btnVersion).Margin = new Padding(2);
		((Control)btnVersion).MinimumSize = new Size(1, 1);
		((Control)btnVersion).Name = "btnVersion";
		btnVersion.Radius = 25;
		btnVersion.RectColor = Color.FromArgb(130, 130, 130);
		btnVersion.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnVersion.RectPressColor = Color.FromArgb(130, 130, 130);
		btnVersion.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnVersion).Size = new Size(74, 29);
		btnVersion.Style = UIStyle.Black;
		((Control)btnVersion).TabIndex = 21;
		((Control)btnVersion).Text = "单机版本";
		((Control)btnVersion).Visible = false;
		((Control)btnVersion).Click += btnVersion_Click;
		((Control)uiLabel7).BackColor = Color.Transparent;
		((Control)uiLabel7).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel7).ForeColor = Color.Silver;
		((Control)uiLabel7).Location = new Point(369, 30);
		((Control)uiLabel7).Name = "uiLabel7";
		((Control)uiLabel7).Size = new Size(84, 29);
		uiLabel7.Style = UIStyle.Black;
		((Control)uiLabel7).TabIndex = 22;
		((Control)uiLabel7).Text = "文件路径:";
		((Label)uiLabel7).TextAlign = (ContentAlignment)32;
		((Control)uiLabel2).BackColor = Color.Transparent;
		((Control)uiLabel2).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel2).ForeColor = Color.Silver;
		((Control)uiLabel2).Location = new Point(226, 679);
		((Control)uiLabel2).Name = "uiLabel2";
		((Control)uiLabel2).Size = new Size(100, 29);
		uiLabel2.Style = UIStyle.Black;
		((Control)uiLabel2).TabIndex = 22;
		((Control)uiLabel2).Text = "寄存器地址:";
		((Label)uiLabel2).TextAlign = (ContentAlignment)32;
		((Control)uiLabel2).Visible = false;
		((Control)uiLabel1).BackColor = Color.Transparent;
		((Control)uiLabel1).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel1).ForeColor = Color.Silver;
		((Control)uiLabel1).Location = new Point(18, 583);
		((Control)uiLabel1).Name = "uiLabel1";
		((Control)uiLabel1).Size = new Size(94, 29);
		uiLabel1.Style = UIStyle.Black;
		((Control)uiLabel1).TabIndex = 22;
		((Control)uiLabel1).Text = "升级进度:";
		((Label)uiLabel1).TextAlign = (ContentAlignment)16;
		((Control)uiLabel3).BackColor = Color.Transparent;
		((Control)uiLabel3).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel3).ForeColor = Color.Silver;
		((Control)uiLabel3).Location = new Point(467, 679);
		((Control)uiLabel3).Name = "uiLabel3";
		((Control)uiLabel3).Size = new Size(102, 29);
		uiLabel3.Style = UIStyle.Black;
		((Control)uiLabel3).TabIndex = 22;
		((Control)uiLabel3).Text = "寄存器数据:";
		((Label)uiLabel3).TextAlign = (ContentAlignment)32;
		((Control)uiLabel3).Visible = false;
		((Control)uiProcessBar1).BackColor = Color.Transparent;
		uiProcessBar1.DecimalCount = 1;
		uiProcessBar1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiProcessBar1).Font = new Font("微软雅黑", 12f);
		((Control)uiProcessBar1).ForeColor = Color.FromArgb(230, 230, 232);
		((Control)uiProcessBar1).Location = new Point(118, 583);
		((Control)uiProcessBar1).MinimumSize = new Size(70, 5);
		((Control)uiProcessBar1).Name = "uiProcessBar1";
		uiProcessBar1.Radius = 25;
		uiProcessBar1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiProcessBar1).Size = new Size(740, 29);
		uiProcessBar1.Style = UIStyle.Black;
		((Control)uiProcessBar1).TabIndex = 24;
		((Control)uiProcessBar1).Text = "0.0%";
		uiLine1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine1).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine1).ForeColor = Color.Silver;
		uiLine1.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine1).Location = new Point(22, 62);
		((Control)uiLine1).MinimumSize = new Size(2, 2);
		((Control)uiLine1).Name = "uiLine1";
		((Control)uiLine1).Size = new Size(836, 29);
		uiLine1.Style = UIStyle.Black;
		((Control)uiLine1).TabIndex = 25;
		((Control)uiLine1).Text = "日志信息";
		uiLine1.TextAlign = (ContentAlignment)16;
		((Control)uiLabel4).BackColor = Color.Transparent;
		((Control)uiLabel4).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel4).ForeColor = Color.Silver;
		((Control)uiLabel4).Location = new Point(-6, 679);
		((Control)uiLabel4).Name = "uiLabel4";
		((Control)uiLabel4).Size = new Size(111, 29);
		uiLabel4.Style = UIStyle.Black;
		((Control)uiLabel4).TabIndex = 27;
		((Control)uiLabel4).Text = "寄存器类型:";
		((Label)uiLabel4).TextAlign = (ContentAlignment)32;
		((Control)uiLabel4).Visible = false;
		((Control)btnHand).BackColor = Color.Transparent;
		((Control)btnHand).Cursor = Cursors.Hand;
		btnHand.FillColor = Color.FromArgb(15, 40, 70);
		btnHand.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnHand.FillPressColor = Color.FromArgb(235, 243, 255);
		btnHand.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnHand).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnHand.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnHand.ForePressColor = Color.FromArgb(130, 130, 130);
		btnHand.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnHand).Location = new Point(14, 496);
		((Control)btnHand).Margin = new Padding(2);
		((Control)btnHand).MinimumSize = new Size(1, 1);
		((Control)btnHand).Name = "btnHand";
		btnHand.Radius = 25;
		btnHand.RectColor = Color.FromArgb(130, 130, 130);
		btnHand.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnHand.RectPressColor = Color.FromArgb(130, 130, 130);
		btnHand.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnHand).Size = new Size(90, 29);
		btnHand.Style = UIStyle.Black;
		((Control)btnHand).TabIndex = 32;
		((Control)btnHand).Text = "握手";
		((Control)btnHand).Visible = false;
		((Control)btnHand).Click += btnHand_Click;
		((Control)btnClearData).BackColor = Color.Transparent;
		((Control)btnClearData).Cursor = Cursors.Hand;
		btnClearData.FillColor = Color.FromArgb(15, 40, 70);
		btnClearData.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnClearData.FillPressColor = Color.FromArgb(235, 243, 255);
		btnClearData.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnClearData).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnClearData.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnClearData.ForePressColor = Color.FromArgb(130, 130, 130);
		btnClearData.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnClearData).Location = new Point(108, 496);
		((Control)btnClearData).Margin = new Padding(2);
		((Control)btnClearData).MinimumSize = new Size(1, 1);
		((Control)btnClearData).Name = "btnClearData";
		btnClearData.Radius = 25;
		btnClearData.RectColor = Color.FromArgb(130, 130, 130);
		btnClearData.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnClearData.RectPressColor = Color.FromArgb(130, 130, 130);
		btnClearData.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnClearData).Size = new Size(74, 29);
		btnClearData.Style = UIStyle.Black;
		((Control)btnClearData).TabIndex = 33;
		((Control)btnClearData).Text = "擦除";
		((Control)btnClearData).Visible = false;
		((Control)btnClearData).Click += btnClearData_Click;
		((Control)btnStepUpgate).BackColor = Color.Transparent;
		((Control)btnStepUpgate).Cursor = Cursors.Hand;
		btnStepUpgate.FillColor = Color.FromArgb(15, 40, 70);
		btnStepUpgate.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnStepUpgate.FillPressColor = Color.FromArgb(235, 243, 255);
		btnStepUpgate.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnStepUpgate).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnStepUpgate.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnStepUpgate.ForePressColor = Color.FromArgb(130, 130, 130);
		btnStepUpgate.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStepUpgate).Location = new Point(186, 496);
		((Control)btnStepUpgate).Margin = new Padding(2);
		((Control)btnStepUpgate).MinimumSize = new Size(1, 1);
		((Control)btnStepUpgate).Name = "btnStepUpgate";
		btnStepUpgate.Radius = 25;
		btnStepUpgate.RectColor = Color.FromArgb(130, 130, 130);
		btnStepUpgate.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnStepUpgate.RectPressColor = Color.FromArgb(130, 130, 130);
		btnStepUpgate.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStepUpgate).Size = new Size(74, 29);
		btnStepUpgate.Style = UIStyle.Black;
		((Control)btnStepUpgate).TabIndex = 34;
		((Control)btnStepUpgate).Text = "升级";
		((Control)btnStepUpgate).Visible = false;
		((Control)btnStepUpgate).Click += btnStepUpgate_Click;
		((ScrollableControl)uiPanel1).AutoScroll = true;
		((Control)uiPanel1).Controls.Add((Control)(object)btnResetDevice);
		((Control)uiPanel1).Controls.Add((Control)(object)btnVersionAll);
		((Control)uiPanel1).Controls.Add((Control)(object)panelArgu);
		((Control)uiPanel1).Controls.Add((Control)(object)cbxRegType);
		((Control)uiPanel1).Controls.Add((Control)(object)txtLog1);
		((Control)uiPanel1).Controls.Add((Control)(object)txtID);
		((Control)uiPanel1).Controls.Add((Control)(object)lblID);
		((Control)uiPanel1).Controls.Add((Control)(object)btnAllScreen);
		((Control)uiPanel1).Controls.Add((Control)(object)btnSpem);
		((Control)uiPanel1).Controls.Add((Control)(object)cbxUpgradeType);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel5);
		((Control)uiPanel1).Controls.Add((Control)(object)txtRegData);
		((Control)uiPanel1).Controls.Add((Control)(object)txtRegAddr);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLine1);
		((Control)uiPanel1).Controls.Add((Control)(object)uiProcessBar1);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel3);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel1);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel2);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel7);
		((Control)uiPanel1).Controls.Add((Control)(object)btnVersion);
		((Control)uiPanel1).Controls.Add((Control)(object)btnClear);
		((Control)uiPanel1).Controls.Add((Control)(object)btnWriteReg);
		((Control)uiPanel1).Controls.Add((Control)(object)btnReadReg);
		((Control)uiPanel1).Controls.Add((Control)(object)btnUpgrade);
		((Control)uiPanel1).Controls.Add((Control)(object)btnView);
		((Control)uiPanel1).Controls.Add((Control)(object)txtPath);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel4);
		((Control)uiPanel1).Dock = (DockStyle)5;
		uiPanel1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel1).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel1).ForeColor = Color.Silver;
		((Control)uiPanel1).Location = new Point(0, 0);
		((Control)uiPanel1).Margin = new Padding(5, 6, 5, 6);
		((Control)uiPanel1).MinimumSize = new Size(1, 1);
		((Control)uiPanel1).Name = "uiPanel1";
		uiPanel1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel1).Size = new Size(1276, 832);
		uiPanel1.Style = UIStyle.Black;
		((Control)uiPanel1).TabIndex = 0;
		((Control)uiPanel1).Text = null;
		uiPanel1.TextAlignment = (ContentAlignment)32;
		((Control)btnResetDevice).BackColor = Color.Transparent;
		((Control)btnResetDevice).Cursor = Cursors.Hand;
		btnResetDevice.FillColor = Color.FromArgb(15, 40, 70);
		btnResetDevice.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnResetDevice.FillPressColor = Color.FromArgb(235, 243, 255);
		btnResetDevice.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnResetDevice).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnResetDevice.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnResetDevice.ForePressColor = Color.FromArgb(130, 130, 130);
		btnResetDevice.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnResetDevice).Location = new Point(214, 630);
		((Control)btnResetDevice).MinimumSize = new Size(1, 1);
		((Control)btnResetDevice).Name = "btnResetDevice";
		btnResetDevice.Radius = 25;
		btnResetDevice.RectColor = Color.FromArgb(130, 130, 130);
		btnResetDevice.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnResetDevice.RectPressColor = Color.FromArgb(130, 130, 130);
		btnResetDevice.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnResetDevice).Size = new Size(114, 31);
		btnResetDevice.Style = UIStyle.Black;
		((Control)btnResetDevice).TabIndex = 90;
		((Control)btnResetDevice).Text = "恢复出厂设置";
		((Control)btnResetDevice).Visible = false;
		((Control)btnResetDevice).Click += btnResetDevice_Click;
		((Control)btnVersionAll).BackColor = Color.Transparent;
		((Control)btnVersionAll).Cursor = Cursors.Hand;
		btnVersionAll.FillColor = Color.FromArgb(15, 40, 70);
		btnVersionAll.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnVersionAll.FillPressColor = Color.FromArgb(235, 243, 255);
		btnVersionAll.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnVersionAll).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnVersionAll.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnVersionAll.ForePressColor = Color.FromArgb(130, 130, 130);
		btnVersionAll.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnVersionAll).Location = new Point(551, 632);
		((Control)btnVersionAll).Margin = new Padding(2);
		((Control)btnVersionAll).MinimumSize = new Size(1, 1);
		((Control)btnVersionAll).Name = "btnVersionAll";
		btnVersionAll.Radius = 25;
		btnVersionAll.RectColor = Color.FromArgb(130, 130, 130);
		btnVersionAll.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnVersionAll.RectPressColor = Color.FromArgb(130, 130, 130);
		btnVersionAll.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnVersionAll).Size = new Size(106, 29);
		btnVersionAll.Style = UIStyle.Black;
		((Control)btnVersionAll).TabIndex = 89;
		((Control)btnVersionAll).Text = "联屏版本";
		((Control)btnVersionAll).Click += btnVersionAll_Click;
		((Control)panelArgu).Controls.Add((Control)(object)panel1);
		((Control)panelArgu).Controls.Add((Control)(object)btnUpdateDebug);
		((Control)panelArgu).Controls.Add((Control)(object)btnForChange);
		((Control)panelArgu).Controls.Add((Control)(object)btnStepUpgate);
		((Control)panelArgu).Controls.Add((Control)(object)txtTime);
		((Control)panelArgu).Controls.Add((Control)(object)btnWriteSN);
		((Control)panelArgu).Controls.Add((Control)(object)ubxImageType);
		((Control)panelArgu).Controls.Add((Control)(object)btnReadSN);
		((Control)panelArgu).Controls.Add((Control)(object)uiLabel10);
		((Control)panelArgu).Controls.Add((Control)(object)txtSN);
		((Control)panelArgu).Controls.Add((Control)(object)btnWhite);
		((Control)panelArgu).Controls.Add((Control)(object)btnBlue);
		((Control)panelArgu).Controls.Add((Control)(object)btnStop);
		((Control)panelArgu).Controls.Add((Control)(object)btnClearData);
		((Control)panelArgu).Controls.Add((Control)(object)uiLabel6);
		((Control)panelArgu).Controls.Add((Control)(object)btnStopReg);
		((Control)panelArgu).Controls.Add((Control)(object)btnHand);
		((Control)panelArgu).Controls.Add((Control)(object)btnAutoReg);
		((Control)panelArgu).Controls.Add((Control)(object)btnAuto);
		((Control)panelArgu).Controls.Add((Control)(object)btnGreen);
		((Control)panelArgu).Controls.Add((Control)(object)btnRed);
		((Control)panelArgu).Controls.Add((Control)(object)txtD4);
		((Control)panelArgu).Controls.Add((Control)(object)uiButton1);
		((Control)panelArgu).Controls.Add((Control)(object)txtA4);
		((Control)panelArgu).Controls.Add((Control)(object)txtD3);
		((Control)panelArgu).Controls.Add((Control)(object)txtA3);
		((Control)panelArgu).Controls.Add((Control)(object)txtD2);
		((Control)panelArgu).Controls.Add((Control)(object)txtA2);
		((Control)panelArgu).Controls.Add((Control)(object)txtD1);
		((Control)panelArgu).Controls.Add((Control)(object)txtA1);
		((Control)panelArgu).Controls.Add((Control)(object)uiLabel9);
		((Control)panelArgu).Controls.Add((Control)(object)uiLabel11);
		((Control)panelArgu).Controls.Add((Control)(object)uiLabel8);
		((Control)panelArgu).Dock = (DockStyle)4;
		panelArgu.FillColor = Color.FromArgb(24, 24, 24);
		((Control)panelArgu).Font = new Font("微软雅黑", 12f);
		((Control)panelArgu).ForeColor = Color.Silver;
		((Control)panelArgu).Location = new Point(996, 0);
		((Control)panelArgu).Margin = new Padding(4, 5, 4, 5);
		((Control)panelArgu).MinimumSize = new Size(1, 1);
		((Control)panelArgu).Name = "panelArgu";
		panelArgu.RectColor = Color.FromArgb(130, 130, 130);
		((Control)panelArgu).Size = new Size(280, 832);
		panelArgu.Style = UIStyle.Black;
		((Control)panelArgu).TabIndex = 88;
		((Control)panelArgu).Text = null;
		panelArgu.TextAlignment = (ContentAlignment)32;
		((Control)panelArgu).Visible = false;
		((Control)panel1).BackColor = SystemColors.ActiveCaptionText;
		((Control)panel1).Controls.Add((Control)(object)txtName);
		((Control)panel1).Controls.Add((Control)(object)uiLabel15);
		((Control)panel1).Controls.Add((Control)(object)txtlA);
		((Control)panel1).Controls.Add((Control)(object)txthA);
		((Control)panel1).Controls.Add((Control)(object)txtOver);
		((Control)panel1).Controls.Add((Control)(object)uiLabel14);
		((Control)panel1).Controls.Add((Control)(object)uiLabel13);
		((Control)panel1).Controls.Add((Control)(object)uiLabel12);
		((Control)panel1).Controls.Add((Control)(object)btnCreate);
		((Control)panel1).Location = new Point(6, 13);
		((Control)panel1).Name = "panel1";
		((Control)panel1).Size = new Size(267, 268);
		((Control)panel1).TabIndex = 111;
		((Control)txtName).BackColor = Color.Transparent;
		((Control)txtName).Cursor = Cursors.IBeam;
		txtName.FillColor = Color.White;
		((Control)txtName).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtName).Location = new Point(114, 15);
		((Control)txtName).Margin = new Padding(4, 5, 4, 5);
		txtName.Maximum = 2147483647.0;
		txtName.Minimum = -2147483648.0;
		((Control)txtName).MinimumSize = new Size(1, 1);
		((Control)txtName).Name = "txtName";
		txtName.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtName).Size = new Size(137, 29);
		txtName.Style = UIStyle.Black;
		((Control)txtName).TabIndex = 93;
		((Control)txtName).Text = "top";
		txtName.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel15).BackColor = Color.Transparent;
		((Control)uiLabel15).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel15).ForeColor = Color.Silver;
		((Control)uiLabel15).Location = new Point(15, 15);
		((Control)uiLabel15).Name = "uiLabel15";
		((Control)uiLabel15).Size = new Size(100, 29);
		uiLabel15.Style = UIStyle.Black;
		((Control)uiLabel15).TabIndex = 92;
		((Control)uiLabel15).Text = "name：";
		((Label)uiLabel15).TextAlign = (ContentAlignment)32;
		((Control)txtlA).BackColor = Color.Transparent;
		((Control)txtlA).Cursor = Cursors.IBeam;
		txtlA.DoubleValue = 255.0;
		txtlA.FillColor = Color.White;
		((Control)txtlA).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		txtlA.IntValue = 255;
		((Control)txtlA).Location = new Point(114, 135);
		((Control)txtlA).Margin = new Padding(4, 5, 4, 5);
		txtlA.Maximum = 2147483647.0;
		txtlA.Minimum = -2147483648.0;
		((Control)txtlA).MinimumSize = new Size(1, 1);
		((Control)txtlA).Name = "txtlA";
		txtlA.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtlA).Size = new Size(137, 29);
		txtlA.Style = UIStyle.Black;
		((Control)txtlA).TabIndex = 28;
		((Control)txtlA).Text = "255";
		txtlA.TextAlignment = (ContentAlignment)16;
		((Control)txthA).BackColor = Color.Transparent;
		((Control)txthA).Cursor = Cursors.IBeam;
		txthA.DoubleValue = 150.0;
		txthA.FillColor = Color.White;
		((Control)txthA).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		txthA.IntValue = 150;
		((Control)txthA).Location = new Point(114, 96);
		((Control)txthA).Margin = new Padding(4, 5, 4, 5);
		txthA.Maximum = 2147483647.0;
		txthA.Minimum = -2147483648.0;
		((Control)txthA).MinimumSize = new Size(1, 1);
		((Control)txthA).Name = "txthA";
		txthA.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txthA).Size = new Size(137, 29);
		txthA.Style = UIStyle.Black;
		((Control)txthA).TabIndex = 28;
		((Control)txthA).Text = "150";
		txthA.TextAlignment = (ContentAlignment)16;
		((Control)txtOver).BackColor = Color.Transparent;
		((Control)txtOver).Cursor = Cursors.IBeam;
		txtOver.DoubleValue = 80.0;
		txtOver.FillColor = Color.White;
		((Control)txtOver).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		txtOver.IntValue = 80;
		((Control)txtOver).Location = new Point(114, 55);
		((Control)txtOver).Margin = new Padding(4, 5, 4, 5);
		txtOver.Maximum = 2147483647.0;
		txtOver.Minimum = -2147483648.0;
		((Control)txtOver).MinimumSize = new Size(1, 1);
		((Control)txtOver).Name = "txtOver";
		txtOver.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtOver).Size = new Size(137, 29);
		txtOver.Style = UIStyle.Black;
		((Control)txtOver).TabIndex = 28;
		((Control)txtOver).Text = "80";
		txtOver.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel14).BackColor = Color.Transparent;
		((Control)uiLabel14).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel14).ForeColor = Color.Silver;
		((Control)uiLabel14).Location = new Point(15, 135);
		((Control)uiLabel14).Name = "uiLabel14";
		((Control)uiLabel14).Size = new Size(100, 29);
		uiLabel14.Style = UIStyle.Black;
		((Control)uiLabel14).TabIndex = 27;
		((Control)uiLabel14).Text = "lAlhpa：";
		((Label)uiLabel14).TextAlign = (ContentAlignment)32;
		((Control)uiLabel13).BackColor = Color.Transparent;
		((Control)uiLabel13).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel13).ForeColor = Color.Silver;
		((Control)uiLabel13).Location = new Point(15, 96);
		((Control)uiLabel13).Name = "uiLabel13";
		((Control)uiLabel13).Size = new Size(100, 29);
		uiLabel13.Style = UIStyle.Black;
		((Control)uiLabel13).TabIndex = 27;
		((Control)uiLabel13).Text = "hAlhpa：";
		((Label)uiLabel13).TextAlign = (ContentAlignment)32;
		((Control)uiLabel12).BackColor = Color.Transparent;
		((Control)uiLabel12).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel12).ForeColor = Color.Silver;
		((Control)uiLabel12).Location = new Point(15, 55);
		((Control)uiLabel12).Name = "uiLabel12";
		((Control)uiLabel12).Size = new Size(100, 29);
		uiLabel12.Style = UIStyle.Black;
		((Control)uiLabel12).TabIndex = 27;
		((Control)uiLabel12).Text = "overlap：";
		((Label)uiLabel12).TextAlign = (ContentAlignment)32;
		((Control)btnCreate).BackColor = Color.Transparent;
		((Control)btnCreate).Cursor = Cursors.Hand;
		btnCreate.FillColor = Color.FromArgb(15, 40, 70);
		btnCreate.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnCreate.FillPressColor = Color.FromArgb(235, 243, 255);
		btnCreate.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnCreate).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnCreate.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnCreate.ForePressColor = Color.FromArgb(130, 130, 130);
		btnCreate.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnCreate).Location = new Point(102, 181);
		((Control)btnCreate).MinimumSize = new Size(1, 1);
		((Control)btnCreate).Name = "btnCreate";
		btnCreate.Radius = 25;
		btnCreate.RectColor = Color.FromArgb(130, 130, 130);
		btnCreate.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnCreate.RectPressColor = Color.FromArgb(130, 130, 130);
		btnCreate.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnCreate).Size = new Size(148, 31);
		btnCreate.Style = UIStyle.Black;
		((Control)btnCreate).TabIndex = 91;
		((Control)btnCreate).Text = "生成上层整合数据";
		((Control)btnCreate).Click += btnCreate_Click;
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
		((Control)btnUpdateDebug).Location = new Point(14, 580);
		((Control)btnUpdateDebug).MinimumSize = new Size(1, 1);
		((Control)btnUpdateDebug).Name = "btnUpdateDebug";
		btnUpdateDebug.Radius = 25;
		btnUpdateDebug.RectColor = Color.FromArgb(130, 130, 130);
		btnUpdateDebug.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnUpdateDebug.RectPressColor = Color.FromArgb(130, 130, 130);
		btnUpdateDebug.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnUpdateDebug).Size = new Size(131, 31);
		btnUpdateDebug.Style = UIStyle.Black;
		((Control)btnUpdateDebug).TabIndex = 110;
		((Control)btnUpdateDebug).Text = "修复图像溢出";
		((Control)btnUpdateDebug).Click += btnUpdateDebug_Click;
		((Control)btnForChange).BackColor = Color.Transparent;
		((Control)btnForChange).Cursor = Cursors.Hand;
		btnForChange.FillColor = Color.FromArgb(15, 40, 70);
		btnForChange.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnForChange.FillPressColor = Color.FromArgb(235, 243, 255);
		btnForChange.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnForChange).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnForChange.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnForChange.ForePressColor = Color.FromArgb(130, 130, 130);
		btnForChange.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnForChange).Location = new Point(186, 579);
		((Control)btnForChange).Margin = new Padding(2);
		((Control)btnForChange).MinimumSize = new Size(1, 1);
		((Control)btnForChange).Name = "btnForChange";
		btnForChange.Radius = 25;
		btnForChange.RectColor = Color.FromArgb(130, 130, 130);
		btnForChange.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnForChange.RectPressColor = Color.FromArgb(130, 130, 130);
		btnForChange.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnForChange).Size = new Size(74, 29);
		btnForChange.Style = UIStyle.Black;
		((Control)btnForChange).TabIndex = 109;
		((Control)btnForChange).Text = "循环切换";
		((Control)btnForChange).Click += btnForChange_Click;
		((Control)txtTime).Cursor = Cursors.IBeam;
		txtTime.DoubleValue = 3000.0;
		txtTime.FillColor = Color.White;
		((Control)txtTime).Font = new Font("微软雅黑", 12f);
		txtTime.IntValue = 3000;
		((Control)txtTime).Location = new Point(55, 301);
		((Control)txtTime).Margin = new Padding(4, 5, 4, 5);
		txtTime.Maximum = 2147483647.0;
		txtTime.Minimum = -2147483648.0;
		((Control)txtTime).MinimumSize = new Size(1, 1);
		((Control)txtTime).Name = "txtTime";
		txtTime.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtTime).Size = new Size(59, 34);
		txtTime.Style = UIStyle.Black;
		((Control)txtTime).TabIndex = 108;
		((Control)txtTime).Text = "3000";
		txtTime.TextAlignment = (ContentAlignment)16;
		((Control)btnWriteSN).BackColor = Color.Transparent;
		((Control)btnWriteSN).Cursor = Cursors.Hand;
		btnWriteSN.FillColor = Color.FromArgb(15, 40, 70);
		btnWriteSN.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnWriteSN.FillPressColor = Color.FromArgb(235, 243, 255);
		btnWriteSN.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnWriteSN).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnWriteSN.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnWriteSN.ForePressColor = Color.FromArgb(130, 130, 130);
		btnWriteSN.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWriteSN).Location = new Point(182, 406);
		((Control)btnWriteSN).Margin = new Padding(2);
		((Control)btnWriteSN).MinimumSize = new Size(1, 1);
		((Control)btnWriteSN).Name = "btnWriteSN";
		btnWriteSN.Radius = 25;
		btnWriteSN.RectColor = Color.FromArgb(130, 130, 130);
		btnWriteSN.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnWriteSN.RectPressColor = Color.FromArgb(130, 130, 130);
		btnWriteSN.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWriteSN).Size = new Size(74, 29);
		btnWriteSN.Style = UIStyle.Black;
		((Control)btnWriteSN).TabIndex = 106;
		((Control)btnWriteSN).Text = "写入";
		((Control)btnWriteSN).Click += btnWriteSN_Click;
		ubxImageType.DataSource = null;
		ubxImageType.DropDownStyle = UIDropDownStyle.DropDownList;
		ubxImageType.FillColor = Color.White;
		((Control)ubxImageType).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		ubxImageType.Items.AddRange(new object[14]
		{
			"2：同步视频", "3：棋盘格 ", "4：渐变色  ", "5：竖彩条 ", "6：黑色", "7：白色 ", "8：红色  ", "9：绿色    ", "10：蓝色", "11：青色  ",
			"12：紫色 ", "13：黄色  ", "14：v灰阶", "15：h灰阶"
		});
		((Control)ubxImageType).Location = new Point(108, 543);
		((Control)ubxImageType).Margin = new Padding(4, 5, 4, 5);
		((Control)ubxImageType).MinimumSize = new Size(63, 0);
		((Control)ubxImageType).Name = "ubxImageType";
		((Control)ubxImageType).Padding = new Padding(0, 0, 30, 2);
		ubxImageType.RectColor = Color.FromArgb(130, 130, 130);
		((Control)ubxImageType).Size = new Size(152, 29);
		ubxImageType.Style = UIStyle.Black;
		((Control)ubxImageType).TabIndex = 38;
		((Control)ubxImageType).Text = "2：同步视频";
		ubxImageType.TextAlignment = (ContentAlignment)16;
		((Control)ubxImageType).Visible = false;
		ubxImageType.SelectedIndexChanged += ubxImageType_SelectedIndexChanged;
		((Control)btnReadSN).BackColor = Color.Transparent;
		((Control)btnReadSN).Cursor = Cursors.Hand;
		btnReadSN.FillColor = Color.FromArgb(15, 40, 70);
		btnReadSN.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnReadSN.FillPressColor = Color.FromArgb(235, 243, 255);
		btnReadSN.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnReadSN).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnReadSN.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnReadSN.ForePressColor = Color.FromArgb(130, 130, 130);
		btnReadSN.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReadSN).Location = new Point(104, 406);
		((Control)btnReadSN).Margin = new Padding(2);
		((Control)btnReadSN).MinimumSize = new Size(1, 1);
		((Control)btnReadSN).Name = "btnReadSN";
		btnReadSN.Radius = 25;
		btnReadSN.RectColor = Color.FromArgb(130, 130, 130);
		btnReadSN.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnReadSN.RectPressColor = Color.FromArgb(130, 130, 130);
		btnReadSN.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReadSN).Size = new Size(74, 29);
		btnReadSN.Style = UIStyle.Black;
		((Control)btnReadSN).TabIndex = 107;
		((Control)btnReadSN).Text = "读取";
		((Control)btnReadSN).Click += btnReadSN_Click;
		((Control)uiLabel10).BackColor = Color.Transparent;
		((Control)uiLabel10).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel10).ForeColor = Color.Silver;
		((Control)uiLabel10).Location = new Point(6, 363);
		((Control)uiLabel10).Name = "uiLabel10";
		((Control)uiLabel10).Size = new Size(69, 29);
		uiLabel10.Style = UIStyle.Black;
		((Control)uiLabel10).TabIndex = 105;
		((Control)uiLabel10).Text = "序列号:";
		((Label)uiLabel10).TextAlign = (ContentAlignment)64;
		((Control)txtSN).Cursor = Cursors.IBeam;
		txtSN.FillColor = Color.White;
		((Control)txtSN).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtSN).Location = new Point(80, 360);
		((Control)txtSN).Margin = new Padding(4, 5, 4, 5);
		txtSN.Maximum = 2147483647.0;
		txtSN.Minimum = -2147483648.0;
		((Control)txtSN).MinimumSize = new Size(1, 1);
		((Control)txtSN).Name = "txtSN";
		txtSN.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtSN).Size = new Size(174, 34);
		txtSN.Style = UIStyle.Black;
		((Control)txtSN).TabIndex = 104;
		txtSN.TextAlignment = (ContentAlignment)16;
		txtSN.Watermark = "";
		((Control)btnWhite).BackColor = Color.Transparent;
		((Control)btnWhite).Cursor = Cursors.Hand;
		btnWhite.FillColor = Color.FromArgb(15, 40, 70);
		btnWhite.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnWhite.FillPressColor = Color.FromArgb(235, 243, 255);
		btnWhite.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnWhite).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnWhite.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnWhite.ForePressColor = Color.FromArgb(130, 130, 130);
		btnWhite.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWhite).Location = new Point(94, 672);
		((Control)btnWhite).Margin = new Padding(2);
		((Control)btnWhite).MinimumSize = new Size(1, 1);
		((Control)btnWhite).Name = "btnWhite";
		btnWhite.Radius = 25;
		btnWhite.RectColor = Color.FromArgb(130, 130, 130);
		btnWhite.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnWhite.RectPressColor = Color.FromArgb(130, 130, 130);
		btnWhite.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnWhite).Size = new Size(74, 29);
		btnWhite.Style = UIStyle.Black;
		((Control)btnWhite).TabIndex = 98;
		((Control)btnWhite).Text = "白色";
		((Control)btnWhite).Click += btnWhite_Click;
		((Control)btnBlue).BackColor = Color.Transparent;
		((Control)btnBlue).Cursor = Cursors.Hand;
		btnBlue.FillColor = Color.FromArgb(15, 40, 70);
		btnBlue.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnBlue.FillPressColor = Color.FromArgb(235, 243, 255);
		btnBlue.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnBlue).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnBlue.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnBlue.ForePressColor = Color.FromArgb(130, 130, 130);
		btnBlue.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnBlue).Location = new Point(9, 672);
		((Control)btnBlue).Margin = new Padding(2);
		((Control)btnBlue).MinimumSize = new Size(1, 1);
		((Control)btnBlue).Name = "btnBlue";
		btnBlue.Radius = 25;
		btnBlue.RectColor = Color.FromArgb(130, 130, 130);
		btnBlue.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnBlue.RectPressColor = Color.FromArgb(130, 130, 130);
		btnBlue.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnBlue).Size = new Size(74, 29);
		btnBlue.Style = UIStyle.Black;
		((Control)btnBlue).TabIndex = 99;
		((Control)btnBlue).Text = "蓝色";
		((Control)btnBlue).Click += btnBlue_Click;
		((Control)btnStop).BackColor = Color.Transparent;
		((Control)btnStop).Cursor = Cursors.Hand;
		btnStop.FillColor = Color.FromArgb(15, 40, 70);
		btnStop.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnStop.FillPressColor = Color.FromArgb(235, 243, 255);
		btnStop.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnStop).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnStop.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnStop.ForePressColor = Color.FromArgb(130, 130, 130);
		btnStop.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStop).Location = new Point(186, 672);
		((Control)btnStop).Margin = new Padding(2);
		((Control)btnStop).MinimumSize = new Size(1, 1);
		((Control)btnStop).Name = "btnStop";
		btnStop.Radius = 25;
		btnStop.RectColor = Color.FromArgb(130, 130, 130);
		btnStop.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnStop.RectPressColor = Color.FromArgb(130, 130, 130);
		btnStop.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStop).Size = new Size(74, 29);
		btnStop.Style = UIStyle.Black;
		((Control)btnStop).TabIndex = 100;
		((Control)btnStop).Text = "停止";
		((Control)btnStop).Click += btnStop_Click;
		((Control)uiLabel6).BackColor = Color.Transparent;
		((Control)uiLabel6).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel6).ForeColor = Color.Silver;
		((Control)uiLabel6).Location = new Point(10, 543);
		((Control)uiLabel6).Name = "uiLabel6";
		((Control)uiLabel6).Size = new Size(94, 29);
		uiLabel6.Style = UIStyle.Black;
		((Control)uiLabel6).TabIndex = 37;
		((Control)uiLabel6).Text = "图像类型:";
		((Label)uiLabel6).TextAlign = (ContentAlignment)32;
		((Control)uiLabel6).Visible = false;
		((Control)btnStopReg).BackColor = Color.Transparent;
		((Control)btnStopReg).Cursor = Cursors.Hand;
		btnStopReg.FillColor = Color.FromArgb(15, 40, 70);
		btnStopReg.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnStopReg.FillPressColor = Color.FromArgb(235, 243, 255);
		btnStopReg.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnStopReg).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnStopReg.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnStopReg.ForePressColor = Color.FromArgb(130, 130, 130);
		btnStopReg.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStopReg).Location = new Point(196, 306);
		((Control)btnStopReg).Margin = new Padding(2);
		((Control)btnStopReg).MinimumSize = new Size(1, 1);
		((Control)btnStopReg).Name = "btnStopReg";
		btnStopReg.Radius = 25;
		btnStopReg.RectColor = Color.FromArgb(130, 130, 130);
		btnStopReg.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnStopReg.RectPressColor = Color.FromArgb(130, 130, 130);
		btnStopReg.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStopReg).Size = new Size(74, 29);
		btnStopReg.Style = UIStyle.Black;
		((Control)btnStopReg).TabIndex = 101;
		((Control)btnStopReg).Text = "停止读";
		((Control)btnStopReg).Click += btnStopReg_Click;
		((Control)btnAutoReg).BackColor = Color.Transparent;
		((Control)btnAutoReg).Cursor = Cursors.Hand;
		btnAutoReg.FillColor = Color.FromArgb(15, 40, 70);
		btnAutoReg.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnAutoReg.FillPressColor = Color.FromArgb(235, 243, 255);
		btnAutoReg.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnAutoReg).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnAutoReg.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnAutoReg.ForePressColor = Color.FromArgb(130, 130, 130);
		btnAutoReg.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnAutoReg).Location = new Point(118, 306);
		((Control)btnAutoReg).Margin = new Padding(2);
		((Control)btnAutoReg).MinimumSize = new Size(1, 1);
		((Control)btnAutoReg).Name = "btnAutoReg";
		btnAutoReg.Radius = 25;
		btnAutoReg.RectColor = Color.FromArgb(130, 130, 130);
		btnAutoReg.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnAutoReg.RectPressColor = Color.FromArgb(130, 130, 130);
		btnAutoReg.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnAutoReg).Size = new Size(74, 29);
		btnAutoReg.Style = UIStyle.Black;
		((Control)btnAutoReg).TabIndex = 101;
		((Control)btnAutoReg).Text = "启动读";
		((Control)btnAutoReg).Click += btnAutoReg_Click;
		((Control)btnAuto).BackColor = Color.Transparent;
		((Control)btnAuto).Cursor = Cursors.Hand;
		btnAuto.FillColor = Color.FromArgb(15, 40, 70);
		btnAuto.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnAuto.FillPressColor = Color.FromArgb(235, 243, 255);
		btnAuto.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnAuto).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnAuto.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnAuto.ForePressColor = Color.FromArgb(130, 130, 130);
		btnAuto.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnAuto).Location = new Point(186, 625);
		((Control)btnAuto).Margin = new Padding(2);
		((Control)btnAuto).MinimumSize = new Size(1, 1);
		((Control)btnAuto).Name = "btnAuto";
		btnAuto.Radius = 25;
		btnAuto.RectColor = Color.FromArgb(130, 130, 130);
		btnAuto.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnAuto.RectPressColor = Color.FromArgb(130, 130, 130);
		btnAuto.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnAuto).Size = new Size(74, 29);
		btnAuto.Style = UIStyle.Black;
		((Control)btnAuto).TabIndex = 101;
		((Control)btnAuto).Text = "自动";
		((Control)btnAuto).Click += btnAuto_Click;
		((Control)btnGreen).BackColor = Color.Transparent;
		((Control)btnGreen).Cursor = Cursors.Hand;
		btnGreen.FillColor = Color.FromArgb(15, 40, 70);
		btnGreen.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnGreen.FillPressColor = Color.FromArgb(235, 243, 255);
		btnGreen.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnGreen).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnGreen.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnGreen.ForePressColor = Color.FromArgb(130, 130, 130);
		btnGreen.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnGreen).Location = new Point(94, 625);
		((Control)btnGreen).Margin = new Padding(2);
		((Control)btnGreen).MinimumSize = new Size(1, 1);
		((Control)btnGreen).Name = "btnGreen";
		btnGreen.Radius = 25;
		btnGreen.RectColor = Color.FromArgb(130, 130, 130);
		btnGreen.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnGreen.RectPressColor = Color.FromArgb(130, 130, 130);
		btnGreen.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnGreen).Size = new Size(74, 29);
		btnGreen.Style = UIStyle.Black;
		((Control)btnGreen).TabIndex = 102;
		((Control)btnGreen).Text = "绿色";
		((Control)btnGreen).Click += btnGreen_Click;
		((Control)btnRed).BackColor = Color.Transparent;
		((Control)btnRed).Cursor = Cursors.Hand;
		btnRed.FillColor = Color.FromArgb(15, 40, 70);
		btnRed.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnRed.FillPressColor = Color.FromArgb(235, 243, 255);
		btnRed.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnRed).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnRed.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnRed.ForePressColor = Color.FromArgb(130, 130, 130);
		btnRed.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRed).Location = new Point(9, 625);
		((Control)btnRed).Margin = new Padding(2);
		((Control)btnRed).MinimumSize = new Size(1, 1);
		((Control)btnRed).Name = "btnRed";
		btnRed.Radius = 25;
		btnRed.RectColor = Color.FromArgb(130, 130, 130);
		btnRed.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnRed.RectPressColor = Color.FromArgb(130, 130, 130);
		btnRed.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRed).Size = new Size(74, 29);
		btnRed.Style = UIStyle.Black;
		((Control)btnRed).TabIndex = 103;
		((Control)btnRed).Text = "红色";
		((Control)btnRed).Click += btnRed_Click;
		((Control)txtD4).Cursor = Cursors.IBeam;
		txtD4.FillColor = Color.White;
		((Control)txtD4).Font = new Font("微软雅黑", 12f);
		((Control)txtD4).Location = new Point(137, 206);
		((Control)txtD4).Margin = new Padding(4, 5, 4, 5);
		txtD4.Maximum = 2147483647.0;
		txtD4.Minimum = -2147483648.0;
		((Control)txtD4).MinimumSize = new Size(1, 1);
		((Control)txtD4).Name = "txtD4";
		txtD4.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtD4).Size = new Size(117, 34);
		txtD4.Style = UIStyle.Black;
		((Control)txtD4).TabIndex = 90;
		txtD4.TextAlignment = (ContentAlignment)16;
		txtD4.Leave += txtD4_Leave;
		((Control)uiButton1).BackColor = Color.Transparent;
		((Control)uiButton1).Cursor = Cursors.Hand;
		uiButton1.FillColor = Color.FromArgb(15, 40, 70);
		uiButton1.FillHoverColor = Color.FromArgb(216, 233, 255);
		uiButton1.FillPressColor = Color.FromArgb(235, 243, 255);
		uiButton1.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)uiButton1).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		uiButton1.ForeHoverColor = Color.FromArgb(130, 130, 130);
		uiButton1.ForePressColor = Color.FromArgb(130, 130, 130);
		uiButton1.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton1).Location = new Point(13, 252);
		((Control)uiButton1).MinimumSize = new Size(1, 1);
		((Control)uiButton1).Name = "uiButton1";
		uiButton1.Radius = 25;
		uiButton1.RectColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectHoverColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectPressColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton1).Size = new Size(96, 29);
		uiButton1.Style = UIStyle.Black;
		((Control)uiButton1).TabIndex = 86;
		((Control)uiButton1).Text = "显示图表";
		((Control)uiButton1).Click += uiButton1_Click;
		((Control)txtA4).Cursor = Cursors.IBeam;
		txtA4.FillColor = Color.White;
		((Control)txtA4).Font = new Font("微软雅黑", 12f);
		((Control)txtA4).Location = new Point(13, 206);
		((Control)txtA4).Margin = new Padding(4, 5, 4, 5);
		txtA4.Maximum = 2147483647.0;
		txtA4.Minimum = -2147483648.0;
		((Control)txtA4).MinimumSize = new Size(1, 1);
		((Control)txtA4).Name = "txtA4";
		txtA4.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtA4).Size = new Size(117, 34);
		txtA4.Style = UIStyle.Black;
		((Control)txtA4).TabIndex = 91;
		((Control)txtA4).Text = "0x00";
		txtA4.TextAlignment = (ContentAlignment)16;
		((Control)txtD3).Cursor = Cursors.IBeam;
		txtD3.FillColor = Color.White;
		((Control)txtD3).Font = new Font("微软雅黑", 12f);
		((Control)txtD3).Location = new Point(137, 155);
		((Control)txtD3).Margin = new Padding(4, 5, 4, 5);
		txtD3.Maximum = 2147483647.0;
		txtD3.Minimum = -2147483648.0;
		((Control)txtD3).MinimumSize = new Size(1, 1);
		((Control)txtD3).Name = "txtD3";
		txtD3.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtD3).Size = new Size(117, 34);
		txtD3.Style = UIStyle.Black;
		((Control)txtD3).TabIndex = 92;
		txtD3.TextAlignment = (ContentAlignment)16;
		txtD3.Leave += txtD3_Leave;
		((Control)txtA3).Cursor = Cursors.IBeam;
		txtA3.FillColor = Color.White;
		((Control)txtA3).Font = new Font("微软雅黑", 12f);
		((Control)txtA3).Location = new Point(13, 155);
		((Control)txtA3).Margin = new Padding(4, 5, 4, 5);
		txtA3.Maximum = 2147483647.0;
		txtA3.Minimum = -2147483648.0;
		((Control)txtA3).MinimumSize = new Size(1, 1);
		((Control)txtA3).Name = "txtA3";
		txtA3.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtA3).Size = new Size(117, 34);
		txtA3.Style = UIStyle.Black;
		((Control)txtA3).TabIndex = 93;
		((Control)txtA3).Text = "0x00";
		txtA3.TextAlignment = (ContentAlignment)16;
		((Control)txtD2).Cursor = Cursors.IBeam;
		txtD2.FillColor = Color.White;
		((Control)txtD2).Font = new Font("微软雅黑", 12f);
		((Control)txtD2).Location = new Point(137, 104);
		((Control)txtD2).Margin = new Padding(4, 5, 4, 5);
		txtD2.Maximum = 2147483647.0;
		txtD2.Minimum = -2147483648.0;
		((Control)txtD2).MinimumSize = new Size(1, 1);
		((Control)txtD2).Name = "txtD2";
		txtD2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtD2).Size = new Size(117, 34);
		txtD2.Style = UIStyle.Black;
		((Control)txtD2).TabIndex = 94;
		txtD2.TextAlignment = (ContentAlignment)16;
		txtD2.Leave += txtD2_Leave;
		((Control)txtA2).Cursor = Cursors.IBeam;
		txtA2.FillColor = Color.White;
		((Control)txtA2).Font = new Font("微软雅黑", 12f);
		((Control)txtA2).Location = new Point(13, 104);
		((Control)txtA2).Margin = new Padding(4, 5, 4, 5);
		txtA2.Maximum = 2147483647.0;
		txtA2.Minimum = -2147483648.0;
		((Control)txtA2).MinimumSize = new Size(1, 1);
		((Control)txtA2).Name = "txtA2";
		txtA2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtA2).Size = new Size(117, 34);
		txtA2.Style = UIStyle.Black;
		((Control)txtA2).TabIndex = 95;
		((Control)txtA2).Text = "0x00";
		txtA2.TextAlignment = (ContentAlignment)16;
		((Control)txtD1).Cursor = Cursors.IBeam;
		txtD1.FillColor = Color.White;
		((Control)txtD1).Font = new Font("微软雅黑", 12f);
		((Control)txtD1).Location = new Point(137, 52);
		((Control)txtD1).Margin = new Padding(4, 5, 4, 5);
		txtD1.Maximum = 2147483647.0;
		txtD1.Minimum = -2147483648.0;
		((Control)txtD1).MinimumSize = new Size(1, 1);
		((Control)txtD1).Name = "txtD1";
		txtD1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtD1).Size = new Size(117, 34);
		txtD1.Style = UIStyle.Black;
		((Control)txtD1).TabIndex = 96;
		txtD1.TextAlignment = (ContentAlignment)16;
		txtD1.Leave += txtD1_Leave;
		((Control)txtA1).Cursor = Cursors.IBeam;
		txtA1.FillColor = Color.White;
		((Control)txtA1).Font = new Font("微软雅黑", 12f);
		((Control)txtA1).Location = new Point(13, 52);
		((Control)txtA1).Margin = new Padding(4, 5, 4, 5);
		txtA1.Maximum = 2147483647.0;
		txtA1.Minimum = -2147483648.0;
		((Control)txtA1).MinimumSize = new Size(1, 1);
		((Control)txtA1).Name = "txtA1";
		txtA1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtA1).Size = new Size(117, 34);
		txtA1.Style = UIStyle.Black;
		((Control)txtA1).TabIndex = 97;
		((Control)txtA1).Text = "0x00";
		txtA1.TextAlignment = (ContentAlignment)16;
		((Control)uiLabel9).BackColor = Color.Transparent;
		((Control)uiLabel9).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel9).ForeColor = Color.Silver;
		((Control)uiLabel9).Location = new Point(133, 15);
		((Control)uiLabel9).Name = "uiLabel9";
		((Control)uiLabel9).Size = new Size(102, 29);
		uiLabel9.Style = UIStyle.Black;
		((Control)uiLabel9).TabIndex = 88;
		((Control)uiLabel9).Text = "寄存器数据:";
		((Label)uiLabel9).TextAlign = (ContentAlignment)16;
		((Control)uiLabel11).BackColor = Color.Transparent;
		((Control)uiLabel11).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel11).ForeColor = Color.Silver;
		((Control)uiLabel11).Location = new Point(3, 304);
		((Control)uiLabel11).Name = "uiLabel11";
		((Control)uiLabel11).Size = new Size(72, 29);
		uiLabel11.Style = UIStyle.Black;
		((Control)uiLabel11).TabIndex = 89;
		((Control)uiLabel11).Text = "时间:";
		((Label)uiLabel11).TextAlign = (ContentAlignment)16;
		((Control)uiLabel8).BackColor = Color.Transparent;
		((Control)uiLabel8).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel8).ForeColor = Color.Silver;
		((Control)uiLabel8).Location = new Point(9, 15);
		((Control)uiLabel8).Name = "uiLabel8";
		((Control)uiLabel8).Size = new Size(100, 29);
		uiLabel8.Style = UIStyle.Black;
		((Control)uiLabel8).TabIndex = 89;
		((Control)uiLabel8).Text = "寄存器地址:";
		((Label)uiLabel8).TextAlign = (ContentAlignment)16;
		cbxRegType.DataSource = null;
		cbxRegType.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxRegType.FillColor = Color.White;
		((Control)cbxRegType).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxRegType.Items.AddRange(new object[3] { "视频盒", "定子", "转子" });
		((Control)cbxRegType).Location = new Point(107, 679);
		((Control)cbxRegType).Margin = new Padding(4, 5, 4, 5);
		((Control)cbxRegType).MinimumSize = new Size(63, 0);
		((Control)cbxRegType).Name = "cbxRegType";
		((Control)cbxRegType).Padding = new Padding(0, 0, 30, 2);
		cbxRegType.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxRegType).Size = new Size(116, 29);
		cbxRegType.Style = UIStyle.Black;
		((Control)cbxRegType).TabIndex = 28;
		((Control)cbxRegType).Text = "视频盒";
		cbxRegType.TextAlignment = (ContentAlignment)16;
		((Control)cbxRegType).Visible = false;
		cbxRegType.SelectedIndexChanged += cbxRegType_SelectedIndexChanged;
		((Control)txtLog1).BackColor = Color.Black;
		((TextBoxBase)txtLog1).BorderStyle = (BorderStyle)1;
		((Control)txtLog1).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtLog1).ForeColor = Color.White;
		((Control)txtLog1).Location = new Point(27, 97);
		((TextBoxBase)txtLog1).Multiline = true;
		((Control)txtLog1).Name = "txtLog1";
		txtLog1.ScrollBars = (ScrollBars)2;
		((Control)txtLog1).Size = new Size(831, 480);
		((Control)txtLog1).TabIndex = 2;
		((Control)txtLog1).KeyDown += new KeyEventHandler(txtLog1_KeyDown);
		((Control)txtID).BackColor = Color.Transparent;
		((Control)txtID).Cursor = Cursors.IBeam;
		txtID.DoubleValue = 1.0;
		txtID.FillColor = Color.White;
		((Control)txtID).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		txtID.IntValue = 1;
		((Control)txtID).Location = new Point(294, 28);
		((Control)txtID).Margin = new Padding(4, 5, 4, 5);
		txtID.Maximum = 2147483647.0;
		txtID.Minimum = -2147483648.0;
		((Control)txtID).MinimumSize = new Size(1, 1);
		((Control)txtID).Name = "txtID";
		txtID.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtID).Size = new Size(76, 29);
		txtID.Style = UIStyle.Black;
		((Control)txtID).TabIndex = 41;
		((Control)txtID).Text = "01";
		txtID.TextAlignment = (ContentAlignment)16;
		((Control)lblID).BackColor = Color.Transparent;
		((Control)lblID).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)lblID).ForeColor = Color.Silver;
		((Control)lblID).Location = new Point(203, 28);
		((Control)lblID).Name = "lblID";
		((Control)lblID).Size = new Size(93, 29);
		lblID.Style = UIStyle.Black;
		((Control)lblID).TabIndex = 40;
		((Control)lblID).Text = "设备ID:";
		((Label)lblID).TextAlign = (ContentAlignment)32;
		((Control)btnAllScreen).BackColor = Color.Transparent;
		((Control)btnAllScreen).Cursor = Cursors.Hand;
		btnAllScreen.FillColor = Color.FromArgb(15, 40, 70);
		btnAllScreen.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnAllScreen.FillPressColor = Color.FromArgb(235, 243, 255);
		btnAllScreen.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnAllScreen).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnAllScreen.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnAllScreen.ForePressColor = Color.FromArgb(130, 130, 130);
		btnAllScreen.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnAllScreen).Location = new Point(661, 632);
		((Control)btnAllScreen).Margin = new Padding(2);
		((Control)btnAllScreen).MinimumSize = new Size(1, 1);
		((Control)btnAllScreen).Name = "btnAllScreen";
		btnAllScreen.Radius = 25;
		btnAllScreen.RectColor = Color.FromArgb(130, 130, 130);
		btnAllScreen.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnAllScreen.RectPressColor = Color.FromArgb(130, 130, 130);
		btnAllScreen.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnAllScreen).Size = new Size(119, 29);
		btnAllScreen.Style = UIStyle.Black;
		((Control)btnAllScreen).TabIndex = 39;
		((Control)btnAllScreen).Text = "应用升级";
		((Control)btnAllScreen).Click += btnAllScreen_Click;
		((Control)btnSpem).BackColor = Color.Transparent;
		((Control)btnSpem).Cursor = Cursors.Hand;
		btnSpem.FillColor = Color.FromArgb(15, 40, 70);
		btnSpem.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnSpem.FillPressColor = Color.FromArgb(235, 243, 255);
		btnSpem.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnSpem).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnSpem.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnSpem.ForePressColor = Color.FromArgb(130, 130, 130);
		btnSpem.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSpem).Location = new Point(333, 632);
		((Control)btnSpem).Margin = new Padding(2);
		((Control)btnSpem).MinimumSize = new Size(1, 1);
		((Control)btnSpem).Name = "btnSpem";
		btnSpem.Radius = 25;
		btnSpem.RectColor = Color.FromArgb(130, 130, 130);
		btnSpem.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSpem.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSpem.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSpem).Size = new Size(90, 29);
		btnSpem.Style = UIStyle.Black;
		((Control)btnSpem).TabIndex = 39;
		((Control)btnSpem).Text = "温度查询";
		((Control)btnSpem).Visible = false;
		((Control)btnSpem).Click += btnSpem_Click;
		cbxUpgradeType.DataSource = null;
		cbxUpgradeType.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxUpgradeType.FillColor = Color.White;
		((Control)cbxUpgradeType).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxUpgradeType.Items.AddRange(new object[3] { "视频盒", "定子", "转子" });
		((Control)cbxUpgradeType).Location = new Point(118, 28);
		((Control)cbxUpgradeType).Margin = new Padding(4, 5, 4, 5);
		((Control)cbxUpgradeType).MinimumSize = new Size(63, 0);
		((Control)cbxUpgradeType).Name = "cbxUpgradeType";
		((Control)cbxUpgradeType).Padding = new Padding(0, 0, 30, 2);
		cbxUpgradeType.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxUpgradeType).Size = new Size(90, 29);
		cbxUpgradeType.Style = UIStyle.Black;
		((Control)cbxUpgradeType).TabIndex = 36;
		((Control)cbxUpgradeType).Text = "视频盒";
		cbxUpgradeType.TextAlignment = (ContentAlignment)16;
		cbxUpgradeType.SelectedIndexChanged += cbxUpgradeType_SelectedIndexChanged;
		((Control)uiLabel5).BackColor = Color.Transparent;
		((Control)uiLabel5).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel5).ForeColor = Color.Silver;
		((Control)uiLabel5).Location = new Point(3, 28);
		((Control)uiLabel5).Name = "uiLabel5";
		((Control)uiLabel5).Size = new Size(120, 29);
		uiLabel5.Style = UIStyle.Black;
		((Control)uiLabel5).TabIndex = 35;
		((Control)uiLabel5).Text = "板卡类型:";
		((Label)uiLabel5).TextAlign = (ContentAlignment)32;
		((Control)txtRegData).BackColor = Color.Transparent;
		((Control)txtRegData).Cursor = Cursors.IBeam;
		txtRegData.FillColor = Color.White;
		((Control)txtRegData).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtRegData).Location = new Point(562, 679);
		((Control)txtRegData).Margin = new Padding(4, 5, 4, 5);
		txtRegData.Maximum = 2147483647.0;
		txtRegData.Minimum = -2147483648.0;
		((Control)txtRegData).MinimumSize = new Size(1, 1);
		((Control)txtRegData).Name = "txtRegData";
		txtRegData.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtRegData).Size = new Size(136, 29);
		txtRegData.Style = UIStyle.Black;
		((Control)txtRegData).TabIndex = 26;
		txtRegData.TextAlignment = (ContentAlignment)16;
		((Control)txtRegData).Visible = false;
		((Control)txtRegAddr).BackColor = Color.Transparent;
		((Control)txtRegAddr).Cursor = Cursors.IBeam;
		txtRegAddr.FillColor = Color.White;
		((Control)txtRegAddr).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtRegAddr).Location = new Point(325, 679);
		((Control)txtRegAddr).Margin = new Padding(4, 5, 4, 5);
		txtRegAddr.Maximum = 2147483647.0;
		txtRegAddr.Minimum = -2147483648.0;
		((Control)txtRegAddr).MinimumSize = new Size(1, 1);
		((Control)txtRegAddr).Name = "txtRegAddr";
		txtRegAddr.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtRegAddr).Size = new Size(137, 29);
		txtRegAddr.Style = UIStyle.Black;
		((Control)txtRegAddr).TabIndex = 26;
		txtRegAddr.TextAlignment = (ContentAlignment)16;
		((Control)txtRegAddr).Visible = false;
		((Control)txtPath).BackColor = Color.Transparent;
		((Control)txtPath).Cursor = Cursors.IBeam;
		txtPath.FillColor = Color.White;
		((Control)txtPath).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtPath).Location = new Point(460, 25);
		((Control)txtPath).Margin = new Padding(4, 5, 4, 5);
		txtPath.Maximum = 2147483647.0;
		txtPath.Minimum = -2147483648.0;
		((Control)txtPath).MinimumSize = new Size(1, 1);
		((Control)txtPath).Name = "txtPath";
		txtPath.RectColor = Color.FromArgb(130, 130, 130);
		((Control)txtPath).Size = new Size(335, 34);
		txtPath.Style = UIStyle.Black;
		((Control)txtPath).TabIndex = 20;
		txtPath.TextAlignment = (ContentAlignment)16;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 15f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).Controls.Add((Control)(object)uiPanel1);
		((Control)this).Margin = new Padding(4);
		((Control)this).Name = "UserControl3";
		((Control)this).Size = new Size(1276, 832);
		((Control)uiPanel1).ResumeLayout(false);
		((Control)uiPanel1).PerformLayout();
		((Control)panelArgu).ResumeLayout(false);
		((Control)panel1).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
	}
}
