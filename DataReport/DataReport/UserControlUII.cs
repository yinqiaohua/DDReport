using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataReport;
using DataReport.util;
using Fiddler;
using System.IO;

namespace DataReport
{
    public partial class UserControlUII : UserControl
    {
        public UserControlUII()
        {
            InitializeComponent();
            requesFlag = new bool[9];
            for (int i = 0; i != requesFlag.Length; i++)
            {
                requesFlag[i] = false;
            }
            adUrlHost = "AAAAAAAAAAAAAAA";
            adUrl = "AAAAAAAAAAAAAAAAAA";
            adUrls = "AAAAAAAAAAAAAAA";
            videoHost = "AAAAAAAAAAAA";
            videoUrls = "AAAAAAAAAAAAAAAAAAA";

            //getinfo字段初始化
            fmt = "";
            clip = "";
            duration = "";
            rate = "";
            status = "";
            type = "";
            dltype = "";
            uip = "";
            testid = "";
            platform = "";
            vid = "";

            first = true;

        }
        public DeviceType getDeviceType()
        {
            if (this.MobileH5.Checked)
            {
                return DeviceType.MobileH5;
            }
            else if (this.flash.Checked)
            {
                return DeviceType.flash;
            }
            else if (this.PCH5.Checked)
            {
                return DeviceType.PCH5;
            }
            else
            {
                return DeviceType.error;
            }
        }

        private void clearbutton_Click(object sender, EventArgs e)
        {
            this.Sessions.Items.Clear();
            /*清空表格的数据*/
            DataTable DTclear = (DataTable)ReportdataGridView.DataSource;
            DTclear.Rows.Clear();
            ReportdataGridView.DataSource = DTclear;

            

            DataTable DTclearnew = new DataTable();
            DTclearnew.Columns.Add(new DataColumn("参数", typeof(string)));
            DTclearnew.Columns.Add(new DataColumn("getinfo之前", typeof(string)));
            DTclearnew.Columns.Add(new DataColumn("getinfo", typeof(string)));
            DTclearnew.Columns.Add(new DataColumn("getinfo之后", typeof(string)));
            DTclearnew.Columns.Add(new DataColumn("字段含义", typeof(string)));
            

            DataRow dr;
            string[] parameters = { "clip", "dltype", "duration", "fmt", "platform", "rate", "status", "testid", "type", "uip", "vid" };
            string[] describle = { "视频分片数", "下载类型", "视频本身时长(单位:s)", "首次加载对应的视频格式", "平台号", "首次加载对应的音视频码率(单位:kByte/s) ", "视频本身状态", "ABTest测试分组id", "视频类型", "用户IP: ipv4, ipv6", "视频ID" };
            for (int i = 0; i < parameters.Length; i++)
            {
                dr = DTclearnew.NewRow();
                dr["参数"] = parameters[i];
                dr["字段含义"] = describle[i];
                DTclearnew.Rows.Add(dr);
            }
            newReportdataGridView.DataSource = DTclearnew;



            DataTable DTclearnew2 = (DataTable)newReportdataGridView2.DataSource;
            DTclearnew2.Rows.Clear();
            newReportdataGridView2.DataSource = DTclearnew2;


            for (int i = 0; i != requesFlag.Length; i++)
            {
                requesFlag[i] = false;
            }
            adUrlHost = "AAAAAAAAAAAAAAA";
            adUrl = "AAAAAAAAAAAAAAAAAA";
            adUrls = "AAAAAAAAAAAAAAA";
            videoHost = "AAAAAAAAAAAA";
            videoUrls = "AAAAAAAAAAAAAAAAAAA";

            fmt = "";
            clip = "";
            duration = "";
            rate = "";
            status = "";
            type = "";
            dltype = "";
            uip = "";
            testid = "";
            platform = "";
            vid = "";

            first = true;

            adlist.Text = "";
            videolist.Text = "";
            ZC.Text = "";
            HT.Text = "";
            PSJ.Text = "";
            ZT.Text = "";
            ZI.Text = "";
            PPB.Text = "";
            PDF.Text = "";
            getkey.Text = "";
            getinfo.Text = "";
        }
        public delegate void Delegate_AddResult(string strUrl);//定义输出结果的委托
        public void AddResult(string strUrl)
        {
            
            if (!this.Sessions.InvokeRequired)
            {   
                this.Sessions.Items.Add(strUrl);
                //MessageBox.Show(strUrl);

            }
                
            else
            {
                Delegate_AddResult delegate_addresult = new Delegate_AddResult(this.AddResult);
                this.Sessions.Invoke(delegate_addresult, strUrl);
                //MessageBox.Show("a2");
            }
        }

        private void Sessions_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index > -1)
            {
                string s = this.Sessions.Items[e.Index].ToString();
                if (s.Contains("【ORDER-ERROR】"))
                {
                    //ListBox listbox = sender as ListBox;
                    e.DrawBackground();
                    e.DrawFocusRectangle();
                    e.Graphics.DrawString(s, this.Sessions.Font, Brushes.Red, e.Bounds);
                }
                else
                {
                    e.DrawBackground();
                    e.DrawFocusRectangle();
                    e.Graphics.DrawString(s, this.Sessions.Font, Brushes.Black, e.Bounds);
                }
            }

        }

        private void Sessions_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 60;
        }

        private void MobileH5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ReportdataGridView_CellContentClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {

        }

        private void Sessions_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void flash_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void Introduction_Enter(object sender, EventArgs e)
        {
            
        }

        private void labelhead_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void adlist_TextChanged(object sender, EventArgs e)
        {

        }

        private void vd_CheckedChanged(object sender, EventArgs e)
        {

        }





    }
}
