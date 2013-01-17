using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace Download
{
    public class Logger
    {
        public string LogFilePath { get; private set; }

        public Logger(string logFilePath)
        {
            LogFilePath = logFilePath;
        }

        public void WriteMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            bool append = false;
            if (File.Exists(LogFilePath))
            {
                append = true;
            }

            StreamWriter writer = new StreamWriter(LogFilePath, append);

            DateTime date = DateTime.Now;

            writer.WriteLine(date.ToShortDateString() + " " + date.ToShortTimeString() + " : " + message);
            writer.Close();
        }
    }
}
