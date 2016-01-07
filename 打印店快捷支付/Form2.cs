using cn.bmob.api;
using cn.bmob.io;
using cn.bmob.tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using 打印店快捷支付.BmobModel;

namespace 打印店快捷支付
{
    public partial class Form2 : Form
    {
        private string shop;//店名
        private BmobWindows bmob;
        public Form2()
        {
            InitializeComponent();
        }

        public Form2(string shop)
        {
            bmob = new BmobWindows();
            bmob = new BmobWindows();
            //初始化ApplicationId，这个ApplicationId需要更改为你自己的ApplicationId（ http://www.bmob.cn 上注册登录之后，创建应用可获取到ApplicationId）
            bmob.initialize("4c69d6ed89b6591d050f809d70930b2a", "bce9f6f7da21f79dd7f3d77c3b84d335");
            BmobDebug.Register(msg => { Debug.WriteLine(msg); });

            InitializeComponent();
            this.shop = shop;
            this.Text = shop + "--收款记录";
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            var query = new BmobQuery();
            query.WhereContainedIn<string>("shop", this.shop);
            query.OrderByDescending("ID");
            var future = bmob.FindTaskAsync<BmobQuickChargeObject>("Quick_Charge", query);
            var res = future.Result;
            foreach (var one in future.Result.results)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = one.objectId;
                lvi.SubItems.Add(float.Parse(one.total)/100 + "");
                lvi.SubItems.Add(one.channel);
                if (one.state.Equals("PAID"))
                {
                    lvi.SubItems.Add("√");
                }
                else
                {
                    lvi.SubItems.Add("×");
                }
                lvi.SubItems.Add(one.createdAt);
                lvi.SubItems.Add(one.remark);
                //lvi.SubItems.Add((one.totalPrice.Get() / 100.0).ToString() + "元");
                this.listView1.Items.Add(lvi);
            }
        }
    }
}
