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
    public partial class UserControUI : UserControl
    {
        public UserControUI()
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
           
        }

        private void flash_CheckedChanged(object sender, EventArgs e)
        {

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
            else
            {
                return DeviceType.error;
            }
        }

        private void Sessions_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void results_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void ReportEnabled_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void clearbutton_Click(object sender, EventArgs e)
        {
            this.Sessions.Items.Clear();
            /*清空表格的数据*/
            DataTable DTclear = (DataTable)ReportdataGridView.DataSource;
            DTclear.Rows.Clear();
            ReportdataGridView.DataSource = DTclear;
            for (int i = 0; i != requesFlag.Length; i++)
            {
                requesFlag[i] = false;
            }
            adUrlHost = "AAAAAAAAAAAAAAA";
            adUrl = "AAAAAAAAAAAAAAAAAA";
            adUrls = "AAAAAAAAAAAAAAA";
            videoHost = "AAAAAAAAAAAA";
            videoUrls = "AAAAAAAAAAAAAAAAAAA";
            
        }


        public delegate void Delegate_AddResult(string strUrl);//定义输出结果的委托
        public void AddResult(string strUrl)
        {
            if (!this.Sessions.InvokeRequired)
                this.Sessions.Items.Add(strUrl);
            else
            {
                Delegate_AddResult delegate_addresult = new Delegate_AddResult(this.AddResult);
                this.Sessions.Invoke(delegate_addresult, strUrl);
            }
        }
        //展示方面，如果session中含有【ORDER-ERROR】则标红展示
        private void Sessions_DrawItem(object sender, DrawItemEventArgs e)
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

        private void Sessions_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 60;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

       /* private  void Sessions_DrawItem(object sender, DrawItemEventArgs e)
        {
            
            ListBox listbox = sender as ListBox;
            

            Color vColor = e.ForeColor;
            string s = this.Sessions.Items[e.Index].ToString();
            if (s.Contains("【ORDER-ERROR】"))
            {
                vColor = Color.Red;
            }
            e.Graphics.FillRectangle(new SolidBrush (vColor),e.Bounds);
  
            e.Graphics.DrawString(s, e.Font, new SolidBrush (e.ForeColor), e.Bounds);
            e.DrawFocusRectangle();
            
        }*/

    }
}
