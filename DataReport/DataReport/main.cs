using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fiddler;
using System.Windows.Forms;
using DataReport;
using DataReport.util;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Web;
using System.Data;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

[assembly: Fiddler.RequiredVersion("2.4.9.7")]
public class main : IAutoTamper
{
    
    private TabPage oPage;
    private UserControlUII ui = null;
    private List<Fiddler.Session> oSessionList;

    private DeviceType dt;//设备类型
    private DataTable DT;//数据表
    private DataTable DTnew;//4501/4497的包头数据
    private DataTable DTnew2;//4501/4497的data数据

    //正则表达，筛选字段
    /*
    private const string pattern = @"val(\d*)=(\d+)";//匹配val
    private const string pattern1 = @"bi=(\d+)";//匹配bi
    private const string pattern2 = @"bt=(\d+)";//匹配bt
    private const string pattern3 = @"vid=([\d\w]+)";//匹配vid
    private const string pattern4 = @"vt=(\d+)";//匹配vt
    */
    //private string[] PATTERN = { @"\Wval(\d*)=(\d+)", @"\Wbi=(\d+)", @"\Wbt=(\d+)", @"\Wvid=([\d\w]+)", @"\Wvt=(\d+)" };
    private string[] PATTERN = { @"([\W]{0,1})val=([^&]*|$)", @"([\W]{0,1})val1=([^&]*|$)", @"([\W]{0,1})val2=([^&]*|$)", @"([\W]{0,1})bi=([^&]*|$)", @"([\W]{0,1})bt=([^&]*|$)", @"([\W]{0,1})vid=([\d\w]+)", @"([\W]{0,1})vt=([^&]*|$)" };
    private int N = 7;//PATTERN的长度

    //private string[] newPATTERN = { @"\Wclip=(\d*)", @"\Wdltype=(\d*)", @"\Wduration=([\d\.]*)", @"\Wfmt=(\d*)", @"\Wplatform=(\d*)", @"\Wrate=(\d*)", @"\Wstatus=(\d*)", @"\Wtestid=(\d*)", @"\Wtype=(\d*)", @"\Wuip=([\d\.\%]*)", @"\Wvid=([\d\w]*)" };
    private string[] newPATTERN = { @"([\W]{0,1})clip=([^&]*)", @"([\W]{0,1})dltype=([^&]*)", @"([\W]{0,1})duration=([^&]*)", @"([\W]{0,1})fmt=([^&]*)", @"([\W]{0,1})platform=([^&]*)", @"([\W]{0,1})rate=([^&]*)", @"([\W]{0,1})status=([^&]*)", @"([\W]{0,1})testid=([^&]*)", @"([\W]{0,1})type=([^&]*)", @"([\W]{0,1})uip=([^&]*)", @"([\W]{0,1})vid=([^&]*)" };
    private string[] stepPATTERN = { @"([\W]{0,1})step=([\d]+\$)*0(\W|$)", @"([\W]{0,1})step=([\d]+\$)*1(\W|$)", @"([\W]{0,1})step=([\d]+\$)*2(\W|$)", @"([\W]{0,1})step=([\d]+\$)*3(\W|$)", @"([\W]{0,1})step=([\d]+\$)*4(\W|$)", @"([\W]{0,1})step=([\d]+\$)*5(\W|$)", @"([\W]{0,1})step=([\d]+\$)*6(\W|$)", @"([\W]{0,1})step=([\d]+\$)*7(\W|$)" };
    public main()
    {

    }
    public void OnLoad()
    {
        /* 在这编写加载插件是需要执行的code 如加载UI */
        oPage = new TabPage("DataReport");//建立属于此插件的Tab页
        //======init 

        ui = new UserControlUII();
        oPage.Controls.Add(ui);
        ui.Dock = DockStyle.Fill;
        oSessionList = new List<Session>();     
        dt = ui.getDeviceType();

        /*2865的初始化数据表table*/
        DT = new DataTable();
        DT.Columns.Add(new DataColumn("STEP", typeof(string)));
        DT.Columns.Add(new DataColumn("val", typeof(string)));
        DT.Columns.Add(new DataColumn("val1", typeof(string)));
        DT.Columns.Add(new DataColumn("val2", typeof(string)));
        DT.Columns.Add(new DataColumn("bi", typeof(string)));
        DT.Columns.Add(new DataColumn("bt", typeof(string)));
        DT.Columns.Add(new DataColumn("vid",typeof(string)));
        DT.Columns.Add(new DataColumn("vt", typeof(string)));
        DT.Columns.Add(new DataColumn("Session-id", typeof(int)));
        ui.ReportdataGridView.DataSource = DT; 

        /*4501/4497的上报包头数据*/
        DTnew = new DataTable();
        DTnew.Columns.Add(new DataColumn("参数", typeof(string)));
        DTnew.Columns.Add(new DataColumn("getinfo之前", typeof(string)));
        DTnew.Columns.Add(new DataColumn("getinfo", typeof(string)));
        DTnew.Columns.Add(new DataColumn("getinfo之后", typeof(string)));
        DTnew.Columns.Add(new DataColumn("字段含义", typeof(string)));
        ui.newReportdataGridView.DataSource = DTnew;
        
        DataRow dr;
        string[] parameters = { "clip", "dltype", "duration", "fmt", "platform", "rate", "status", "testid", "type", "uip", "vid" };
        string[] describle = { "视频分片数", "下载类型", "视频本身时长(单位:s)", "首次加载对应的视频格式", "平台号", "首次加载对应的音视频码率(单位:kByte/s) ", "视频本身状态", "ABTest测试分组id", "视频类型", "用户IP: ipv4, ipv6", "视频ID" };
        for (int i = 0; i < parameters.Length; i++)
        {
            dr = DTnew.NewRow();
            dr["参数"] = parameters[i];
            dr["字段含义"] = describle[i];
            DTnew.Rows.Add(dr);
        }
        /*4501/4497的上报数据data*/
        DTnew2 = new DataTable();
        DTnew2.Columns.Add(new DataColumn("step",typeof(string )));
        DTnew2.Columns.Add(new DataColumn("seq",typeof(string)));
        DTnew2.Columns.Add(new DataColumn("flowid",typeof(string)));
        DTnew2.Columns.Add(new DataColumn ("data",typeof(string)));
        DTnew2.Columns.Add(new DataColumn("Session-id", typeof(int)));
        ui.newReportdataGridView2.DataSource = DTnew2;

        
        
        FiddlerApplication.UI.tabsViews.TabPages.Add(oPage);
    }

    public void OnBeforeUnload()
    {
        /*编写在unload插件之前所要执行的code*/

    }
    public void AutoTamperRequestBefore(Session oSession)
    {
        /*在这编写请求之前需要执行的code */
        /*在这编写响应之前需要执行的code */

    }
    public void AutoTamperRequestAfter(Session oSession)
    {
        /*在这编写请求之后需要执行的code */

    }
    public void AutoTamperResponseBefore(Session oSession)
    {
        
        /*在这编写响应之前需要执行的code */
        if (ui.ReportEnabled.Checked)
        {
            DeviceType dt = ui.getDeviceType();
            switch (dt)
            {
                case DeviceType.MobileH5:
                    {
                        checkMobileH5Pre(oSession);
                        break;
                    }

                case DeviceType.flash:
                    {
                        checkPcFlashPre(oSession);
                        break;
                    }
                case DeviceType.PCH5:
                    {
                        checkPCH5Pre(oSession);
                        break;
                    }
                default:
                    break;

            }
        }

    }
    public void AutoTamperResponseAfter(Session oSession)
    {
      
        //选择代理什么域名
        bool flagSwitch = false;
        if (ui.vi.Checked && (!ui.vd.Checked))
        {
            flagSwitch = oSession.HostnameIs("vi.l.qq.com");
        }
        if ((!ui.vi.Checked) && ui.vd.Checked)
        {
            flagSwitch = oSession.HostnameIs("vd.l.qq.com");
        }
        if(ui.vi.Checked && ui.vd.Checked)
        {
            flagSwitch = oSession.HostnameIs("vi.l.qq.com") || oSession.HostnameIs("vd.l.qq.com");
        }
        //
        if (flagSwitch && (oSession.uriContains("proxyhttp"))&&oSession.HTTPMethodIs("POST"))
        {
            string adadress = "";
            string videoadress = "";
            string keyadress = "";
            string infoadress = "";
            if (oSession.GetRequestBodyAsString().IndexOf("ad_type=LD") != -1)
            {
                adadress = ui.adlist.Text.ToString();
                videoadress = ui.videolist.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("ad_type=ZC") != -1)
            {
                adadress = ui.ZC.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("ad_type=HT") != -1)
            {
                adadress = ui.HT.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("ad_type=PSJ") != -1)
            {
                adadress = ui.PSJ.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("ad_type=ZT") != -1)
            {
                adadress = ui.ZT.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("ad_type=ZI") != -1)
            {
                adadress = ui.ZI.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("ad_type=PPB") != -1)
            {
                adadress = ui.PPB.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("ad_type=PDF") != -1)
            {
                adadress = ui.PDF.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("onlyvkey") != -1)
            {
                keyadress = ui.getkey.Text.ToString();
            }
            else if (oSession.GetRequestBodyAsString().IndexOf("onlyvinfo") != -1)
            {
                infoadress = ui.getinfo.Text.ToString();
 
            }
            else
            {
                adadress = "";
                videoadress = "";
                keyadress = "";
                infoadress = "";
            }

            string videooBody = "";
            string adoBody = "";
            string keyBody = "";
            string infoBody = "";

            if (adadress != "" && (adadress != "remove") && (adadress != "none"))
            {
                adoBody = File.ReadAllText(adadress);
            }
            if (videoadress != "" && (videoadress != "remove") && (videoadress != "none"))
            {
                videooBody = File.ReadAllText(videoadress);
            }
            if (keyadress != "" && (keyadress != "remove")&&(keyadress!="none"))
            {
                keyBody = File.ReadAllText(keyadress);
            }
            if (infoadress != "" && (infoadress != "remove") && (infoadress != "none"))
            {
                infoBody = File.ReadAllText(infoadress);
            }
            if (oSession.oResponse.headers.HTTPResponseCode == 200 )
            {
                string allBody = oSession.GetResponseBodyAsString();
                JObject responseObject = (JObject)JsonConvert.DeserializeObject(allBody);//转换成json格式
                if (responseObject.Property("vinfo") != null && (videoadress != "") && (videoadress != "remove") && (videoadress != "none"))
                {
                    responseObject["vinfo"] = videooBody;
                }
                if (responseObject.Property("ad") != null && (adadress != "") && (adadress != "remove")&&(adadress!="none"))
                {
                    responseObject["ad"] = adoBody;
                }
                if (responseObject.Property("vkey") != null && (keyadress != "") && (keyadress != "remove")&&(keyadress!="none"))
                {
                    responseObject["vkey"] = keyBody;
                }
                if (responseObject.Property("vinfo") != null && (infoadress != "") && (infoadress != "remove") && (infoadress != "none"))
                {
                    responseObject["vinfo"] = infoBody;
                }


                if (responseObject.Property("vinfo") != null && (videoadress != "") && (videoadress == "remove"))
                {
                    responseObject.Property("vinfo").Remove();

                }
                if (responseObject.Property("vinfo") != null && (videoadress != "") && (videoadress == "none"))
                {
                    responseObject.Property("vinfo").Remove();
                    if (responseObject.Property("errCode") != null)
                        responseObject.Property("errCode").Remove();
                }


                if (responseObject.Property("ad") != null && (adadress != "") && (adadress == "remove"))
                {
                    responseObject.Property("ad").Remove();
                }
                if (responseObject.Property("ad") != null && (adadress != "") && (adadress == "none"))
                {
                    responseObject.Property("ad").Remove();
                    if (responseObject.Property("errCode") != null)
                        responseObject.Property("errCode").Remove();
                }

                
                if (responseObject.Property("vkey") != null && (keyadress != "") && (keyadress == "remove"))
                {
                    responseObject.Property("vkey").Remove();
                }
                if (responseObject.Property("vkey") != null && (keyadress != "") && (keyadress == "none"))
                {
                    responseObject.Property("vkey").Remove();
                    if (responseObject.Property("errCode") != null)
                        responseObject.Property("errCode").Remove();
 
                }

                if (responseObject.Property("vinfo") != null && (infoadress != "") && (infoadress == "remove"))
                {
                    responseObject.Property("vinfo").Remove();
                }
                if (responseObject.Property("vinfo") != null && (infoadress != "") && (infoadress == "none"))
                {
                    responseObject.Property("vinfo").Remove();
                    if (responseObject.Property("errCode") != null)
                        responseObject.Property("errCode").Remove();

                }

                string newres = responseObject.ToString();
                oSession.utilReplaceInResponse(allBody, newres);
            }

        }
        
        
        /*在这编写响应之后需要执行的code */

        if (ui.ReportEnabled.Checked)
        {
            DeviceType dt = ui.getDeviceType();

            switch (dt)
            {
                case DeviceType.MobileH5:
                    {
                        checkMobileH5Post(oSession);
                        break;
                    }
                    
                case DeviceType.flash:
                    {
                        checkPcFlashPost(oSession);
                        break;
                    }
                case DeviceType.PCH5:
                    {
                        checkPCH5Post(oSession);
                        break;
                    }
                default:
                    break;
            }
        }
    }

    public void OnBeforeReturningError(Session oSession)
    {
        /*在这编写有错误返回时需要执行的code */
    }

    bool changead(Session oSession, string ads)
    {
        byte[] jsonByte = oSession.ResponseBody;//获取的是已byte为单位的字符串
        //string jsonString = System.Text.Encoding.Default.GetString(jsonByte);
        string jsonString = System.Text.Encoding.UTF8.GetString(jsonByte);//此处直接用Dafault会出现中文乱码，要用UTF8编码

        JObject responseObject = (JObject)JsonConvert.DeserializeObject(jsonString);//转换成json格式 
        //responseObject["ad"] = ads;
        //将getinfo的字段值提取出来
        oSession.utilReplaceInResponse(responseObject["ad"].ToString(), ads);
        return true;
 
    }
    private bool isChar(string str)//判断是否是字符
    {
        if (str.Length > 0 && ((str[0] > 'a') && ((str[0] < 'z')) || ((str[0] > 'A') && (str[0] < 'Z'))))
        {
            return true;
        }
        return false;
 
    }
    private bool [] getDr( DataRow dr, string url, string []pattern, int N)
    {/*检查字段*/

        bool []valFlag=new bool[7];
        for (int i = 0; i < 7; i++)
        {
            valFlag[i] = false;
        }
        for (int j = 0; j < N;j++ )
        {
            Match mc = Regex.Match(url, pattern[j]);
            if (mc.Success)
            {
                //MessageBox.Show(mc[i].Value);
                string[] s = mc.Value.Split('=');
                //保证字段是在首位（如step=4）和字段在中间(如&step=4)的情况可以正常展示
                if ((isChar(s[0]) && s[0].Substring(0) == "val") || (!isChar(s[0]) && s[0].Substring(1) == "val"))
                {
                    //ui.ReportdataGridView.Rows[1].Cells[1].Value = s[1];
                    dr["val"] = System.Web.HttpUtility.UrlDecode(s[1]);

                    valFlag[0] = true;
                }
                else if ((isChar(s[0]) && s[0].Substring(0) == "val1") || (!isChar(s[0]) && s[0].Substring(1) == "val1"))
                {
                    //ui.ReportdataGridView.Rows[1].Cells[2].Value = s[1];
                    dr["val1"] = System.Web.HttpUtility.UrlDecode(s[1]);
                    valFlag[1] = true;
                }
                else if ((isChar(s[0]) && s[0].Substring(0) == "val2") || (!isChar(s[0]) && s[0].Substring(1) == "val2"))
                {
                    //ui.ReportdataGridView.Rows[1].Cells[3].Value = s[1];
                    dr["val2"] = System.Web.HttpUtility.UrlDecode(s[1]);
                    valFlag[2] = true;
                }
                else if ((isChar(s[0]) && s[0].Substring(0) == "bi") || (!isChar(s[0]) && s[0].Substring(1) == "bi"))
                {
                    dr["bi"] = System.Web.HttpUtility.UrlDecode(s[1]);
                    valFlag[3] = true;
                }
                else if ((isChar(s[0]) && s[0].Substring(0) == "bt") || (!isChar(s[0]) && s[0].Substring(1) == "bt"))
                {
                    dr["bt"] = System.Web.HttpUtility.UrlDecode(s[1]);
                    valFlag[4] = true;
                }
                else if ((isChar(s[0]) && s[0].Substring(0) == "vid") || (!isChar(s[0]) && s[0].Substring(1) == "vid"))
                {
                    dr["vid"] = System.Web.HttpUtility.UrlDecode(s[1]);
                    // MessageBox.Show(s[1]);
                    valFlag[5] = true;
                }
                else if ((isChar(s[0]) && s[0].Substring(0) == "vt") || (!isChar(s[0]) && s[0].Substring(1) == "vt"))
                {
                    dr["vt"] = System.Web.HttpUtility.UrlDecode(s[1]);
                    valFlag[6] = true;
                }
                continue;
 
            }
              
           }

                
        DT.Rows.Add(dr);
        return valFlag;
        
    }
    private void getData(DataRow dr, string url)
    {
      
        /*将data提取出来*/
        string patterndata = @"([\W]{0,1})data=([^&]*)";
        string patternseq = @"([\W]{0,1})seq=(\d+)";
        string patternflowid = @"([\W]{0,1})flowid=([^&]*)";
        string flowid = "";
        string seq = "";
        string data = "";

        Match m1 = Regex.Match(url, patternseq);
        if (m1.Success)
        {
            string[] s = m1.Value.Split('=');
            seq = s[1];
        }

        Match m2 = Regex.Match(url, patternflowid);
        if (m2.Success)
        {
            string[] s = m2.Value.Split('=');
            flowid = s[1];
        }

        Match m3 = Regex.Match(url, patterndata);
        if (m3.Success)
        {
            string[] s = m3.Value.Split('=');
            data = s[1];
        }
        data = System.Web.HttpUtility.UrlDecode(data);//不先将url解码是防止在data中也含有url时，data会在中途被截断
        StringBuilder str = new StringBuilder();
        if (data != "" && data[0] == '{')//表示是JObject
        {
            JObject dataObject = (JObject)JsonConvert.DeserializeObject(data);//转换成json格式
            foreach (var item in dataObject)
            {
                str.Append(item.Key + ":" + item.Value + " ");//加一个空格，有利于datagridview中自动换行
            }
        }
        if (data != "" && data[0] == '[')//表示是JArray
        {
            JArray Jdata = JArray.Parse(data);
            foreach (JObject items in Jdata)
            {
                str.Append("{");
                foreach (var item in items)
                {
                    str.Append(item.Key + ":" + item.Value + " ");
                }
                str.Append("}");
            }
        }
        dr["data"] = str.ToString();
        dr["seq"] = seq;
        dr["flowid"] = flowid;
        DTnew2.Rows.Add(dr);
    }
    private void getHeadPost(string url, string[] newPATTERN, int step)
    {
        for (int i = 0; i < newPATTERN.Length; i++)
        {
            Match m = Regex.Match(url, newPATTERN[i]);
            if (m.Success)
            {
                string[] s = m.Value.Split('=');
                string value = System.Web.HttpUtility.UrlDecode(s[1]);
                if (ui.newReportdataGridView.Rows[i].Cells[3].Value.ToString() == "")
                {
                    ui.newReportdataGridView.Rows[i].Cells[3].Value = (value == "" ? "null" : value) + "(" + step + ") ";
                }
                else
                {
                    ui.newReportdataGridView.Rows[i].Cells[3].Value = ui.newReportdataGridView.Rows[i].Cells[3].Value + "+" + (value == "" ? "null" : value) + "(" + step + ") ";
                }               
            }
            
        }
 
    }
    private void getHeadPre(string url, string[] newPATTERN, int step)
    {
        for (int i = 0; i < newPATTERN.Length; i++)
        {
            Match m = Regex.Match(url, newPATTERN[i]);
            if (m.Success)
            {
                string[] s = m.Value.Split('=');
                if (s[1] != "")
                {
                    string value = System.Web.HttpUtility.UrlDecode(s[1]);
                    if (ui.newReportdataGridView.Rows[i].Cells[1].Value.ToString() == "")
                    {
                        ui.newReportdataGridView.Rows[i].Cells[1].Value = value + "(" + step + ") ";
                    }
                    else
                    {
                        ui.newReportdataGridView.Rows[i].Cells[1].Value = ui.newReportdataGridView.Rows[i].Cells[1].Value + "+" + (value == "" ? "null" : value) + "(" + step + ") ";
                    }
 
                }
                
            }
        }

    }
    private void checkMobileH5Pre(Session oSession)
    {

        
            /*广告资源请求*/
            //每次这条请求展示的时候都特别卡（委托）
            if (oSession.HostnameIs(ui.adUrlHost) && oSession.uriContains(ui.adUrl))
            {
                ui.requesFlag[2] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【广告资源】" + oSession.fullUrl);

            }
            /*视频资源请求*/
            if (oSession.HostnameIs(ui.videoHost) && oSession.uriContains(ui.videoUrls))
            {
                ui.requesFlag[8] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【视频资源】" + oSession.fullUrl);
            }

            /*上报*/

            //if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=2865") && oSession.uriContains("step=3&"))
            if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=2865") && (Regex.Match(oSession.fullUrl, stepPATTERN[3]).Success))                
            {
                Match m=Regex.Match(oSession.fullUrl, stepPATTERN[3]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[3] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step="+step +"】"+ oSession.fullUrl);
                //MessageBox.Show("step3");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;

                /*判断上报参数是否缺失*/
                string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                /*初始化*/
                bool[] valFlag = getDr(dr, url, PATTERN, N);
                /*valFlag用于标志上报中是否包含某个参数*/
                //valFlag[0]:val
                //valFlag[1]:val1
                //valFlag[2]:val2
                //valFlag[3]:bi
                //valFlag[4]:bt
                //valFlag[5]:vid
                //valFlag[6]:vt

                /*step=3的上报中必须含有的是：val，vid*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;
                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }

                if (!(valFlag[5]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
                }

            }
            if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=2865") && (Regex.Match(oSession.fullUrl, stepPATTERN[4]).Success))
            {
            
                string Ecode = "";
                /*
                if ((!ui.requesFlag[3] && ui.requesFlag[0]))
                {
                    Ecode = "【ORDER-ERROR】";
                }
                 */
                Match m = Regex.Match(oSession.fullUrl, stepPATTERN[4]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[4] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + Ecode + oSession.fullUrl);
                //MessageBox.Show("step4");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;



                /*判断上报参数是否缺失*/
                string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
                bool[] valFlag = getDr(dr, url, PATTERN, N);

                /*step=4的上报中必须含有的是：val，val1,bi,vid,vt*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;
                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }
                if (!(valFlag[1]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
                }
                if (!(valFlag[3]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
                }
                if (!(valFlag[5]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
                }
                if (!(valFlag[6]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
                }
            }
            if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=2865") && (Regex.Match(oSession.fullUrl, stepPATTERN[5]).Success))
            {

                Match m = Regex.Match(oSession.fullUrl, stepPATTERN[5]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[5] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】"  + oSession.fullUrl);
                //MessageBox.Show("step5");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;


                /*判断上报参数是否缺失*/
                string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
                bool[] valFlag = getDr(dr, url, PATTERN, N);

                /*step=5的上报中必须含有的是：val，val1,bi,bt,vt*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;

                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }
                if (!(valFlag[1]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
                }
                if (!(valFlag[3]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
                }
                if (!(valFlag[4]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[5].Style.BackColor = Color.Red;
                }
                if (!(valFlag[6]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
                }

            }
            if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=2865") && (Regex.Match(oSession.fullUrl, stepPATTERN[6]).Success))
            {
                
                string Ecode = "";
                /*
                if (!(ui.requesFlag[4] && ui.requesFlag[8] && (!ui.requesFlag[2] || ui.requesFlag[7])))
                {
                    Ecode = "【ORDER-ERROR】";
                }
                 */

                Match m = Regex.Match(oSession.fullUrl, stepPATTERN[6]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[6] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + Ecode + oSession.fullUrl);
                //MessageBox.Show("step6");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;

                /*判断上报参数是否缺失*/
                string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
                bool[] valFlag = getDr(dr, url, PATTERN, N);

                /*step=6的上报中必须含有的是：val，val1,bi,vid,vt*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;
                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }
                if (!(valFlag[1]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
                }
                if (!(valFlag[3]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
                }
                if (!(valFlag[5]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
                }
                if (!(valFlag[6]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
                }
            }
            if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=2865") && (Regex.Match(oSession.fullUrl, stepPATTERN[7]).Success))
            {
              
                string Ecode = "";
                /*
                if (!(ui.requesFlag[2] && ui.requesFlag[3]))
                {
                    Ecode = "【ORDER-ERROR】";
                }
                 */
                Match m = Regex.Match(oSession.fullUrl, stepPATTERN[7]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[7] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + Ecode + oSession.fullUrl);
                //MessageBox.Show("step7");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;

                /*判断上报参数是否缺失*/
                string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);

                bool[] valFlag = getDr(dr, url, PATTERN, N);

                /*step=7的上报中必须含有的是：val，val1,bi,vid,vt*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;
                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }
                if (!(valFlag[1]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
                }
                if (!(valFlag[3]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
                }
                if (!(valFlag[5]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
                }
                if (!(valFlag[6]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
                }
            }
        
 
    }
    private void checkMobileH5Post(Session oSession)
    {
        
        //getinfo获取视频资源的地址，移动端H5播放器
            if (oSession.uriContains("getinfo"))
            {
                ui.requesFlag[0] = true;
                oSessionList.Add(oSession);
                //ui.Sessions.Items.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【getinfo】" + oSession.fullUrl);

                byte[] jsonByte = oSession.ResponseBody;//获取的是已byte为单位的字符串
                //string jsonString = System.Text.Encoding.Default.GetString(jsonByte);
                string jsonString = System.Text.Encoding.UTF8.GetString(jsonByte);//此处直接用Dafault会出现中文乱码，要用UTF8编码
                int start = jsonString.IndexOf('(');
                int end = jsonString.LastIndexOf(')');
                string responseStringJson = jsonString.Substring(start + 1, end - start - 1);
               
                JObject responseObject = (JObject)JsonConvert.DeserializeObject(responseStringJson);//转换成json格式 

                string vi = responseObject["vl"]["vi"].ToString();
                JArray Jvi = JArray.Parse(vi);
                JArray Jui = JArray.Parse(((JObject)(Jvi[0]))["ul"]["ui"].ToString());//移动端H5播放器只取第一个cdn地址
                string fn = ((JObject)(Jvi[0]))["fn"].ToString();
                string url = ((JObject)(Jui[0]))["url"].ToString();


                string[] sArray = url.Split('/');
                ui.videoHost = sArray[2];//视频资源的host
                ui.videoUrls = sArray[3] + "/" + fn;//视频资源资源的url
            }
            if (oSession.HostnameIs("livew.l.qq.com") && oSession.uriContains("livemsg") && oSession.uriContains("ad_type=WL"))
            {

                ui.requesFlag[1] = true;
                oSessionList.Add(oSession);
                //ui.Sessions.Items.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【广告请求】" + oSession.fullUrl);
                byte[] responseByte = oSession.ResponseBody;
                string responseString = System.Text.Encoding.Default.GetString(responseByte);

                //判断为json格式(!!!!!!!!!!如何读取json的数据)
                if (responseString != "" && responseString[0] != '<')
                {
                    string responseStringJson = responseString.Split('(')[1].Split(')')[0];//因为广告返回是jsonp格式，会有函数名加（），需要取出纯粹的json数据
                    JObject responseObject = (JObject)JsonConvert.DeserializeObject(responseStringJson);
                    string item = responseObject["adList"]["item"].ToString();
                    JArray Jitem = JArray.Parse(item);
                    foreach (var ad in Jitem)
                    {
                        JArray image = (JArray.Parse(((JObject)ad)["image"].ToString()));
                        string url = ((JObject)image[0])["url"].ToString();
                        if (url != "")
                        {
                            ui.adUrls = url;
                            string[] sArray = ui.adUrls.Split('/');
                            ui.adUrlHost = sArray[2];//广告资源的host
                            ui.adUrl = sArray[3];//广告资源的url
                            //MessageBox.Show(url);
                            break;
                        }
                    }
                }

                //判断为xml格式，移动端H5 V2播放器
                if (responseString != "" && responseString[0] == '<')
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(responseString);

                    string rootNode = "root";
                    string goalElem = "url";

                    XmlNode xn = doc.SelectSingleNode(rootNode);
                    XmlNodeList xnList = xn.ChildNodes;

                    foreach (XmlNode xnl1 in xnList)
                    {
                        if (xnl1.Name == "adList")
                        {
                            foreach (XmlNode xnl2 in xnl1.ChildNodes)
                            {
                                if (xnl2.Name == "item")
                                {
                                    foreach (XmlNode xnl3 in xnl2.ChildNodes)
                                    {
                                        if (xnl3.Name == "image")
                                        {
                                            foreach (XmlNode xnl4 in xnl3.ChildNodes)
                                            {
                                                if (xnl4.Name == goalElem && xnl4.InnerText != "")
                                                {
                                                    ui.adUrls = xnl4.InnerText;
                                                    //ui.Sessions.Items.Add(xnl4.InnerText); 
                                                    string[] sArray = ui.adUrls.Split('/');
                                                    ui.adUrlHost = sArray[2];//广告资源的host
                                                    ui.adUrl = sArray[3];//广告资源的url

                                                    /*输出结果*/
                                                    //ui.Sessions.Items.Add(adUrlHost);
                                                    //ui.Sessions.Items.Add(adUrl);
                                                    //ui.AddResult(adUrlHost);
                                                    //ui.AddResult(adUrl);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                    if (ui.adUrls != "AAAAAAAAAAAAAAA")
                                    {
                                        break;
                                    }

                                }
                            }
                            break;
                        }

                    }
                }

            }
       
    }
    private void checkPcFlashPre(Session oSession)
    {
        /*广告资源请求*/
        //每次这条请求展示的时候都特别卡（委托）
        
        if (oSession.HostnameIs(ui.adUrlHost) && oSession.uriContains(ui.adUrl))
        {
            ui.requesFlag[2] = true;
            oSessionList.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【广告资源】" + oSession.fullUrl);

        }
         
        /*视频资源请求*/
        /*
        if (oSession.HostnameIs(ui.videoHost) && oSession.uriContains(ui.videoUrls))
        {
            ui.requesFlag[8] = true;
            oSessionList.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【视频资源】" + oSession.fullUrl);
        }
         */

        /*上报2577*/
        
        //byte[] jsonByte = oSession.ResponseBody;//获取的是以byte为单位的字符串
        //string jsonString = System.Text.Encoding.UTF8.GetString(jsonByte);//此处直接用Dafault会出现中文乱码，要用UTF8编码
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("kvcollect"))
        {
            //MessageBox.Show("btrace.video.qq.com");
            var requestBody = oSession.GetRequestBodyAsString();
            string request = System.Web.HttpUtility.UrlDecode(requestBody).ToLower();//将request的body解码,并全部小写
            //MessageBox.Show(request);
            
            if ((request.IndexOf("bossid=2577") != -1) && (Regex.Match(request, stepPATTERN[3]).Success))
            {

                Match m = Regex.Match(request, stepPATTERN[3]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[3] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
                //MessageBox.Show("step3");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;


                /*判断上报参数是否缺失*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码

                /*初始化*/
                bool[] valFlag = getDr(dr, requestBody, PATTERN, N);

                /*step=3的上报中必须含有的是：val，vid*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;
                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }

                if (!(valFlag[5]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
                }

            }
            if ((request.IndexOf("bossid=2577") != -1) && (Regex.Match(request, stepPATTERN[4]).Success))
            {

                Match m = Regex.Match(request, stepPATTERN[4]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[4] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】"  + oSession.fullUrl);
                //MessageBox.Show("step4");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;

                /*判断上报参数是否缺失*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
                bool[] valFlag = getDr(dr, requestBody, PATTERN, N);

                /*step=4的上报中必须含有的是：val，val1,bi,vid,vt*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;
                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }
                if (!(valFlag[1]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
                }
                if (!(valFlag[3]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
                }
                if (!(valFlag[5]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
                }
                if (!(valFlag[6]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
                }
            }
            if ((request.IndexOf("bossid=2577") != -1) && (Regex.Match(request, stepPATTERN[5]).Success))
            {

                Match m = Regex.Match(request, stepPATTERN[5]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[5] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
                //MessageBox.Show("step4");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;

                /*判断上报参数是否缺失*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
                bool[] valFlag = getDr(dr, requestBody, PATTERN, N);

                /*step=5的上报中必须含有的是：val，val1,bi,bt,vt*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;

                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }
                if (!(valFlag[1]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
                }
                if (!(valFlag[3]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
                }
                if (!(valFlag[4]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[5].Style.BackColor = Color.Red;
                }
                if (!(valFlag[6]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
                }

            }
            if ((request.IndexOf("bossid=2577") != -1) && (Regex.Match(request, stepPATTERN[6]).Success))
            {

                Match m = Regex.Match(request, stepPATTERN[6]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[6] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
                //MessageBox.Show("step4");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;

                /*判断上报参数是否缺失*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
                bool[] valFlag = getDr(dr, requestBody, PATTERN, N);

                /*step=6的上报中必须含有的是：val，val1,bi,vid,vt*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;
                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }
                if (!(valFlag[1]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
                }
                if (!(valFlag[3]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
                }
                if (!(valFlag[5]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
                }
                if (!(valFlag[6]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
                }
            }
            if ((request.IndexOf("bossid=2577") != -1) && (Regex.Match(request, stepPATTERN[7]).Success))
            {

                Match m = Regex.Match(request, stepPATTERN[7]);
                string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
                ui.requesFlag[7] = true;
                oSessionList.Add(oSession);
                ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
                //MessageBox.Show("step4");
                /*动态增加行数显示上报的参数*/
                DataRow dr;
                dr = DT.NewRow();
                dr["STEP"] = step;
                dr["Session-id"] = oSession.id;

                /*判断上报参数是否缺失*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);

                bool[] valFlag = getDr(dr, requestBody, PATTERN, N);

                /*step=7的上报中必须含有的是：val，val1,bi,vid,vt*/
                int curRow = ui.ReportdataGridView.Rows.Count - 2;
                if (!(valFlag[0]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
                }
                if (!(valFlag[1]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
                }
                if (!(valFlag[3]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
                }
                if (!(valFlag[5]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
                }
                if (!(valFlag[6]))
                {
                    ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
                }
            }

            /*上报4497*/
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=0&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step0";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPre(url, newPATTERN, 0);


            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=5&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step5";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPre(url, newPATTERN, 5);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=10&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step10";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPre(url, newPATTERN, 10);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=15&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step15";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 15);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=20&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step20";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 20);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=25&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step25";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 25);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=30&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step30";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 30);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=31&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step31";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 31);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=35&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step35";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 35);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=40&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step40";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = request;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 40);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=45&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step45";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 45);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=48&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step48";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 48);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=49&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step49";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 49);
            }
            if ((request.IndexOf("bossid=4497") != -1) && (request.IndexOf("step=50&") != -1))
            {
                DataRow dr;
                dr = DTnew2.NewRow();
                dr["STEP"] = "step50";
                dr["Session-id"] = oSession.id;

                /*将data提取出来*/
                //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
                string url = requestBody;
                getData(dr, url);
                getHeadPost(url, newPATTERN, 50);
            }
 
        }


        
    }
    private void checkPcFlashPost(Session oSession)
    {

        //getvinfo获取视频资源的地址
        if (oSession.uriContains("getvinfo"))
        {
            ui.requesFlag[0] = true;
            oSessionList.Add(oSession);
            //ui.Sessions.Items.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【gevtinfo】" + oSession.fullUrl);

            /*getvinfo的request*/
            var requestBody = oSession.GetRequestBodyAsString();
            string request = System.Web.HttpUtility.UrlDecode(requestBody).ToLower();//将request的body解码,并全部小写
            string patternvid = @"\Wvid=([\d\w]*)";
            string patternplatform = @"\Wplatform=(\d*)";
            Match m = Regex.Match(request, patternvid);
            if (m.Success)
            {
                string[] s = m.Value.Split('=');
                ui.vid = s[1];
            }
            m = Regex.Match(request, patternplatform);
            if (m.Success)
            {
                string[] s = m.Value.Split('=');
                ui.platform = s[1];

            }
            
            /*getvinfo的response*/
            byte[] responseByte = oSession.ResponseBody;
            string responseString = System.Text.Encoding.UTF8.GetString(responseByte);

            //提取getvinfo中的字段（XML格式）
            if (responseString != "" && responseString[0] == '<')
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseString);

                string rootNode = "root";
                //string goalElem = "url";

                XmlNode xn = doc.SelectSingleNode(rootNode);
                XmlNodeList xnList = xn.ChildNodes;

                foreach (XmlNode xnl1 in xnList)
                {
                    if (xnl1.Name == "dltype")
                    {
                        ui.dltype = xnl1.InnerText;
                    }
                    if (xnl1.Name == "ip")
                    {
                        ui.uip = xnl1.InnerText;
                    }
                    if (xnl1.Name == "tstid")
                    {
                        ui.testid = xnl1.InnerText;
                    }
                    if (xnl1.Name == "fl")
                    {
                        foreach (XmlNode xnl2 in xnl1.ChildNodes)
                        {
                            if (xnl2.Name == "fi")
                            {
                                foreach (XmlNode xnl3 in xnl2.ChildNodes)
                                {
                                    if (xnl3.Name == "id")
                                    {
                                        ui.fmt = xnl3.InnerText;
                                        break;
                                    }
                                        
                                }
                                break;
                            }
 
                        }
                    }
                    if (xnl1.Name == "vl")
                    {
                        foreach (XmlNode xnl2 in xnl1.ChildNodes)
                        {
                            if (xnl2.Name == "vi")
                            {
                                foreach (XmlNode xnl3 in xnl2.ChildNodes)
                                {
                                    if (xnl3.Name == "br")
                                        ui.rate = xnl3.InnerText;
                                    if (xnl3.Name == "cl")//flash中没有cl节点
                                    { }
                                    if (xnl3.Name == "vst")
                                        ui.status = xnl3.InnerText;
                                    if (xnl3.Name == "type")
                                        ui.type = xnl3.InnerText;
                                    if (xnl3.Name == "td")
                                        ui.duration = xnl3.InnerText;
                                }
                                break;
                            }

                        }
                    }
                }
            }
            ui.newReportdataGridView.Rows[0].Cells[2].Value = ui.clip;
            ui.newReportdataGridView.Rows[1].Cells[2].Value = ui.dltype;
            ui.newReportdataGridView.Rows[2].Cells[2].Value = ui.duration;
            ui.newReportdataGridView.Rows[3].Cells[2].Value = ui.fmt;
            ui.newReportdataGridView.Rows[4].Cells[2].Value = ui.platform;
            ui.newReportdataGridView.Rows[5].Cells[2].Value = ui.rate;
            ui.newReportdataGridView.Rows[6].Cells[2].Value = ui.status;
            ui.newReportdataGridView.Rows[7].Cells[2].Value = ui.testid;
            ui.newReportdataGridView.Rows[8].Cells[2].Value = ui.type;
            ui.newReportdataGridView.Rows[9].Cells[2].Value = ui.uip;
            ui.newReportdataGridView.Rows[10].Cells[2].Value = ui.vid;

        }
        if (oSession.HostnameIs("livew.l.qq.com") && oSession.uriContains("livemsg") && oSession.uriContains("ad_type=LD"))
        {

            ui.requesFlag[1] = true;
            oSessionList.Add(oSession);
            //ui.Sessions.Items.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【广告请求】" + oSession.fullUrl);
            byte[] responseByte = oSession.ResponseBody;
            string responseString = System.Text.Encoding.UTF8.GetString(responseByte);

            //判断为json格式(!!!!!!!!!!如何读取json的数据)
            if (responseString != "" && responseString[0] != '<')
            {
                string responseStringJson = responseString.Split('(')[1].Split(')')[0];//因为广告返回是jsonp格式，会有函数名加（），需要取出纯粹的json数据
                JObject responseObject = (JObject)JsonConvert.DeserializeObject(responseStringJson);
                string item = responseObject["adList"]["item"].ToString();
                JArray Jitem = JArray.Parse(item);
                foreach (var ad in Jitem)
                {
                    JArray image = (JArray.Parse(((JObject)ad)["image"].ToString()));
                    string url = ((JObject)image[0])["url"].ToString();
                    if (url != "")
                    {
                        ui.adUrls = url;
                        string[] sArray = ui.adUrls.Split('/');
                        ui.adUrlHost = sArray[2];//广告资源的host
                        ui.adUrl = sArray[3];//广告资源的url
                        //MessageBox.Show(url);
                        break;
                    }
                }
            }

            //判断为xml格式，移动端H5 V2播放器
            if (responseString != "" && responseString[0] == '<')
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseString);

                string rootNode = "root";
                string goalElem = "url";

                XmlNode xn = doc.SelectSingleNode(rootNode);
                XmlNodeList xnList = xn.ChildNodes;

                foreach (XmlNode xnl1 in xnList)
                {
                    if (xnl1.Name == "adList")
                    {
                        foreach (XmlNode xnl2 in xnl1.ChildNodes)
                        {
                            if (xnl2.Name == "item")
                            {
                                foreach (XmlNode xnl3 in xnl2.ChildNodes)
                                {
                                    if (xnl3.Name == "image")
                                    {
                                        foreach (XmlNode xnl4 in xnl3.ChildNodes)
                                        {
                                            if (xnl4.Name == goalElem && xnl4.InnerText != "")
                                            {
                                                ui.adUrls = xnl4.InnerText;
                                                //ui.Sessions.Items.Add(xnl4.InnerText); 
                                                string[] sArray = ui.adUrls.Split('/');
                                                ui.adUrlHost = sArray[2];//广告资源的host
                                                ui.adUrl = sArray[3];//广告资源的url

                                                /*输出结果*/
                                                //ui.Sessions.Items.Add(adUrlHost);
                                                //ui.Sessions.Items.Add(adUrl);
                                                //ui.AddResult(adUrlHost);
                                                //ui.AddResult(adUrl);
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                if (ui.adUrls != "AAAAAAAAAAAAAAA")
                                {
                                    break;
                                }

                            }
                        }
                        break;
                    }

                }
            }

        }

    }
    private void checkPCH5Pre(Session oSession)
    {     
            
        /*上报4298*/
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4298") && (Regex.Match(oSession.fullUrl, stepPATTERN[3]).Success))
        {

            Match m = Regex.Match(oSession.fullUrl, stepPATTERN[3]);
            string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
            ui.requesFlag[3] = true;
            oSessionList.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
            //MessageBox.Show("step3");
            /*动态增加行数显示上报的参数*/
            DataRow dr;
            dr = DT.NewRow();
            dr["STEP"] = step;
            dr["Session-id"] = oSession.id;

            /*判断上报参数是否缺失*/
            string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            /*初始化*/
            bool[] valFlag = getDr(dr, url, PATTERN, N);
            /*valFlag用于标志上报中是否包含某个参数*/
            //valFlag[0]:val
            //valFlag[1]:val1
            //valFlag[2]:val2
            //valFlag[3]:bi
            //valFlag[4]:bt
            //valFlag[5]:vid
            //valFlag[6]:vt

            /*step=3的上报中必须含有的是：val，vid*/
            int curRow = ui.ReportdataGridView.Rows.Count - 2;
            if (!(valFlag[0]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
            }

            if (!(valFlag[5]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
            }

        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4298") && (Regex.Match(oSession.fullUrl, stepPATTERN[4]).Success))
        {
            Match m = Regex.Match(oSession.fullUrl, stepPATTERN[4]);
            string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
            ui.requesFlag[4] = true;
            oSessionList.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
            //MessageBox.Show("step3");
            /*动态增加行数显示上报的参数*/
            DataRow dr;
            dr = DT.NewRow();
            dr["STEP"] = step;
            dr["Session-id"] = oSession.id;

            /*判断上报参数是否缺失*/
            string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
            bool[] valFlag = getDr(dr, url, PATTERN, N);

            /*step=4的上报中必须含有的是：val，val1,bi,vid,vt*/
            int curRow = ui.ReportdataGridView.Rows.Count - 2;
            if (!(valFlag[0]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
            }
            if (!(valFlag[1]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
            }
            if (!(valFlag[3]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
            }
            if (!(valFlag[5]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
            }
            if (!(valFlag[6]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
            }
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4298") && (Regex.Match(oSession.fullUrl, stepPATTERN[5]).Success))
        {
            Match m = Regex.Match(oSession.fullUrl, stepPATTERN[5]);
            string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
            ui.requesFlag[5] = true;
            oSessionList.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
            //MessageBox.Show("step3");
            /*动态增加行数显示上报的参数*/
            DataRow dr;
            dr = DT.NewRow();
            dr["STEP"] = step;
            dr["Session-id"] = oSession.id;

            /*判断上报参数是否缺失*/
            string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
            bool[] valFlag = getDr(dr, url, PATTERN, N);

            /*step=5的上报中必须含有的是：val，val1,bi,bt,vt*/
            int curRow = ui.ReportdataGridView.Rows.Count - 2;

            if (!(valFlag[0]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
            }
            if (!(valFlag[1]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
            }
            if (!(valFlag[3]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
            }
            if (!(valFlag[4]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[5].Style.BackColor = Color.Red;
            }
            if (!(valFlag[6]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
            }

        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4298") && (Regex.Match(oSession.fullUrl, stepPATTERN[6]).Success))
        {
            Match m = Regex.Match(oSession.fullUrl, stepPATTERN[6]);
            string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
            ui.requesFlag[6] = true;
            oSessionList.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
            //MessageBox.Show("step3");
            /*动态增加行数显示上报的参数*/
            DataRow dr;
            dr = DT.NewRow();
            dr["STEP"] = step;
            dr["Session-id"] = oSession.id;

            /*判断上报参数是否缺失*/
            string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);
            bool[] valFlag = getDr(dr, url, PATTERN, N);

            /*step=6的上报中必须含有的是：val，val1,bi,vid,vt*/
            int curRow = ui.ReportdataGridView.Rows.Count - 2;
            if (!(valFlag[0]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
            }
            if (!(valFlag[1]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
            }
            if (!(valFlag[3]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
            }
            if (!(valFlag[5]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
            }
            if (!(valFlag[6]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
            }
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4298") && (Regex.Match(oSession.fullUrl, stepPATTERN[7]).Success))
        {
            Match m = Regex.Match(oSession.fullUrl, stepPATTERN[7]);
            string step = m.Value.Split('=')[1].Split('&')[0];//有可能是合并上报
            ui.requesFlag[7] = true;
            oSessionList.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【上报step=" + step + "】" + oSession.fullUrl);
            //MessageBox.Show("step3");
            /*动态增加行数显示上报的参数*/
            DataRow dr;
            dr = DT.NewRow();
            dr["STEP"] = step;
            dr["Session-id"] = oSession.id;

            /*判断上报参数是否缺失*/
            string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);

            bool[] valFlag = getDr(dr, url, PATTERN, N);

            /*step=7的上报中必须含有的是：val，val1,bi,vid,vt*/
            int curRow = ui.ReportdataGridView.Rows.Count - 2;
            if (!(valFlag[0]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[1].Style.BackColor = Color.Red;
            }
            if (!(valFlag[1]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[2].Style.BackColor = Color.Red;
            }
            if (!(valFlag[3]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[4].Style.BackColor = Color.Red;
            }
            if (!(valFlag[5]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[6].Style.BackColor = Color.Red;
            }
            if (!(valFlag[6]))
            {
                ui.ReportdataGridView.Rows[curRow].Cells[7].Style.BackColor = Color.Red;
            }
        }
       
        /*上报4501*/
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=0&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step0";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr,url);
            getHeadPre(url, newPATTERN,0);


        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=5&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step5";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPre(url, newPATTERN, 5);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=10&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step10";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPre(url, newPATTERN, 10);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=15&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step15";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 15);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=20&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step20";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 20);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=25&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step25";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 25);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=30&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step30";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 30);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=31&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step31";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 31);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=35&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step35";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 35);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=40&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step40";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 40);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=45&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step45";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 45);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=48&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step48";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 48);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=49&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step49";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 49);
        }
        if (oSession.HostnameIs("btrace.video.qq.com") && oSession.uriContains("bossid=4501") && oSession.uriContains("step=50&"))
        {
            DataRow dr;
            dr = DTnew2.NewRow();
            dr["STEP"] = "step50";
            dr["Session-id"] = oSession.id;

            /*将data提取出来*/
            //string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string url = oSession.fullUrl;
            getData(dr, url);
            getHeadPost(url, newPATTERN, 50);
        }

    }
    private void checkPCH5Post(Session oSession)
    {

        //getinfo。isHLS=0限定为是mp4格式
        if (oSession.HostnameIs("vv.video.qq.com") && oSession.uriContains("getinfo") )
        {
            ui.requesFlag[0] = true;
            oSessionList.Add(oSession);
            //ui.Sessions.Items.Add(oSession);
            ui.AddResult("【" + oSession.id + "】" + "【getinfo】" + oSession.fullUrl);

            //处理getinfo的请求
            string url = System.Web.HttpUtility.UrlDecode(oSession.fullUrl);//将url解码
            string patternvid = @"([\W]{0,1})vid=([\d\w]*)";
            string patternplatform = @"([\W]{0,1})platform=(\d*)";
            Match m = Regex.Match(url,patternvid);
            if (m.Success)
            {
                string[] s = m.Value.Split('=');
                ui.vid = s[1];
            }
            m = Regex.Match(url,patternplatform);
            if (m.Success)
            {
                string[] s = m.Value.Split('=');
                ui.platform = s[1];
               
            }
            //处理getinfo的返回结果
            byte[] jsonByte = oSession.ResponseBody;//获取的是已byte为单位的字符串
            //string jsonString = System.Text.Encoding.Default.GetString(jsonByte);
            string jsonString = System.Text.Encoding.UTF8.GetString(jsonByte);//此处直接用Dafault会出现中文乱码，要用UTF8编码
            int start = jsonString.IndexOf('(');
            int end = jsonString.LastIndexOf(')');
            string responseStringJson = jsonString.Substring(start + 1, end - start - 1);

            JObject responseObject = (JObject)JsonConvert.DeserializeObject(responseStringJson);//转换成json格式 

            //将getinfo的字段值提取出来

            JArray Jfi = JArray.Parse(responseObject["fl"]["fi"].ToString());
            foreach (var item in Jfi)
            {
                
                //if (((JObject)item).Property("sl") != null && ((JObject)item).Property("sl").ToString() == "1" && ((JObject)item).Property("id") != null && ((JObject)item).Property("id").ToString() != "")
                if (((JObject)item).Property("sl") != null && ((JObject)item)["sl"].ToString() == "1")
                {
                    //MessageBox.Show("item");
                    ui.fmt=((JObject)item)["id"].ToString();
                    //MessageBox.Show(((JObject)item).Property("id").ToString());//会显示"id":10212
                    break;
                }
            }

            string vi = responseObject["vl"]["vi"].ToString();
            JArray Jvi = JArray.Parse(vi);
            JObject JviObj = (JObject)(Jvi[0]);


            if (JviObj.Property("cl")!=null&&((JObject)(JviObj["cl"])).Property("fc") != null && ((JObject)(JviObj["cl"])).Property("fc").ToString() != "")
            {
                ui.clip = JviObj["cl"]["fc"].ToString();
            }
            if (JviObj.Property("td") != null && JviObj.Property("td").ToString() != "")
            {
                ui.duration = JviObj["td"].ToString();
            }
            if (JviObj.Property("br") != null && JviObj.Property("br").ToString() != "")
            {
                ui.rate = JviObj["br"].ToString();
            }
            if (JviObj.Property("vst") != null && JviObj.Property("vst").ToString() != "")
            {
                ui.status = JviObj["vst"].ToString();
            }
            if (JviObj.Property("type") != null && JviObj.Property("type").ToString() != "")
            {
                ui.type = JviObj["type"].ToString();
            }
            if (responseObject.Property("dltype") != null && responseObject.Property("dltype").ToString() != "")
            {
                ui.dltype = responseObject["dltype"].ToString();
            }
            if (responseObject.Property("uip") != null && responseObject.Property("uip").ToString() != "")
            {
                ui.uip = responseObject["uip"].ToString();
            }
            if (responseObject.Property("tstid") != null && responseObject.Property("tstid").ToString() != "")
            {
                ui.testid = responseObject["tstid"].ToString();
            }
            ui.newReportdataGridView.Rows[0].Cells[2].Value = ui.clip;
            ui.newReportdataGridView.Rows[1].Cells[2].Value  = ui.dltype;
            ui.newReportdataGridView.Rows[2].Cells[2].Value = ui.duration;
            ui.newReportdataGridView.Rows[3].Cells[2].Value = ui.fmt;
            ui.newReportdataGridView.Rows[4].Cells[2].Value = ui.platform;
            ui.newReportdataGridView.Rows[5].Cells[2].Value = ui.rate;
            ui.newReportdataGridView.Rows[6].Cells[2].Value = ui.status;
            ui.newReportdataGridView.Rows[7].Cells[2].Value = ui.testid;
            ui.newReportdataGridView.Rows[8].Cells[2].Value = ui.type;
            ui.newReportdataGridView.Rows[9].Cells[2].Value = ui.uip;
            ui.newReportdataGridView.Rows[10].Cells[2].Value =ui.vid;

            //string clip = JviObj["cl"]["fc"].ToString();
            //string duration = JviObj["td"].ToString();
            //string rate = JviObj["br"].ToString();
            //string status = JviObj["vst"].ToString();
            //string type = JviObj["type"].ToString();
            //string dltype = responseObject["dltype"].ToString();
            //string uip = responseObject["uip"].ToString();
            //string clip2=(((JObject)(JviObj["cl"])).Property("fc") == null || ((JObject)(JviObj["cl"])).Property("fc").ToString() == "") ? "" : JviObj["cl"]["fc"].ToString();

            /*
            JArray Jui = JArray.Parse(((JObject)(Jvi[0]))["ul"]["ui"].ToString());//移动端H5播放器只取第一个cdn地址
            string fn = ((JObject)(Jvi[0]))["fn"].ToString();
            string url = ((JObject)(Jui[0]))["url"].ToString();
            string[] sArray = url.Split('/');
            ui.videoHost = sArray[2];//视频资源的host
            ui.videoUrls = sArray[3] + "/" + fn;//视频资源资源的url
            */


        }
    }
        
}

