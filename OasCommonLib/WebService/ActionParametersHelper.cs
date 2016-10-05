namespace OasCommonLib.WebService
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public sealed class ActionParametersHelper
    {
        public static string GenerateParameters(
            string actionName,
            string clientInfo,
            List<KeyValuePair<string, object>> paramList = null)
        {
            StringBuilder paramString = new StringBuilder();

            if (string.IsNullOrEmpty(actionName))
            {
                throw new ArgumentException("action name can not be empty");
            }

            paramString.AppendFormat("action={0}", actionName);

            if (!string.IsNullOrEmpty(clientInfo))
            {
                paramString.AppendFormat("&client={0}", clientInfo);
            }

            if (null != paramList)
            {
                foreach (var p in paramList)
                {
                    paramString.AppendFormat("&{0}={1}", p.Key, p.Value);
                }
            }

            return paramString.ToString();
        }
    }
}
