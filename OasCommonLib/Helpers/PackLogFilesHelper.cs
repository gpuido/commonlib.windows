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
        public static bool Pack(string logFolder, int howManyDaysSaveInArchive)
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
                var logs = new Dictionary<string, List<string>>();

                foreach (var ll in loglist)
                {
                    if (HowManyDaysFromNow(ll, out YearMonthInfo logYearMonth) > howManyDaysSaveInArchive)
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

            using (ZipFile z = File.Exists(zipFilePath) ? new ZipFile(zipFilePath) : ZipFile.Create(zipFilePath))
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

        public static int HowManyDaysFromNow(string fileName, out YearMonthInfo logDate)
        {
            int howManyDaysFromNow = 0;

            fileName = Path.GetFileName(fileName.Replace('/', '\\'));

            logDate = null;
            if (String.IsNullOrEmpty(fileName))
            {
                throw new Exception(String.Format("wrong file name: '{0}'", fileName));
            }

            string[] parts = fileName.Split('-');
            if (3 == parts.Length)
            {
                if (parts[0].StartsWith("updater"))
                {
                    parts[0] = parts[0].Replace("updater", String.Empty);
                }

                if (!int.TryParse(parts[0], out int year))
                {
                    throw new Exception(String.Format("wrong year in name : '{0}'", fileName));
                }
                if (!int.TryParse(parts[1], out int month))
                {
                    throw new Exception(String.Format("wrong month in name : '{0}'", fileName));
                }

                var index = parts[2].IndexOf('.');
                var tmp = parts[2].Substring(0, index);
                if (!int.TryParse(tmp, out int day))
                {
                    throw new Exception(String.Format("wrong month in name : '{0}'", fileName));
                }

                logDate = new YearMonthInfo(year, month);
                var parsedDate = new DateTime(year, month, day);
                var dateDiff = DateTime.Now - parsedDate.Date;

                howManyDaysFromNow = dateDiff.Days;
            }
            else
            {
                throw new Exception(String.Format("wrong file name format : '{0}'", fileName));
            }

            return howManyDaysFromNow;
        }
    }
}
