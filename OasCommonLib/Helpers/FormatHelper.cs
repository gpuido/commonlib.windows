namespace OasCommonLib.Helpers
{
    using Constants;
    using System;

    public class FormatHelper
    {
        public readonly static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        public readonly static string LogDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public readonly static string DateFormat = "yyyy-MM-dd";
        public readonly static string TimeFormat = "HH:mm";
        public readonly static string DateTimeImageFormat = "MMM/dd/yyyy HH:mm:ss";
        public readonly static string DateSqlFormat = "yyyy-MM-dd";

        public static string FormatBytes(ulong bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "" };
            ulong max = (ulong)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0";
        }

        public static string HtmlDecode(string data)
        {
            if (String.IsNullOrEmpty(data))
            {
                return String.Empty;
            }

            return data.Replace(OasStringConstants.Plus, OasStringConstants.Space).Replace("%20", OasStringConstants.Space);
        }
    }
}
