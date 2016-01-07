using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using cn.bmob.api;
using cn.bmob.io;

namespace 打印店快捷支付.BmobModel
{

    class BmobQuickChargeObject : BmobTable
    {
        private String fTable;

        //以下对应云端字段名称
        public string shop { get; set; }
        public string pingppId { get; set; }
        public string total { get; set; }
        public string state { get; set; }
        public BmobBoolean iswithdrawn { get; set; }
        public string channel { get; set; }
        public string remark { set; get; }
        //构造函数
        public BmobQuickChargeObject()
        {
            this.fTable = "Quick_Charge";
        }

        public override string table
        {
            get
            {
                if (fTable != null)
                {
                    return fTable;
                }
                return base.table;
            }
        }

        //读字段信息
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.shop = input.getString("shop");
            this.state = input.getString("state");
            this.total = input.getString("total");
            this.pingppId = input.getString("pingppId");
            this.iswithdrawn = input.getBoolean("iswithdrawn");
            this.remark = input.getString("remark");
            this.channel = input.getString("channel");
        }

        //写字段信息
        public override void write(BmobOutput output, bool all)
        {
            base.write(output, all);

            output.Put("state", this.state);
            output.Put("pingppId", this.pingppId);
            output.Put("shop", this.shop);
            output.Put("total", this.total);
            output.Put("remark",this.remark);
            output.Put("channel",this.channel);
        }

    }
}
