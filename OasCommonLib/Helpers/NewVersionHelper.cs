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

        private static readonly LogQueue _log = LogQueue.Instance;

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

            Error = string.Empty;
            try
            {
                long currentAppVersion = GetVersion(currentVersion);
                string serverVersion = GetServerFileVersion(productName);
                long installerVersion = GetVersion(serverVersion);

                _log.Add(
                    TAG,
                    string.Format("local binary version:{0}, remote binary version:{1}",
                        currentVersion,
                        serverVersion),
                    LogItemType.Info);

                if (0 != installerVersion && 0 != currentAppVersion && installerVersion > currentAppVersion)
                {
                    res = true;
                }
            }
            catch (Exception ex)
            {
                _log.AddError(TAG, ex, "IsNewVersionAvailable failed");
            }

            return res;
        }

        private static string GetServerFileVersion(string productName)
        {
            string json = string.Empty;
            string installerVersion = "unknown";
            string tmpFile = Path.GetTempFileName();

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
                }
            }
            catch (JsonReaderException jre)
            {
                SaveError(jre.Message);
                _log.AddError(
                           TAG,
                           jre,
                           string.Format("json parser error: '{0}' ", json));
            }
            catch (Exception ex)
            {
                SaveError(ex.Message + Environment.NewLine + ex.StackTrace);
                _log.AddError(TAG, ex);
            }

            try
            {
                File.Delete(tmpFile);
            }
            catch { }

            return installerVersion;
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

        public static string Error { get; private set; }

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
            string stepComment = string.Empty;
            string downloadUrl = NewVersionHelper.InstallerLocation;
            string newArchive = Path.Combine(_downloadPath, Path.GetFileName(_archiveName));

            try
            {
                using (WebClient wc = new WebClient())
                {
                    step = 1;

                    if (File.Exists(newArchive))
                    {
                        _log.Add(TAG, stepComment = string.Format("going to delete '{0}'", newArchive), LogItemType.Info);
                        File.Delete(newArchive);
                    }

                    step = 2;

                    _log.Add(TAG, stepComment = string.Format("going to create '{0}'", _downloadPath), LogItemType.Info);
                    if (!FileHelper.CreateDirectoryRecursively(_downloadPath))
                    {
                        Error = string.Format("cannot create temporary directory '{0}' : {1}", _downloadPath, FileHelper.LastError);
                        return res;
                    }

                    step = 3;

                    _log.Add(TAG, stepComment = string.Format("downloading {0} to {1}", downloadUrl, newArchive), LogItemType.Info);
                    wc.DownloadFile(downloadUrl, newArchive);
                }

                step = 4;

                long archiveLength = FileHelper.Length(newArchive);
                if (File.Exists(newArchive) && archiveLength > FileHelper.MinimalLength)
                {
                    _log.Add(TAG, stepComment = string.Format("downloaded '{0}' with size {1}", newArchive, archiveLength), LogItemType.Info);
                    res = true;
                }
                else
                {
                    Error = "download new version failed. zero file length : " + Path.GetFileName(newArchive);
                }

                step = 5;

            }
            catch (Exception ex)
            {
                Error = "update download failed. error : " + ex.Message + ". step " + step + stepComment;
                _log.AddError(TAG, ex, "update download failed. step : " + step + stepComment);
            }

            return res;
        }

        private static bool UnpackNewVersion()
        {
            bool res = false;
            string source = Path.Combine(_downloadPath, _archiveName);
            string destination = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
            int step = 0;
            string stepComment = string.Empty;

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
                    Error = string.Format("cannot create tempotrary directory '{0}': {1}", destination, FileHelper.LastError);
                    return res;
                }

                step = 3;
                RarArchive archive = RarArchive.Open(source);

                step = 4;
                foreach (RarArchiveEntry entry in archive.Entries)
                {
                    _log.Add(TAG, "going to unpack : " + Path.GetFileName(entry.FilePath));

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
                Error = string.Format("failed to unpack update : '{0}', error : {1}, step : {2}", source, ex.Message, step);
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
