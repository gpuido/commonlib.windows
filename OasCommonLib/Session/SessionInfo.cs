namespace OasCommonLib.Session
{
    using Interfaces;
    using Newtonsoft.Json.Linq;
    using OasCommonLib.Data;
    using System;
    using System.Linq;
    using System.Collections.Generic;
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
        public string UserName { get; private set; }
        public string UserLogin { get; private set; }
        public Dictionary<long, string[]> Roles { get; private set; }
        public List<CompanyInfo> UserCompanies { get; private set; }
        public string Pin { get; private set; }

        public string AuthKey { get; private set; }

        public bool HasConnection { get; private set; }

        public string LastError { get; private set; }

        public SessionInfo()
        {
            SessionId = String.Empty;
            UserId = 0;
            UserName = String.Empty;
            UserLogin = String.Empty;
            Roles = new Dictionary<long, string[]>();
            UserCompanies = new List<CompanyInfo>();
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


            string userLogin = result["user_login"].Value<string>();
            int companyId = result["company_id"].Value<int>();
            int userId = result["user_id"].Value<int>();
            string userName = result["user_name"].Value<string>();
            string pin = result["pin"].Value<string>();

            var companies = ParseUserCompanies((JArray)result["companies"]);
            var roles = ParseRoles((JArray)result["roles"]);

            si.SetSessionInfo(session, userLogin, userId, userName, pin, companies, roles);

            return si;
        }

        private static Dictionary<long, string[]> ParseRoles(JArray jArray)
        {
            var roles = new Dictionary<long, string[]>();

            foreach (var jt in jArray)
            {
                long companyId = jt["company_id"].Value<long>();
                var ra = jt["roles"].ToObject<string[]>();

                roles.Add(companyId, ra);
            }

            return roles;
        }

        public static List<CompanyInfo> ParseUserCompanies(JArray jArray)
        {
            var companies = new List<CompanyInfo>();
            string name;
            string role;
            string abbr;
            long companyId;
            int insGrpId;
            bool isDefault;
            string companyTimeZone;

            foreach (var jt in jArray)
            {
                companyId = jt["company_id"].Value<long>();
                name = jt["name"].Value<string>();
                abbr = jt["abbr"].Value<string>();
                role = jt["role"].Value<string>();
                insGrpId = jt["ins_grp_id"].Value<int>();
                isDefault = jt["is_default"].Value<bool>();
                companyTimeZone = jt["tz"].Value<string>();

                companies.Add(new CompanyInfo(companyId, name, abbr, role, insGrpId, isDefault, companyTimeZone));
            }

            return companies;
        }

        public bool SetSessionInfo(string session, string json)
        {
            bool res = false;

            try
            {
                var si = Parse(session, json);
                SetSessionInfo(session, si.UserLogin, si.UserId, si.UserName, si.Pin, si.UserCompanies, si.Roles);

                res = true;
            }
            catch (Exception ex)
            {
                LastError = "error in set session :" + ex.Message;
            }

            return res;
        }

        public void SetSessionInfo(string sessionId, string userLogin, int userId, string userName, string pin, List<CompanyInfo> companies, Dictionary<long, string[]> roles)
        {
            UserLogin = userLogin;
            SessionId = sessionId;
            UserId = userId;
            UserName = userName;
            Pin = pin;

            UserCompanies = companies;
            Roles = roles;
        }

        public bool HasRole(string role)
        {
            foreach (var pair in Roles)
            {
                if (pair.Value.Contains(role))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
