using ToolLib.AreaSelectorLib;
using ToolLib.ErrorReportLib;
using ToolLib.PosSelectorLib;
using ToolLib.AutoStartLib;
using ToolLib.CmdLib;
using ToolLib.DownloaderLib;
using ToolLib.GdiToolLib;
using ToolLib.HashLib;
using ToolLib.HexLib;
using ToolLib.HotkeyManagerLib;
using ToolLib.IniLib;
using ToolLib.InputLib;
using ToolLib.JsonLib;
using ToolLib.KeyboardHookLib;
using ToolLib.LogLib;
using ToolLib.MemoryLib;
using ToolLib.RegistryLib;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using KeyHotspot.Class;
using System.Windows.Forms.DataVisualization.Charting;

namespace KeyHotspot.Forms
{
    public partial class Main_Form : Form
    {
        #region Func
        public void Initialize()
        {
            try
            {
                if (!File.Exists(DataPath))
                {
                    GlobalData = new JsonConfig.Data.Root
                    {
                        data = new JsonConfig.Data.KeyInfo[] {
                            new JsonConfig.Data.KeyInfo
                            {
                                key = Keys.Space,
                                press_count = 1
                            }
                        }
                    };
                    Json.WriteJson(DataPath, GlobalData);
                }

                GlobalData = Json.ReadJson<JsonConfig.Data.Root>(DataPath);



                //初始化图表

                chart1.Series.Clear();
                chart1.Legends.Clear();

                Series series = new Series("按键使用率");
                series.ChartType = SeriesChartType.Column;
                series.IsValueShownAsLabel = true;
                series.Font = new Font("Microsoft YaHei", 10);

                var area = chart1.ChartAreas[0];
                area.AxisX.Title = "按键";
                area.AxisY.Title = "点击次数";
                area.AxisX.Interval = 1;
                area.AxisX.LabelStyle.Angle = -45;
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisY.MajorGrid.LineColor = Color.LightGray;

                area.AxisX.TitleFont = new Font("Microsoft YaHei", 10);
                area.AxisY.TitleFont = new Font("Microsoft YaHei", 10);

                chart1.Legends.Clear();
                chart1.Series.Add(series);

                //更新图表
                UpdateChart();



                //初始化钩子
                keyboardHook.KeyDownEvent += KeyDownEvent;

                //记录控件初始值
                chartSize = this.Height - chart1.Height;

            }
            catch (Exception ex)
            {
                if (ErrorReportBox.Show("Error", "在初始化程序时发生错误", ex) == DialogResult.Abort)
                    Environment.Exit(1);
            }
        }

        public void KeyDownEvent(object sender, KeyboardHookEventArgs e)
        {
            bool found = false;
            for (int i = 0; i < GlobalData.data.Length; i++)
            {
                if (GlobalData.data[i].key == e.Key)
                {
                    GlobalData.data[i].press_count++;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var temp = new List<JsonConfig.Data.KeyInfo>(GlobalData.data);
                temp.Add(new JsonConfig.Data.KeyInfo
                {
                    key = e.Key,
                    press_count = 1
                });
                GlobalData.data = temp.ToArray();
            }

            // 写入前先排序（按次数降序）
            GlobalData.data = GlobalData.data.OrderByDescending(k => k.press_count).ToArray();

            Json.WriteJson(DataPath, GlobalData);
            UpdateChart();
        }


        public void UpdateChart()
        {
            var series = chart1.Series[0];
            long pressCountTotal = 0;
            series.Points.Clear();

            for (int i = 0; i < GlobalData.data.Length; i++)
            {
                series.Points.AddXY(GlobalData.data[i].key.ToString(), GlobalData.data[i].press_count);
                pressCountTotal = pressCountTotal + GlobalData.data[i].press_count;
            }
            label_Total.Text = $"最高点击键: {GlobalData.data[0].key.ToString()}  最高点击数: {GlobalData.data[0].press_count}  总点击数: {pressCountTotal}";
        }
        #endregion

        #region Obj
        public KeyboardHook keyboardHook = new KeyboardHook();
        #endregion

        #region Var
        public static string RunPath = $"{Directory.GetCurrentDirectory()}";
        public static string DataPath = $"{RunPath}\\data.json";

        public static JsonConfig.Data.Root GlobalData;

        public static int chartSize;

        public static bool FormIsShow = true;
        #endregion


        public Main_Form()
        {
            InitializeComponent();
        }

        private void Main_Form_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Main_Form_Resize(object sender, EventArgs e)
        {
            chart1.Height = this.Height - chartSize;

            label_Total.Location = new Point(0, chart1.Height);
            label_Total.Width = this.Width;
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否要清除所有数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (MessageBox.Show("真的要清除所有数据吗？这是最后一次警告！", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    GlobalData.data = new JsonConfig.Data.KeyInfo[] { new JsonConfig.Data.KeyInfo { key = Keys.Space, press_count = 1 } };
                    Json.WriteJson(DataPath, GlobalData);
                    UpdateChart();
                }
            }
        }

        private void button_ClearSome_Click(object sender, EventArgs e)
        {
            var temp = new List<JsonConfig.Data.KeyInfo>();

            for (int i = 0; i < GlobalData.data.Length; i++)
            {
                if (GlobalData.data[i].press_count >= numericUpDown1.Value)
                {
                    temp.Add(GlobalData.data[i]);
                }
            }

            GlobalData.data = temp.ToArray();
            Json.WriteJson(DataPath, GlobalData);
            UpdateChart();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (FormIsShow)
            {
                FormIsShow = false;
                this.Visible = false;
                this.ShowInTaskbar = false;
            }
            else
            {
                FormIsShow = true;
                this.Visible = true;
                this.ShowInTaskbar = true;
            }
        }

        private void checkBox_Top_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_Top.Checked)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }
    }
}
