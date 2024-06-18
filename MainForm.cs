using sunflower_aria2_ui.UI;
using Aria2NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sunflower_aria2_ui
{
    public partial class MainForm : Form
    {
        private Point mouseOff;//鼠标移动位置变量
        private bool leftFlag;//鼠标是否为左键
        bool sidebarExpand = true;//侧边栏状态

        public DownloadForm downloadForm = new DownloadForm(); //子窗口准备
        AboutForm aboutForm = new AboutForm();
        SettingForm SettingForm = new SettingForm();

        static string exePath = System.Reflection.Assembly.GetEntryAssembly().Location; // 获取当前exe的路径
        static string exeDirectory = Path.GetDirectoryName(exePath); // 获取exe所在目录的路径
        static string exe = Path.Combine(Path.Combine(exeDirectory, "aria2c"), "aria2c_64.exe");

        Process Aria2cProcess = ExeHelper.CreateCmdProcess(exe, "--conf-path=aria2.conf", "aria2c");

        private void FormSizeSetting(object sender, EventArgs e)
        {
            downloadForm.Width = flowLayoutPanel2.Width;
            downloadForm.Height = flowLayoutPanel2.Height;
            aboutForm.Width = flowLayoutPanel2.Width;
            aboutForm.Height = flowLayoutPanel2.Height;
            SettingForm.Width = flowLayoutPanel2.Width;
            SettingForm.Height = flowLayoutPanel2.Height;
        }
        public MainForm()
        {
            InitializeComponent();
            this.IsMdiContainer = true;
            this.Load += new EventHandler(Form_Load);
        }
        private void Form_Load(object sender, EventArgs e)
        {
            SettingForm.Hide();
            aboutForm.Hide();

            downloadForm.MdiParent = this;
            downloadForm.Parent = flowLayoutPanel2;
            downloadForm.Show();
            downloadForm.MdiParent = this;
            downloadForm.Parent = flowLayoutPanel2;

            button3.BackColor = Config.InvertColor();
            button2.BackColor = Config.UIColor;
            button5.BackColor = Config.UIColor;

            Aria2cProcess.Start();

        }
        #region 隐藏标题栏后移动窗口

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y);//获得当前鼠标的坐标
                leftFlag = true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                this.WindowState = FormWindowState.Normal;
                Point mouseSet = Control.MousePosition;//获得移动后鼠标的坐标
                mouseSet.Offset(mouseOff.X, mouseOff.Y);//设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;
            }
        }

        #endregion
        private void panel2_Click(object sender, EventArgs e)
        {
            flowLayoutPanel2.Visible = false;
            sidebarTimer.Start();
        }

        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            if (sidebarExpand)
            {
                sidebar.Width -= (10 + sidebar.Width / 10);
                if (sidebar.Width == sidebar.MinimumSize.Width)
                {
                    label1.Visible = false;
                    button9.Image = Properties.Resources.展开菜单;
                    sidebarExpand = false;
                    flowLayoutPanel2.Visible = true;
                    sidebarTimer.Stop();
                }
            }
            else
            {
                sidebar.Width += (10 + sidebar.Width / 10);
                if (sidebar.Width == sidebar.MaximumSize.Width)
                {
                    label1.Visible = true;
                    button9.Image = Properties.Resources.收起菜单;
                    sidebarExpand = true;
                    flowLayoutPanel2.Visible = true;
                    sidebarTimer.Stop();
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            flowLayoutPanel2.Visible = false;
            //For_test();
            sidebarTimer.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SettingForm.Hide();
            aboutForm.Hide();

            downloadForm.MdiParent = this;
            downloadForm.Parent = flowLayoutPanel2;
            downloadForm.Show();
            downloadForm.MdiParent = this;
            downloadForm.Parent = flowLayoutPanel2;


            button3.BackColor = Config.InvertColor();
            button2.BackColor = Config.UIColor;
            button5.BackColor = Config.UIColor;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            aboutForm.Hide();
            downloadForm.Hide();

            SettingForm.MdiParent = this;
            SettingForm.Parent = flowLayoutPanel2;
            SettingForm.Show();
            SettingForm.MdiParent = this;
            SettingForm.Parent = flowLayoutPanel2;

            button2.BackColor = Config.InvertColor();
            button3.BackColor = Config.UIColor;
            button5.BackColor = Config.UIColor;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SettingForm.Hide();
            downloadForm.Hide();

            aboutForm.MdiParent = this;
            aboutForm.Parent = flowLayoutPanel2;
            aboutForm.Show();
            aboutForm.MdiParent = this;
            aboutForm.Parent = flowLayoutPanel2;

            button5.BackColor = Config.InvertColor();
            button2.BackColor = Config.UIColor;
            button3.BackColor = Config.UIColor;
        }
        private void tableLayoutPanel1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Aria2cProcess.Kill();
        }
    }
}
