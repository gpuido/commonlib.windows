namespace OasCommonLib.Helpers
{
    using ICSharpCode.SharpZipLib.Zip;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class PackLogsHelper
    {
        public class YearMonthInfo
        {
            public int Year { get; private set; }
            public int Month { get; private set; }

            public YearMonthInfo(int year, int mon)
            {
                Year = year;
                Month = mon;
            }

            public override string ToString()
            {
                return String.Format("{0:0000}-{1:00}", Year, Month);
            }
        }

        public static string LastError { get; private set; }
        public static bool Pack(string logFolder)
        {
            bool res = false;

            LastError = String.Empty;

            try
            {
                if (!Directory.Exists(logFolder))
                {
                    throw new ArgumentException(String.Format("log folder '{0}' doesn't exist", logFolder));
                }

                string[] loglist = Directory.GetFiles(logFolder, "*.log");
                string todayLogName = DateTime.Now.ToString(FormatHelper.DateFormat) + ".log";
                YearMonthInfo logYearMonth;
                Dictionary<string, List<string>> logs = new Dictionary<string, List<string>>();

                foreach (var ll in loglist)
                {
                    if (!IsCurrentMonth(ll, out logYearMonth))
                    {
                        if (!logs.ContainsKey(logYearMonth.ToString()))
                        {
                            logs.Add(logYearMonth.ToString(), new List<string>());
                        }
                        logs[logYearMonth.ToString()].Add(ll);
                    }
                }

                foreach (var l in logs.Keys)
                {
                    PackFilesToZip(logFolder, l, logs[l]);
                }

                res = true;
            }
            catch (Exception ex)
            {
                LastError = String.Format("PackLogFiles: error : {0}", ex.Message);
            }

            return res;
        }

        private static void PackFilesToZip(string logFolder, string yearMonth, List<string> list)
        {
            string zipFilePath = Path.Combine(logFolder, String.Format("{0}.log.zip", yearMonth));

            using (ZipFile z = ZipFile.Create(zipFilePath))
            {
                z.BeginUpdate();

                foreach (var l in list)
                {
                    z.Add(l);
                }

                z.CommitUpdate();
                z.Close();
            }

            foreach (var l in list)
            {
                File.Delete(l);
            }
        }

        public static bool IsCurrentMonth(string fileName, out YearMonthInfo logDate)
        {
            int year, month;
            bool res = false;

            fileName = Path.GetFileName(fileName.Replace('/', '\\'));

            logDate = null;
            if (String.IsNullOrEmpty(fileName))
            {
                throw new Exception(String.Format("wrong file name: '{0}'", fileName));
            }

            string[] parts = fileName.Split('-');
            if (3 == parts.Length)
            {
                if (!int.TryParse(parts[0], out year))
                {
                    throw new Exception(String.Format("wrong year in name : '{0}'", fileName));
                }
                if (!int.TryParse(parts[1], out month))
                {
                    throw new Exception(String.Format("wrong month in name : '{0}'", fileName));
                }

                logDate = new YearMonthInfo(year, month);
                res = year == DateTime.Now.Year && month == DateTime.Now.Month;
            }
            else
            {
                throw new Exception(String.Format("wrong file name format : '{0}'", fileName));
            }

            return res;
        }
    }
}
