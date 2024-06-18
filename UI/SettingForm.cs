using System;
using IWshRuntimeLibrary;
using System.Windows.Forms;

namespace sunflower_aria2_ui.UI
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\太阳花下载器.lnk";

            CreateShortcut(exePath, shortcutPath);

            MessageBox.Show("快捷方式已创建在桌面！");
        }

        static void CreateShortcut(string targetPath, string shortcutPath)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(targetPath);

            shortcut.Save();
        }
    }
}
