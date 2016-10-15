namespace OasCommonLib.Helpers
{
    using Logger;
    using System;

    public class UtcHelper
    {
        public static readonly string TAG = "UtcHelper";

        public static DateTime UtcToLocal(DateTime utc, string tz)
        {
            try
            {
                TimeZoneInfo tzi;

                var windowsTZ = OlsonTimeZoneHelper.GetTimeZone(tz);
                if (null != windowsTZ)
                {
                    tzi = TimeZoneInfo.FindSystemTimeZoneById(windowsTZ);
                }
                else
                {
                    tzi = TimeZoneInfo.FindSystemTimeZoneById(tz);
                }
                return TimeZoneInfo.ConvertTimeFromUtc(utc, tzi);
            }
            catch (TimeZoneNotFoundException ex)
            {
                LogQueue.Instance.AddError(TAG, ex);
            }
            catch (InvalidTimeZoneException ex)
            {
                LogQueue.Instance.AddError(TAG, ex);
            }
            catch (Exception ex)
            {
                LogQueue.Instance.AddError(TAG, ex);
            }

            return utc;
        }

        public static string UtcToLocal(DateTime utc, string tz, string format)
        {
            string[] letters = tz.Split(' ');
            string timeZone = string.Empty;
            DateTime dt = UtcToLocal(utc, tz);

            foreach (var l in letters)
            {
                timeZone += l[0];
            }

            return dt.ToString(format) + " " + timeZone;
        }
    }
}
