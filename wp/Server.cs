using Aria2NET;
using Newtonsoft.Json;
using sunflower_aria2_ui.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using sunflower_aria2_ui;

namespace sunflower_aria2_ui.wp
{
    internal class Server
    {
        private static object data;

        public async static Task<Dictionary<string, string>> GetDownloadUrl(
            string mcode = null,
            string url = null,
            string pwd = null,
            string wcode = null
        )
        {
            HttpClient client = new HttpClient();
            string apiUrl = "http://vps.home56.top:1234/api/v1/baiduwp/";


            if (mcode != null) {
                data = new { mcode };
            }
            else
            {
                data = new { url, pwd, wcode };
            }

            // 将 JSON 字符串转换为 HttpContent
            HttpContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string json = await response.Content.ReadAsStringAsync();
                Dictionary<string, dynamic> dictionary = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

                if (dictionary.ContainsKey("err"))
                {
                    

                    MessageBox.Show((string)dictionary["err"]);

                    return null;
                }

                if (dictionary.ContainsKey("mcode"))
                {
                    CopyForm copyFrom = new CopyForm((string)dictionary["mcode"]);
                    copyFrom.ShowDialog();

                    return null;
                }

                await TraverseDictionaryAsync(dictionary);

                return null;
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"解析服务器目前故障中... {ex}");
                    
                return null;
            }
        }

        static async Task TraverseDictionaryAsync(Dictionary<string, dynamic> dict)
        {
            foreach (var kvp in dict)
            {
                //Console.WriteLine($"{kvp.Key} : {kvp.Value}");

                if (kvp.Key == "msg")
                {
                    Aria2NetClient aria2 = await Aria2.GetClinetAsync();
                    Newtonsoft.Json.Linq.JObject msgJson = dict["msg"];
                    Dictionary<string, string> msgDict = msgJson.ToObject<Dictionary<string, string>>();
                    foreach (var item in msgDict)
                    {
                        //Console.WriteLine($"{item.Key} : {item.Value}");

                        var path = Aria2.DownloadPath("Download");
                        var temp = item.Key.Split('/');
                        foreach (var path_ in temp.Take(temp.Count() - 1))
                        {
                            path = Path.Combine(path, path_);

                        }
                        try
                        {
                            await Aria2.AddBaiduUriAsync(aria2, item.Value.ToString(), path, temp[temp.Count() - 1]);
                        }
                        catch
                        {
                            MessageBox.Show($"存在异常的文件{temp[temp.Count() - 1]}");
                        }
                    }
                }
                if (kvp.Value is Dictionary<string, dynamic> nestedDict)
                {
                    await TraverseDictionaryAsync(nestedDict);
                }
            }
        }
        private static Dictionary<string, string> TaskListGet(Dictionary<string, string> task)
        {
            if (task == null)
            {
                Dictionary<string, string> taskList = new Dictionary<string, string>();
            }

            return task;
        }
    }
}
