using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sunflower_aria2_ui.UI
{
    public partial class AddTaskForm : Form
    {
        private Point mouseOff;//鼠标移动位置变量
        private bool leftFlag;//鼠标是否为左键

        public string[] url = { };

        public AddTaskForm()
        {
            InitializeComponent();
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

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (richTextBox1.Text == "")
            {
                label1.Visible = true;
            }
            else
            {
                label1.Visible = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            url = richTextBox1.Lines;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            url = richTextBox1.Lines;
            richTextBox1.Text = "";
            Close();
        }
    }
}
