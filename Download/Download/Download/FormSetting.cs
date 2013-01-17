using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Download
{
    public partial class FormSetting : Form
    {
        bool startProgram = true;
        public FormSetting(bool flag)
        {
            startProgram = flag;
            InitializeComponent();
            DownloadServer = Properties.Settings.Default.DownloadServer;
            numFileDownload = Properties.Settings.Default.numFileDownload;
            numThreadFile = Properties.Settings.Default.numTread;
            timeWaitMin = Properties.Settings.Default.timeWait;
            if (!startProgram)
            {
                label2.Visible = false;
            }
        }
  
        public int timeWaitMin
        {
            get
            {
                return Convert.ToInt32(timeWait.Value);
            }
            set
            {
                timeWait.Value = value;
            }
        }
        public string DownloadServer
        {
            get
            {
                return tb_DownloadServer.Text;
            }
            set
            {
                tb_DownloadServer.Text = value;
            }
        }
        public int numFileDownload
        {
            get
            {
                return Convert.ToInt32(numerFileDownload.Value);
            }
            set
            {
                numerFileDownload.Value = value;
            }
        }
        public int numThreadFile
        {
            get
            {
                return Convert.ToInt32(numerThreadFile.Value);
            }
            set
            {
                numerThreadFile.Value = value;
            }
        }
        private void SettingSave(object sender, EventArgs e)
        {
            Properties.Settings.Default.timeWait = timeWaitMin;
            Properties.Settings.Default.numFileDownload = numFileDownload;
            Properties.Settings.Default.numTread = numThreadFile;
            Properties.Settings.Default.DownloadServer=DownloadServer;

            Properties.Settings.Default.Save(); 
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

    }
}
