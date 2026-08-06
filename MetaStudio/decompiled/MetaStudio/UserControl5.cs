using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CSharpWin_JD.CaptureImage;
using MetaStudio.Properties;
using Sunny.UI;

namespace MetaStudio;

public class UserControl5 : UserControl
{
	private int radiusRect;

	private int radiusElipse;

	private int radiusFR;

	private int r_x;

	private int r_y;

	private int e_x;

	private int e_y;

	private int fr_x;

	private int fr_y;

	private double rate;

	private int flag = 0;

	private Form1 mainFrm = null;

	private IContainer components = null;

	private UIPanel uiPanel1;

	private UIComboBox cbmCom;

	private UISwitch switchScreen;

	private UILabel uiLabel1;

	private UIButton btnTwo;

	private UIButton btnStop;

	private UIButton btnStart;

	private UILine uiLine9;

	private UIPanel uiPanelDraw;

	private UIAnalogMeter uiAnalogMeter1;

	private UILine uiLine7;

	private UIIntegerUpDown upDown;

	private UILabel uiLabel7;

	private UISwitch switchLight;

	private UITrackBar trackBarLiangDu;

	private UILine uiLine5;

	private UIComboBox cbxBackground;

	private UILabel uiLabel2;

	private UIComboBox cbxSpeed;

	private UILabel uiLabel12;

	private UIButton btnSelectScreen;

	private UILabel uiLabel3;

	private UILabel lblzoom;

	private UIImageButton btnLeft;

	private UIImageButton btnRigth;

	private UIImageButton btnButtom;

	private UILabel uiLabel5;

	private UIImageButton btnTop;

	private UILabel uiLabel4;

	public UILabel lblBright;

	private UITrackBar trackBarScale;

	private UIButton btnReset;

	private UIButton btnOpenConfig;

	private Timer timer1;

	private UIButton btnRefresh;

	private UIButton btnJian;

	private UIButton btnJia;

	private UIButton btnTurn;

	private UIButton btnOutput;

	private UIButton btnSaveConfig;

	private UILabel uiLabel6;

	private UISwitch switchStart;

	public UserControl5(Form1 _frmMain)
	{
		InitializeComponent();
		InitDraw();
		mainFrm = _frmMain;
		mainFrm.GetComPort2(cbmCom);
		timer1.Enabled = false;
	}

	private void InitDraw()
	{
		radiusRect = 490;
		radiusElipse = 490;
		radiusFR = 3;
		rate = 245.0 / 512.0;
		fr_x = (int)((double)((Control)uiPanelDraw).Width / 2.0);
		fr_y = (int)((double)((Control)uiPanelDraw).Height / 2.0);
		e_x = fr_x - (int)((double)radiusElipse / 2.0);
		e_y = 0;
		r_x = fr_x - (int)((double)radiusRect / 2.0);
		r_y = 0;
	}

	private void uiPanelDraw_Paint(object sender, PaintEventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		Graphics graphics = e.Graphics;
		Pen val = new Pen(Color.Blue, 1f);
		Brush val2 = (Brush)new SolidBrush(Color.Red);
		graphics.DrawRectangle(val, r_x, 5 + r_y, radiusRect, radiusRect);
		graphics.DrawEllipse(val, e_x, 5 + e_y, radiusElipse, radiusElipse);
		graphics.FillRectangle(val2, fr_x, fr_y, radiusFR, radiusFR);
	}

	private void btnRigth_Click(object sender, EventArgs e)
	{
		int startX = MetaTool.GetStartX(1);
		int imageWidth = MetaTool.GetImageWidth(1);
		if (startX + imageWidth + 10 <= ConstData.Ori_Width)
		{
			r_x += 10;
			e_x += 10;
			fr_x += 10;
			((Control)uiPanelDraw).Refresh();
			MetaTool.SetStartX(0, startX + 10);
		}
	}

	private void btnLeft_Click(object sender, EventArgs e)
	{
		int startX = MetaTool.GetStartX(1);
		int imageWidth = MetaTool.GetImageWidth(1);
		if (startX - 10 >= 0)
		{
			r_x -= 10;
			e_x -= 10;
			fr_x -= 10;
			((Control)uiPanelDraw).Refresh();
			MetaTool.SetStartX(0, startX - 10);
		}
	}

	private void btnTop_Click(object sender, EventArgs e)
	{
		int startY = MetaTool.GetStartY(1);
		int imageHeight = MetaTool.GetImageHeight(1);
		if (startY - 10 >= 0)
		{
			r_y -= 10;
			e_y -= 10;
			fr_y -= 10;
			((Control)uiPanelDraw).Refresh();
			MetaTool.SetStartY(0, startY - 10);
		}
	}

	private void btnButtom_Click(object sender, EventArgs e)
	{
		int startY = MetaTool.GetStartY(1);
		int imageHeight = MetaTool.GetImageHeight(1);
		if (startY + imageHeight + 10 <= ConstData.Ori_Height)
		{
			r_y += 10;
			e_y += 10;
			fr_y += 10;
			((Control)uiPanelDraw).Refresh();
			MetaTool.SetStartY(0, startY + 10);
		}
	}

	private void trackBarScale_ValueChanged(object sender, EventArgs e)
	{
		radiusElipse = (int)((double)trackBarScale.Value * rate);
		e_x = fr_x - (int)((double)radiusElipse / 2.0);
		e_y = fr_y - (int)((double)radiusElipse / 2.0);
		((Control)uiPanelDraw).Refresh();
		((Control)lblzoom).Text = trackBarScale.Value.ToString();
		if (trackBarScale.Value <= 1024)
		{
			MetaTool.SetScale(0, trackBarScale.Value);
			return;
		}
		int num = ConstData.frontWidth - (trackBarScale.Value - 1024);
		if (num < 32)
		{
			num = 32;
			int x = ConstData.frontPoint.X;
			int y = ConstData.frontPoint.Y;
			mainFrm.CaptureScreen(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
			CutScreenTOStator(new Point(x, y), num, num, 1, 1, iscenter: false);
		}
		else
		{
			int x = ConstData.frontPoint.X + (trackBarScale.Value - 1024) / 2;
			int y = ConstData.frontPoint.Y + (trackBarScale.Value - 1024) / 2;
			mainFrm.CaptureScreen(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
			CutScreenTOStator(new Point(x, y), num, num, 1, 1, iscenter: false);
		}
	}

	private void cbmCom_SelectedIndexChanged(object sender, EventArgs e)
	{
		mainFrm.ComChange(cbmCom.SelectedItem.ToString());
	}

	private void btnStart_Click(object sender, EventArgs e)
	{
		MetaTool.Start(0);
	}

	private void btnStop_Click(object sender, EventArgs e)
	{
		MetaTool.Stop(0);
	}

	private void cbxSpeed_SelectedIndexChanged(object sender, EventArgs e)
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
	}

	private void btnTwo_Click(object sender, EventArgs e)
	{
		MetaTool.MatchDevice(0, 0);
	}

	private void trackBarLiangDu_ValueChanged(object sender, EventArgs e)
	{
		((Control)lblBright).Text = trackBarLiangDu.Value.ToString();
		int value = trackBarLiangDu.Value;
		MetaTool.SetBrightness(0, value);
	}

	private void btnFront_Click(object sender, EventArgs e)
	{
		trackBarLiangDu.Value -= 1;
	}

	private void btnNext_Click(object sender, EventArgs e)
	{
		trackBarLiangDu.Value += 1;
	}

	private void upDown_ValueChanged(object sender, int value)
	{
		try
		{
			if (value < 0)
			{
				uiAnalogMeter1.Value = 0.0;
				upDown.Value = 0;
			}
			else if (value > 360)
			{
				uiAnalogMeter1.Value = 360.0;
				upDown.Value = 360;
			}
			else
			{
				uiAnalogMeter1.Value = value;
				MetaTool.SetAngle(0, value);
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void switchScreen_ValueChanged(object sender, bool value)
	{
		if (!value)
		{
			MetaTool.SetScreenProjection(0, value: false);
		}
		else
		{
			MetaTool.SetScreenProjection(0, value: true);
		}
	}

	private void switchLight_ValueChanged(object sender, bool value)
	{
		if (value)
		{
			MetaTool.SetBreathingLight(0, value: true);
		}
		else
		{
			MetaTool.SetBreathingLight(0, value: false);
		}
	}

	public void GetSerData(byte[] buf)
	{
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Expected O, but got Unknown
		try
		{
			if (!((Control)this).Visible || !SPHelper.CheckHead(buf) || buf.Length != 26)
			{
				return;
			}
			if (buf[4] == 129 && buf[7] == 128)
			{
				if (buf[16] == 16)
				{
					ConstData.CurAngle = SPHelper.ConvetInt(buf, 20);
				}
			}
			else if (buf[4] == 129 && buf[7] == 0)
			{
				if (buf[16] == 32)
				{
					ConstData.CurStartX = SPHelper.ConvetInt(buf, 20);
				}
				if (buf[16] == 33)
				{
					ConstData.CurStartY = SPHelper.ConvetInt(buf, 20);
				}
				if (buf[16] == 34)
				{
					ConstData.CurImageWidth = SPHelper.ConvetShort(buf, 20);
					ConstData.CurImageHeight = SPHelper.ConvetShort(buf, 22);
				}
			}
			else
			{
				if (buf[4] != 1 || buf[7] != 0 || buf[16] != 34)
				{
					return;
				}
				MethodInvoker val = null;
				int data = SPHelper.ConvetInt(buf, 20);
				if (!((Control)this).IsHandleCreated)
				{
					return;
				}
				if (val == null)
				{
					val = (MethodInvoker)delegate
					{
						trackBarLiangDu.Value = data;
					};
				}
				((Control)this).BeginInvoke((Delegate)(object)val);
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public void SelectScreen()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		try
		{
			((Control)mainFrm).Visible = false;
			CaptureImageTool captureImageTool = new CaptureImageTool();
			if ((int)((Form)captureImageTool).ShowDialog() == 1)
			{
				Image image = captureImageTool.Image;
				Point startPoint = captureImageTool.StartPoint;
				mainFrm.CaptureScreen(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
				CutScreenTOStator(startPoint, image.Width, image.Height, 1, 1, iscenter: false);
				((Control)mainFrm).Visible = true;
				ConstData.frontPoint = startPoint;
				ConstData.frontWidth = image.Height;
				trackBarScale.Value = 1024;
			}
			else
			{
				((Control)mainFrm).Visible = true;
			}
			((Form)captureImageTool).Close();
			((Component)(object)captureImageTool).Dispose();
			GC.Collect();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public void SelectScreen2(int id)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		try
		{
			((Control)mainFrm).Visible = false;
			CaptureImageTool captureImageTool = new CaptureImageTool();
			if ((int)((Form)captureImageTool).ShowDialog() == 1)
			{
				Image image = captureImageTool.Image;
				Point startPoint = captureImageTool.StartPoint;
				mainFrm.CaptureScreen(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
				CutScreenTOStator3(startPoint, image.Width, image.Height, 1, 1, iscenter: false, id);
				((Control)mainFrm).Visible = true;
				ConstData.frontPoint = startPoint;
				ConstData.frontWidth = image.Height;
			}
			else
			{
				((Control)mainFrm).Visible = true;
			}
			((Form)captureImageTool).Close();
			((Component)(object)captureImageTool).Dispose();
			GC.Collect();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void btnSelectScreen_Click(object sender, EventArgs e)
	{
		SelectScreen();
	}

	public void CutScreenTOStator2(Point point, int _width, int _height, int row, int col, bool iscenter)
	{
		try
		{
			double num = (double)_height / (ConstData.Radical_sign * (double)(row - 1) + 2.0);
			double num2 = (double)_width / (ConstData.Radical_sign * (double)(col - 1) + 2.0);
			if (num > num2)
			{
				num = num2;
			}
			ConstData.Diameter = num * 2.0;
			int num3 = (int)(ConstData.Radical_sign * num * (double)(col - 1));
			int num4 = point.X;
			int y = point.Y;
			if (iscenter)
			{
				num4 = (int)(((double)_width - (num * 2.0 + (double)num3)) / 2.0);
			}
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < col; j++)
				{
					int id = 0;
					MetaTool.CaptureScreenTOStator(id, new Point(num4 + (int)((double)j * (ConstData.Radical_sign * num)), y + (int)((double)i * (ConstData.Radical_sign * num))), (int)ConstData.Diameter, (int)ConstData.Diameter);
				}
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public int CutScreenTOStator(Point point, int _width, int _height, int row, int col, bool iscenter)
	{
		try
		{
			double num = (double)_height / (ConstData.Radical_sign * (double)(row - 1) + 2.0);
			double num2 = (double)_width / (ConstData.Radical_sign * (double)(col - 1) + 2.0);
			if (num > num2)
			{
				num = num2;
			}
			ConstData.Diameter = num * 2.0;
			int num3 = (int)(ConstData.Radical_sign * num * (double)(col - 1));
			int num4 = point.X;
			int y = point.Y;
			if (iscenter)
			{
				num4 = (int)(((double)_width - (num * 2.0 + (double)num3)) / 2.0);
			}
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < col; j++)
				{
					int id = 0;
					MetaTool.CaptureScreenTOStator(id, new Point(num4 + (int)((double)j * (ConstData.Radical_sign * num)), y + (int)((double)i * (ConstData.Radical_sign * num))), (int)ConstData.Diameter, (int)ConstData.Diameter);
				}
			}
			return num4;
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
		return 0;
	}

	public int CutScreenTOStator3(Point point, int _width, int _height, int row, int col, bool iscenter, int id)
	{
		try
		{
			double num = (double)_height / (ConstData.Radical_sign * (double)(row - 1) + 2.0);
			double num2 = (double)_width / (ConstData.Radical_sign * (double)(col - 1) + 2.0);
			if (num > num2)
			{
				num = num2;
			}
			ConstData.Diameter = num * 2.0;
			int num3 = (int)(ConstData.Radical_sign * num * (double)(col - 1));
			int num4 = point.X;
			int y = point.Y;
			if (iscenter)
			{
				num4 = (int)(((double)_width - (num * 2.0 + (double)num3)) / 2.0);
			}
			for (int i = 0; i < 1; i++)
			{
				for (int j = 0; j < 1; j++)
				{
					MetaTool.CaptureScreenTOStator(id, new Point(num4 + (int)((double)j * (ConstData.Radical_sign * num)), y + (int)((double)i * (ConstData.Radical_sign * num))), (int)ConstData.Diameter, (int)ConstData.Diameter);
				}
			}
			return num4;
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
		return 0;
	}

	private void btnReset_Click(object sender, EventArgs e)
	{
		ResetClick();
	}

	public void ResetClick()
	{
		mainFrm.CaptureScreen(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
		int x = CutScreenTOStator(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height, 1, 1, iscenter: true);
		mainFrm.ShowForm("初始化", 70);
		ConstData.frontPoint = new Point(x, 0);
		ConstData.frontWidth = ConstData.Ori_Height;
		MetaTool.SetVideoOutputEn(1);
		MetaTool.ResetID(0);
		MetaTool.CloseFusion(0);
		MetaTool.MotoDirct(0, 0);
	}

	private void cbxBackground_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = cbxBackground.SelectedIndex;
		MetaTool.SetBackground(0, selectedIndex);
	}

	private void UserControl5_VisibleChanged(object sender, EventArgs e)
	{
		MetaTool.GetBrightness(0);
	}

	private void btnSaveConfig_Click(object sender, EventArgs e)
	{
		mainFrm.SaveConfig();
	}

	private void btnOpenConfig_Click(object sender, EventArgs e)
	{
		MetaTool.Stop(0);
		mainFrm.ImportConfig();
		RefreshUI();
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		MethodInvoker val = null;
		int val2 = MetaTool.GetAngle(1);
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		if (val == null)
		{
			val = (MethodInvoker)delegate
			{
				if (val2 != -1)
				{
					upDown.Value = val2;
				}
			};
		}
		((Control)this).BeginInvoke((Delegate)(object)val);
	}

	private void btnRefresh_Click(object sender, EventArgs e)
	{
		RefreshUI();
	}

	public void RefreshUI()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		MethodInvoker val = null;
		ConstData.canUpdate = false;
		int angle = MetaTool.GetAngle(1);
		int brigth = MetaTool.GetBrightness(1);
		int speed = MetaTool.GetDeviceSpeed(1);
		int scale = MetaTool.GetScale(1);
		int autoS = MetaTool.GetAutoStart(1);
		Thread.Sleep(50);
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		if (val == null)
		{
			val = (MethodInvoker)delegate
			{
				if (ConstData.canUpdate)
				{
					upDown.Value = angle;
					trackBarLiangDu.Value = brigth;
					trackBarScale.Value = scale;
					if (speed == 474)
					{
						cbxSpeed.SelectedIndex = 1;
					}
					else if (speed == 385)
					{
						cbxSpeed.SelectedIndex = 0;
					}
					if (autoS == 0)
					{
						switchStart.Active = true;
					}
					else if (autoS == 1)
					{
						switchStart.Active = false;
					}
				}
			};
		}
		((Control)this).BeginInvoke((Delegate)(object)val);
	}

	private void btnJia_Click(object sender, EventArgs e)
	{
		trackBarScale.Value += 1;
	}

	private void btnJian_Click(object sender, EventArgs e)
	{
		trackBarScale.Value -= 1;
	}

	private void btnTurn_Click(object sender, EventArgs e)
	{
		btnStop_Click(null, null);
		if (((Control)btnTurn).Text == "反转")
		{
			((Control)btnTurn).Text = "正转";
			MetaTool.MotoDirct(0, 1);
		}
		else if (((Control)btnTurn).Text == "正转")
		{
			((Control)btnTurn).Text = "反转";
			MetaTool.MotoDirct(0, 0);
		}
		if (((Control)btnTurn).Text == "Anticlockwise")
		{
			((Control)btnTurn).Text = "Clockwise";
			MetaTool.MotoDirct(0, 1);
		}
		else if (((Control)btnTurn).Text == "Clockwise")
		{
			((Control)btnTurn).Text = "Anticlockwise";
			MetaTool.MotoDirct(0, 0);
		}
		btnStart_Click(null, null);
	}

	public void Translate(int type)
	{
		switch (type)
		{
		case 0:
			((Control)btnRefresh).Text = "刷新";
			((Control)btnOpenConfig).Text = "导入";
			((Control)btnReset).Text = "重置";
			((Control)uiLabel3).Text = "缩放:";
			((Control)btnSelectScreen).Text = "选择区域";
			((Control)uiLabel12).Text = "转速:";
			((Control)btnStop).Text = "停止";
			((Control)btnStart).Text = "启动";
			((Control)btnSaveConfig).Text = "保存";
			((Control)btnOutput).Text = "导出";
			((Control)btnTwo).Text = "匹配遥控器";
			((Control)uiLine5).Text = "亮度调节";
			((Control)uiLine7).Text = "角度调节";
			((Control)uiLabel7).Text = "呼吸灯开关:";
			((Control)uiLabel6).Text = "上电自启动:";
			((Control)uiLabel1).Text = "投屏:";
			switchScreen.ActiveText = "开";
			switchScreen.InActiveText = "关";
			switchStart.ActiveText = "开";
			switchStart.InActiveText = "关";
			switchLight.ActiveText = "开";
			switchLight.InActiveText = "关";
			((Control)btnTurn).Text = "正转";
			((Control)uiLabel2).Text = "色调:";
			((Control)cbxBackground).Text = "标准";
			cbxBackground.Items.Clear();
			cbxBackground.Items.AddRange(new object[3] { "暖色", "标准", "冷色" });
			((Control)uiLabel5).Text = "水平:";
			((Control)uiLabel4).Text = "垂直:";
			break;
		case 1:
			((Control)btnRefresh).Text = "Refresh";
			((Control)btnOpenConfig).Text = "Import";
			((Control)btnReset).Text = "Reset";
			((Control)uiLabel3).Text = "Scale:";
			((Control)btnSelectScreen).Text = "Select Area";
			((Control)uiLabel12).Text = "Speed:";
			((Control)btnStop).Text = "Stop";
			((Control)btnStart).Text = "Start";
			((Control)btnSaveConfig).Text = "Save";
			((Control)btnOutput).Text = "Export";
			((Control)btnTwo).Text = "Match";
			((Control)uiLine5).Text = "Light Adjust";
			((Control)uiLine7).Text = "Angle Adjust";
			((Control)uiLabel7).Text = "Breathing Light:";
			((Control)uiLabel6).Text = "AutoStart:";
			((Control)uiLabel1).Text = "Projection Screen:";
			switchScreen.ActiveText = "Open";
			switchScreen.InActiveText = "Close";
			switchLight.ActiveText = "Open";
			switchLight.InActiveText = "Close";
			switchStart.ActiveText = "Open";
			switchStart.InActiveText = "Close";
			((Control)btnTurn).Text = "Clockwise";
			((Control)uiLabel2).Text = "Hue:";
			((Control)cbxBackground).Text = "Standard";
			cbxBackground.Items.Clear();
			cbxBackground.Items.AddRange(new object[3] { "Warm Color", "Standard", "Cold Color" });
			((Control)uiLabel5).Text = "Horizontal:";
			((Control)uiLabel4).Text = "Vertical:";
			break;
		}
	}

	private void btnOutput_Click(object sender, EventArgs e)
	{
		mainFrm.OutputConfig();
	}

	private void switchStart_ValueChanged(object sender, bool value)
	{
		if (value)
		{
			MetaTool.SetAutoStart(0, value: true);
		}
		else
		{
			MetaTool.SetAutoStart(0, value: false);
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
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Expected O, but got Unknown
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_061a: Expected O, but got Unknown
		//IL_0649: Unknown result type (might be due to invalid IL or missing references)
		//IL_0710: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Expected O, but got Unknown
		//IL_07f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fd: Expected O, but got Unknown
		//IL_08be: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Expected O, but got Unknown
		//IL_09f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fe: Expected O, but got Unknown
		//IL_0c21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c2b: Expected O, but got Unknown
		//IL_0e4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e58: Expected O, but got Unknown
		//IL_0edb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1090: Unknown result type (might be due to invalid IL or missing references)
		//IL_109a: Expected O, but got Unknown
		//IL_111d: Unknown result type (might be due to invalid IL or missing references)
		//IL_12d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_12dc: Expected O, but got Unknown
		//IL_14ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_1509: Expected O, but got Unknown
		//IL_172c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1736: Expected O, but got Unknown
		//IL_1959: Unknown result type (might be due to invalid IL or missing references)
		//IL_1963: Expected O, but got Unknown
		//IL_19e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b34: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b3e: Expected O, but got Unknown
		//IL_1c41: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c4b: Expected O, but got Unknown
		//IL_1d3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d49: Expected O, but got Unknown
		//IL_1df7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e01: Expected O, but got Unknown
		//IL_1eb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ebf: Expected O, but got Unknown
		//IL_1f73: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f7d: Expected O, but got Unknown
		//IL_2031: Unknown result type (might be due to invalid IL or missing references)
		//IL_203b: Expected O, but got Unknown
		//IL_20f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_2100: Expected O, but got Unknown
		//IL_21ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_21b5: Expected O, but got Unknown
		//IL_22f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_22fc: Expected O, but got Unknown
		//IL_24df: Unknown result type (might be due to invalid IL or missing references)
		//IL_24e9: Expected O, but got Unknown
		//IL_253d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2578: Unknown result type (might be due to invalid IL or missing references)
		//IL_2640: Unknown result type (might be due to invalid IL or missing references)
		//IL_264a: Expected O, but got Unknown
		//IL_2719: Unknown result type (might be due to invalid IL or missing references)
		//IL_2723: Expected O, but got Unknown
		//IL_276f: Unknown result type (might be due to invalid IL or missing references)
		//IL_27aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_2872: Unknown result type (might be due to invalid IL or missing references)
		//IL_287c: Expected O, but got Unknown
		//IL_2948: Unknown result type (might be due to invalid IL or missing references)
		//IL_2952: Expected O, but got Unknown
		//IL_2a3c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a46: Expected O, but got Unknown
		//IL_2a7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b95: Expected O, but got Unknown
		//IL_2c6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c78: Expected O, but got Unknown
		//IL_2d5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d64: Expected O, but got Unknown
		//IL_2e61: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e6b: Expected O, but got Unknown
		//IL_2f3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f49: Expected O, but got Unknown
		//IL_2f71: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f7b: Expected O, but got Unknown
		//IL_2fac: Unknown result type (might be due to invalid IL or missing references)
		//IL_3059: Unknown result type (might be due to invalid IL or missing references)
		//IL_3063: Expected O, but got Unknown
		//IL_309f: Unknown result type (might be due to invalid IL or missing references)
		//IL_30a9: Expected O, but got Unknown
		//IL_320b: Unknown result type (might be due to invalid IL or missing references)
		//IL_3215: Expected O, but got Unknown
		//IL_3295: Unknown result type (might be due to invalid IL or missing references)
		//IL_345b: Unknown result type (might be due to invalid IL or missing references)
		//IL_3465: Expected O, but got Unknown
		//IL_34e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_36ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_36b5: Expected O, but got Unknown
		//IL_3735: Unknown result type (might be due to invalid IL or missing references)
		//IL_3897: Unknown result type (might be due to invalid IL or missing references)
		//IL_38a1: Expected O, but got Unknown
		//IL_395f: Unknown result type (might be due to invalid IL or missing references)
		//IL_3969: Expected O, but got Unknown
		//IL_3a49: Unknown result type (might be due to invalid IL or missing references)
		//IL_3a53: Expected O, but got Unknown
		//IL_3a70: Unknown result type (might be due to invalid IL or missing references)
		//IL_3aab: Unknown result type (might be due to invalid IL or missing references)
		//IL_3b61: Unknown result type (might be due to invalid IL or missing references)
		//IL_3b6b: Expected O, but got Unknown
		components = new Container();
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(UserControl5));
		uiPanel1 = new UIPanel();
		uiLabel6 = new UILabel();
		switchStart = new UISwitch();
		uiLabel4 = new UILabel();
		btnOutput = new UIButton();
		btnTurn = new UIButton();
		btnJian = new UIButton();
		btnJia = new UIButton();
		btnRefresh = new UIButton();
		btnOpenConfig = new UIButton();
		btnSaveConfig = new UIButton();
		btnReset = new UIButton();
		trackBarScale = new UITrackBar();
		uiAnalogMeter1 = new UIAnalogMeter();
		lblBright = new UILabel();
		btnLeft = new UIImageButton();
		btnRigth = new UIImageButton();
		btnButtom = new UIImageButton();
		btnTop = new UIImageButton();
		uiLabel3 = new UILabel();
		lblzoom = new UILabel();
		btnSelectScreen = new UIButton();
		cbxBackground = new UIComboBox();
		uiLabel2 = new UILabel();
		cbxSpeed = new UIComboBox();
		uiLabel12 = new UILabel();
		uiLine7 = new UILine();
		upDown = new UIIntegerUpDown();
		uiLabel7 = new UILabel();
		switchLight = new UISwitch();
		trackBarLiangDu = new UITrackBar();
		uiLine5 = new UILine();
		uiPanelDraw = new UIPanel();
		uiLine9 = new UILine();
		btnTwo = new UIButton();
		btnStop = new UIButton();
		btnStart = new UIButton();
		switchScreen = new UISwitch();
		uiLabel1 = new UILabel();
		cbmCom = new UIComboBox();
		uiLabel5 = new UILabel();
		timer1 = new Timer(components);
		((Control)uiPanel1).SuspendLayout();
		((ISupportInitialize)(object)btnLeft).BeginInit();
		((ISupportInitialize)(object)btnRigth).BeginInit();
		((ISupportInitialize)(object)btnButtom).BeginInit();
		((ISupportInitialize)(object)btnTop).BeginInit();
		((Control)this).SuspendLayout();
		((Control)uiPanel1).BackColor = Color.Transparent;
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel6);
		((Control)uiPanel1).Controls.Add((Control)(object)switchStart);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel4);
		((Control)uiPanel1).Controls.Add((Control)(object)btnOutput);
		((Control)uiPanel1).Controls.Add((Control)(object)btnTurn);
		((Control)uiPanel1).Controls.Add((Control)(object)btnJian);
		((Control)uiPanel1).Controls.Add((Control)(object)btnJia);
		((Control)uiPanel1).Controls.Add((Control)(object)btnRefresh);
		((Control)uiPanel1).Controls.Add((Control)(object)btnOpenConfig);
		((Control)uiPanel1).Controls.Add((Control)(object)btnSaveConfig);
		((Control)uiPanel1).Controls.Add((Control)(object)btnReset);
		((Control)uiPanel1).Controls.Add((Control)(object)trackBarScale);
		((Control)uiPanel1).Controls.Add((Control)(object)uiAnalogMeter1);
		((Control)uiPanel1).Controls.Add((Control)(object)lblBright);
		((Control)uiPanel1).Controls.Add((Control)(object)btnLeft);
		((Control)uiPanel1).Controls.Add((Control)(object)btnRigth);
		((Control)uiPanel1).Controls.Add((Control)(object)btnButtom);
		((Control)uiPanel1).Controls.Add((Control)(object)btnTop);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel3);
		((Control)uiPanel1).Controls.Add((Control)(object)lblzoom);
		((Control)uiPanel1).Controls.Add((Control)(object)btnSelectScreen);
		((Control)uiPanel1).Controls.Add((Control)(object)cbxBackground);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel2);
		((Control)uiPanel1).Controls.Add((Control)(object)cbxSpeed);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel12);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLine7);
		((Control)uiPanel1).Controls.Add((Control)(object)upDown);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel7);
		((Control)uiPanel1).Controls.Add((Control)(object)switchLight);
		((Control)uiPanel1).Controls.Add((Control)(object)trackBarLiangDu);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLine5);
		((Control)uiPanel1).Controls.Add((Control)(object)uiPanelDraw);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLine9);
		((Control)uiPanel1).Controls.Add((Control)(object)btnTwo);
		((Control)uiPanel1).Controls.Add((Control)(object)btnStop);
		((Control)uiPanel1).Controls.Add((Control)(object)btnStart);
		((Control)uiPanel1).Controls.Add((Control)(object)switchScreen);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel1);
		((Control)uiPanel1).Controls.Add((Control)(object)cbmCom);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel5);
		((Control)uiPanel1).Dock = (DockStyle)5;
		uiPanel1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel1).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel1).ForeColor = Color.Silver;
		((Control)uiPanel1).Location = new Point(0, 0);
		((Control)uiPanel1).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel1).MinimumSize = new Size(1, 1);
		((Control)uiPanel1).Name = "uiPanel1";
		uiPanel1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel1).Size = new Size(1208, 713);
		uiPanel1.Style = UIStyle.Black;
		((Control)uiPanel1).TabIndex = 0;
		((Control)uiPanel1).Text = null;
		uiPanel1.TextAlignment = (ContentAlignment)32;
		((Control)uiLabel6).BackColor = Color.Transparent;
		((Control)uiLabel6).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel6).ForeColor = Color.Silver;
		((Control)uiLabel6).Location = new Point(1017, 132);
		((Control)uiLabel6).Name = "uiLabel6";
		((Control)uiLabel6).Size = new Size(93, 28);
		uiLabel6.Style = UIStyle.Custom;
		((Control)uiLabel6).TabIndex = 72;
		((Control)uiLabel6).Text = "上电启动:";
		((Label)uiLabel6).TextAlign = (ContentAlignment)32;
		switchStart.Active = true;
		switchStart.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)switchStart).BackColor = Color.Transparent;
		((Control)switchStart).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)switchStart).Location = new Point(1111, 131);
		((Control)switchStart).MinimumSize = new Size(1, 1);
		((Control)switchStart).Name = "switchStart";
		((Control)switchStart).Size = new Size(75, 29);
		switchStart.Style = UIStyle.Black;
		((Control)switchStart).TabIndex = 71;
		((Control)switchStart).Text = "uiSwitch1";
		switchStart.ValueChanged += switchStart_ValueChanged;
		((Control)uiLabel4).BackColor = Color.Transparent;
		((Control)uiLabel4).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel4).ForeColor = Color.Silver;
		((Control)uiLabel4).Location = new Point(467, 655);
		((Control)uiLabel4).Name = "uiLabel4";
		((Control)uiLabel4).Size = new Size(73, 23);
		uiLabel4.Style = UIStyle.Custom;
		((Control)uiLabel4).TabIndex = 60;
		((Control)uiLabel4).Text = "垂直:";
		((Label)uiLabel4).TextAlign = (ContentAlignment)32;
		((Control)btnOutput).Cursor = Cursors.Hand;
		btnOutput.FillColor = Color.FromArgb(15, 40, 70);
		btnOutput.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnOutput.FillPressColor = Color.FromArgb(235, 243, 255);
		btnOutput.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnOutput).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnOutput.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnOutput.ForePressColor = Color.FromArgb(130, 130, 130);
		btnOutput.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOutput).Location = new Point(1037, 27);
		((Control)btnOutput).MinimumSize = new Size(1, 1);
		((Control)btnOutput).Name = "btnOutput";
		btnOutput.Radius = 26;
		btnOutput.RectColor = Color.FromArgb(130, 130, 130);
		btnOutput.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnOutput.RectPressColor = Color.FromArgb(130, 130, 130);
		btnOutput.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOutput).Size = new Size(74, 37);
		btnOutput.Style = UIStyle.Black;
		((Control)btnOutput).TabIndex = 70;
		((Control)btnOutput).Text = "导出";
		((Control)btnOutput).Click += btnOutput_Click;
		((Control)btnTurn).Cursor = Cursors.Hand;
		btnTurn.FillColor = Color.FromArgb(15, 40, 70);
		btnTurn.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnTurn.FillPressColor = Color.FromArgb(235, 243, 255);
		btnTurn.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnTurn).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnTurn.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnTurn.ForePressColor = Color.FromArgb(130, 130, 130);
		btnTurn.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTurn).Location = new Point(929, 27);
		((Control)btnTurn).MinimumSize = new Size(1, 1);
		((Control)btnTurn).Name = "btnTurn";
		btnTurn.Radius = 26;
		btnTurn.RectColor = Color.FromArgb(130, 130, 130);
		btnTurn.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnTurn.RectPressColor = Color.FromArgb(130, 130, 130);
		btnTurn.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTurn).Size = new Size(102, 37);
		btnTurn.Style = UIStyle.Black;
		((Control)btnTurn).TabIndex = 69;
		((Control)btnTurn).Text = "正转";
		((Control)btnTurn).Click += btnTurn_Click;
		((Control)btnJian).Cursor = Cursors.Hand;
		btnJian.FillColor = Color.FromArgb(15, 40, 70);
		btnJian.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJian.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJian.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJian).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJian.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJian.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJian.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJian).Location = new Point(160, 655);
		((Control)btnJian).Margin = new Padding(2);
		((Control)btnJian).MinimumSize = new Size(1, 1);
		((Control)btnJian).Name = "btnJian";
		btnJian.Radius = 26;
		btnJian.RectColor = Color.FromArgb(130, 130, 130);
		btnJian.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJian.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJian.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJian).Size = new Size(29, 23);
		btnJian.Style = UIStyle.Black;
		((Control)btnJian).TabIndex = 68;
		((Control)btnJian).Text = "-";
		((Control)btnJian).Click += btnJian_Click;
		((Control)btnJia).Cursor = Cursors.Hand;
		btnJia.FillColor = Color.FromArgb(15, 40, 70);
		btnJia.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJia.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJia.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJia).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJia.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJia.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJia.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJia).Location = new Point(388, 655);
		((Control)btnJia).Margin = new Padding(2);
		((Control)btnJia).MinimumSize = new Size(1, 1);
		((Control)btnJia).Name = "btnJia";
		btnJia.Radius = 26;
		btnJia.RectColor = Color.FromArgb(130, 130, 130);
		btnJia.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJia.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJia.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJia).Size = new Size(29, 23);
		btnJia.Style = UIStyle.Black;
		((Control)btnJia).TabIndex = 68;
		((Control)btnJia).Text = "+";
		((Control)btnJia).Click += btnJia_Click;
		((Control)btnRefresh).Cursor = Cursors.Hand;
		btnRefresh.FillColor = Color.FromArgb(15, 40, 70);
		btnRefresh.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnRefresh.FillPressColor = Color.FromArgb(235, 243, 255);
		btnRefresh.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnRefresh).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnRefresh.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnRefresh.ForePressColor = Color.FromArgb(130, 130, 130);
		btnRefresh.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRefresh).Location = new Point(853, 27);
		((Control)btnRefresh).MinimumSize = new Size(1, 1);
		((Control)btnRefresh).Name = "btnRefresh";
		btnRefresh.Radius = 26;
		btnRefresh.RectColor = Color.FromArgb(130, 130, 130);
		btnRefresh.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnRefresh.RectPressColor = Color.FromArgb(130, 130, 130);
		btnRefresh.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRefresh).Size = new Size(70, 37);
		btnRefresh.Style = UIStyle.Black;
		((Control)btnRefresh).TabIndex = 66;
		((Control)btnRefresh).Text = "刷新";
		((Control)btnRefresh).Click += btnRefresh_Click;
		((Control)btnOpenConfig).Cursor = Cursors.Hand;
		btnOpenConfig.FillColor = Color.FromArgb(15, 40, 70);
		btnOpenConfig.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnOpenConfig.FillPressColor = Color.FromArgb(235, 243, 255);
		btnOpenConfig.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnOpenConfig).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnOpenConfig.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.ForePressColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOpenConfig).Location = new Point(1117, 27);
		((Control)btnOpenConfig).MinimumSize = new Size(1, 1);
		((Control)btnOpenConfig).Name = "btnOpenConfig";
		btnOpenConfig.Radius = 26;
		btnOpenConfig.RectColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectPressColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOpenConfig).Size = new Size(74, 37);
		btnOpenConfig.Style = UIStyle.Black;
		((Control)btnOpenConfig).TabIndex = 66;
		((Control)btnOpenConfig).Text = "导入";
		((Control)btnOpenConfig).Click += btnOpenConfig_Click;
		((Control)btnSaveConfig).Cursor = Cursors.Hand;
		btnSaveConfig.FillColor = Color.FromArgb(15, 40, 70);
		btnSaveConfig.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnSaveConfig.FillPressColor = Color.FromArgb(235, 243, 255);
		btnSaveConfig.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnSaveConfig).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnSaveConfig.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnSaveConfig.ForePressColor = Color.FromArgb(130, 130, 130);
		btnSaveConfig.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSaveConfig).Location = new Point(766, 27);
		((Control)btnSaveConfig).MinimumSize = new Size(1, 1);
		((Control)btnSaveConfig).Name = "btnSaveConfig";
		btnSaveConfig.Radius = 26;
		btnSaveConfig.RectColor = Color.FromArgb(130, 130, 130);
		btnSaveConfig.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSaveConfig.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSaveConfig.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSaveConfig).Size = new Size(78, 37);
		btnSaveConfig.Style = UIStyle.Black;
		((Control)btnSaveConfig).TabIndex = 67;
		((Control)btnSaveConfig).Text = "保存";
		((Control)btnSaveConfig).Click += btnSaveConfig_Click;
		((Control)btnReset).Cursor = Cursors.Hand;
		btnReset.FillColor = Color.FromArgb(15, 40, 70);
		btnReset.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnReset.FillPressColor = Color.FromArgb(235, 243, 255);
		btnReset.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnReset).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnReset.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnReset.ForePressColor = Color.FromArgb(130, 130, 130);
		btnReset.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReset).Location = new Point(732, 651);
		((Control)btnReset).Margin = new Padding(2);
		((Control)btnReset).MinimumSize = new Size(1, 1);
		((Control)btnReset).Name = "btnReset";
		btnReset.Radius = 26;
		btnReset.RectColor = Color.FromArgb(130, 130, 130);
		btnReset.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnReset.RectPressColor = Color.FromArgb(130, 130, 130);
		btnReset.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReset).Size = new Size(70, 34);
		btnReset.Style = UIStyle.Black;
		((Control)btnReset).TabIndex = 65;
		((Control)btnReset).Text = "重置";
		((Control)btnReset).Click += btnReset_Click;
		trackBarScale.DisableColor = Color.Silver;
		trackBarScale.FillColor = Color.FromArgb(24, 24, 24);
		((Control)trackBarScale).Font = new Font("微软雅黑", 12f);
		((Control)trackBarScale).Location = new Point(192, 655);
		trackBarScale.Maximum = 1056;
		trackBarScale.Minimum = 32;
		((Control)trackBarScale).MinimumSize = new Size(1, 1);
		((Control)trackBarScale).Name = "trackBarScale";
		((Control)trackBarScale).Size = new Size(192, 25);
		trackBarScale.Style = UIStyle.Black;
		((Control)trackBarScale).TabIndex = 8;
		((Control)trackBarScale).Text = "uiTrackBar6";
		trackBarScale.Value = 1024;
		trackBarScale.ValueChanged += trackBarScale_ValueChanged;
		((Control)uiAnalogMeter1).BackColor = Color.Transparent;
		uiAnalogMeter1.BodyColor = Color.FromArgb(15, 40, 70);
		((Control)uiAnalogMeter1).Font = new Font("微软雅黑", 12f);
		((Control)uiAnalogMeter1).ForeColor = Color.Black;
		((Control)uiAnalogMeter1).Location = new Point(999, 440);
		uiAnalogMeter1.MaxValue = 360.0;
		((Control)uiAnalogMeter1).MinimumSize = new Size(1, 1);
		uiAnalogMeter1.MinValue = 0.0;
		((Control)uiAnalogMeter1).Name = "uiAnalogMeter1";
		uiAnalogMeter1.Renderer = null;
		((Control)uiAnalogMeter1).Size = new Size(187, 187);
		uiAnalogMeter1.Style = UIStyle.Black;
		((Control)uiAnalogMeter1).TabIndex = 52;
		((Control)uiAnalogMeter1).Text = "uiAnalogMeter1";
		uiAnalogMeter1.Value = 148.0;
		((Control)lblBright).Font = new Font("微软雅黑", 12f);
		((Control)lblBright).ForeColor = Color.Silver;
		((Control)lblBright).Location = new Point(1146, 314);
		((Control)lblBright).Name = "lblBright";
		((Control)lblBright).Size = new Size(52, 23);
		lblBright.Style = UIStyle.Custom;
		((Control)lblBright).TabIndex = 63;
		((Control)lblBright).Text = "255";
		((Label)lblBright).TextAlign = (ContentAlignment)16;
		((Control)btnLeft).Cursor = Cursors.Hand;
		((Control)btnLeft).Font = new Font("微软雅黑", 12f);
		((PictureBox)btnLeft).Image = (Image)(object)Resources._2023_09_06_141700;
		((Control)btnLeft).Location = new Point(667, 650);
		((Control)btnLeft).Name = "btnLeft";
		((Control)btnLeft).Size = new Size(27, 40);
		((PictureBox)btnLeft).TabIndex = 61;
		((PictureBox)btnLeft).TabStop = false;
		((Control)btnLeft).Text = null;
		((Control)btnLeft).Click += btnLeft_Click;
		((Control)btnRigth).Cursor = Cursors.Hand;
		((Control)btnRigth).Font = new Font("微软雅黑", 12f);
		((PictureBox)btnRigth).Image = (Image)(object)Resources._2023_09_06_141641;
		((Control)btnRigth).Location = new Point(700, 650);
		((Control)btnRigth).Name = "btnRigth";
		((Control)btnRigth).Size = new Size(30, 40);
		((PictureBox)btnRigth).TabIndex = 61;
		((PictureBox)btnRigth).TabStop = false;
		((Control)btnRigth).Text = null;
		((Control)btnRigth).Click += btnRigth_Click;
		((Control)btnButtom).Cursor = Cursors.Hand;
		((Control)btnButtom).Font = new Font("微软雅黑", 12f);
		((PictureBox)btnButtom).Image = (Image)(object)Resources._2023_09_06_135750;
		((Control)btnButtom).Location = new Point(539, 669);
		((Control)btnButtom).Name = "btnButtom";
		((Control)btnButtom).Size = new Size(43, 31);
		((PictureBox)btnButtom).TabIndex = 61;
		((PictureBox)btnButtom).TabStop = false;
		((Control)btnButtom).Text = null;
		((Control)btnButtom).Click += btnButtom_Click;
		((Control)btnTop).Cursor = Cursors.Hand;
		((Control)btnTop).Font = new Font("微软雅黑", 12f);
		((PictureBox)btnTop).Image = (Image)(object)Resources._2023_09_06_135731;
		((Control)btnTop).Location = new Point(539, 633);
		((Control)btnTop).Name = "btnTop";
		((Control)btnTop).Size = new Size(43, 31);
		((PictureBox)btnTop).TabIndex = 61;
		((PictureBox)btnTop).TabStop = false;
		((Control)btnTop).Text = null;
		((Control)btnTop).Click += btnTop_Click;
		((Control)uiLabel3).BackColor = Color.Transparent;
		((Control)uiLabel3).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel3).ForeColor = Color.Silver;
		((Control)uiLabel3).Location = new Point(103, 655);
		((Control)uiLabel3).Name = "uiLabel3";
		((Control)uiLabel3).Size = new Size(54, 23);
		uiLabel3.Style = UIStyle.Custom;
		((Control)uiLabel3).TabIndex = 60;
		((Control)uiLabel3).Text = "缩放:";
		((Label)uiLabel3).TextAlign = (ContentAlignment)16;
		((Control)lblzoom).BackColor = Color.Transparent;
		((Control)lblzoom).Font = new Font("微软雅黑", 12f);
		((Control)lblzoom).ForeColor = Color.Silver;
		((Control)lblzoom).Location = new Point(414, 655);
		((Control)lblzoom).Name = "lblzoom";
		((Control)lblzoom).Size = new Size(62, 23);
		lblzoom.Style = UIStyle.Custom;
		((Control)lblzoom).TabIndex = 59;
		((Control)lblzoom).Text = "1024";
		((Label)lblzoom).TextAlign = (ContentAlignment)16;
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
		((Control)btnSelectScreen).Location = new Point(2, 645);
		((Control)btnSelectScreen).MinimumSize = new Size(1, 1);
		((Control)btnSelectScreen).Name = "btnSelectScreen";
		btnSelectScreen.Radius = 25;
		btnSelectScreen.RectColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.RectSelectedColor = Color.FromArgb(130, 130, 130);
		btnSelectScreen.ShowTips = true;
		((Control)btnSelectScreen).Size = new Size(101, 40);
		btnSelectScreen.Style = UIStyle.Black;
		((Control)btnSelectScreen).TabIndex = 57;
		((Control)btnSelectScreen).Text = "选择区域";
		btnSelectScreen.TipsColor = Color.Transparent;
		((Control)btnSelectScreen).Click += btnSelectScreen_Click;
		cbxBackground.DataSource = null;
		cbxBackground.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxBackground.FillColor = Color.White;
		((Control)cbxBackground).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxBackground.Items.AddRange(new object[3] { "暖色", "标准", "冷色" });
		((Control)cbxBackground).Location = new Point(1064, 209);
		((Control)cbxBackground).Margin = new Padding(4);
		((Control)cbxBackground).MinimumSize = new Size(62, 0);
		((Control)cbxBackground).Name = "cbxBackground";
		((Control)cbxBackground).Padding = new Padding(0, 0, 42, 2);
		cbxBackground.Radius = 15;
		cbxBackground.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxBackground).Size = new Size(93, 29);
		cbxBackground.Style = UIStyle.Black;
		((Control)cbxBackground).TabIndex = 56;
		((Control)cbxBackground).Text = "标准";
		cbxBackground.TextAlignment = (ContentAlignment)16;
		cbxBackground.SelectedIndexChanged += cbxBackground_SelectedIndexChanged;
		((Control)uiLabel2).BackColor = Color.Transparent;
		((Control)uiLabel2).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel2).ForeColor = Color.Silver;
		((Control)uiLabel2).Location = new Point(1017, 209);
		((Control)uiLabel2).Name = "uiLabel2";
		((Control)uiLabel2).Size = new Size(53, 29);
		uiLabel2.Style = UIStyle.Custom;
		((Control)uiLabel2).TabIndex = 55;
		((Control)uiLabel2).Text = "色调:";
		((Label)uiLabel2).TextAlign = (ContentAlignment)32;
		cbxSpeed.DataSource = null;
		cbxSpeed.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxSpeed.FillColor = Color.White;
		((Control)cbxSpeed).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxSpeed.Items.AddRange(new object[2] { "750", "900" });
		((Control)cbxSpeed).Location = new Point(880, 209);
		((Control)cbxSpeed).Margin = new Padding(4);
		((Control)cbxSpeed).MinimumSize = new Size(62, 0);
		((Control)cbxSpeed).Name = "cbxSpeed";
		((Control)cbxSpeed).Padding = new Padding(0, 0, 42, 2);
		cbxSpeed.Radius = 15;
		cbxSpeed.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxSpeed).Size = new Size(93, 29);
		cbxSpeed.Style = UIStyle.Black;
		((Control)cbxSpeed).TabIndex = 54;
		((Control)cbxSpeed).Text = "750";
		cbxSpeed.TextAlignment = (ContentAlignment)16;
		cbxSpeed.SelectedIndexChanged += cbxSpeed_SelectedIndexChanged;
		((Control)uiLabel12).BackColor = Color.Transparent;
		((Control)uiLabel12).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel12).ForeColor = Color.Silver;
		((Control)uiLabel12).Location = new Point(816, 209);
		((Control)uiLabel12).Name = "uiLabel12";
		((Control)uiLabel12).Size = new Size(68, 29);
		uiLabel12.Style = UIStyle.Custom;
		((Control)uiLabel12).TabIndex = 53;
		((Control)uiLabel12).Text = "转速:";
		((Label)uiLabel12).TextAlign = (ContentAlignment)32;
		((Control)uiLine7).BackColor = Color.Black;
		uiLine7.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine7).ForeColor = Color.Silver;
		uiLine7.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine7).Location = new Point(826, 405);
		((Control)uiLine7).MinimumSize = new Size(2, 2);
		((Control)uiLine7).Name = "uiLine7";
		((Control)uiLine7).Size = new Size(360, 29);
		uiLine7.Style = UIStyle.Black;
		((Control)uiLine7).TabIndex = 51;
		((Control)uiLine7).Text = "角度调节";
		uiLine7.TextAlign = (ContentAlignment)16;
		upDown.FillColor = Color.FromArgb(24, 24, 24);
		((Control)upDown).Font = new Font("微软雅黑", 12f);
		((Control)upDown).ForeColor = Color.Silver;
		((Control)upDown).Location = new Point(857, 500);
		((Control)upDown).Margin = new Padding(4, 5, 4, 5);
		upDown.Maximum = 360;
		upDown.Minimum = 0;
		((Control)upDown).MinimumSize = new Size(100, 0);
		((Control)upDown).Name = "upDown";
		upDown.RectColor = Color.FromArgb(130, 130, 130);
		((Control)upDown).Size = new Size(116, 29);
		upDown.Style = UIStyle.Black;
		((Control)upDown).TabIndex = 50;
		((Control)upDown).Text = "uiIntegerUpDown1";
		upDown.TextAlignment = (ContentAlignment)32;
		upDown.Value = 148;
		upDown.ValueChanged += upDown_ValueChanged;
		((Control)uiLabel7).BackColor = Color.Transparent;
		((Control)uiLabel7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel7).ForeColor = Color.Silver;
		((Control)uiLabel7).Location = new Point(811, 131);
		((Control)uiLabel7).Name = "uiLabel7";
		((Control)uiLabel7).Size = new Size(124, 28);
		uiLabel7.Style = UIStyle.Custom;
		((Control)uiLabel7).TabIndex = 49;
		((Control)uiLabel7).Text = "呼吸灯开关:";
		((Label)uiLabel7).TextAlign = (ContentAlignment)32;
		switchLight.Active = true;
		switchLight.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)switchLight).BackColor = Color.Transparent;
		((Control)switchLight).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)switchLight).Location = new Point(941, 131);
		((Control)switchLight).MinimumSize = new Size(1, 1);
		((Control)switchLight).Name = "switchLight";
		((Control)switchLight).Size = new Size(75, 29);
		switchLight.Style = UIStyle.Black;
		((Control)switchLight).TabIndex = 48;
		((Control)switchLight).Text = "uiSwitch1";
		switchLight.ValueChanged += switchLight_ValueChanged;
		((Control)trackBarLiangDu).BackColor = Color.Black;
		trackBarLiangDu.DisableColor = Color.Silver;
		trackBarLiangDu.FillColor = Color.FromArgb(24, 24, 24);
		((Control)trackBarLiangDu).Font = new Font("微软雅黑", 12f);
		((Control)trackBarLiangDu).Location = new Point(823, 316);
		trackBarLiangDu.Maximum = 255;
		((Control)trackBarLiangDu).MinimumSize = new Size(1, 1);
		((Control)trackBarLiangDu).Name = "trackBarLiangDu";
		((Control)trackBarLiangDu).Size = new Size(319, 25);
		trackBarLiangDu.Style = UIStyle.Black;
		((Control)trackBarLiangDu).TabIndex = 47;
		((Control)trackBarLiangDu).Text = "uiTrackBar5";
		trackBarLiangDu.Value = 255;
		trackBarLiangDu.ValueChanged += trackBarLiangDu_ValueChanged;
		((Control)uiLine5).BackColor = Color.Black;
		uiLine5.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine5).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine5).ForeColor = Color.Silver;
		uiLine5.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine5).Location = new Point(823, 281);
		((Control)uiLine5).MinimumSize = new Size(2, 2);
		((Control)uiLine5).Name = "uiLine5";
		((Control)uiLine5).Size = new Size(360, 29);
		uiLine5.Style = UIStyle.Black;
		((Control)uiLine5).TabIndex = 46;
		((Control)uiLine5).Text = "亮度调节";
		uiLine5.TextAlign = (ContentAlignment)16;
		((Control)uiPanelDraw).BackgroundImage = (Image)componentResourceManager.GetObject("uiPanelDraw.BackgroundImage");
		uiPanelDraw.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanelDraw).Font = new Font("微软雅黑", 12f);
		((Control)uiPanelDraw).ForeColor = Color.Silver;
		((Control)uiPanelDraw).Location = new Point(9, 112);
		((Control)uiPanelDraw).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanelDraw).MinimumSize = new Size(1, 1);
		((Control)uiPanelDraw).Name = "uiPanelDraw";
		uiPanelDraw.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanelDraw).Size = new Size(782, 500);
		uiPanelDraw.Style = UIStyle.Black;
		((Control)uiPanelDraw).TabIndex = 42;
		((Control)uiPanelDraw).Text = null;
		uiPanelDraw.TextAlignment = (ContentAlignment)32;
		((Control)uiPanelDraw).Paint += new PaintEventHandler(uiPanelDraw_Paint);
		uiLine9.Direction = UILine.LineDirection.Vertical;
		uiLine9.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine9).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine9).ForeColor = Color.Silver;
		uiLine9.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine9).Location = new Point(792, 112);
		((Control)uiLine9).MinimumSize = new Size(2, 2);
		((Control)uiLine9).Name = "uiLine9";
		((Control)uiLine9).Size = new Size(25, 578);
		uiLine9.Style = UIStyle.Black;
		((Control)uiLine9).TabIndex = 41;
		((Control)uiLine9).Text = "1080";
		((Control)btnTwo).BackColor = Color.Transparent;
		((Control)btnTwo).Cursor = Cursors.Hand;
		btnTwo.FillColor = Color.FromArgb(15, 40, 70);
		btnTwo.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnTwo.FillPressColor = Color.FromArgb(235, 243, 255);
		btnTwo.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnTwo).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnTwo.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnTwo.ForePressColor = Color.FromArgb(130, 130, 130);
		btnTwo.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTwo).Location = new Point(653, 27);
		((Control)btnTwo).Margin = new Padding(2);
		((Control)btnTwo).MinimumSize = new Size(1, 1);
		((Control)btnTwo).Name = "btnTwo";
		btnTwo.Radius = 26;
		btnTwo.RectColor = Color.FromArgb(130, 130, 130);
		btnTwo.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnTwo.RectPressColor = Color.FromArgb(130, 130, 130);
		btnTwo.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTwo).Size = new Size(103, 37);
		btnTwo.Style = UIStyle.Black;
		((Control)btnTwo).TabIndex = 38;
		((Control)btnTwo).Text = "匹配遥控器";
		((Control)btnTwo).Click += btnTwo_Click;
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
		((Control)btnStop).Location = new Point(543, 27);
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
		((Control)btnStop).TabIndex = 39;
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
		((Control)btnStart).Location = new Point(432, 27);
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
		((Control)btnStart).TabIndex = 40;
		((Control)btnStart).Text = "启动";
		((Control)btnStart).Click += btnStart_Click;
		switchScreen.Active = true;
		switchScreen.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)switchScreen).BackColor = Color.Transparent;
		((Control)switchScreen).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)switchScreen).Location = new Point(298, 27);
		((Control)switchScreen).MinimumSize = new Size(1, 1);
		((Control)switchScreen).Name = "switchScreen";
		((Control)switchScreen).Size = new Size(75, 29);
		switchScreen.Style = UIStyle.Black;
		((Control)switchScreen).TabIndex = 36;
		((Control)switchScreen).Text = "uiSwitch2";
		switchScreen.ValueChanged += switchScreen_ValueChanged;
		((Control)uiLabel1).BackColor = Color.Transparent;
		((Control)uiLabel1).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel1).ForeColor = Color.Silver;
		((Control)uiLabel1).Location = new Point(129, 29);
		((Control)uiLabel1).Name = "uiLabel1";
		((Control)uiLabel1).Size = new Size(173, 23);
		uiLabel1.Style = UIStyle.Custom;
		((Control)uiLabel1).TabIndex = 37;
		((Control)uiLabel1).Text = "投屏:";
		((Label)uiLabel1).TextAlign = (ContentAlignment)64;
		((Control)cbmCom).BackColor = Color.Black;
		cbmCom.DataSource = null;
		cbmCom.DropDownStyle = UIDropDownStyle.DropDownList;
		cbmCom.FillColor = Color.White;
		((Control)cbmCom).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)cbmCom).Location = new Point(24, 27);
		((Control)cbmCom).Margin = new Padding(4);
		((Control)cbmCom).MinimumSize = new Size(62, 0);
		((Control)cbmCom).Name = "cbmCom";
		((Control)cbmCom).Padding = new Padding(0, 0, 42, 2);
		cbmCom.Radius = 15;
		cbmCom.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbmCom).Size = new Size(98, 29);
		cbmCom.Style = UIStyle.Black;
		((Control)cbmCom).TabIndex = 1;
		cbmCom.TextAlignment = (ContentAlignment)16;
		cbmCom.SelectedIndexChanged += cbmCom_SelectedIndexChanged;
		((Control)uiLabel5).BackColor = Color.Transparent;
		((Control)uiLabel5).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel5).ForeColor = Color.Silver;
		((Control)uiLabel5).Location = new Point(575, 657);
		((Control)uiLabel5).Name = "uiLabel5";
		((Control)uiLabel5).Size = new Size(88, 23);
		uiLabel5.Style = UIStyle.Custom;
		((Control)uiLabel5).TabIndex = 60;
		((Control)uiLabel5).Text = "水平:";
		((Label)uiLabel5).TextAlign = (ContentAlignment)32;
		timer1.Interval = 3000;
		timer1.Tick += timer1_Tick;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 15f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.Black;
		((Control)this).Controls.Add((Control)(object)uiPanel1);
		((Control)this).ForeColor = Color.Transparent;
		((Control)this).Name = "UserControl5";
		((Control)this).Size = new Size(1208, 713);
		((Control)this).VisibleChanged += UserControl5_VisibleChanged;
		((Control)uiPanel1).ResumeLayout(false);
		((ISupportInitialize)(object)btnLeft).EndInit();
		((ISupportInitialize)(object)btnRigth).EndInit();
		((ISupportInitialize)(object)btnButtom).EndInit();
		((ISupportInitialize)(object)btnTop).EndInit();
		((Control)this).ResumeLayout(false);
	}
}
