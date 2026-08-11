using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;

namespace MetaStudio;

public class UserControl2 : UserControl
{
	private SplitContainer sp;

	private UserControl1 u1;

	private Form1 frm;

	private int direction = 0;

	private GridData data;

	private int curRow = 0;

	private int curCol = 0;

	private string key = string.Empty;

	private string nextKey = string.Empty;

	private int ovly_cmd = 0;

	private int centerX = 0;

	private int centerY = 0;

	private int distance = 256;

	private int liangDu = 127;

	private int attenuation = 0;

	private int angle = -1;

	private int motodire = 0;

	private IContainer components = null;

	private UIPanel uiPanel1;

	private UIButton uiButton7;

	private UIButton btnReset;

	private UIButton btnSingleStop;

	private UIButton btnSingleStart;

	private UIPanel uiPanel2;

	private UIIntegerUpDown upDown;

	private UILabel uiLabel7;

	private UISwitch uiSwitch1;

	public UILabel lblBright;

	private UITrackBar uiTrackBar5;

	private UIPanel uiPanel3;

	private UILabel lblOri_y;

	private UILabel lblori_x;

	private UITrackBar uiTrackBar2;

	private UILabel lblzoom;

	private UILine uiLine2;

	private UILine uiLine1;

	private UITrackBar uiTrackBar6;

	private UILine uiLine6;

	private UIAnalogMeter uiAnalogMeter1;

	private UILine uiLine7;

	private UILine uiLine5;

	private UISwitch uiSwitch2;

	private UILabel uiLabel1;

	private UILabel uiLabel2;

	private UIComboBox cbxSpeed;

	private UILabel uiLabel12;

	private UIButton btnTurn;

	private UIPanel uiPanel4;

	private UIComboBox cbxDirection;

	private UILabel uiLabel3;

	private UILabel lblHy;

	private UIIntegerUpDown upDownY;

	private UILabel lblSx;

	private UIIntegerUpDown upDownX;

	private UILine uiLine3;

	private UILabel uiLabel6;

	private UISwitch uiSwitch3;

	private UIIntegerUpDown upDownLiangDu;

	private UIIntegerUpDown upDownWidth;

	private UILabel uiLabel8;

	private UILabel lblDir;

	private UILabel uiLabel10;

	private UIIntegerUpDown upDownAtten;

	private UITrackBar uiTrackBar1;

	private UIComboBox cbxMode;

	private UILabel uiLabel11;

	private UIPanel uiPanel5;

	private UIComboBox cbxBackground;

	private UILabel uiLabel13;

	private UIButton btnRefresh;

	private UIComboBox cbxNum;

	private UIButton uiButton1;

	private UIButton btnJianX;

	private UIButton btnjianY;

	private UIButton btnJiaY;

	private UIButton btnJiaX;

	public UserControl2(Form1 _frm, SplitContainer sp, UserControl1 u1)
	{
		InitializeComponent();
		this.sp = sp;
		this.u1 = u1;
		frm = _frm;
	}

	public void SetID(string id, GridData _data)
	{
		try
		{
			if (ConstData.isSmall)
			{
				uiTrackBar6.ReadOnly = true;
			}
			else
			{
				uiTrackBar6.ReadOnly = false;
			}
			data = _data;
			foreach (KeyValuePair<string, string> item in data.Dic)
			{
				if (item.Value == id)
				{
					key = item.Key;
					curRow = int.Parse(key.Split(new char[1] { '-' })[0]);
					curCol = int.Parse(key.Split(new char[1] { '-' })[1]);
					break;
				}
			}
			cbxNum.Items.Clear();
			for (int i = 1; i <= data.Dic.Count; i++)
			{
				cbxNum.Items.Add((object)i);
			}
			((Control)cbxNum).Text = id;
			GetPageData();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void cbxNum_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			ConstData.curOperID = Convert.ToInt32(((Control)cbxNum).Text) + 1;
			GetPageData();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void GetPageData()
	{
		cbxMode.SelectedIndex = 1;
		cbxDirection.SelectedIndex = 0;
		cbxDirection_SelectedIndexChanged(null, null);
		Task task = new Task(delegate
		{
			GetRotorValue();
			Thread.Sleep(20);
			GetStatorValue();
		});
		task.Start();
	}

	private void GetRotorValue()
	{
		SPHelper.SendTORotor(ConstData.curOperID, 1, 16, 0);
		Thread.Sleep(20);
		SPHelper.SendTORotor(ConstData.curOperID, 1, 25, 0);
		Thread.Sleep(20);
		SPHelper.SendTORotor(ConstData.curOperID, 1, 22, 0);
	}

	private void GetStatorValue()
	{
		SPHelper.SendTOStator(ConstData.curOperID, 1, 32, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(ConstData.curOperID, 1, 33, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(ConstData.curOperID, 1, 49, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(ConstData.curOperID, 1, 3, 0);
	}

	public void GetSerData(byte[] buf)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Expected O, but got Unknown
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Expected O, but got Unknown
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Expected O, but got Unknown
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Expected O, but got Unknown
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Expected O, but got Unknown
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Expected O, but got Unknown
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Expected O, but got Unknown
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Expected O, but got Unknown
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Expected O, but got Unknown
		MethodInvoker val = null;
		MethodInvoker val2 = null;
		MethodInvoker val3 = null;
		MethodInvoker val4 = null;
		MethodInvoker val5 = null;
		MethodInvoker val6 = null;
		try
		{
			if (!SPHelper.CheckHead(buf) || buf.Length != 26)
			{
				return;
			}
			if (buf[4] == 129 && buf[7] == 128)
			{
				if (buf[16] == 16)
				{
					int num = SPHelper.ConvetInt(buf, 20);
					angle = (int)((double)num / 2.8444444444444446) + 1;
					if (((Control)this).IsHandleCreated)
					{
						if (val == null)
						{
							val = (MethodInvoker)delegate
							{
								upDown.Value = angle;
								uiAnalogMeter1.Value = angle;
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val);
					}
				}
				if (buf[16] == 25)
				{
					MethodInvoker val7 = null;
					int data = SPHelper.ConvetInt(buf, 20);
					if (((Control)this).IsHandleCreated)
					{
						if (val7 == null)
						{
							val7 = (MethodInvoker)delegate
							{
								uiTrackBar5.Value = data;
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val7);
					}
				}
				if (buf[16] == 22)
				{
					MethodInvoker val8 = null;
					int data2 = SPHelper.ConvetInt(buf, 20);
					if (((Control)this).IsHandleCreated)
					{
						if (val8 == null)
						{
							val8 = (MethodInvoker)delegate
							{
								motodire = data2;
								if (data2 == 1)
								{
									if (ConstData.versionType == "0")
									{
										((Control)btnTurn).Text = "正转";
									}
									else if (ConstData.versionType == "1")
									{
										((Control)btnTurn).Text = "Clockwise";
									}
								}
								else if (data2 == 0)
								{
									if (ConstData.versionType == "0")
									{
										((Control)btnTurn).Text = "反转";
									}
									else if (ConstData.versionType == "1")
									{
										((Control)btnTurn).Text = "Anticlockwise";
									}
								}
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val8);
					}
				}
				if (buf[16] == 95)
				{
				}
				if (buf[16] == 99 || buf[16] == 105 || buf[16] == 111 || buf[16] == 117)
				{
					centerX = SPHelper.ConvetInt(buf, 20);
					if (((Control)this).IsHandleCreated)
					{
						if (val2 == null)
						{
							val2 = (MethodInvoker)delegate
							{
								upDownX.Value = centerX;
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val2);
					}
				}
				if (buf[16] == 100 || buf[16] == 106 || buf[16] == 112 || buf[16] == 118)
				{
					centerY = SPHelper.ConvetInt(buf, 20);
					if (((Control)this).IsHandleCreated)
					{
						if (val3 == null)
						{
							val3 = (MethodInvoker)delegate
							{
								upDownY.Value = centerY;
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val3);
					}
				}
				if (buf[16] == 102 || buf[16] == 108 || buf[16] == 114 || buf[16] == 120)
				{
					distance = SPHelper.ConvetInt(buf, 20);
					if (((Control)this).IsHandleCreated)
					{
						if (val4 == null)
						{
							val4 = (MethodInvoker)delegate
							{
								upDownWidth.Value = distance;
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val4);
					}
				}
				if (buf[16] == 103 || buf[16] == 109 || buf[16] == 115 || buf[16] == 121)
				{
					attenuation = SPHelper.ConvetInt(buf, 20);
					if (((Control)this).IsHandleCreated)
					{
						if (val5 == null)
						{
							val5 = (MethodInvoker)delegate
							{
								upDownAtten.Value = attenuation;
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val5);
					}
				}
				if (buf[16] == 104 || buf[16] == 110 || buf[16] == 116 || buf[16] == 122)
				{
					liangDu = SPHelper.ConvetInt(buf, 20);
					if (((Control)this).IsHandleCreated)
					{
						if (val6 == null)
						{
							val6 = (MethodInvoker)delegate
							{
								upDownLiangDu.Value = liangDu;
							};
						}
						((Control)this).BeginInvoke((Delegate)(object)val6);
					}
				}
			}
			if (buf[4] != 129 || buf[7] != 0)
			{
				return;
			}
			if (buf[16] == 32)
			{
				MethodInvoker val9 = null;
				int data3 = SPHelper.ConvetInt(buf, 20);
				if (((Control)this).IsHandleCreated)
				{
					if (val9 == null)
					{
						val9 = (MethodInvoker)delegate
						{
							uiTrackBar1.Value = data3;
							((Control)lblori_x).Text = data3.ToString();
						};
					}
					((Control)this).BeginInvoke((Delegate)(object)val9);
				}
			}
			if (buf[16] == 33)
			{
				MethodInvoker val10 = null;
				int data4 = SPHelper.ConvetInt(buf, 20);
				if (((Control)this).IsHandleCreated)
				{
					if (val10 == null)
					{
						val10 = (MethodInvoker)delegate
						{
							uiTrackBar2.Value = data4;
							((Control)lblOri_y).Text = data4.ToString();
						};
					}
					((Control)this).BeginInvoke((Delegate)(object)val10);
				}
			}
			if (buf[16] == 49)
			{
				MethodInvoker val11 = null;
				int data5 = SPHelper.ConvetInt(buf, 20);
				if (((Control)this).IsHandleCreated)
				{
					if (val11 == null)
					{
						val11 = (MethodInvoker)delegate
						{
							uiTrackBar6.Value = (int)((double)data5 / 1.0);
							((Control)lblzoom).Text = uiTrackBar6.Value.ToString();
						};
					}
					((Control)this).BeginInvoke((Delegate)(object)val11);
				}
			}
			if (buf[16] == 242)
			{
				ovly_cmd = SPHelper.ConvetInt(buf, 20);
			}
			if (buf[16] != 3)
			{
				return;
			}
			MethodInvoker val12 = null;
			int data6 = SPHelper.ConvetInt(buf, 20);
			if (!((Control)this).IsHandleCreated)
			{
				return;
			}
			if (val12 == null)
			{
				val12 = (MethodInvoker)delegate
				{
					if (data6 == 419)
					{
						cbxSpeed.SelectedIndex = 0;
					}
					else if (data6 == 461)
					{
						cbxSpeed.SelectedIndex = 1;
					}
					else if (data6 == 525)
					{
						cbxSpeed.SelectedIndex = 2;
					}
				};
			}
			((Control)this).BeginInvoke((Delegate)(object)val12);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void uiButton7_Click(object sender, EventArgs e)
	{
		((Control)sp).Visible = false;
		((Control)u1).Visible = true;
		((Control)this).Visible = false;
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
				ConfigAngle(value);
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public void ConfigAngle(int value)
	{
		int num = (int)((double)value * 2.8444444444444446);
		SPHelper.SendTORotor(ConstData.curOperID, 2, 16, num);
	}

	private void uiSwitch2_ValueChanged(object sender, bool value)
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

	private void uiSwitch1_ValueChanged(object sender, bool value)
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

	private void btnSingleStart_Click(object sender, EventArgs e)
	{
		SPHelper.SendTOStator(ConstData.curOperID, 2, 1, 0);
	}

	private void btnSingleStop_Click(object sender, EventArgs e)
	{
		SPHelper.SendTOStator(ConstData.curOperID, 2, 1, 1);
	}

	private void uiTrackBar5_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			((Control)lblBright).Text = uiTrackBar5.Value.ToString();
			int value = uiTrackBar5.Value;
			SPHelper.SendTORotor(ConstData.curOperID, 2, 25, value);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void cbxSpeed_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			switch (cbxSpeed.SelectedIndex)
			{
			case 0:
				MetaTool.SetDeviceSpeed(ConstData.curOperID, 750);
				break;
			case 1:
				MetaTool.SetDeviceSpeed(ConstData.curOperID, 900);
				break;
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void uiTrackBar6_ValueChanged(object sender, EventArgs e)
	{
	}

	public void ScaleStator(int Id, Point point, int m_width, int m_height, int scaleValue)
	{
		try
		{
			int num = (scaleValue << 16) | scaleValue;
			SPHelper.SendTOStator(Id, 2, 44, num);
			int num2 = m_width;
			int num3 = m_height;
			if (m_width < 40)
			{
				num2 = 40;
			}
			if (m_height < 40)
			{
				num3 = 40;
			}
			SPHelper.Factor_Sclr(Id, (ushort)ConstData.Diameter, (ushort)ConstData.Diameter, scaleValue, scaleValue);
			SPHelper.SendTOStator(Id, 2, 43, 0);
			SPHelper.SendTOStator(Id, 2, 43, 1);
			SPHelper.SendTOStator(Id, 2, 43, 0);
			SPHelper.SendTOStator(Id, 2, 42, 0);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public void ScaleStator(int Id, Point p_out, int width_in, int heigth_in, int width_out, int heigth_out)
	{
		try
		{
			int num = (width_out << 16) | heigth_out;
			SPHelper.SendTOStator(Id, 2, 44, num);
			int num2 = width_out;
			int num3 = heigth_out;
			if (width_out < 40)
			{
				num2 = 40;
			}
			if (heigth_out < 40)
			{
				num3 = 40;
			}
			SPHelper.Factor_Sclr(Id, width_in, heigth_in, width_out, heigth_out);
			SPHelper.SendTOStator(Id, 2, 43, 0);
			SPHelper.SendTOStator(Id, 2, 43, 1);
			SPHelper.SendTOStator(Id, 2, 43, 0);
			SPHelper.SendTOStator(Id, 2, 42, 0);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void btnRefresh_Click(object sender, EventArgs e)
	{
		GetPageData();
	}

	private void btnTurn_Click_1(object sender, EventArgs e)
	{
		btnSingleStop_Click(null, null);
		if (((Control)btnTurn).Text == "反转")
		{
			((Control)btnTurn).Text = "正转";
			MetaTool.MotoDirct(ConstData.curOperID, 1);
			upDown.Value = 148;
		}
		else if (((Control)btnTurn).Text == "正转")
		{
			((Control)btnTurn).Text = "反转";
			MetaTool.MotoDirct(ConstData.curOperID, 0);
			upDown.Value = 59;
		}
		if (((Control)btnTurn).Text == "Anticlockwise")
		{
			((Control)btnTurn).Text = "Clockwise";
			MetaTool.MotoDirct(ConstData.curOperID, 1);
			upDown.Value = 148;
		}
		else if (((Control)btnTurn).Text == "Clockwise")
		{
			((Control)btnTurn).Text = "Anticlockwise";
			MetaTool.MotoDirct(ConstData.curOperID, 0);
			upDown.Value = 59;
		}
		btnSingleStart_Click(null, null);
	}

	private void btnReset_Click(object sender, EventArgs e)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		string text = string.Empty;
		string text2 = string.Empty;
		if (ConstData.versionType == "0")
		{
			text = "确认执行重置操作(Y/N)？";
			text2 = "系统提示";
		}
		else if (ConstData.versionType == "1")
		{
			text = "Are you sure to execute(Y/N)？";
			text2 = "System Prompt";
		}
		if ((int)MessageBox.Show(text, text2, (MessageBoxButtons)4, (MessageBoxIcon)64) == 6)
		{
			uiTrackBar1.Value = 0;
			uiTrackBar2.Value = 0;
			uiTrackBar5.Value = 255;
			MetaTool.MotoDirct(ConstData.curOperID, 0);
			upDown.Value = 59;
			ResetScale(ConstData.curOperID);
			MetaTool.SetVideoOutputEn(ConstData.curOperID);
			RegisterHelper.CloseFusion(ConstData.curOperID);
			ScreenHelper.CutScreenTOStator(ConstData.curOperID, new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height, 1, 1, iscenter: true);
		}
	}

	public void ResetScale(int id)
	{
		SPHelper.SendTOStator(id, 2, 44, 67109888);
		SPHelper.Factor_Sclr(id, (ushort)ConstData.Diameter, (ushort)ConstData.Diameter, 1024, 1024);
		SPHelper.SendTOStator(id, 2, 43, 0);
		SPHelper.SendTOStator(id, 2, 43, 1);
		SPHelper.SendTOStator(id, 2, 43, 0);
		SPHelper.SendTOStator(id, 2, 42, 0);
	}

	public void SetVideoOutputEn(int id)
	{
		SPHelper.SendTOStator(id, 2, 41, 1);
	}

	private void uiTrackBar1_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			int value = uiTrackBar1.Value;
			((Control)lblori_x).Text = value.ToString();
			if (!((double)value + ConstData.Diameter > (double)ConstData.Ori_Width))
			{
				SPHelper.SendTOStator(ConstData.curOperID, 2, 32, value);
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void uiTrackBar2_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			int value = uiTrackBar2.Value;
			((Control)lblOri_y).Text = value.ToString();
			if (!((double)value + ConstData.Diameter > 1080.0))
			{
				SPHelper.SendTOStator(ConstData.curOperID, 2, 33, value);
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void cbxDirection_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			direction = cbxDirection.SelectedIndex;
			int mode = cbxMode.SelectedIndex;
			SPHelper.SendTORotor(ConstData.curOperID, 1, 95, 0);
			Thread.Sleep(50);
			if (direction == 0)
			{
				SPHelper.SendTORotor(ConstData.curOperID, 1, 99, 0);
				Thread.Sleep(10);
				SPHelper.SendTORotor(ConstData.curOperID, 1, 100, 0);
			}
			else if (direction == 1)
			{
				SPHelper.SendTORotor(ConstData.curOperID, 1, 105, 0);
				Thread.Sleep(10);
				SPHelper.SendTORotor(ConstData.curOperID, 1, 106, 0);
			}
			else if (direction == 2)
			{
				SPHelper.SendTORotor(ConstData.curOperID, 1, 111, 0);
				Thread.Sleep(10);
				SPHelper.SendTORotor(ConstData.curOperID, 1, 112, 0);
			}
			else if (direction == 3)
			{
				SPHelper.SendTORotor(ConstData.curOperID, 1, 117, 0);
				Thread.Sleep(10);
				SPHelper.SendTORotor(ConstData.curOperID, 1, 118, 0);
			}
			if (ovly_cmd == 32768)
			{
				mode = 0;
				cbxMode.SelectedIndex = 0;
			}
			OpenSwitch(mode);
			EnableReg(ConstData.curOperID, direction, mode);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void OpenSwitch(int mode)
	{
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		MethodInvoker val = null;
		int value = 0;
		switch (mode)
		{
		case 0:
			if (direction == 0)
			{
				value = (ovly_cmd & 0x8000) >> 15;
			}
			else if (direction == 1)
			{
				value = (ovly_cmd & 0x2000) >> 13;
			}
			else if (direction == 2)
			{
				value = (ovly_cmd & 0x800) >> 11;
			}
			else if (direction == 3)
			{
				value = (ovly_cmd & 0x200) >> 9;
			}
			break;
		case 1:
			if (direction == 0)
			{
				value = (ovly_cmd & 0x4000) >> 14;
			}
			else if (direction == 1)
			{
				value = (ovly_cmd & 0x1000) >> 12;
			}
			else if (direction == 2)
			{
				value = (ovly_cmd & 0x400) >> 10;
			}
			else if (direction == 3)
			{
				value = (ovly_cmd & 0x100) >> 8;
			}
			break;
		}
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		if (val == null)
		{
			val = (MethodInvoker)delegate
			{
				if (value == 0)
				{
					uiSwitch3.Active = false;
				}
				else if (value == 1)
				{
					uiSwitch3.Active = true;
				}
			};
		}
		((Control)this).BeginInvoke((Delegate)(object)val);
	}

	private void uiSwitch3_ValueChanged(object sender, bool value)
	{
		int selectedIndex = cbxMode.SelectedIndex;
		if (!value)
		{
			switch (selectedIndex)
			{
			case 0:
				RegisterHelper.CloseFusion(ConstData.curOperID);
				break;
			case 1:
				CloseBottomFusion(ConstData.curOperID, direction);
				break;
			}
		}
		else
		{
			switch (selectedIndex)
			{
			case 0:
				RegisterHelper.OpenTopFusion(ConstData.curOperID);
				break;
			case 1:
				OpenBottomFusion(ConstData.curOperID, direction);
				break;
			}
		}
		EnableReg(ConstData.curOperID, direction, selectedIndex);
	}

	public void CloseBottomFusion(int id, int direction)
	{
		SPHelper.SendTOStator(id, 1, 242, 0);
		Thread.Sleep(30);
		int num = 0;
		switch (direction)
		{
		case 0:
			num = ovly_cmd & 0xE;
			SPHelper.SendTOStator(id, 2, 242, num);
			Console.WriteLine("上方CloseBottomFusion" + id);
			break;
		case 1:
			num = ovly_cmd & 0xB;
			SPHelper.SendTOStator(id, 2, 242, num);
			Console.WriteLine("下方CloseBottomFusion" + id);
			break;
		case 2:
			num = ovly_cmd & 7;
			SPHelper.SendTOStator(id, 2, 242, num);
			Console.WriteLine("左边CloseBottomFusion" + id);
			break;
		case 3:
			num = ovly_cmd & 0xD;
			SPHelper.SendTOStator(id, 2, 242, num);
			Console.WriteLine("右边CloseBottomFusion" + id);
			break;
		}
	}

	public void OpenBottomFusion(int id, int direction)
	{
		SPHelper.SendTOStator(id, 1, 242, 0);
		Thread.Sleep(30);
		int num = 0;
		switch (direction)
		{
		case 0:
			num = ovly_cmd & 0xF;
			num |= 1;
			SPHelper.SendTOStator(id, 2, 242, num);
			Console.WriteLine("右边OpenBottomFusion" + id);
			break;
		case 1:
			num = ovly_cmd & 0xF;
			num |= 4;
			SPHelper.SendTOStator(id, 2, 242, num);
			Console.WriteLine("下方OpenBottomFusion" + id);
			break;
		case 2:
			num = ovly_cmd & 0xF;
			num |= 8;
			SPHelper.SendTOStator(id, 2, 242, num);
			Console.WriteLine("左边OpenBottomFusion" + id);
			break;
		case 3:
			num = ovly_cmd & 0xF;
			num |= 2;
			SPHelper.SendTOStator(id, 2, 242, num);
			Console.WriteLine("右边OpenBottomFusion" + id);
			break;
		}
	}

	public void EnableReg(int id, int direction, int mode)
	{
		SPHelper.SendTORotor(id, 2, 96, 1280);
		SPHelper.SendTORotor(id, 2, 97, 1280);
		SPHelper.SendTORotor(id, 2, 98, 512);
		switch (mode)
		{
		case 0:
			SPHelper.SendTORotor(id, 2, 101, 512);
			SPHelper.SendTORotor(id, 2, 102, 80);
			SPHelper.SendTORotor(id, 2, 103, 32);
			SPHelper.SendTORotor(id, 2, 104, 0);
			SPHelper.SendTORotor(id, 2, 107, 512);
			SPHelper.SendTORotor(id, 2, 108, 80);
			SPHelper.SendTORotor(id, 2, 109, 32);
			SPHelper.SendTORotor(id, 2, 110, 0);
			SPHelper.SendTORotor(id, 2, 113, 512);
			SPHelper.SendTORotor(id, 2, 114, 80);
			SPHelper.SendTORotor(id, 2, 115, 32);
			SPHelper.SendTORotor(id, 2, 116, 0);
			SPHelper.SendTORotor(id, 2, 119, 512);
			SPHelper.SendTORotor(id, 2, 120, 80);
			SPHelper.SendTORotor(id, 2, 121, 32);
			SPHelper.SendTORotor(id, 2, 122, 0);
			break;
		case 1:
			switch (direction)
			{
			case 0:
				SPHelper.SendTORotor(id, 2, 101, 512);
				SPHelper.SendTORotor(id, 2, 102, 128);
				SPHelper.SendTORotor(id, 2, 103, 22);
				SPHelper.SendTORotor(id, 2, 104, 0);
				break;
			case 1:
				SPHelper.SendTORotor(id, 2, 107, 512);
				SPHelper.SendTORotor(id, 2, 108, 128);
				SPHelper.SendTORotor(id, 2, 109, 22);
				SPHelper.SendTORotor(id, 2, 110, 0);
				break;
			case 2:
				SPHelper.SendTORotor(id, 2, 113, 512);
				SPHelper.SendTORotor(id, 2, 114, 128);
				SPHelper.SendTORotor(id, 2, 115, 22);
				SPHelper.SendTORotor(id, 2, 116, 0);
				break;
			case 3:
				SPHelper.SendTORotor(id, 2, 119, 512);
				SPHelper.SendTORotor(id, 2, 120, 128);
				SPHelper.SendTORotor(id, 2, 121, 22);
				SPHelper.SendTORotor(id, 2, 122, 0);
				break;
			}
			break;
		}
	}

	private void upDownX_ValueChanged(object sender, int value)
	{
		int num = value - centerX;
		if (direction == 0 || direction == 4)
		{
			SPHelper.SendTORotor(ConstData.curOperID, 2, 99, centerX + num);
		}
		else if (direction == 1 || direction == 5)
		{
			SPHelper.SendTORotor(ConstData.curOperID, 2, 105, centerX + num);
		}
		else if (direction == 2 || direction == 6)
		{
			SPHelper.SendTORotor(ConstData.curOperID, 2, 111, centerX + num);
		}
		else if (direction == 3 || direction == 7)
		{
			SPHelper.SendTORotor(ConstData.curOperID, 2, 117, centerX + num);
		}
	}

	private void upDownY_ValueChanged(object sender, int value)
	{
		int num = value - centerY;
		if (direction == 0 || direction == 4)
		{
			SPHelper.SendTORotor(ConstData.curOperID, 2, 100, centerY + num);
		}
		else if (direction == 1 || direction == 5)
		{
			SPHelper.SendTORotor(ConstData.curOperID, 2, 106, centerY + num);
		}
		else if (direction == 2 || direction == 6)
		{
			SPHelper.SendTORotor(ConstData.curOperID, 2, 112, centerY + num);
		}
		else if (direction == 3 || direction == 7)
		{
			SPHelper.SendTORotor(ConstData.curOperID, 2, 118, centerY + num);
		}
	}

	private void upDownWidth_ValueChanged(object sender, int value)
	{
	}

	private void upDownLiangDu_ValueChanged(object sender, int value)
	{
	}

	private void upDownAtten_ValueChanged(object sender, int value)
	{
	}

	private void cbxMode_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (cbxMode.SelectedIndex)
		{
		case 0:
			((Control)lblDir).Visible = false;
			((Control)lblSx).Visible = false;
			((Control)lblHy).Visible = false;
			((Control)cbxDirection).Visible = false;
			((Control)upDownX).Visible = false;
			((Control)upDownY).Visible = false;
			break;
		case 1:
			((Control)lblDir).Visible = true;
			((Control)lblSx).Visible = true;
			((Control)lblHy).Visible = true;
			((Control)cbxDirection).Visible = true;
			((Control)upDownX).Visible = true;
			((Control)upDownY).Visible = true;
			break;
		}
	}

	private void cbxBackground_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = cbxBackground.SelectedIndex;
		MetaTool.SetBackground(ConstData.curOperID, selectedIndex);
	}

	private void btnJianX_Click(object sender, EventArgs e)
	{
		uiTrackBar1.Value -= 1;
	}

	private void btnJiaX_Click(object sender, EventArgs e)
	{
		uiTrackBar1.Value += 1;
	}

	private void btnjianY_Click(object sender, EventArgs e)
	{
		uiTrackBar2.Value -= 1;
	}

	private void btnJiaY_Click(object sender, EventArgs e)
	{
		uiTrackBar2.Value += 1;
	}

	public void Translate(int type)
	{
		switch (type)
		{
		case 0:
			((Control)uiButton1).Text = "调试";
			((Control)uiButton7).Text = "返回";
			((Control)btnSingleStart).Text = "启动";
			((Control)btnSingleStop).Text = "停止";
			((Control)btnReset).Text = "重置";
			((Control)uiLabel2).Text = "设备编号:";
			((Control)uiLine1).Text = "开始横坐标";
			((Control)uiLine2).Text = "开始纵坐标";
			((Control)uiLine5).Text = "亮度调节";
			((Control)uiLine7).Text = "角度调节";
			((Control)uiLine3).Text = "联屏融合";
			((Control)uiLabel6).Text = "融合开关:";
			((Control)uiLabel11).Text = "融合模式:";
			((Control)lblDir).Text = "融合方向:";
			uiSwitch3.ActiveText = "开";
			uiSwitch3.InActiveText = "关";
			cbxMode.Items.Clear();
			((Control)cbxMode).Text = "顶层融合";
			cbxMode.Items.AddRange(new object[2] { "顶层融合", "底层融合" });
			cbxDirection.Items.Clear();
			((Control)cbxDirection).Text = "上";
			cbxDirection.Items.AddRange(new object[4] { "上", "下", "左", "右" });
			((Control)lblSx).Text = "水平距离:";
			((Control)lblHy).Text = "垂直距离:";
			((Control)btnTurn).Text = "反转";
			break;
		case 1:
			((Control)uiButton1).Text = "Debug";
			((Control)uiButton7).Text = "Back";
			((Control)btnSingleStart).Text = "Start";
			((Control)btnSingleStop).Text = "Stop";
			((Control)btnReset).Text = "Reset";
			((Control)uiLabel2).Text = "Device Num:";
			((Control)uiLine1).Text = "Starting X-axis";
			((Control)uiLine2).Text = "Starting Y-axis";
			((Control)uiLine5).Text = "Light Adjust";
			((Control)uiLine7).Text = "Angle Adjust";
			((Control)uiLine3).Text = "Mix Screen";
			((Control)uiLabel6).Text = "Mixing Enable:";
			((Control)uiLabel11).Text = "Mixing Mode:";
			((Control)lblDir).Text = "Mixing Direction:";
			uiSwitch3.ActiveText = "Open";
			uiSwitch3.InActiveText = "Close";
			cbxMode.Items.Clear();
			((Control)cbxMode).Text = "Top Mixing";
			cbxMode.Items.AddRange(new object[2] { "Top Mixing", "Bottom Mixing" });
			cbxDirection.Items.Clear();
			((Control)cbxDirection).Text = "Top";
			cbxDirection.Items.AddRange(new object[4] { "Top", "Bottom", "Left", "Right" });
			((Control)lblSx).Text = "Mixing X-axis:";
			((Control)lblHy).Text = "Mixing Y-axis:";
			((Control)btnTurn).Text = "Anticlockwise";
			break;
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
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Expected O, but got Unknown
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_0623: Expected O, but got Unknown
		//IL_06a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fa: Expected O, but got Unknown
		//IL_0853: Unknown result type (might be due to invalid IL or missing references)
		//IL_088e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0974: Unknown result type (might be due to invalid IL or missing references)
		//IL_097e: Expected O, but got Unknown
		//IL_09cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0adf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae9: Expected O, but got Unknown
		//IL_0cbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc5: Expected O, but got Unknown
		//IL_0cf9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e4a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e54: Expected O, but got Unknown
		//IL_0e87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f65: Expected O, but got Unknown
		//IL_0f9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1096: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a0: Expected O, but got Unknown
		//IL_115e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1168: Expected O, but got Unknown
		//IL_122a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1234: Expected O, but got Unknown
		//IL_12f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_12fb: Expected O, but got Unknown
		//IL_132f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1446: Unknown result type (might be due to invalid IL or missing references)
		//IL_1450: Expected O, but got Unknown
		//IL_1484: Unknown result type (might be due to invalid IL or missing references)
		//IL_159f: Unknown result type (might be due to invalid IL or missing references)
		//IL_15a9: Expected O, but got Unknown
		//IL_1675: Unknown result type (might be due to invalid IL or missing references)
		//IL_167f: Expected O, but got Unknown
		//IL_16cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1706: Unknown result type (might be due to invalid IL or missing references)
		//IL_17c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_17ca: Expected O, but got Unknown
		//IL_1878: Unknown result type (might be due to invalid IL or missing references)
		//IL_1882: Expected O, but got Unknown
		//IL_18b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_19c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_19cd: Expected O, but got Unknown
		//IL_1a04: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b36: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b40: Expected O, but got Unknown
		//IL_1bfe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c08: Expected O, but got Unknown
		//IL_1cc3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ccd: Expected O, but got Unknown
		//IL_1dcc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dd6: Expected O, but got Unknown
		//IL_1e35: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e70: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f29: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f33: Expected O, but got Unknown
		//IL_1fe3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fed: Expected O, but got Unknown
		//IL_2129: Unknown result type (might be due to invalid IL or missing references)
		//IL_2133: Expected O, but got Unknown
		//IL_21b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_2386: Unknown result type (might be due to invalid IL or missing references)
		//IL_2390: Expected O, but got Unknown
		//IL_2410: Unknown result type (might be due to invalid IL or missing references)
		//IL_255b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2565: Expected O, but got Unknown
		//IL_25ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_25e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_26be: Unknown result type (might be due to invalid IL or missing references)
		//IL_26c8: Expected O, but got Unknown
		//IL_2787: Unknown result type (might be due to invalid IL or missing references)
		//IL_2791: Expected O, but got Unknown
		//IL_28dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_28e7: Expected O, but got Unknown
		//IL_2918: Unknown result type (might be due to invalid IL or missing references)
		//IL_29f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a01: Expected O, but got Unknown
		//IL_2afd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b07: Expected O, but got Unknown
		//IL_2bfc: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c06: Expected O, but got Unknown
		//IL_2d24: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d2e: Expected O, but got Unknown
		//IL_2d62: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e69: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e73: Expected O, but got Unknown
		//IL_2f35: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f3f: Expected O, but got Unknown
		//IL_31a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_31ae: Expected O, but got Unknown
		//IL_31dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_32c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_32cb: Expected O, but got Unknown
		//IL_3399: Unknown result type (might be due to invalid IL or missing references)
		//IL_33a3: Expected O, but got Unknown
		//IL_34ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_34f7: Expected O, but got Unknown
		//IL_3576: Unknown result type (might be due to invalid IL or missing references)
		//IL_373c: Unknown result type (might be due to invalid IL or missing references)
		//IL_3746: Expected O, but got Unknown
		//IL_37c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_398f: Unknown result type (might be due to invalid IL or missing references)
		//IL_3999: Expected O, but got Unknown
		//IL_3a19: Unknown result type (might be due to invalid IL or missing references)
		//IL_3bdf: Unknown result type (might be due to invalid IL or missing references)
		//IL_3be9: Expected O, but got Unknown
		//IL_3c65: Unknown result type (might be due to invalid IL or missing references)
		//IL_3d9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_3da6: Expected O, but got Unknown
		//IL_3e5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_3e64: Expected O, but got Unknown
		//IL_3f1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_3f26: Expected O, but got Unknown
		//IL_3ff4: Unknown result type (might be due to invalid IL or missing references)
		//IL_3ffe: Expected O, but got Unknown
		//IL_40d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_40e3: Expected O, but got Unknown
		//IL_41ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_41b7: Expected O, but got Unknown
		//IL_427f: Unknown result type (might be due to invalid IL or missing references)
		//IL_4289: Expected O, but got Unknown
		//IL_437c: Unknown result type (might be due to invalid IL or missing references)
		//IL_4386: Expected O, but got Unknown
		//IL_445a: Unknown result type (might be due to invalid IL or missing references)
		//IL_4464: Expected O, but got Unknown
		//IL_4557: Unknown result type (might be due to invalid IL or missing references)
		//IL_4561: Expected O, but got Unknown
		//IL_4664: Unknown result type (might be due to invalid IL or missing references)
		//IL_466e: Expected O, but got Unknown
		//IL_47e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_47f3: Expected O, but got Unknown
		//IL_4873: Unknown result type (might be due to invalid IL or missing references)
		//IL_4a39: Unknown result type (might be due to invalid IL or missing references)
		//IL_4a43: Expected O, but got Unknown
		//IL_4ac3: Unknown result type (might be due to invalid IL or missing references)
		//IL_4c89: Unknown result type (might be due to invalid IL or missing references)
		//IL_4c93: Expected O, but got Unknown
		//IL_4d10: Unknown result type (might be due to invalid IL or missing references)
		//IL_4ed6: Unknown result type (might be due to invalid IL or missing references)
		//IL_4ee0: Expected O, but got Unknown
		//IL_4f5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_50b0: Unknown result type (might be due to invalid IL or missing references)
		uiPanel1 = new UIPanel();
		uiButton1 = new UIButton();
		cbxNum = new UIComboBox();
		cbxBackground = new UIComboBox();
		uiLabel13 = new UILabel();
		uiPanel4 = new UIPanel();
		uiPanel5 = new UIPanel();
		upDownAtten = new UIIntegerUpDown();
		uiLabel3 = new UILabel();
		uiLabel10 = new UILabel();
		uiLabel8 = new UILabel();
		upDownWidth = new UIIntegerUpDown();
		upDownLiangDu = new UIIntegerUpDown();
		lblSx = new UILabel();
		cbxMode = new UIComboBox();
		uiLabel6 = new UILabel();
		upDownX = new UIIntegerUpDown();
		upDownY = new UIIntegerUpDown();
		uiSwitch3 = new UISwitch();
		lblHy = new UILabel();
		uiLine3 = new UILine();
		cbxDirection = new UIComboBox();
		uiLabel11 = new UILabel();
		lblDir = new UILabel();
		btnRefresh = new UIButton();
		btnTurn = new UIButton();
		cbxSpeed = new UIComboBox();
		uiLabel12 = new UILabel();
		uiLabel2 = new UILabel();
		uiPanel2 = new UIPanel();
		uiLine7 = new UILine();
		uiLine5 = new UILine();
		uiAnalogMeter1 = new UIAnalogMeter();
		upDown = new UIIntegerUpDown();
		lblBright = new UILabel();
		uiTrackBar5 = new UITrackBar();
		uiPanel3 = new UIPanel();
		uiSwitch2 = new UISwitch();
		uiLabel1 = new UILabel();
		btnjianY = new UIButton();
		btnJiaY = new UIButton();
		btnJiaX = new UIButton();
		btnJianX = new UIButton();
		lblOri_y = new UILabel();
		uiLabel7 = new UILabel();
		lblori_x = new UILabel();
		uiSwitch1 = new UISwitch();
		uiTrackBar2 = new UITrackBar();
		lblzoom = new UILabel();
		uiLine2 = new UILine();
		uiTrackBar1 = new UITrackBar();
		uiLine1 = new UILine();
		uiTrackBar6 = new UITrackBar();
		uiLine6 = new UILine();
		btnReset = new UIButton();
		btnSingleStop = new UIButton();
		btnSingleStart = new UIButton();
		uiButton7 = new UIButton();
		((Control)uiPanel1).SuspendLayout();
		((Control)uiPanel4).SuspendLayout();
		((Control)uiPanel5).SuspendLayout();
		((Control)uiPanel2).SuspendLayout();
		((Control)uiPanel3).SuspendLayout();
		((Control)this).SuspendLayout();
		((ScrollableControl)uiPanel1).AutoScroll = true;
		((Control)uiPanel1).AutoSize = true;
		((Control)uiPanel1).BackColor = Color.Black;
		((Control)uiPanel1).Controls.Add((Control)(object)uiButton1);
		((Control)uiPanel1).Controls.Add((Control)(object)cbxNum);
		((Control)uiPanel1).Controls.Add((Control)(object)cbxBackground);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel13);
		((Control)uiPanel1).Controls.Add((Control)(object)uiPanel4);
		((Control)uiPanel1).Controls.Add((Control)(object)btnRefresh);
		((Control)uiPanel1).Controls.Add((Control)(object)btnTurn);
		((Control)uiPanel1).Controls.Add((Control)(object)cbxSpeed);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel12);
		((Control)uiPanel1).Controls.Add((Control)(object)uiLabel2);
		((Control)uiPanel1).Controls.Add((Control)(object)uiPanel2);
		((Control)uiPanel1).Controls.Add((Control)(object)uiPanel3);
		((Control)uiPanel1).Controls.Add((Control)(object)btnReset);
		((Control)uiPanel1).Controls.Add((Control)(object)btnSingleStop);
		((Control)uiPanel1).Controls.Add((Control)(object)btnSingleStart);
		((Control)uiPanel1).Controls.Add((Control)(object)uiButton7);
		((Control)uiPanel1).Dock = (DockStyle)5;
		uiPanel1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel1).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel1).ForeColor = Color.Silver;
		((Control)uiPanel1).Location = new Point(0, 0);
		((Control)uiPanel1).Margin = new Padding(5, 6, 5, 6);
		((Control)uiPanel1).MinimumSize = new Size(1, 1);
		((Control)uiPanel1).Name = "uiPanel1";
		uiPanel1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel1).Size = new Size(1331, 811);
		uiPanel1.Style = UIStyle.Black;
		((Control)uiPanel1).TabIndex = 0;
		((Control)uiPanel1).Text = null;
		uiPanel1.TextAlignment = (ContentAlignment)32;
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
		((Control)uiButton1).Location = new Point(621, 13);
		((Control)uiButton1).Margin = new Padding(2);
		((Control)uiButton1).MinimumSize = new Size(1, 1);
		((Control)uiButton1).Name = "uiButton1";
		uiButton1.Radius = 26;
		uiButton1.RectColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectHoverColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectPressColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton1).Size = new Size(64, 29);
		uiButton1.Style = UIStyle.Black;
		((Control)uiButton1).TabIndex = 60;
		((Control)uiButton1).Text = "调试";
		((Control)uiButton1).Visible = false;
		cbxNum.DataSource = null;
		cbxNum.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxNum.FillColor = Color.White;
		((Control)cbxNum).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxNum.Items.AddRange(new object[4] { "1", "2", "3", "4" });
		((Control)cbxNum).Location = new Point(445, 11);
		((Control)cbxNum).Margin = new Padding(4);
		((Control)cbxNum).MinimumSize = new Size(62, 0);
		((Control)cbxNum).Name = "cbxNum";
		((Control)cbxNum).Padding = new Padding(0, 0, 42, 2);
		cbxNum.Radius = 15;
		cbxNum.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxNum).Size = new Size(62, 29);
		cbxNum.Style = UIStyle.Black;
		((Control)cbxNum).TabIndex = 59;
		((Control)cbxNum).Text = "1";
		cbxNum.TextAlignment = (ContentAlignment)16;
		cbxNum.SelectedIndexChanged += cbxNum_SelectedIndexChanged;
		((Control)cbxBackground).BackColor = Color.Transparent;
		cbxBackground.DataSource = null;
		cbxBackground.FillColor = Color.White;
		((Control)cbxBackground).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxBackground.Items.AddRange(new object[3] { "暖色", "标准", "冷色" });
		((Control)cbxBackground).Location = new Point(768, 14);
		((Control)cbxBackground).Margin = new Padding(4);
		((Control)cbxBackground).MinimumSize = new Size(62, 0);
		((Control)cbxBackground).Name = "cbxBackground";
		((Control)cbxBackground).Padding = new Padding(0, 0, 42, 2);
		cbxBackground.Radius = 15;
		cbxBackground.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxBackground).Size = new Size(80, 29);
		cbxBackground.Style = UIStyle.Black;
		((Control)cbxBackground).TabIndex = 58;
		((Control)cbxBackground).Text = "标准";
		cbxBackground.TextAlignment = (ContentAlignment)16;
		((Control)cbxBackground).Visible = false;
		cbxBackground.SelectedIndexChanged += cbxBackground_SelectedIndexChanged;
		((Control)uiLabel13).BackColor = Color.Transparent;
		((Control)uiLabel13).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel13).ForeColor = Color.Silver;
		((Control)uiLabel13).Location = new Point(726, 14);
		((Control)uiLabel13).Name = "uiLabel13";
		((Control)uiLabel13).Size = new Size(53, 29);
		uiLabel13.Style = UIStyle.Black;
		((Control)uiLabel13).TabIndex = 57;
		((Control)uiLabel13).Text = "色调:";
		((Label)uiLabel13).TextAlign = (ContentAlignment)16;
		((Control)uiLabel13).Visible = false;
		((Control)uiPanel4).Controls.Add((Control)(object)uiPanel5);
		((Control)uiPanel4).Controls.Add((Control)(object)lblSx);
		((Control)uiPanel4).Controls.Add((Control)(object)cbxMode);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel6);
		((Control)uiPanel4).Controls.Add((Control)(object)upDownX);
		((Control)uiPanel4).Controls.Add((Control)(object)upDownY);
		((Control)uiPanel4).Controls.Add((Control)(object)uiSwitch3);
		((Control)uiPanel4).Controls.Add((Control)(object)lblHy);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLine3);
		((Control)uiPanel4).Controls.Add((Control)(object)cbxDirection);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel11);
		((Control)uiPanel4).Controls.Add((Control)(object)lblDir);
		uiPanel4.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel4).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel4).ForeColor = Color.Silver;
		((Control)uiPanel4).Location = new Point(12, 425);
		((Control)uiPanel4).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel4).MinimumSize = new Size(1, 1);
		((Control)uiPanel4).Name = "uiPanel4";
		uiPanel4.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel4).Size = new Size(943, 285);
		uiPanel4.Style = UIStyle.Black;
		((Control)uiPanel4).TabIndex = 37;
		((Control)uiPanel4).Text = null;
		uiPanel4.TextAlignment = (ContentAlignment)32;
		((Control)uiPanel5).Controls.Add((Control)(object)upDownAtten);
		((Control)uiPanel5).Controls.Add((Control)(object)uiLabel3);
		((Control)uiPanel5).Controls.Add((Control)(object)uiLabel10);
		((Control)uiPanel5).Controls.Add((Control)(object)uiLabel8);
		((Control)uiPanel5).Controls.Add((Control)(object)upDownWidth);
		((Control)uiPanel5).Controls.Add((Control)(object)upDownLiangDu);
		uiPanel5.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel5).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel5).ForeColor = Color.Silver;
		((Control)uiPanel5).Location = new Point(509, 0);
		((Control)uiPanel5).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel5).MinimumSize = new Size(1, 1);
		((Control)uiPanel5).Name = "uiPanel5";
		uiPanel5.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel5).Size = new Size(434, 218);
		uiPanel5.Style = UIStyle.Black;
		((Control)uiPanel5).TabIndex = 38;
		((Control)uiPanel5).Text = null;
		uiPanel5.TextAlignment = (ContentAlignment)32;
		((Control)uiPanel5).Visible = false;
		upDownAtten.FillColor = Color.FromArgb(24, 24, 24);
		((Control)upDownAtten).Font = new Font("微软雅黑", 12f);
		((Control)upDownAtten).ForeColor = Color.Silver;
		((Control)upDownAtten).Location = new Point(296, 141);
		((Control)upDownAtten).Margin = new Padding(4, 5, 4, 5);
		((Control)upDownAtten).MinimumSize = new Size(100, 0);
		((Control)upDownAtten).Name = "upDownAtten";
		upDownAtten.RectColor = Color.FromArgb(130, 130, 130);
		((Control)upDownAtten).Size = new Size(116, 29);
		upDownAtten.Style = UIStyle.Black;
		((Control)upDownAtten).TabIndex = 27;
		((Control)upDownAtten).Text = "uiIntegerUpDown1";
		upDownAtten.TextAlignment = (ContentAlignment)32;
		upDownAtten.Value = 80;
		((Control)upDownAtten).Visible = false;
		upDownAtten.ValueChanged += upDownAtten_ValueChanged;
		((Control)uiLabel3).BackColor = Color.Transparent;
		((Control)uiLabel3).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel3).ForeColor = Color.Silver;
		((Control)uiLabel3).Location = new Point(216, 49);
		((Control)uiLabel3).Name = "uiLabel3";
		((Control)uiLabel3).Size = new Size(79, 23);
		uiLabel3.Style = UIStyle.Black;
		((Control)uiLabel3).TabIndex = 0;
		((Control)uiLabel3).Text = "融合亮度:";
		((Label)uiLabel3).TextAlign = (ContentAlignment)16;
		((Control)uiLabel3).Visible = false;
		((Control)uiLabel10).BackColor = Color.Transparent;
		((Control)uiLabel10).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel10).ForeColor = Color.Silver;
		((Control)uiLabel10).Location = new Point(216, 141);
		((Control)uiLabel10).Name = "uiLabel10";
		((Control)uiLabel10).Size = new Size(79, 23);
		uiLabel10.Style = UIStyle.Black;
		((Control)uiLabel10).TabIndex = 24;
		((Control)uiLabel10).Text = "衰减系数:";
		((Label)uiLabel10).TextAlign = (ContentAlignment)16;
		((Control)uiLabel10).Visible = false;
		((Control)uiLabel8).BackColor = Color.Transparent;
		((Control)uiLabel8).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel8).ForeColor = Color.Silver;
		((Control)uiLabel8).Location = new Point(216, 93);
		((Control)uiLabel8).Name = "uiLabel8";
		((Control)uiLabel8).Size = new Size(79, 23);
		uiLabel8.Style = UIStyle.Black;
		((Control)uiLabel8).TabIndex = 0;
		((Control)uiLabel8).Text = "融合宽度:";
		((Label)uiLabel8).TextAlign = (ContentAlignment)16;
		((Control)uiLabel8).Visible = false;
		upDownWidth.FillColor = Color.FromArgb(24, 24, 24);
		((Control)upDownWidth).Font = new Font("微软雅黑", 12f);
		((Control)upDownWidth).ForeColor = Color.Silver;
		((Control)upDownWidth).Location = new Point(296, 87);
		((Control)upDownWidth).Margin = new Padding(4, 5, 4, 5);
		upDownWidth.Maximum = 360;
		upDownWidth.Minimum = 0;
		((Control)upDownWidth).MinimumSize = new Size(100, 0);
		((Control)upDownWidth).Name = "upDownWidth";
		upDownWidth.RectColor = Color.FromArgb(130, 130, 130);
		((Control)upDownWidth).Size = new Size(116, 29);
		upDownWidth.Style = UIStyle.Black;
		((Control)upDownWidth).TabIndex = 21;
		((Control)upDownWidth).Text = "uiIntegerUpDown1";
		upDownWidth.TextAlignment = (ContentAlignment)32;
		upDownWidth.Value = 16;
		((Control)upDownWidth).Visible = false;
		upDownWidth.ValueChanged += upDownWidth_ValueChanged;
		upDownLiangDu.FillColor = Color.FromArgb(24, 24, 24);
		((Control)upDownLiangDu).Font = new Font("微软雅黑", 12f);
		((Control)upDownLiangDu).ForeColor = Color.Silver;
		((Control)upDownLiangDu).Location = new Point(296, 43);
		((Control)upDownLiangDu).Margin = new Padding(4, 5, 4, 5);
		upDownLiangDu.Maximum = 360;
		upDownLiangDu.Minimum = 0;
		((Control)upDownLiangDu).MinimumSize = new Size(100, 0);
		((Control)upDownLiangDu).Name = "upDownLiangDu";
		upDownLiangDu.RectColor = Color.FromArgb(130, 130, 130);
		((Control)upDownLiangDu).Size = new Size(116, 29);
		upDownLiangDu.Style = UIStyle.Black;
		((Control)upDownLiangDu).TabIndex = 21;
		((Control)upDownLiangDu).Text = "uiIntegerUpDown1";
		upDownLiangDu.TextAlignment = (ContentAlignment)32;
		upDownLiangDu.Value = 218;
		((Control)upDownLiangDu).Visible = false;
		upDownLiangDu.ValueChanged += upDownLiangDu_ValueChanged;
		((Control)lblSx).BackColor = Color.Transparent;
		((Control)lblSx).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)lblSx).ForeColor = Color.Silver;
		((Control)lblSx).Location = new Point(260, 123);
		((Control)lblSx).Name = "lblSx";
		((Control)lblSx).Size = new Size(116, 23);
		lblSx.Style = UIStyle.Black;
		((Control)lblSx).TabIndex = 22;
		((Control)lblSx).Text = "水平距离:";
		((Label)lblSx).TextAlign = (ContentAlignment)64;
		cbxMode.DataSource = null;
		cbxMode.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxMode.FillColor = Color.White;
		((Control)cbxMode).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxMode.Items.AddRange(new object[2] { "顶层融合", "底层融合" });
		((Control)cbxMode).Location = new Point(139, 117);
		((Control)cbxMode).Margin = new Padding(4, 5, 4, 5);
		((Control)cbxMode).MinimumSize = new Size(63, 0);
		((Control)cbxMode).Name = "cbxMode";
		((Control)cbxMode).Padding = new Padding(0, 0, 30, 2);
		cbxMode.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxMode).Size = new Size(118, 29);
		cbxMode.Style = UIStyle.Black;
		((Control)cbxMode).TabIndex = 27;
		((Control)cbxMode).Text = "顶层融合";
		cbxMode.TextAlignment = (ContentAlignment)16;
		cbxMode.SelectedIndexChanged += cbxMode_SelectedIndexChanged;
		((Control)uiLabel6).BackColor = Color.Transparent;
		((Control)uiLabel6).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel6).ForeColor = Color.Silver;
		((Control)uiLabel6).Location = new Point(28, 54);
		((Control)uiLabel6).Name = "uiLabel6";
		((Control)uiLabel6).Size = new Size(103, 28);
		uiLabel6.Style = UIStyle.Black;
		((Control)uiLabel6).TabIndex = 26;
		((Control)uiLabel6).Text = "融合开关:";
		((Label)uiLabel6).TextAlign = (ContentAlignment)64;
		upDownX.FillColor = Color.FromArgb(24, 24, 24);
		((Control)upDownX).Font = new Font("微软雅黑", 12f);
		((Control)upDownX).ForeColor = Color.Silver;
		((Control)upDownX).Location = new Point(385, 117);
		((Control)upDownX).Margin = new Padding(4, 5, 4, 5);
		upDownX.Maximum = 360;
		upDownX.Minimum = 0;
		((Control)upDownX).MinimumSize = new Size(100, 0);
		((Control)upDownX).Name = "upDownX";
		upDownX.RectColor = Color.FromArgb(130, 130, 130);
		((Control)upDownX).Size = new Size(116, 29);
		upDownX.Style = UIStyle.Black;
		((Control)upDownX).TabIndex = 21;
		((Control)upDownX).Text = "uiIntegerUpDown1";
		upDownX.TextAlignment = (ContentAlignment)32;
		upDownX.Value = 512;
		upDownX.ValueChanged += upDownX_ValueChanged;
		upDownY.FillColor = Color.FromArgb(24, 24, 24);
		((Control)upDownY).Font = new Font("微软雅黑", 12f);
		((Control)upDownY).ForeColor = Color.Silver;
		((Control)upDownY).Location = new Point(386, 166);
		((Control)upDownY).Margin = new Padding(4, 5, 4, 5);
		upDownY.Maximum = 360;
		upDownY.Minimum = 0;
		((Control)upDownY).MinimumSize = new Size(100, 0);
		((Control)upDownY).Name = "upDownY";
		upDownY.RectColor = Color.FromArgb(130, 130, 130);
		((Control)upDownY).Size = new Size(116, 29);
		upDownY.Style = UIStyle.Black;
		((Control)upDownY).TabIndex = 23;
		((Control)upDownY).Text = "uiIntegerUpDown2";
		upDownY.TextAlignment = (ContentAlignment)32;
		upDownY.Value = 512;
		upDownY.ValueChanged += upDownY_ValueChanged;
		uiSwitch3.Active = true;
		uiSwitch3.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)uiSwitch3).BackColor = Color.Transparent;
		((Control)uiSwitch3).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiSwitch3).Location = new Point(137, 54);
		((Control)uiSwitch3).MinimumSize = new Size(1, 1);
		((Control)uiSwitch3).Name = "uiSwitch3";
		((Control)uiSwitch3).Size = new Size(75, 29);
		uiSwitch3.Style = UIStyle.Black;
		((Control)uiSwitch3).TabIndex = 25;
		((Control)uiSwitch3).Text = "uiSwitch3";
		uiSwitch3.ValueChanged += uiSwitch3_ValueChanged;
		((Control)lblHy).BackColor = Color.Transparent;
		((Control)lblHy).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)lblHy).ForeColor = Color.Silver;
		((Control)lblHy).Location = new Point(264, 170);
		((Control)lblHy).Name = "lblHy";
		((Control)lblHy).Size = new Size(112, 23);
		lblHy.Style = UIStyle.Black;
		((Control)lblHy).TabIndex = 24;
		((Control)lblHy).Text = "垂直距离:";
		((Label)lblHy).TextAlign = (ContentAlignment)64;
		uiLine3.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine3).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine3).ForeColor = Color.Silver;
		uiLine3.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine3).Location = new Point(14, 19);
		((Control)uiLine3).MinimumSize = new Size(2, 2);
		((Control)uiLine3).Name = "uiLine3";
		((Control)uiLine3).Size = new Size(456, 29);
		uiLine3.Style = UIStyle.Black;
		((Control)uiLine3).TabIndex = 20;
		((Control)uiLine3).Text = "联屏融合";
		uiLine3.TextAlign = (ContentAlignment)16;
		cbxDirection.DataSource = null;
		cbxDirection.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxDirection.FillColor = Color.White;
		((Control)cbxDirection).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxDirection.Items.AddRange(new object[4] { "上", "下", "左", "右" });
		((Control)cbxDirection).Location = new Point(139, 170);
		((Control)cbxDirection).Margin = new Padding(4, 5, 4, 5);
		((Control)cbxDirection).MinimumSize = new Size(63, 0);
		((Control)cbxDirection).Name = "cbxDirection";
		((Control)cbxDirection).Padding = new Padding(0, 0, 30, 2);
		cbxDirection.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxDirection).Size = new Size(118, 27);
		cbxDirection.Style = UIStyle.Black;
		((Control)cbxDirection).TabIndex = 1;
		((Control)cbxDirection).Text = "上";
		cbxDirection.TextAlignment = (ContentAlignment)16;
		cbxDirection.SelectedIndexChanged += cbxDirection_SelectedIndexChanged;
		((Control)uiLabel11).BackColor = Color.Transparent;
		((Control)uiLabel11).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel11).ForeColor = Color.Silver;
		((Control)uiLabel11).Location = new Point(3, 117);
		((Control)uiLabel11).Name = "uiLabel11";
		((Control)uiLabel11).Size = new Size(129, 23);
		uiLabel11.Style = UIStyle.Black;
		((Control)uiLabel11).TabIndex = 0;
		((Control)uiLabel11).Text = "融合模式:";
		((Label)uiLabel11).TextAlign = (ContentAlignment)64;
		((Control)lblDir).BackColor = Color.Transparent;
		((Control)lblDir).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)lblDir).ForeColor = Color.Silver;
		((Control)lblDir).Location = new Point(-13, 170);
		((Control)lblDir).Name = "lblDir";
		((Control)lblDir).Size = new Size(146, 23);
		lblDir.Style = UIStyle.Black;
		((Control)lblDir).TabIndex = 0;
		((Control)lblDir).Text = "融合方向:";
		((Label)lblDir).TextAlign = (ContentAlignment)64;
		((Control)btnRefresh).BackColor = Color.Transparent;
		((Control)btnRefresh).Cursor = Cursors.Hand;
		btnRefresh.FillColor = Color.FromArgb(15, 40, 70);
		btnRefresh.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnRefresh.FillPressColor = Color.FromArgb(235, 243, 255);
		btnRefresh.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnRefresh).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnRefresh.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnRefresh.ForePressColor = Color.FromArgb(130, 130, 130);
		btnRefresh.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRefresh).Location = new Point(691, 14);
		((Control)btnRefresh).Margin = new Padding(2);
		((Control)btnRefresh).MinimumSize = new Size(1, 1);
		((Control)btnRefresh).Name = "btnRefresh";
		btnRefresh.Radius = 26;
		btnRefresh.RectColor = Color.FromArgb(130, 130, 130);
		btnRefresh.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnRefresh.RectPressColor = Color.FromArgb(130, 130, 130);
		btnRefresh.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnRefresh).Size = new Size(64, 29);
		btnRefresh.Style = UIStyle.Black;
		((Control)btnRefresh).TabIndex = 35;
		((Control)btnRefresh).Text = "刷新";
		((Control)btnRefresh).Visible = false;
		((Control)btnRefresh).Click += btnRefresh_Click;
		((Control)btnTurn).BackColor = Color.Transparent;
		((Control)btnTurn).Cursor = Cursors.Hand;
		btnTurn.FillColor = Color.FromArgb(15, 40, 70);
		btnTurn.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnTurn.FillPressColor = Color.FromArgb(235, 243, 255);
		btnTurn.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnTurn).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnTurn.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnTurn.ForePressColor = Color.FromArgb(130, 130, 130);
		btnTurn.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTurn).Location = new Point(513, 13);
		((Control)btnTurn).Margin = new Padding(2);
		((Control)btnTurn).MinimumSize = new Size(1, 1);
		((Control)btnTurn).Name = "btnTurn";
		btnTurn.Radius = 26;
		btnTurn.RectColor = Color.FromArgb(130, 130, 130);
		btnTurn.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnTurn.RectPressColor = Color.FromArgb(130, 130, 130);
		btnTurn.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnTurn).Size = new Size(104, 29);
		btnTurn.Style = UIStyle.Black;
		((Control)btnTurn).TabIndex = 35;
		((Control)btnTurn).Text = "反转";
		((Control)btnTurn).Click += btnTurn_Click_1;
		cbxSpeed.DataSource = null;
		cbxSpeed.FillColor = Color.White;
		((Control)cbxSpeed).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxSpeed.Items.AddRange(new object[2] { "750", "900" });
		((Control)cbxSpeed).Location = new Point(913, 14);
		((Control)cbxSpeed).Margin = new Padding(4);
		((Control)cbxSpeed).MinimumSize = new Size(62, 0);
		((Control)cbxSpeed).Name = "cbxSpeed";
		((Control)cbxSpeed).Padding = new Padding(0, 0, 42, 2);
		cbxSpeed.Radius = 15;
		cbxSpeed.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxSpeed).Size = new Size(71, 29);
		cbxSpeed.Style = UIStyle.Black;
		((Control)cbxSpeed).TabIndex = 32;
		((Control)cbxSpeed).Text = "750";
		cbxSpeed.TextAlignment = (ContentAlignment)16;
		((Control)cbxSpeed).Visible = false;
		cbxSpeed.SelectedIndexChanged += cbxSpeed_SelectedIndexChanged;
		((Control)uiLabel12).BackColor = Color.Transparent;
		((Control)uiLabel12).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel12).ForeColor = Color.Silver;
		((Control)uiLabel12).Location = new Point(866, 14);
		((Control)uiLabel12).Name = "uiLabel12";
		((Control)uiLabel12).Size = new Size(53, 29);
		uiLabel12.Style = UIStyle.Black;
		((Control)uiLabel12).TabIndex = 31;
		((Control)uiLabel12).Text = "转速:";
		((Label)uiLabel12).TextAlign = (ContentAlignment)16;
		((Control)uiLabel12).Visible = false;
		((Control)uiLabel2).BackColor = Color.Transparent;
		((Control)uiLabel2).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel2).ForeColor = Color.Silver;
		((Control)uiLabel2).Location = new Point(319, 11);
		((Control)uiLabel2).Name = "uiLabel2";
		((Control)uiLabel2).Size = new Size(119, 28);
		uiLabel2.Style = UIStyle.Black;
		((Control)uiLabel2).TabIndex = 23;
		((Control)uiLabel2).Text = "设备编号:";
		((Label)uiLabel2).TextAlign = (ContentAlignment)64;
		((Control)uiPanel2).BackColor = Color.Black;
		((Control)uiPanel2).Controls.Add((Control)(object)uiLine7);
		((Control)uiPanel2).Controls.Add((Control)(object)uiLine5);
		((Control)uiPanel2).Controls.Add((Control)(object)uiAnalogMeter1);
		((Control)uiPanel2).Controls.Add((Control)(object)upDown);
		((Control)uiPanel2).Controls.Add((Control)(object)lblBright);
		((Control)uiPanel2).Controls.Add((Control)(object)uiTrackBar5);
		uiPanel2.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel2).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel2).ForeColor = Color.Silver;
		((Control)uiPanel2).Location = new Point(521, 84);
		((Control)uiPanel2).Margin = new Padding(4);
		((Control)uiPanel2).MinimumSize = new Size(1, 1);
		((Control)uiPanel2).Name = "uiPanel2";
		uiPanel2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel2).Size = new Size(434, 332);
		uiPanel2.Style = UIStyle.Black;
		((Control)uiPanel2).TabIndex = 20;
		((Control)uiPanel2).Text = null;
		uiPanel2.TextAlignment = (ContentAlignment)32;
		((Control)uiLine7).BackColor = Color.Transparent;
		uiLine7.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine7).ForeColor = Color.Silver;
		uiLine7.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine7).Location = new Point(19, 86);
		((Control)uiLine7).MinimumSize = new Size(2, 2);
		((Control)uiLine7).Name = "uiLine7";
		((Control)uiLine7).Size = new Size(360, 29);
		uiLine7.Style = UIStyle.Black;
		((Control)uiLine7).TabIndex = 19;
		((Control)uiLine7).Text = "角度调节";
		uiLine7.TextAlign = (ContentAlignment)16;
		((Control)uiLine5).BackColor = Color.Transparent;
		uiLine5.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine5).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine5).ForeColor = Color.Silver;
		uiLine5.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine5).Location = new Point(16, 13);
		((Control)uiLine5).MinimumSize = new Size(2, 2);
		((Control)uiLine5).Name = "uiLine5";
		((Control)uiLine5).Size = new Size(360, 29);
		uiLine5.Style = UIStyle.Black;
		((Control)uiLine5).TabIndex = 18;
		((Control)uiLine5).Text = "亮度调节";
		uiLine5.TextAlign = (ContentAlignment)16;
		((Control)uiAnalogMeter1).BackColor = Color.Transparent;
		uiAnalogMeter1.BodyColor = Color.FromArgb(15, 40, 70);
		((Control)uiAnalogMeter1).Font = new Font("微软雅黑", 12f);
		((Control)uiAnalogMeter1).Location = new Point(208, 114);
		uiAnalogMeter1.MaxValue = 360.0;
		((Control)uiAnalogMeter1).MinimumSize = new Size(1, 1);
		uiAnalogMeter1.MinValue = 0.0;
		((Control)uiAnalogMeter1).Name = "uiAnalogMeter1";
		uiAnalogMeter1.RadiusSides = UICornerRadiusSides.None;
		uiAnalogMeter1.RectSides = (ToolStripStatusLabelBorderSides)0;
		uiAnalogMeter1.Renderer = null;
		((Control)uiAnalogMeter1).Size = new Size(198, 193);
		uiAnalogMeter1.Style = UIStyle.Black;
		uiAnalogMeter1.StyleCustomMode = true;
		((Control)uiAnalogMeter1).TabIndex = 17;
		((Control)uiAnalogMeter1).Text = "uiAnalogMeter1";
		uiAnalogMeter1.Value = 148.0;
		upDown.FillColor = Color.FromArgb(24, 24, 24);
		((Control)upDown).Font = new Font("微软雅黑", 12f);
		((Control)upDown).ForeColor = Color.Silver;
		((Control)upDown).Location = new Point(40, 179);
		((Control)upDown).Margin = new Padding(4, 5, 4, 5);
		upDown.Maximum = 360;
		upDown.Minimum = 0;
		((Control)upDown).MinimumSize = new Size(100, 0);
		((Control)upDown).Name = "upDown";
		upDown.RectColor = Color.FromArgb(130, 130, 130);
		((Control)upDown).Size = new Size(116, 29);
		upDown.Style = UIStyle.Black;
		((Control)upDown).TabIndex = 14;
		((Control)upDown).Text = "uiIntegerUpDown1";
		upDown.TextAlignment = (ContentAlignment)32;
		upDown.Value = 148;
		upDown.ValueChanged += upDown_ValueChanged;
		((Control)lblBright).BackColor = Color.Transparent;
		((Control)lblBright).Font = new Font("微软雅黑", 12f);
		((Control)lblBright).ForeColor = Color.Silver;
		((Control)lblBright).Location = new Point(366, 47);
		((Control)lblBright).Name = "lblBright";
		((Control)lblBright).Size = new Size(62, 23);
		lblBright.Style = UIStyle.Black;
		((Control)lblBright).TabIndex = 10;
		((Control)lblBright).Text = "255";
		((Label)lblBright).TextAlign = (ContentAlignment)16;
		uiTrackBar5.DisableColor = Color.Silver;
		uiTrackBar5.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiTrackBar5).Font = new Font("微软雅黑", 12f);
		((Control)uiTrackBar5).Location = new Point(6, 46);
		uiTrackBar5.Maximum = 255;
		((Control)uiTrackBar5).MinimumSize = new Size(1, 1);
		((Control)uiTrackBar5).Name = "uiTrackBar5";
		((Control)uiTrackBar5).Size = new Size(360, 25);
		uiTrackBar5.Style = UIStyle.Black;
		((Control)uiTrackBar5).TabIndex = 9;
		((Control)uiTrackBar5).Text = "uiTrackBar5";
		uiTrackBar5.Value = 255;
		uiTrackBar5.ValueChanged += uiTrackBar5_ValueChanged;
		((Control)uiPanel3).Controls.Add((Control)(object)uiSwitch2);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLabel1);
		((Control)uiPanel3).Controls.Add((Control)(object)btnjianY);
		((Control)uiPanel3).Controls.Add((Control)(object)btnJiaY);
		((Control)uiPanel3).Controls.Add((Control)(object)btnJiaX);
		((Control)uiPanel3).Controls.Add((Control)(object)btnJianX);
		((Control)uiPanel3).Controls.Add((Control)(object)lblOri_y);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLabel7);
		((Control)uiPanel3).Controls.Add((Control)(object)lblori_x);
		((Control)uiPanel3).Controls.Add((Control)(object)uiSwitch1);
		((Control)uiPanel3).Controls.Add((Control)(object)uiTrackBar2);
		((Control)uiPanel3).Controls.Add((Control)(object)lblzoom);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLine2);
		((Control)uiPanel3).Controls.Add((Control)(object)uiTrackBar1);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLine1);
		((Control)uiPanel3).Controls.Add((Control)(object)uiTrackBar6);
		((Control)uiPanel3).Controls.Add((Control)(object)uiLine6);
		uiPanel3.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel3).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel3).ForeColor = Color.Silver;
		((Control)uiPanel3).Location = new Point(12, 84);
		((Control)uiPanel3).Margin = new Padding(4);
		((Control)uiPanel3).MinimumSize = new Size(1, 1);
		((Control)uiPanel3).Name = "uiPanel3";
		uiPanel3.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel3).Size = new Size(498, 332);
		uiPanel3.Style = UIStyle.Black;
		((Control)uiPanel3).TabIndex = 21;
		((Control)uiPanel3).Text = null;
		uiPanel3.TextAlignment = (ContentAlignment)32;
		uiSwitch2.Active = true;
		uiSwitch2.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)uiSwitch2).BackColor = Color.Transparent;
		((Control)uiSwitch2).Font = new Font("微软雅黑", 12f);
		((Control)uiSwitch2).Location = new Point(262, 294);
		((Control)uiSwitch2).MinimumSize = new Size(1, 1);
		((Control)uiSwitch2).Name = "uiSwitch2";
		((Control)uiSwitch2).Size = new Size(75, 29);
		uiSwitch2.Style = UIStyle.Black;
		((Control)uiSwitch2).TabIndex = 36;
		((Control)uiSwitch2).Text = "uiSwitch2";
		((Control)uiSwitch2).Visible = false;
		uiSwitch2.ValueChanged += uiSwitch2_ValueChanged;
		((Control)uiLabel1).BackColor = Color.Transparent;
		((Control)uiLabel1).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel1).ForeColor = Color.Silver;
		((Control)uiLabel1).Location = new Point(214, 296);
		((Control)uiLabel1).Name = "uiLabel1";
		((Control)uiLabel1).Size = new Size(62, 23);
		uiLabel1.Style = UIStyle.Black;
		((Control)uiLabel1).TabIndex = 37;
		((Control)uiLabel1).Text = "投屏";
		((Label)uiLabel1).TextAlign = (ContentAlignment)16;
		((Control)uiLabel1).Visible = false;
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
		((Control)btnjianY).Location = new Point(8, 137);
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
		((Control)btnjianY).TabIndex = 71;
		((Control)btnjianY).Text = "-";
		((Control)btnjianY).Click += btnjianY_Click;
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
		((Control)btnJiaY).Location = new Point(407, 137);
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
		((Control)btnJiaY).TabIndex = 70;
		((Control)btnJiaY).Text = "+";
		((Control)btnJiaY).Click += btnJiaY_Click;
		((Control)btnJiaX).BackColor = Color.Transparent;
		((Control)btnJiaX).Cursor = Cursors.Hand;
		btnJiaX.FillColor = Color.FromArgb(15, 40, 70);
		btnJiaX.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJiaX.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJiaX.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJiaX).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJiaX.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaX.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJiaX.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaX).Location = new Point(407, 53);
		((Control)btnJiaX).Margin = new Padding(2);
		((Control)btnJiaX).MinimumSize = new Size(1, 1);
		((Control)btnJiaX).Name = "btnJiaX";
		btnJiaX.Radius = 26;
		btnJiaX.RectColor = Color.FromArgb(130, 130, 130);
		btnJiaX.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJiaX.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJiaX.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJiaX).Size = new Size(29, 23);
		btnJiaX.Style = UIStyle.Black;
		((Control)btnJiaX).TabIndex = 70;
		((Control)btnJiaX).Text = "+";
		((Control)btnJiaX).Click += btnJiaX_Click;
		((Control)btnJianX).BackColor = Color.Transparent;
		((Control)btnJianX).Cursor = Cursors.Hand;
		btnJianX.FillColor = Color.FromArgb(15, 40, 70);
		btnJianX.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnJianX.FillPressColor = Color.FromArgb(235, 243, 255);
		btnJianX.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnJianX).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnJianX.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnJianX.ForePressColor = Color.FromArgb(130, 130, 130);
		btnJianX.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJianX).Location = new Point(8, 53);
		((Control)btnJianX).Margin = new Padding(2);
		((Control)btnJianX).MinimumSize = new Size(1, 1);
		((Control)btnJianX).Name = "btnJianX";
		btnJianX.Radius = 26;
		btnJianX.RectColor = Color.FromArgb(130, 130, 130);
		btnJianX.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnJianX.RectPressColor = Color.FromArgb(130, 130, 130);
		btnJianX.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnJianX).Size = new Size(29, 23);
		btnJianX.Style = UIStyle.Black;
		((Control)btnJianX).TabIndex = 69;
		((Control)btnJianX).Text = "-";
		((Control)btnJianX).Click += btnJianX_Click;
		((Control)lblOri_y).BackColor = Color.Transparent;
		((Control)lblOri_y).Font = new Font("微软雅黑", 12f);
		((Control)lblOri_y).ForeColor = Color.Silver;
		((Control)lblOri_y).Location = new Point(433, 135);
		((Control)lblOri_y).Name = "lblOri_y";
		((Control)lblOri_y).Size = new Size(62, 23);
		lblOri_y.Style = UIStyle.Black;
		((Control)lblOri_y).TabIndex = 5;
		((Control)lblOri_y).Text = "0";
		((Label)lblOri_y).TextAlign = (ContentAlignment)16;
		((Control)uiLabel7).BackColor = Color.Transparent;
		((Control)uiLabel7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel7).ForeColor = Color.Silver;
		((Control)uiLabel7).Location = new Point(31, 295);
		((Control)uiLabel7).Name = "uiLabel7";
		((Control)uiLabel7).Size = new Size(75, 28);
		uiLabel7.Style = UIStyle.Black;
		((Control)uiLabel7).TabIndex = 13;
		((Control)uiLabel7).Text = "呼吸灯开关";
		((Label)uiLabel7).TextAlign = (ContentAlignment)16;
		((Control)uiLabel7).Visible = false;
		((Control)lblori_x).BackColor = Color.Transparent;
		((Control)lblori_x).Font = new Font("微软雅黑", 12f);
		((Control)lblori_x).ForeColor = Color.Silver;
		((Control)lblori_x).Location = new Point(433, 51);
		((Control)lblori_x).Name = "lblori_x";
		((Control)lblori_x).Size = new Size(62, 23);
		lblori_x.Style = UIStyle.Black;
		((Control)lblori_x).TabIndex = 5;
		((Control)lblori_x).Text = "0";
		((Label)lblori_x).TextAlign = (ContentAlignment)16;
		uiSwitch1.Active = true;
		uiSwitch1.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)uiSwitch1).BackColor = Color.Transparent;
		((Control)uiSwitch1).Font = new Font("微软雅黑", 12f);
		((Control)uiSwitch1).Location = new Point(112, 293);
		((Control)uiSwitch1).MinimumSize = new Size(1, 1);
		((Control)uiSwitch1).Name = "uiSwitch1";
		((Control)uiSwitch1).Size = new Size(75, 29);
		uiSwitch1.Style = UIStyle.Black;
		((Control)uiSwitch1).TabIndex = 12;
		((Control)uiSwitch1).Text = "uiSwitch1";
		((Control)uiSwitch1).Visible = false;
		uiSwitch1.ValueChanged += uiSwitch1_ValueChanged;
		uiTrackBar2.DisableColor = Color.Silver;
		uiTrackBar2.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiTrackBar2).Font = new Font("微软雅黑", 12f);
		((Control)uiTrackBar2).Location = new Point(42, 135);
		uiTrackBar2.Maximum = 1080;
		((Control)uiTrackBar2).MinimumSize = new Size(1, 1);
		((Control)uiTrackBar2).Name = "uiTrackBar2";
		((Control)uiTrackBar2).Size = new Size(360, 25);
		uiTrackBar2.Style = UIStyle.Black;
		((Control)uiTrackBar2).TabIndex = 4;
		((Control)uiTrackBar2).Text = "uiTrackBar2";
		uiTrackBar2.ValueChanged += uiTrackBar2_ValueChanged;
		((Control)lblzoom).BackColor = Color.Transparent;
		((Control)lblzoom).Font = new Font("微软雅黑", 12f);
		((Control)lblzoom).ForeColor = Color.Silver;
		((Control)lblzoom).Location = new Point(433, 227);
		((Control)lblzoom).Name = "lblzoom";
		((Control)lblzoom).Size = new Size(62, 23);
		lblzoom.Style = UIStyle.Black;
		((Control)lblzoom).TabIndex = 11;
		((Control)lblzoom).Text = "1024";
		((Label)lblzoom).TextAlign = (ContentAlignment)16;
		((Control)lblzoom).Visible = false;
		uiLine2.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine2).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine2).ForeColor = Color.Silver;
		uiLine2.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine2).Location = new Point(3, 100);
		((Control)uiLine2).MinimumSize = new Size(2, 2);
		((Control)uiLine2).Name = "uiLine2";
		((Control)uiLine2).Size = new Size(447, 29);
		uiLine2.Style = UIStyle.Black;
		((Control)uiLine2).TabIndex = 3;
		((Control)uiLine2).Text = "开始纵坐标";
		uiLine2.TextAlign = (ContentAlignment)16;
		uiTrackBar1.DisableColor = Color.Silver;
		uiTrackBar1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiTrackBar1).Font = new Font("微软雅黑", 12f);
		((Control)uiTrackBar1).Location = new Point(42, 51);
		uiTrackBar1.Maximum = 1920;
		((Control)uiTrackBar1).MinimumSize = new Size(1, 1);
		((Control)uiTrackBar1).Name = "uiTrackBar1";
		((Control)uiTrackBar1).Size = new Size(360, 25);
		uiTrackBar1.Style = UIStyle.Black;
		((Control)uiTrackBar1).TabIndex = 2;
		((Control)uiTrackBar1).Text = "uiTrackBar1";
		uiTrackBar1.ValueChanged += uiTrackBar1_ValueChanged;
		uiLine1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine1).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine1).ForeColor = Color.Silver;
		uiLine1.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine1).Location = new Point(3, 16);
		((Control)uiLine1).MinimumSize = new Size(2, 2);
		((Control)uiLine1).Name = "uiLine1";
		((Control)uiLine1).Size = new Size(447, 29);
		uiLine1.Style = UIStyle.Black;
		((Control)uiLine1).TabIndex = 0;
		((Control)uiLine1).Text = "开始横坐标";
		uiLine1.TextAlign = (ContentAlignment)16;
		uiTrackBar6.DisableColor = Color.Silver;
		uiTrackBar6.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiTrackBar6).Font = new Font("微软雅黑", 12f);
		((Control)uiTrackBar6).Location = new Point(32, 227);
		uiTrackBar6.Maximum = 1024;
		uiTrackBar6.Minimum = 64;
		((Control)uiTrackBar6).MinimumSize = new Size(1, 1);
		((Control)uiTrackBar6).Name = "uiTrackBar6";
		((Control)uiTrackBar6).Size = new Size(395, 25);
		uiTrackBar6.Style = UIStyle.Black;
		((Control)uiTrackBar6).TabIndex = 7;
		((Control)uiTrackBar6).Text = "uiTrackBar6";
		uiTrackBar6.Value = 1024;
		((Control)uiTrackBar6).Visible = false;
		uiTrackBar6.ValueChanged += uiTrackBar6_ValueChanged;
		uiLine6.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine6).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine6).ForeColor = Color.Silver;
		uiLine6.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine6).Location = new Point(3, 192);
		((Control)uiLine6).MinimumSize = new Size(2, 2);
		((Control)uiLine6).Name = "uiLine6";
		((Control)uiLine6).Size = new Size(447, 29);
		uiLine6.Style = UIStyle.Black;
		((Control)uiLine6).TabIndex = 6;
		((Control)uiLine6).Text = "缩放比例";
		uiLine6.TextAlign = (ContentAlignment)16;
		((Control)uiLine6).Visible = false;
		((Control)btnReset).BackColor = Color.Transparent;
		((Control)btnReset).Cursor = Cursors.Hand;
		btnReset.FillColor = Color.FromArgb(15, 40, 70);
		btnReset.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnReset.FillPressColor = Color.FromArgb(235, 243, 255);
		btnReset.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnReset).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnReset.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnReset.ForePressColor = Color.FromArgb(130, 130, 130);
		btnReset.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReset).Location = new Point(242, 12);
		((Control)btnReset).Margin = new Padding(2);
		((Control)btnReset).MinimumSize = new Size(1, 1);
		((Control)btnReset).Name = "btnReset";
		btnReset.Radius = 26;
		btnReset.RectColor = Color.FromArgb(130, 130, 130);
		btnReset.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnReset.RectPressColor = Color.FromArgb(130, 130, 130);
		btnReset.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReset).Size = new Size(74, 29);
		btnReset.Style = UIStyle.Black;
		((Control)btnReset).TabIndex = 10;
		((Control)btnReset).Text = "重置";
		((Control)btnReset).Click += btnReset_Click;
		((Control)btnSingleStop).BackColor = Color.Transparent;
		((Control)btnSingleStop).Cursor = Cursors.Hand;
		btnSingleStop.FillColor = Color.FromArgb(15, 40, 70);
		btnSingleStop.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnSingleStop.FillPressColor = Color.FromArgb(235, 243, 255);
		btnSingleStop.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnSingleStop).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnSingleStop.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnSingleStop.ForePressColor = Color.FromArgb(130, 130, 130);
		btnSingleStop.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSingleStop).Location = new Point(170, 12);
		((Control)btnSingleStop).Margin = new Padding(2);
		((Control)btnSingleStop).MinimumSize = new Size(1, 1);
		((Control)btnSingleStop).Name = "btnSingleStop";
		btnSingleStop.Radius = 26;
		btnSingleStop.RectColor = Color.FromArgb(130, 130, 130);
		btnSingleStop.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSingleStop.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSingleStop.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSingleStop).Size = new Size(65, 29);
		btnSingleStop.Style = UIStyle.Black;
		((Control)btnSingleStop).TabIndex = 11;
		((Control)btnSingleStop).Text = "停止";
		((Control)btnSingleStop).Click += btnSingleStop_Click;
		((Control)btnSingleStart).BackColor = Color.Transparent;
		((Control)btnSingleStart).Cursor = Cursors.Hand;
		btnSingleStart.FillColor = Color.FromArgb(15, 40, 70);
		btnSingleStart.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnSingleStart.FillPressColor = Color.FromArgb(235, 243, 255);
		btnSingleStart.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnSingleStart).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnSingleStart.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnSingleStart.ForePressColor = Color.FromArgb(130, 130, 130);
		btnSingleStart.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSingleStart).Location = new Point(95, 12);
		((Control)btnSingleStart).Margin = new Padding(2);
		((Control)btnSingleStart).MinimumSize = new Size(1, 1);
		((Control)btnSingleStart).Name = "btnSingleStart";
		btnSingleStart.Radius = 26;
		btnSingleStart.RectColor = Color.FromArgb(130, 130, 130);
		btnSingleStart.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSingleStart.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSingleStart.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSingleStart).Size = new Size(67, 29);
		btnSingleStart.Style = UIStyle.Black;
		((Control)btnSingleStart).TabIndex = 12;
		((Control)btnSingleStart).Text = "启动";
		((Control)btnSingleStart).Click += btnSingleStart_Click;
		((Control)uiButton7).BackColor = Color.Transparent;
		((Control)uiButton7).Cursor = Cursors.Hand;
		uiButton7.FillColor = Color.FromArgb(15, 40, 70);
		uiButton7.FillHoverColor = Color.FromArgb(216, 233, 255);
		uiButton7.FillPressColor = Color.FromArgb(235, 243, 255);
		uiButton7.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)uiButton7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		uiButton7.ForeHoverColor = Color.FromArgb(130, 130, 130);
		uiButton7.ForePressColor = Color.FromArgb(130, 130, 130);
		uiButton7.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton7).Location = new Point(12, 12);
		((Control)uiButton7).Margin = new Padding(2);
		((Control)uiButton7).MinimumSize = new Size(1, 1);
		((Control)uiButton7).Name = "uiButton7";
		uiButton7.Radius = 26;
		uiButton7.RectColor = Color.FromArgb(130, 130, 130);
		uiButton7.RectHoverColor = Color.FromArgb(130, 130, 130);
		uiButton7.RectPressColor = Color.FromArgb(130, 130, 130);
		uiButton7.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton7).Size = new Size(74, 29);
		uiButton7.Style = UIStyle.Black;
		((Control)uiButton7).TabIndex = 3;
		((Control)uiButton7).Text = "返回";
		((Control)uiButton7).Click += uiButton7_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 15f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Controls.Add((Control)(object)uiPanel1);
		((Control)this).Margin = new Padding(4);
		((Control)this).Name = "UserControl2";
		((Control)this).Size = new Size(1331, 811);
		((Control)uiPanel1).ResumeLayout(false);
		((Control)uiPanel4).ResumeLayout(false);
		((Control)uiPanel5).ResumeLayout(false);
		((Control)uiPanel2).ResumeLayout(false);
		((Control)uiPanel3).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
