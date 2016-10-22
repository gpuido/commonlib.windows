using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OasCommonLib.Constants;
using OasCommonLib.Data;
using OasCommonLib.Data.Config;
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

            if (!_cfg.EncodeTraffic)
            {
                postParameters.Add(WebStringConstants.ACTION, "ping");
                postParameters.Add(WebStringConstants.CLIENT, ClientInfo);
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters("ping", ClientInfo);
                postParameters.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

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

                if (null != jObj[WebStringConstants.ENC_DATA])
                {
                    string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                    responsebody = CoderHelper.Decode(encodedResponse);

                    jObj = JObject.Parse(responsebody);
                }

                if (null != jObj[JsonStringConstants.RESULT])
                {
                    var ok = jObj[JsonStringConstants.RESULT].Value<string>();
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

        #region images
        public static bool UploadFile(long envelopeId, long dbReference, string pathToFile, InfoTypeEnum infoType, string tz, string proof, out long uploadedId)
        {
            bool res = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();
            int uploadedSize = 0;
            string uploadType = "upload_image";
            string fileName = Path.GetFileName(pathToFile);

            Debug.Assert(envelopeId > 0);

            LastError = string.Empty;
            uploadedId = 0L;

            switch (infoType)
            {
                case InfoTypeEnum.AiDetail:
                    uploadType = "upload_image";
                    break;
                case InfoTypeEnum.Precondition:
                    uploadType = "upload_precondition";
                    break;
                case InfoTypeEnum.Supplement:
                    uploadType = "upload_suppliment";
                    dbReference = 0L;
                    break;
                case InfoTypeEnum.AudioNote:
                    uploadType = "upload_audio";
                    dbReference = 0L;
                    break;
                default:
                    Debug.Fail("unsupported upload type");
                    break;
            }


            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   string.Format("no session info found in 'upload_image'"),
                   LogItemType.Error);

                return res;
            }

            if (!File.Exists(pathToFile) || FileHelper.Length(pathToFile) < FileHelper.MinimalLength)
            {
                LastError = string.Format("file '{0}' doesn't exist", pathToFile);
                return res;
            }

            var nvc = new NameValueCollection();
            nvc.Add(WebStringConstants.ACTION, uploadType);
            nvc.Add(WebStringConstants.CLIENT, ClientInfo);
            nvc.Add(WebStringConstants.ENVELOPE_ID, envelopeId.ToString());
            nvc.Add(WebStringConstants.TZ, tz);
            nvc.Add(WebStringConstants.PROOF, proof);
            nvc.Add("filename", fileName);
            if (dbReference > 0)
            {
                nvc.Add(WebStringConstants.DB_REFERENCE, dbReference.ToString());
            }

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_cfg.DataServiceUrl);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = WebStringConstants.POST;
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

            if (request.CookieContainer == null)
            {
                request.CookieContainer = new CookieContainer();
            }

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", CookieDomain));
            request.CookieContainer.Add(cc);

            using (Stream rs = request.GetRequestStream())
            {
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, "file", fileName, "application/octet-stream");
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);
                using (FileStream fileStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                }
                byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
                rs.Close();
            }

            try
            {
                using (WebResponse wresp = request.GetResponse())
                {
                    using (Stream stream2 = wresp.GetResponseStream())
                    {
                        using (StreamReader reader2 = new StreamReader(stream2))
                        {
                            responsebody = reader2.ReadToEnd();
                            reader2.Close();
                        }
                        stream2.Close();
                    }
                    wresp.Close();
                }

                JObject jObj = JObject.Parse(responsebody);

                if (null != jObj[JsonStringConstants.ERROR])
                {
                    LastError = jObj[JsonStringConstants.ERROR].Value<string>();
                    _log.Add(TAG, jObj[JsonStringConstants.ERROR].Value<string>(), LogItemType.Error);
                    return res;
                }
                if (null != jObj[JsonStringConstants.RESULT])
                {
                    var result = jObj[JsonStringConstants.RESULT];

                    uploadedSize = result["received_size"].Value<int>();
                    if (uploadedSize == FileHelper.Length(pathToFile))
                    {
                        var uId = result["uploaded_id"];
                        if (null != uId)
                        {
                            uploadedId = uId.Value<long>();
                        }

                        res = true;
                    }
                    else
                    {
                        LastError = "Wrong size of uploaded files " + pathToFile;
                        _log.Add(
                            TAG,
                            "Wrong size of uploaded files " + pathToFile,
                            LogItemType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("timed out"))
                {
                    Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
                }
                LastError = ex.Message;
                _log.AddError(TAG, ex, "fileupload failed");
            }

            return res;
        }
        #endregion

        public static SessionInfo Login(string login, string passwd, bool saveSession)
        {
            SessionInfo si = null;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            string session = string.Empty;
            string DataServiceUrl = _cfg.DataServiceUrl;

            if (!_cfg.EncodeTraffic)
            {
                reqparm.Add(WebStringConstants.ACTION, "login");
                reqparm.Add(WebStringConstants.CLIENT, ClientInfo);
                reqparm.Add(WebStringConstants.LOGIN, login);
                reqparm.Add(WebStringConstants.PASSWD, passwd);
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters(WebStringConstants.LOGIN, ClientInfo, new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>(WebStringConstants.LOGIN, login),
                    new KeyValuePair<string, object>(WebStringConstants.PASSWD, passwd)
                }); 
                reqparm.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(DataServiceUrl, WebStringConstants.POST, reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }

                var cookie = cookies.List().FirstOrDefault((it) => it.Name.Equals(WebStringConstants.SESSION));
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

            if (null != jObj[WebStringConstants.ENC_DATA])
            {
                string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj[JsonStringConstants.ERROR])
            {
                _log.Add(TAG, jObj[JsonStringConstants.ERROR].Value<string>(), LogItemType.Error);
                return si;
            }
            if (null != jObj[JsonStringConstants.RESULT])
            {
                var result = jObj[JsonStringConstants.RESULT];

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

            if (!_cfg.EncodeTraffic)
            {
                reqparm.Add(WebStringConstants.ACTION, "login");
                reqparm.Add(WebStringConstants.CLIENT, ClientInfo);
                reqparm.Add(WebStringConstants.LOGIN, login);
                reqparm.Add(WebStringConstants.PASSWD, passwd);
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters(WebStringConstants.LOGIN, ClientInfo, new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>(WebStringConstants.LOGIN, login),
                    new KeyValuePair<string, object>(WebStringConstants.PASSWD, passwd)
                });
                reqparm.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }

                var cookie = cookies.List().FirstOrDefault((it) => it.Name.Equals(WebStringConstants.SESSION));
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

            if (null != jObj[WebStringConstants.ENC_DATA])
            {
                string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj[JsonStringConstants.ERROR])
            {
                LastError = jObj[JsonStringConstants.ERROR].Value<string>();
                _log.Add(TAG, LastError, LogItemType.Error);
            }
            if (null != jObj[JsonStringConstants.RESULT])
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

            LastError = string.Empty;
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


            if (!_cfg.EncodeTraffic)
            {
                reqparm.Add(WebStringConstants.ACTION, "read_vin_info");
                reqparm.Add(WebStringConstants.CLIENT, ClientInfo);
                reqparm.Add(WebStringConstants.VIN, vin);
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters("read_vin_info", ClientInfo, new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>(WebStringConstants.VIN, vin)
                });
                reqparm.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
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
            if (null != jObj[WebStringConstants.ENC_DATA])
            {
                string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
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

            LastError = string.Empty;

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

            if (!_cfg.EncodeTraffic)
            {
                reqparm.Add(WebStringConstants.ACTION, "save_vin_info");
                reqparm.Add(WebStringConstants.CLIENT, ClientInfo);
                reqparm.Add(WebStringConstants.VIN, vinInfo.Vin);
                reqparm.Add(WebStringConstants.YEAR, vinInfo.Year.ToString());
                reqparm.Add(WebStringConstants.MAKE, vinInfo.Make);
                reqparm.Add(WebStringConstants.MODEL, vinInfo.Model);
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters("save_vin_info", ClientInfo, new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>(WebStringConstants.VIN, vinInfo.Vin),
                    new KeyValuePair<string, object>(WebStringConstants.YEAR, vinInfo.Year),
                    new KeyValuePair<string, object>(WebStringConstants.MAKE, vinInfo.Make),
                    new KeyValuePair<string, object>(WebStringConstants.MODEL, vinInfo.Model)
                });
                reqparm.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
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

            if (null != jObj[WebStringConstants.ENC_DATA])
            {
                string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj[JsonStringConstants.ERROR])
            {
                LastError = jObj[JsonStringConstants.ERROR].Value<string>();
                _log.Add(TAG, jObj[JsonStringConstants.ERROR].Value<string>(), LogItemType.Error);
                return res;
            }
            if (null != jObj[JsonStringConstants.RESULT])
            {
                var result = jObj[JsonStringConstants.RESULT].Value<string>();

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

            LastError = string.Empty;

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

            if (!_cfg.EncodeTraffic)
            {
                reqparm.Add(WebStringConstants.ACTION, "update_vin_info");
                reqparm.Add(WebStringConstants.CLIENT, ClientInfo);
                reqparm.Add(WebStringConstants.VIN, vinInfo.Vin);
                reqparm.Add(WebStringConstants.YEAR, vinInfo.Year.ToString());
                reqparm.Add(WebStringConstants.MAKE, vinInfo.Make);
                reqparm.Add(WebStringConstants.MODEL, vinInfo.Model);
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters("update_vin_info", ClientInfo, new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>(WebStringConstants.VIN, vinInfo.Vin),
                    new KeyValuePair<string, object>(WebStringConstants.YEAR, vinInfo.Year),
                    new KeyValuePair<string, object>(WebStringConstants.MAKE, vinInfo.Make),
                    new KeyValuePair<string, object>(WebStringConstants.MODEL, vinInfo.Model)
                });
                reqparm.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
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

            if (null != jObj[WebStringConstants.ENC_DATA])
            {
                string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj[JsonStringConstants.ERROR])
            {
                LastError = jObj[JsonStringConstants.ERROR].Value<string>();
                _log.Add(TAG, jObj[JsonStringConstants.ERROR].Value<string>(), LogItemType.Error);
                return res;
            }
            if (null != jObj[JsonStringConstants.RESULT])
            {
                var result = jObj[JsonStringConstants.RESULT].Value<int>();
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

            LastError = string.Empty;
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

            if (!_cfg.EncodeTraffic)
            {
                reqparm.Add(WebStringConstants.ACTION, "read_vin_info");
                reqparm.Add(WebStringConstants.CLIENT, ClientInfo);
                reqparm.Add(WebStringConstants.VIN, vin);
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters("read_vin_info", ClientInfo, new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>(WebStringConstants.VIN, vin)
                });
                reqparm.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", WebServiceCall.CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
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

                if (null != jObj[WebStringConstants.ENC_DATA])
                {
                    string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                    responsebody = CoderHelper.Decode(encodedResponse);

                    jObj = JObject.Parse(responsebody);
                }

                if (null != jObj[JsonStringConstants.ERROR])
                {
                    LastError = jObj[JsonStringConstants.ERROR].Value<string>();
                    _log.Add(TAG, jObj[JsonStringConstants.ERROR].Value<string>(), LogItemType.Error);
                    return res;
                }
                if (null != jObj[JsonStringConstants.RESULT])
                {
                    var result = jObj[JsonStringConstants.RESULT];

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
        public static bool DownloadPrecondition(long envelopeId, int precNumber, string pathToImage)
        {
            Debug.Assert(envelopeId > 0);
            return DownloadInfoImage(envelopeId, precNumber, pathToImage, InfoTypeEnum.Precondition);
        }

        public static bool DownloadAdditionalInfoImage(long envelopeId, long dbReference, string pathToImage)
        {
            Debug.Assert(envelopeId > 0);
            return DownloadInfoImage(envelopeId, dbReference, pathToImage, InfoTypeEnum.AiDetail);
        }

        public static bool DownloadSupplementInfoImage(long envelopeId, string pathToImage)
        {
            Debug.Assert(envelopeId > 0);
            return DownloadInfoImage(envelopeId, -1L, pathToImage, InfoTypeEnum.Supplement);
        }

        public static bool DownloadInfoImage(long envelopeId, long dbReference, string pathToImage, InfoTypeEnum infoType)
        {
            bool res = false;
            string downloadUrl;
            SessionInfo sessionInfo = SessionInfo.Instance;
            string DataServiceUrl = _cfg.DataServiceUrl;
            string imageName = Path.GetFileName(pathToImage);
            int stage = 0;

            string downloadAction = "dl";

            switch (infoType)
            {
                case InfoTypeEnum.AiDetail:
                    downloadAction = "dl";
                    break;
                case InfoTypeEnum.Precondition:
                    downloadAction = "dp";
                    break;
                case InfoTypeEnum.Supplement:
                    downloadAction = "ds";
                    break;
                case InfoTypeEnum.AudioNote:
                    downloadAction = "da";
                    break;
            }

            Debug.Assert(envelopeId > 0);

            if (!DataServiceUrl.EndsWith("/"))
            {
                DataServiceUrl += "/";
            }

            LastError = string.Empty;
            string requestParameters;
            if (dbReference > 0L)
            {
                requestParameters = downloadAction + "/" + envelopeId.ToString() + "/" + dbReference.ToString() + "/" + imageName;
            }
            else
            {
                requestParameters = downloadAction + "/" + envelopeId.ToString() + "/" + imageName;
            }

            if (!_cfg.EncodeTraffic)
            {
                downloadUrl = DataServiceUrl + requestParameters;
            }
            else
            {
                downloadUrl = DataServiceUrl + WebStringConstants.ENC_DATA + CoderHelper.Encode(requestParameters);
            }

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(downloadUrl, pathToImage);
                }
                stage = 1;

                if (File.Exists(pathToImage) && FileHelper.Length(pathToImage) < FileHelper.MinimalLength)
                {
                    stage = 2;
                    string text = File.ReadAllText(pathToImage);
                    string error = string.Empty;
                    if (!string.IsNullOrEmpty(text))
                    {
                        JObject jObj = JObject.Parse(text);

                        if (null != jObj[WebStringConstants.ENC_DATA])
                        {
                            string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                            text = CoderHelper.Decode(encodedResponse);

                            jObj = JObject.Parse(text);
                        }

                        error = jObj[JsonStringConstants.ERROR].Value<string>();
                    }
                    stage = 3;
                    if (null != error)
                    {
                        LastError = string.Format("file '{0}' download error: '{1}', stage : {2}", requestParameters, error.ToString(), stage);
                        _log.Add(TAG, LastError, LogItemType.Error);
                        return res;
                    }
                }

                stage = 4;
                res = true;
            }
            catch (JsonException jre)
            {
                _log.AddError(TAG, jre, "stage: " + stage);
                Debug.Fail(jre.Message + Environment.NewLine + jre.StackTrace + Environment.NewLine + "stage: " + stage);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                _log.AddError(
                            TAG,
                            ex,
                            string.Format("file '{0}' download error, stage: {1}", downloadUrl, stage));
                Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return res;
        }
        #endregion

        #region audio 
        public static bool DownloadAudio(long envelopeId, string caseAudioName)
        {
            bool res = false;
            string downloadUrl;
            SessionInfo sessionInfo = SessionInfo.Instance;
            string DataServiceUrl = _cfg.DataServiceUrl;
            string audioName = Path.GetFileName(caseAudioName);

            if (!DataServiceUrl.EndsWith("/"))
            {
                DataServiceUrl += "/";
            }

            LastError = string.Empty;
            string requestParameters = "da/" + envelopeId.ToString() + "/" + audioName;

            if (!_cfg.EncodeTraffic)
            {
                downloadUrl = DataServiceUrl + requestParameters;
            }
            else
            {
                downloadUrl = DataServiceUrl + WebStringConstants.ENC_DATA + CoderHelper.Encode(requestParameters);
            }

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

                    if (null != jObj[WebStringConstants.ENC_DATA])
                    {
                        string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                        text = CoderHelper.Decode(encodedResponse);

                        jObj = JObject.Parse(text);
                    }

                    if (null != jObj[JsonStringConstants.ERROR])
                    {
                        _log.Add(TAG, "server download failed : " + jObj[JsonStringConstants.ERROR].Value<string>(), LogItemType.Error);
                    }
                    else
                    {
                        res = true;
                    }
                }
            }
            catch (JsonException jre)
            {
                Debug.Fail(jre.Message + Environment.NewLine + jre.StackTrace);
                _log.AddError(TAG, jre);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                _log.AddError(
                            TAG,
                            ex,
                            string.Format("file '{0}' download error", downloadUrl));
            }
            return res;
        }

        public static bool UploadAudio(long envelopeId, string fullPath, string tz, string proof, out long uploadedId)
        {
            uploadedId = 0L;
            long uId;

            bool res = WebServiceCall.UploadFile(envelopeId, 0, fullPath, InfoTypeEnum.AudioNote, tz, proof, out uId);

            if (res)
            {
                uploadedId = uId;
            }

            return res;
        }

        public static bool ReadAudioNotes(long envelopeId, out IList<CommonInfo> anList)
        {
            bool res = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();

            anList = null;
            LastError = string.Empty;

            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   string.Format("no session info found in 'read_audioinfo'"),
                   LogItemType.Error);

                return res;
            }

            if (!_cfg.EncodeTraffic)
            {
                reqparm.Add(WebStringConstants.ACTION, "read_audioinfo");
                reqparm.Add(WebStringConstants.CLIENT, ClientInfo);
                reqparm.Add(WebStringConstants.ENVELOPE_ID, envelopeId.ToString());
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters("read_audioinfo", ClientInfo, new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>(WebStringConstants.ENVELOPE_ID, envelopeId)
                });
                reqparm.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", WebServiceCall.CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
                LastError = ex.Message;
                responsebody = WebServiceCall.ErrorResponse(ex);
                _log.AddError(TAG, ex);
            }

            JObject jObj = JObject.Parse(responsebody);

            if (null != jObj[WebStringConstants.ENC_DATA])
            {
                string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj[JsonStringConstants.ERROR])
            {
                LastError = jObj[JsonStringConstants.ERROR].Value<string>();
                _log.Add(TAG, jObj[JsonStringConstants.ERROR].Value<string>(), LogItemType.Error);
                return res;
            }
            if (null != jObj[JsonStringConstants.RESULT])
            {
                anList = new List<CommonInfo>();

                var result = jObj[JsonStringConstants.RESULT];
                DateTime updated;

                foreach (var d in result["audio_notes"])
                {
                    try
                    {
                        updated = d["updated"].Value<DateTime>();
                    }
                    catch
                    {
                        Debug.Fail("failed to parse string date " + d["updated"].Value<string>());
                        updated = DateTime.UtcNow;
                    }

                    anList.Add(new AudioNote()
                    {
                        EnvelopeId = envelopeId,
                        Id = d["id"].Value<long>(),
                        FileName = d["file_name"].Value<string>(),
                        Updated = updated,
                        TZ = d["tz"].Value<string>(),
                        ProofStamp = d["proof"].Value<string>()
                    });
                }

                res = true;
            }

            return res;
        }

        public static bool DeleteAudioNote(long envelopeId, long audioNoteId, string audioFile)
        {
            bool res = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();
            int deletedAudioNoteId = 0;

            LastError = string.Empty;

            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   string.Format("no session info found in 'clear_audioinfo'"),
                   LogItemType.Error);

                return res;
            }

            if (!_cfg.EncodeTraffic)
            {
                reqparm.Add(WebStringConstants.ACTION, "clear_audioinfo");
                reqparm.Add(WebStringConstants.CLIENT, ClientInfo);
                reqparm.Add(WebStringConstants.ENVELOPE_ID, envelopeId.ToString());
                reqparm.Add(WebStringConstants.ID, audioNoteId.ToString());
                reqparm.Add("audio_note", audioFile);
            }
            else
            {
                string data = ActionParametersHelper.GenerateParameters("clear_audioinfo", ClientInfo, new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>(WebStringConstants.ENVELOPE_ID, envelopeId),
                    new KeyValuePair<string, object>(WebStringConstants.ID, audioNoteId),
                    new KeyValuePair<string, object>("audio_note", audioFile)
                });
                reqparm.Add(WebStringConstants.ENC_DATA, CoderHelper.Encode(data));
            }

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", WebServiceCall.CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
                LastError = ex.Message;
                responsebody = WebServiceCall.ErrorResponse(ex);
                _log.AddError(TAG, ex);
            }

            JObject jObj = JObject.Parse(responsebody);

            if (null != jObj[WebStringConstants.ENC_DATA])
            {
                string encodedResponse = jObj[WebStringConstants.ENC_DATA].Value<string>();
                responsebody = CoderHelper.Decode(encodedResponse);

                jObj = JObject.Parse(responsebody);
            }

            if (null != jObj[JsonStringConstants.ERROR])
            {
                LastError = jObj[JsonStringConstants.ERROR].Value<string>();
                _log.Add(TAG, jObj[JsonStringConstants.ERROR].Value<string>(), LogItemType.Error);
                return res;
            }
            if (null != jObj[JsonStringConstants.RESULT])
            {
                var result = jObj[JsonStringConstants.RESULT];

                deletedAudioNoteId = result["id"].Value<int>();
                _log.Add(
                   TAG,
                   "deleted audio_note_id : " + deletedAudioNoteId);

                res = true;
            }

            return res;
        }

        #endregion

        public static bool ReadConfig(out List<AdditionalActivity> cfgAddActivities, out List<OperationCode> cfgOperCodes, out List<PreconditionInfo> cfgPreconds)
        {
            bool res = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();

            cfgAddActivities = new List<AdditionalActivity>();
            cfgOperCodes = new List<OperationCode>();
            cfgPreconds = new List<PreconditionInfo>();
            LastError = string.Empty;

            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   "no session info found in 'read_full_case'",
                   LogItemType.Error);

                return res;
            }

            reqparm.Add(WebStringConstants.ACTION, "read_full_config");
            reqparm.Add(WebStringConstants.CLIENT, ClientInfo);

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", WebServiceCall.CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }
            }
            catch (Exception ex)
            {
                LastError = "read case failed. error : " + ex.Message;
                _oasEvent.RaiseEvent(OasEventType.ErrorMessage, LastError);
                _log.AddError(TAG, ex, LastError);

                responsebody = WebServiceCall.ErrorResponse(ex);
            }

            if (string.IsNullOrEmpty(responsebody))
            {
                return false;
            }
            try
            {
                JObject jObj = JObject.Parse(responsebody);

                if (null != jObj[JsonStringConstants.ERROR])
                {
                    LastError = jObj[JsonStringConstants.ERROR].Value<string>();
                    _log.Add(TAG, LastError, LogItemType.Error);
                    return false;
                }

                if (null != jObj[JsonStringConstants.RESULT])
                {
                    var result = jObj[JsonStringConstants.RESULT];

                    var codes = result["Codes"];
                    if (null != codes)
                    {
                        foreach (var c in codes)
                        {
                            cfgOperCodes.Add(new OperationCode()
                            {
                                Id = c["id"].Value<long>(),
                                CompanyId = c["company_id"].Value<long>(),
                                Code = c["code"].Value<string>(),
                                Abbr = c["abbr"].Value<string>(),
                                Description = c["description"].Value<string>()
                            });
                        }
                    }

                    var activities = result["AddActivities"];
                    if (null != activities)
                    {
                        foreach (var c in activities)
                        {
                            cfgAddActivities.Add(new AdditionalActivity()
                            {
                                Id = c["id"].Value<long>(),
                                CompanyId = c["company_id"].Value<long>(),
                                Code = c["code"].Value<string>(),
                                Description = c["description"].Value<string>()
                            });
                        }
                    }

                    var preconds = result["Preconditions"];
                    if (null != preconds)
                    {
                        foreach (var c in preconds)
                        {
                            cfgPreconds.Add(new PreconditionInfo()
                            {
                                CompanyId = c["company_id"].Value<long>(),
                                Index = c["idx"].Value<int>(),
                                Code = c["code"].Value<string>(),
                                Description = c["description"].Value<string>(),
                                PicturesToTake = c["pictures_to_take"].Value<int>()
                            });
                        }
                    }
                }

                res = true;
            }
            catch (JsonReaderException jre)
            {
                LastError = "read case failed. error : " + jre.Message;
                _oasEvent.RaiseEvent(OasEventType.ErrorMessage, LastError);
                _log.AddError(TAG, jre, LastError);
            }
            catch (Exception ex)
            {
                LastError = "read case failed. error : " + ex.Message;
                _oasEvent.RaiseEvent(OasEventType.ErrorMessage, LastError);
                _log.AddError(TAG, ex, LastError);
            }

            return res;
        }


        public static bool SendErrorReport(Exception ex)
        {
            bool result = false;
            string responsebody = string.Empty;
            NameValueCollection reqparm = new NameValueCollection();
            CookieContainer cookies = new CookieContainer();
            CookieCollection cc = new CookieCollection();

            LastError = string.Empty;

            SessionInfo sessionInfo = SessionInfo.Instance;
            if (null == sessionInfo || string.IsNullOrEmpty(sessionInfo.SessionId))
            {
                LastError = "no session info found";
                _log.Add(
                   TAG,
                   string.Format("no session info found in 'SaveAssignment'"),
                   LogItemType.Error);

                return result;
            }

            reqparm.Add("PHONE_MODEL", "DC");
            reqparm.Add("APP_VERSION_NAME", ClientInfo);
            reqparm.Add("ANDROID_VERSION", OSInfo.GetFullInfo());
            reqparm.Add("STACK_TRACE", ex.StackTrace);
            reqparm.Add("LOGCAT", ex.Message);

            cc.Add(new Cookie(WebStringConstants.SESSION, sessionInfo.SessionId, "/", WebServiceCall.CookieDomain));
            cookies.Add(cc);
            try
            {
                using (WebClientEx client = new WebClientEx(cookies))
                {
                    byte[] responsebytes = client.UploadValues(_cfg.DataServiceUrl, WebStringConstants.POST, reqparm);
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }

                result = true;
            }
            catch (Exception e)
            {
                Debug.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
                _log.AddError(TAG, e);
            }

            return result;
        }

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
