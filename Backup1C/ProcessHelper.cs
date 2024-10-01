using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Backup1C
{
    internal static class ProcessHelper
    {
        public static List<Process> GetMatchingProcesses(List<string> processPatterns)
        {
            List<Process> matchingProcesses = new List<Process>();

            var runningProcesses = Process.GetProcesses();

            foreach (var process in runningProcesses)
            {
                foreach (var pattern in processPatterns)
                {
                    // Преобразуем шаблон с '*' в регулярное выражение (заменяем '*' на '.*')
                    string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";

                    Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

                    if (regex.IsMatch(process.ProcessName))
                    {
                        matchingProcesses.Add(process);
                        break;
                    }
                }
            }

            return matchingProcesses;
        }
    }
}
