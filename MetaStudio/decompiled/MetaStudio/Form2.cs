using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Sunny.UI;

namespace MetaStudio;

public class Form2 : UIForm
{
	private Queue<double> dataQueue = new Queue<double>(100);

	private int curValue = 0;

	private int num = 1;

	private UserControl3 u3;

	private int max = -1;

	private int min = -1;

	private int diffVal = -1;

	private int frontVal = -1;

	private bool canSta = false;

	private bool canStop = false;

	private IContainer components = null;

	private UIPanel uiPanel1;

	private UIPanel uiPanel2;

	private UIButton btnReadReg;

	private Chart chart1;

	private Timer timer1;

	private UIButton uiButton1;

	private TextBox txtChaz;

	private Label label3;

	private TextBox txtMin;

	private Label label2;

	private TextBox txtMax;

	private Label label1;

	private UIButton btnClear;

	private UIButton btnStaic;

	private UIButton btnInit;

	private TextBox txtYmin;

	private Label label5;

	private TextBox txtYmax;

	private Label label4;

	public Form2(UserControl3 u3)
	{
		InitializeComponent();
		this.u3 = u3;
	}

	private void InitChart()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		((Collection<ChartArea>)(object)chart1.ChartAreas).Clear();
		ChartArea item = new ChartArea("C1");
		((Collection<ChartArea>)(object)chart1.ChartAreas).Add(item);
		((Collection<Series>)(object)chart1.Series).Clear();
		Series val = new Series("S1");
		val.ChartArea = "C1";
		((Collection<Series>)(object)chart1.Series).Add(val);
		((Collection<ChartArea>)(object)chart1.ChartAreas)[0].AxisY.Minimum = int.Parse(((Control)txtYmin).Text);
		((Collection<ChartArea>)(object)chart1.ChartAreas)[0].AxisY.Maximum = int.Parse(((Control)txtYmax).Text);
		((Collection<ChartArea>)(object)chart1.ChartAreas)[0].AxisX.Interval = 5.0;
		((Collection<ChartArea>)(object)chart1.ChartAreas)[0].AxisX.MajorGrid.LineColor = Color.Silver;
		((Collection<ChartArea>)(object)chart1.ChartAreas)[0].AxisY.MajorGrid.LineColor = Color.Silver;
		((Collection<Title>)(object)chart1.Titles).Clear();
		chart1.Titles.Add("S01");
		((Collection<Title>)(object)chart1.Titles)[0].Text = "寄存器波形图";
		((Collection<Title>)(object)chart1.Titles)[0].ForeColor = Color.RoyalBlue;
		((Collection<Title>)(object)chart1.Titles)[0].Font = new Font("Microsoft Sans Serif", 12f);
		((DataPointCustomProperties)((Collection<Series>)(object)chart1.Series)[0]).Color = Color.Red;
		((Collection<Title>)(object)chart1.Titles)[0].Text = string.Format("寄存器 {0} 显示", "波形图");
		((Collection<Series>)(object)chart1.Series)[0].ChartType = (SeriesChartType)3;
		((Collection<Title>)(object)chart1.Titles)[0].Text = string.Format("寄存器 {0} 显示", "波形图");
		((Collection<Series>)(object)chart1.Series)[0].ChartType = (SeriesChartType)4;
		((Collection<DataPoint>)(object)((Collection<Series>)(object)chart1.Series)[0].Points).Clear();
	}

	private void btnStart_Click(object sender, EventArgs e)
	{
		timer1.Start();
	}

	private void btnStop_Click(object sender, EventArgs e)
	{
		timer1.Stop();
		int statorID = u3.GetStatorID();
		SPHelper.SendTOStator(statorID, 2, 167, 0);
		canStop = true;
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		u3.btnReadReg_Click(null, null);
		Thread.Sleep(10);
		UpdatChar();
	}

	public void UpdatChar()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		((Control)this).BeginInvoke((Delegate)(MethodInvoker)delegate
		{
			try
			{
				((Collection<DataPoint>)(object)((Collection<Series>)(object)chart1.Series)[0].Points).Clear();
				for (int i = 0; i < dataQueue.Count; i++)
				{
					((Collection<Series>)(object)chart1.Series)[0].Points.AddXY((double)(i + 1), dataQueue.ElementAt(i));
				}
			}
			catch (Exception ex)
			{
				LogerHelper.Error(ex.Message);
			}
		});
	}

	public void UpdateQueueValue(int value)
	{
		Console.WriteLine(value.ToString("X"));
		if (canSta)
		{
			StaticData(value);
		}
		if (dataQueue.Count > 100)
		{
			for (int i = 0; i < num; i++)
			{
				dataQueue.Dequeue();
			}
		}
		dataQueue.Enqueue(value);
	}

	private void StaticData(int value)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		if (max == -1)
		{
			max = value;
		}
		if (min == -1)
		{
			min = value;
		}
		if (frontVal == -1)
		{
			frontVal = value;
		}
		if (value > max)
		{
			max = value;
		}
		if (value < min)
		{
			min = value;
		}
		diffVal = value - frontVal;
		frontVal = value;
		((Control)this).BeginInvoke((Delegate)(MethodInvoker)delegate
		{
			((Control)txtMax).Text = max.ToString();
			((Control)txtMin).Text = min.ToString();
			((Control)txtChaz).Text = diffVal.ToString();
		});
	}

	private void btnStaic_Click(object sender, EventArgs e)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		if (!canSta)
		{
			canSta = true;
		}
		else
		{
			canSta = false;
		}
		max = -1;
		min = -1;
		diffVal = -1;
		frontVal = -1;
		((Control)this).BeginInvoke((Delegate)(MethodInvoker)delegate
		{
			((Control)txtMax).Text = max.ToString();
			((Control)txtMin).Text = min.ToString();
			((Control)txtChaz).Text = diffVal.ToString();
		});
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		int statorID = u3.GetStatorID();
		SPHelper.SendTOStator(statorID, 2, 167, 1);
		canStop = false;
		Task task = new Task(delegate
		{
			while (!canStop)
			{
				UpdatChar();
				Thread.Sleep(30);
			}
		});
		task.Start();
	}

	private void btnInit_Click(object sender, EventArgs e)
	{
		InitChart();
	}

	private void Form2_FormClosing(object sender, FormClosingEventArgs e)
	{
		btnStop_Click(null, null);
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
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Expected O, but got Unknown
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Expected O, but got Unknown
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_064e: Expected O, but got Unknown
		//IL_0870: Unknown result type (might be due to invalid IL or missing references)
		//IL_087a: Expected O, but got Unknown
		//IL_0e80: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e8a: Expected O, but got Unknown
		//IL_0f09: Unknown result type (might be due to invalid IL or missing references)
		//IL_10cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d9: Expected O, but got Unknown
		//IL_1158: Unknown result type (might be due to invalid IL or missing references)
		//IL_12b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_12c3: Expected O, but got Unknown
		//IL_12f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1520: Unknown result type (might be due to invalid IL or missing references)
		//IL_152a: Expected O, but got Unknown
		components = new Container();
		ChartArea val = new ChartArea();
		Legend val2 = new Legend();
		Series val3 = new Series();
		uiPanel1 = new UIPanel();
		btnInit = new UIButton();
		btnClear = new UIButton();
		btnStaic = new UIButton();
		txtChaz = new TextBox();
		label3 = new Label();
		txtYmin = new TextBox();
		label5 = new Label();
		txtMin = new TextBox();
		txtYmax = new TextBox();
		label2 = new Label();
		label4 = new Label();
		txtMax = new TextBox();
		label1 = new Label();
		uiButton1 = new UIButton();
		btnReadReg = new UIButton();
		uiPanel2 = new UIPanel();
		chart1 = new Chart();
		timer1 = new Timer(components);
		((Control)uiPanel1).SuspendLayout();
		((Control)uiPanel2).SuspendLayout();
		((ISupportInitialize)chart1).BeginInit();
		((Control)this).SuspendLayout();
		((Control)uiPanel1).Controls.Add((Control)(object)btnInit);
		((Control)uiPanel1).Controls.Add((Control)(object)btnClear);
		((Control)uiPanel1).Controls.Add((Control)(object)btnStaic);
		((Control)uiPanel1).Controls.Add((Control)(object)txtChaz);
		((Control)uiPanel1).Controls.Add((Control)(object)label3);
		((Control)uiPanel1).Controls.Add((Control)(object)txtYmin);
		((Control)uiPanel1).Controls.Add((Control)(object)label5);
		((Control)uiPanel1).Controls.Add((Control)(object)txtMin);
		((Control)uiPanel1).Controls.Add((Control)(object)txtYmax);
		((Control)uiPanel1).Controls.Add((Control)(object)label2);
		((Control)uiPanel1).Controls.Add((Control)(object)label4);
		((Control)uiPanel1).Controls.Add((Control)(object)txtMax);
		((Control)uiPanel1).Controls.Add((Control)(object)label1);
		((Control)uiPanel1).Controls.Add((Control)(object)uiButton1);
		((Control)uiPanel1).Controls.Add((Control)(object)btnReadReg);
		((Control)uiPanel1).Dock = (DockStyle)2;
		uiPanel1.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel1).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel1).ForeColor = Color.Silver;
		((Control)uiPanel1).Location = new Point(0, 533);
		((Control)uiPanel1).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel1).MinimumSize = new Size(1, 1);
		((Control)uiPanel1).Name = "uiPanel1";
		uiPanel1.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel1).Size = new Size(1243, 107);
		uiPanel1.Style = UIStyle.Black;
		((Control)uiPanel1).TabIndex = 1;
		((Control)uiPanel1).Text = null;
		uiPanel1.TextAlignment = (ContentAlignment)32;
		((Control)btnInit).Cursor = Cursors.Hand;
		btnInit.FillColor = Color.FromArgb(15, 40, 70);
		btnInit.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnInit.FillPressColor = Color.FromArgb(235, 243, 255);
		btnInit.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnInit).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnInit.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnInit.ForePressColor = Color.FromArgb(130, 130, 130);
		btnInit.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnInit).Location = new Point(420, 58);
		((Control)btnInit).MinimumSize = new Size(1, 1);
		((Control)btnInit).Name = "btnInit";
		btnInit.RectColor = Color.FromArgb(130, 130, 130);
		btnInit.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnInit.RectPressColor = Color.FromArgb(130, 130, 130);
		btnInit.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnInit).Size = new Size(74, 29);
		btnInit.Style = UIStyle.Black;
		((Control)btnInit).TabIndex = 38;
		((Control)btnInit).Text = "初始化";
		((Control)btnInit).Click += btnInit_Click;
		((Control)btnClear).Cursor = Cursors.Hand;
		btnClear.FillColor = Color.FromArgb(15, 40, 70);
		btnClear.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnClear.FillPressColor = Color.FromArgb(235, 243, 255);
		btnClear.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnClear).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnClear.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnClear.ForePressColor = Color.FromArgb(130, 130, 130);
		btnClear.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnClear).Location = new Point(671, 7);
		((Control)btnClear).MinimumSize = new Size(1, 1);
		((Control)btnClear).Name = "btnClear";
		btnClear.Radius = 25;
		btnClear.RectColor = Color.FromArgb(130, 130, 130);
		btnClear.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnClear.RectPressColor = Color.FromArgb(130, 130, 130);
		btnClear.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnClear).Size = new Size(74, 29);
		btnClear.Style = UIStyle.Black;
		((Control)btnClear).TabIndex = 37;
		((Control)btnClear).Text = "自动";
		((Control)btnClear).Click += btnClear_Click;
		((Control)btnStaic).Cursor = Cursors.Hand;
		btnStaic.FillColor = Color.FromArgb(15, 40, 70);
		btnStaic.FillHoverColor = Color.FromArgb(216, 233, 255);
		btnStaic.FillPressColor = Color.FromArgb(235, 243, 255);
		btnStaic.FillSelectedColor = Color.FromArgb(235, 243, 255);
		((Control)btnStaic).Font = new Font("微软雅黑", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)134);
		btnStaic.ForeHoverColor = Color.FromArgb(130, 130, 130);
		btnStaic.ForePressColor = Color.FromArgb(130, 130, 130);
		btnStaic.ForeSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStaic).Location = new Point(570, 6);
		((Control)btnStaic).MinimumSize = new Size(1, 1);
		((Control)btnStaic).Name = "btnStaic";
		btnStaic.Radius = 25;
		btnStaic.RectColor = Color.FromArgb(130, 130, 130);
		btnStaic.RectHoverColor = Color.FromArgb(130, 130, 130);
		btnStaic.RectPressColor = Color.FromArgb(130, 130, 130);
		btnStaic.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)btnStaic).Size = new Size(74, 29);
		btnStaic.Style = UIStyle.Black;
		((Control)btnStaic).TabIndex = 36;
		((Control)btnStaic).Text = "统计";
		((Control)btnStaic).Click += btnStaic_Click;
		((Control)txtChaz).Location = new Point(439, 6);
		((Control)txtChaz).Name = "txtChaz";
		((Control)txtChaz).Size = new Size(100, 34);
		((Control)txtChaz).TabIndex = 35;
		((Control)txtChaz).Text = "0";
		((Control)label3).AutoSize = true;
		((Control)label3).Location = new Point(375, 12);
		((Control)label3).Name = "label3";
		((Control)label3).Size = new Size(72, 27);
		((Control)label3).TabIndex = 34;
		((Control)label3).Text = "差值：";
		((Control)txtYmin).Location = new Point(281, 60);
		((Control)txtYmin).Name = "txtYmin";
		((Control)txtYmin).Size = new Size(100, 34);
		((Control)txtYmin).TabIndex = 35;
		((Control)txtYmin).Text = "2054000";
		((Control)label5).AutoSize = true;
		((Control)label5).Location = new Point(200, 66);
		((Control)label5).Name = "label5";
		((Control)label5).Size = new Size(75, 27);
		((Control)label5).TabIndex = 34;
		((Control)label5).Text = "Y-Min:";
		((Control)txtMin).Location = new Point(249, 8);
		((Control)txtMin).Name = "txtMin";
		((Control)txtMin).Size = new Size(100, 34);
		((Control)txtMin).TabIndex = 35;
		((Control)txtMin).Text = "0";
		((Control)txtYmax).Location = new Point(87, 61);
		((Control)txtYmax).Name = "txtYmax";
		((Control)txtYmax).Size = new Size(100, 34);
		((Control)txtYmax).TabIndex = 35;
		((Control)txtYmax).Text = "2056000";
		((Control)label2).AutoSize = true;
		((Control)label2).Location = new Point(185, 14);
		((Control)label2).Name = "label2";
		((Control)label2).Size = new Size(54, 27);
		((Control)label2).TabIndex = 34;
		((Control)label2).Text = "Min:";
		((Control)label4).AutoSize = true;
		((Control)label4).Location = new Point(8, 67);
		((Control)label4).Name = "label4";
		((Control)label4).Size = new Size(79, 27);
		((Control)label4).TabIndex = 34;
		((Control)label4).Text = "Y-Max:";
		((Control)txtMax).Location = new Point(72, 9);
		((Control)txtMax).Name = "txtMax";
		((Control)txtMax).Size = new Size(100, 34);
		((Control)txtMax).TabIndex = 35;
		((Control)txtMax).Text = "0";
		((Control)label1).AutoSize = true;
		((Control)label1).Location = new Point(8, 15);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(58, 27);
		((Control)label1).TabIndex = 34;
		((Control)label1).Text = "Max:";
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
		((Control)uiButton1).Location = new Point(859, 7);
		((Control)uiButton1).Margin = new Padding(2);
		((Control)uiButton1).MinimumSize = new Size(1, 1);
		((Control)uiButton1).Name = "uiButton1";
		uiButton1.Radius = 25;
		uiButton1.RectColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectHoverColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectPressColor = Color.FromArgb(130, 130, 130);
		uiButton1.RectSelectedColor = Color.FromArgb(130, 130, 130);
		((Control)uiButton1).Size = new Size(74, 29);
		uiButton1.Style = UIStyle.Black;
		((Control)uiButton1).TabIndex = 33;
		((Control)uiButton1).Text = "停止";
		((Control)uiButton1).Click += btnStop_Click;
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
		((Control)btnReadReg).Location = new Point(771, 7);
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
		((Control)btnReadReg).TabIndex = 33;
		((Control)btnReadReg).Text = "手动";
		((Control)btnReadReg).Click += btnStart_Click;
		((Control)uiPanel2).Controls.Add((Control)(object)chart1);
		((Control)uiPanel2).Dock = (DockStyle)5;
		uiPanel2.FillColor = Color.FromArgb(24, 24, 24);
		((Control)uiPanel2).Font = new Font("微软雅黑", 12f);
		((Control)uiPanel2).ForeColor = Color.Silver;
		((Control)uiPanel2).Location = new Point(0, 35);
		((Control)uiPanel2).Margin = new Padding(4, 5, 4, 5);
		((Control)uiPanel2).MinimumSize = new Size(1, 1);
		((Control)uiPanel2).Name = "uiPanel2";
		uiPanel2.RectColor = Color.FromArgb(130, 130, 130);
		((Control)uiPanel2).Size = new Size(1243, 498);
		uiPanel2.Style = UIStyle.Black;
		((Control)uiPanel2).TabIndex = 2;
		((Control)uiPanel2).Text = "uiPanel2";
		uiPanel2.TextAlignment = (ContentAlignment)32;
		((ChartNamedElement)val).Name = "ChartArea1";
		((Collection<ChartArea>)(object)chart1.ChartAreas).Add(val);
		((Control)chart1).Dock = (DockStyle)5;
		((ChartNamedElement)val2).Name = "Legend1";
		((Collection<Legend>)(object)chart1.Legends).Add(val2);
		((Control)chart1).Location = new Point(0, 0);
		((Control)chart1).Name = "chart1";
		val3.ChartArea = "ChartArea1";
		val3.ChartType = (SeriesChartType)4;
		val3.Legend = "Legend1";
		((ChartNamedElement)val3).Name = "Series1";
		((Collection<Series>)(object)chart1.Series).Add(val3);
		chart1.Size = new Size(1243, 498);
		((Control)chart1).TabIndex = 0;
		((Control)chart1).Text = "chart1";
		timer1.Interval = 30;
		timer1.Tick += timer1_Tick;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(12f, 27f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(1243, 640);
		((Control)this).Controls.Add((Control)(object)uiPanel2);
		((Control)this).Controls.Add((Control)(object)uiPanel1);
		((Control)this).Name = "Form2";
		base.Style = UIStyle.Custom;
		((Control)this).Text = "波形图";
		((Form)this).FormClosing += new FormClosingEventHandler(Form2_FormClosing);
		((Control)uiPanel1).ResumeLayout(false);
		((Control)uiPanel1).PerformLayout();
		((Control)uiPanel2).ResumeLayout(false);
		((ISupportInitialize)chart1).EndInit();
		((Control)this).ResumeLayout(false);
	}
}
