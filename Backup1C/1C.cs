using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Backup1C
{
    internal class C1
    {
        protected Settings settings;

        public C1(Settings settings)
        {
            if (!File.Exists(settings.Executable1CPath))
                throw new FileNotFoundException();
            
            this.settings = settings;
        }

        public string Executable1CPath
        {
            get
            {
                return settings.Executable1CPath;
            }
        }

        public string CommandBackupArguments
        { 
            get
            {
                StringBuilder argumentsBuilder = new StringBuilder("DESIGNER /F \"");
                argumentsBuilder.Append(settings.FolderBasePath);
                argumentsBuilder.Append("\" /N \"");
                argumentsBuilder.Append(settings.Login);
                argumentsBuilder.Append("\" /P \"");
                argumentsBuilder.Append(settings.Password);
                argumentsBuilder.Append("\" /DumpIB \"");
                argumentsBuilder.Append(settings.FolderBackupPath);
                argumentsBuilder.Append("\\Base_");
                argumentsBuilder.Append(DateTime.Now.ToString("dd_MM_yyyy"));
                argumentsBuilder.Append(".dt\" /Out ");
                argumentsBuilder.Append(settings.FolderBackupPath);
                argumentsBuilder.Append("\\1C.log -NoTruncate");

                return argumentsBuilder.ToString();
            }
        }

        public string CommandStart1CEnterprise
        {
            get
            {
                StringBuilder argumentsBuilder = new StringBuilder("ENTERPRISE /F \"");
                argumentsBuilder.Append(settings.FolderBasePath);
                argumentsBuilder.Append("\"");
                return argumentsBuilder.ToString();
            }
        }

        //заглушка под будущую команду выкидывания пользователей
        public string CommandLogoutUsers
        {
            get
            {
                return "";
            }
        }

        //заглушка под будущую команду восстановления БД
        public string CommandRepairDB
        {
            get
            {
                return "";
            }
        }
    }
}
