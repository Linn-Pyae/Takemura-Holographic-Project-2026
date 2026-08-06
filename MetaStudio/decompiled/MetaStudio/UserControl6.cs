using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Sunny.UI;

namespace MetaStudio;

public class UserControl6 : UserControl
{
	private int ori_offsetX = 140;

	private int ori_offsetY = 140;

	private int col = 3;

	private int row = 3;

	private int offsetx = 0;

	private int offsety = 0;

	private int centerx = 0;

	private int centery = 0;

	private bool isCenter = false;

	private Form1 frmMain;

	private AppConfig appConfig = new AppConfig();

	private System.Timers.Timer timer = new System.Timers.Timer();

	private bool canSave = false;

	private bool isRead = false;

	private IContainer components = null;

	private UIPanel uiPanel1;

	private UIPanel uiPanel4;

	private UIPanel uiPanel3;

	private UIPanel uiPanel2;

	private UIPanel uiPanel5;

	private UIButton btnStop;

	private UIButton btnStart;

	private UISwitch switchScreen;

	private UILabel uiLabel1;

	private UILabel uiLabel7;

	private UISwitch switchLight;

	private UILabel uiLabel6;

	private UISwitch switchStart;

	private UIButton btnOutput;

	private UIButton btnOpenConfig;

	private UIComboBox cbmCom;

	private UIComboBox cbxSpeed;

	private UILabel uiLabel12;

	private UIComboBox cbxBackground;

	private UILabel uiLabel2;

	private UITrackBar trackBarLiangDu;

	private UILine uiLine5;

	private UIButton btnSelectScreen;

	private UIButton uiButton1;

	private UIButton uiButton2;

	private UILine uiLine7;

	private UIComboBox cbxNum;

	private UILabel uiLabel3;

	private UILine uiLine1;

	private UITrackBar uiTrackBar2;

	private UIButton btnjianY;

	private UIButton btnJiaY;

	private UILine uiLine2;

	private UILabel lblOri_y;

	private UIButton btnDown;

	private UIButton btnRight;

	private UIButton btnLeft;

	private UIButton btnCenter;

	private UIButton btnUp;

	private UIComboBox cbxCol;

	private UILabel uiLabel5;

	private UIComboBox cbxRow;

	private UILabel uiLabel4;

	private UIButton btnInit;

	private UILabel lblBright;

	private UICheckBox cbxIsBottom;

	private UICheckBox chkMode;

	public UserControl6()
	{
		InitializeComponent();
	}

	public UserControl6(Form1 _frmMain)
	{
		InitializeComponent();
		frmMain = _frmMain;
		frmMain.GetComPort2(cbmCom);
		if (string.IsNullOrEmpty(AppConfig.GetAppSetting("Col")))
		{
			cbxCol.SelectedIndex = 1;
		}
		else
		{
			cbxCol.SelectedIndex = Convert.ToInt32(AppConfig.GetAppSetting("Col"));
		}
		if (string.IsNullOrEmpty(AppConfig.GetAppSetting("Row")))
		{
			cbxRow.SelectedIndex = 1;
		}
		else
		{
			cbxRow.SelectedIndex = Convert.ToInt32(AppConfig.GetAppSetting("Row"));
		}
		timer.Elapsed += timer_Elapsed;
		timer.Enabled = true;
		timer.Interval = 3000.0;
	}

	private void timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		lock (ConstData.o)
		{
			if (canSave)
			{
				RegisterHelper.SaveConfig(0);
				canSave = false;
				Console.WriteLine("*********Saving**********");
			}
		}
	}

	private void InitSys()
	{
		offsetx = 0;
		offsety = 0;
		centerx = 0;
		centery = 0;
	}

	private void uiPanel5_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		foreach (KeyValuePair<string, string> item in frmMain.dic)
		{
			if (item.Value == ((Control)cbxNum).Text)
			{
				ConstData.selKey = item.Key;
			}
		}
		if (!frmMain.isbottom)
		{
			frmMain.isbottom = false;
			Helper.DrawEllipse(graphics, row, col, 100, 100, isbottom: false);
			DrawString();
		}
		else if (frmMain.isbottom)
		{
			frmMain.isbottom = true;
			Helper.DrawEllipse(graphics, row, col, 100, 100, isbottom: true);
			DrawString();
		}
		frmMain.SaveData();
		frmMain.c1.row = row;
		frmMain.c1.col = col;
		ChangeListBox();
	}

	private void DrawString()
	{
		int num = 1;
		for (int i = 0; i < col; i++)
		{
			if (i % 2 == 0)
			{
				for (int num2 = row - 1; num2 >= 0; num2--)
				{
					AutoDraw2(num2, i, num++);
				}
			}
			else
			{
				for (int num2 = 0; num2 < row; num2++)
				{
					AutoDraw2(num2, i, num++);
				}
			}
		}
	}

	private void AutoDraw2(int r, int c, int count)
	{
		Point point = new Point(ori_offsetX + c * 90, ori_offsetY + r * 90);
		string text = count.ToString();
		string text2 = r + "-" + c;
		if (Enumerable.Contains(frmMain.dic.Keys, text2))
		{
			frmMain.dic[text2] = text;
		}
		else
		{
			frmMain.dic.Add(text2, text);
		}
		Graphics g = ((Control)uiPanel5).CreateGraphics();
		Helper.DrawString6(g, new Point(point.X, point.Y), text, ori_offsetX, ori_offsetY);
	}

	private void cbxCol_SelectedIndexChanged(object sender, EventArgs e)
	{
		col = Convert.ToInt32(cbxCol.SelectedText);
		frmMain.dic.Clear();
		((Control)uiPanel5).Refresh();
		AppConfig.WriteConfig("Col", (col - 1).ToString());
		ConstData.DeviceCount = row * col;
	}

	private void cbxRow_SelectedIndexChanged(object sender, EventArgs e)
	{
		row = Convert.ToInt32(cbxRow.SelectedText);
		frmMain.dic.Clear();
		((Control)uiPanel5).Refresh();
		AppConfig.WriteConfig("Row", (row - 1).ToString());
		ConstData.DeviceCount = row * col;
	}

	private void ChangeListBox()
	{
		cbxNum.Items.Clear();
		for (int i = 1; i <= row * col; i++)
		{
			cbxNum.Items.Add((object)i);
		}
	}

	private void btnStart_Click(object sender, EventArgs e)
	{
		MetaTool.Start(0);
	}

	private void btnStop_Click(object sender, EventArgs e)
	{
		MetaTool.Stop(0);
	}

	private void switchScreen_ValueChanged(object sender, bool value)
	{
		lock (ConstData.o)
		{
			if (!value)
			{
				MetaTool.SetScreenProjection(0, value: false);
			}
			else
			{
				MetaTool.SetScreenProjection(0, value: true);
			}
			canSave = true;
		}
	}

	private void switchLight_ValueChanged(object sender, bool value)
	{
		lock (ConstData.o)
		{
			if (value)
			{
				MetaTool.SetBreathingLight(0, value: true);
			}
			else
			{
				MetaTool.SetBreathingLight(0, value: false);
			}
			canSave = true;
		}
	}

	private void cbxSpeed_SelectedIndexChanged(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			switch (cbxSpeed.SelectedIndex)
			{
			case 0:
				MetaTool.SetDeviceSpeed(0, 750);
				break;
			case 1:
				MetaTool.SetDeviceSpeed(0, 900);
				break;
			}
			canSave = true;
		}
	}

	private void cbxBackground_SelectedIndexChanged(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			int selectedIndex = cbxBackground.SelectedIndex;
			MetaTool.SetBackground(0, selectedIndex);
			canSave = true;
		}
	}

	private void trackBarLiangDu_ValueChanged(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			((Control)lblBright).Text = trackBarLiangDu.Value.ToString();
			int value = trackBarLiangDu.Value;
			MetaTool.SetBrightness(0, value);
			canSave = true;
		}
	}

	private void InitReg()
	{
		SPHelper.SendTOVdbox(0, 2, 5, 0);
		SPHelper.SendTOVdbox(0, 2, 6, 0);
		SPHelper.SendTOVdbox(0, 2, 7, 125830200);
		SPHelper.SendTOVdbox(0, 2, 14, 0);
		SPHelper.SendTOVdbox(0, 2, 15, 1);
		SPHelper.SendTOStator(0, 2, 1, 1);
		SPHelper.SendTOStator(0, 2, 12, 0);
		SPHelper.SendTOStator(0, 2, 42, 0);
		SPHelper.SendTORotor(0, 2, 123, 0);
	}

	private void btnInit_Click(object sender, EventArgs e)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		string text = string.Empty;
		string text2 = string.Empty;
		if (ConstData.versionType == "0")
		{
			text = "确认执行初始化操作(Y/N)？";
			text2 = "系统提示";
		}
		else if (ConstData.versionType == "1")
		{
			text = "Are you sure to execute(Y/N)？";
			text2 = "System Prompt";
		}
		if ((int)MessageBox.Show(text, text2, (MessageBoxButtons)4, (MessageBoxIcon)64) != 6)
		{
			return;
		}
		lock (ConstData.o)
		{
			SetTimer(0);
			InitSys();
			InitReg();
			if (!chkMode.Checked)
			{
				if (cbxCol.SelectedIndex == 0 && cbxRow.SelectedIndex == 0)
				{
					frmMain.u5.ResetClick();
					frmMain.c1.SetID2();
					MetaTool.EnableFusion(0, 1);
				}
				else
				{
					frmMain.c1.Init_Click();
					MetaTool.EnableFusion(0, 0);
				}
			}
			else
			{
				frmMain.u5.ResetClick();
				frmMain.c1.SetID();
				MetaTool.EnableFusion(0, 1);
			}
			cbxNum.SelectedIndex = 0;
			canSave = true;
		}
	}

	public void SetTimer(int ctrl)
	{
		Task task = new Task(delegate
		{
			timer.Enabled = false;
			Console.WriteLine(DateTime.Now.ToString() + "******Close Timer!********");
			Thread.Sleep(6000);
			timer.Enabled = true;
			Console.WriteLine(DateTime.Now.ToString() + "******Open Timer!********");
		});
		task.Start();
	}

	private void cbxNum_SelectedIndexChanged(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			((Control)uiPanel5).Refresh();
			SPHelper.SendTORotor(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 16, 0);
			Thread.Sleep(20);
			SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 32, 0);
			Thread.Sleep(20);
			SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 33, 0);
			Thread.Sleep(20);
			canSave = true;
		}
	}

	private void uiTrackBar2_ValueChanged(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			((Control)lblOri_y).Text = uiTrackBar2.Value.ToString();
			int value = uiTrackBar2.Value;
			int data = (int)((double)value * 2.8444444444444446);
			if (row * col == 1)
			{
				if (AppConfig.GetAppSetting("MotoDirct" + ((Control)cbxNum).Text) == "1")
				{
					data = 360 - (int)((double)value * 2.8444444444444446);
				}
				SPHelper.SendTORotor(Convert.ToInt32(0), 2, 16, data);
			}
			else
			{
				if (AppConfig.GetAppSetting("MotoDirct" + ((Control)cbxNum).Text) == "0")
				{
					if (!isRead)
					{
						int num = (int)((double)value * 2.8444444444444446);
						data = 360 - num;
						SPHelper.SendTORotor(Convert.ToInt32(((Control)cbxNum).Text) + 1, 2, 16, data);
					}
					else
					{
						data = (int)((double)value * 2.8444444444444446);
					}
				}
				else
				{
					SPHelper.SendTORotor(Convert.ToInt32(((Control)cbxNum).Text) + 1, 2, 16, data);
				}
				isRead = false;
			}
			canSave = true;
		}
	}

	private void btnJiaY_Click(object sender, EventArgs e)
	{
		if (uiTrackBar2.Value < 360)
		{
			uiTrackBar2.Value += 1;
		}
	}

	private void btnjianY_Click(object sender, EventArgs e)
	{
		if (uiTrackBar2.Value > 0)
		{
			uiTrackBar2.Value -= 1;
		}
	}

	public void GetSerData(byte[] buf)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		try
		{
			if (!((Control)this).Visible || !SPHelper.CheckHead(buf) || buf.Length != 26)
			{
				return;
			}
			if (buf[4] == 129 && buf[7] == 128 && buf[16] == 16)
			{
				MethodInvoker val = null;
				int num = SPHelper.ConvetInt(buf, 20);
				int angle = (int)((double)num / 2.8444444444444446) + 1;
				if (((Control)this).IsHandleCreated)
				{
					if (val == null)
					{
						val = (MethodInvoker)delegate
						{
							isRead = true;
							if (AppConfig.GetAppSetting("MotoDirct" + ((Control)cbxNum).Text) == "0")
							{
								uiTrackBar2.Value = angle + 17;
							}
							else
							{
								uiTrackBar2.Value = angle;
							}
						};
					}
					((Control)this).BeginInvoke((Delegate)(object)val);
				}
			}
			if (buf[4] != 129 || buf[7] != 0)
			{
				return;
			}
			if (buf[16] == 32)
			{
				MethodInvoker val2 = null;
				int data = SPHelper.ConvetInt(buf, 20);
				if (((Control)this).IsHandleCreated)
				{
					if (val2 == null)
					{
						val2 = (MethodInvoker)delegate
						{
							Console.WriteLine("X=" + data);
							SetXValue(data);
						};
					}
					((Control)this).BeginInvoke((Delegate)(object)val2);
				}
			}
			if (buf[16] != 33)
			{
				return;
			}
			MethodInvoker val3 = null;
			int data2 = SPHelper.ConvetInt(buf, 20);
			if (!((Control)this).IsHandleCreated)
			{
				return;
			}
			if (val3 == null)
			{
				val3 = (MethodInvoker)delegate
				{
					Console.WriteLine("Y=" + data2);
					SetYValue(data2);
				};
			}
			((Control)this).BeginInvoke((Delegate)(object)val3);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void SetXValue(int val)
	{
		if (!isCenter)
		{
			if (!((double)(val + offsetx) + ConstData.Diameter > (double)ConstData.Ori_Width) && val + offsetx >= 0)
			{
				if (row * col == 1)
				{
					SPHelper.SendTOStator(1, 2, 32, val + offsetx);
				}
				else
				{
					SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 2, 32, val + offsetx);
				}
				centerx += offsetx;
				Console.WriteLine("centerx=" + centerx);
				offsetx = 0;
			}
		}
		else
		{
			if (row * col == 1)
			{
				SPHelper.SendTOStator(1, 2, 32, val - centerx);
			}
			else
			{
				SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 2, 32, val - centerx);
			}
			centerx = 0;
		}
	}

	private void SetYValue(int val)
	{
		if (!isCenter)
		{
			if (!((double)(val + offsety) + ConstData.Diameter > 1080.0) && val + offsety >= 0)
			{
				if (row * col == 1)
				{
					SPHelper.SendTOStator(1, 2, 33, val + offsety);
				}
				else
				{
					SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 2, 33, val + offsety);
				}
				centery += offsety;
				Console.WriteLine("centery=" + centery);
				offsety = 0;
			}
		}
		else
		{
			if (row * col == 1)
			{
				SPHelper.SendTOStator(1, 2, 33, val - centery);
			}
			else
			{
				SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 2, 33, val - centery);
			}
			centery = 0;
		}
	}

	private void btnUp_Click(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			isCenter = false;
			offsety = 5;
			if (row * col == 1)
			{
				SPHelper.SendTOStator(1, 1, 33, 0);
			}
			else
			{
				SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 33, 0);
			}
			canSave = true;
		}
	}

	private void btnDown_Click(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			isCenter = false;
			offsety = -5;
			if (row * col == 1)
			{
				SPHelper.SendTOStator(1, 1, 33, 0);
			}
			else
			{
				SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 33, 0);
			}
			canSave = true;
		}
	}

	private void btnLeft_Click(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			isCenter = false;
			offsetx = 5;
			if (row * col == 1)
			{
				SPHelper.SendTOStator(1, 1, 32, 0);
			}
			else
			{
				SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 32, 0);
			}
			canSave = true;
		}
	}

	private void btnRight_Click(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			isCenter = false;
			offsetx = -5;
			if (row * col == 1)
			{
				SPHelper.SendTOStator(1, 1, 32, 0);
			}
			else
			{
				SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 32, 0);
			}
			canSave = true;
		}
	}

	private void btnCenter_Click(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			isCenter = true;
			if (row * col == 1)
			{
				SPHelper.SendTOStator(1, 1, 32, 0);
				Thread.Sleep(30);
				SPHelper.SendTOStator(1, 1, 33, 0);
			}
			else
			{
				SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 32, 0);
				Thread.Sleep(30);
				SPHelper.SendTOStator(Convert.ToInt32(((Control)cbxNum).Text) + 1, 1, 33, 0);
			}
			canSave = true;
		}
	}

	private void btnSelectScreen_Click(object sender, EventArgs e)
	{
		lock (ConstData.o)
		{
			if (!chkMode.Checked)
			{
				if (row * col == 1)
				{
					frmMain.u5.SelectScreen();
				}
				else
				{
					frmMain.c1.SelectImage();
				}
			}
			else
			{
				frmMain.u5.SelectScreen2(Convert.ToInt32(((Control)cbxNum).Text) + 1);
			}
			canSave = true;
		}
	}

	private void btnOutput_Click(object sender, EventArgs e)
	{
		frmMain.c1.OutputConfig();
	}

	private void btnOpenConfig_Click(object sender, EventArgs e)
	{
		frmMain.c1.InputConfig();
	}

	private void switchStart_ValueChanged(object sender, bool value)
	{
		lock (ConstData.o)
		{
			if (value)
			{
				MetaTool.SetAutoStart(0, value: true);
			}
			else
			{
				MetaTool.SetAutoStart(0, value: false);
			}
			canSave = true;
		}
	}

	private void uiButton1_Click(object sender, EventArgs e)
	{
		MetaTool.SetAdujst(0, 3);
	}

	public void Translate(int type)
	{
		switch (type)
		{
		case 0:
			((Control)btnOpenConfig).Text = "导入配置";
			((Control)uiLabel3).Text = "选择设备:";
			((Control)btnSelectScreen).Text = "设置显示区域";
			((Control)uiLabel12).Text = "转速:";
			((Control)btnStop).Text = "停止";
			((Control)btnStart).Text = "启动";
			((Control)btnOutput).Text = "导出配置";
			((Control)uiLine5).Text = "亮度调节";
			((Control)uiLine7).Text = "画面校准";
			((Control)uiLine1).Text = "角度调节";
			((Control)uiLabel7).Text = "呼吸灯:";
			((Control)uiLabel6).Text = "自启动:";
			((Control)uiLabel1).Text = "投屏:";
			switchScreen.ActiveText = "开";
			switchScreen.InActiveText = "关";
			switchStart.ActiveText = "开";
			switchStart.InActiveText = "关";
			switchLight.ActiveText = "开";
			switchLight.InActiveText = "关";
			((Control)uiLabel2).Text = "色调:";
			((Control)cbxBackground).Text = "标准";
			cbxBackground.Items.Clear();
			cbxBackground.Items.AddRange(new object[3] { "暖色", "标准", "冷色" });
			((Control)uiLabel5).Text = "X";
			((Control)uiLabel4).Text = "行 X 列:";
			((Control)btnUp).Text = "上移";
			((Control)btnDown).Text = "下移";
			((Control)btnRight).Text = "右移";
			((Control)btnLeft).Text = "左移";
			((Control)btnCenter).Text = "居中";
			((Control)uiLine2).Text = "画面移动";
			((Control)uiButton1).Text = "进入校准模式";
			((Control)uiButton2).Text = "退出校准模式";
			((Control)btnInit).Text = "初始化";
			((Control)chkMode).Text = "分散模式";
			((Control)cbxIsBottom).Text = "高";
			cbxSpeed.Items.Clear();
			((Control)cbxSpeed).Text = "普通";
			cbxSpeed.Items.AddRange(new object[2] { "普通", "高速" });
			break;
		case 1:
			((Control)btnOpenConfig).Text = "Import";
			((Control)uiLabel3).Text = "Select Device:";
			((Control)btnSelectScreen).Text = "Select Area";
			((Control)uiLabel12).Text = "Speed:";
			((Control)btnStop).Text = "Stop";
			((Control)btnStart).Text = "Start";
			((Control)btnOutput).Text = "Export";
			((Control)uiLine5).Text = "Light Adjust";
			((Control)uiLine7).Text = "Screen Adjust";
			((Control)uiLine1).Text = "Angle Adjust";
			((Control)uiLabel7).Text = "Breathing Light:";
			((Control)uiLabel6).Text = "AutoStart:";
			((Control)uiLabel1).Text = "Projection Screen:";
			switchScreen.ActiveText = "Open";
			switchScreen.InActiveText = "Close";
			switchLight.ActiveText = "Open";
			switchLight.InActiveText = "Close";
			switchStart.ActiveText = "Open";
			switchStart.InActiveText = "Close";
			((Control)uiLabel2).Text = "Hue:";
			((Control)cbxBackground).Text = "Standard";
			cbxBackground.Items.Clear();
			cbxBackground.Items.AddRange(new object[3] { "Warm Color", "Standard", "Cold Color" });
			((Control)uiLabel5).Text = "X";
			((Control)uiLabel4).Text = "Row X Column";
			cbxSpeed.Items.Clear();
			((Control)cbxSpeed).Text = "Normal";
			cbxSpeed.Items.AddRange(new object[2] { "Normal", "Fast" });
			((Control)btnUp).Text = "Up";
			((Control)btnDown).Text = "Down";
			((Control)btnRight).Text = "Right";
			((Control)btnLeft).Text = "Left";
			((Control)btnCenter).Text = "Center";
			((Control)uiLine2).Text = "Screen Move";
			((Control)uiButton1).Text = "Enter Adjust";
			((Control)uiButton2).Text = "Exit Adjust";
			((Control)btnInit).Text = "Initialize";
			((Control)chkMode).Text = "Discrete Mode";
			((Control)cbxIsBottom).Text = "High";
			break;
		}
	}

	private void uiTrackBar2_MouseUp(object sender, MouseEventArgs e)
	{
	}

	private void uiButton2_Click(object sender, EventArgs e)
	{
		MetaTool.SetAdujst(0, 0);
	}

	private void cbxIsBottom_CheckedChanged(object sender, EventArgs e)
	{
		Graphics g = ((Control)uiPanel5).CreateGraphics();
		((Control)uiPanel5).Refresh();
		if (!cbxIsBottom.Checked)
		{
			frmMain.isbottom = false;
			Helper.DrawEllipse(g, row, col, 100, 100, isbottom: false);
			DrawString();
		}
		else
		{
			frmMain.isbottom = true;
			Helper.DrawEllipse(g, row, col, 100, 100, isbottom: true);
			DrawString();
		}
	}

	public void ShowCheck()
	{
		if (((Control)cbxIsBottom).Visible)
		{
			((Control)cbxIsBottom).Visible = false;
			((Control)chkMode).Visible = false;
		}
		else
		{
			((Control)cbxIsBottom).Visible = true;
			((Control)chkMode).Visible = true;
		}
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
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Expected O, but got Unknown
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Expected O, but got Unknown
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_057e: Expected O, but got Unknown
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Expected O, but got Unknown
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_069b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a5: Expected O, but got Unknown
		//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_080e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0818: Expected O, but got Unknown
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fa: Expected O, but got Unknown
		//IL_0a73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b76: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b80: Expected O, but got Unknown
		//IL_0c4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c56: Expected O, but got Unknown
		//IL_0ccf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ddc: Expected O, but got Unknown
		//IL_100a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1014: Expected O, but got Unknown
		//IL_1048: Unknown result type (might be due to invalid IL or missing references)
		//IL_1197: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a1: Expected O, but got Unknown
		//IL_1224: Unknown result type (might be due to invalid IL or missing references)
		//IL_13dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_13e6: Expected O, but got Unknown
		//IL_1469: Unknown result type (might be due to invalid IL or missing references)
		//IL_1621: Unknown result type (might be due to invalid IL or missing references)
		//IL_162b: Expected O, but got Unknown
		//IL_16ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_1863: Unknown result type (might be due to invalid IL or missing references)
		//IL_186d: Expected O, but got Unknown
		//IL_18f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aa8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ab2: Expected O, but got Unknown
		//IL_1b35: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c86: Expected O, but got Unknown
		//IL_1d67: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d71: Expected O, but got Unknown
		//IL_1eae: Unknown result type (might be due to invalid IL or missing references)
		//IL_1eb8: Expected O, but got Unknown
		//IL_1f3b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2101: Unknown result type (might be due to invalid IL or missing references)
		//IL_210b: Expected O, but got Unknown
		//IL_218b: Unknown result type (might be due to invalid IL or missing references)
		//IL_22d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_22e3: Expected O, but got Unknown
		//IL_23a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_23b2: Expected O, but got Unknown
		//IL_23f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_23fc: Expected O, but got Unknown
		//IL_24fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_2508: Expected O, but got Unknown
		//IL_2564: Unknown result type (might be due to invalid IL or missing references)
		//IL_259f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2667: Unknown result type (might be due to invalid IL or missing references)
		//IL_2671: Expected O, but got Unknown
		//IL_279d: Unknown result type (might be due to invalid IL or missing references)
		//IL_27a7: Expected O, but got Unknown
		//IL_29c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_29d1: Expected O, but got Unknown
		//IL_2b94: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b9e: Expected O, but got Unknown
		//IL_2dff: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e09: Expected O, but got Unknown
		//IL_2e39: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ef9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f03: Expected O, but got Unknown
		//IL_3040: Unknown result type (might be due to invalid IL or missing references)
		//IL_304a: Expected O, but got Unknown
		//IL_3235: Unknown result type (might be due to invalid IL or missing references)
		//IL_323f: Expected O, but got Unknown
		//IL_3339: Unknown result type (might be due to invalid IL or missing references)
		//IL_3343: Expected O, but got Unknown
		//IL_3445: Unknown result type (might be due to invalid IL or missing references)
		//IL_344f: Expected O, but got Unknown
		//IL_34a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_34de: Unknown result type (might be due to invalid IL or missing references)
		//IL_35a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_35b0: Expected O, but got Unknown
		//IL_368a: Unknown result type (might be due to invalid IL or missing references)
		//IL_3694: Expected O, but got Unknown
		//IL_36e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_371b: Unknown result type (might be due to invalid IL or missing references)
		//IL_37e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_37ed: Expected O, but got Unknown
		//IL_38ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_38b7: Expected O, but got Unknown
		//IL_398e: Unknown result type (might be due to invalid IL or missing references)
		//IL_3998: Expected O, but got Unknown
		//IL_3a59: Unknown result type (might be due to invalid IL or missing references)
		//IL_3a63: Expected O, but got Unknown
		//IL_3b46: Unknown result type (might be due to invalid IL or missing references)
		//IL_3b50: Expected O, but got Unknown
		//IL_3c35: Unknown result type (might be due to invalid IL or missing references)
		//IL_3c3f: Expected O, but got Unknown
		//IL_3d00: Unknown result type (might be due to invalid IL or missing references)
		//IL_3d0a: Expected O, but got Unknown
		//IL_3e55: Unknown result type (might be due to invalid IL or missing references)
		//IL_3e5f: Expected O, but got Unknown
		//IL_3edf: Unknown result type (might be due to invalid IL or missing references)
		//IL_40a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_40af: Expected O, but got Unknown
		//IL_412c: Unknown result type (might be due to invalid IL or missing references)
		//IL_42bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_42c5: Expected O, but got Unknown
		//IL_42f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_43e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_43ed: Expected O, but got Unknown
		//IL_440a: Unknown result type (might be due to invalid IL or missing references)
		//IL_4445: Unknown result type (might be due to invalid IL or missing references)
		//IL_4567: Unknown result type (might be due to invalid IL or missing references)
		//IL_4571: Expected O, but got Unknown
		//IL_47a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_47ab: Expected O, but got Unknown
		uiPanel1 = new UIPanel();
		uiPanel5 = new UIPanel();
		chkMode = new UICheckBox();
		cbxIsBottom = new UICheckBox();
		btnInit = new UIButton();
		cbxCol = new UIComboBox();
		uiLabel5 = new UILabel();
		cbxRow = new UIComboBox();
		uiLabel4 = new UILabel();
		uiPanel4 = new UIPanel();
		btnDown = new UIButton();
		btnRight = new UIButton();
		btnLeft = new UIButton();
		btnCenter = new UIButton();
		btnUp = new UIButton();
		uiLine2 = new UILine();
		lblOri_y = new UILabel();
		btnJiaY = new UIButton();
		btnjianY = new UIButton();
		uiTrackBar2 = new UITrackBar();
		uiLine1 = new UILine();
		cbxNum = new UIComboBox();
		uiLabel3 = new UILabel();
		uiButton1 = new UIButton();
		uiButton2 = new UIButton();
		uiLine7 = new UILine();
		uiPanel3 = new UIPanel();
		lblBright = new UILabel();
		btnSelectScreen = new UIButton();
		trackBarLiangDu = new UITrackBar();
		uiLine5 = new UILine();
		cbxBackground = new UIComboBox();
		uiLabel2 = new UILabel();
		cbxSpeed = new UIComboBox();
		uiLabel12 = new UILabel();
		uiLabel6 = new UILabel();
		switchStart = new UISwitch();
		uiLabel7 = new UILabel();
		switchLight = new UISwitch();
		switchScreen = new UISwitch();
		uiLabel1 = new UILabel();
		btnStop = new UIButton();
		btnStart = new UIButton();
		uiPanel2 = new UIPanel();
		cbmCom = new UIComboBox();
		btnOutput = new UIButton();
		btnOpenConfig = new UIButton();
		((Control)uiPanel1).SuspendLayout();
		((Control)uiPanel5).SuspendLayout();
		((Control)uiPanel4).SuspendLayout();
		((Control)uiPanel3).SuspendLayout();
		((Control)uiPanel2).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)uiPanel1).Controls.Add((Control)(object)uiPanel5);
		((Control)uiPanel1).Controls.Add((Control)(object)uiPanel4);
		((Control)uiPanel1).Controls.Add((Control)(object)uiPanel3);
		((Control)uiPanel1).Controls.Add((Control)(object)uiPanel2);
		((Control)uiPanel1).Dock = (DockStyle)5;
		uiPanel1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel1).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel1).ForeColor = Color.Silver;
		((Control)uiPanel1).Location = new Point(0, 0);
		((Control)uiPanel1).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel1).MinimumSize = new Size(1, 1);
		((Control)uiPanel1).Name = "uiPanel1";
		uiPanel1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel1).Size = new Size(1326, 754);
		uiPanel1.Style = UIStyle.Black;
		((Control)uiPanel1).TabIndex = 0;
		((Control)uiPanel1).Text = "uiPanel1";
		uiPanel1.TextAlignment = (ContentAlignment)32;
		((Control)uiPanel5).Controls.Add((Control)(object)chkMode);
		((Control)uiPanel5).Controls.Add((Control)(object)cbxIsBottom);
		((Control)uiPanel5).Controls.Add((Control)(object)btnInit);
		((Control)uiPanel5).Controls.Add((Control)(object)cbxCol);
		((Control)uiPanel5).Controls.Add((Control)(object)uiLabel5);
		((Control)uiPanel5).Controls.Add((Control)(object)cbxRow);
		((Control)uiPanel5).Controls.Add((Control)(object)uiLabel4);
		((Control)uiPanel5).Dock = (DockStyle)5;
		uiPanel5.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel5).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel5).ForeColor = Color.Silver;
		((Control)uiPanel5).Location = new Point(270, 55);
		((Control)uiPanel5).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel5).MinimumSize = new Size(1, 1);
		((Control)uiPanel5).Name = "uiPanel5";
		uiPanel5.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel5).Size = new Size(722, 699);
		uiPanel5.Style = UIStyle.Black;
		((Control)uiPanel5).TabIndex = 3;
		((Control)uiPanel5).Text = null;
		uiPanel5.TextAlignment = (ContentAlignment)32;
		((Control)uiPanel5).Paint += new PaintEventHandler(uiPanel5_Paint);
		((Control)chkMode).Cursor = Cursors.Hand;
		((Control)chkMode).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)chkMode).ForeColor = Color.Silver;
		((Control)chkMode).Location = new Point(482, 32);
		((Control)chkMode).MinimumSize = new Size(1, 1);
		((Control)chkMode).Name = "chkMode";
		((Control)chkMode).Padding = new Padding(22, 0, 0, 0);
		((Control)chkMode).Size = new Size(130, 27);
		chkMode.Style = UIStyle.Black;
		((Control)chkMode).TabIndex = 81;
		((Control)chkMode).Text = "分散模式";
		((Control)chkMode).Visible = false;
		cbxIsBottom.Checked = true;
		((Control)cbxIsBottom).Cursor = Cursors.Hand;
		((Control)cbxIsBottom).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)cbxIsBottom).ForeColor = Color.Silver;
		((Control)cbxIsBottom).Location = new Point(615, 32);
		((Control)cbxIsBottom).MinimumSize = new Size(1, 1);
		((Control)cbxIsBottom).Name = "cbxIsBottom";
		((Control)cbxIsBottom).Padding = new Padding(22, 0, 0, 0);
		((Control)cbxIsBottom).Size = new Size(78, 27);
		cbxIsBottom.Style = UIStyle.Black;
		((Control)cbxIsBottom).TabIndex = 81;
		((Control)cbxIsBottom).Text = "高度";
		((Control)cbxIsBottom).Visible = false;
		cbxIsBottom.CheckedChanged += cbxIsBottom_CheckedChanged;
		((Control)btnInit).Cursor = Cursors.Hand;
		btnInit.FillColor = Color.FromArgb(15, 40, 70);
		btnInit.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnInit.FillPressColor = Color.FromArgb(235, 243, 255);
		btnInit.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnInit).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnInit.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnInit.ForePressColor = Color.FromArgb(130, 130, 130);
		btnInit.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnInit).Location = new Point(383, 30);
		((Control)btnInit).Margin = new Padding(2);
		((Control)btnInit).MinimumSize = new Size(1, 1);
		((Control)btnInit).Name = "btnInit";
		btnInit.Radius = 26;
		btnInit.RectColor = Color.FromArgb(130, 130, 130);
		btnInit.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnInit.RectPressColor = Color.FromArgb(130, 130, 130);
		btnInit.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnInit).Size = new Size(90, 29);
		btnInit.Style = UIStyle.Black;
		((Control)btnInit).TabIndex = 80;
		((Control)btnInit).Text = "初始化";
		((Control)btnInit).Click += btnInit_Click;
		cbxCol.DataSource = null;
		cbxCol.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxCol.FillColor = Color.White;
		((Control)cbxCol).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxCol.Items.AddRange(new object[8] { "1", "2", "3", "4", "5", "6", "7", "8" });
		((Control)cbxCol).Location = new Point(287, 30);
		((Control)cbxCol).Margin = new Padding(4);
		((Control)cbxCol).MinimumSize = new Size(62, 0);
		((Control)cbxCol).Name = "cbxCol";
		((Control)cbxCol).Padding = new Padding(0, 0, 42, 2);
		cbxCol.Radius = 15;
		cbxCol.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxCol).Size = new Size(65, 29);
		cbxCol.Style = UIStyle.Black;
		((Control)cbxCol).TabIndex = 79;
		((Control)cbxCol).Text = "3";
		cbxCol.TextAlignment = (ContentAlignment)16;
		cbxCol.SelectedIndexChanged += cbxCol_SelectedIndexChanged;
		((Control)uiLabel5).BackColor = Color.Transparent;
		((Control)uiLabel5).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel5).ForeColor = Color.Silver;
		((Control)uiLabel5).Location = new Point(233, 32);
		((Control)uiLabel5).Name = "uiLabel5";
		((Control)uiLabel5).Size = new Size(32, 28);
		uiLabel5.Style = UIStyle.Black;
		((Control)uiLabel5).TabIndex = 78;
		((Control)uiLabel5).Text = " X ";
		((Label)uiLabel5).TextAlign = (ContentAlignment)64;
		cbxRow.DataSource = null;
		cbxRow.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxRow.FillColor = Color.White;
		((Control)cbxRow).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxRow.Items.AddRange(new object[8] { "1", "2", "3", "4", "5", "6", "7", "8" });
		((Control)cbxRow).Location = new Point(157, 31);
		((Control)cbxRow).Margin = new Padding(4);
		((Control)cbxRow).MinimumSize = new Size(62, 0);
		((Control)cbxRow).Name = "cbxRow";
		((Control)cbxRow).Padding = new Padding(0, 0, 42, 2);
		cbxRow.Radius = 15;
		cbxRow.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxRow).Size = new Size(69, 29);
		cbxRow.Style = UIStyle.Black;
		((Control)cbxRow).TabIndex = 77;
		((Control)cbxRow).Text = "3";
		cbxRow.TextAlignment = (ContentAlignment)16;
		cbxRow.SelectedIndexChanged += cbxRow_SelectedIndexChanged;
		((Control)uiLabel4).BackColor = Color.Transparent;
		((Control)uiLabel4).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel4).ForeColor = Color.Silver;
		((Control)uiLabel4).Location = new Point(9, 31);
		((Control)uiLabel4).Name = "uiLabel4";
		((Control)uiLabel4).Size = new Size(141, 28);
		uiLabel4.Style = UIStyle.Black;
		((Control)uiLabel4).TabIndex = 76;
		((Control)uiLabel4).Text = "行 X 列:";
		((Label)uiLabel4).TextAlign = (ContentAlignment)64;
		((Control)uiPanel4).Controls.Add((Control)(object)btnDown);
		((Control)uiPanel4).Controls.Add((Control)(object)btnRight);
		((Control)uiPanel4).Controls.Add((Control)(object)btnLeft);
		((Control)uiPanel4).Controls.Add((Control)(object)btnCenter);
		((Control)uiPanel4).Controls.Add((Control)(object)btnUp);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLine2);
		((Control)uiPanel4).Controls.Add((Control)(object)lblOri_y);
		((Control)uiPanel4).Controls.Add((Control)(object)btnJiaY);
		((Control)uiPanel4).Controls.Add((Control)(object)btnjianY);
		((Control)uiPanel4).Controls.Add((Control)(object)uiTrackBar2);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLine1);
		((Control)uiPanel4).Controls.Add((Control)(object)cbxNum);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel3);
		((Control)uiPanel4).Controls.Add((Control)(object)uiButton1);
		((Control)uiPanel4).Controls.Add((Control)(object)uiButton2);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLine7);
		((Control)uiPanel4).Dock = (DockStyle)4;
		uiPanel4.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel4).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel4).ForeColor = Color.Silver;
		((Control)uiPanel4).Location = new Point(992, 55);
		((Control)uiPanel4).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel4).MinimumSize = new Size(1, 1);
		((Control)uiPanel4).Name = "uiPanel4";
		uiPanel4.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel4).Size = new Size(334, 699);
		uiPanel4.Style = UIStyle.Black;
		((Control)uiPanel4).TabIndex = 2;
		((Control)uiPanel4).Text = null;
		uiPanel4.TextAlignment = (ContentAlignment)32;
		((Control)btnDown).BackColor = Color.Transparent;
		((Control)btnDown).Cursor = Cursors.Hand;
		btnDown.FillColor = Color.FromArgb(15, 40, 70);
		btnDown.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnDown.FillPressColor = Color.FromArgb(235, 243, 255);
		btnDown.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnDown).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnDown.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnDown.ForePressColor = Color.FromArgb(130, 130, 130);
		btnDown.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnDown).Location = new Point(134, 591);
		((Control)btnDown).Margin = new Padding(2);
		((Control)btnDown).MinimumSize = new Size(1, 1);
		((Control)btnDown).Name = "btnDown";
		btnDown.RectColor = Color.FromArgb(130, 130, 130);
		btnDown.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnDown.RectPressColor = Color.FromArgb(130, 130, 130);
		btnDown.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnDown).Size = new Size(57, 39);
		btnDown.Style = UIStyle.Black;
		((Control)btnDown).TabIndex = 83;
		((Control)btnDown).Text = "下移";
		((Control)btnDown).Click += btnDown_Click;
		((Control)btnRight).BackColor = Color.Transparent;
		((Control)btnRight).Cursor = Cursors.Hand;
		btnRight.FillColor = Color.FromArgb(15, 40, 70);
		btnRight.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnRight.FillPressColor = Color.FromArgb(235, 243, 255);
		btnRight.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnRight).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnRight.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnRight.ForePressColor = Color.FromArgb(130, 130, 130);
		btnRight.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRight).Location = new Point(204, 537);
		((Control)btnRight).Margin = new Padding(2);
		((Control)btnRight).MinimumSize = new Size(1, 1);
		((Control)btnRight).Name = "btnRight";
		btnRight.RectColor = Color.FromArgb(130, 130, 130);
		btnRight.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnRight.RectPressColor = Color.FromArgb(130, 130, 130);
		btnRight.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRight).Size = new Size(57, 39);
		btnRight.Style = UIStyle.Black;
		((Control)btnRight).TabIndex = 83;
		((Control)btnRight).Text = "右移";
		((Control)btnRight).Click += btnRight_Click;
		((Control)btnLeft).BackColor = Color.Transparent;
		((Control)btnLeft).Cursor = Cursors.Hand;
		btnLeft.FillColor = Color.FromArgb(15, 40, 70);
		btnLeft.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnLeft.FillPressColor = Color.FromArgb(235, 243, 255);
		btnLeft.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnLeft).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnLeft.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnLeft.ForePressColor = Color.FromArgb(130, 130, 130);
		btnLeft.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnLeft).Location = new Point(62, 537);
		((Control)btnLeft).Margin = new Padding(2);
		((Control)btnLeft).MinimumSize = new Size(1, 1);
		((Control)btnLeft).Name = "btnLeft";
		btnLeft.RectColor = Color.FromArgb(130, 130, 130);
		btnLeft.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnLeft.RectPressColor = Color.FromArgb(130, 130, 130);
		btnLeft.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnLeft).Size = new Size(57, 39);
		btnLeft.Style = UIStyle.Black;
		((Control)btnLeft).TabIndex = 83;
		((Control)btnLeft).Text = "左移";
		((Control)btnLeft).Click += btnLeft_Click;
		((Control)btnCenter).BackColor = Color.Transparent;
		((Control)btnCenter).Cursor = Cursors.Hand;
		btnCenter.FillColor = Color.FromArgb(15, 40, 70);
		btnCenter.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnCenter.FillPressColor = Color.FromArgb(235, 243, 255);
		btnCenter.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnCenter).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnCenter.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnCenter.ForePressColor = Color.FromArgb(130, 130, 130);
		btnCenter.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnCenter).Location = new Point(134, 537);
		((Control)btnCenter).Margin = new Padding(2);
		((Control)btnCenter).MinimumSize = new Size(1, 1);
		((Control)btnCenter).Name = "btnCenter";
		btnCenter.RectColor = Color.FromArgb(130, 130, 130);
		btnCenter.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnCenter.RectPressColor = Color.FromArgb(130, 130, 130);
		btnCenter.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnCenter).Size = new Size(57, 39);
		btnCenter.Style = UIStyle.Black;
		((Control)btnCenter).TabIndex = 83;
		((Control)btnCenter).Text = "居中";
		((Control)btnCenter).Click += btnCenter_Click;
		((Control)btnUp).BackColor = Color.Transparent;
		((Control)btnUp).Cursor = Cursors.Hand;
		btnUp.FillColor = Color.FromArgb(15, 40, 70);
		btnUp.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnUp.FillPressColor = Color.FromArgb(235, 243, 255);
		btnUp.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnUp).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnUp.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnUp.ForePressColor = Color.FromArgb(130, 130, 130);
		btnUp.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnUp).Location = new Point(134, 478);
		((Control)btnUp).Margin = new Padding(2);
		((Control)btnUp).MinimumSize = new Size(1, 1);
		((Control)btnUp).Name = "btnUp";
		btnUp.RectColor = Color.FromArgb(130, 130, 130);
		btnUp.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnUp.RectPressColor = Color.FromArgb(130, 130, 130);
		btnUp.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnUp).Size = new Size(57, 39);
		btnUp.Style = UIStyle.Black;
		((Control)btnUp).TabIndex = 83;
		((Control)btnUp).Text = "上移";
		((Control)btnUp).Click += btnUp_Click;
		((Control)uiLine2).BackColor = Color.Black;
		uiLine2.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine2).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine2).ForeColor = Color.Silver;
		uiLine2.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine2).Location = new Point(32, 412);
		((Control)uiLine2).MinimumSize = new Size(2, 2);
		((Control)uiLine2).Name = "uiLine2";
		((Control)uiLine2).Size = new Size(284, 29);
		uiLine2.Style = UIStyle.Black;
		((Control)uiLine2).TabIndex = 82;
		((Control)uiLine2).Text = "画面移动";
		uiLine2.TextAlign = (ContentAlignment)16;
		((Control)lblOri_y).BackColor = Color.Transparent;
		((Control)lblOri_y).Font = new Font("微软雅黑", 12f);
		((Control)lblOri_y).ForeColor = Color.Silver;
		((Control)lblOri_y).Location = new Point(209, 316);
		((Control)lblOri_y).Name = "lblOri_y";
		((Control)lblOri_y).Size = new Size(62, 23);
		lblOri_y.Style = UIStyle.Black;
		((Control)lblOri_y).TabIndex = 81;
		((Control)lblOri_y).Text = "80";
		((Label)lblOri_y).TextAlign = (ContentAlignment)64;
		((Control)btnJiaY).BackColor = Color.Transparent;
		((Control)btnJiaY).Cursor = Cursors.Hand;
		btnJiaY.FillColor = Color.FromArgb(15, 40, 70);
		btnJiaY.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJiaY.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJiaY.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJiaY).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJiaY.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaY.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJiaY.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaY).Location = new Point(287, 339);
		((Control)btnJiaY).Margin = new Padding(2);
		((Control)btnJiaY).MinimumSize = new Size(1, 1);
		((Control)btnJiaY).Name = "btnJiaY";
		btnJiaY.Radius = 26;
		btnJiaY.RectColor = Color.FromArgb(130, 130, 130);
		btnJiaY.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaY.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJiaY.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaY).Size = new Size(29, 23);
		btnJiaY.Style = UIStyle.Black;
		((Control)btnJiaY).TabIndex = 80;
		((Control)btnJiaY).Text = "+";
		((Control)btnJiaY).Click += btnJiaY_Click;
		((Control)btnjianY).BackColor = Color.Transparent;
		((Control)btnjianY).Cursor = Cursors.Hand;
		btnjianY.FillColor = Color.FromArgb(15, 40, 70);
		btnjianY.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnjianY.FillPressColor = Color.FromArgb(235, 243, 255);
		btnjianY.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnjianY).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnjianY.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnjianY.ForePressColor = Color.FromArgb(130, 130, 130);
		btnjianY.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnjianY).Location = new Point(42, 339);
		((Control)btnjianY).Margin = new Padding(2);
		((Control)btnjianY).MinimumSize = new Size(1, 1);
		((Control)btnjianY).Name = "btnjianY";
		btnjianY.Radius = 26;
		btnjianY.RectColor = Color.FromArgb(130, 130, 130);
		btnjianY.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnjianY.RectPressColor = Color.FromArgb(130, 130, 130);
		btnjianY.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnjianY).Size = new Size(29, 23);
		btnjianY.Style = UIStyle.Black;
		((Control)btnjianY).TabIndex = 79;
		((Control)btnjianY).Text = "-";
		((Control)btnjianY).Click += btnjianY_Click;
		uiTrackBar2.DisableColor = Color.Silver;
		uiTrackBar2.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiTrackBar2).Font = new Font("微软雅黑", 12f);
		((Control)uiTrackBar2).Location = new Point(76, 337);
		uiTrackBar2.Maximum = 360;
		((Control)uiTrackBar2).MinimumSize = new Size(1, 1);
		((Control)uiTrackBar2).Name = "uiTrackBar2";
		((Control)uiTrackBar2).Size = new Size(206, 25);
		uiTrackBar2.Style = UIStyle.Black;
		((Control)uiTrackBar2).TabIndex = 78;
		((Control)uiTrackBar2).Text = "uiTrackBar2";
		uiTrackBar2.Value = 80;
		uiTrackBar2.ValueChanged += uiTrackBar2_ValueChanged;
		((Control)uiTrackBar2).MouseUp += new MouseEventHandler(uiTrackBar2_MouseUp);
		((Control)uiLine1).BackColor = Color.Black;
		uiLine1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine1).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine1).ForeColor = Color.Silver;
		uiLine1.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine1).Location = new Point(35, 288);
		((Control)uiLine1).MinimumSize = new Size(2, 2);
		((Control)uiLine1).Name = "uiLine1";
		((Control)uiLine1).Size = new Size(284, 29);
		uiLine1.Style = UIStyle.Black;
		((Control)uiLine1).TabIndex = 77;
		((Control)uiLine1).Text = "角度调节";
		uiLine1.TextAlign = (ContentAlignment)16;
		cbxNum.DataSource = null;
		cbxNum.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxNum.FillColor = Color.White;
		((Control)cbxNum).Font = new Font("微软雅黑", 16.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxNum.Items.AddRange(new object[4] { "1", "2", "3", "4" });
		((Control)cbxNum).Location = new Point(193, 210);
		((Control)cbxNum).Margin = new Padding(4);
		((Control)cbxNum).MinimumSize = new Size(62, 0);
		((Control)cbxNum).Name = "cbxNum";
		((Control)cbxNum).Padding = new Padding(0, 0, 42, 2);
		cbxNum.Radius = 15;
		cbxNum.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxNum).Size = new Size(112, 29);
		cbxNum.Style = UIStyle.Black;
		((Control)cbxNum).TabIndex = 76;
		((Control)cbxNum).Text = "1";
		cbxNum.TextAlignment = (ContentAlignment)16;
		cbxNum.SelectedIndexChanged += cbxNum_SelectedIndexChanged;
		((Control)uiLabel3).BackColor = Color.Transparent;
		((Control)uiLabel3).Font = new Font("微软雅黑", 12f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel3).ForeColor = Color.Silver;
		((Control)uiLabel3).Location = new Point(31, 212);
		((Control)uiLabel3).Name = "uiLabel3";
		((Control)uiLabel3).Size = new Size(155, 28);
		uiLabel3.Style = UIStyle.Black;
		((Control)uiLabel3).TabIndex = 75;
		((Control)uiLabel3).Text = "选择设备:";
		((Label)uiLabel3).TextAlign = (ContentAlignment)64;
		((Control)uiButton1).Cursor = Cursors.Hand;
		uiButton1.FillColor = Color.FromArgb(15, 40, 70);
		uiButton1.FillHoverColor = Color.FromArgb(216, 233, 255);
		uiButton1.FillPressColor = Color.FromArgb(235, 243, 255);
		uiButton1.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)uiButton1).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		uiButton1.ForeHoverColor = Color.FromArgb(130, 130, 130);
		uiButton1.ForePressColor = Color.FromArgb(130, 130, 130);
		uiButton1.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton1).Location = new Point(28, 108);
		((Control)uiButton1).MinimumSize = new Size(1, 1);
		((Control)uiButton1).Name = "uiButton1";
		uiButton1.Radius = 26;
		uiButton1.RectColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectHoverColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectPressColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton1).Size = new Size(122, 37);
		uiButton1.Style = UIStyle.Black;
		((Control)uiButton1).TabIndex = 74;
		((Control)uiButton1).Text = "进入校准模式";
		((Control)uiButton1).Click += uiButton1_Click;
		((Control)uiButton2).Cursor = Cursors.Hand;
		uiButton2.FillColor = Color.FromArgb(15, 40, 70);
		uiButton2.FillHoverColor = Color.FromArgb(216, 233, 255);
		uiButton2.FillPressColor = Color.FromArgb(235, 243, 255);
		uiButton2.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)uiButton2).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		uiButton2.ForeHoverColor = Color.FromArgb(130, 130, 130);
		uiButton2.ForePressColor = Color.FromArgb(130, 130, 130);
		uiButton2.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton2).Location = new Point(193, 108);
		((Control)uiButton2).MinimumSize = new Size(1, 1);
		((Control)uiButton2).Name = "uiButton2";
		uiButton2.Radius = 26;
		uiButton2.RectColor = Color.FromArgb(130, 130, 130);
		uiButton2.RectHoverColor = Color.FromArgb(130, 130, 130);
		uiButton2.RectPressColor = Color.FromArgb(130, 130, 130);
		uiButton2.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton2).Size = new Size(112, 37);
		uiButton2.Style = UIStyle.Black;
		((Control)uiButton2).TabIndex = 73;
		((Control)uiButton2).Text = "退出校准模式";
		((Control)uiButton2).Click += uiButton2_Click;
		((Control)uiLine7).BackColor = Color.Black;
		uiLine7.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine7).ForeColor = Color.Silver;
		uiLine7.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine7).Location = new Point(28, 51);
		((Control)uiLine7).MinimumSize = new Size(2, 2);
		((Control)uiLine7).Name = "uiLine7";
		((Control)uiLine7).Size = new Size(277, 29);
		uiLine7.Style = UIStyle.Black;
		((Control)uiLine7).TabIndex = 52;
		((Control)uiLine7).Text = "画面校准";
		uiLine7.TextAlign = (ContentAlignment)16;
		((Control)uiPanel3).Controls.Add((Control)(object)lblBright);
		((Control)uiPanel3).Controls.Add((Control)(object)btnSelectScreen);
		((Control)uiPanel3).Controls.Add((Control)(object)trackBarLiangDu);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLine5);
		((Control)uiPanel3).Controls.Add((Control)(object)cbxBackground);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLabel2);
		((Control)uiPanel3).Controls.Add((Control)(object)cbxSpeed);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLabel12);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLabel6);
		((Control)uiPanel3).Controls.Add((Control)(object)switchStart);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLabel7);
		((Control)uiPanel3).Controls.Add((Control)(object)switchLight);
		((Control)uiPanel3).Controls.Add((Control)(object)switchScreen);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLabel1);
		((Control)uiPanel3).Controls.Add((Control)(object)btnStop);
		((Control)uiPanel3).Controls.Add((Control)(object)btnStart);
		((Control)uiPanel3).Dock = (DockStyle)3;
		uiPanel3.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel3).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel3).ForeColor = Color.Silver;
		((Control)uiPanel3).Location = new Point(0, 55);
		((Control)uiPanel3).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel3).MinimumSize = new Size(1, 1);
		((Control)uiPanel3).Name = "uiPanel3";
		uiPanel3.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel3).Size = new Size(270, 699);
		uiPanel3.Style = UIStyle.Black;
		((Control)uiPanel3).TabIndex = 1;
		((Control)uiPanel3).Text = null;
		uiPanel3.TextAlignment = (ContentAlignment)32;
		((Control)lblBright).BackColor = Color.Transparent;
		((Control)lblBright).Font = new Font("微软雅黑", 12f);
		((Control)lblBright).ForeColor = Color.Silver;
		((Control)lblBright).Location = new Point(159, 536);
		((Control)lblBright).Name = "lblBright";
		((Control)lblBright).Size = new Size(62, 23);
		lblBright.Style = UIStyle.Black;
		((Control)lblBright).TabIndex = 82;
		((Control)lblBright).Text = "255";
		((Label)lblBright).TextAlign = (ContentAlignment)64;
		((Control)btnSelectScreen).BackColor = Color.Transparent;
		((Control)btnSelectScreen).Cursor = Cursors.Hand;
		btnSelectScreen.FillColor = Color.FromArgb(15, 40, 70);
		btnSelectScreen.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnSelectScreen.FillPressColor = Color.FromArgb(235, 243, 255);
		btnSelectScreen.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnSelectScreen).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnSelectScreen.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.ForePressColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSelectScreen).Location = new Point(20, 613);
		((Control)btnSelectScreen).MinimumSize = new Size(1, 1);
		((Control)btnSelectScreen).Name = "btnSelectScreen";
		btnSelectScreen.Radius = 25;
		btnSelectScreen.RectColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.RectSelectedColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.ShowTips = true;
		((Control)btnSelectScreen).Size = new Size(125, 40);
		btnSelectScreen.Style = UIStyle.Black;
		((Control)btnSelectScreen).TabIndex = 81;
		((Control)btnSelectScreen).Text = "设置显示区域";
		btnSelectScreen.TipsColor = Color.Transparent;
		((Control)btnSelectScreen).Click += btnSelectScreen_Click;
		((Control)trackBarLiangDu).BackColor = Color.Black;
		trackBarLiangDu.DisableColor = Color.Silver;
		trackBarLiangDu.FillColor = Color.FromArgb(24, 24, 24);
		((Control)trackBarLiangDu).Font = new Font("微软雅黑", 12f);
		((Control)trackBarLiangDu).Location = new Point(10, 560);
		trackBarLiangDu.Maximum = 255;
		((Control)trackBarLiangDu).MinimumSize = new Size(1, 1);
		((Control)trackBarLiangDu).Name = "trackBarLiangDu";
		((Control)trackBarLiangDu).Size = new Size(225, 25);
		trackBarLiangDu.Style = UIStyle.Black;
		((Control)trackBarLiangDu).TabIndex = 80;
		((Control)trackBarLiangDu).Text = "uiTrackBar5";
		trackBarLiangDu.Value = 255;
		trackBarLiangDu.ValueChanged += trackBarLiangDu_ValueChanged;
		((Control)uiLine5).BackColor = Color.Black;
		uiLine5.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine5).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine5).ForeColor = Color.Silver;
		uiLine5.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine5).Location = new Point(10, 513);
		((Control)uiLine5).MinimumSize = new Size(2, 2);
		((Control)uiLine5).Name = "uiLine5";
		((Control)uiLine5).Size = new Size(225, 29);
		uiLine5.Style = UIStyle.Black;
		((Control)uiLine5).TabIndex = 79;
		((Control)uiLine5).Text = "亮度调节";
		uiLine5.TextAlign = (ContentAlignment)16;
		cbxBackground.DataSource = null;
		cbxBackground.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxBackground.FillColor = Color.White;
		((Control)cbxBackground).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxBackground.Items.AddRange(new object[3] { "暖色", "标准", "冷色" });
		((Control)cbxBackground).Location = new Point(143, 443);
		((Control)cbxBackground).Margin = new Padding(4);
		((Control)cbxBackground).MinimumSize = new Size(62, 0);
		((Control)cbxBackground).Name = "cbxBackground";
		((Control)cbxBackground).Padding = new Padding(0, 0, 42, 2);
		cbxBackground.Radius = 15;
		cbxBackground.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxBackground).Size = new Size(93, 29);
		cbxBackground.Style = UIStyle.Black;
		((Control)cbxBackground).TabIndex = 78;
		((Control)cbxBackground).Text = "标准";
		cbxBackground.TextAlignment = (ContentAlignment)16;
		cbxBackground.SelectedIndexChanged += cbxBackground_SelectedIndexChanged;
		((Control)uiLabel2).BackColor = Color.Transparent;
		((Control)uiLabel2).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel2).ForeColor = Color.Silver;
		((Label)uiLabel2).ImageAlign = (ContentAlignment)16;
		((Control)uiLabel2).Location = new Point(77, 443);
		((Control)uiLabel2).Name = "uiLabel2";
		((Control)uiLabel2).Size = new Size(53, 29);
		uiLabel2.Style = UIStyle.Black;
		((Control)uiLabel2).TabIndex = 77;
		((Control)uiLabel2).Text = "色调:";
		((Label)uiLabel2).TextAlign = (ContentAlignment)64;
		cbxSpeed.DataSource = null;
		cbxSpeed.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxSpeed.FillColor = Color.White;
		((Control)cbxSpeed).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxSpeed.Items.AddRange(new object[2] { "普通", "高速" });
		((Control)cbxSpeed).Location = new Point(143, 375);
		((Control)cbxSpeed).Margin = new Padding(4);
		((Control)cbxSpeed).MinimumSize = new Size(62, 0);
		((Control)cbxSpeed).Name = "cbxSpeed";
		((Control)cbxSpeed).Padding = new Padding(0, 0, 42, 2);
		cbxSpeed.Radius = 15;
		cbxSpeed.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxSpeed).Size = new Size(93, 29);
		cbxSpeed.Style = UIStyle.Black;
		((Control)cbxSpeed).TabIndex = 76;
		((Control)cbxSpeed).Text = "普通";
		cbxSpeed.TextAlignment = (ContentAlignment)16;
		cbxSpeed.SelectedIndexChanged += cbxSpeed_SelectedIndexChanged;
		((Control)uiLabel12).BackColor = Color.Transparent;
		((Control)uiLabel12).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel12).ForeColor = Color.Silver;
		((Label)uiLabel12).ImageAlign = (ContentAlignment)16;
		((Control)uiLabel12).Location = new Point(62, 375);
		((Control)uiLabel12).Name = "uiLabel12";
		((Control)uiLabel12).Size = new Size(68, 29);
		uiLabel12.Style = UIStyle.Black;
		((Control)uiLabel12).TabIndex = 75;
		((Control)uiLabel12).Text = "转速:";
		((Label)uiLabel12).TextAlign = (ContentAlignment)64;
		((Control)uiLabel6).BackColor = Color.Transparent;
		((Control)uiLabel6).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel6).ForeColor = Color.Silver;
		((Label)uiLabel6).ImageAlign = (ContentAlignment)16;
		((Control)uiLabel6).Location = new Point(37, 302);
		((Control)uiLabel6).Name = "uiLabel6";
		((Control)uiLabel6).Size = new Size(93, 28);
		uiLabel6.Style = UIStyle.Black;
		((Control)uiLabel6).TabIndex = 74;
		((Control)uiLabel6).Text = "自启动:";
		((Label)uiLabel6).TextAlign = (ContentAlignment)64;
		switchStart.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)switchStart).BackColor = Color.Transparent;
		((Control)switchStart).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)switchStart).Location = new Point(161, 302);
		((Control)switchStart).MinimumSize = new Size(1, 1);
		((Control)switchStart).Name = "switchStart";
		((Control)switchStart).Size = new Size(75, 29);
		switchStart.Style = UIStyle.Black;
		((Control)switchStart).TabIndex = 73;
		((Control)switchStart).Text = "uiSwitch1";
		switchStart.ValueChanged += switchStart_ValueChanged;
		((Control)uiLabel7).BackColor = Color.Transparent;
		((Control)uiLabel7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel7).ForeColor = Color.Silver;
		((Label)uiLabel7).ImageAlign = (ContentAlignment)16;
		((Control)uiLabel7).Location = new Point(6, 230);
		((Control)uiLabel7).Name = "uiLabel7";
		((Control)uiLabel7).Size = new Size(124, 28);
		uiLabel7.Style = UIStyle.Black;
		((Control)uiLabel7).TabIndex = 51;
		((Control)uiLabel7).Text = "呼吸灯:";
		((Label)uiLabel7).TextAlign = (ContentAlignment)64;
		switchLight.Active = true;
		switchLight.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)switchLight).BackColor = Color.Transparent;
		((Control)switchLight).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)switchLight).Location = new Point(161, 230);
		((Control)switchLight).MinimumSize = new Size(1, 1);
		((Control)switchLight).Name = "switchLight";
		((Control)switchLight).Size = new Size(75, 29);
		switchLight.Style = UIStyle.Black;
		((Control)switchLight).TabIndex = 50;
		((Control)switchLight).Text = "uiSwitch1";
		switchLight.ValueChanged += switchLight_ValueChanged;
		switchScreen.Active = true;
		switchScreen.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)switchScreen).BackColor = Color.Transparent;
		((Control)switchScreen).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)switchScreen).Location = new Point(161, 168);
		((Control)switchScreen).MinimumSize = new Size(1, 1);
		((Control)switchScreen).Name = "switchScreen";
		((Control)switchScreen).Size = new Size(75, 29);
		switchScreen.Style = UIStyle.Black;
		((Control)switchScreen).TabIndex = 43;
		((Control)switchScreen).Text = "uiSwitch2";
		switchScreen.ValueChanged += switchScreen_ValueChanged;
		((Control)uiLabel1).BackColor = Color.Transparent;
		((Control)uiLabel1).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel1).ForeColor = Color.Silver;
		((Label)uiLabel1).ImageAlign = (ContentAlignment)16;
		((Control)uiLabel1).Location = new Point(-20, 168);
		((Control)uiLabel1).Name = "uiLabel1";
		((Control)uiLabel1).Size = new Size(150, 23);
		uiLabel1.Style = UIStyle.Black;
		((Control)uiLabel1).TabIndex = 44;
		((Control)uiLabel1).Text = "投屏:";
		((Label)uiLabel1).TextAlign = (ContentAlignment)64;
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
		((Control)btnStop).Location = new Point(136, 51);
		((Control)btnStop).Margin = new Padding(2);
		((Control)btnStop).MinimumSize = new Size(1, 1);
		((Control)btnStop).Name = "btnStop";
		btnStop.Radius = 26;
		btnStop.RectColor = Color.FromArgb(130, 130, 130);
		btnStop.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnStop.RectPressColor = Color.FromArgb(130, 130, 130);
		btnStop.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStop).Size = new Size(99, 37);
		btnStop.Style = UIStyle.Black;
		((Control)btnStop).TabIndex = 41;
		((Control)btnStop).Text = "停止";
		((Control)btnStop).Click += btnStop_Click;
		((Control)btnStart).BackColor = Color.Transparent;
		((Control)btnStart).Cursor = Cursors.Hand;
		btnStart.FillColor = Color.FromArgb(15, 40, 70);
		btnStart.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnStart.FillPressColor = Color.FromArgb(235, 243, 255);
		btnStart.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnStart).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnStart.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnStart.ForePressColor = Color.FromArgb(130, 130, 130);
		btnStart.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStart).Location = new Point(25, 51);
		((Control)btnStart).Margin = new Padding(2);
		((Control)btnStart).MinimumSize = new Size(1, 1);
		((Control)btnStart).Name = "btnStart";
		btnStart.Radius = 26;
		btnStart.RectColor = Color.FromArgb(130, 130, 130);
		btnStart.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnStart.RectPressColor = Color.FromArgb(130, 130, 130);
		btnStart.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStart).Size = new Size(96, 39);
		btnStart.Style = UIStyle.Black;
		((Control)btnStart).TabIndex = 42;
		((Control)btnStart).Text = "启动";
		((Control)btnStart).Click += btnStart_Click;
		((Control)uiPanel2).Controls.Add((Control)(object)cbmCom);
		((Control)uiPanel2).Controls.Add((Control)(object)btnOutput);
		((Control)uiPanel2).Controls.Add((Control)(object)btnOpenConfig);
		((Control)uiPanel2).Dock = (DockStyle)1;
		uiPanel2.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel2).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel2).ForeColor = Color.Silver;
		((Control)uiPanel2).Location = new Point(0, 0);
		((Control)uiPanel2).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel2).MinimumSize = new Size(1, 1);
		((Control)uiPanel2).Name = "uiPanel2";
		uiPanel2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel2).Size = new Size(1326, 55);
		uiPanel2.Style = UIStyle.Black;
		((Control)uiPanel2).TabIndex = 0;
		((Control)uiPanel2).Text = null;
		uiPanel2.TextAlignment = (ContentAlignment)32;
		((Control)cbmCom).BackColor = Color.White;
		cbmCom.DataSource = null;
		cbmCom.DropDownStyle = UIDropDownStyle.DropDownList;
		cbmCom.FillColor = Color.White;
		((Control)cbmCom).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)cbmCom).Location = new Point(23, 14);
		((Control)cbmCom).Margin = new Padding(4);
		((Control)cbmCom).MinimumSize = new Size(62, 0);
		((Control)cbmCom).Name = "cbmCom";
		((Control)cbmCom).Padding = new Padding(0, 0, 42, 2);
		cbmCom.Radius = 0;
		cbmCom.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbmCom).Size = new Size(98, 29);
		cbmCom.Style = UIStyle.Black;
		((Control)cbmCom).TabIndex = 73;
		cbmCom.TextAlignment = (ContentAlignment)16;
		((Control)btnOutput).Anchor = (AnchorStyles)8;
		((Control)btnOutput).Cursor = Cursors.Hand;
		btnOutput.FillColor = Color.FromArgb(15, 40, 70);
		btnOutput.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnOutput.FillPressColor = Color.FromArgb(235, 243, 255);
		btnOutput.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnOutput).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnOutput.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnOutput.ForePressColor = Color.FromArgb(130, 130, 130);
		btnOutput.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOutput).Location = new Point(1068, 10);
		((Control)btnOutput).MinimumSize = new Size(1, 1);
		((Control)btnOutput).Name = "btnOutput";
		btnOutput.Radius = 26;
		btnOutput.RectColor = Color.FromArgb(130, 130, 130);
		btnOutput.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnOutput.RectPressColor = Color.FromArgb(130, 130, 130);
		btnOutput.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOutput).Size = new Size(96, 37);
		btnOutput.Style = UIStyle.Black;
		((Control)btnOutput).TabIndex = 72;
		((Control)btnOutput).Text = "导出配置";
		((Control)btnOutput).Click += btnOutput_Click;
		((Control)btnOpenConfig).Anchor = (AnchorStyles)8;
		((Control)btnOpenConfig).Cursor = Cursors.Hand;
		btnOpenConfig.FillColor = Color.FromArgb(15, 40, 70);
		btnOpenConfig.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnOpenConfig.FillPressColor = Color.FromArgb(235, 243, 255);
		btnOpenConfig.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnOpenConfig).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnOpenConfig.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.ForePressColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOpenConfig).Location = new Point(1185, 10);
		((Control)btnOpenConfig).MinimumSize = new Size(1, 1);
		((Control)btnOpenConfig).Name = "btnOpenConfig";
		btnOpenConfig.Radius = 26;
		btnOpenConfig.RectColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectPressColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOpenConfig).Size = new Size(93, 37);
		btnOpenConfig.Style = UIStyle.Black;
		((Control)btnOpenConfig).TabIndex = 71;
		((Control)btnOpenConfig).Text = "导入配置";
		((Control)btnOpenConfig).Click += btnOpenConfig_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 15f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).Controls.Add((Control)(object)uiPanel1);
		((Control)this).Name = "UserControl6";
		((Control)this).Size = new Size(1326, 754);
		((Control)uiPanel1).ResumeLayout(false);
		((Control)uiPanel5).ResumeLayout(false);
		((Control)uiPanel4).ResumeLayout(false);
		((Control)uiPanel3).ResumeLayout(false);
		((Control)uiPanel2).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
	}
}
