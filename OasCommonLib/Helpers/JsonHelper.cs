namespace OasCommonLib.Helpers
{
    using Newtonsoft.Json.Linq;
    using System;

    public class JsonHelper
    {
        public static DateTime GetDateTime(JToken p)
        {
            string tmp = p.Value<string>();
            DateTime.TryParse(tmp, out DateTime dt);

            return dt;
        }
    }
}
