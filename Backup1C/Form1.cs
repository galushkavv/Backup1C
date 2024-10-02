using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace Backup1C
{
    public partial class Form1 : Form
    {
        private string settingFile = "backup1c.ini";
        Settings settings = new Settings();
        C1 c1;
        Logger logger;

        private double averageDtSize = 0;

        private List<string> processesToCheck = new List<string>()
        { 
            "1cv8*"
        };

        public Form1()
        {
            InitializeComponent();

            labelProgress.Text = string.Empty;

            try
            {
                settings.LoadFromIni(settingFile);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Не найден файл с настройками " + settingFile, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            
            //string folderPath = AppDomain.CurrentDomain.BaseDirectory;
            logger = new Logger(settings.FolderBackupPath + "\\1C.log", settings.FolderBackupPath + "\\application.log");

            try
            {
                c1 = new C1(settings);
            }
            catch(FileNotFoundException)
            {
                label1.Text = "";
                label2.Text = "";
                logger.AppendError("Не найден исполняемый файл 1С");
                MessageBox.Show("Не найден исполняемый файл 1С", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            logger.AppendInfo("Программа запущена");

            richTextBox1.AppendText(c1.Executable1CPath + " " + c1.CommandBackupArguments + Environment.NewLine);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Backup();
        }

        private void Backup()
        {
            // проверка необходимости делать бэкап

            DateTime lastBaseChange = DateTime.MaxValue;
            DateTime lastBackup = DateTime.MinValue;

            try
            {
                lastBaseChange = Files.MaxChangeDate(
                        settings.FolderBasePath,
                        new List<string> { ".*" },
                        new List<string> { ".log", ".txt", ".ini", ".exe", ".vbs", ".dll", ".bat", ".lnk"} );
            }
            catch (Exception ex)
            {
                label1.Text = "";
                label2.Text = "";
                logger.AppendError(ex.Message);
                MessageBox.Show("Проблема при обращении к базе 1С", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Start1cAndCloseApplication();
            }
            try
            {
                lastBackup = Files.MaxChangeDate(
                        settings.FolderBackupPath,
                        new List<string> { ".dt" },
                        new List<string> { ".log", ".txt", ".ini", ".exe", ".vbs", ".dll", ".bat", ".lnk" });
            }
            catch (FileNotFoundException)
            {
                logger.AppendInfo("Бэкапы не найдены");
            }
            TimeSpan difference = lastBaseChange - lastBackup;

            if (difference.TotalHours < 24 && lastBaseChange.Date == lastBackup.Date)
            {
                logger.AppendInfo("Бэкап не требуется. Последний бэкап " + lastBackup + " последнее изменение базы " + lastBaseChange + " время между ними " + Math.Round(difference.TotalHours, 1) + " часов");
                Start1cAndCloseApplication();
                return;
            }

            // проверка не запущены ли процессы 1С
            List<Process> foundProcesses = ProcessHelper.GetMatchingProcesses(processesToCheck);

            if (foundProcesses.Any())
            {
                string processList = string.Empty;
                foreach(Process process in foundProcesses)
                {
                    processList += Environment.NewLine + "\t" + process.ProcessName ;
                }
                logger.AppendError("Найден работающий процесс 1С:" + processList);
                logger.AppendWarning("Попытка создания бэкапа, несмотря на работающий процесс 1С");
            }

            averageDtSize = Files.AverageSize(settings.FolderBackupPath, "dt");

            timer1.Enabled = true;
            timer1.Start();

            logger.AppendTo1C(String.Empty);//просто ДатаВремя

            logger.AppendInfo(c1.Executable1CPath + " " + c1.CommandBackupArguments);

            Task.Run(() => RunProcess(c1.CommandBackupArguments)).ContinueWith(t =>
            {
                richTextBox1.AppendText(t.Result.ToString() + Environment.NewLine);

                this.Invoke((Action)(() => { timer1.Stop(); }));

                if (t.Result == 0)
                    logger.AppendInfo("Процесс выгрузки базы завершился успехом");
                else if (t.Result == 1)
                    logger.AppendError("Процесс выгрузки базы завершился неудачей");
                else if (t.Result == 101)
                    logger.AppendError("Процесс выгрузки базы вернул код завершения указывающий на проблемы с данными в базе");
                else
                    logger.AppendError("Процесс выгрузки базы вернул неизвестный код завершения");

                Start1cAndCloseApplication();
            });
        }

        private void Start1cAndCloseApplication()
        {
            //Process.Start(c1.Executable1CPath, c1.CommandStart1CEnterprise);
            Process.Start(c1.Executable1CPath, string.Empty);

            Application.Exit();
        }

        private int RunProcess(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = c1.Executable1CPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                process.WaitForExit();
                return process.ExitCode;
            }
        }

        private void AppendTextToRichTextBox(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action<string>(AppendTextToRichTextBox), text);
            }
            else
            {
                richTextBox1.AppendText(text + Environment.NewLine);
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            await Task.Run(() => PrintSizeAndPercents(settings.FolderBackupPath));
        }

        private void PrintSizeAndPercents(string directoryPath)
        {
            double complete = Files.SumSize(directoryPath, "n?");
            string message = "";

            if (complete > 0)
            {
                message = "Сохранено " + complete.ToMegabytes() + " Мб";

                if (averageDtSize > 0)
                {
                    int percents = (int)(complete * 100 / averageDtSize);

                    message += ", что составляет примерно " + percents + " %";
                }
                Invoke(new Action(() => labelProgress.Text = message));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Thread.Sleep(100);

            Backup();
        }
    }
}
