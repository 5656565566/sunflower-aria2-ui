using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.CodeDom.Compiler;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using sunflower_aria2_ui.UI;
using sunflower_aria2_ui.wp;
using Aria2NET;
using System.Security.Policy;

namespace sunflower_aria2_ui.UI
{
    public partial class DownloadForm : Form
    {
        public class DownloadItem
        {
            public string Gid {  get; set; }
            public string FileName { get; set; }
            public string Speed { get; set; }
            public string Progress { get; set; }
            public string Status { get; set; }
            public string RemainderTime { get; set; }
        }

        double formWidth;//窗体原始宽度
        double formHeight;//窗体原始高度
        double scaleX;//水平缩放比例
        double scaleY;//垂直缩放比例

        Aria2NetClient aria2 = null;

        AddTaskForm AddTaskForm = new AddTaskForm();

        Dictionary<string, string> ControlsInfo = new Dictionary<string, string>();//控件中心Left,Top,控件Width,控件Height,控件字体Size

        BindingList<DownloadItem> downloadItems = new BindingList<DownloadItem>();
        List<string> gids = new List<string>();

        public class MoveOverInfoTip
        {
            //信息提示组件
            private static ToolTip toolTip1 = new ToolTip();

            /// <summary>
            /// 设置单个控件提示信息
            /// </summary>
            /// <typeparam name="T">组件类型</typeparam>
            /// <param name="t">组件</param>
            /// <param name="tipInfo">需要显示的提示信息</param>
            public static void SettingSingleTipInfo<T>(T t, string tipInfo) where T : Control
            {
                toolTip1.SetToolTip(t, tipInfo);
            }


            /// <summary>
            /// 设置多个同种类型的提示信息
            /// </summary>
            /// <typeparam name="T">组件类型</typeparam>
            /// <param name="dic">组件和提示信息字典</param>
            public static void SettingMutiTipInfo<T>(Dictionary<T, string> dic) where T : Control
            {
                if (dic == null || dic.Count <= 0) return;

                foreach (var item in dic)
                {
                    toolTip1.SetToolTip(item.Key, item.Value);
                }

            }
        } 

        private void NewHeaderCell()
        {

            dataGridView1.ScrollBars = ScrollBars.Vertical;

            dataGridView1.DataSource = downloadItems;

            dataGridView1.Columns["Gid"].Visible = false;

            dataGridView1.Columns["FileName"].HeaderText = "文件名";
            dataGridView1.Columns["FileName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["FileName"].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns["FileName"].ReadOnly = true;
            dataGridView1.Columns["FileName"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.Columns["Speed"].HeaderText = "速度";
            dataGridView1.Columns["Speed"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["Speed"].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns["Speed"].ReadOnly = true;
            dataGridView1.Columns["Speed"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.Columns["Progress"].HeaderText = "进度";
            dataGridView1.Columns["Progress"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["Progress"].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns["Progress"].ReadOnly = true;
            dataGridView1.Columns["Progress"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.Columns["Status"].HeaderText = "状态";
            dataGridView1.Columns["Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["Status"].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns["Status"].ReadOnly = true;
            dataGridView1.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.Columns["RemainderTime"].HeaderText = "剩余时间";
            dataGridView1.Columns["RemainderTime"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["RemainderTime"].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns["RemainderTime"].ReadOnly = true;
            dataGridView1.Columns["RemainderTime"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private async void Form_Load(object sender, EventArgs e)
        {
            aria2 = await Aria2.GetClinetAsync();
            Dictionary<Button, string> dic = new Dictionary<Button, string>
            {
                { button1, "刷新" }
            };
            MoveOverInfoTip.SettingMutiTipInfo(dic);

            dataGridView1.RowHeadersVisible = false; // 行头隐藏 

            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Font = new Font("黑体", 13);
            dataGridView1.ReadOnly = true;

            dataGridView1.RowHeadersWidth = (int)(tableLayoutPanel1.Width * 0.8);
            dataGridView1.ColumnHeadersHeight = tableLayoutPanel1.Height;

            timer1.Enabled = true;

        }
        protected void GetAllInitInfo(Control ctrlContainer)
        {
            if (ctrlContainer.Parent == this)//获取窗体的高度和宽度
            {
                formWidth = Convert.ToDouble(ctrlContainer.Width);
                formHeight = Convert.ToDouble(ctrlContainer.Height);
            }
            foreach (Control item in ctrlContainer.Controls)
            {
                if (item.Name.Trim() != "")
                {
                    //添加信息：键值：控件名，内容：据左边距离，距顶部距离，控件宽度，控件高度，控件字体。
                    ControlsInfo.Add(item.Name, (item.Left + item.Width / 2) + "," + (item.Top + item.Height / 2) + "," + item.Width + "," + item.Height + "," + item.Font.Size);
                }
                if ((item as UserControl) == null && item.Controls.Count > 0)
                {
                    GetAllInitInfo(item);
                }
            }
            

        }
        private void ControlsChangeInit(Control ctrlContainer)
        {
            scaleX = (Convert.ToDouble(ctrlContainer.Width) / formWidth);
            scaleY = (Convert.ToDouble(ctrlContainer.Height) / formHeight);
        }
        /// <summary>
        /// 改变控件大小
        /// </summary>
        /// <param name="ctrlContainer"></param>
        private void ControlsChange(Control ctrlContainer)
        {
            /*
            double[] pos = new double[5];//pos数组保存当前控件中心Left,Top,控件Width,控件Height,控件字体Size

            
            foreach (DataGridViewTextBoxColumn item in ColumnHead)
            {
                string[] strs = ControlsInfo[item.HeaderText].Split(',');
                for (int i = 0; i < 1; i++)
                {
                    pos[i] = Convert.ToDouble(strs[i]);//添加到临时数组
                }
                double itemWidth = pos[0] * scaleX;     //计算控件宽度，double类型
                item.Width = Convert.ToInt32(itemWidth);//控件宽度，int类型
            }
            

            foreach (Control item in ctrlContainer.Controls)//遍历控件
            {
                if (item.Name.Trim() != "")//如果控件名不是空，则执行
                {
                    if ((item as UserControl) == null && item.Controls.Count > 0)//如果不是自定义控件
                    {
                        ControlsChange(item);//循环执行
                    }
                    string[] strs = ControlsInfo[item.Name].Split(',');//从字典中查出的数据，以‘，’分割成字符串组

                    for (int i = 0; i < 5; i++)
                    {
                        pos[i] = Convert.ToDouble(strs[i]);//添加到临时数组
                    }
                    double itemWidth = pos[2] * scaleX;     //计算控件宽度，double类型
                    double itemHeight = pos[3] * scaleY;    //计算控件高度
                    item.Left = Convert.ToInt32(pos[0] * scaleX - itemWidth / 2);//计算控件距离左边距离
                    item.Top = Convert.ToInt32(pos[1] * scaleY - itemHeight / 2);//计算控件距离顶部距离
                    item.Width = Convert.ToInt32(itemWidth);//控件宽度，int类型
                    item.Height = Convert.ToInt32(itemHeight);//控件高度
                    try
                    {
                        item.Font = new Font(item.Font.Name, float.Parse((pos[4] * Math.Min(scaleX, scaleY)).ToString()));//字体
                    }
                    catch
                    {

                    }
                }
            }
            */

        }
        private void Form_SizeChanged(object sender, EventArgs e)
        {

            if (ControlsInfo.Count > 0)//如果字典中有数据，即窗体改变
            {
                DataGridSize();

                ControlsChangeInit(Controls[0]);//表示pannel控件

                ControlsChange(Controls[0]);

            }
        }

        private async Task GetAllTasks()
        {
            if(aria2 != null)
            {
                try
                {
                    await Aria2.Tell(aria2, "All");
                }
                catch
                {
                    Aria2.AllTasks.Clear();
                }

                foreach (var task in Aria2.AllTasks)
                {
                    DownloadItem downloadItem = new DownloadItem();

                    if (task.Files[0].Path.Split('/').Last() != "")
                    {   
                        downloadItem.FileName = task.Files[0].Path.Split('/').Last();
                        try
                        {
                            downloadItem.Progress = $"{task.CompletedLength * 100 / task.TotalLength} %";
                        }
                        catch
                        {
                            downloadItem.Progress =  "0 %";
                        }

                        downloadItem.Speed = $"{Math.Round(task.DownloadSpeed / 1024.0 / 1024.0, 2)} MB/s";

                        if (task.Status == "active")
                        {
                            downloadItem.Status = "下载中";
                        }
                        else if (task.Status == "waiting")
                        {
                            downloadItem.Status = "等待中";
                        }
                        else if (task.Status == "paused")
                        {
                            downloadItem.Status = "暂停中";
                        }
                        else if (task.Status == "error")
                        {
                            downloadItem.Status = "发生错误";
                        }
                        else if (task.Status == "complete")
                        {
                            downloadItem.Status = "下载完成";
                        }
                        else
                        {
                            downloadItem.Status = "被删除";
                        }
                        try
                        {
                            long time = (task.TotalLength - task.CompletedLength) / task.DownloadSpeed;
                            long hours = time / 3600; // 计算小时
                            long minutes = (time % 3600) / 60; // 计算分钟
                            long seconds = time % 60; // 计算秒

                            if (hours > 0 && minutes> 0 && seconds > 0)
                            {
                                downloadItem.RemainderTime = $"{hours} 时 {minutes} 分 {seconds} 秒";
                            }
                            else if (minutes > 0 && seconds > 0)
                            {
                                downloadItem.RemainderTime = $"{minutes} 分 {seconds} 秒";
                            }
                            else
                            {
                                downloadItem.RemainderTime = time.ToString() + " 秒";
                            }
                        }
                        catch
                        {
                            downloadItem.RemainderTime = "未知";
                        }

                        downloadItem.Gid = task.Gid;

                        if (gids.Contains(downloadItem.Gid))
                        {
                            foreach (DownloadItem temp in downloadItems)
                            {
                                if (temp.Gid == task.Gid)
                                {
                                    temp.FileName = downloadItem.FileName;
                                    temp.Progress = downloadItem.Progress;
                                    temp.Speed = downloadItem.Speed;
                                    temp.Status = downloadItem.Status;
                                    temp.RemainderTime = downloadItem.RemainderTime;
                                }
                            }
                        }
                        else
                        {
                            downloadItems.Add(downloadItem);
                            gids.Add(task.Gid);
                        }
                    }
                }
            }
        }
        private void DataGridSize()
        {
            int totalWidth = dataGridView1.ClientSize.Width; // DataGridView的宽度
            int FileName = totalWidth * 30 / 100;
            int Speed = totalWidth * 15 / 100;
            int Progress = totalWidth * 15 / 100;
            int Status = totalWidth * 15 / 100;
            int RemainderTime = totalWidth * 25 / 100;

            dataGridView1.Columns["FileName"].Width = FileName;
            dataGridView1.Columns["Speed"].Width = Speed;
            dataGridView1.Columns["Progress"].Width = Progress;
            dataGridView1.Columns["Status"].Width = Status;
            dataGridView1.Columns["RemainderTime"].Width = RemainderTime;
        }

        private void Refresh_data()
        {
            DataGridSize();
            dataGridView1.DataSource = downloadItems;

            List<int> Selecteds = new List<int>();
            foreach (DataGridViewRow r in dataGridView1.SelectedRows)
            {
                if (!r.IsNewRow)
                {
                    int rowIndex = r.Index;
                    Selecteds.Add(rowIndex);
                }
            }
            try
            {
                dataGridView1.Rows[0].Selected = false;
            }
            catch
            {

            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (Selecteds.Contains(i))
                {
                    dataGridView1.Rows[i].Selected = true;
                }
            }
            dataGridView1.Refresh();
        }
        public DownloadForm()
        {
            InitializeComponent();
            NewHeaderCell();
            GetAllInitInfo(Controls[0]);
            Load += new EventHandler(Form_Load);
            /*
            foreach (DataGridViewTextBoxColumn item in ColumnHead)
            {
                ControlsInfo.Add(item.HeaderText, item.Width.ToString());
            }
            */
        }
        private async void timer1_Tick(object sender, EventArgs e)
        {
            await GetAllTasks();
            Refresh_data();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await GetAllTasks();
            Refresh_data();
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            
            MessageBox.Show("test");
        }

        private void dataGridView1_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Selected = true;
            }
        }

        private void 反选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Selected =  !dataGridView1.Rows[i].Cells[0].Selected;
            }
        }

        private void 全不选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Selected = false;
            }
        }

        private async void 开始ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in dataGridView1.SelectedRows)
            {
                if (!r.IsNewRow)
                {
                    int rowIndex = r.Index;
                    var downloadItem = downloadItems[rowIndex];
                    var gid = downloadItem.Gid;
                    var aria2 = await Aria2.GetClinetAsync();
                    if (aria2 != null)
                    {
                        await aria2.UnpauseAsync(gid);
                    }
                    gids.Remove(gid);
                    dataGridView1.Rows.Remove(r);
                }
            }
        }

        private async void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in dataGridView1.SelectedRows)
            {
            if (!r.IsNewRow)
                {
                    int rowIndex = r.Index;
                    var downloadItem = downloadItems[rowIndex];
                    var gid = downloadItem.Gid;
                    var aria2 = await Aria2.GetClinetAsync();
                    if (aria2 != null)
                    {
                        try
                        {
                            var task = await aria2.TellStatusAsync(gid);
                            var taskDir = task.Dir;
                            var taskName = task.Files[0].Path.Split('/').Last();
                            await aria2.PauseAsync(gid);
                            await aria2.RemoveAsync(gid);
                            File.Delete(taskDir + "\\" + taskName);
                            File.Delete(taskDir + "\\" + taskName + ".aria2");
                        }
                        catch
                        {
                            try
                            {
                                await aria2.RemoveDownloadResultAsync(gid);
                            }
                            catch
                            {
                                await aria2.ForceRemoveAsync(gid);
                            }
                        }
                    }
                    gids.Remove(gid);
                    dataGridView1.Rows.Remove(r);
                }
            }
        }

        private async void 新任务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTaskForm.ShowDialog();
            if(AddTaskForm.url.Length > 0)
            {
                foreach(var url in AddTaskForm.url)
                {
                    var aria2 = await Aria2.GetClinetAsync();
                    if (aria2 != null)
                    {
                        var path = Aria2.DownloadPath("Download");
                        await Aria2.AddOneUriAsync(aria2, url, path);
                    }
                }
                AddTaskForm.url = null;
            }
        }

        private async void 暂停ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in dataGridView1.SelectedRows)
            {
                if (!r.IsNewRow)
                {
                    int rowIndex = r.Index;
                    var downloadItem = downloadItems[rowIndex];
                    var gid = downloadItem.Gid;
                    var aria2 = await Aria2.GetClinetAsync();
                    if (aria2 != null)
                    {
                        try
                        {
                            await aria2.PauseAsync(gid);
                        }
                        catch { }
                    }
                }
            }
        }

        private async void 停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in dataGridView1.SelectedRows)
            {
                if (!r.IsNewRow)
                {
                    int rowIndex = r.Index;
                    var downloadItem = downloadItems[rowIndex];
                    var gid = downloadItem.Gid;
                    var aria2 = await Aria2.GetClinetAsync();
                    if (aria2 != null)
                    {
                        try
                        {
                            await aria2.ForcePauseAsync(gid);
                        }
                        catch { }
                    }
                }
            }
        }


        private void 前往下载文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Aria2.DownloadPath("Download")))
            {
                Directory.CreateDirectory(Aria2.DownloadPath("Download"));
            }
            try
            {
                Process.Start("explorer.exe", Aria2.DownloadPath("Download"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}