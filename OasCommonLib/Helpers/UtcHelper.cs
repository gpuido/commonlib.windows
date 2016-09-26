namespace OasCommonLib.Helpers
{
    using System;

    public class UtcHelper
    {
        public static DateTime UtcToLocal(DateTime utc, string tz)
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(tz);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, tzi);
        }

        public static string UtcToLocal(DateTime utc, string tz, string format)
        {
            string[] letters = tz.Split(' ');
            string timeZone = "";
            DateTime dt = UtcToLocal(utc, tz);

            foreach (var l in letters)
            {
                timeZone += l[0];
            }

            return dt.ToString(format) + " " + timeZone;
        }
    }
}
