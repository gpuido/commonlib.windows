namespace OasCommonLib.Helpers
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;

    public sealed class VersionCheckerHelper
    {
        public static string LastError { get; private set; }
        public static bool IsVersionValid(JObject o, string appName, string currentVersion, string serverVersion, string dbVersion)
        {
            bool res = false;

            LastError = String.Empty;
            if (null == o[appName])
            {
                LastError = String.Format("can not find application '{0}'", appName);
                return false;
            }
            foreach (var appVersion in o[appName])
            {
                if (null != appVersion[currentVersion])
                {
                    if (null != appVersion[currentVersion]["server"])
                    {
                        string foundRemoteVersion = appVersion[currentVersion]["server"].Value<string>();
                        long lfoundRemoteVersion = VersionToLong(foundRemoteVersion);
                        long lRemoteVersion = VersionToLong(serverVersion);

                        if (lRemoteVersion < lfoundRemoteVersion)
                        {
                            LastError = String.Format("found older server version : '{0}' insted of '{1}'", foundRemoteVersion, serverVersion);
                            break;
                        }
                        else
                        {
                            res = true;
                        }
                    }

                    if (res && null != appVersion[currentVersion]["db"])
                    {
                        string foundRemoteVersion = appVersion[currentVersion]["db"].Value<string>();
                        long lfoundRemoteVersion = VersionToLong(foundRemoteVersion);
                        long lRemoteVersion = VersionToLong(dbVersion);

                        if (lRemoteVersion < lfoundRemoteVersion)
                        {
                            LastError = String.Format("found older database version : '{0}' insted of '{1}'", foundRemoteVersion, serverVersion);
                            res = false;
                        }
                        else
                        {
                            res = true;
                        }

                        break;
                    }
                }
            }

            return res;
        }

        public static long VersionToLong(string version)
        {
            if (String.IsNullOrEmpty(version))
            {
                return 0;
            }

            long lv = 0;
            var parts = version.Split('.').Reverse();
            for (int i = 0; i < parts.Count(); ++i)
            {
                if (long.TryParse(parts.ElementAt(i), out long value))
                {
                    lv += value * (long)Math.Pow(1000, i);
                }
            }

            return lv;
        }
    }
}
