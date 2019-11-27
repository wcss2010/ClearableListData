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
        public static string configFile = System.IO.Path.Combine(Application.StartupPath, "local.ieviewconfig");

        /// <summary>
        /// 配置对象
        /// </summary>
        public static ClearableListObject Config { get; set; }

        public MainForm()
        {
            InitializeComponent();

            //载入配置
            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show("配置文件载入失败！Ex:" + ex.ToString());
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Text += "(" + System.IO.Path.GetFileName(configFile) + ")";

            //载入需要导入的域名列表
            if (Config != null && Config.WebSiteList != null)
            {
                foreach (string s in Config.WebSiteList)
                {
                    lbxSource.Items.Add(s);
                }
            }

            //载入IE兼容性视图配置
            btnRefresh.PerformClick();
        }

        private void btnLoadDomainList_Click(object sender, EventArgs e)
        {
            try
            {
                if (Config != null && Config.WebSiteList != null)
                {
                    //当前列表
                    List<string> existList = new List<string>(new ClearableListDataHelper().GetDomains());

                    foreach (string s in Config.WebSiteList)
                    {
                        if (existList.Contains(s))
                        {
                            continue;
                        }

                        new ClearableListDataHelper().AddNewSiteToCompatibilityViewList(s);
                        existList.Add(s);
                    }
                }

                //刷新IE兼容性视图配置
                btnRefresh.PerformClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载失败！Ex:" + ex.ToString());
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lbxCurrent.SelectedItems.Count >= 1)
            {
                if (MessageBox.Show("真的要删除吗？", "提示", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        foreach (object obj in lbxCurrent.SelectedItems)
                        {
                            if (obj != null)
                            {
                                new ClearableListDataHelper().RemoveUserFilter(obj.ToString());
                            }
                        }
                        btnRefresh.PerformClick();
                        MessageBox.Show("删除完成！");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("删除失败！Ex:" + ex.ToString());
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string[] ttt = new ClearableListDataHelper().GetDomains();
                if (ttt != null)
                {
                    try
                    {
                        string tempFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "兼容性视图配置.ieviewconfig");
                        ClearableListObject clo = new ClearableListObject();
                        clo.WebSiteList.AddRange(ttt);
                        ClearableListObject.toFile(clo, tempFile);

                        MessageBox.Show("保存完成!路径:" + tempFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("保存失败！Ex:" + ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败！Ex:" + ex.ToString());
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                string[] ttt = new ClearableListDataHelper().GetDomains();
                if (ttt != null)
                {
                    foreach (string t in ttt)
                    {
                        lbxCurrent.Items.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("IE兼容性视图列表载入失败！Ex:" + ex.ToString());
            }
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