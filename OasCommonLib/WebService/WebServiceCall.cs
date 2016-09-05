using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OasCommonLib.Data;
using OasCommonLib.Helpers;
using OasCommonLib.Logger;
using OasCommonLib.OasEventManager;
using OasCommonLib.Session;
using OasCommonLib.VinParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace OasCommonLib.WebService
{
    public class WebServiceCall
    {
        public static readonly string TAG = "WebServiceCall";

        public static string LastError { get; private set; }

        private static readonly OasEventSource _oasEvent = GlobalEventManager.Instance.OasEventSource;
        private static readonly LogQueue _log = LogQueue.Instance;
        private static readonly Config.OasConfig _cfg = Config.OasConfig.Instance;

        public static string ClientInfo { get; set; }

        public static string CookieDomain { get; private set; }

        public static bool Ping(string testUrl, out ServerInfo serverInfo)
        {
            bool result = false;
            string responsebody;
            string url;

            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            serverInfo = new ServerInfo();
            LastError = string.Empty;

#if UNDECODED
            postParameters.Add("action", "ping");
            postParameters.Add("client", VersionHelper.ClientInfo);
#else
            string data = string.Format("action=ping&client={0}", ClientInfo);
            postParameters.Add("_d", CoderHelper.Encode(data));
#endif

            if (!string.IsNullOrEmpty(testUrl))
            {
                url = testUrl;
            }
            else
            {
                url = _cfg.DataServiceUrl;
            }

            try
            {
                // Create request and receive response
                string userAgent = "estvis";
                using (HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(url, userAgent, postParameters))
                {
                    // Process response
                    using (StreamReader responseReader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        responsebody = responseReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
                _log.AddError(TAG, ex, "'Ping'");
                responsebody = WebServiceCall.ErrorResponse(ex);
                LastError = "ping failed";
            }

            try
            {
                JObject jObj = JObject.Parse(responsebody);

                if (null != jObj["_d"])
                {
                    string encodedResponse = jObj["_d"].Value<string>();
                    responsebody = CoderHelper.Decode(encodedResponse);

                    jObj = JObject.Parse(responsebody);
                }

                if (null != jObj["result"])
                {
                    var ok = jObj["result"].Value<string>();
                    if (ok.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        serverInfo.ServerVersion = jObj["version"].Value<string>();
                        serverInfo.ServerType = ServerTypeEnum.web;
                        if (null != jObj["server_type"])
                        {
                            var server_type = jObj["server_type"].Value<string>();

                            if (null != server_type)
                            {
                                if ("web".Equals(server_type))
                                {
                                    serverInfo.ServerType = ServerTypeEnum.web;
                                }
                                else if ("local".Equals(server_type))
                                {
                                    serverInfo.ServerType = ServerTypeEnum.local;
                                }
                            }
                        }
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
                _log.AddError(TAG, ex, "error during parsing ping response");
                LastError = "ping failed";
            }

            return result;
        }

        public static SessionInfo Login(string login, string passwd, bool saveSession)
        {
            SessionInfo si = null;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            string session = string.Empty;
            string DataServiceUrl = _cfg.DataServiceUrl;

#if UNDECODED
            reqparm.Add("action", "login");
            reqparm.Add("client", ClientInfo);
            reqparm.Add("login", login);
            reqparm.Add("passwd", passwd);
#else
            string data = string.Format("action=login&login={0}&passwd={1}&client={2}", login, passwd, ClientInfo);
            reqparm.Add("_d", CoderHelper.Encode(data));
#endif
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(DataServiceUrl, "POST", reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }

                var cookie = cookies.List().FirstOrDefault((it) => it.Name.Equals("session"));
                CookieDomain = cookie.Domain;
                session = cookie.Value;
            }
            catch (WebException ex)
            {
                _log.AddError(TAG, ex);
                responsebody = ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                _log.AddError(TAG, ex);
                responsebody = ErrorResponse(ex);
            }

            JObject jObj = JObject.Parse(responsebody);

            if (null != jObj["_d"])
            {
                string encodedResponse = jObj["_d"].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj["error"])
            {
                _log.Add(TAG, jObj["error"].Value<string>(), LogItemType.Error);
                return si;
            }
            if (null != jObj["result"])
            {
                var result = jObj["result"];

                string[] roles = result["roles"].Value<string>().Split(',');

                si = SessionInfo.Instance;

                int companyId = result["company_id"].Value<int>();
                string companyName = result["company_name"].Value<string>();
                string companyAbbr = result["company_abbr"].Value<string>();
                int userId = result["user_id"].Value<int>();
                string userName = result["user_name"].Value<string>();
                string companyRole = result["company_role"].Value<string>();

                if (saveSession)
                {
                    si.SetSessionInfo(session, companyId, companyName, companyAbbr, userId, userName, login, roles, companyRole);
                }
            }

            return si;
        }

        public static bool Login(string login, string passwd, bool saveSession, out string session, out string json)
        {
            bool result = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();

            json = string.Empty;
            session = string.Empty;

#if UNDECODED
            reqparm.Add("action", "login");
            reqparm.Add("client", ClientInfo);
            reqparm.Add("login", login);
            reqparm.Add("passwd", passwd);
#else
            string data = string.Format("action=login&login={0}&passwd={1}&client={2}", login, passwd, ClientInfo);
            reqparm.Add("_d", CoderHelper.Encode(data));
#endif

            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, "POST", reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }

                var cookie = cookies.List().FirstOrDefault((it) => it.Name.Equals("session"));
                CookieDomain = cookie.Domain;
                session = cookie.Value;
            }
            catch (WebException ex)
            {
                LastError = "login failed.error : " + ex.Message;
                _oasEvent.RaiseEvent(OasEventType.ErrorMessage, LastError);
                _log.AddError(TAG, ex, LastError);

                responsebody = WebServiceCall.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                LastError = "login failed.error : " + ex.Message;
                _oasEvent.RaiseEvent(OasEventType.ErrorMessage, LastError);
                _log.AddError(TAG, ex, LastError);
                responsebody = WebServiceCall.ErrorResponse(ex);
            }

            JObject jObj = JObject.Parse(responsebody);

            if (null != jObj["_d"])
            {
                string encodedResponse = jObj["_d"].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj["error"])
            {
                LastError = jObj["error"].Value<string>();
                _log.Add(TAG, LastError, LogItemType.Error);
            }
            if (null != jObj["result"])
            {
                json = responsebody;
                result = true;
            }

            return result;
        }

        #region vin info reader/parse/update
        public static bool EdmundsVinInfo(string vin, out VinInfo vinInfo)
        {
            bool res = false;
            string responsebody = string.Empty;
            string url = "http://api.edmunds.com/v1/api/toolsrepository/vindecoder?vin={0}&fmt=json&api_key={1}";
            string edmundsApiKey = "cbaqfbt2kvb2xpcjwsv99h3q";
            string getUrl = string.Format(url, vin, edmundsApiKey);

            LastError = string.Empty;
            vinInfo = null;

            try
            {
                responsebody = (new WebClient()).DownloadString(getUrl);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                responsebody = ErrorResponse(ex);
                _log.AddError(TAG, ex);
            }

            EdmundsJsonVinParser parser = new EdmundsJsonVinParser(vin);
            vinInfo = parser.Parse(responsebody);

            if (null != vinInfo)
            {
                res = true;
            }
            else
            {
                LastError = parser.LastError;
                _log.Add(TAG, LastError, LogItemType.Error);
            }

            return res;
        }

        public static bool NHTSAVinInfo(string vin, out VinInfo vinInfo)
        {
            bool res = false;
            string responsebody = string.Empty;
            string url = "http://vpic.nhtsa.dot.gov/api/vehicles/decodevin/{0}?format=json";
            string getUrl = string.Format(url, vin);

            LastError = "";
            vinInfo = null;

            try
            {
                responsebody = (new WebClient()).DownloadString(getUrl);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                responsebody = ErrorResponse(ex);
                _log.AddError(TAG, ex);
            }

            NHTSAJsonVinParser parser = new NHTSAJsonVinParser(vin);
            vinInfo = parser.Parse(responsebody);

            if (null != vinInfo)
            {
                res = true;
            }
            else
            {
                LastError = parser.LastError;
                _log.Add(TAG, LastError, LogItemType.Error);
            }

            return res;
        }

        public static bool OasVinInfo(string vin, out VinInfo vinInfo)
        {
            bool res = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();

            LastError = "";
            vinInfo = null;

            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   string.Format("no session info found in 'read_addinfo'"),
                   LogItemType.Error);

                return res;
            }


#if UNDECODED
            reqparm.Add("action", "read_vin_info");
            reqparm.Add("client", ClientInfo);
            reqparm.Add("vin", vin);
#else
            string data = string.Format("action=read_vin_info&vin={0}&client={1}", vin, ClientInfo);
            reqparm.Add("_d", CoderHelper.Encode(data));
#endif

            cc.Add(new Cookie("session", sessionInfo.SessionId, "/", CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, "POST", reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                responsebody = ErrorResponse(ex);
                _log.AddError(TAG, ex);
            }

            JToken jObj = JObject.Parse(responsebody);
            if (null != jObj["_d"])
            {
                string encodedResponse = jObj["_d"].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);
            }

            OasJsonVinParser parser = new OasJsonVinParser(vin);
            vinInfo = parser.Parse(responsebody);

            if (null != vinInfo)
            {
                res = true;
            }
            else
            {
                LastError = parser.LastError;
                _log.Add(TAG, LastError, LogItemType.Error);
            }

            return res;
        }

        public static bool SaveVinInfo(VinInfo vinInfo)
        {
            bool res = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();

            LastError = "";

            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   string.Format("no session info found in 'read_addinfo'"),
                   LogItemType.Error);

                return res;
            }

#if UNDECODED
            reqparm.Add("action", "save_vin_info");
            reqparm.Add("client", ClientInfo);
            reqparm.Add("vin", vinInfo.Vin);
            reqparm.Add("year", vinInfo.Year.ToString());
            reqparm.Add("make", vinInfo.Make);
            reqparm.Add("model", vinInfo.Model);
#else
            string data = string.Format("action=save_vin_info&client={0}&vin={1}&year={2}&make={3}&model={4}", ClientInfo, vinInfo.Vin, vinInfo.Year, vinInfo.Make, vinInfo.Model);
            reqparm.Add("_d", CoderHelper.Encode(data));
#endif

            cc.Add(new Cookie("session", sessionInfo.SessionId, "/", CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, "POST", reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                responsebody = ErrorResponse(ex);
                _log.AddError(TAG, ex);
            }

            JObject jObj = JObject.Parse(responsebody);

            if (null != jObj["_d"])
            {
                string encodedResponse = jObj["_d"].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj["error"])
            {
                LastError = jObj["error"].Value<string>();
                _log.Add(TAG, jObj["error"].Value<string>(), LogItemType.Error);
                return res;
            }
            if (null != jObj["result"])
            {
                var result = jObj["result"].Value<string>();

                if ("ok".Equals(result))
                {
                    res = true;
                }
            }

            return res;
        }

        public static bool UpdateCaseInfo(VinInfo vinInfo)
        {
            bool res = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();

            LastError = "";

            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   string.Format("no session info found in 'read_addinfo'"),
                   LogItemType.Error);

                return res;
            }

#if UNDECODED
            reqparm.Add("action", "update_vin_info");
            reqparm.Add("client", ClientInfo);
            reqparm.Add("vin", vinInfo.Vin);
            reqparm.Add("year", vinInfo.Year.ToString());
            reqparm.Add("make", vinInfo.Make);
            reqparm.Add("model", vinInfo.Model);
#else
            string data = string.Format("action=update_vin_info&client={0}&vin={1}&year={2}&make={3}&model={4}", ClientInfo, vinInfo.Vin, vinInfo.Year, vinInfo.Make, vinInfo.Model);
            reqparm.Add("_d", CoderHelper.Encode(data));
#endif

            cc.Add(new Cookie("session", sessionInfo.SessionId, "/", CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, "POST", reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                responsebody = ErrorResponse(ex);
                _log.AddError(TAG, ex);
            }

            JObject jObj = JObject.Parse(responsebody);

            if (null != jObj["_d"])
            {
                string encodedResponse = jObj["_d"].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj["error"])
            {
                LastError = jObj["error"].Value<string>();
                _log.Add(TAG, jObj["error"].Value<string>(), LogItemType.Error);
                return res;
            }
            if (null != jObj["result"])
            {
                var result = jObj["result"].Value<int>();
                res = result > 0;
            }

            return res;
        }

        public static bool ReadVinInfo(string vin, out VinInfo vinInfo)
        {
            bool res = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();

            LastError = "";
            vinInfo = null;

            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   string.Format("no session info found in 'read_addinfo'"),
                   LogItemType.Error);

                return res;
            }

#if UNDECODED
            reqparm.Add("action", "read_vin_info");
            reqparm.Add("client", ClientInfo);
            reqparm.Add("vin", vin);
#else
            string data = string.Format("action=read_vin_info&client={0}&vin={1}", ClientInfo, vin);
            reqparm.Add("_d", CoderHelper.Encode(data));
#endif


            cc.Add(new Cookie("session", sessionInfo.SessionId, "/", WebServiceCall.CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, "POST", reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                responsebody = WebServiceCall.ErrorResponse(ex);
                _log.AddError(TAG, ex);
            }

            try
            {
                JObject jObj = JObject.Parse(responsebody);

                if (null != jObj["_d"])
                {
                    string encodedResponse = jObj["_d"].Value<string>();
                    responsebody = CoderHelper.Decode(encodedResponse);

                    jObj = JObject.Parse(responsebody);
                }

                if (null != jObj["error"])
                {
                    LastError = jObj["error"].Value<string>();
                    _log.Add(TAG, jObj["error"].Value<string>(), LogItemType.Error);
                    return res;
                }
                if (null != jObj["result"])
                {
                    var result = jObj["result"];

                    VinInfo vi = new VinInfo();
                    vi.Vin = vin;
                    vi.Make = result["make"].Value<string>();
                    vi.Model = result["model"].Value<string>();
                    vi.Year = result["year"].Value<int>();

                    vinInfo = vi;
                    res = true;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                _log.AddError(TAG, ex);
            }

            return res;
        }
        #endregion

        #region download data files
        public static bool DownloadPrecondition(long envelopeId, long precNumber, string imageName, string detailImageName)
        {
            Debug.Assert(envelopeId > 0);
            //
            // todo: call directly download 'dp'
            //
            return DownloadAdditionalInfoImage(-envelopeId, precNumber, imageName, detailImageName);
        }

        public static bool DownloadAdditionalInfoImage(long envelopeId, long dbRef, string imageName, string filePath, string downloadAction = "dl")
        {
            bool res = false;
            string downloadUrl;
            SessionInfo sessionInfo = SessionInfo.Instance;
            string DataServiceUrl = _cfg.DataServiceUrl;

            //
            // todo: uncomment after DownloadPrecondition will be revuild
            //
            //Debug.Assert(envelopeId > 0);


            if (!DataServiceUrl.EndsWith("/"))
            {
                DataServiceUrl += "/";
            }

            LastError = string.Empty;
            string requestParameters;
            if (dbRef > -1L)
            {
                requestParameters = downloadAction + "/" + envelopeId.ToString() + "/" + dbRef.ToString() + "/" + imageName;
            }
            else
            {
                requestParameters = downloadAction + "/" + envelopeId.ToString() + "/" + imageName;
            }

#if UNDECODED
            downloadUrl = DataServiceUrl + requestParameters;

#else
            downloadUrl = DataServiceUrl + "_d" + CoderHelper.Encode(requestParameters);
#endif

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(downloadUrl, filePath);
                }

                if (File.Exists(filePath) && FileHelper.Length(filePath) < FileHelper.MinimalLength)
                {
                    string text = File.ReadAllText(filePath);
                    string error = "";
                    if (!string.IsNullOrEmpty(text))
                    {
                        JObject jObj = JObject.Parse(text);

                        if (null != jObj["_d"])
                        {
                            string encodedResponse = jObj["_d"].Value<string>();
                            text = CoderHelper.Decode(encodedResponse);

                            jObj = JObject.Parse(text);
                        }
                        error = (string)jObj["error"].Value<string>();
                    }

                    if (null != error)
                    {
                        LastError = string.Format("file '{0}' download error", downloadUrl);
                        _log.Add(TAG, LastError, LogItemType.Error);
                        return res;
                    }
                }

                res = true;
            }
            catch (JsonException jre)
            {
                _log.AddError(TAG, jre);
                Debug.Fail(jre.Message + Environment.NewLine + jre.StackTrace);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                _log.AddError(
                            TAG,
                            ex,
                            string.Format("file '{0}' download error", downloadUrl));
                Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return res;
        }

        public static bool DownloadAudio(long envelopeId, string audioName, string caseAudioName)
        {
            bool res = false;
            string downloadUrl;
            SessionInfo sessionInfo = SessionInfo.Instance;
            string DataServiceUrl = _cfg.DataServiceUrl;

            if (!DataServiceUrl.EndsWith("/"))
            {
                DataServiceUrl += "/";
            }

            LastError = "";
            string requestParameters = "da/" + envelopeId.ToString() + "/" + audioName;

#if UNDECODED
            downloadUrl = DataServiceUrl + requestParameters;
#else
            downloadUrl = DataServiceUrl + "_d" + CoderHelper.Encode(requestParameters);
#endif

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(downloadUrl, caseAudioName);
                }

                if (File.Exists(caseAudioName) && FileHelper.Length(caseAudioName) < FileHelper.MinimalLength)
                {
                    string text = File.ReadAllText(caseAudioName);
                    JObject jObj = JObject.Parse(text);

                    if (null != jObj["_d"])
                    {
                        string encodedResponse = jObj["_d"].Value<string>();
                        text = CoderHelper.Decode(encodedResponse);

                        jObj = JObject.Parse(text);
                    }

                    JObject error = (JObject)jObj["error"];

                    if (null != error)
                    {
                        throw new Exception("server download failed : " + error.Value<string>());
                    }
                }

                res = true;
            }
            catch (JsonException jre)
            {
                Debug.Fail(jre.Message + Environment.NewLine + jre.StackTrace);
                _log.AddError(TAG, jre);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
                LastError = ex.Message;
                _log.AddError(
                            TAG,
                            ex,
                            string.Format("file '{0}' download error", downloadUrl));
            }
            return res;
        }
#endregion

        public static string ErrorResponse(Exception ex)
        {
            return "{\"error\":\"" + ex.Message + "\"}";
        }
    }

    public static class CookieContainerExtension
    {
        public static List<Cookie> List(this CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    new object[] { });

            foreach (var key in table.Keys)
            {

                Uri uri = null;

                var domain = key as string;

                if (domain == null)
                    continue;

                if (domain.StartsWith("."))
                {
                    domain = domain.Substring(1);
                }

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri) == false)
                    continue;

                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    cookies.Add(cookie);
                }
            }
            return cookies;
        }
    }
}
