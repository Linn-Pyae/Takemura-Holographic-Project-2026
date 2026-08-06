using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using CSharpWin_JD.CaptureImage;
using MetaStudio.Properties;
using Sunny.UI;

namespace MetaStudio;

public class UserControl1 : UserControl
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

	private SplitContainer sp;

	public UserControl2 u2;

	public int row = 0;

	public int col = 0;

	private bool candraw = false;

	private int ori_offsetX = 300;

	private int ori_offsetY = 150;

	private List<GridData> lst = null;

	private string id = string.Empty;

	private Form1 frm;

	public GridData data = null;

	private OperStep operStep = OperStep.Init;

	private IContainer components = null;

	private UIPanel uiPanel1;

	private SplitContainer splitContainer1;

	private UIPanel uiPanel2;

	private UIButton btnReturn;

	private UIButton btnStopAll;

	private UIButton btnStartAll;

	private UIButton btnSelectImage;

	private UIButton btnInit;

	private UILabel lblName;

	private UIPanel uiPanel3;

	private UIButton btnReset;

	private UIButton btnEdit;

	private UIButton btnSaveConfig;

	private UIButton btnOpenConfig;

	private UILabel uiLabel1;

	private UISwitch sw_Large;

	private UIPanel uiPanel4;

	public UILabel lblBright;

	private UIComboBox cbxBackground;

	private UILabel uiLabel2;

	private UIComboBox cbxSpeed;

	private UILabel uiLabel12;

	private UILabel uiLabel7;

	private UISwitch switchLight;

	private UITrackBar trackBarLiangDu;

	private UILine uiLine5;

	private UILabel uiLabel3;

	private UISwitch uiSwitch1;

	private UIComboBox cbmCom;

	private UIButton btnPiPei;

	private UIPanel uiPanelDraw;

	private UITrackBar trackBarScale;

	private UILabel uiLabel4;

	private UILabel lblzoom;

	private UIImageButton btnLeft;

	private UIImageButton btnRigth;

	private UIImageButton btnButtom;

	private UILabel uiLabel5;

	private UIImageButton btnTop;

	private UILabel uiLabel6;

	private UIButton btnImport;

	private UIButton btnSetID;

	private UILabel uiLabel8;

	private UISwitch switchStart;

	public UserControl1(Form1 _frm, SplitContainer sp)
	{
		InitializeComponent();
		this.sp = sp;
		frm = _frm;
		frm.GetComPort(cbmCom);
		InitDraw();
	}

	private void AddConfigButton(Point p, string id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		PictureBox val = new PictureBox();
		val.Image = Image.FromFile(Application.StartupPath + "\\config.png");
		((Control)val).BackColor = Color.Blue;
		((Control)val).Size = new Size(15, 15);
		((Control)val).Cursor = Cursors.Hand;
		((Control)val).Tag = id;
		((Control)val).Location = p;
		((Control)val).Click += pictureBox1_Click;
		((Control)uiPanel3).Controls.Add((Control)(object)val);
	}

	private void uiButton7_Click(object sender, EventArgs e)
	{
		((Control)sp).Visible = true;
		((Control)this).Visible = false;
	}

	private void btnEdit_Click(object sender, EventArgs e)
	{
		((Control)sp).Visible = true;
		((Control)this).Visible = false;
		frm.ShowControl(EidtMode.Eidt);
		frm.ReDrawEllipse(id);
		frm.ReDrawString();
	}

	private GridData GetData(string guid)
	{
		GridData result = null;
		foreach (GridData item in lst)
		{
			if (item.Guid == guid)
			{
				result = item;
				break;
			}
		}
		return result;
	}

	public void ReDraw(List<GridData> _lst, string guid)
	{
		candraw = false;
		((Control)uiPanel3).Refresh();
		lst = _lst;
		id = guid;
		Graphics g = ((Control)uiPanel3).CreateGraphics();
		data = GetData(guid);
		if (data != null)
		{
			((Control)lblName).Text = data.Name;
			string[] array = data.Size.Split(new char[1] { 'x' });
			row = int.Parse(array[0]);
			col = int.Parse(array[1]);
			Helper.DrawEllipse(g, row, col, ori_offsetX, ori_offsetY, data.IsBottom);
			candraw = true;
			ClearPictureBox();
			CalPictureBoxPoint(data);
			CalDrawStringPoint(g, data);
			DrawRect(g, row, col);
			ConstData.DeviceCount = data.Dic.Count;
		}
	}

	private void ClearPictureBox()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		List<PictureBox> list = new List<PictureBox>();
		foreach (Control item in (ArrangedElementCollection)((Control)uiPanel3).Controls)
		{
			Control val = item;
			if (val is PictureBox)
			{
				list.Add((PictureBox)(object)((val is PictureBox) ? val : null));
			}
		}
		foreach (PictureBox item2 in list)
		{
			((Control)uiPanel3).Controls.Remove((Control)(object)item2);
		}
	}

	private void DrawRect(Graphics g, int row, int col)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		Pen val = new Pen(Color.Blue, 1f);
		g.DrawRectangle(val, ori_offsetX, ori_offsetY, col * 100 - (col - 1) * 10, row * 100 - (row - 1) * 10);
	}

	private void CalDrawStringPoint(Graphics g, GridData data)
	{
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < col; j++)
			{
				Point point = new Point(ori_offsetX + 40 + j * 90, ori_offsetY + 40 + i * 90);
				string key = i + "-" + j;
				Helper.DrawString2(g, point, data.Dic[key]);
			}
		}
	}

	public void CalPictureBoxPoint(GridData data)
	{
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < col; j++)
			{
				Point p = new Point(ori_offsetX + 45 + j * 90, ori_offsetY + 70 + i * 90);
				string key = i + "-" + j;
				AddConfigButton(p, data.Dic[key]);
			}
		}
	}

	private void pictureBox1_Click(object sender, EventArgs e)
	{
		try
		{
			((Control)sp).Visible = false;
			((Control)this).Visible = false;
			((Control)u2).Visible = true;
			ConstData.curOperID = int.Parse(((Control)((sender is PictureBox) ? sender : null)).Tag.ToString()) + 1;
			u2.SetID(((Control)((sender is PictureBox) ? sender : null)).Tag.ToString(), data);
			((Form)frm).Size = new Size(970, 797);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void uiPanel3_Paint(object sender, PaintEventArgs e)
	{
		if (candraw)
		{
			GridData gridData = GetData(id);
			Helper.DrawEllipse(e.Graphics, row, col, ori_offsetX, ori_offsetY, gridData.IsBottom);
			CalDrawStringPoint(e.Graphics, gridData);
			DrawRect(e.Graphics, row, col);
		}
	}

	private void btnSetID_Click(object sender, EventArgs e)
	{
		SetID();
		if (data != null)
		{
			ConstData.DeviceCount = data.Dic.Count;
		}
	}

	private void btnInit_Click(object sender, EventArgs e)
	{
		Init_Click();
	}

	public void Init_Click()
	{
		frm.ShowForm("初始化", 70);
		operStep = OperStep.Init;
		InitScreen();
		Thread.Sleep(50);
		MetaTool.SetDeviceSpeed(0, 750);
		InitFusion();
		MetaTool.ResetFusionXY(0);
	}

	public void InitFusion()
	{
		Task task = new Task(delegate
		{
			Thread.Sleep(100);
			InitBottomFusion();
			CloseSideFusion();
			InitTopFusion();
			GetMotoDireEx();
		});
		task.Start();
	}

	public void InitTopFusion()
	{
		if (data == null)
		{
			return;
		}
		Console.WriteLine("this.data.IsBottom=" + data.IsBottom);
		for (int i = 1; i <= row; i++)
		{
			for (int j = 1; j <= col; j++)
			{
				Thread.Sleep(50);
				if (data.IsBottom)
				{
					if (i % 2 != 0)
					{
						if (j % 2 != 0)
						{
							int deviceID = int.Parse(data.Dic[i - 1 + "-" + (j - 1)]) + 1;
							RegisterHelper.OpenTopFusion(deviceID);
							u2.EnableReg(deviceID, 0, 0);
							MetaTool.MotoDirct(deviceID, 0);
						}
					}
					else if (i % 2 == 0 && j % 2 == 0)
					{
						int deviceID = int.Parse(data.Dic[i - 1 + "-" + (j - 1)]) + 1;
						RegisterHelper.OpenTopFusion(deviceID);
						u2.EnableReg(deviceID, 0, 0);
						MetaTool.MotoDirct(deviceID, 0);
					}
				}
				else if (i % 2 != 0)
				{
					if (j % 2 == 0)
					{
						int deviceID = int.Parse(data.Dic[i - 1 + "-" + (j - 1)]) + 1;
						RegisterHelper.OpenTopFusion(deviceID);
						u2.EnableReg(deviceID, 0, 0);
						MetaTool.MotoDirct(deviceID, 0);
					}
				}
				else if (i % 2 == 0 && j % 2 != 0)
				{
					int deviceID = int.Parse(data.Dic[i - 1 + "-" + (j - 1)]) + 1;
					RegisterHelper.OpenTopFusion(deviceID);
					u2.EnableReg(deviceID, 0, 0);
					MetaTool.MotoDirct(deviceID, 0);
				}
			}
		}
	}

	public void CloseSideFusion()
	{
		for (int i = 0; i < col; i++)
		{
			int num = int.Parse(data.Dic[0 + "-" + i]) + 1;
			u2.CloseBottomFusion(num, 0);
		}
		for (int i = 0; i < col; i++)
		{
			int num = int.Parse(data.Dic[row - 1 + "-" + i]) + 1;
			u2.CloseBottomFusion(num, 1);
		}
		for (int j = 0; j < row; j++)
		{
			int num = int.Parse(data.Dic[j + "-" + 0]) + 1;
			u2.CloseBottomFusion(num, 2);
		}
		for (int j = 0; j < row; j++)
		{
			int num = int.Parse(data.Dic[j + "-" + (col - 1)]) + 1;
			u2.CloseBottomFusion(num, 3);
		}
	}

	public void InitBottomFusion()
	{
		if (data == null)
		{
			return;
		}
		Console.WriteLine("this.data.IsBottom=" + data.IsBottom);
		for (int i = 1; i <= row; i++)
		{
			for (int j = 1; j <= col; j++)
			{
				Thread.Sleep(50);
				if (data.IsBottom)
				{
					if (i % 2 != 0)
					{
						if (j % 2 == 0)
						{
							int deviceID = int.Parse(data.Dic[i - 1 + "-" + (j - 1)]) + 1;
							for (int k = 0; k < 4; k++)
							{
								u2.OpenBottomFusion(deviceID, k);
								u2.EnableReg(deviceID, k, 1);
							}
							MetaTool.MotoDirct(deviceID, 1);
						}
					}
					else if (i % 2 == 0 && j % 2 != 0)
					{
						int deviceID = int.Parse(data.Dic[i - 1 + "-" + (j - 1)]) + 1;
						for (int k = 0; k < 4; k++)
						{
							u2.OpenBottomFusion(deviceID, k);
							u2.EnableReg(deviceID, k, 1);
						}
						MetaTool.MotoDirct(deviceID, 1);
					}
				}
				else if (i % 2 == 0)
				{
					if (j % 2 == 0)
					{
						int deviceID = int.Parse(data.Dic[i - 1 + "-" + (j - 1)]) + 1;
						for (int k = 0; k < 4; k++)
						{
							u2.OpenBottomFusion(deviceID, k);
							u2.EnableReg(deviceID, k, 1);
						}
						MetaTool.MotoDirct(deviceID, 1);
					}
				}
				else if (i % 2 != 0 && j % 2 != 0)
				{
					int deviceID = int.Parse(data.Dic[i - 1 + "-" + (j - 1)]) + 1;
					for (int k = 0; k < 4; k++)
					{
						u2.OpenBottomFusion(deviceID, k);
						u2.EnableReg(deviceID, k, 1);
					}
					MetaTool.MotoDirct(deviceID, 1);
				}
			}
		}
	}

	public void InitScreen()
	{
		Task task = new Task(delegate
		{
			try
			{
				if (data != null)
				{
					ConstData.DeviceCount = data.Dic.Count;
					frm.CaptureScreen(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
					SetID();
					CutScreenTOStator(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height, center: true);
					for (int i = 0; i < data.Dic.Count; i++)
					{
						u2.ResetScale(i + 2);
						u2.SetVideoOutputEn(i + 2);
					}
				}
			}
			catch (Exception ex)
			{
				LogerHelper.Error(ex.Message);
			}
		});
		task.Start();
	}

	public void SetID()
	{
		MetaTool.Stop(0);
		ResetStatorID();
		for (int i = 0; i < data.Dic.Count; i++)
		{
			int num = 0x55AA0000 | (i + 2);
			SPHelper.SendTOStator(1, 2, 173, num);
			Thread.Sleep(1);
		}
	}

	public void SetID2()
	{
		MetaTool.Stop(0);
		ResetStatorID();
	}

	public void GetMotoDireEx()
	{
		for (int i = 0; i < data.Dic.Count; i++)
		{
			GetMotoDire(i + 2);
			Thread.Sleep(100);
		}
	}

	public void GetMotoDire(int id)
	{
		SPHelper.SendTORotor(id, 1, 22, 0);
	}

	private void SwitchScreen()
	{
		if (!ConstData.isSmall)
		{
			CutScreenTOStator(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height, center: true);
			for (int i = 0; i < data.Dic.Count; i++)
			{
				u2.ResetScale(i + 2);
				u2.SetVideoOutputEn(i + 2);
			}
		}
		else if (ConstData.isSmall)
		{
			if (row == 2 && col == 1)
			{
				CutScreenTOStator3(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
			}
			else if (row == 1 && col == 2)
			{
				CutScreenTOStator2(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
			}
		}
	}

	private void CutScreenTOStator(Point point, int _width, int _height, bool center)
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
			if (center)
			{
				num4 = (int)(((double)_width - (num * 2.0 + (double)num3)) / 2.0) + point.X;
			}
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < col; j++)
				{
					int num5 = int.Parse(data.Dic[i + "-" + j]) + 1;
					CaptureScreenTOStator(num5, new Point(num4 + (int)((double)j * (ConstData.Radical_sign * num)), y + (int)((double)i * (ConstData.Radical_sign * num))), (int)ConstData.Diameter, (int)ConstData.Diameter);
				}
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void CutScreenTOStator3(Point point, int _width, int _height)
	{
		try
		{
			double num = (double)_height / (ConstData.Radical_sign * 2.0);
			double num2 = (double)_width / ConstData.Radical_sign;
			if (num > num2)
			{
				num = num2;
			}
			double num3 = num * 2.0;
			double num4 = num * ConstData.Radical_sign;
			double num5 = (num3 - num4) / 2.0;
			CaptureScreenTOStator(2, new Point(0, 0), (int)num4, (int)(num4 + num5));
			CaptureScreenTOStator(3, new Point(0, (int)(num4 - num5)), (int)num4, (int)(num4 + num5));
			double num6 = 1024.0;
			double num7 = 512.0 * ConstData.Radical_sign;
			double num8 = (num6 - num7) / 2.0;
			u2.ScaleStator(2, new Point((int)num8, (int)num8), (int)num4, (int)(num4 + num5), (int)num7, (int)(num7 + num8));
			u2.ScaleStator(3, new Point((int)num8, 0), (int)num4, (int)(num4 + num5), (int)num7, (int)(num7 + num8));
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void CutScreenTOStator2(Point point, int _width, int _height)
	{
		try
		{
			double num = (double)_height / ConstData.Radical_sign;
			double num2 = (double)_width / (ConstData.Radical_sign * 2.0);
			if (num > num2)
			{
				num = num2;
			}
			double num3 = num * 2.0;
			double num4 = num * ConstData.Radical_sign;
			double num5 = (num3 - num4) / 2.0;
			CaptureScreenTOStator(2, new Point(0, 0), (int)(num4 + num5), (int)(num4 + num5));
			CaptureScreenTOStator(3, new Point((int)(num4 - num5), 0), (int)(num4 + num5), (int)num4);
			double num6 = 1024.0;
			double num7 = 512.0 * ConstData.Radical_sign;
			double num8 = (num6 - num7) / 2.0;
			u2.ScaleStator(2, new Point((int)num8, (int)num8), (int)(num4 + num5), (int)num4, (int)(num7 + num8), (int)num7);
			u2.ScaleStator(3, new Point(0, (int)num8), (int)(num4 + num5), (int)num4, (int)(num7 + num8), (int)num7);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void ResetStatorID()
	{
		SPHelper.SendTOStator(0, 2, 173, 1437204481);
	}

	public void CaptureScreenTOStator(int Id, Point point, int m_width, int m_height)
	{
		SPHelper.SendTOStator(Id, 2, 32, point.X);
		SPHelper.SendTOStator(Id, 2, 33, point.Y);
		SPHelper.SendTOStator(Id, 2, 239, (point.X << 16) | point.Y);
		int num = m_width;
		if (num < 32)
		{
			num = 32;
		}
		int num2 = m_height;
		if (num2 < 32)
		{
			num2 = 32;
		}
		SPHelper.SendTOStator(Id, 2, 34, (num << 16) | num2);
		SPHelper.Factor_Sclr(Id, num, num2, 1024, 1024);
		SPHelper.SendTOStator(Id, 2, 43, 0);
		SPHelper.SendTOStator(Id, 2, 43, 1);
		SPHelper.SendTOStator(Id, 2, 43, 0);
		SPHelper.SendTOStator(Id, 2, 42, 0);
	}

	private void btnStartAll_Click(object sender, EventArgs e)
	{
		MetaTool.Start(0);
	}

	private void btnStopAll_Click(object sender, EventArgs e)
	{
		MetaTool.Stop(0);
	}

	private void btnPiPei_Click(object sender, EventArgs e)
	{
		MetaTool.MatchDevice(0, 0);
	}

	private void btnSelectImage_Click(object sender, EventArgs e)
	{
		SelectImage();
	}

	public void SelectImage()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		try
		{
			((Control)frm).Visible = false;
			CaptureImageTool captureImageTool = new CaptureImageTool();
			if ((int)((Form)captureImageTool).ShowDialog() == 1)
			{
				Image image = captureImageTool.Image;
				Point startPoint = captureImageTool.StartPoint;
				if (startPoint.X < 0)
				{
					startPoint.X = 0;
				}
				if (startPoint.Y < 0)
				{
					startPoint.Y = 0;
				}
				int width = image.Width;
				int height = image.Height;
				if (startPoint.X + image.Width > 1920)
				{
					width = 1920 - startPoint.X;
				}
				if (startPoint.Y + image.Height > 1080)
				{
					height = 1080 - startPoint.Y;
				}
				for (int i = 0; i < data.Dic.Count; i++)
				{
					u2.ResetScale(i + 2);
				}
				frm.CaptureScreen(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
				CutScreenTOStator(startPoint, width, height, center: true);
				((Control)frm).Visible = true;
			}
			else
			{
				((Control)frm).Visible = true;
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
		if ((int)MessageBox.Show(text, text2, (MessageBoxButtons)4, (MessageBoxIcon)64) == 6 && data != null)
		{
			frm.CaptureScreen(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height);
			CutScreenTOStator(new Point(0, 0), ConstData.Ori_Width, ConstData.Ori_Height, center: true);
			for (int i = 0; i < data.Dic.Count; i++)
			{
				u2.ResetScale(i + 2);
				u2.SetVideoOutputEn(i + 2);
			}
			trackBarScale.Value = 1024;
			cbxSpeed.SelectedIndex = 0;
			trackBarLiangDu.Value = 255;
			cbxBackground.SelectedIndex = 1;
			InitDraw();
			((Control)uiPanelDraw).Refresh();
			MetaTool.ResetFusionXY(0);
			MetaTool.CloseFusion(0);
			uiSwitch1.Active = false;
			uiSwitch1.Active = true;
			switchLight.Active = false;
			switchLight.Active = true;
		}
	}

	private void btnOpenConfig_Click(object sender, EventArgs e)
	{
		InputConfig();
	}

	public void InputConfig()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		OpenFileDialog val = new OpenFileDialog();
		((FileDialog)val).Title = "选择一个文件";
		val.Multiselect = false;
		((FileDialog)val).Filter = "json文件|*.json|所有文件|*.*";
		if ((int)((CommonDialog)val).ShowDialog() == 1)
		{
			frm.ShowForm("导入", 150);
			MetaTool.Stop(0);
			string[] fileNames = ((FileDialog)val).FileNames;
			foreach (string filename in fileNames)
			{
				List<RegInfo> lstReg = JsonHelper.ReadConfigJson2(filename);
				ImportConfig2(lstReg);
			}
		}
	}

	private void btnSaveConfig_Click(object sender, EventArgs e)
	{
		frm.ShowForm("保存", 30);
		SaveConfig();
	}

	private void btnImport_Click(object sender, EventArgs e)
	{
		OutputConfig();
	}

	public void OutputConfig()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		SaveFileDialog val = new SaveFileDialog();
		((FileDialog)val).Filter = "json文件|*.json|所有文件|*.*";
		if ((int)((CommonDialog)val).ShowDialog() == 1)
		{
			operStep = OperStep.Import;
			frm.lstAllReg = new List<RegInfo>();
			frm.ShowForm("保存", 150);
			string fileName = ((FileDialog)val).FileName;
			ReadConfig(fileName);
		}
	}

	public void ImportConfig2(List<RegInfo> lstReg)
	{
		Thread.Sleep(30);
		foreach (RegInfo item in lstReg)
		{
			if (item.devType == 2)
			{
				SPHelper.SendTORotor(item.deviceID, 2, Helper.Decryption(item.value1), item.value2);
			}
			else if (item.devType == 1)
			{
				SPHelper.SendTOStator(item.deviceID, 2, Helper.Decryption(item.value1), item.value2);
			}
			else if (item.devType == 0)
			{
				SPHelper.SendTOVdbox(item.deviceID, 2, Helper.Decryption(item.value1), item.value2);
			}
			Thread.Sleep(30);
		}
		for (int i = 0; i < data.Dic.Count; i++)
		{
			MetaTool.EnableRGBReg(i + 2);
		}
		RegisterHelper.SaveConfig(0);
	}

	public void SaveConfig()
	{
		if (data != null)
		{
			RegisterHelper.SaveConfig(0);
		}
	}

	public void ReadConfig(string filePath)
	{
		try
		{
			Task task = new Task(delegate
			{
				if (data != null)
				{
					ConstData.Importing = true;
					for (int i = 0; i < data.Dic.Count; i++)
					{
						RegisterHelper.ReadAllReg(i + 2);
					}
					Thread.Sleep(1000);
					JsonHelper.WriteConfigJson2(frm.lstAllReg, filePath);
					ConstData.Importing = false;
				}
			});
			task.Start();
		}
		catch (Exception ex)
		{
			ConstData.Importing = false;
			LogerHelper.Error(ex.Message);
		}
	}

	private void sw_Large_ValueChanged(object sender, bool value)
	{
		if (!value)
		{
			ConstData.isSmall = false;
			SwitchScreen();
		}
		else if (value)
		{
			ConstData.isSmall = true;
			SwitchScreen();
		}
	}

	public void GetSerData(byte[] buf)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		try
		{
			if (!SPHelper.CheckHead(buf) || buf.Length != 26)
			{
				return;
			}
			if (buf[4] == 129 && buf[7] == 128)
			{
				if (buf[16] != 22)
				{
					return;
				}
				MethodInvoker val = null;
				int data = SPHelper.ConvetInt(buf, 20);
				if (val == null)
				{
					val = (MethodInvoker)delegate
					{
						if (operStep == OperStep.Init)
						{
							if (data == 1)
							{
								Console.WriteLine("1");
							}
							else if (data == 0)
							{
								Console.WriteLine("0");
							}
						}
					};
				}
				((Control)this).BeginInvoke((Delegate)(object)val);
			}
			else
			{
				if ((buf[4] == 129 && buf[7] == 0) || buf[4] != 1 || buf[7] != 0 || buf[16] != 34)
				{
					return;
				}
				MethodInvoker val2 = null;
				int data2 = SPHelper.ConvetInt(buf, 20);
				if (!((Control)this).IsHandleCreated)
				{
					return;
				}
				if (val2 == null)
				{
					val2 = (MethodInvoker)delegate
					{
						trackBarLiangDu.Value = data2;
					};
				}
				((Control)this).BeginInvoke((Delegate)(object)val2);
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
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

	private void cbxBackground_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = cbxBackground.SelectedIndex;
		MetaTool.SetBackground(0, selectedIndex);
	}

	private void trackBarLiangDu_ValueChanged(object sender, EventArgs e)
	{
		((Control)lblBright).Text = trackBarLiangDu.Value.ToString();
		int value = trackBarLiangDu.Value;
		MetaTool.SetBrightness(0, value);
	}

	private void uiSwitch1_ValueChanged(object sender, bool value)
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

	private void cbmCom_SelectedIndexChanged(object sender, EventArgs e)
	{
		frm.ComChange(cbmCom.SelectedItem.ToString());
	}

	private void InitDraw()
	{
		radiusRect = 212;
		radiusElipse = 212;
		radiusFR = 3;
		rate = 53.0 / 256.0;
		fr_x = (int)((double)((Control)uiPanelDraw).Width / 2.0);
		fr_y = (int)((double)((Control)uiPanelDraw).Height / 2.0);
		e_x = fr_x - (int)((double)radiusElipse / 2.0);
		e_y = 0;
		r_x = fr_x - (int)((double)radiusRect / 2.0);
		r_y = 0;
	}

	private void uiPanel5_Paint(object sender, PaintEventArgs e)
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

	private void trackBarScale_ValueChanged(object sender, EventArgs e)
	{
		radiusElipse = (int)((double)trackBarScale.Value * rate);
		e_x = fr_x - (int)((double)radiusElipse / 2.0);
		e_y = fr_y - (int)((double)radiusElipse / 2.0);
		((Control)uiPanelDraw).Refresh();
		((Control)lblzoom).Text = trackBarScale.Value.ToString();
		MetaTool.SetScale(0, trackBarScale.Value);
	}

	private void UserControl1_VisibleChanged(object sender, EventArgs e)
	{
		((Form)frm).Size = new Size(1199, 797);
	}

	public void Translate(int type)
	{
		switch (type)
		{
		case 0:
			((Control)btnOpenConfig).Text = "导入";
			((Control)btnReset).Text = "重置";
			((Control)uiLabel3).Text = "投屏:";
			((Control)btnSelectImage).Text = "选择区域";
			((Control)uiLabel12).Text = "转速:";
			((Control)btnStopAll).Text = "停止";
			((Control)btnStartAll).Text = "启动";
			((Control)btnSaveConfig).Text = "保存";
			((Control)btnImport).Text = "导出";
			((Control)btnPiPei).Text = "匹配遥控器";
			((Control)uiLine5).Text = "亮度调节";
			((Control)uiLabel7).Text = "呼吸灯开关:";
			((Control)uiLabel8).Text = "上电自启动:";
			((Control)uiLabel4).Text = "缩放:";
			uiSwitch1.ActiveText = "开";
			uiSwitch1.InActiveText = "关";
			switchLight.ActiveText = "开";
			switchLight.InActiveText = "关";
			switchStart.ActiveText = "开";
			switchStart.InActiveText = "关";
			((Control)uiLabel2).Text = "色调:";
			((Control)cbxBackground).Text = "标准";
			cbxBackground.Items.Clear();
			cbxBackground.Items.AddRange(new object[3] { "暖色", "标准", "冷色" });
			cbxSpeed.Items.Clear();
			((Control)cbxSpeed).Text = "普通";
			cbxSpeed.Items.AddRange(new object[2] { "普通", "高速" });
			((Control)uiLabel5).Text = "水平:";
			((Control)uiLabel6).Text = "垂直:";
			((Control)btnReturn).Text = "返回";
			((Control)btnInit).Text = "初始化";
			((Control)btnEdit).Text = "编辑";
			((Control)btnSetID).Text = "设置ID";
			break;
		case 1:
			((Control)btnOpenConfig).Text = "Import";
			((Control)btnReset).Text = "Reset";
			((Control)uiLabel3).Text = "Projection Screen:";
			((Control)btnSelectImage).Text = "Select Area";
			((Control)uiLabel12).Text = "Speed:";
			((Control)btnStopAll).Text = "Stop";
			((Control)btnStartAll).Text = "Start";
			((Control)btnSaveConfig).Text = "Save";
			((Control)btnImport).Text = "Export";
			((Control)btnPiPei).Text = "Match";
			((Control)uiLine5).Text = "Light Adjust";
			((Control)uiLabel7).Text = "Breathing Light:";
			((Control)uiLabel8).Text = "Auto Start:";
			((Control)uiLabel4).Text = "Scale:";
			uiSwitch1.ActiveText = "Open";
			uiSwitch1.InActiveText = "Close";
			switchLight.ActiveText = "Open";
			switchLight.InActiveText = "Close";
			switchStart.ActiveText = "Open";
			switchStart.InActiveText = "Close";
			((Control)uiLabel2).Text = "Hue:";
			((Control)cbxBackground).Text = "Standard";
			cbxBackground.Items.Clear();
			cbxBackground.Items.AddRange(new object[3] { "Warm Color", "Standard", "Cold Color" });
			cbxSpeed.Items.Clear();
			((Control)cbxSpeed).Text = "Normal";
			cbxSpeed.Items.AddRange(new object[2] { "Normal", "Fast" });
			((Control)uiLabel5).Text = "Horizontal:";
			((Control)uiLabel6).Text = "Vertical:";
			((Control)btnReturn).Text = "Back";
			((Control)btnInit).Text = "Initialize";
			((Control)btnEdit).Text = "Edit";
			((Control)btnSetID).Text = "Set ID";
			break;
		}
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
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Expected O, but got Unknown
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Expected O, but got Unknown
		//IL_0643: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0735: Expected O, but got Unknown
		//IL_07f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fa: Expected O, but got Unknown
		//IL_08cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d7: Expected O, but got Unknown
		//IL_0a0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a16: Expected O, but got Unknown
		//IL_0bed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf7: Expected O, but got Unknown
		//IL_0c14: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d0f: Expected O, but got Unknown
		//IL_0dc9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dd3: Expected O, but got Unknown
		//IL_0f09: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f13: Expected O, but got Unknown
		//IL_1136: Unknown result type (might be due to invalid IL or missing references)
		//IL_1140: Expected O, but got Unknown
		//IL_1363: Unknown result type (might be due to invalid IL or missing references)
		//IL_136d: Expected O, but got Unknown
		//IL_13ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_159e: Unknown result type (might be due to invalid IL or missing references)
		//IL_15a8: Expected O, but got Unknown
		//IL_1628: Unknown result type (might be due to invalid IL or missing references)
		//IL_17dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_17e6: Expected O, but got Unknown
		//IL_1866: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a24: Expected O, but got Unknown
		//IL_1aa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c05: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c0f: Expected O, but got Unknown
		//IL_1ccd: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cd7: Expected O, but got Unknown
		//IL_1d8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d96: Expected O, but got Unknown
		//IL_203a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2044: Expected O, but got Unknown
		//IL_2077: Unknown result type (might be due to invalid IL or missing references)
		//IL_2137: Unknown result type (might be due to invalid IL or missing references)
		//IL_2141: Expected O, but got Unknown
		//IL_21ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_21f4: Expected O, but got Unknown
		//IL_229d: Unknown result type (might be due to invalid IL or missing references)
		//IL_22a7: Expected O, but got Unknown
		//IL_2354: Unknown result type (might be due to invalid IL or missing references)
		//IL_235e: Expected O, but got Unknown
		//IL_2419: Unknown result type (might be due to invalid IL or missing references)
		//IL_2423: Expected O, but got Unknown
		//IL_24d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_24da: Expected O, but got Unknown
		//IL_25a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_25b2: Expected O, but got Unknown
		//IL_272e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2738: Expected O, but got Unknown
		//IL_27bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_28f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_2902: Expected O, but got Unknown
		//IL_2936: Unknown result type (might be due to invalid IL or missing references)
		//IL_29e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_29ed: Expected O, but got Unknown
		//IL_29fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a08: Expected O, but got Unknown
		//IL_2ad7: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ae1: Expected O, but got Unknown
		//IL_2b32: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c35: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c3f: Expected O, but got Unknown
		//IL_2d0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d15: Expected O, but got Unknown
		//IL_2d5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d96: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e68: Expected O, but got Unknown
		//IL_2f38: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f42: Expected O, but got Unknown
		//IL_303c: Unknown result type (might be due to invalid IL or missing references)
		//IL_3046: Expected O, but got Unknown
		//IL_31a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_31ac: Expected O, but got Unknown
		//IL_322b: Unknown result type (might be due to invalid IL or missing references)
		//IL_3361: Unknown result type (might be due to invalid IL or missing references)
		//IL_336b: Expected O, but got Unknown
		//IL_3420: Unknown result type (might be due to invalid IL or missing references)
		//IL_342a: Expected O, but got Unknown
		//IL_3544: Unknown result type (might be due to invalid IL or missing references)
		//IL_354e: Expected O, but got Unknown
		//IL_357d: Unknown result type (might be due to invalid IL or missing references)
		//IL_3629: Unknown result type (might be due to invalid IL or missing references)
		//IL_3633: Expected O, but got Unknown
		//IL_3644: Unknown result type (might be due to invalid IL or missing references)
		//IL_364e: Expected O, but got Unknown
		//IL_3773: Unknown result type (might be due to invalid IL or missing references)
		//IL_377d: Expected O, but got Unknown
		//IL_37fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_39b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_39bb: Expected O, but got Unknown
		//IL_3a3b: Unknown result type (might be due to invalid IL or missing references)
		//IL_3bef: Unknown result type (might be due to invalid IL or missing references)
		//IL_3bf9: Expected O, but got Unknown
		//IL_3c79: Unknown result type (might be due to invalid IL or missing references)
		//IL_3dd8: Unknown result type (might be due to invalid IL or missing references)
		uiPanel1 = new UIPanel();
		splitContainer1 = new SplitContainer();
		uiPanel2 = new UIPanel();
		switchStart = new UISwitch();
		uiLabel8 = new UILabel();
		switchLight = new UISwitch();
		btnImport = new UIButton();
		cbmCom = new UIComboBox();
		sw_Large = new UISwitch();
		uiLabel1 = new UILabel();
		btnOpenConfig = new UIButton();
		btnSaveConfig = new UIButton();
		btnReturn = new UIButton();
		btnPiPei = new UIButton();
		btnStopAll = new UIButton();
		btnStartAll = new UIButton();
		uiSwitch1 = new UISwitch();
		uiLabel3 = new UILabel();
		uiLabel7 = new UILabel();
		uiPanel4 = new UIPanel();
		btnLeft = new UIImageButton();
		btnRigth = new UIImageButton();
		btnButtom = new UIImageButton();
		uiLabel5 = new UILabel();
		btnTop = new UIImageButton();
		uiLabel6 = new UILabel();
		trackBarScale = new UITrackBar();
		btnReset = new UIButton();
		uiPanelDraw = new UIPanel();
		lblBright = new UILabel();
		cbxBackground = new UIComboBox();
		uiLabel2 = new UILabel();
		cbxSpeed = new UIComboBox();
		uiLabel12 = new UILabel();
		trackBarLiangDu = new UITrackBar();
		uiLine5 = new UILine();
		btnSelectImage = new UIButton();
		lblzoom = new UILabel();
		uiLabel4 = new UILabel();
		uiPanel3 = new UIPanel();
		lblName = new UILabel();
		btnSetID = new UIButton();
		btnInit = new UIButton();
		btnEdit = new UIButton();
		((Control)uiPanel1).SuspendLayout();
		((ISupportInitialize)splitContainer1).BeginInit();
		((Control)splitContainer1.Panel1).SuspendLayout();
		((Control)splitContainer1.Panel2).SuspendLayout();
		((Control)splitContainer1).SuspendLayout();
		((Control)uiPanel2).SuspendLayout();
		((Control)uiPanel4).SuspendLayout();
		((ISupportInitialize)(object)btnLeft).BeginInit();
		((ISupportInitialize)(object)btnRigth).BeginInit();
		((ISupportInitialize)(object)btnButtom).BeginInit();
		((ISupportInitialize)(object)btnTop).BeginInit();
		((Control)uiPanel3).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)uiPanel1).Controls.Add((Control)(object)splitContainer1);
		((Control)uiPanel1).Dock = (DockStyle)5;
		uiPanel1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel1).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel1).ForeColor = Color.Silver;
		((Control)uiPanel1).Location = new Point(0, 0);
		((Control)uiPanel1).Margin = new Padding(5, 6, 5, 6);
		((Control)uiPanel1).MinimumSize = new Size(1, 1);
		((Control)uiPanel1).Name = "uiPanel1";
		uiPanel1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel1).Size = new Size(1233, 728);
		uiPanel1.Style = UIStyle.Black;
		((Control)uiPanel1).TabIndex = 0;
		((Control)uiPanel1).Text = null;
		uiPanel1.TextAlignment = (ContentAlignment)32;
		splitContainer1.Dock = (DockStyle)5;
		splitContainer1.FixedPanel = (FixedPanel)1;
		((Control)splitContainer1).Location = new Point(0, 0);
		((Control)splitContainer1).Name = "splitContainer1";
		splitContainer1.Orientation = (Orientation)0;
		((Control)splitContainer1.Panel1).Controls.Add((Control)(object)uiPanel2);
		((Control)splitContainer1.Panel2).BackColor = Color.Transparent;
		((Control)splitContainer1.Panel2).Controls.Add((Control)(object)uiPanel4);
		((Control)splitContainer1.Panel2).Controls.Add((Control)(object)uiPanel3);
		((Control)splitContainer1).Size = new Size(1233, 728);
		splitContainer1.SplitterDistance = 61;
		((Control)splitContainer1).TabIndex = 0;
		((Control)uiPanel2).Controls.Add((Control)(object)switchLight);
		((Control)uiPanel2).Controls.Add((Control)(object)uiLabel7);
		((Control)uiPanel2).Controls.Add((Control)(object)btnImport);
		((Control)uiPanel2).Controls.Add((Control)(object)cbmCom);
		((Control)uiPanel2).Controls.Add((Control)(object)sw_Large);
		((Control)uiPanel2).Controls.Add((Control)(object)uiLabel1);
		((Control)uiPanel2).Controls.Add((Control)(object)btnOpenConfig);
		((Control)uiPanel2).Controls.Add((Control)(object)btnSaveConfig);
		((Control)uiPanel2).Controls.Add((Control)(object)btnReturn);
		((Control)uiPanel2).Controls.Add((Control)(object)btnPiPei);
		((Control)uiPanel2).Controls.Add((Control)(object)btnStopAll);
		((Control)uiPanel2).Controls.Add((Control)(object)btnStartAll);
		((Control)uiPanel2).Controls.Add((Control)(object)uiSwitch1);
		((Control)uiPanel2).Controls.Add((Control)(object)uiLabel3);
		((Control)uiPanel2).Dock = (DockStyle)5;
		uiPanel2.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel2).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel2).ForeColor = Color.Silver;
		((Control)uiPanel2).Location = new Point(0, 0);
		((Control)uiPanel2).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel2).MinimumSize = new Size(1, 1);
		((Control)uiPanel2).Name = "uiPanel2";
		uiPanel2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel2).Size = new Size(1233, 61);
		uiPanel2.Style = UIStyle.Black;
		((Control)uiPanel2).TabIndex = 0;
		((Control)uiPanel2).Text = null;
		uiPanel2.TextAlignment = (ContentAlignment)32;
		switchStart.Active = true;
		switchStart.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)switchStart).BackColor = Color.Transparent;
		((Control)switchStart).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)switchStart).Location = new Point(105, 75);
		((Control)switchStart).MinimumSize = new Size(1, 1);
		((Control)switchStart).Name = "switchStart";
		((Control)switchStart).Size = new Size(75, 29);
		switchStart.Style = UIStyle.Black;
		((Control)switchStart).TabIndex = 76;
		((Control)switchStart).Text = "uiSwitch2";
		switchStart.ValueChanged += switchStart_ValueChanged;
		((Control)uiLabel8).BackColor = Color.Transparent;
		((Control)uiLabel8).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel8).ForeColor = Color.Silver;
		((Control)uiLabel8).Location = new Point(13, 76);
		((Control)uiLabel8).Name = "uiLabel8";
		((Control)uiLabel8).Size = new Size(93, 28);
		uiLabel8.Style = UIStyle.Black;
		((Control)uiLabel8).TabIndex = 77;
		((Control)uiLabel8).Text = "上电启动:";
		((Label)uiLabel8).TextAlign = (ContentAlignment)32;
		switchLight.Active = true;
		switchLight.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)switchLight).BackColor = Color.Transparent;
		((Control)switchLight).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)switchLight).Location = new Point(950, 18);
		((Control)switchLight).MinimumSize = new Size(1, 1);
		((Control)switchLight).Name = "switchLight";
		((Control)switchLight).Size = new Size(75, 29);
		switchLight.Style = UIStyle.Black;
		((Control)switchLight).TabIndex = 66;
		((Control)switchLight).Text = "uiSwitch1";
		switchLight.ValueChanged += switchLight_ValueChanged;
		((Control)btnImport).Cursor = Cursors.Hand;
		btnImport.FillColor = Color.FromArgb(15, 40, 70);
		btnImport.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnImport.FillPressColor = Color.FromArgb(235, 243, 255);
		btnImport.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnImport).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnImport.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnImport.ForePressColor = Color.FromArgb(130, 130, 130);
		btnImport.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnImport).Location = new Point(1035, 18);
		((Control)btnImport).MinimumSize = new Size(1, 1);
		((Control)btnImport).Name = "btnImport";
		btnImport.Radius = 26;
		btnImport.RectColor = Color.FromArgb(130, 130, 130);
		btnImport.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnImport.RectPressColor = Color.FromArgb(130, 130, 130);
		btnImport.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnImport).Size = new Size(73, 29);
		btnImport.Style = UIStyle.Black;
		((Control)btnImport).TabIndex = 75;
		((Control)btnImport).Text = "导出";
		((Control)btnImport).Click += btnImport_Click;
		((Control)cbmCom).BackColor = Color.Black;
		cbmCom.DataSource = null;
		cbmCom.DropDownStyle = UIDropDownStyle.DropDownList;
		cbmCom.FillColor = Color.White;
		((Control)cbmCom).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)cbmCom).Location = new Point(89, 18);
		((Control)cbmCom).Margin = new Padding(4);
		((Control)cbmCom).MinimumSize = new Size(62, 0);
		((Control)cbmCom).Name = "cbmCom";
		((Control)cbmCom).Padding = new Padding(0, 0, 42, 2);
		cbmCom.Radius = 15;
		cbmCom.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbmCom).Size = new Size(95, 29);
		cbmCom.Style = UIStyle.Black;
		((Control)cbmCom).TabIndex = 15;
		cbmCom.TextAlignment = (ContentAlignment)16;
		cbmCom.SelectedIndexChanged += cbmCom_SelectedIndexChanged;
		sw_Large.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)sw_Large).Font = new Font("微软雅黑", 12f);
		((Control)sw_Large).Location = new Point(985, 20);
		((Control)sw_Large).MinimumSize = new Size(1, 1);
		((Control)sw_Large).Name = "sw_Large";
		((Control)sw_Large).Size = new Size(75, 29);
		sw_Large.Style = UIStyle.Black;
		((Control)sw_Large).TabIndex = 13;
		((Control)sw_Large).Text = "uiSwitch1";
		((Control)sw_Large).Visible = false;
		sw_Large.ValueChanged += sw_Large_ValueChanged;
		((Control)uiLabel1).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel1).ForeColor = Color.Silver;
		((Control)uiLabel1).Location = new Point(903, 24);
		((Control)uiLabel1).Name = "uiLabel1";
		((Control)uiLabel1).Size = new Size(104, 23);
		uiLabel1.Style = UIStyle.Custom;
		((Control)uiLabel1).TabIndex = 14;
		((Control)uiLabel1).Text = "显示大小切换:";
		((Label)uiLabel1).TextAlign = (ContentAlignment)16;
		((Control)uiLabel1).Visible = false;
		((Control)btnOpenConfig).Cursor = Cursors.Hand;
		btnOpenConfig.FillColor = Color.FromArgb(15, 40, 70);
		btnOpenConfig.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnOpenConfig.FillPressColor = Color.FromArgb(235, 243, 255);
		btnOpenConfig.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnOpenConfig).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnOpenConfig.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.ForePressColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOpenConfig).Location = new Point(1114, 18);
		((Control)btnOpenConfig).MinimumSize = new Size(1, 1);
		((Control)btnOpenConfig).Name = "btnOpenConfig";
		btnOpenConfig.Radius = 26;
		btnOpenConfig.RectColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectPressColor = Color.FromArgb(130, 130, 130);
		btnOpenConfig.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnOpenConfig).Size = new Size(73, 29);
		btnOpenConfig.Style = UIStyle.Black;
		((Control)btnOpenConfig).TabIndex = 12;
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
		((Control)btnSaveConfig).Location = new Point(707, 18);
		((Control)btnSaveConfig).MinimumSize = new Size(1, 1);
		((Control)btnSaveConfig).Name = "btnSaveConfig";
		btnSaveConfig.Radius = 26;
		btnSaveConfig.RectColor = Color.FromArgb(130, 130, 130);
		btnSaveConfig.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSaveConfig.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSaveConfig.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSaveConfig).Size = new Size(75, 29);
		btnSaveConfig.Style = UIStyle.Black;
		((Control)btnSaveConfig).TabIndex = 12;
		((Control)btnSaveConfig).Text = "保存";
		((Control)btnSaveConfig).Click += btnSaveConfig_Click;
		((Control)btnReturn).Cursor = Cursors.Hand;
		btnReturn.FillColor = Color.FromArgb(15, 40, 70);
		btnReturn.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnReturn.FillPressColor = Color.FromArgb(235, 243, 255);
		btnReturn.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnReturn).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnReturn.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnReturn.ForePressColor = Color.FromArgb(130, 130, 130);
		btnReturn.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReturn).Location = new Point(9, 18);
		((Control)btnReturn).Margin = new Padding(2);
		((Control)btnReturn).MinimumSize = new Size(1, 1);
		((Control)btnReturn).Name = "btnReturn";
		btnReturn.Radius = 26;
		btnReturn.RectColor = Color.FromArgb(130, 130, 130);
		btnReturn.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnReturn.RectPressColor = Color.FromArgb(130, 130, 130);
		btnReturn.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnReturn).Size = new Size(74, 29);
		btnReturn.Style = UIStyle.Black;
		((Control)btnReturn).TabIndex = 2;
		((Control)btnReturn).Text = "返回";
		((Control)btnReturn).Click += uiButton7_Click;
		((Control)btnPiPei).Cursor = Cursors.Hand;
		btnPiPei.FillColor = Color.FromArgb(15, 40, 70);
		btnPiPei.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnPiPei.FillPressColor = Color.FromArgb(235, 243, 255);
		btnPiPei.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnPiPei).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnPiPei.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnPiPei.ForePressColor = Color.FromArgb(130, 130, 130);
		btnPiPei.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnPiPei).Location = new Point(608, 18);
		((Control)btnPiPei).Margin = new Padding(2);
		((Control)btnPiPei).MinimumSize = new Size(1, 1);
		((Control)btnPiPei).Name = "btnPiPei";
		btnPiPei.Radius = 26;
		btnPiPei.RectColor = Color.FromArgb(130, 130, 130);
		btnPiPei.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnPiPei.RectPressColor = Color.FromArgb(130, 130, 130);
		btnPiPei.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnPiPei).Size = new Size(88, 29);
		btnPiPei.Style = UIStyle.Black;
		((Control)btnPiPei).TabIndex = 2;
		((Control)btnPiPei).Text = "匹配遥控器";
		((Control)btnPiPei).Click += btnPiPei_Click;
		((Control)btnStopAll).Cursor = Cursors.Hand;
		btnStopAll.FillColor = Color.FromArgb(15, 40, 70);
		btnStopAll.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnStopAll.FillPressColor = Color.FromArgb(235, 243, 255);
		btnStopAll.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnStopAll).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnStopAll.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnStopAll.ForePressColor = Color.FromArgb(130, 130, 130);
		btnStopAll.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStopAll).Location = new Point(516, 18);
		((Control)btnStopAll).Margin = new Padding(2);
		((Control)btnStopAll).MinimumSize = new Size(1, 1);
		((Control)btnStopAll).Name = "btnStopAll";
		btnStopAll.Radius = 26;
		btnStopAll.RectColor = Color.FromArgb(130, 130, 130);
		btnStopAll.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnStopAll.RectPressColor = Color.FromArgb(130, 130, 130);
		btnStopAll.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStopAll).Size = new Size(88, 29);
		btnStopAll.Style = UIStyle.Black;
		((Control)btnStopAll).TabIndex = 2;
		((Control)btnStopAll).Text = "停止";
		((Control)btnStopAll).Click += btnStopAll_Click;
		((Control)btnStartAll).Cursor = Cursors.Hand;
		btnStartAll.FillColor = Color.FromArgb(15, 40, 70);
		btnStartAll.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnStartAll.FillPressColor = Color.FromArgb(235, 243, 255);
		btnStartAll.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnStartAll).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnStartAll.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnStartAll.ForePressColor = Color.FromArgb(130, 130, 130);
		btnStartAll.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStartAll).Location = new Point(423, 18);
		((Control)btnStartAll).Margin = new Padding(2);
		((Control)btnStartAll).MinimumSize = new Size(1, 1);
		((Control)btnStartAll).Name = "btnStartAll";
		btnStartAll.Radius = 26;
		btnStartAll.RectColor = Color.FromArgb(130, 130, 130);
		btnStartAll.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnStartAll.RectPressColor = Color.FromArgb(130, 130, 130);
		btnStartAll.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStartAll).Size = new Size(89, 29);
		btnStartAll.Style = UIStyle.Black;
		((Control)btnStartAll).TabIndex = 2;
		((Control)btnStartAll).Text = "启动";
		((Control)btnStartAll).Click += btnStartAll_Click;
		uiSwitch1.Active = true;
		uiSwitch1.ActiveColor = Color.FromArgb(15, 40, 70);
		((Control)uiSwitch1).BackColor = Color.Transparent;
		((Control)uiSwitch1).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiSwitch1).Location = new Point(345, 17);
		((Control)uiSwitch1).MinimumSize = new Size(1, 1);
		((Control)uiSwitch1).Name = "uiSwitch1";
		((Control)uiSwitch1).Size = new Size(75, 29);
		uiSwitch1.Style = UIStyle.Black;
		((Control)uiSwitch1).TabIndex = 74;
		((Control)uiSwitch1).Text = "uiSwitch1";
		uiSwitch1.ValueChanged += uiSwitch1_ValueChanged;
		((Control)uiLabel3).BackColor = Color.Transparent;
		((Control)uiLabel3).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel3).ForeColor = Color.Silver;
		((Control)uiLabel3).Location = new Point(185, 20);
		((Control)uiLabel3).Name = "uiLabel3";
		((Control)uiLabel3).Size = new Size(161, 23);
		uiLabel3.Style = UIStyle.Custom;
		((Control)uiLabel3).TabIndex = 73;
		((Control)uiLabel3).Text = "投屏:";
		((Label)uiLabel3).TextAlign = (ContentAlignment)64;
		((Control)uiLabel7).BackColor = Color.Transparent;
		((Control)uiLabel7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel7).ForeColor = Color.Silver;
		((Control)uiLabel7).Location = new Point(788, 19);
		((Control)uiLabel7).Name = "uiLabel7";
		((Control)uiLabel7).Size = new Size(151, 28);
		uiLabel7.Style = UIStyle.Custom;
		((Control)uiLabel7).TabIndex = 67;
		((Control)uiLabel7).Text = "呼吸灯开关:";
		((Label)uiLabel7).TextAlign = (ContentAlignment)64;
		((Control)uiPanel4).Controls.Add((Control)(object)switchStart);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel8);
		((Control)uiPanel4).Controls.Add((Control)(object)btnLeft);
		((Control)uiPanel4).Controls.Add((Control)(object)btnRigth);
		((Control)uiPanel4).Controls.Add((Control)(object)btnButtom);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel5);
		((Control)uiPanel4).Controls.Add((Control)(object)btnTop);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel6);
		((Control)uiPanel4).Controls.Add((Control)(object)trackBarScale);
		((Control)uiPanel4).Controls.Add((Control)(object)btnReset);
		((Control)uiPanel4).Controls.Add((Control)(object)uiPanelDraw);
		((Control)uiPanel4).Controls.Add((Control)(object)lblBright);
		((Control)uiPanel4).Controls.Add((Control)(object)cbxBackground);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel2);
		((Control)uiPanel4).Controls.Add((Control)(object)cbxSpeed);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel12);
		((Control)uiPanel4).Controls.Add((Control)(object)trackBarLiangDu);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLine5);
		((Control)uiPanel4).Controls.Add((Control)(object)btnSelectImage);
		((Control)uiPanel4).Controls.Add((Control)(object)lblzoom);
		((Control)uiPanel4).Controls.Add((Control)(object)uiLabel4);
		((Control)uiPanel4).Dock = (DockStyle)4;
		uiPanel4.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel4).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel4).ForeColor = Color.Silver;
		((Control)uiPanel4).Location = new Point(833, 0);
		((Control)uiPanel4).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel4).MinimumSize = new Size(1, 1);
		((Control)uiPanel4).Name = "uiPanel4";
		uiPanel4.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel4).Size = new Size(400, 663);
		uiPanel4.Style = UIStyle.Black;
		((Control)uiPanel4).TabIndex = 3;
		((Control)uiPanel4).Text = null;
		uiPanel4.TextAlignment = (ContentAlignment)32;
		((Control)btnLeft).Cursor = Cursors.Hand;
		((Control)btnLeft).Font = new Font("微软雅黑", 12f);
		((PictureBox)btnLeft).Image = (Image)(object)Resources._2023_09_06_141700;
		((Control)btnLeft).Location = new Point(223, 532);
		((Control)btnLeft).Name = "btnLeft";
		((Control)btnLeft).Size = new Size(27, 40);
		((PictureBox)btnLeft).TabIndex = 81;
		((PictureBox)btnLeft).TabStop = false;
		((Control)btnLeft).Text = null;
		((Control)btnLeft).Visible = false;
		((Control)btnRigth).Cursor = Cursors.Hand;
		((Control)btnRigth).Font = new Font("微软雅黑", 12f);
		((PictureBox)btnRigth).Image = (Image)(object)Resources._2023_09_06_141641;
		((Control)btnRigth).Location = new Point(256, 532);
		((Control)btnRigth).Name = "btnRigth";
		((Control)btnRigth).Size = new Size(30, 40);
		((PictureBox)btnRigth).TabIndex = 82;
		((PictureBox)btnRigth).TabStop = false;
		((Control)btnRigth).Text = null;
		((Control)btnRigth).Visible = false;
		((Control)btnButtom).Cursor = Cursors.Hand;
		((Control)btnButtom).Font = new Font("微软雅黑", 12f);
		((PictureBox)btnButtom).Image = (Image)(object)Resources._2023_09_06_135750;
		((Control)btnButtom).Location = new Point(88, 551);
		((Control)btnButtom).Name = "btnButtom";
		((Control)btnButtom).Size = new Size(43, 31);
		((PictureBox)btnButtom).TabIndex = 83;
		((PictureBox)btnButtom).TabStop = false;
		((Control)btnButtom).Text = null;
		((Control)btnButtom).Visible = false;
		((Control)uiLabel5).BackColor = Color.Transparent;
		((Control)uiLabel5).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel5).ForeColor = Color.Silver;
		((Control)uiLabel5).Location = new Point(134, 537);
		((Control)uiLabel5).Name = "uiLabel5";
		((Control)uiLabel5).Size = new Size(93, 23);
		uiLabel5.Style = UIStyle.Custom;
		((Control)uiLabel5).TabIndex = 79;
		((Control)uiLabel5).Text = "水平:";
		((Label)uiLabel5).TextAlign = (ContentAlignment)32;
		((Control)uiLabel5).Visible = false;
		((Control)btnTop).Cursor = Cursors.Hand;
		((Control)btnTop).Font = new Font("微软雅黑", 12f);
		((PictureBox)btnTop).Image = (Image)(object)Resources._2023_09_06_135731;
		((Control)btnTop).Location = new Point(88, 515);
		((Control)btnTop).Name = "btnTop";
		((Control)btnTop).Size = new Size(43, 31);
		((PictureBox)btnTop).TabIndex = 84;
		((PictureBox)btnTop).TabStop = false;
		((Control)btnTop).Text = null;
		((Control)btnTop).Visible = false;
		((Control)uiLabel6).BackColor = Color.Transparent;
		((Control)uiLabel6).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel6).ForeColor = Color.Silver;
		((Control)uiLabel6).Location = new Point(6, 537);
		((Control)uiLabel6).Name = "uiLabel6";
		((Control)uiLabel6).Size = new Size(76, 23);
		uiLabel6.Style = UIStyle.Custom;
		((Control)uiLabel6).TabIndex = 80;
		((Control)uiLabel6).Text = "垂直:";
		((Label)uiLabel6).TextAlign = (ContentAlignment)32;
		((Control)uiLabel6).Visible = false;
		trackBarScale.DisableColor = Color.Silver;
		trackBarScale.FillColor = Color.FromArgb(24, 24, 24);
		((Control)trackBarScale).Font = new Font("微软雅黑", 12f);
		((Control)trackBarScale).Location = new Point(152, 471);
		trackBarScale.Maximum = 1024;
		trackBarScale.Minimum = 64;
		((Control)trackBarScale).MinimumSize = new Size(1, 1);
		((Control)trackBarScale).Name = "trackBarScale";
		((Control)trackBarScale).Size = new Size(193, 25);
		trackBarScale.Style = UIStyle.Black;
		((Control)trackBarScale).TabIndex = 76;
		((Control)trackBarScale).Text = "uiTrackBar6";
		trackBarScale.Value = 1024;
		trackBarScale.ValueChanged += trackBarScale_ValueChanged;
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
		((Control)btnReset).Location = new Point(298, 537);
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
		((Control)btnReset).TabIndex = 11;
		((Control)btnReset).Text = "重置";
		((Control)btnReset).Click += btnReset_Click;
		uiPanelDraw.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanelDraw).Font = new Font("微软雅黑", 12f);
		((Control)uiPanelDraw).ForeColor = Color.Silver;
		((Control)uiPanelDraw).Location = new Point(11, 216);
		((Control)uiPanelDraw).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanelDraw).MinimumSize = new Size(1, 1);
		((Control)uiPanelDraw).Name = "uiPanelDraw";
		uiPanelDraw.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanelDraw).Size = new Size(379, 222);
		uiPanelDraw.Style = UIStyle.Black;
		((Control)uiPanelDraw).TabIndex = 75;
		((Control)uiPanelDraw).Text = null;
		uiPanelDraw.TextAlignment = (ContentAlignment)32;
		((Control)uiPanelDraw).Paint += new PaintEventHandler(uiPanel5_Paint);
		((Control)lblBright).Font = new Font("微软雅黑", 12f);
		((Control)lblBright).ForeColor = Color.Silver;
		((Control)lblBright).Location = new Point(325, 157);
		((Control)lblBright).Name = "lblBright";
		((Control)lblBright).Size = new Size(52, 23);
		lblBright.Style = UIStyle.Custom;
		((Control)lblBright).TabIndex = 72;
		((Control)lblBright).Text = "255";
		((Label)lblBright).TextAlign = (ContentAlignment)16;
		cbxBackground.DataSource = null;
		cbxBackground.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxBackground.FillColor = Color.White;
		((Control)cbxBackground).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxBackground.Items.AddRange(new object[3] { "暖色", "标准", "冷色" });
		((Control)cbxBackground).Location = new Point(259, 22);
		((Control)cbxBackground).Margin = new Padding(4);
		((Control)cbxBackground).MinimumSize = new Size(62, 0);
		((Control)cbxBackground).Name = "cbxBackground";
		((Control)cbxBackground).Padding = new Padding(0, 0, 42, 2);
		cbxBackground.Radius = 15;
		cbxBackground.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxBackground).Size = new Size(118, 29);
		cbxBackground.Style = UIStyle.Black;
		((Control)cbxBackground).TabIndex = 71;
		((Control)cbxBackground).Text = "标准";
		cbxBackground.TextAlignment = (ContentAlignment)16;
		cbxBackground.SelectedIndexChanged += cbxBackground_SelectedIndexChanged;
		((Control)uiLabel2).BackColor = Color.Transparent;
		((Control)uiLabel2).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel2).ForeColor = Color.Silver;
		((Control)uiLabel2).Location = new Point(212, 22);
		((Control)uiLabel2).Name = "uiLabel2";
		((Control)uiLabel2).Size = new Size(53, 29);
		uiLabel2.Style = UIStyle.Custom;
		((Control)uiLabel2).TabIndex = 70;
		((Control)uiLabel2).Text = "色调:";
		((Label)uiLabel2).TextAlign = (ContentAlignment)16;
		cbxSpeed.DataSource = null;
		cbxSpeed.DropDownStyle = UIDropDownStyle.DropDownList;
		cbxSpeed.FillColor = Color.White;
		((Control)cbxSpeed).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		cbxSpeed.Items.AddRange(new object[2] { "750", "900" });
		((Control)cbxSpeed).Location = new Point(89, 22);
		((Control)cbxSpeed).Margin = new Padding(4);
		((Control)cbxSpeed).MinimumSize = new Size(62, 0);
		((Control)cbxSpeed).Name = "cbxSpeed";
		((Control)cbxSpeed).Padding = new Padding(0, 0, 42, 2);
		cbxSpeed.Radius = 15;
		cbxSpeed.RectColor = Color.FromArgb(130, 130, 130);
		((Control)cbxSpeed).Size = new Size(85, 29);
		cbxSpeed.Style = UIStyle.Black;
		((Control)cbxSpeed).TabIndex = 69;
		((Control)cbxSpeed).Text = "750";
		cbxSpeed.TextAlignment = (ContentAlignment)16;
		cbxSpeed.SelectedIndexChanged += cbxSpeed_SelectedIndexChanged;
		((Control)uiLabel12).BackColor = Color.Transparent;
		((Control)uiLabel12).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel12).ForeColor = Color.Silver;
		((Control)uiLabel12).Location = new Point(11, 22);
		((Control)uiLabel12).Name = "uiLabel12";
		((Control)uiLabel12).Size = new Size(71, 29);
		uiLabel12.Style = UIStyle.Custom;
		((Control)uiLabel12).TabIndex = 68;
		((Control)uiLabel12).Text = "转速:";
		((Label)uiLabel12).TextAlign = (ContentAlignment)32;
		((Control)trackBarLiangDu).BackColor = Color.Black;
		trackBarLiangDu.DisableColor = Color.Silver;
		trackBarLiangDu.FillColor = Color.FromArgb(24, 24, 24);
		((Control)trackBarLiangDu).Font = new Font("微软雅黑", 12f);
		((Control)trackBarLiangDu).Location = new Point(29, 157);
		trackBarLiangDu.Maximum = 255;
		((Control)trackBarLiangDu).MinimumSize = new Size(1, 1);
		((Control)trackBarLiangDu).Name = "trackBarLiangDu";
		((Control)trackBarLiangDu).Size = new Size(291, 25);
		trackBarLiangDu.Style = UIStyle.Black;
		((Control)trackBarLiangDu).TabIndex = 65;
		((Control)trackBarLiangDu).Text = "uiTrackBar5";
		trackBarLiangDu.Value = 255;
		trackBarLiangDu.ValueChanged += trackBarLiangDu_ValueChanged;
		((Control)uiLine5).BackColor = Color.Black;
		uiLine5.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiLine5).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLine5).ForeColor = Color.Silver;
		uiLine5.LineColor = Color.FromArgb(130, 130, 130);
		((Control)uiLine5).Location = new Point(29, 122);
		((Control)uiLine5).MinimumSize = new Size(2, 2);
		((Control)uiLine5).Name = "uiLine5";
		((Control)uiLine5).Size = new Size(331, 29);
		uiLine5.Style = UIStyle.Black;
		((Control)uiLine5).TabIndex = 64;
		((Control)uiLine5).Text = "亮度调节";
		uiLine5.TextAlign = (ContentAlignment)16;
		((Control)btnSelectImage).Cursor = Cursors.Hand;
		btnSelectImage.FillColor = Color.FromArgb(15, 40, 70);
		btnSelectImage.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnSelectImage.FillPressColor = Color.FromArgb(235, 243, 255);
		btnSelectImage.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnSelectImage).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnSelectImage.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnSelectImage.ForePressColor = Color.FromArgb(130, 130, 130);
		btnSelectImage.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSelectImage).Location = new Point(2, 467);
		((Control)btnSelectImage).Margin = new Padding(2);
		((Control)btnSelectImage).MinimumSize = new Size(1, 1);
		((Control)btnSelectImage).Name = "btnSelectImage";
		btnSelectImage.Radius = 26;
		btnSelectImage.RectColor = Color.FromArgb(130, 130, 130);
		btnSelectImage.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSelectImage.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSelectImage.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSelectImage).Size = new Size(93, 29);
		btnSelectImage.Style = UIStyle.Black;
		((Control)btnSelectImage).TabIndex = 2;
		((Control)btnSelectImage).Text = "选择区域";
		((Control)btnSelectImage).Click += btnSelectImage_Click;
		((Control)lblzoom).BackColor = Color.Transparent;
		((Control)lblzoom).Font = new Font("微软雅黑", 12f);
		((Control)lblzoom).ForeColor = Color.Silver;
		((Control)lblzoom).Location = new Point(341, 471);
		((Control)lblzoom).Name = "lblzoom";
		((Control)lblzoom).Size = new Size(62, 23);
		lblzoom.Style = UIStyle.Custom;
		((Control)lblzoom).TabIndex = 78;
		((Control)lblzoom).Text = "1024";
		((Label)lblzoom).TextAlign = (ContentAlignment)16;
		((Control)uiLabel4).BackColor = Color.Transparent;
		((Control)uiLabel4).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel4).ForeColor = Color.Silver;
		((Control)uiLabel4).Location = new Point(90, 471);
		((Control)uiLabel4).Name = "uiLabel4";
		((Control)uiLabel4).Size = new Size(64, 23);
		uiLabel4.Style = UIStyle.Custom;
		((Control)uiLabel4).TabIndex = 77;
		((Control)uiLabel4).Text = "缩放:";
		((Label)uiLabel4).TextAlign = (ContentAlignment)64;
		((Control)uiPanel3).Controls.Add((Control)(object)lblName);
		((Control)uiPanel3).Controls.Add((Control)(object)btnSetID);
		((Control)uiPanel3).Controls.Add((Control)(object)btnInit);
		((Control)uiPanel3).Controls.Add((Control)(object)btnEdit);
		((Control)uiPanel3).Dock = (DockStyle)5;
		uiPanel3.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel3).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel3).ForeColor = Color.Silver;
		((Control)uiPanel3).Location = new Point(0, 0);
		((Control)uiPanel3).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel3).MinimumSize = new Size(1, 1);
		((Control)uiPanel3).Name = "uiPanel3";
		uiPanel3.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel3).Size = new Size(1233, 663);
		uiPanel3.Style = UIStyle.Black;
		((Control)uiPanel3).TabIndex = 2;
		((Control)uiPanel3).Text = null;
		uiPanel3.TextAlignment = (ContentAlignment)32;
		((Control)uiPanel3).Paint += new PaintEventHandler(uiPanel3_Paint);
		((Control)lblName).Font = new Font("微软雅黑", 12f);
		((Control)lblName).ForeColor = Color.Silver;
		((Control)lblName).Location = new Point(19, 23);
		((Control)lblName).Name = "lblName";
		((Control)lblName).Size = new Size(100, 23);
		lblName.Style = UIStyle.Custom;
		((Control)lblName).TabIndex = 0;
		((Control)lblName).Text = "demo";
		((Label)lblName).TextAlign = (ContentAlignment)16;
		((Control)btnSetID).Cursor = Cursors.Hand;
		btnSetID.FillColor = Color.FromArgb(15, 40, 70);
		btnSetID.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnSetID.FillPressColor = Color.FromArgb(235, 243, 255);
		btnSetID.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnSetID).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnSetID.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnSetID.ForePressColor = Color.FromArgb(130, 130, 130);
		btnSetID.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSetID).Location = new Point(524, 15);
		((Control)btnSetID).Margin = new Padding(2);
		((Control)btnSetID).MinimumSize = new Size(1, 1);
		((Control)btnSetID).Name = "btnSetID";
		btnSetID.Radius = 26;
		btnSetID.RectColor = Color.FromArgb(130, 130, 130);
		btnSetID.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnSetID.RectPressColor = Color.FromArgb(130, 130, 130);
		btnSetID.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnSetID).Size = new Size(74, 29);
		btnSetID.Style = UIStyle.Black;
		((Control)btnSetID).TabIndex = 2;
		((Control)btnSetID).Text = "设置ID";
		((Control)btnSetID).Click += btnSetID_Click;
		((Control)btnInit).Cursor = Cursors.Hand;
		btnInit.FillColor = Color.FromArgb(15, 40, 70);
		btnInit.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnInit.FillPressColor = Color.FromArgb(235, 243, 255);
		btnInit.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnInit).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnInit.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnInit.ForePressColor = Color.FromArgb(130, 130, 130);
		btnInit.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnInit).Location = new Point(610, 15);
		((Control)btnInit).Margin = new Padding(2);
		((Control)btnInit).MinimumSize = new Size(1, 1);
		((Control)btnInit).Name = "btnInit";
		btnInit.Radius = 26;
		btnInit.RectColor = Color.FromArgb(130, 130, 130);
		btnInit.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnInit.RectPressColor = Color.FromArgb(130, 130, 130);
		btnInit.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnInit).Size = new Size(74, 29);
		btnInit.Style = UIStyle.Black;
		((Control)btnInit).TabIndex = 2;
		((Control)btnInit).Text = "初始化";
		((Control)btnInit).Click += btnInit_Click;
		((Control)btnEdit).Cursor = Cursors.Hand;
		btnEdit.FillColor = Color.FromArgb(15, 40, 70);
		btnEdit.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnEdit.FillPressColor = Color.FromArgb(235, 243, 255);
		btnEdit.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnEdit).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnEdit.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnEdit.ForePressColor = Color.FromArgb(130, 130, 130);
		btnEdit.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnEdit).Location = new Point(699, 15);
		((Control)btnEdit).Margin = new Padding(2);
		((Control)btnEdit).MinimumSize = new Size(1, 1);
		((Control)btnEdit).Name = "btnEdit";
		btnEdit.Radius = 26;
		btnEdit.RectColor = Color.FromArgb(130, 130, 130);
		btnEdit.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnEdit.RectPressColor = Color.FromArgb(130, 130, 130);
		btnEdit.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnEdit).Size = new Size(74, 29);
		btnEdit.Style = UIStyle.Black;
		((Control)btnEdit).TabIndex = 2;
		((Control)btnEdit).Text = "编辑";
		((Control)btnEdit).Click += btnEdit_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(8f, 15f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Controls.Add((Control)(object)uiPanel1);
		((Control)this).ForeColor = SystemColors.ControlLight;
		((Control)this).Margin = new Padding(4);
		((Control)this).Name = "UserControl1";
		((Control)this).Size = new Size(1233, 728);
		((Control)this).VisibleChanged += UserControl1_VisibleChanged;
		((Control)uiPanel1).ResumeLayout(false);
		((Control)splitContainer1.Panel1).ResumeLayout(false);
		((Control)splitContainer1.Panel2).ResumeLayout(false);
		((ISupportInitialize)splitContainer1).EndInit();
		((Control)splitContainer1).ResumeLayout(false);
		((Control)uiPanel2).ResumeLayout(false);
		((Control)uiPanel4).ResumeLayout(false);
		((ISupportInitialize)(object)btnLeft).EndInit();
		((ISupportInitialize)(object)btnRigth).EndInit();
		((ISupportInitialize)(object)btnButtom).EndInit();
		((ISupportInitialize)(object)btnTop).EndInit();
		((Control)uiPanel3).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
	}
}
