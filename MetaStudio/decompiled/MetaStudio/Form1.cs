using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;

namespace MetaStudio;

public class Form1 : UIForm
{
	private enum ProcessDPIAwareness
	{
		ProcessDPIUnaware,
		ProcessSystemDPIAware,
		ProcessPerMonitorDPIAware
	}

	private List<Data> datas = new List<Data>();

	private SerialPort sp = new SerialPort();

	private int x;

	private int y;

	private int x1;

	private int y1;

	public UserControl1 c1 = null;

	public UserControl2 c2 = null;

	public UserControl3 u3 = null;

	public UserControl4 u4 = null;

	public UserControl5 u5 = null;

	public UserControl6 u6 = null;

	private string drawString = "";

	private string frontDrawString = "";

	private int heigth = 1080;

	private int width = 1920;

	private int realHeight = 1080;

	private int realWidth = 1080;

	private string serialName = string.Empty;

	private int row = 2;

	private int col = 3;

	public List<GridData> lstData = new List<GridData>();

	public Dictionary<string, string> dic = new Dictionary<string, string>();

	private Dictionary<string, ConfigData> dicConfig = new Dictionary<string, ConfigData>();

	private int ori_offsetX = 140;

	private int ori_offsetY = 75;

	private int offsetX_Rect = 100;

	private int offsetY_Rect = 40;

	public bool isbottom = true;

	private GridData data;

	private string editNum = string.Empty;

	public List<RegInfo> lstAllReg = new List<RegInfo>();

	private EidtMode editMode = EidtMode.Add;

	private Dictionary<string, Point> dicPoint = new Dictionary<string, Point>();

	private DataGridViewButtonColumn btn2 = null;

	private IContainer components = null;

	private TabPage tabPage1;

	private UITabControl uiTabControl1;

	private TabPage tabPage2;

	private SplitContainer splitContainer1;

	private UIPanel uiPanel6;

	private UIButton btnDelDev;

	private UITextBox txtCol;

	private UILabel uiLabel11;

	private UITextBox txtRow;

	private UIButton uiButton7;

	private UIButton uiButton8;

	private UILabel uiLabel10;

	private UIButton btnSave;

	private UITextBox txtName;

	private UILabel uiLabel9;

	private UIPanel uiPanel5;

	private UINavMenu uiNavMenu1;

	private UIStyleManager uiStyleManager1;

	private UIDataGridView uiDataGridView1;

	private UILabel lblInfo;

	private UICheckBox cbxIsBottom;

	private UIContextMenuStrip uiContextMenuStrip1;

	private ToolStripMenuItem china;

	private ToolStripMenuItem english;

	private UIRadioButton radS;

	private UIRadioButton radN;

	private TabPage tabPage3;

	private Panel panel1;

	private PictureBox pictureBox1;

	[DllImport("shcore.dll")]
	private static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);

	public Form1()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		InitializeComponent();
		InitSys();
		InitSerial();
		x = 200;
		lstData = JsonHelper.ReadJson(ConstData.jsonfilename);
		if (lstData == null)
		{
			lstData = new List<GridData>();
		}
		InitDataGrid();
		SPHelper.mainFrm = this;
		QueryFeq();
		ConstData.versionType = AppConfig.GetAppSetting("versionType");
		if (ConstData.versionType == "0")
		{
			china_Click(null, null);
		}
		else if (ConstData.versionType == "1")
		{
			english_Click(null, null);
		}
		SetProcessDpiAwareness(ProcessDPIAwareness.ProcessSystemDPIAware);
		((Form)this).Size = new Size(1299, 897);
	}

	private void InitSys()
	{
		((Control)splitContainer1).Visible = true;
		c1 = new UserControl1(this, splitContainer1);
		((Control)c1).Visible = false;
		((Control)c1).Width = 100;
		((Control)c1).Height = 100;
		((Control)c1).Dock = (DockStyle)5;
		((Control)tabPage2).Controls.Add((Control)(object)c1);
		c2 = new UserControl2(this, splitContainer1, c1);
		((Control)c2).Visible = false;
		((Control)c2).Width = 100;
		((Control)c2).Height = 100;
		((Control)c2).Dock = (DockStyle)5;
		c1.u2 = c2;
		((Control)tabPage2).Controls.Add((Control)(object)c2);
		u5 = new UserControl5(this);
		((Control)u5).Width = 100;
		((Control)u5).Height = 100;
		((Control)u5).Dock = (DockStyle)5;
		u6 = new UserControl6(this);
		((Control)u6).Width = 100;
		((Control)u6).Height = 100;
		((Control)u6).Dock = (DockStyle)5;
		((Control)tabPage1).Controls.Add((Control)(object)u6);
		((TabControl)uiTabControl1).TabPages.RemoveAt(1);
	}

	private void InitDataGrid()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		uiDataGridView1.AddColumn("SN", "Column0", 100, (DataGridViewContentAlignment)32);
		uiDataGridView1.AddColumn("行号", "Column1", 100, (DataGridViewContentAlignment)32).SetFixedMode(120);
		uiDataGridView1.AddColumn("名称", "Column2", 100, (DataGridViewContentAlignment)32);
		uiDataGridView1.AddColumn("大小", "Column3", 100, (DataGridViewContentAlignment)32);
		((DataGridViewBand)((DataGridView)uiDataGridView1).Columns["SN"]).Visible = false;
		((DataGridView)uiDataGridView1).ReadOnly = true;
		btn2 = new DataGridViewButtonColumn();
		((DataGridViewColumn)btn2).Width = 20;
		((DataGridViewColumn)btn2).Name = "de";
		((DataGridViewColumn)btn2).HeaderText = "操作";
		((DataGridViewBand)btn2).DefaultCellStyle.NullValue = "详情";
		((DataGridView)uiDataGridView1).Columns.Add((DataGridViewColumn)(object)btn2);
		if (lstData == null)
		{
			return;
		}
		foreach (GridData lstDatum in lstData)
		{
			AddDataToGrid(lstDatum);
		}
	}

	private void AddDataToGrid(GridData data)
	{
		int num = ((DataGridView)uiDataGridView1).Rows.Add();
		((DataGridView)uiDataGridView1).Rows[num].Cells[0].Value = data.Guid;
		((DataGridView)uiDataGridView1).Rows[num].Cells[1].Value = data.ID;
		((DataGridView)uiDataGridView1).Rows[num].Cells[2].Value = data.Name;
		((DataGridView)uiDataGridView1).Rows[num].Cells[3].Value = data.Size;
	}

	public void ShowForm(string msg, int waitTime)
	{
		Task task = new Task(delegate
		{
			ShowStatusForm(100, msg + "中......", 0);
			for (int i = 0; i < 100; i++)
			{
				SystemEx.Delay(waitTime);
				SetStatusFormDescription(msg + "中(" + i + "%)......");
				StatusFormStepIt();
			}
			HideStatusForm();
		});
		task.Start();
	}

	private void ResetSys()
	{
		QueryFeq();
	}

	public void ComChange(string _serialName)
	{
		CloseSerial();
		serialName = _serialName;
		if (OpenSerial())
		{
			ResetSys();
		}
	}

	public bool OpenSerial()
	{
		if (string.IsNullOrEmpty(serialName))
		{
			return false;
		}
		try
		{
			if (!sp.IsOpen)
			{
				sp.PortName = serialName;
				sp.BaudRate = 2000000;
				sp.DataBits = 8;
				sp.StopBits = (StopBits)1;
				sp.Parity = (Parity)0;
				sp.ReadBufferSize = 80000;
				sp.ReadTimeout = 100;
				sp.ReceivedBytesThreshold = 1;
				sp.Open();
			}
			return true;
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
			return false;
		}
	}

	public void CloseSerial()
	{
		if (sp != null && sp.IsOpen)
		{
			sp.Close();
		}
	}

	public void InitSerial()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		try
		{
			sp.DataReceived += new SerialDataReceivedEventHandler(Sp1_DataReceived);
			sp.DtrEnable = true;
			sp.RtsEnable = true;
			sp.ReadTimeout = 1000;
			sp.BaudRate = 2000000;
			sp.Close();
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public void GetComPort2(UIComboBox _cbmCom)
	{
		try
		{
			string[] portNames = SerialPort.GetPortNames();
			_cbmCom.Items.Clear();
			List<string> portDeviceName = Helper.GetPortDeviceName();
			string[] array = portNames;
			foreach (string text in array)
			{
				if (Helper.ContainerCom(text, portDeviceName))
				{
					_cbmCom.Items.Add((object)text);
					_cbmCom.SelectedIndex = 0;
				}
			}
			if (_cbmCom.SelectedItem != null)
			{
				serialName = _cbmCom.SelectedItem.ToString();
				_cbmCom.FillColor = Color.White;
			}
			else
			{
				_cbmCom.FillColor = Color.Red;
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public void GetComPort(UIComboBox _cbmCom)
	{
		try
		{
			string[] portNames = SerialPort.GetPortNames();
			_cbmCom.Items.Clear();
			List<string> portDeviceName = Helper.GetPortDeviceName();
			string[] array = portNames;
			foreach (string text in array)
			{
				if (Helper.ContainerCom(text, portDeviceName))
				{
					_cbmCom.Items.Add((object)text);
					_cbmCom.SelectedIndex = 0;
				}
			}
			if (_cbmCom.SelectedItem != null)
			{
				serialName = _cbmCom.SelectedItem.ToString();
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	private void Sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		try
		{
			if (!sp.IsOpen)
			{
				return;
			}
			byte[] array = new byte[sp.BytesToRead];
			sp.Read(array, 0, array.Length);
			if (ConstData.Importing)
			{
				ReadAndSaveReg(array);
				return;
			}
			if (c1 != null)
			{
				c1.GetSerData(array);
			}
			if (u3 != null)
			{
				u3.GetSerData(array);
			}
			if (u4 != null && ((Control)u4).Visible)
			{
				u4.GetSerRegData(array);
			}
			if (c2 != null)
			{
				c2.GetSerData(array);
			}
			if (u5 != null && ((Control)u5).Visible)
			{
				u5.GetSerData(array);
			}
			if (u6 != null && ((Control)u6).Visible)
			{
				u6.GetSerData(array);
			}
			ParseData(array);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public byte[] Send(byte[] buf)
	{
		try
		{
			if (buf == null || buf.Length == 0)
			{
				return buf;
			}
			if (!sp.IsOpen)
			{
				OpenSerial();
			}
			byte[] array = Helper.CheckCRC(buf);
			if (sp.IsOpen)
			{
				sp.Write(array, 0, array.Length);
				return array;
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
		return buf;
	}

	public void SaveConfig()
	{
		ShowForm("保存", 30);
		RegisterHelper.SaveConfig(0);
	}

	public void OutputConfig()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		try
		{
			SaveFileDialog val = new SaveFileDialog();
			((FileDialog)val).Filter = "json文件|*.json|所有文件|*.*";
			if ((int)((CommonDialog)val).ShowDialog() == 1)
			{
				lstAllReg = new List<RegInfo>();
				ShowForm("保存", 150);
				string filePath = ((FileDialog)val).FileName;
				Task task = new Task(delegate
				{
					ConstData.Importing = true;
					RegisterHelper.ReadAllReg(1);
					Thread.Sleep(1000);
					JsonHelper.WriteConfigJson2(lstAllReg, filePath);
					ConstData.Importing = false;
				});
				task.Start();
			}
		}
		catch (Exception ex)
		{
			ConstData.Importing = false;
			LogerHelper.Error(ex.Message);
		}
	}

	public void ImportConfig()
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
			ShowForm("导入", 150);
			string[] fileNames = ((FileDialog)val).FileNames;
			foreach (string filename in fileNames)
			{
				List<RegInfo> lstReg = JsonHelper.ReadConfigJson2(filename);
				RegisterHelper.ImportConfig(lstReg);
			}
			MetaTool.EnableRGBReg(1);
			RegisterHelper.SaveConfig(1);
		}
	}

	public void ReadAndSaveReg(byte[] receivedData)
	{
		if (receivedData != null && receivedData.Length != 0 && receivedData.Length != 20)
		{
			if (receivedData[4] == 129 && receivedData[7] == 128)
			{
				RegInfo regInfo = new RegInfo();
				regInfo.deviceID = receivedData[6];
				regInfo.devType = 2;
				regInfo.value1 = Helper.Encryption(receivedData[16]);
				regInfo.value2 = SPHelper.ConvetInt(receivedData, 20);
				RegInfo item = regInfo;
				lstAllReg.Add(item);
			}
			else if (receivedData[4] == 129 && receivedData[7] == 0)
			{
				RegInfo regInfo2 = new RegInfo();
				regInfo2.deviceID = receivedData[6];
				regInfo2.devType = 1;
				regInfo2.value1 = Helper.Encryption(receivedData[16]);
				regInfo2.value2 = SPHelper.ConvetInt(receivedData, 20);
				RegInfo item = regInfo2;
				lstAllReg.Add(item);
			}
			else if (receivedData[4] == 1 && receivedData[7] == 0)
			{
				RegInfo regInfo3 = new RegInfo();
				regInfo3.deviceID = receivedData[6];
				regInfo3.devType = 0;
				regInfo3.value1 = Helper.Encryption(receivedData[16]);
				regInfo3.value2 = SPHelper.ConvetInt(receivedData, 20);
				RegInfo item = regInfo3;
				lstAllReg.Add(item);
			}
		}
	}

	private void ParseData(byte[] receivedData)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (receivedData == null || receivedData.Length == 0 || receivedData.Length == 20)
			{
				return;
			}
			if (receivedData[4] == 1 && receivedData[16] == 9)
			{
				if (receivedData[20] == 1)
				{
					QueryFeq();
				}
				else
				{
					MessageBox.Show("分辨率无效！", "系统提示", (MessageBoxButtons)0, (MessageBoxIcon)64);
				}
			}
			else if (receivedData[4] == 1 && receivedData[16] == 10)
			{
				int num = SPHelper.ConvetShort(receivedData, 20);
				int num2 = SPHelper.ConvetShort(receivedData, 22);
				if (num == 0)
				{
					ConstData.Ori_Height = (heigth = 1080);
				}
				else
				{
					ConstData.Ori_Height = (heigth = num);
				}
				if (num2 == 0)
				{
					ConstData.Ori_Width = (width = 1920);
				}
				else
				{
					ConstData.Ori_Width = (width = num2);
				}
			}
			else if (receivedData[4] == 1 && receivedData[16] == 7)
			{
				byte[] array = new byte[2]
				{
					receivedData[20],
					receivedData[21]
				};
				realHeight = BitConverter.ToInt16(array, 0);
				array[0] = receivedData[22];
				array[1] = receivedData[23];
				realWidth = BitConverter.ToInt16(array, 0);
			}
			else if (receivedData[4] == 129 && receivedData[7] == 128)
			{
				switch (receivedData[16])
				{
				case 36:
					ConstData.R_reg_1 = SPHelper.ConvetInt(receivedData, 20);
					break;
				case 37:
					ConstData.G_reg_1 = SPHelper.ConvetInt(receivedData, 20);
					break;
				case 38:
					ConstData.B_reg_1 = SPHelper.ConvetInt(receivedData, 20);
					break;
				case 26:
					ConstData.fan_ctrl = SPHelper.ConvetInt(receivedData, 20);
					break;
				}
			}
			else if (receivedData[4] == 129 && receivedData[7] == 0)
			{
				switch (receivedData[16])
				{
				case 3:
					ConstData.CurSpeed = SPHelper.ConvetInt(receivedData, 20);
					break;
				case 44:
					ConstData.canUpdate = true;
					ConstData.CurScale = SPHelper.ConvetShort(receivedData, 20);
					break;
				case 2:
					ConstData.CurAutoStart = SPHelper.ConvetShort(receivedData, 20);
					break;
				}
			}
			else if (receivedData[4] == 1 && receivedData[7] == 0)
			{
				byte b = receivedData[16];
				byte b2 = b;
				if (b2 == 34)
				{
					ConstData.CurLight = SPHelper.ConvetInt(receivedData, 20);
				}
			}
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public void CaptureScreen(Point point, int m_width, int m_height)
	{
		byte[] array = new byte[26]
		{
			240, 165, 90, 15, 1, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 5, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		byte[] bytes = BitConverter.GetBytes(point.X);
		byte[] bytes2 = BitConverter.GetBytes(point.Y);
		array[20] = bytes[0];
		array[21] = bytes[1];
		array[22] = bytes[2];
		array[23] = bytes[3];
		Send(array);
		byte[] array2 = new byte[26]
		{
			240, 165, 90, 15, 1, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 6, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		array2[20] = bytes2[0];
		array2[21] = bytes2[1];
		array2[22] = bytes2[2];
		array2[23] = bytes2[3];
		Send(array2);
		byte[] array3 = new byte[26]
		{
			240, 165, 90, 15, 1, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 7, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		int num = m_width;
		int num2 = m_height;
		if (m_width < 102)
		{
			num = 102;
		}
		if (m_height < 102)
		{
			num2 = 102;
		}
		byte[] bytes3 = BitConverter.GetBytes(num);
		byte[] bytes4 = BitConverter.GetBytes(num2);
		array3[20] = bytes4[0];
		array3[21] = bytes4[1];
		array3[22] = bytes3[0];
		array3[23] = bytes3[1];
		Send(array3);
		byte[] array4 = new byte[26]
		{
			240, 165, 90, 15, 1, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 11, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		ushort value = (ushort)((num + 1) * 2048 / 1025);
		ushort value2 = (ushort)((num2 + 1) * 2048 / 1025);
		byte[] bytes5 = BitConverter.GetBytes(value);
		byte[] bytes6 = BitConverter.GetBytes(value2);
		array4[20] = bytes6[0];
		array4[21] = bytes6[1];
		array4[22] = bytes5[0];
		array4[23] = bytes5[1];
		Send(array4);
		byte[] array5 = new byte[26]
		{
			240, 165, 90, 15, 1, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 17, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		Send(array5);
		array5[20] = 1;
		Send(array5);
		array5[20] = 0;
		Send(array5);
		byte[] buf = new byte[26]
		{
			240, 165, 90, 15, 1, 0, 0, 0, 2, 0,
			8, 0, 0, 0, 204, 204, 16, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		Send(buf);
	}

	private void uiPanel5_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Helper.DrawEllipse(graphics, row, col, offsetX_Rect, offsetY_Rect, isbottom);
	}

	private void uiNavMenu1_MenuItemClick(TreeNode node, NavMenuItem item, int pageIndex)
	{
		drawString = node.FullPath;
	}

	private void radN_CheckedChanged(object sender, EventArgs e)
	{
		AutoDel();
		if (!radN.Checked)
		{
			return;
		}
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

	private void radS_CheckedChanged(object sender, EventArgs e)
	{
		AutoDel();
		if (!radS.Checked)
		{
			return;
		}
		int num = 1;
		if (row % 2 != 0)
		{
			for (int num2 = row - 1; num2 >= 0; num2--)
			{
				if (num2 % 2 == 0)
				{
					for (int i = 0; i < col; i++)
					{
						AutoDraw2(num2, i, num++);
					}
				}
				else
				{
					for (int i = col - 1; i >= 0; i--)
					{
						AutoDraw2(num2, i, num++);
					}
				}
			}
			return;
		}
		for (int num2 = row - 1; num2 >= 0; num2--)
		{
			if (num2 % 2 != 0)
			{
				for (int i = 0; i < col; i++)
				{
					AutoDraw2(num2, i, num++);
				}
			}
			else
			{
				for (int i = col - 1; i >= 0; i--)
				{
					AutoDraw2(num2, i, num++);
				}
			}
		}
	}

	private void AutoDel()
	{
		dic.Clear();
		string text = "";
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < col; j++)
			{
				text = i + "-" + j;
				DelDrawString(text);
			}
		}
	}

	private void AutoDraw2(int r, int c, int count)
	{
		Point point = new Point(ori_offsetX + c * 90, ori_offsetY + r * 90);
		drawString = count.ToString();
		string text = r + "-" + c;
		if (Enumerable.Contains(dic.Keys, text))
		{
			dic[text] = drawString;
		}
		else
		{
			dic.Add(text, drawString);
		}
		Graphics g = ((Control)uiPanel5).CreateGraphics();
		Helper.DrawString6(g, new Point(point.X, point.Y), drawString, ori_offsetX, ori_offsetY);
		Console.WriteLine(point);
	}

	private void AutoDraw(Point point)
	{
		Console.WriteLine(point);
		int num = col * 100 - (col - 1) * 10;
		int num2 = row * 100 - (row - 1) * 10;
		if (point.X >= offsetX_Rect + num || point.Y >= offsetY_Rect + num2 || point.X <= offsetX_Rect || point.Y <= offsetY_Rect || string.IsNullOrEmpty(drawString))
		{
			return;
		}
		Graphics g = ((Control)uiPanel5).CreateGraphics();
		string key = Helper.GetKey2(dicPoint, point);
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		DelDrawString(key);
		Helper.DrawString(g, dicPoint[key], drawString, ori_offsetX, ori_offsetY);
		if (!string.IsNullOrEmpty(key))
		{
			if (Enumerable.Contains(dic.Keys, key))
			{
				dic[key] = drawString;
				dicConfig[key] = new ConfigData
				{
					id = Convert.ToInt32(drawString)
				};
			}
			else
			{
				dic.Add(key, drawString);
				dicConfig[key] = new ConfigData
				{
					id = Convert.ToInt32(drawString)
				};
			}
			frontDrawString = drawString;
			drawString = string.Empty;
		}
	}

	private void uiPanel5_MouseClick(object sender, MouseEventArgs e)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Invalid comparison between Unknown and I4
		int num = col * 100 - (col - 1) * 10;
		int num2 = row * 100 - (row - 1) * 10;
		if (e.X < offsetX_Rect + num && e.Y < offsetY_Rect + num2 && e.X > offsetX_Rect && e.Y > offsetY_Rect)
		{
			if ((int)e.Button == 1048576)
			{
				AutoDraw(new Point(e.X, e.Y));
			}
			else if ((int)e.Button == 2097152)
			{
				string key = Helper.GetKey2(dicPoint, new Point(e.X, e.Y));
				DelDrawString(key);
				drawString = frontDrawString;
			}
		}
	}

	private void btnSave_Click(object sender, EventArgs e)
	{
		SaveData();
	}

	public void SaveData()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		string text = string.Empty;
		string text2 = string.Empty;
		if (ConstData.versionType == "0")
		{
			text = "请设置单元编号!";
			text2 = "系统提示";
		}
		else if (ConstData.versionType == "1")
		{
			text = "Please set the unit number!";
			text2 = "System Prompt";
		}
		foreach (KeyValuePair<string, string> item in dic)
		{
			if (string.IsNullOrEmpty(item.Value))
			{
				MessageBox.Show(text, text2, (MessageBoxButtons)0, (MessageBoxIcon)64);
				return;
			}
		}
		if (editMode == EidtMode.Add)
		{
			GridData gridData = new GridData();
			gridData.Guid = Guid.NewGuid().ToString("N");
			gridData.ID = (lstData.Count + 1).ToString();
			gridData.Name = ((Control)txtName).Text;
			gridData.Size = ((Control)txtRow).Text + "x" + ((Control)txtCol).Text;
			gridData.Dic = dic;
			gridData.IsBottom = isbottom;
			gridData.DicConfig = dicConfig;
			c1.data = gridData;
		}
		else if (editMode == EidtMode.Eidt)
		{
			data.IsBottom = isbottom;
			JsonHelper.WriteJson(lstData, ConstData.jsonfilename);
			HideControl();
			HideGridControl();
			c1.ReDraw(lstData, editNum);
		}
	}

	private void btnDelDev_Click(object sender, EventArgs e)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		if (((BaseCollection)((DataGridView)uiDataGridView1).SelectedRows).Count <= 0)
		{
			return;
		}
		string text = string.Empty;
		string text2 = string.Empty;
		if (ConstData.versionType == "0")
		{
			text = "确认是否删除(Y/N)？";
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
		DataGridViewRow val = ((DataGridView)uiDataGridView1).SelectedRows[0];
		((DataGridView)uiDataGridView1).Rows.Remove(val);
		string guid = val.Cells[0].Value.ToString();
		GridData item = GetData(guid);
		lstData.Remove(item);
		for (int i = 0; i < lstData.Count; i++)
		{
			lstData[i].ID = (i + 1).ToString();
		}
		JsonHelper.WriteJson(lstData, ConstData.jsonfilename);
		((DataGridView)uiDataGridView1).Rows.Clear();
		foreach (GridData lstDatum in lstData)
		{
			AddDataToGrid(lstDatum);
		}
	}

	private void AddNodeNavMenu()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		((TreeView)uiNavMenu1).Nodes.Clear();
		for (int i = 0; i < row * col; i++)
		{
			((TreeView)uiNavMenu1).Nodes.Add(new TreeNode((i + 1).ToString()));
		}
	}

	private void txtRow_TextChanged(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(((Control)txtRow).Text))
		{
			int val = 0;
			if (Helper.CheckStringIsDigit(((Control)txtRow).Text, out val) && val > 0)
			{
				dic.Clear();
				row = val;
				((Control)uiPanel5).Refresh();
				AddNodeNavMenu();
				CalPoint();
			}
		}
	}

	private void txtCol_TextChanged(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(((Control)txtCol).Text))
		{
			int val = 0;
			if (Helper.CheckStringIsDigit(((Control)txtCol).Text, out val) && val > 0)
			{
				dicPoint.Clear();
				dic.Clear();
				col = val;
				((Control)uiPanel5).Refresh();
				AddNodeNavMenu();
				CalPoint();
			}
		}
	}

	private void CalPoint()
	{
		dicPoint.Clear();
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < col; j++)
			{
				if (dicPoint.ContainsKey(i + "-" + j))
				{
					dicPoint[i + "-" + j] = new Point(ori_offsetX + j * 90, ori_offsetY + i * 90);
				}
				else
				{
					dicPoint.Add(i + "-" + j, new Point(ori_offsetX + j * 90, ori_offsetY + i * 90));
				}
			}
		}
	}

	public void ReDrawEllipse(string num)
	{
		if (string.IsNullOrEmpty(num))
		{
			((Control)txtRow).Text = "2";
			((Control)txtCol).Text = "3";
			((Control)txtName).Text = "demo";
			row = 2;
			col = 3;
			((Control)uiPanel5).Refresh();
			CalPoint();
			return;
		}
		data = GetData(num);
		if (data != null)
		{
			row = int.Parse(data.Size.Split(new char[1] { 'x' })[0]);
			col = int.Parse(data.Size.Split(new char[1] { 'x' })[1]);
			((Control)txtRow).Text = row.ToString();
			((Control)txtCol).Text = col.ToString();
			((Control)txtName).Text = data.Name;
			isbottom = data.IsBottom;
			dic = data.Dic;
			((Control)uiPanel5).Refresh();
			editNum = num;
			cbxIsBottom.Checked = data.IsBottom;
		}
	}

	public void ReDrawString()
	{
		Graphics g = ((Control)uiPanel5).CreateGraphics();
		foreach (KeyValuePair<string, string> item in dic)
		{
			if (!string.IsNullOrEmpty(item.Key))
			{
				int num = int.Parse(item.Key.Split(new char[1] { '-' })[0]);
				int num2 = int.Parse(item.Key.Split(new char[1] { '-' })[1]);
				Helper.DrawString3(g, num2, num, item.Value, ori_offsetX, ori_offsetY);
			}
		}
	}

	public void DelDrawString(string key)
	{
		((Control)uiPanel5).Refresh();
		dic[key] = string.Empty;
		ReDrawString();
	}

	private GridData GetData(string guid)
	{
		GridData result = null;
		foreach (GridData lstDatum in lstData)
		{
			if (lstDatum.Guid == guid)
			{
				result = lstDatum;
				break;
			}
		}
		return result;
	}

	private void uiButton7_Click_2(object sender, EventArgs e)
	{
		isbottom = true;
		ShowControl(EidtMode.Add);
		ReDrawEllipse("");
	}

	private void uiButton8_Click_1(object sender, EventArgs e)
	{
		if (editMode == EidtMode.Add)
		{
			HideControl();
			return;
		}
		HideControl();
		HideGridControl();
	}

	public void ShowControl(EidtMode editMode)
	{
		this.editMode = editMode;
		((Control)uiDataGridView1).Visible = false;
		((Control)uiButton7).Visible = false;
		((Control)btnDelDev).Visible = false;
		((Control)uiButton8).Visible = true;
		((Control)btnSave).Visible = true;
		((Control)uiLabel9).Visible = true;
		((Control)uiLabel10).Visible = true;
		((Control)uiLabel11).Visible = true;
		((Control)txtName).Visible = true;
		((Control)txtRow).Visible = true;
		((Control)txtCol).Visible = true;
		((Control)lblInfo).Visible = true;
		((Control)uiNavMenu1).Visible = true;
		((Control)cbxIsBottom).Visible = true;
		((Control)radN).Visible = true;
		((Control)radS).Visible = true;
		dic = new Dictionary<string, string>();
		if (editMode == EidtMode.Eidt)
		{
			txtName.ReadOnly = true;
			txtCol.ReadOnly = true;
			txtRow.ReadOnly = true;
		}
		else
		{
			txtName.ReadOnly = false;
			txtCol.ReadOnly = false;
			txtRow.ReadOnly = false;
		}
	}

	public void HideControl()
	{
		((Control)uiDataGridView1).Visible = true;
		((Control)uiButton7).Visible = true;
		((Control)btnDelDev).Visible = true;
		((Control)uiButton8).Visible = false;
		((Control)btnSave).Visible = false;
		((Control)uiLabel9).Visible = false;
		((Control)uiLabel10).Visible = false;
		((Control)uiLabel11).Visible = false;
		((Control)txtName).Visible = false;
		((Control)txtRow).Visible = false;
		((Control)txtCol).Visible = false;
		((Control)uiNavMenu1).Visible = false;
		((Control)lblInfo).Visible = false;
		((Control)cbxIsBottom).Visible = false;
		((Control)radN).Visible = false;
		((Control)radS).Visible = false;
	}

	private void HideGridControl()
	{
		((Control)splitContainer1).Visible = false;
		((Control)c1).Visible = true;
	}

	private void uiDataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((DataGridView)uiDataGridView1).Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex > -1)
			{
				string guid = ((DataGridView)uiDataGridView1).Rows[e.RowIndex].Cells[0].Value.ToString();
				HideGridControl();
				c1.ReDraw(lstData, guid);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString());
		}
	}

	private void uiTabControl1_KeyDown(object sender, KeyEventArgs e)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		if ((int)e.KeyCode == 77 && (int)e.Modifiers == 131072)
		{
			AddUpgrateTabPage();
		}
		if ((int)e.KeyCode == 79 && (int)e.Modifiers == 262144)
		{
			AddConfigTabPage();
		}
		if ((int)e.KeyCode == 84 && (int)e.Modifiers == 262144)
		{
			u6.ShowCheck();
		}
	}

	private void AddConfigTabPage()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		u4 = new UserControl4(this);
		string text = string.Empty;
		if (ConstData.versionType == "0")
		{
			text = "配置";
			u4.Translate(0);
		}
		else if (ConstData.versionType == "1")
		{
			text = "Config";
			u4.Translate(1);
		}
		TabPage val = null;
		foreach (TabPage tabPage in ((TabControl)uiTabControl1).TabPages)
		{
			TabPage val2 = tabPage;
			if (((Control)val2).Text == text)
			{
				val = val2;
				break;
			}
		}
		if (val != null)
		{
			((TabControl)uiTabControl1).TabPages.Remove(val);
			return;
		}
		TabPage val3 = new TabPage();
		((Control)val3).Text = text;
		((TabControl)uiTabControl1).TabPages.Add(val3);
		((TabControl)uiTabControl1).SelectedIndex = ((TabControl)uiTabControl1).TabPages.Count - 1;
		((Control)u4).Dock = (DockStyle)5;
		((Control)val3).Controls.Add((Control)(object)u4);
	}

	private void AddUpgrateTabPage()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		string text = string.Empty;
		u3 = new UserControl3(this);
		if (ConstData.versionType == "0")
		{
			text = "升级";
			u3.Translate(0);
		}
		else if (ConstData.versionType == "1")
		{
			text = "Upgrade";
			u3.Translate(1);
		}
		TabPage val = null;
		foreach (TabPage tabPage in ((TabControl)uiTabControl1).TabPages)
		{
			TabPage val2 = tabPage;
			if (((Control)val2).Text == text)
			{
				val = val2;
				break;
			}
		}
		if (val != null)
		{
			((TabControl)uiTabControl1).TabPages.Remove(val);
			return;
		}
		TabPage val3 = new TabPage();
		((Control)val3).Text = text;
		((TabControl)uiTabControl1).TabPages.Add(val3);
		((TabControl)uiTabControl1).SelectedIndex = ((TabControl)uiTabControl1).TabPages.Count - 1;
		((Control)u3).Dock = (DockStyle)5;
		((Control)val3).Controls.Add((Control)(object)u3);
	}

	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		string text = string.Empty;
		string text2 = string.Empty;
		if (ConstData.versionType == "0")
		{
			text = "确认是否退出(Y/N)";
			text2 = "系统提示";
		}
		else if (ConstData.versionType == "1")
		{
			text = "Are you sure to execute(Y/N)";
			text2 = "System Prompt";
		}
		if ((int)MessageBox.Show(text, text2, (MessageBoxButtons)4, (MessageBoxIcon)64) == 6)
		{
			Process.GetCurrentProcess().Kill();
		}
		else
		{
			((CancelEventArgs)(object)e).Cancel = true;
		}
	}

	private void QueryFeq()
	{
		byte[] buf = new byte[26]
		{
			240, 165, 90, 15, 1, 0, 0, 0, 1, 0,
			8, 0, 0, 0, 204, 204, 10, 0, 0, 0,
			0, 0, 0, 0, 204, 204
		};
		Send(buf);
	}

	private void cbxIsBottom_CheckedChanged(object sender, EventArgs e)
	{
		Graphics g = ((Control)uiPanel5).CreateGraphics();
		((Control)uiPanel5).Refresh();
		if (!cbxIsBottom.Checked)
		{
			isbottom = false;
			Helper.DrawEllipse(g, row, col, 100, 40, isbottom: false);
		}
		else
		{
			isbottom = true;
			Helper.DrawEllipse(g, row, col, 100, 40, isbottom: true);
		}
		ReDrawString();
	}

	private void uiTabControl1_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = ((TabControl)uiTabControl1).SelectedIndex;
		TabPage val = ((TabControl)uiTabControl1).TabPages[selectedIndex];
		if (selectedIndex == 1)
		{
			((Form)this).Size = new Size(1299, 897);
			if (((Control)c2).Visible)
			{
				((Form)this).Size = new Size(970, 797);
			}
		}
		else
		{
			((Form)this).Size = new Size(1299, 897);
		}
		if (((Control)val).Text == "配置" || ((Control)val).Text == "Config")
		{
			((Form)this).Size = new Size(970, 797);
		}
		if ((((Control)val).Text == "Upgrade" || ((Control)val).Text == "升级") && !u3.canShow)
		{
			((Form)this).Size = new Size(900, 797);
		}
		if (((Control)val).Text == "帮助" || ((Control)val).Text == "Helper")
		{
			((Form)this).Size = new Size(1020, 897);
		}
	}

	private void english_Click(object sender, EventArgs e)
	{
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Expected O, but got Unknown
		((Control)tabPage1).Text = "Device";
		((Control)tabPage2).Text = "Joint-screen";
		((Control)tabPage3).Text = "Helper";
		((DataGridView)uiDataGridView1).Columns[1].HeaderText = "Number";
		((DataGridView)uiDataGridView1).Columns[2].HeaderText = "Name";
		((DataGridView)uiDataGridView1).Columns[3].HeaderText = "Size";
		((DataGridViewColumn)btn2).HeaderText = "Operate";
		((DataGridViewBand)btn2).DefaultCellStyle.NullValue = "Detail";
		((Control)uiButton8).Text = "Cancel";
		((Control)uiButton7).Text = "Add";
		((Control)btnSave).Text = "Save";
		((Control)btnDelDev).Text = "Delete";
		((Control)uiLabel11).Text = "Columns:";
		((Control)uiLabel10).Text = "Rows:";
		((Control)uiLabel9).Text = "Name:";
		((Control)cbxIsBottom).Text = "Height";
		((Control)lblInfo).Text = "Please set the serial number position according to the fiber connection sequence,the first device connected to the fiber is serial number 1(left click to set,right click to clear).";
		((Control)radN).Text = "N Mode";
		((Control)radS).Text = "S Mode";
		u5.Translate(1);
		c1.Translate(1);
		c2.Translate(1);
		u6.Translate(1);
		ConstData.versionType = "1";
		AppConfig.WriteConfig("versionType", ConstData.versionType);
		foreach (TabPage tabPage in ((TabControl)uiTabControl1).TabPages)
		{
			TabPage val = tabPage;
			if (((Control)val).Text == "升级")
			{
				((Control)val).Text = "Upgrade";
				break;
			}
		}
		if (u3 != null)
		{
			u3.Translate(1);
		}
		foreach (TabPage tabPage2 in ((TabControl)uiTabControl1).TabPages)
		{
			TabPage val = tabPage2;
			if (((Control)val).Text == "配置")
			{
				((Control)val).Text = "Config";
				break;
			}
		}
		if (u4 != null)
		{
			u4.Translate(1);
		}
		string text = Path.Combine(Application.StartupPath, "doc") + "\\helper.png";
		pictureBox1.Image = (Image)new Bitmap(text);
	}

	private void china_Click(object sender, EventArgs e)
	{
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Expected O, but got Unknown
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Expected O, but got Unknown
		((Control)tabPage1).Text = "设备";
		((Control)tabPage2).Text = "联屏";
		((Control)tabPage3).Text = "帮助";
		((DataGridView)uiDataGridView1).Columns[1].HeaderText = "行号";
		((DataGridView)uiDataGridView1).Columns[2].HeaderText = "名称";
		((DataGridView)uiDataGridView1).Columns[3].HeaderText = "大小";
		((DataGridViewColumn)btn2).HeaderText = "操作";
		((DataGridViewBand)btn2).DefaultCellStyle.NullValue = "详情";
		((Control)uiButton7).Text = "添加";
		((Control)uiButton8).Text = "取消";
		((Control)btnSave).Text = "保存";
		((Control)btnDelDev).Text = "删除";
		((Control)uiLabel11).Text = "列数:";
		((Control)uiLabel10).Text = "行数:";
		((Control)uiLabel9).Text = "名称:";
		((Control)cbxIsBottom).Text = "高度";
		((Control)lblInfo).Text = "请根据光纤连接顺序设置设备编号，第一个连接光纤的设备编号为01(左键设置，右键清除)";
		((Control)radN).Text = "N 型";
		((Control)radS).Text = "S 型";
		u5.Translate(0);
		u6.Translate(0);
		c1.Translate(0);
		c2.Translate(0);
		if (u3 != null)
		{
			u3.Translate(0);
		}
		ConstData.versionType = "0";
		AppConfig.WriteConfig("versionType", ConstData.versionType);
		foreach (TabPage tabPage in ((TabControl)uiTabControl1).TabPages)
		{
			TabPage val = tabPage;
			if (((Control)val).Text == "Upgrade")
			{
				((Control)val).Text = "升级";
				break;
			}
		}
		foreach (TabPage tabPage2 in ((TabControl)uiTabControl1).TabPages)
		{
			TabPage val = tabPage2;
			if (((Control)val).Text == "Config")
			{
				((Control)val).Text = "配置";
				break;
			}
		}
		if (u4 != null)
		{
			u4.Translate(0);
		}
		string text = Path.Combine(Application.StartupPath, "doc") + "\\帮助.png";
		pictureBox1.Image = (Image)new Bitmap(text);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Expected O, but got Unknown
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Expected O, but got Unknown
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Expected O, but got Unknown
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0779: Unknown result type (might be due to invalid IL or missing references)
		//IL_0783: Expected O, but got Unknown
		//IL_07b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0876: Unknown result type (might be due to invalid IL or missing references)
		//IL_0880: Expected O, but got Unknown
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0961: Unknown result type (might be due to invalid IL or missing references)
		//IL_096b: Expected O, but got Unknown
		//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a59: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a63: Expected O, but got Unknown
		//IL_0aab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b4e: Expected O, but got Unknown
		//IL_0b6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c59: Expected O, but got Unknown
		//IL_0c89: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d77: Expected O, but got Unknown
		//IL_0e4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e55: Expected O, but got Unknown
		//IL_0e85: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f84: Expected O, but got Unknown
		//IL_0fa0: Unknown result type (might be due to invalid IL or missing references)
		//IL_105e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1068: Expected O, but got Unknown
		//IL_1084: Unknown result type (might be due to invalid IL or missing references)
		//IL_113e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1148: Expected O, but got Unknown
		//IL_11f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1200: Expected O, but got Unknown
		//IL_121d: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1303: Expected O, but got Unknown
		//IL_1326: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ff: Expected O, but got Unknown
		//IL_150b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1515: Expected O, but got Unknown
		//IL_1544: Unknown result type (might be due to invalid IL or missing references)
		//IL_15f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_15fa: Expected O, but got Unknown
		//IL_1608: Unknown result type (might be due to invalid IL or missing references)
		//IL_1612: Expected O, but got Unknown
		//IL_16ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_16c4: Expected O, but got Unknown
		//IL_1745: Unknown result type (might be due to invalid IL or missing references)
		//IL_174f: Expected O, but got Unknown
		//IL_17d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_17df: Expected O, but got Unknown
		//IL_185e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1868: Expected O, but got Unknown
		//IL_1980: Unknown result type (might be due to invalid IL or missing references)
		//IL_198a: Expected O, but got Unknown
		//IL_19cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_19d9: Expected O, but got Unknown
		//IL_1b8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b97: Expected O, but got Unknown
		//IL_1dc8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dd2: Expected O, but got Unknown
		//IL_1f66: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f70: Expected O, but got Unknown
		//IL_1f7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f88: Expected O, but got Unknown
		//IL_1f8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_2036: Unknown result type (might be due to invalid IL or missing references)
		//IL_2040: Expected O, but got Unknown
		components = new Container();
		DataGridViewCellStyle val = new DataGridViewCellStyle();
		DataGridViewCellStyle val2 = new DataGridViewCellStyle();
		DataGridViewCellStyle val3 = new DataGridViewCellStyle();
		DataGridViewCellStyle val4 = new DataGridViewCellStyle();
		DataGridViewCellStyle val5 = new DataGridViewCellStyle();
		TreeNode val6 = new TreeNode("1");
		TreeNode val7 = new TreeNode("2");
		TreeNode val8 = new TreeNode("3");
		TreeNode val9 = new TreeNode("4");
		TreeNode val10 = new TreeNode("5");
		TreeNode val11 = new TreeNode("6");
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Form1));
		tabPage1 = new TabPage();
		uiTabControl1 = new UITabControl();
		tabPage2 = new TabPage();
		splitContainer1 = new SplitContainer();
		uiPanel6 = new UIPanel();
		radS = new UIRadioButton();
		radN = new UIRadioButton();
		cbxIsBottom = new UICheckBox();
		btnDelDev = new UIButton();
		txtCol = new UITextBox();
		uiLabel11 = new UILabel();
		txtRow = new UITextBox();
		uiButton7 = new UIButton();
		uiButton8 = new UIButton();
		uiLabel10 = new UILabel();
		btnSave = new UIButton();
		txtName = new UITextBox();
		uiLabel9 = new UILabel();
		uiPanel5 = new UIPanel();
		uiDataGridView1 = new UIDataGridView();
		uiNavMenu1 = new UINavMenu();
		lblInfo = new UILabel();
		tabPage3 = new TabPage();
		panel1 = new Panel();
		pictureBox1 = new PictureBox();
		uiStyleManager1 = new UIStyleManager(components);
		uiContextMenuStrip1 = new UIContextMenuStrip();
		china = new ToolStripMenuItem();
		english = new ToolStripMenuItem();
		((Control)uiTabControl1).SuspendLayout();
		((Control)tabPage2).SuspendLayout();
		((ISupportInitialize)splitContainer1).BeginInit();
		((Control)splitContainer1.Panel1).SuspendLayout();
		((Control)splitContainer1.Panel2).SuspendLayout();
		((Control)splitContainer1).SuspendLayout();
		((Control)uiPanel6).SuspendLayout();
		((Control)uiPanel5).SuspendLayout();
		((ISupportInitialize)uiDataGridView1).BeginInit();
		((Control)tabPage3).SuspendLayout();
		((Control)panel1).SuspendLayout();
		((ISupportInitialize)pictureBox1).BeginInit();
		((Control)uiContextMenuStrip1).SuspendLayout();
		((Control)this).SuspendLayout();
		((ScrollableControl)tabPage1).AutoScroll = true;
		((Control)tabPage1).BackColor = Color.FromArgb(24, 24, 24);
		((Panel)tabPage1).BorderStyle = (BorderStyle)1;
		tabPage1.Location = new Point(0, 40);
		((Control)tabPage1).Margin = new Padding(2, 1, 2, 1);
		((Control)tabPage1).Name = "tabPage1";
		((Control)tabPage1).Size = new Size(995, 601);
		tabPage1.TabIndex = 0;
		((Control)tabPage1).Text = "设备";
		((Control)uiTabControl1).Controls.Add((Control)(object)tabPage1);
		((Control)uiTabControl1).Controls.Add((Control)(object)tabPage2);
		((Control)uiTabControl1).Controls.Add((Control)(object)tabPage3);
		((Control)uiTabControl1).Dock = (DockStyle)5;
		((TabControl)uiTabControl1).DrawMode = (TabDrawMode)1;
		uiTabControl1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiTabControl1).Font = new Font("微软雅黑", 9.75f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((TabControl)uiTabControl1).ItemSize = new Size(150, 40);
		((Control)uiTabControl1).Location = new Point(2, 35);
		uiTabControl1.MainPage = "";
		((Control)uiTabControl1).Margin = new Padding(2, 1, 2, 1);
		((Control)uiTabControl1).Name = "uiTabControl1";
		((TabControl)uiTabControl1).SelectedIndex = 0;
		((Control)uiTabControl1).Size = new Size(995, 641);
		((TabControl)uiTabControl1).SizeMode = (TabSizeMode)2;
		uiTabControl1.Style = UIStyle.Custom;
		((Control)uiTabControl1).TabIndex = 0;
		((TabControl)uiTabControl1).SelectedIndexChanged += uiTabControl1_SelectedIndexChanged;
		((Control)uiTabControl1).KeyDown += new KeyEventHandler(uiTabControl1_KeyDown);
		((Control)tabPage2).BackColor = Color.FromArgb(24, 24, 24);
		((Panel)tabPage2).BorderStyle = (BorderStyle)1;
		((Control)tabPage2).Controls.Add((Control)(object)splitContainer1);
		tabPage2.Location = new Point(0, 40);
		((Control)tabPage2).Margin = new Padding(2, 1, 2, 1);
		((Control)tabPage2).Name = "tabPage2";
		((Control)tabPage2).Size = new Size(995, 601);
		tabPage2.TabIndex = 1;
		((Control)tabPage2).Text = "联屏";
		splitContainer1.Dock = (DockStyle)5;
		splitContainer1.FixedPanel = (FixedPanel)1;
		((Control)splitContainer1).Location = new Point(0, 0);
		((Control)splitContainer1).Margin = new Padding(2);
		((Control)splitContainer1).Name = "splitContainer1";
		splitContainer1.Orientation = (Orientation)0;
		((Control)splitContainer1.Panel1).Controls.Add((Control)(object)uiPanel6);
		((Control)splitContainer1.Panel2).Controls.Add((Control)(object)uiPanel5);
		((Control)splitContainer1).Size = new Size(993, 599);
		splitContainer1.SplitterDistance = 52;
		splitContainer1.SplitterWidth = 3;
		((Control)splitContainer1).TabIndex = 6;
		((Control)uiPanel6).Controls.Add((Control)(object)radS);
		((Control)uiPanel6).Controls.Add((Control)(object)radN);
		((Control)uiPanel6).Controls.Add((Control)(object)cbxIsBottom);
		((Control)uiPanel6).Controls.Add((Control)(object)btnDelDev);
		((Control)uiPanel6).Controls.Add((Control)(object)txtCol);
		((Control)uiPanel6).Controls.Add((Control)(object)uiLabel11);
		((Control)uiPanel6).Controls.Add((Control)(object)txtRow);
		((Control)uiPanel6).Controls.Add((Control)(object)uiButton7);
		((Control)uiPanel6).Controls.Add((Control)(object)uiButton8);
		((Control)uiPanel6).Controls.Add((Control)(object)uiLabel10);
		((Control)uiPanel6).Controls.Add((Control)(object)btnSave);
		((Control)uiPanel6).Controls.Add((Control)(object)txtName);
		((Control)uiPanel6).Controls.Add((Control)(object)uiLabel9);
		((Control)uiPanel6).Dock = (DockStyle)5;
		uiPanel6.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel6).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel6).ForeColor = Color.Silver;
		((Control)uiPanel6).Location = new Point(0, 0);
		((Control)uiPanel6).Margin = new Padding(3, 5, 3, 5);
		((Control)uiPanel6).MinimumSize = new Size(1, 1);
		((Control)uiPanel6).Name = "uiPanel6";
		uiPanel6.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel6).Size = new Size(993, 52);
		uiPanel6.Style = UIStyle.Custom;
		((Control)uiPanel6).TabIndex = 4;
		((Control)uiPanel6).Text = null;
		uiPanel6.TextAlignment = (ContentAlignment)32;
		((Control)radS).Cursor = Cursors.Hand;
		((Control)radS).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)radS).Location = new Point(786, 13);
		((Control)radS).MinimumSize = new Size(1, 1);
		((Control)radS).Name = "radS";
		((Control)radS).Padding = new Padding(22, 0, 0, 0);
		((Control)radS).Size = new Size(79, 29);
		radS.Style = UIStyle.Custom;
		((Control)radS).TabIndex = 24;
		((Control)radS).Text = "S型";
		((Control)radS).Visible = false;
		radS.CheckedChanged += radS_CheckedChanged;
		((Control)radN).Cursor = Cursors.Hand;
		((Control)radN).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)radN).Location = new Point(693, 13);
		((Control)radN).MinimumSize = new Size(1, 1);
		((Control)radN).Name = "radN";
		((Control)radN).Padding = new Padding(22, 0, 0, 0);
		((Control)radN).Size = new Size(93, 29);
		radN.Style = UIStyle.Custom;
		((Control)radN).TabIndex = 24;
		((Control)radN).Text = "N型";
		((Control)radN).Visible = false;
		radN.CheckedChanged += radN_CheckedChanged;
		cbxIsBottom.Checked = true;
		((Control)cbxIsBottom).Cursor = Cursors.Hand;
		((Control)cbxIsBottom).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)cbxIsBottom).Location = new Point(609, 13);
		((Control)cbxIsBottom).MinimumSize = new Size(1, 1);
		((Control)cbxIsBottom).Name = "cbxIsBottom";
		((Control)cbxIsBottom).Padding = new Padding(22, 0, 0, 0);
		((Control)cbxIsBottom).Size = new Size(78, 29);
		cbxIsBottom.Style = UIStyle.Custom;
		((Control)cbxIsBottom).TabIndex = 21;
		((Control)cbxIsBottom).Text = "高度";
		((Control)cbxIsBottom).Visible = false;
		cbxIsBottom.CheckedChanged += cbxIsBottom_CheckedChanged;
		((Control)btnDelDev).Cursor = Cursors.Hand;
		((Control)btnDelDev).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)btnDelDev).Location = new Point(84, 10);
		((Control)btnDelDev).Margin = new Padding(2);
		((Control)btnDelDev).MinimumSize = new Size(1, 1);
		((Control)btnDelDev).Name = "btnDelDev";
		btnDelDev.Radius = 26;
		((Control)btnDelDev).Size = new Size(74, 29);
		btnDelDev.Style = UIStyle.Custom;
		((Control)btnDelDev).TabIndex = 20;
		((Control)btnDelDev).Text = "删除";
		((Control)btnDelDev).Click += btnDelDev_Click;
		((Control)txtCol).Cursor = Cursors.IBeam;
		txtCol.DoubleValue = 3.0;
		txtCol.FillColor = Color.White;
		((Control)txtCol).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		txtCol.IntValue = 3;
		((Control)txtCol).Location = new Point(537, 13);
		((Control)txtCol).Margin = new Padding(4, 5, 4, 5);
		txtCol.Maximum = 2147483647.0;
		txtCol.Minimum = -2147483648.0;
		((Control)txtCol).MinimumSize = new Size(1, 1);
		((Control)txtCol).Name = "txtCol";
		((Control)txtCol).Size = new Size(51, 29);
		txtCol.Style = UIStyle.Custom;
		((Control)txtCol).TabIndex = 18;
		((Control)txtCol).Text = "3";
		txtCol.TextAlignment = (ContentAlignment)16;
		((Control)txtCol).Visible = false;
		txtCol.TextChanged += txtCol_TextChanged;
		((Control)uiLabel11).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel11).Location = new Point(477, 16);
		((Control)uiLabel11).Name = "uiLabel11";
		((Control)uiLabel11).Size = new Size(61, 23);
		uiLabel11.Style = UIStyle.Custom;
		((Control)uiLabel11).TabIndex = 17;
		((Control)uiLabel11).Text = "列数：";
		((Label)uiLabel11).TextAlign = (ContentAlignment)32;
		((Control)uiLabel11).Visible = false;
		((Control)txtRow).Cursor = Cursors.IBeam;
		txtRow.DoubleValue = 2.0;
		txtRow.FillColor = Color.White;
		((Control)txtRow).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		txtRow.IntValue = 2;
		((Control)txtRow).Location = new Point(419, 13);
		((Control)txtRow).Margin = new Padding(4, 5, 4, 5);
		txtRow.Maximum = 2147483647.0;
		txtRow.Minimum = -2147483648.0;
		((Control)txtRow).MinimumSize = new Size(1, 1);
		((Control)txtRow).Name = "txtRow";
		((Control)txtRow).Size = new Size(51, 29);
		txtRow.Style = UIStyle.Custom;
		((Control)txtRow).TabIndex = 16;
		((Control)txtRow).Text = "2";
		txtRow.TextAlignment = (ContentAlignment)16;
		((Control)txtRow).Visible = false;
		txtRow.TextChanged += txtRow_TextChanged;
		((Control)uiButton7).Cursor = Cursors.Hand;
		((Control)uiButton7).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiButton7).Location = new Point(6, 10);
		((Control)uiButton7).Margin = new Padding(2);
		((Control)uiButton7).MinimumSize = new Size(1, 1);
		((Control)uiButton7).Name = "uiButton7";
		uiButton7.Radius = 26;
		((Control)uiButton7).Size = new Size(74, 29);
		uiButton7.Style = UIStyle.Custom;
		((Control)uiButton7).TabIndex = 11;
		((Control)uiButton7).Text = "添加 ";
		((Control)uiButton7).Click += uiButton7_Click_2;
		((Control)uiButton8).Cursor = Cursors.Hand;
		((Control)uiButton8).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiButton8).Location = new Point(6, 13);
		((Control)uiButton8).Margin = new Padding(2);
		((Control)uiButton8).MinimumSize = new Size(1, 1);
		((Control)uiButton8).Name = "uiButton8";
		uiButton8.Radius = 26;
		((Control)uiButton8).Size = new Size(74, 29);
		uiButton8.Style = UIStyle.Custom;
		((Control)uiButton8).TabIndex = 11;
		((Control)uiButton8).Text = "取消";
		((Control)uiButton8).Visible = false;
		((Control)uiButton8).Click += uiButton8_Click_1;
		((Control)uiLabel10).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel10).Location = new Point(367, 16);
		((Control)uiLabel10).Name = "uiLabel10";
		((Control)uiLabel10).Size = new Size(45, 23);
		uiLabel10.Style = UIStyle.Custom;
		((Control)uiLabel10).TabIndex = 15;
		((Control)uiLabel10).Text = "行数：";
		((Label)uiLabel10).TextAlign = (ContentAlignment)32;
		((Control)uiLabel10).Visible = false;
		((Control)btnSave).Cursor = Cursors.Hand;
		((Control)btnSave).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)btnSave).Location = new Point(82, 13);
		((Control)btnSave).Margin = new Padding(2);
		((Control)btnSave).MinimumSize = new Size(1, 1);
		((Control)btnSave).Name = "btnSave";
		btnSave.Radius = 26;
		((Control)btnSave).Size = new Size(74, 29);
		btnSave.Style = UIStyle.Custom;
		((Control)btnSave).TabIndex = 12;
		((Control)btnSave).Text = "保存";
		((Control)btnSave).Visible = false;
		((Control)btnSave).Click += btnSave_Click;
		((Control)txtName).Cursor = Cursors.IBeam;
		txtName.FillColor = Color.White;
		((Control)txtName).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)txtName).Location = new Point(204, 13);
		((Control)txtName).Margin = new Padding(4, 5, 4, 5);
		txtName.Maximum = 2147483647.0;
		txtName.Minimum = -2147483648.0;
		((Control)txtName).MinimumSize = new Size(1, 1);
		((Control)txtName).Name = "txtName";
		((Control)txtName).Size = new Size(150, 29);
		txtName.Style = UIStyle.Custom;
		((Control)txtName).TabIndex = 14;
		((Control)txtName).Text = "demo";
		txtName.TextAlignment = (ContentAlignment)16;
		((Control)txtName).Visible = false;
		((Control)uiLabel9).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)uiLabel9).Location = new Point(161, 16);
		((Control)uiLabel9).Name = "uiLabel9";
		((Control)uiLabel9).Size = new Size(45, 23);
		uiLabel9.Style = UIStyle.Custom;
		((Control)uiLabel9).TabIndex = 13;
		((Control)uiLabel9).Text = "名称：";
		((Label)uiLabel9).TextAlign = (ContentAlignment)16;
		((Control)uiLabel9).Visible = false;
		((ScrollableControl)uiPanel5).AutoScroll = true;
		((Control)uiPanel5).Controls.Add((Control)(object)uiDataGridView1);
		((Control)uiPanel5).Controls.Add((Control)(object)uiNavMenu1);
		((Control)uiPanel5).Controls.Add((Control)(object)lblInfo);
		((Control)uiPanel5).Dock = (DockStyle)5;
		uiPanel5.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel5).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel5).ForeColor = Color.Silver;
		((Control)uiPanel5).Location = new Point(0, 0);
		((Control)uiPanel5).Margin = new Padding(3, 5, 3, 5);
		((Control)uiPanel5).MinimumSize = new Size(1, 1);
		((Control)uiPanel5).Name = "uiPanel5";
		uiPanel5.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel5).Size = new Size(993, 544);
		uiPanel5.Style = UIStyle.Custom;
		((Control)uiPanel5).TabIndex = 6;
		((Control)uiPanel5).Text = null;
		uiPanel5.TextAlignment = (ContentAlignment)32;
		((Control)uiPanel5).Paint += new PaintEventHandler(uiPanel5_Paint);
		((Control)uiPanel5).MouseClick += new MouseEventHandler(uiPanel5_MouseClick);
		((DataGridView)uiDataGridView1).AllowUserToAddRows = false;
		((DataGridView)uiDataGridView1).AllowUserToResizeRows = false;
		val.BackColor = Color.FromArgb(235, 243, 255);
		((DataGridView)uiDataGridView1).AlternatingRowsDefaultCellStyle = val;
		((DataGridView)uiDataGridView1).AutoSizeColumnsMode = (DataGridViewAutoSizeColumnsMode)16;
		((DataGridView)uiDataGridView1).BackgroundColor = Color.White;
		((DataGridView)uiDataGridView1).BorderStyle = (BorderStyle)0;
		((DataGridView)uiDataGridView1).ColumnHeadersBorderStyle = (DataGridViewHeaderBorderStyle)1;
		val2.Alignment = (DataGridViewContentAlignment)32;
		val2.BackColor = Color.FromArgb(80, 160, 255);
		val2.Font = new Font("微软雅黑", 12f);
		val2.ForeColor = Color.White;
		val2.SelectionBackColor = Color.FromArgb(80, 160, 255);
		val2.SelectionForeColor = SystemColors.HighlightText;
		val2.WrapMode = (DataGridViewTriState)1;
		((DataGridView)uiDataGridView1).ColumnHeadersDefaultCellStyle = val2;
		((DataGridView)uiDataGridView1).ColumnHeadersHeight = 32;
		((DataGridView)uiDataGridView1).ColumnHeadersHeightSizeMode = (DataGridViewColumnHeadersHeightSizeMode)1;
		val3.Alignment = (DataGridViewContentAlignment)16;
		val3.BackColor = SystemColors.Window;
		val3.Font = new Font("微软雅黑", 12f);
		val3.ForeColor = Color.Silver;
		val3.SelectionBackColor = Color.FromArgb(155, 200, 255);
		val3.SelectionForeColor = Color.FromArgb(48, 48, 48);
		val3.WrapMode = (DataGridViewTriState)2;
		((DataGridView)uiDataGridView1).DefaultCellStyle = val3;
		((Control)uiDataGridView1).Dock = (DockStyle)5;
		((DataGridView)uiDataGridView1).EditMode = (DataGridViewEditMode)3;
		((DataGridView)uiDataGridView1).EnableHeadersVisualStyles = false;
		((Control)uiDataGridView1).Font = new Font("微软雅黑", 12f);
		((DataGridView)uiDataGridView1).GridColor = Color.FromArgb(80, 160, 255);
		((Control)uiDataGridView1).Location = new Point(77, 0);
		((DataGridView)uiDataGridView1).MultiSelect = false;
		((Control)uiDataGridView1).Name = "uiDataGridView1";
		val4.Alignment = (DataGridViewContentAlignment)16;
		val4.BackColor = Color.FromArgb(235, 243, 255);
		val4.Font = new Font("微软雅黑", 12f);
		val4.ForeColor = Color.FromArgb(48, 48, 48);
		val4.SelectionBackColor = Color.FromArgb(80, 160, 255);
		val4.SelectionForeColor = Color.White;
		val4.WrapMode = (DataGridViewTriState)1;
		((DataGridView)uiDataGridView1).RowHeadersDefaultCellStyle = val4;
		((DataGridView)uiDataGridView1).RowHeadersVisible = false;
		uiDataGridView1.RowHeight = 29;
		val5.BackColor = Color.White;
		val5.ForeColor = Color.Black;
		((DataGridView)uiDataGridView1).RowsDefaultCellStyle = val5;
		((DataGridView)uiDataGridView1).RowTemplate.Height = 29;
		uiDataGridView1.SelectedIndex = -1;
		((DataGridView)uiDataGridView1).SelectionMode = (DataGridViewSelectionMode)1;
		uiDataGridView1.ShowGridLine = true;
		uiDataGridView1.ShowRect = false;
		((Control)uiDataGridView1).Size = new Size(916, 544);
		uiDataGridView1.Style = UIStyle.Custom;
		((Control)uiDataGridView1).TabIndex = 6;
		((DataGridView)uiDataGridView1).CellContentClick += new DataGridViewCellEventHandler(uiDataGridView1_CellClick);
		((TreeView)uiNavMenu1).BorderStyle = (BorderStyle)0;
		((Control)uiNavMenu1).Dock = (DockStyle)3;
		((TreeView)uiNavMenu1).DrawMode = (TreeViewDrawMode)2;
		uiNavMenu1.ExpandSelectFirst = true;
		((Control)uiNavMenu1).Font = new Font("微软雅黑", 12f);
		((TreeView)uiNavMenu1).FullRowSelect = true;
		((TreeView)uiNavMenu1).ItemHeight = 50;
		((Control)uiNavMenu1).Location = new Point(0, 0);
		uiNavMenu1.MenuStyle = UIMenuStyle.Custom;
		((Control)uiNavMenu1).Name = "uiNavMenu1";
		val6.Name = "节点0";
		val6.Text = "1";
		val7.Name = "节点1";
		val7.Text = "2";
		val8.Name = "节点2";
		val8.Text = "3";
		val9.Name = "节点3";
		val9.Text = "4";
		val10.Name = "节点4";
		val10.Text = "5";
		val11.Name = "节点5";
		val11.Text = "6";
		((TreeView)uiNavMenu1).Nodes.AddRange((TreeNode[])(object)new TreeNode[6] { val6, val7, val8, val9, val10, val11 });
		((TreeView)uiNavMenu1).ShowLines = false;
		((Control)uiNavMenu1).Size = new Size(77, 544);
		uiNavMenu1.Style = UIStyle.Custom;
		((Control)uiNavMenu1).TabIndex = 0;
		((Control)uiNavMenu1).Visible = false;
		uiNavMenu1.MenuItemClick += uiNavMenu1_MenuItemClick;
		((Control)lblInfo).BackColor = Color.Transparent;
		((Control)lblInfo).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Control)lblInfo).Location = new Point(105, -1);
		((Control)lblInfo).Name = "lblInfo";
		((Control)lblInfo).Size = new Size(818, 45);
		lblInfo.Style = UIStyle.Custom;
		((Control)lblInfo).TabIndex = 16;
		((Control)lblInfo).Text = "请根据光纤连接顺序设置设备编号，第一个连接光纤的设备编号为01(左键设置，右键清除)";
		((Label)lblInfo).TextAlign = (ContentAlignment)16;
		((Control)lblInfo).Visible = false;
		((Control)tabPage3).BackColor = Color.FromArgb(24, 24, 24);
		((Control)tabPage3).Controls.Add((Control)(object)panel1);
		tabPage3.Location = new Point(0, 40);
		((Control)tabPage3).Name = "tabPage3";
		((Control)tabPage3).Size = new Size(995, 601);
		tabPage3.TabIndex = 2;
		((Control)tabPage3).Text = "帮助";
		((ScrollableControl)panel1).AutoScroll = true;
		((Control)panel1).Controls.Add((Control)(object)pictureBox1);
		((Control)panel1).Dock = (DockStyle)5;
		((Control)panel1).Location = new Point(0, 0);
		((Control)panel1).Name = "panel1";
		((Control)panel1).Size = new Size(995, 601);
		((Control)panel1).TabIndex = 0;
		((Control)pictureBox1).Location = new Point(3, 3);
		((Control)pictureBox1).Name = "pictureBox1";
		((Control)pictureBox1).Size = new Size(992, 595);
		pictureBox1.SizeMode = (PictureBoxSizeMode)2;
		pictureBox1.TabIndex = 0;
		pictureBox1.TabStop = false;
		uiStyleManager1.Style = UIStyle.Black;
		((ToolStrip)uiContextMenuStrip1).BackColor = Color.FromArgb(230, 230, 232);
		((Control)uiContextMenuStrip1).Font = new Font("微软雅黑", 12f);
		((ToolStrip)uiContextMenuStrip1).ImageScalingSize = new Size(20, 20);
		((ToolStrip)uiContextMenuStrip1).Items.AddRange((ToolStripItem[])(object)new ToolStripItem[2]
		{
			(ToolStripItem)china,
			(ToolStripItem)english
		});
		((Control)uiContextMenuStrip1).Name = "uiContextMenuStrip1";
		((Control)uiContextMenuStrip1).Size = new Size(152, 68);
		uiContextMenuStrip1.Style = UIStyle.Custom;
		((ToolStripItem)china).Name = "china";
		((ToolStripItem)china).Size = new Size(151, 32);
		((ToolStripItem)china).Text = "中文";
		((ToolStripItem)china).Click += china_Click;
		((ToolStripItem)english).Name = "english";
		((ToolStripItem)english).Size = new Size(151, 32);
		((ToolStripItem)english).Text = "English";
		((ToolStripItem)english).Click += english_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(10f, 23f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((ScrollableControl)this).AutoScroll = true;
		((Form)this).ClientSize = new Size(999, 678);
		((Control)this).Controls.Add((Control)(object)uiTabControl1);
		base.ExtendBox = true;
		base.ExtendMenu = uiContextMenuStrip1;
		((Control)this).Font = new Font("微软雅黑", 10.2f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Form)this).Margin = new Padding(2, 1, 2, 1);
		((Control)this).MaximumSize = new Size(1600, 869);
		((Control)this).Name = "Form1";
		((Control)this).Padding = new Padding(2, 35, 2, 2);
		base.RectColor = Color.FromArgb(130, 130, 130);
		base.ShowDragStretch = true;
		((Form)this).ShowIcon = true;
		base.ShowRadius = false;
		base.Style = UIStyle.Custom;
		((Control)this).Text = "MetaStudio_v1.0.5.3";
		base.TitleColor = Color.FromArgb(130, 130, 130);
		((Form)this).FormClosing += new FormClosingEventHandler(Form1_FormClosing);
		((Control)uiTabControl1).ResumeLayout(false);
		((Control)tabPage2).ResumeLayout(false);
		((Control)splitContainer1.Panel1).ResumeLayout(false);
		((Control)splitContainer1.Panel2).ResumeLayout(false);
		((ISupportInitialize)splitContainer1).EndInit();
		((Control)splitContainer1).ResumeLayout(false);
		((Control)uiPanel6).ResumeLayout(false);
		((Control)uiPanel5).ResumeLayout(false);
		((ISupportInitialize)uiDataGridView1).EndInit();
		((Control)tabPage3).ResumeLayout(false);
		((Control)panel1).ResumeLayout(false);
		((Control)panel1).PerformLayout();
		((ISupportInitialize)pictureBox1).EndInit();
		((Control)uiContextMenuStrip1).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
	}
}
