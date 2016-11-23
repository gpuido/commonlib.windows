namespace OasCommonLib.UpdateHelper
{
    using Logger;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnrar.Archive;
    using Helpers;
    using System;
    using System.IO;
    using System.Net;

    public class NewVersionHelper
    {
        public static readonly string TAG = "NewVersion";

#if DEBUG
        public static readonly string LatestVersionJson = "http://ccpro.no-ip.org/versions.json";
#else
        public static readonly string LatestVersionJson = "http://estvis.com/versions.json";
#endif
        public static string InstallerLocation
        {
            get; private set;
        }

        public static string InstallerName
        {
            get; private set;
        }

        public static bool IsNewVersionAvailable(string productName, string currentVersion)
        {
            bool res = false;

            LastError = String.Empty;
            try
            {
                long currentAppVersion = GetVersion(currentVersion);
                string serverVersion;
                var rs = GetServerFileVersion(productName, out serverVersion);
                if (!rs)
                {
                    return false;
                }
                long installerVersion = GetVersion(serverVersion);

                if (0 != installerVersion && 0 != currentAppVersion && installerVersion > currentAppVersion)
                {
                    res = true;
                }
            }
            catch (Exception ex)
            {
                LastError = "IsNewVersionAvailable failed :" + ex.Message;
            }

            return res;
        }

        private static bool GetServerFileVersion(string productName, out string installerVersion)
        {
            string json = String.Empty;
            string tmpFile = Path.GetTempFileName();
            bool res = false;

            LastError = String.Empty;
            installerVersion = "unknown";
            try
            {
                var url = LatestVersionJson + "?" + DateTime.Now.Ticks;
                using (var wc = new WebClient())
                {
                    wc.DownloadFile(url, tmpFile);
                    json = File.ReadAllText(tmpFile);
                    //var bytes = wc.DownloadData(new Uri(url));
                    //json = System.Text.Encoding.UTF8.GetString(bytes);
                }

                JObject jObj = JObject.Parse(json);

                if (null != jObj["versions"])
                {
                    var v = jObj["versions"];
                    string windowsVersion = v[productName].Value<string>();

                    installerVersion = windowsVersion;
                }

                if (null != jObj["setup_location"])
                {
                    var l = jObj["setup_location"];
                    InstallerLocation = l[productName].Value<string>();
                    InstallerName = InstallerLocation.Substring(InstallerLocation.LastIndexOf("/") + 1);

                    res = true;
                }
            }
            catch (JsonReaderException jre)
            {
                SaveError(jre.Message);
                LastError = String.Format("json parser failed: {0} on '{1}'", jre.Message, json);
            }
            catch (Exception ex)
            {
                SaveError(ex.Message + Environment.NewLine + ex.StackTrace);
                LastError = String.Format("json parser failed: {0}", ex.Message);
            }

            try
            {
                File.Delete(tmpFile);
            }
            catch { }

            return res;
        }

        public static long GetVersion(string version)
        {
            string[] parts = version.Split('.');

            if (parts.Length == 4)
            {
                long[] v = new long[4];

                for (int i = 0; i < 4; ++i)
                {

                    v[i] = long.Parse(parts[i]);
                }

                return v[0] * 100000 + v[1] * 10000 + v[2] * 1000 + v[3];
            }

            return 0L;
        }

        public static string LastError { get; private set; }

        private static string _archiveName;

        public static bool DownloadAndUnpack(string archiveName)
        {
            _archiveName = archiveName;

            if (DownloadNewVersion())
            {
                if (UnpackNewVersion())
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly string _downloadPath = Path.GetTempPath();
        //        private static readonly string _downloadPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tmp");

        private static bool DownloadNewVersion()
        {
            bool res = false;
            int step = 0;
            string stepComment = String.Empty;
            string downloadUrl = NewVersionHelper.InstallerLocation;
            string newArchive = Path.Combine(_downloadPath, Path.GetFileName(_archiveName));

            try
            {
                using (WebClient wc = new WebClient())
                {
                    step = 1;

                    if (File.Exists(newArchive))
                    {
                        File.Delete(newArchive);
                    }

                    step = 2;

                    if (!FileHelper.CreateDirectoryRecursively(_downloadPath))
                    {
                        LastError = string.Format("cannot create temporary directory '{0}' : {1}", _downloadPath, FileHelper.LastError);
                        return res;
                    }

                    step = 3;

                    wc.DownloadFile(downloadUrl, newArchive);
                }

                step = 4;

                if (FileHelper.Exists(newArchive))
                {
                    res = true;
                }
                else
                {
                    LastError = "download new version failed. zero file length : " + Path.GetFileName(newArchive);
                }

                step = 5;

            }
            catch (Exception ex)
            {
                LastError = "update download failed. error : " + ex.Message + ". step " + step + stepComment;
            }

            return res;
        }

        private static bool UnpackNewVersion()
        {
            bool res = false;
            string source = Path.Combine(_downloadPath, _archiveName);
            string destination = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
            int step = 0;
            string stepComment = String.Empty;

            try
            {
                step = 1;
                stepComment = "destination is : " + destination;
                if (Directory.Exists(destination))
                {
                    Directory.Delete(destination, true);
                }

                step = 2;
                if (!FileHelper.CreateDirectoryRecursively(destination))
                {
                    LastError = string.Format("cannot create tempotrary directory '{0}': {1}", destination, FileHelper.LastError);
                    return res;
                }

                step = 3;
                RarArchive archive = RarArchive.Open(source);

                step = 4;
                foreach (RarArchiveEntry entry in archive.Entries)
                {
                    string folder;
                    if (Path.GetDirectoryName(entry.FilePath).EndsWith("x64"))
                    {
                        folder = Path.Combine(destination, "x64");
                    }
                    else if (Path.GetDirectoryName(entry.FilePath).EndsWith("x86"))
                    {
                        folder = Path.Combine(destination, "x86");
                    }
                    else
                    {
                        folder = destination;
                    }

                    if (!Directory.Exists(folder))
                    {
                        FileHelper.CreateDirectoryRecursively(folder);
                    }

                    string path = Path.Combine(folder, Path.GetFileName(entry.FilePath));

                    entry.WriteToFile(path, NUnrar.Common.ExtractOptions.Overwrite);
                }

                step = 5;
                archive = null;
                GC.Collect();

                step = 6;
                res = true;
            }
            catch (Exception ex)
            {
                LastError = string.Format("failed to unpack update : '{0}', error : {1}, step : {2}", source, ex.Message, step);
            }

            return res;
        }

        private static void SaveError(string error)
        {
            string fileName = Path.Combine(
#if DEBUG
            @"..\..\OAS\Logs"
#else
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\OAS\Logs"
#endif

                , "newVersionHelper.log");
            File.AppendAllText(fileName, error + Environment.NewLine);
        }

    }
}
