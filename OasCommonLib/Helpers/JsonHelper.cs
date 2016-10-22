namespace OasCommonLib.Helpers
{
    using Newtonsoft.Json.Linq;
    using System;

    public class JsonHelper
    {
        public static DateTime GetDateTime(JToken p)
        {
            DateTime dt;

            string tmp = p.Value<string>();
            DateTime.TryParse(tmp, out dt);

            return dt;
        }
    }
}
