using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClearableListDataApp
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                if (args[0] != null && args[0].EndsWith(".ieviewconfig") && System.IO.File.Exists(args[0]))
                {
                    MainForm.configFile = args[0];
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
