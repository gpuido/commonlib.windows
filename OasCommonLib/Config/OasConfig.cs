using OasConfig.Data;

namespace OasCommonLib.Config
{
    using Microsoft.Win32;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OasEventManager;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public sealed class CaseConfigData : IConfigData
    {
        // place where check for EMS files
        public string[] EMSCasePath;
        public int[] CasePathWatcherDelay;
        public string MCFPath;
        // catalog where we copying ENS files for local use
        public string DBFCasePath;
        public string RemovedCasePath;
        public string CaseExt;

        // place for unlinked images
        public string ImagePath;
        public string ImageExts;

        // place for linked images
        public string CaseImagePath;
        public string CaseAudioPath;

        public void ExtractData(JToken caseConfig)
        {
            JArray emsCasePath = (JArray)caseConfig["EMSCasePath"];
            if (null != emsCasePath)
            {
                EMSCasePath = new string[emsCasePath.Count];
                for (int i = 0; i < emsCasePath.Count; ++i)
                {
                    EMSCasePath[i] = emsCasePath[i].Value<string>();
                }
            }
            else
            {
                InitEmsCasePath(OasConfig.Instance.DataPath);
            }

            JArray casePathWatcherDelay = (JArray)caseConfig["CasePathWatcherDelay"];
            if (null != casePathWatcherDelay)
            {
                CasePathWatcherDelay = new int[emsCasePath.Count];
                for (int i = 0; i < casePathWatcherDelay.Count; ++i)
                {
                    CasePathWatcherDelay[i] = casePathWatcherDelay[i].Value<int>();
                }
            }
            else
            {
                InitCasePathWatcherDelay();
            }

            JToken jt = caseConfig["MCFPath"];
            if (null != jt)
            {
                MCFPath = jt.Value<string>();
            }

            jt = caseConfig["DBFCasePath"];
            if (null != jt)
            {
                DBFCasePath = jt.Value<string>();
            }

            jt = caseConfig["RemovedCasePath"];
            if (null != jt)
            {
                RemovedCasePath = jt.Value<string>();
            }

            jt = caseConfig["CaseExt"];
            if (null != jt)
            {
                CaseExt = jt.Value<string>();
            }

            jt = caseConfig["ImagePath"];
            if (null != jt)
            {
                ImagePath = jt.Value<string>();
            }

            jt = caseConfig["ImageExts"];
            if (null != jt)
            {
                ImageExts = jt.Value<string>();
            }

            jt = caseConfig["CaseImagePath"];
            if (null != jt)
            {
                CaseImagePath = jt.Value<string>();
            }

            jt = caseConfig["CaseAudioPath"];
            if (null != jt)
            {
                CaseAudioPath = jt.Value<string>();
            }
        }

        public void InitDefault(string dataPath)
        {
            InitEmsCasePath(dataPath);
            InitCasePathWatcherDelay();
            MCFPath = dataPath + @"\MCF\";

            DBFCasePath = dataPath + @"\DBF\";

            RemovedCasePath = dataPath + @"\RemovedDBF\";

            CaseExt = "*.ENV";

            ImagePath = dataPath + @"\Images\";
            ImageExts = "*.jpg;*.png;*.tif";

            CaseImagePath = dataPath + @"\CaseImages\";
            CaseAudioPath = dataPath + @"\CaseAudioNotes\";
        }

        private void InitCasePathWatcherDelay()
        {
            CasePathWatcherDelay = new int[EMSCasePath.Length];
            for (int i = 0; i < EMSCasePath.Length; ++i)
            {
                CasePathWatcherDelay[i] = OasConfig.WATCHER_DELAY;
            }
        }

        public void InitEmsCasePath(string dataPath)
        {
            EMSCasePath = new string[Estimators.List.Count];
            for (int i = 0; i < Estimators.List.Count; ++i)
            {
                var element = Estimators.List.ElementAt(i);
                EMSCasePath[i] = Path.Combine(dataPath, "EMS", Estimators.List[element.Key]);
            }
        }
    }

    public sealed class WebServiceData : IConfigData
    {
        // web service url
        public string Url;
        // encrypted login & password for auto login
        public string LoginInfo;

        public void InitDefault(string dataPath)
        {
#if DEBUG
            Url = @"http://ccpro.no-ip.org/cgi/oaservice";
#else
            Url = @"http://estvis.com/cgi-bin/oaservice.cgi";
#endif
            LoginInfo = string.Empty;
        }

        public void ExtractData(JToken webConfig)
        {
            JToken jt = webConfig["Url"];
            if (null != jt)
            {
                Url = jt.Value<string>();
            }

            jt = webConfig["LoginInfo"];
            if (null != jt)
            {
                LoginInfo = jt.Value<string>();
            }
        }
    }

    public sealed class WebServerData : IConfigData
    {
        public bool RunServer;
        public int Port;
        public List<string> Ips;
        public bool WebServerLog;

        public WebServerData()
        {
            RunServer = false;
            Port = 18080;
            Ips = new List<string>();
            WebServerLog = false;
        }

        public void InitDefault(string dataPath)
        {
        }

        public void ExtractData(JToken webServer)
        {
            JToken jt = webServer["RunServer"];
            if (null != jt)
            {
                RunServer = jt.Value<bool>();
            }

            jt = webServer["WebServerLog"];
            if (null != jt)
            {
                WebServerLog = jt.Value<bool>();
            }

            jt = webServer["Port"];
            if (null != jt)
            {
                Port = jt.Value<int>();
            }

            JArray ips = (JArray)webServer["Ips"];
            if (null != jt)
            {
                Ips = new List<string>();
                foreach (var ip in ips)
                {
                    Ips.Add(ip.Value<string>());
                }
            }
        }
    }

    public sealed class UIData : IConfigData
    {
        public int UITimeInMinutes;

        public UIData()
        {
            UITimeInMinutes = 60;
        }

        public void ExtractData(JToken data)
        {
            JToken jt = data["UITimeInMinutes"];
            if (null != jt)
            {
                UITimeInMinutes = jt.Value<int>();
                UITimeInMinutes = Math.Min(UITimeInMinutes, 60);
                UITimeInMinutes = Math.Max(UITimeInMinutes, 30);
            }
        }

        public void InitDefault(string dataPath)
        {
            UITimeInMinutes = 60; // 2 hours by default
        }
    }

    public class OasConfigData
    {
        public readonly CaseConfigData CaseConfig;
        public readonly WebServiceData WebConfig;
        public readonly WebServerData WebServer;
        public readonly UIData UiConfig;
        public string LogPath { get; set; }

        // place to store additional data
        public string DataPath { get; set; }

        // place for output images
        public string ImageExportPath { get; set; }

        // place for local storage where we're going to save data when there's not inet connection
        public string LocalStoragePath { get; set; }

        // do not set connection to server
        public bool DoNotConnect { get; set; }

        // keep session - auto login if credentials are available
        public bool AutoLogin { get; set; }

        // run export for every single page added to case
        public bool AutoExport { get; set; }

        // Miximize window to kiosk mode
        public bool MaximizeToKiosk { get; set; }

        // show/hide preconditions in detail list
        public bool ShowPreconditions { get; set; }

        // drop down list with standard pictures description
        public string[] StandardDescription { get; set; }

        // how often poll server for updates in secs;
        public int ServerPollTimeout { get; set; }

        // delay after emswatcher noticed files changes
        public int WatcherDelay { get; set; }

        public bool EncodeTraffic { get; set; }

        // 
        public bool StartAppAutomatically { get; set; }

        public string SqLiteDbPath { get; set; }

        // for how many days show data
        public int HowManyDaysToShow { get; set; }

        // show notifications in tray tray 
        public bool ShowNotification { get; set; }

        // close to tray when closed
        public bool CloseToTray { get; set; }

        // column to sort grid on
        public int SortHeader { get; set; }

        // order for column to sort on false = desc, true - asc
        public bool SortHeaderOrder { get; set; }

        public bool StampImages { get; set; }

        public string CurrentVersion { get; private set; }

        public OasConfigData(string currentVersion)
        {
            CaseConfig = new CaseConfigData();
            WebConfig = new WebServiceData();
            WebServer = new WebServerData();
            UiConfig = new UIData();

            CurrentVersion = currentVersion;

            DoNotConnect = false;
            ShowNotification = false;
            CloseToTray = true;
            SortHeaderOrder = false;
            StampImages = false;

            EncodeTraffic = false;

#if DEBUG
            LogPath = @"..\..\Log";
#else
            LogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Log";
#endif
        }
    }

    public enum OasConfigType { evcp, ev, evcl };

    public sealed class OasConfig : IDisposable
    {
        public static readonly string TAG = "OasConfig";

        private static readonly OasEventSource _oasEvent = GlobalEventManager.Instance.OasEventSource;

        public static readonly int WATCHER_DELAY = 15;

        public bool IsAdmin { get; set; }
        public OasConfigData Data { get; set; }
        public string DataFolderPath { get; private set; }
        public bool LocalClientMode { get; set; }

        #region static part
        private static OasConfig _config;
        public static OasConfig Instance
        {
            get
            {
                return _config;
            }
        }
        #endregion

        public bool IsChanged { get; private set; }

        public readonly OasConfigType OasConfigType;
        public readonly string CurrentVersion;

        public OasConfig(OasConfigType cfgType, string currentVersion)
        {
            OasConfigType = cfgType;

            CurrentVersion = currentVersion;

            IsChanged = false;
            IsAdmin = false;
            LocalClientMode = false;

            Data = new OasConfigData(currentVersion);
#if DEBUG
            DataFolderPath = @"..\..\OAS";
#else
            DataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\OAS";
#endif
            Read();

            _config = this;
        }

        #region Read/Save config
        public string ConfigFileName
        {
            get
            {
                return Path.Combine(DataFolderPath, OasConfigType.ToString() + ".conf");
            }
        }

        private void Read()
        {
            string configName = ConfigFileName;

            InitData();

            try
            {
                if (File.Exists(configName))
                {
                    //                    SaveError(configName);
                    var json = File.ReadAllText(configName);
                    //                    SaveError(json);
                    Data = ParseJson(json, CurrentVersion); //  JsonConvert.DeserializeObject<OasConfigData>(json);
                }

            }
            catch (Exception ex)
            {
                SaveError(ex);
                InitData();
            }

            if (string.IsNullOrEmpty(Data.CaseConfig.CaseImagePath))
            {
                Data.CaseConfig.CaseImagePath = DataFolderPath + @"\CaseImages\";
            }
            if (string.IsNullOrEmpty(Data.CaseConfig.CaseAudioPath))
            {
                Data.CaseConfig.CaseAudioPath = DataFolderPath + @"\CaseAudioNotes\";
            }

            if (null == Data.CaseConfig.EMSCasePath || 0 == Data.CaseConfig.EMSCasePath.Length)
            {
                Data.CaseConfig.InitEmsCasePath(DataFolderPath);
            }

            if (string.IsNullOrEmpty(Data.CaseConfig.MCFPath))
            {
                Data.CaseConfig.MCFPath = DataFolderPath + @"\MCF\";
            }

            if (string.IsNullOrEmpty(Data.CaseConfig.DBFCasePath))
            {
                Data.CaseConfig.DBFCasePath = DataFolderPath + @"\DBF\";
            }

            if (string.IsNullOrEmpty(Data.CaseConfig.RemovedCasePath))
            {
                Data.CaseConfig.RemovedCasePath = DataFolderPath + @"\RemovedDBF\";
            }

            if (string.IsNullOrEmpty(Data.SqLiteDbPath))
            {
                Data.SqLiteDbPath = DataFolderPath + @"\SqLite\";
            }

            if (0 == Data.WatcherDelay)
            {
                Data.WatcherDelay = WATCHER_DELAY;
            }

            if (0 == Data.ServerPollTimeout)
            {
#if DEBUG
                Data.ServerPollTimeout = 60; //secs
#else
                Data.ServerPollTimeout = 60 * 5; //secs
#endif
            }

            if (0 == Data.UiConfig.UITimeInMinutes)
            {
#if DEBUG
                Data.UiConfig.UITimeInMinutes = 120; //secs
#else
                Data.ServerPollTimeout = 60 * 5; //secs
#endif
            }

            if (0 == Data.HowManyDaysToShow)
            {
#if DEBUG
                Data.HowManyDaysToShow = 100; //days
#else
                Data.HowManyDaysToShow = 14; //days
#endif
            }

            CheckPath(Data.CaseConfig.ImagePath);
            CheckPath(Data.CaseConfig.CaseImagePath);
            CheckPath(Data.CaseConfig.CaseAudioPath);
            CheckPath(Data.LogPath);
            CheckPath(Data.DataPath);
            CheckPath(Data.SqLiteDbPath);
            CheckPath(Data.ImageExportPath);
            CheckPath(Data.LocalStoragePath);

            if (null != Data.CaseConfig.EMSCasePath)
            {
                foreach (var path in Data.CaseConfig.EMSCasePath)
                {
                    CheckPath(path);
                }
            }

            if (!string.IsNullOrEmpty(Data.CaseConfig.MCFPath))
            {
                CheckPath(Data.CaseConfig.MCFPath);
            }

            if (null != Data.CaseConfig.DBFCasePath)
            {
                CheckPath(Data.CaseConfig.DBFCasePath);
            }

            if (!string.IsNullOrEmpty(Data.CaseConfig.RemovedCasePath))
            {
                CheckPath(Data.CaseConfig.RemovedCasePath);
            }
        }

        public static OasConfigData ParseJson(string json, string currentVersion)
        {
            OasConfigData ocd = new OasConfigData(currentVersion);
            JObject o = JObject.Parse(json);

            JToken caseConfig = o["CaseConfig"];
            if (null != caseConfig)
            {
                ocd.CaseConfig.ExtractData(caseConfig);
            }

            JToken webConfig = o["WebConfig"];
            if (null != webConfig)
            {
                ocd.WebConfig.ExtractData(webConfig);
            }

            JToken webServer = o["WebServer"];
            if (null != webServer)
            {
                ocd.WebServer.ExtractData(webServer);
            }

            JToken UiData = o["UiConfig"];
            if (null != UiData)
            {
                ocd.UiConfig.ExtractData(UiData);
            }

            JToken jt = o["LogPath"];
            if (null != jt)
            {
                ocd.LogPath = jt.Value<string>();
            }

            jt = o["DataPath"];
            if (null != jt)
            {
                ocd.DataPath = jt.Value<string>();
            }

            jt = o["ImageExportPath"];
            if (null != jt)
            {
                ocd.ImageExportPath = jt.Value<string>();
            }

            jt = o["LocalStoragePath"];
            if (null != jt)
            {
                ocd.LocalStoragePath = jt.Value<string>();
            }

            jt = o["DoNotConnect"];
            if (null != jt)
            {
                ocd.DoNotConnect = jt.Value<bool>();
            }

            jt = o["AutoLogin"];
            if (null != jt)
            {
                ocd.AutoLogin = jt.Value<bool>();
            }

            jt = o["AutoExport"];
            if (null != jt)
            {
                ocd.AutoExport = jt.Value<bool>();
            }

            jt = o["MaximizeToKiosk"];
            if (null != jt)
            {
                ocd.MaximizeToKiosk = jt.Value<bool>();
            }

            jt = o["ShowPreconditions"];
            if (null != jt)
            {
                ocd.ShowPreconditions = jt.Value<bool>();
            }

            JArray stdDesc = (JArray)o["StandardDescription"];

            if (null != stdDesc)
            {
                ocd.StandardDescription = new string[stdDesc.Count];
                for (int i = 0; i < stdDesc.Count; ++i)
                {
                    ocd.StandardDescription[i] = stdDesc[i].Value<string>();
                }
            }

            jt = o["ServerPollTimeout"];
            if (null != jt)
            {
                ocd.ServerPollTimeout = jt.Value<int>();
            }

            jt = o["WatcherDelay"];
            if (null != jt)
            {
                ocd.WatcherDelay = jt.Value<int>();
            }

            jt = o["EncodeTraffic"];
            if (null != jt)
            {
                ocd.EncodeTraffic = jt.Value<bool>();
            }

            jt = o["StartAppAutomatically"];
            if (null != jt)
            {
                ocd.StartAppAutomatically = jt.Value<bool>();
            }

            jt = o["SqLiteDbPath"];
            if (null != jt)
            {
                ocd.SqLiteDbPath = jt.Value<string>();
            }

            jt = o["HowManyDaysToShow"];
            if (null != jt)
            {
                ocd.HowManyDaysToShow = jt.Value<int>();
            }

            jt = o["ShowNotification"];
            if (null != jt)
            {
                ocd.ShowNotification = jt.Value<bool>();
            }

            jt = o["CloseToTray"];
            if (null != jt)
            {
                ocd.CloseToTray = jt.Value<bool>();
            }

            jt = o["SortHeader"];
            if (null != jt)
            {
                ocd.SortHeader = jt.Value<int>();
            }

            jt = o["SortHeaderOrder"];
            if (null != jt)
            {
                ocd.SortHeaderOrder = jt.Value<bool>();
            }

            jt = o["StampImages"];
            if (null != jt)
            {
                ocd.StampImages = jt.Value<bool>();
            }

            return ocd;
        }

        private void InitData()
        {
            Data.CaseConfig.InitDefault(DataFolderPath);
            Data.WebConfig.InitDefault(DataFolderPath);

            Data.LogPath = DataFolderPath + @"\Logs\";

            Data.DataPath = DataFolderPath + @"\CaseData\";

            Data.SqLiteDbPath = DataFolderPath + @"\SqLite\";

            Data.ImageExportPath = DataFolderPath + @"\Result\";

            Data.LocalStoragePath = DataFolderPath + @"\LocalData\";

            Data.AutoLogin = true;
            Data.AutoExport = true;

            Data.StandardDescription = new string[] { "Dirt in paint", "Scratches", "Dents", "Sand marks", "Bug damage", "Damaged", "Missing", "Chips", "Color" };

            Data.MaximizeToKiosk = false;
            Data.ShowPreconditions = true;
            Data.ShowNotification = false;
            Data.CloseToTray = true;
            Data.SortHeader = 7;

            Data.UiConfig.UITimeInMinutes = 120;

#if DEBUG
            Data.HowManyDaysToShow = OasConfigType == OasConfigType.evcp ? 1 : 100;
            Data.ServerPollTimeout = 60; //secs
#else
            Data.HowManyDaysToShow = OasConfigType == OasConfigType.evcp ? 1 : 14;
            Data.ServerPollTimeout = 60 * 5; //secs
#endif

        }

        private static void SaveError(string error)
        {
            string fileName = Path.Combine(
#if DEBUG
            @"..\..\OAS\Logs"
#else
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\OAS\Logs"
#endif

                , "config.log");
            File.AppendAllText(fileName, error + Environment.NewLine);
        }

        private static void SaveError(Exception ex)
        {
            string error = string.Format("error in oasConfig : '{0}'\nstack : {1}\n", ex.Message, ex.StackTrace);
            string fileName = Path.Combine(
#if DEBUG
            @"..\..\OAS\Logs"
#else
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\OAS\Logs"
#endif

                , "config.log");
            File.AppendAllText(fileName, error);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            string fileName = ConfigFileName;

            File.WriteAllText(fileName, json);
            //            _logQueue.Add(_moduleName, "config was saved");
        }
        #endregion

        #region access to variables

        public string DBFCasePath
        {
            get
            {
                return Data.CaseConfig.DBFCasePath;
            }
            set
            {
                if (!Data.CaseConfig.DBFCasePath.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                {
                    Data.CaseConfig.DBFCasePath = value;
                    CheckPath(value);
                }
            }
        }

        public string RemovedCasePath
        {
            get
            {
                return Data.CaseConfig.RemovedCasePath;
            }
            set
            {
                if (!Data.CaseConfig.RemovedCasePath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.CaseConfig.RemovedCasePath = value;
                    CheckPath(Data.CaseConfig.RemovedCasePath);
                    _oasEvent.RaiseEvent(OasEventType.CasePathCahnged);
                }
            }
        }

        #region EMS
        public string[] EMSCasePath
        {
            get
            {
                return Data.CaseConfig.EMSCasePath;
            }
        }
        public string GetEmsCasePath(EstimatorEnum est)
        {
            int index = (int)est;
            if (index >= Data.CaseConfig.EMSCasePath.Length)
            {
                throw new ArgumentException("index is greater then array length : " + index);
            }

            return Data.CaseConfig.EMSCasePath[index];
        }

        public int GetCasePathWatcherDelay(EstimatorEnum est)
        {
            int index = (int)est;
            if (index >= Data.CaseConfig.CasePathWatcherDelay.Length)
            {
                throw new ArgumentException("index is greater then array length : " + index);
            }

            return Data.CaseConfig.CasePathWatcherDelay[index];
        }

        public void SetEmsCasePath(EstimatorEnum est, string path)
        {
            int index = (int)est;
            if (index >= Data.CaseConfig.EMSCasePath.Length)
            {
                throw new ArgumentException("index is greater then path array");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("new path is empty");
            }

            if (!Data.CaseConfig.EMSCasePath[index].Equals(path, StringComparison.CurrentCultureIgnoreCase))
            {
                Data.CaseConfig.EMSCasePath[index] = path;
                CheckPath(Data.CaseConfig.EMSCasePath[index]);
                _oasEvent.RaiseEvent(OasEventType.CasePathCahnged);
            }
        }

        public void SetCasePathWatcherDelay(EstimatorEnum est, int delay)
        {
            int index = (int)est;
            if (index >= Data.CaseConfig.CasePathWatcherDelay.Length)
            {
                throw new ArgumentException("index is greater then path array");
            }

            if (delay <= 0)
            {
                throw new ArgumentException("delay can not be zero or negative");
            }

            if (Data.CaseConfig.CasePathWatcherDelay[index] != delay)
            {
                Data.CaseConfig.CasePathWatcherDelay[index] = delay;
                _oasEvent.RaiseEvent(OasEventType.WatcherDelayChanged, est);
            }
        }
        #endregion

        public string MCFPath
        {
            get
            {
                return Data.CaseConfig.MCFPath;
            }
            set
            {
                if (!Data.CaseConfig.MCFPath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.CaseConfig.MCFPath = value;
                    CheckPath(Data.CaseConfig.MCFPath);
                    _oasEvent.RaiseEvent(OasEventType.MCFPathCahnged);
                }
            }
        }

        public string CaseExt
        {
            get
            {
                return Data.CaseConfig.CaseExt;
            }
            set
            {
                if (!Data.CaseConfig.CaseExt.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.CaseConfig.CaseExt = value.ToLower();
                    _oasEvent.RaiseEvent(OasEventType.CasePathCahnged);
                }
            }
        }

        public string ImagePath
        {
            get
            {
                return Data.CaseConfig.ImagePath;
            }
            set
            {
                if (!Data.CaseConfig.ImagePath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.CaseConfig.ImagePath = value;
                    CheckPath(Data.CaseConfig.ImagePath);
                    _oasEvent.RaiseEvent(OasEventType.ImagePathCahnged);
                }
            }
        }

        public string CaseAudioPath
        {
            get
            {
                return Data.CaseConfig.CaseAudioPath;
            }
            set
            {
                if (!Data.CaseConfig.CaseAudioPath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.CaseConfig.CaseAudioPath = value;
                    CheckPath(Data.CaseConfig.CaseAudioPath);
                }
            }
        }

        public string CaseImagePath
        {
            get
            {
                return Data.CaseConfig.CaseImagePath;
            }
            set
            {
                if (!Data.CaseConfig.CaseImagePath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.CaseConfig.CaseImagePath = value;
                    CheckPath(Data.CaseConfig.CaseImagePath);
                }
            }
        }

        public string ImageExts
        {
            get
            {
                return Data.CaseConfig.ImageExts;
            }
            set
            {
                if (!Data.CaseConfig.ImageExts.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.CaseConfig.ImageExts = value;
                    _oasEvent.RaiseEvent(OasEventType.ImagePathCahnged);
                }
            }
        }

        public string LoginInfo
        {
            get
            {
                return Data.WebConfig.LoginInfo;
            }
            set
            {
                if (null == Data.WebConfig.LoginInfo ||
                    !Data.WebConfig.LoginInfo.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.WebConfig.LoginInfo = value;
                    Save();
                }
            }
        }

        public string DataServiceUrl
        {
            get
            {
                return Data.WebConfig.Url;
            }
            set
            {
                if (null == Data.WebConfig.Url ||
                    !Data.WebConfig.Url.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.WebConfig.Url = value;
                    _oasEvent.RaiseEvent(OasEventType.UrlChanged);
                    Save();
                }
            }
        }

        public string LogPath
        {
            get
            {
                return Data.LogPath;
            }
            set
            {
                if (!Data.LogPath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.LogPath = value;
                    CheckPath(Data.LogPath);
                    _oasEvent.RaiseEvent(OasEventType.LogPathChanged);
                }
            }
        }

        public string DataPath
        {
            get
            {
                return Data.DataPath;
            }
            set
            {
                if (!Data.DataPath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.DataPath = value;
                    CheckPath(Data.DataPath);
                    _oasEvent.RaiseEvent(OasEventType.DataPathChanged);
                }
            }
        }

        public string SqLiteDbPath
        {
            get
            {
                return Data.SqLiteDbPath;
            }
            set
            {
                if (!Data.SqLiteDbPath.Equals(value, StringComparison.Ordinal))
                {
                    Data.SqLiteDbPath = value;
                    CheckPath(Data.SqLiteDbPath);
                    _oasEvent.RaiseEvent(OasEventType.SqLitePathChanged);
                }
            }
        }

        public int SortHeader
        {
            get
            {
                return Data.SortHeader;
            }
            set
            {
                Data.SortHeader = value;
            }
        }

        public bool SortHeaderOrder
        {
            get
            {
                return Data.SortHeaderOrder;
            }
            set
            {
                Data.SortHeaderOrder = value;
            }
        }

        public bool CloseToTray
        {
            get
            {
                return Data.CloseToTray;
            }
            set
            {
                Data.CloseToTray = value;
            }
        }

        public bool ShowNotification
        {
            get
            {
                return Data.ShowNotification;
            }
            set
            {
                Data.ShowNotification = value;
            }
        }

        public int HowManyDaysToShow
        {
            get
            {
                return Data.HowManyDaysToShow;
            }
            set
            {
                if (Data.HowManyDaysToShow != value)
                {
                    Data.HowManyDaysToShow = value;
                    CheckPath(Data.SqLiteDbPath);
                    _oasEvent.RaiseEvent(OasEventType.HowManyDaysToShowChanged);
                }
            }
        }

        public string LocalStoragePath
        {
            get
            {
                return Data.LocalStoragePath;
            }
            set
            {
                if (!Data.LocalStoragePath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.LocalStoragePath = value;
                    CheckPath(Data.LocalStoragePath);
                }
            }
        }

        public bool DoNotConnect
        {
            get
            {
                return Data.DoNotConnect;
            }
            set
            {
                Data.DoNotConnect = value;
            }
        }

        public bool AutoLogin
        {
            get
            {
                return Data.AutoLogin;
            }
            set
            {
                Data.AutoLogin = value;
            }
        }

        public bool AutoExport
        {
            get
            {
                return Data.AutoExport;
            }
            set
            {
                Data.AutoExport = value;
            }
        }

        public bool MaximizeToKiosk
        {
            get
            {
                return Data.MaximizeToKiosk;
            }
            set
            {
                if (Data.MaximizeToKiosk != value)
                {
                    IsChanged = true;
                }
                Data.MaximizeToKiosk = value;
            }
        }

        public bool ShowPreconditions
        {
            get
            {
                return Data.ShowPreconditions;
            }
            set
            {
                if (Data.ShowPreconditions != value)
                {
                    IsChanged = true;
                }
                Data.ShowPreconditions = value;
            }
        }

        public int UITimeInMinutes
        {
            get
            {
                return Data.UiConfig.UITimeInMinutes;
            }
            set
            {
                if (Data.UiConfig.UITimeInMinutes != value)
                {
                    IsChanged = true;
                }
                Data.UiConfig.UITimeInMinutes = value;
            }
        }

        public string ImageExportPath
        {
            get
            {
                return Data.ImageExportPath;
            }
            set
            {
                if (!Data.ImageExportPath.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    Data.ImageExportPath = value;
                    CheckPath(Data.ImageExportPath);
                }
            }
        }

        public string[] StandardDescription
        {
            get
            {
                return Data.StandardDescription;
            }
            set
            {
                if (!String.Join("", Data.StandardDescription).Equals(String.Join("", value)))
                {
                    Data.StandardDescription = value;
                }
            }
        }

        public int WatcherDelay
        {
            get
            {
                return Data.WatcherDelay;
            }
            set
            {
                if (Data.WatcherDelay != value)
                {
                    Data.WatcherDelay = value;
                }
            }
        }

        public bool EncodeTraffic
        {
            get
            {
                return Data.EncodeTraffic;
            }
            set
            {
                if (Data.EncodeTraffic != value)
                {
                    Data.EncodeTraffic = value;
                }
            }
        }

        public int ServerPollTimeout
        {
            get
            {
                return Data.ServerPollTimeout;
            }
            set
            {
                if (Data.ServerPollTimeout != value)
                {
                    Data.ServerPollTimeout = value;
                    _oasEvent.RaiseEvent(OasEventType.ServerPollTimeoutChanged);
                }
            }
        }

        public bool StartAppAutomatically
        {
            get
            {
                return Data.StartAppAutomatically;
            }
            set
            {
                if (Data.StartAppAutomatically != value)
                {
                    Data.StartAppAutomatically = value;
                    if (value)
                    {
                        string appPath;

                        appPath = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, Process.GetCurrentProcess().ProcessName);

                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                                "EstVis.net",
                                appPath);
                    }
                    else
                    {
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                                "EstVis.net",
                                "");
                    }
                }
            }
        }

        public bool StartWebServer
        {
            get
            {
                return Data.WebServer.RunServer;
            }
            set
            {
                if (value != Data.WebServer.RunServer)
                {
                    Data.WebServer.RunServer = value;
                    _oasEvent.RaiseEvent(OasEventType.WebServerChanged);
                }
            }
        }

        public int WebServerPort
        {
            get
            {
                return Data.WebServer.Port;
            }
            set
            {
                if (value != Data.WebServer.Port)
                {
                    Data.WebServer.Port = value;
                    if (Data.WebServer.RunServer)
                    {
                        _oasEvent.RaiseEvent(OasEventType.WebServerChanged);
                    }
                }
            }
        }

        public bool WebServerLog
        {
            get
            {
                return Data.WebServer.WebServerLog;
            }
            set
            {
                if (value != Data.WebServer.WebServerLog)
                {
                    Data.WebServer.WebServerLog = value;
                    if (Data.WebServer.RunServer)
                    {
                        _oasEvent.RaiseEvent(OasEventType.WebServerChanged);
                    }
                }
            }
        }

        public List<string> Ips
        {
            get
            {
                return Data.WebServer.Ips;
            }
            set
            {
                if (value != Data.WebServer.Ips)
                {
                    Data.WebServer.Ips = value;
                    if (Data.WebServer.RunServer)
                    {
                        _oasEvent.RaiseEvent(OasEventType.WebServerChanged);
                    }
                }
            }
        }

        public bool StampImages
        {
            get
            {
                return Data.StampImages;
            }
        }
        #endregion

        public static void CheckPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                char letter = path[0];
                if ((letter >= 'a' && letter <= 'z') || (letter >= 'A' && letter <= 'Z'))
                {
                    DriveInfo di = new DriveInfo(path.Substring(0, 3));
                    if (di.DriveType == DriveType.Network && !di.IsReady)
                    {
                        string error = string.Format("it looks like network path '{0}' is not connected", path);
                        //                        LogQueue.Instance.Add(_moduleName, error, LogItemType.Error);
                    }
                }
            }
            //if (!DirectoryHelper.NetworkPathAccessable(path))
            //{
            //    string error = string.Format("it looks like network path '{0}' is not connected", path);

            //    _logQueue.Add(_moduleName, error, LogItemType.Error);
            //    throw new IOException(error);
            //}

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    var error = string.Format("oasconfig : error during directory creation '{0}'", path);
                    Debug.WriteLine(error);
                    SaveError(ex);
                    //LogQueue.Instance.AddError(
                    //    _moduleName,
                    //    ex,
                    //    string.Format("Error during directory creation '{0}'", path));
                }
            }
        }

        public void Dispose()
        {
            if (IsChanged)
            {
                Save();
            }
        }
    }
}
