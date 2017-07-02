namespace DataReport
{
    partial class UserControUI
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserControUI));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.clearbutton = new System.Windows.Forms.Button();
            this.ReportEnabled = new System.Windows.Forms.CheckBox();
            this.flash = new System.Windows.Forms.RadioButton();
            this.MobileH5 = new System.Windows.Forms.RadioButton();
            this.ReportSession = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Sessions = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ReportdataGridView = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            this.ReportSession.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReportdataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.clearbutton);
            this.groupBox1.Controls.Add(this.ReportEnabled);
            this.groupBox1.Controls.Add(this.flash);
            this.groupBox1.Controls.Add(this.MobileH5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // clearbutton
            // 
            resources.ApplyResources(this.clearbutton, "clearbutton");
            this.clearbutton.Name = "clearbutton";
            this.clearbutton.UseVisualStyleBackColor = true;
            this.clearbutton.Click += new System.EventHandler(this.clearbutton_Click);
            // 
            // ReportEnabled
            // 
            resources.ApplyResources(this.ReportEnabled, "ReportEnabled");
            this.ReportEnabled.Name = "ReportEnabled";
            this.ReportEnabled.UseVisualStyleBackColor = true;
            this.ReportEnabled.CheckedChanged += new System.EventHandler(this.ReportEnabled_CheckedChanged);
            // 
            // flash
            // 
            resources.ApplyResources(this.flash, "flash");
            this.flash.Name = "flash";
            this.flash.UseVisualStyleBackColor = true;
            this.flash.CheckedChanged += new System.EventHandler(this.flash_CheckedChanged);
            // 
            // MobileH5
            // 
            resources.ApplyResources(this.MobileH5, "MobileH5");
            this.MobileH5.Checked = true;
            this.MobileH5.Name = "MobileH5";
            this.MobileH5.TabStop = true;
            this.MobileH5.UseVisualStyleBackColor = true;
            // 
            // ReportSession
            // 
            resources.ApplyResources(this.ReportSession, "ReportSession");
            this.ReportSession.Controls.Add(this.tabPage1);
            this.ReportSession.Controls.Add(this.tabPage2);
            this.ReportSession.Name = "ReportSession";
            this.ReportSession.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Sessions);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // Sessions
            // 
            resources.ApplyResources(this.Sessions, "Sessions");
            this.Sessions.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.Sessions.FormattingEnabled = true;
            this.Sessions.Name = "Sessions";
            this.Sessions.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Sessions_DrawItem);
            this.Sessions.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.Sessions_MeasureItem);
            this.Sessions.SelectedIndexChanged += new System.EventHandler(this.Sessions_SelectedIndexChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox3);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ReportdataGridView);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // ReportdataGridView
            // 
            this.ReportdataGridView.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ReportdataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.ReportdataGridView, "ReportdataGridView");
            this.ReportdataGridView.Name = "ReportdataGridView";
            this.ReportdataGridView.ReadOnly = true;
            this.ReportdataGridView.RowTemplate.Height = 23;
            this.ReportdataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // UserControUI
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.ReportSession);
            this.Controls.Add(this.groupBox1);
            this.Name = "UserControUI";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ReportSession.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ReportdataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.RadioButton flash;
        public System.Windows.Forms.RadioButton MobileH5;
        private System.Windows.Forms.TabControl ReportSession;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox3;
        public  System.Windows.Forms.ListBox Sessions;
        public  System.Windows.Forms.CheckBox ReportEnabled;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button clearbutton;
        public System.Windows.Forms.DataGridView ReportdataGridView;

        /*自定义*/
        public bool[] requesFlag;
        public string adUrlHost = "AAAAAAAAAAAAAAA";
        public string adUrl = "AAAAAAAAAAAAAAAAAA";
        public string adUrls = "AAAAAAAAAAAAAAA";
        public string videoHost = "AAAAAAAAAAAA";
        public string videoUrls = "AAAAAAAAAAAAAAAAAAA";
    }
}
