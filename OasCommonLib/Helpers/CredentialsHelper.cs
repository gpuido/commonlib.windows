namespace OasCommonLib.Helpers
{
    using Logger;
    using System;
    using System.Text;

    public static class CredentialsHelper
    {
        static readonly Random _random = new Random();

        public static bool ReadCredentials(string cred, out string login, out string passwd)
        {
            bool ok = false;

            login = null;
            passwd = null;

            try
            {
                if (cred[0] == '_')
                {
                    var tmpString = cred.Substring(5);
                    var base64EncodedBytes = Convert.FromBase64String(tmpString);
                    string decodedString = Encoding.UTF8.GetString(base64EncodedBytes);

                    string l, p;
                    int il;
                    int ip;

                    base64EncodedBytes = Convert.FromBase64String(decodedString.Substring(8));
                    string sp = Encoding.UTF8.GetString(base64EncodedBytes);

                    l = decodedString.Substring(0, 4);
                    il = int.Parse(l);

                    l = decodedString.Substring(4, 4);
                    ip = int.Parse(l);

                    l = sp.Substring(0, il);
                    p = sp.Substring(il, ip);

                    login = l;
                    passwd = p;

                    ok = true;
                }
            }
            catch { }

            return ok;
        }
        public static bool SaveCredentials(string login, string passwd, out string cred)
        {
            bool ok = false;
            StringBuilder sb = new StringBuilder();

            cred = null;

            try
            {
                if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(passwd))
                {
                    int rnd = _random.Next(10, 99);

                    sb.AppendFormat("{0:0000}", login.Length);
                    sb.AppendFormat("{0:0000}", passwd.Length);

                    var plainTextBytes = Encoding.UTF8.GetBytes(login + passwd);
                    sb.Append(Convert.ToBase64String(plainTextBytes));

                    var tmpString = sb.ToString();

                    sb = new StringBuilder();
                    sb.AppendFormat("_{0:00}{1}", rnd.ToString().Length, rnd);

                    plainTextBytes = Encoding.UTF8.GetBytes(tmpString);
                    sb.Append(Convert.ToBase64String(plainTextBytes));

                    cred = sb.ToString();
                    ok = true;
                }
            }
            catch (Exception ex)
            {
                LogQueue.Instance.AddError("CredentialHelper", ex);
            }
            return ok;
        }
    }
}
