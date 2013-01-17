using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using DownloadLibrary;

namespace Download
{
    public partial class AddDownloadForm : Form
    {
        public Uri Uri { get; private set; }
        public string FileName { get; private set; }

        public AddDownloadForm()
        {
            InitializeComponent();
        }

        private void AddDownloadForm_Load(object sender, EventArgs e)
        {

        }
        
        private void Ок(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            bool ok = true;

            try
            {
                Uri = new Uri(tb_Uri.Text);
            }
            catch(Exception ex)
            {
                errorProvider1.SetError(tb_Uri,"Не корректный URL.Ошибка: "+ex.ToString());
                ok = false;
            }

            Regex regex = new Regex(@"[\w\.\s|\-]");

            if(String.IsNullOrEmpty(tb_FileName.Text)||!regex.IsMatch(tb_FileName.Text))
            {
                ok = false;
                errorProvider1.SetError(tb_FileName, "Не корректное имя файла.");
            }

            if (ok)
            {
                FileName = tb_FileName.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void Cancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        /// <summary>
        /// Генерирует имя файла из uri
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GenerateFileNameFromUri(string url)
        {
            Regex r = new Regex(@"^http://[\w/\.\-:|]+/(?<file_name>[\w\.\s|\-]+)", RegexOptions.Compiled);

            string result;

            try
            {
                result = r.Match(url).Result("${file_name}");
            }
            catch (Exception)
            {
                try
                {
                    result = Regex.IsMatch(url, @"^http://[\w/\.\-:|/]+") ? "index.html" : string.Empty;
                }
                catch (Exception)
                {
                    result = string.Empty;
                }
            }

            return result;
        }
        private void CalculateFileName(object sender, EventArgs e)
        {
            tb_FileName.Text = MessageClientServer.GenerateFileNameFromUri(tb_Uri.Text);
        }
    }
    }
