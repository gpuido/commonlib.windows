namespace OasCommonLib.Session
{
    using Interfaces;
    using Newtonsoft.Json.Linq;
    using System;
    using WebService;

    public class SessionInfo : IError
    {
        private static volatile SessionInfo _instance;
        private static object syncRoot = new Object();
        public static SessionInfo Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (syncRoot)
                    {
                        if (null == _instance)
                        {
                            _instance = new SessionInfo();
                        }
                    }
                }
                return _instance;
            }
        }

        public string SessionId { get; private set; }
        public int UserId { get; private set; }
        public int CompanyId { get; private set; }
        public string CompanyName { get; private set; }
        public string CompanyAbbr { get; private set; }
        public string UserName { get; private set; }
        public string UserLogin { get; private set; }
        public string[] Roles { get; private set; }

        public string CompanyRole { get; private set; }
        public string AuthKey { get; private set; }

        public bool HasConnection { get; private set; }

        public string LastError { get; private set; }

        public SessionInfo()
        {
            SessionId = string.Empty;
            UserId = 0;
            UserName = string.Empty;
            CompanyName = string.Empty;
            UserLogin = string.Empty;
            Roles = new string[] { };
            HasConnection = false;
        }

        public bool Login(string login, string passwd, bool saveSession = true)
        {
            bool result = false;

            LastError = string.Empty;

            try
            {
                string json;
                string session;

                if (WebServiceCall.Login(login, passwd, saveSession, out session, out json))
                {
                    JObject jObj = JObject.Parse(json);
                    var res = jObj["result"];

                    string[] roles = res["roles"].Value<string>().Split(',');
                    SessionId = session;
                    CompanyId = res["company_id"].Value<int>();
                    CompanyName = res["company_name"].Value<string>();
                    CompanyAbbr = res["company_abbr"].Value<string>();
                    UserId = res["user_id"].Value<int>();
                    UserName = res["user_name"].Value<string>();
                    UserLogin = login;
                    Roles = roles;
                    CompanyRole = res["company_role"].Value<string>();

                    if (saveSession)
                    {
                        SetSessionInfo(SessionId, CompanyId, CompanyName, CompanyAbbr, UserId, UserName, login, Roles, CompanyRole);
                    }

                    result = true;
                }
                else
                {
                    LastError = WebServiceCall.LastError;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }

            return result;
        }

        public void SetSessionInfo(string sessionId, int companyId, string companyName, string companyAbbr, int userId, string userName, string login, string[] roles, string companyRole)
        {
            SessionId = sessionId;
            UserId = userId;
            UserName = userName;
            CompanyId = companyId;
            CompanyName = companyName;
            CompanyAbbr = companyAbbr;
            UserLogin = login;
            Roles = roles;
            CompanyRole = companyRole;
        }
    }
}
