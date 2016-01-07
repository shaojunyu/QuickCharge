using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace 打印店快捷支付
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            //Application.Run(new Form2("东篱阳光图文"));
        }
    }
}
