using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ClearableListDataApp
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string configFile = System.IO.Path.Combine(Application.StartupPath, "configs.cfg");

        /// <summary>
        /// 配置对象
        /// </summary>
        public static ClearableListObject Config { get; set; }

        public MainForm()
        {
            InitializeComponent();

            //载入配置
            if (System.IO.File.Exists(configFile))
            {
                Config = ClearableListObject.fromFile(configFile);
            }
            else
            {
                Config = new ClearableListObject();
                Config.WebSiteList.Add("test.com");
                ClearableListObject.toFile(Config, configFile);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Text += "(" + System.IO.Path.GetFileName(configFile) + ")";
        }
    }

    /// <summary>
    /// 兼容性视图地址列表对象
    /// </summary>
    public class ClearableListObject
    {
        public ClearableListObject()
        {
            WebSiteList = new List<string>();
        }

        public List<string> WebSiteList { get; set; }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="jsonFile"></param>
        public static void toFile(ClearableListObject obj, string jsonFile)
        {
            System.IO.File.WriteAllText(jsonFile, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public static ClearableListObject fromFile(string jsonFile)
        {
            return JsonConvert.DeserializeObject<ClearableListObject>(System.IO.File.ReadAllText(jsonFile));
        }
    }
}