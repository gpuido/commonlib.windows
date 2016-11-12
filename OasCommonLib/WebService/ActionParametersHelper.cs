namespace OasCommonLib.WebService
{
    using Constants;
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

            paramString.AppendFormat("{0}={1}", WebStringConstants.ACTION, actionName);

            if (!string.IsNullOrEmpty(clientInfo))
            {
                paramString.AppendFormat("&{0}={1}", WebStringConstants.CLIENT, clientInfo);
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
