using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Backup1C
{
    internal static class Files
    {
        public static double AverageSize(string FolderPath, string Extension)
        {
            var files = Directory.GetFiles(FolderPath, $"*.{Extension}");
            if (files.Length == 0)
                return 0;

            return files.Average(f => new FileInfo(f).Length);
        }

        public static long SumSize(string FolderPath, string Extension)
        {
            var files = Directory.GetFiles(FolderPath, $"*.{Extension}");
            //.Where(f => f.Substring(f.LastIndexOf('.') + 2).All(char.IsDigit));

            if (files.Count() == 0)
                return 0;

            return files.Sum(f => new FileInfo(f).Length);
        }

        public static DateTime MaxChangeDate(string FolderPath, List<string> allowedExtensions, List<string> excludedExtensions)
        {
            var files = Directory.GetFiles(FolderPath)
                     .Where(file => (allowedExtensions.Contains(".*") || allowedExtensions.Contains(Path.GetExtension(file).ToLower()))
                                    && !excludedExtensions.Contains(Path.GetExtension(file).ToLower()))
                     .ToList();

            if (files.Count == 0)
                throw new FileNotFoundException();

            var maxChangeDate = files.Max(f => File.GetLastWriteTime(f));

            return maxChangeDate;
        }
    }

    public static class DoubleExtensions
    {
        public static double ToMegabytes(this double bytes) // Конвертация байтов в мегабайты
        {
            return Math.Round(bytes / (1024 * 1024), 2); 
        }
    }
}
