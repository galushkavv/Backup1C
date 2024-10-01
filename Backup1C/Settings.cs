using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup1C
{
    internal class Settings
    {
        public string Executable1CPath
        { get; set; }

        public string FolderBasePath
        { get; set; }

        public string FolderBackupPath
        { get; set; }

        public string Login
        { get; set; }

        public string Password
        { get; set; }

        public void LoadFromIni(string filePath)
        {
            Ini settingsIni = new Ini(filePath);
            settingsIni.Load();

            Executable1CPath = settingsIni.GetValue("bin ", "RUN");
            FolderBasePath = settingsIni.GetValue("base ", "RUN");
            FolderBackupPath = settingsIni.GetValue("backup ", "RUN");
            Login = settingsIni.GetValue("login ", "RUN");
            Password = settingsIni.GetValue("password ", "RUN");
        }
    }
}
