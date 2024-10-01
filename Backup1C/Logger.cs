using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup1C
{
    internal class Logger
    {
        private readonly string logFilePath;
        private readonly string logFile1СPath;

        public Logger(string log1Cpath, string logPath = "application.log")
        {
            logFilePath = logPath;
            logFile1СPath = log1Cpath;
        }

        public void AppendTo1C(string text)
        {
            using (StreamWriter writer = new StreamWriter(logFile1СPath, true))
            {
                writer.WriteLine($"[{DateTime.Now}] {text}");
            }
        }

        private void Append(string text)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now} {text}");
            }
        }

        public void AppendError(string errorText)
        {
            Append($"[ERROR] {errorText}");
        }

        public void AppendInfo(string infoText)
        {
            Append($"[INFO] {infoText}");
        }

        public void AppendWarning(string warningText)
        {
            Append($"[WARNING] {warningText}");
        }
    }
}
