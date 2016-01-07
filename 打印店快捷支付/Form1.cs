using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using pingpp;
using pingpp.Models;
using pingpp.Exception;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using ThoughtWorks.QRCode.Codec;
using System.Threading;
using cn.bmob.api;
using cn.bmob.io;
using cn.bmob.json;
using cn.bmob.tools;
using System.Diagnostics;
using 打印店快捷支付.BmobModel;
using System.Media;

namespace 打印店快捷支付
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> app = new Dictionary<string, string>();
        QRCodeEncoder QRE;
        string shop;
        private BmobWindows bmob;
        public Form1()
        {
            InitializeComponent();
            QRE = new QRCodeEncoder();
            QRE.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            QRE.QRCodeScale = 11;
            QRE.QRCodeVersion = 4;
            QRE.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;

            //设置 apikey
            Pingpp.apiKey = "sk_live_bOz9YlaOHrS7dFw9yYlUif7R";
            //设置 appid
            app.Add("id", "app_SO0anHPWznHCbL0y");

            shop = "东篱阳光图文";
            bmob = new BmobWindows();
            //初始化ApplicationId，这个ApplicationId需要更改为你自己的ApplicationId（ http://www.bmob.cn 上注册登录之后，创建应用可获取到ApplicationId）
            bmob.initialize("4c69d6ed89b6591d050f809d70930b2a", "bce9f6f7da21f79dd7f3d77c3b84d335");
            BmobDebug.Register(msg => { Debug.WriteLine(msg); });
            this.FormClosing += Form1_FormClosing1;

            //backgroundworker
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.WorkerReportsProgress = true;
        }

        private void Form1_FormClosing1(object sender, FormClosingEventArgs e)
        {

        }

        public BmobWindows Bmob
        {
            get { return bmob; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
        }

        public static Charge CreateCharge(Dictionary<String, Object> param)
        {
            //发起 charge 请求获取支付凭据
            Charge charge = Charge.create(param);
            return charge;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            if (textBox1.Text.Length > 0 )
            {
                
                float amount = float.Parse(textBox1.Text) * 100;
                label2.Text = "微信收款   金额：" + textBox1.Text + "元";
                label3.Visible = true;
                try
                {
                    //交易请求参数，
                    Dictionary<string, object> chargeParam = new Dictionary<string, object>();
                    Dictionary<String, Object> extra = new Dictionary<String, Object>();
                    extra.Add("product_id", "print");
                    Random r = new Random();
                    chargeParam.Add("order_no", DateTime.Now.ToFileTime().ToString());
                    chargeParam.Add("amount", (int)amount);
                    chargeParam.Add("extra", extra);
                    chargeParam.Add("channel", "wx_pub_qr");
                    chargeParam.Add("currency", "cny");
                    chargeParam.Add("subject", "打印店快速收款");
                    chargeParam.Add("body", "body");
                    chargeParam.Add("client_ip", "127.0.0.1");
                    //将 appid 添加到发起交易的请求参数中
                    chargeParam.Add("app", app);
                    Charge charge = CreateCharge(chargeParam);
                    Console.WriteLine("****发起交易请求创建 charge 对象****");
                    Console.WriteLine(charge.Credential);
                    wx_credential wx_qr = JsonConvert.DeserializeObject<wx_credential>(charge.Credential.ToString());
                    Console.WriteLine(wx_qr.wx_pub_qr);

                    //保存消息到bmob
                    BmobQuickChargeObject quickCharge = new BmobQuickChargeObject();
                    quickCharge.pingppId = charge.Id;
                    quickCharge.shop = shop;
                    quickCharge.state = "UNPAID";
                    quickCharge.total = amount.ToString();
                    quickCharge.iswithdrawn = false;
                    quickCharge.remark = textBox2.Text;
                    quickCharge.channel = "微信";
                    var future = Bmob.CreateTaskAsync(quickCharge);

                    label3.Visible = false;
                    pictureBox1.Image = QRE.Encode(wx_qr.wx_pub_qr);
                    pictureBox1.Visible = true;

                    //先停止backgroundworker
                    if (backgroundWorker1.IsBusy == true)
                    {
                        backgroundWorker1.CancelAsync();
                        Thread t = new Thread(new ThreadStart(delegate
                        {
                            Thread.Sleep(10000);
                            backgroundWorker1.RunWorkerAsync(charge.Id);
                            
                        }));
                        t.Start();
                    }
                    else
                    {
                        backgroundWorker1.RunWorkerAsync(charge.Id);
                        
                    }
                    
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee);

                }
            }

            button1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("http://www.baidu.com");
            pictureBox1.Image = null;
            if (textBox1.Text.Length > 0)
            {

                float amount = float.Parse(textBox1.Text) * 100;
                label2.Text = "支付宝网页收款   金额：" + textBox1.Text + "元";
                try
                {
                    //交易请求参数，
                    Dictionary<string, object> chargeParam = new Dictionary<string, object>();
                    Dictionary<String, Object> extra = new Dictionary<String, Object>();
                    extra.Add("success_url", "http://www.99dayin.com");
                    Random r = new Random();
                    chargeParam.Add("order_no", DateTime.Now.ToFileTime().ToString());
                    chargeParam.Add("amount", (int)amount);
                    chargeParam.Add("extra", extra);
                    chargeParam.Add("channel", "alipay_pc_direct");
                    chargeParam.Add("currency", "cny");
                    chargeParam.Add("subject", "打印店快速收款");
                    chargeParam.Add("body", "body");
                    chargeParam.Add("client_ip", "127.0.0.1");
                    //将 appid 添加到发起交易的请求参数中
                    chargeParam.Add("app", app);
                    Charge charge = CreateCharge(chargeParam);
                    Console.WriteLine("****发起交易请求创建 charge 对象****");
                    //Console.WriteLine(charge.Credential);
                    ali_credential ali = JsonConvert.DeserializeObject<ali_credential>(charge.Credential.ToString());
                    alipay_pc_direct_data ali_pc = JsonConvert.DeserializeObject<alipay_pc_direct_data>(ali.alipay_pc_direct.ToString());
                    label3.Visible = false;
                    Console.WriteLine(charge.Credential.ToString());

                    //保存消息到bmob
                    BmobQuickChargeObject quickCharge = new BmobQuickChargeObject();
                    quickCharge.pingppId = charge.Id;
                    quickCharge.shop = shop;
                    quickCharge.state = "UNPAID";
                    quickCharge.total = amount.ToString();
                    quickCharge.iswithdrawn = false;
                    quickCharge.remark = textBox2.Text;
                    quickCharge.channel = "支付宝";
                    var future = Bmob.CreateTaskAsync(quickCharge);
                    

                    //写入html
                    string path = System.Environment.CurrentDirectory;
                    string html = "<form id='alipaysubmit' name='alipaysubmit' action='https://mapi.alipay.com/gateway.do?_input_charset=utf-8' method='post'>";
                    html += "<input type='hidden' name='service' value='" + ali_pc.service + "'/><br/>";
                    html += "<input type='hidden' name='_input_charset' value='" + ali_pc._input_charset + "'/><br/>";
                    html += "<input type='hidden' name='return_url' value='" + ali_pc.return_url + "'/><br/>";
                    html += "<input type='hidden' name='notify_url' value='" + ali_pc.notify_url + "'/><br/>";
                    html += "<input type='hidden' name='partner' value='" + ali_pc.partner + "'/><br/>";
                    html += "<input type='hidden' name='out_trade_no' value='" + ali_pc.out_trade_no + "'/><br/>";
                    html += "<input type='hidden' name='subject' value='" + ali_pc.subject + "'/><br/>";
                    html += "<input type='hidden' name='body' value='" + ali_pc.body + "'/><br/>";
                    html += "<input type='hidden' name='total_fee' value='" + ali_pc.total_fee + "'/><br/>";
                    html += "<input type='hidden' name='payment_type' value='" + ali_pc.payment_type + "'/><br/>";
                    html += "<input type='hidden' name='seller_id' value='" + ali_pc.seller_id + "'/><br/>";
                    html += "<input type='hidden' name='it_b_pay' value='" + ali_pc.it_b_pay + "'/><br/>";
                    html += "<input type='hidden' name='sign' value='" + ali_pc.sign + "'/><br/>";
                    html += "<input type='hidden' name='sign_type' value='" + ali_pc.sign_type + "'/><br/>";
                    html += "<form/>";
                    html += "<script>document.forms['alipaysubmit'].submit();</script>";
                    string filename = path + "\\" + ali_pc.out_trade_no + "ali.out_trade_no.html";
                    StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.GetEncoding("utf-8"));
                    sw.Write(html);
                    sw.Close();
                    sw.Dispose();



                    //打开html文件
                    System.Diagnostics.Process.Start(filename);
                    //删除html文件
                    Thread th = new Thread(new ThreadStart(delegate
                    {
                        Thread.Sleep(20000);
                        File.Delete(filename);
                    }));


                    //先停止backgroundworker
                    if (backgroundWorker1.IsBusy == true)
                    {
                        backgroundWorker1.CancelAsync();
                        Thread t = new Thread(new ThreadStart(delegate
                        {
                            Thread.Sleep(10000);
                            backgroundWorker1.RunWorkerAsync(charge.Id);

                        }));
                        t.Start();
                    }
                    else
                    {
                        backgroundWorker1.RunWorkerAsync(charge.Id);

                    }
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee);

                }
                
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;
            //小数点的处理。
            if ((int)e.KeyChar == 46)                           //小数点
            {
                if (textBox1.Text.Length <= 0)
                    e.Handled = true;   //小数点不能在第一位
                else                                             //处理不规则的小数点
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse(textBox1.Text, out oldf);
                    b2 = float.TryParse(textBox1.Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2(shop);
            f2.Show();
        }

        public delegate void SetPaidCallback();
        public void paidCallback()
        {
            if (pictureBox1.InvokeRequired)
            {
                SetPaidCallback pc = new SetPaidCallback(paidCallback);
                this.BeginInvoke(pc);
            }
            else
            {
                pictureBox1.Visible = false;
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //resultLabel.Text = "Canceled!";
            }
            else
            {
                string path1 = System.Environment.CurrentDirectory;
                SoundPlayer p = new SoundPlayer(path1 + "\\new.wav");
                p.Play();
                paidCallback();
                MessageBox.Show("付款成功！", "系统提示！");
                
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string chargeId = e.Argument as string;
            bool isPaid = false;
            while (!isPaid)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    Charge ch = Charge.retrieve(chargeId);
                    isPaid = ch.Paid;
                    backgroundWorker1.ReportProgress(0);
                    Console.WriteLine(chargeId);
                    Thread.Sleep(1000);
                }

            }
            if (isPaid)
            {
                backgroundWorker1.ReportProgress(100);
            }
            else
            {
                e.Cancel = true;
            }
            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.BeginInvoke();
        }

    }
}

class wx_credential
{
    public string wx_pub_qr { set; get; }
}

class ali_credential {

    public object alipay_pc_direct { set; get; }
}
class alipay_pc_direct_data
{
    public string service { set; get; }
    public string _input_charset { set; get; }
    public string return_url { set; get; }
    public string notify_url { set; get; }
    public string partner { set; get; }
    public string out_trade_no { set; get; }
    public string subject { set; get; }
    public string body { set; get; }
    public string total_fee { set; get; }
    public string payment_type { set; get; }
    public string seller_id { set; get; }
    public string it_b_pay { set; get; }
    public string sign { set; get; }
    public string sign_type { set; get; }
}