using Aria2NET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace sunflower_aria2_ui
{
    internal class Aria2
    {
        public static string[] GidList = { }; // 被创建任务的GID
        public static IList<DownloadStatusResult> AllTasks; //所有的任务
        public static IList<DownloadStatusResult> ActiveTasks; //活动中任务
        public static IList<DownloadStatusResult> StoppedTasks; //停止中任务
        public static IList<DownloadStatusResult> WaitingTasks; //等待中任务
        /// <summary>
        /// 用于获取 Aria2 服务端对象
        /// </summary>
        /// <param name="url">链接 默认 http://127.0.0.1:6800/jsonrpc</param>
        /// <returns>Aria2NetClient</returns>
        static public async Task<Aria2NetClient> GetClinetAsync(string url = "http://127.0.0.1:6800/jsonrpc") {
            var client = new Aria2NetClient(url);
            try
            {
                await client.GetVersionAsync();
                return client;
            }

            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 用于拼接下载路径
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>当前文件应当保存位置的绝对路径</returns>
        static public string DownloadPath(string path) {
            string exePath = System.Reflection.Assembly.GetEntryAssembly().Location; // 获取当前exe的路径
            string exeDirectory = Path.GetDirectoryName(exePath); // 获取exe所在目录的路径
            return Path.Combine(exeDirectory, path);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clinet"></param>
        /// <param name="uri"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static public async Task AddBaiduUriAsync(Aria2NetClient clinet, string uri, string path, string name)
        {
            var gid = await clinet.AddUriAsync(new List<string>
            {
                uri
            },
            new Dictionary<string, object>
            {
                { "dir", DownloadPath(path)},
                { "user-agent","LogStatistic" },
                { "out", name }
            }, 0);
            await clinet.PauseAsync(gid);
            GidList.Append(gid);
        }

        /// <summary>
        /// 向 Aria2 发送一个文件下载申请
        /// </summary>
        /// <param name="clinet">Aria2 连接</param>
        /// <param name="uri">下载地址</param>
        /// <param name="path">保存的路径</param>
        static public async Task AddOneUriAsync(Aria2NetClient clinet, string uri, string path)
        {
            try
            {
                var gid = await clinet.AddUriAsync(new List<string>
            {
                uri
            },
            new Dictionary<string, object>
            {
                { "dir", DownloadPath(path) }
            }, 0);
                GidList.Append(gid);
            }
            catch
            {
                MessageBox.Show($"{uri}\n下载失败 !");
            }
        }
        /// <summary>
        /// 获取下载状态
        /// </summary>
        /// <param name="client"></param>
        /// <param name="gid">文件id</param>
        static public async Task<DownloadStatusResult> TellStatus(Aria2NetClient client, string gid)
        {
            return await client.TellStatusAsync(gid);
        }

        /// <summary>
        /// 未完成
        /// </summary>
        /// <param name="clinet"></param>
        /// <param name="uri"></param>
        /// <param name="path"></param>
        static public async void AddOneTorrentAsync(Aria2NetClient clinet, string uri, string path)
        {
            await clinet.AddUriAsync(new List<string>
            {
                uri
            },
            new Dictionary<string, object>
            {
                { "dir", DownloadPath(path)}
            }, 0);
        }
        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="client"></param>
        /// <param name="gid"></param>
        /// <returns></returns>
        static public async Task<string> PauseAsync(Aria2NetClient client, string gid)
        {
            return await client.PauseAsync(gid);
        }
        /// <summary>
        /// 暂停所有任务
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        static public async Task<bool> PauseAllAsync(Aria2NetClient client)
        {
            return await client.PauseAllAsync();
        }
        static public async Task Tell(Aria2NetClient client, string type)
        {
            if (type == "Active")
            {
                ActiveTasks = await client.TellActiveAsync();
            }
            else if (type == "Stopped")
            {
                StoppedTasks = await client.TellStoppedAsync(0, -1);
            }
            else if (type == "Waiting")
            {
                WaitingTasks = await client.TellWaitingAsync(0, -1);
            }
            else
            {
                AllTasks = await client.TellAllAsync();
            }
        }
    }
    public class ExeHelper
    {
        /// <summary>
        /// 使用cmd执行命令
        /// </summary>
        /// <param name="command">要执行的程序</param>
        /// <param name="arguments">需要传递的参数</param>
        /// <param name="workDirPath">工作目录</param>
        /// <returns>执行操作的线程</returns>
        public static Process CreateCmdProcess(string command, string arguments, string workDirPath = "")
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(command, arguments);

            processInfo.UseShellExecute = true;

            processInfo.CreateNoWindow = true;

            processInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (!string.IsNullOrWhiteSpace(workDirPath))
            {
                processInfo.WorkingDirectory = workDirPath;
            }

            Process process = new Process();

            process.StartInfo = processInfo;

            return process;
        }
        /// <summary>
        /// 进程结束
        /// </summary>
        /// <param name="ProcName">进程名</param>
        /// <returns>执行结果</returns>
        public static bool CloseProc(string ProcName)
        {
            bool result = false;
            ArrayList procList = new ArrayList();
            string tempName = "";

            foreach (Process thisProc in Process.GetProcesses())
            {
                tempName = thisProc.ProcessName;
                procList.Add(tempName);
                if (tempName == ProcName)
                {
                    if (!thisProc.CloseMainWindow())
                        thisProc.Kill(); //当发送关闭窗口命令无效时强行结束进程                    
                    result = true;
                }
            }
            return result;
        }
    }
}