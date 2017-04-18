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
        public long InsuranceGroupId { get; private set; }
        public string Pin { get; private set; }

        public string CompanyRole { get; private set; }
        public string AuthKey { get; private set; }

        public bool HasConnection { get; private set; }

        public string LastError { get; private set; }

        public SessionInfo()
        {
            SessionId = String.Empty;
            UserId = 0;
            UserName = String.Empty;
            CompanyName = String.Empty;
            UserLogin = String.Empty;
            Roles = new string[] { };
            HasConnection = false;
        }

        public bool Login(string login, string passwd, bool saveSession = true)
        {
            bool result = false;

            LastError = String.Empty;

            try
            {
                if (WebServiceCall.Login(login, passwd, out string session, out string json))
                {
                    if (saveSession)
                    {
                        if (!SetSessionInfo(session, json))
                        {
                            return result;
                        }
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
                LastError = "login failed :" + ex.Message;
            }

            return result;
        }

        public static SessionInfo Parse(string session, string json)
        {
            SessionInfo si = new SessionInfo();
            JObject jObj = JObject.Parse(json);
            JObject result = (JObject)jObj["result"];

            string[] roles = result["roles"].Value<string>().Split(',');

            string userLogin = result["user_login"].Value<string>();
            int companyId = result["company_id"].Value<int>();
            string companyName = result["company_name"].Value<string>();
            string companyAbbr = result["company_abbr"].Value<string>();
            int userId = result["user_id"].Value<int>();
            string userName = result["user_name"].Value<string>();
            string companyRole = result["company_role"].Value<string>();
            long insGrpId = result["ins_grp_id"].Value<long>();
            string pin = result["pin"].Value<string>();

            si.SetSessionInfo(session, userLogin, companyId, companyName, companyAbbr, userId, userName, roles, companyRole, insGrpId, pin);

            return si;
        }


        public bool SetSessionInfo(string session, string json)
        {
            bool res = false;

            try
            {
                var si = Parse(session, json);
                SetSessionInfo(session, si.UserLogin, si.CompanyId, si.CompanyName, si.CompanyAbbr, si.UserId, si.UserName, si.Roles, si.CompanyRole, si.InsuranceGroupId, si.Pin);

                res = true;
            }
            catch (Exception ex)
            {
                LastError = "error in set session :" + ex.Message;
            }

            return res;
        }

        public void SetSessionInfo(string sessionId, string userLogin, int companyId, string companyName, string companyAbbr, int userId, string userName, string[] roles, string companyRole, long insGrpId, string pin)
        {
            UserLogin = userLogin;
            SessionId = sessionId;
            UserId = userId;
            UserName = userName;
            CompanyId = companyId;
            CompanyName = companyName;
            CompanyAbbr = companyAbbr;
            Roles = roles;
            CompanyRole = companyRole;
            InsuranceGroupId = insGrpId;
            Pin = pin;
        }
    }
}
