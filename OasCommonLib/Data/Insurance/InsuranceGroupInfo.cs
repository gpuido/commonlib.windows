namespace OasCommonLib.Data.Insurance
{
    using Newtonsoft.Json.Linq;
    using OasCommonLib.Helpers;
    using System;
    using System.Diagnostics;

    public class InsuranceGroupInfo
    {
        public int Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public DateTime Registered { get; set; }
        public bool Enabled { get; set; }
        public bool Default { get; set; }

        public static InsuranceGroupInfo Parse(JToken jt)
        {
            InsuranceGroupInfo ins = new InsuranceGroupInfo();

            try
            {
                ins.Id = jt["id"].Value<int>();
                ins.Index = jt["idx"].Value<string>();
                ins.Name = jt["name"].Value<string>();
                ins.Enabled = jt["enabled"].Value<int>() != 0;
                ins.Default = jt["is_default"].Value<int>() != 0;

                if (null != jt["date"])
                {
                    ins.Registered = JsonHelper.GetDateTime(jt["date"]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + Environment.NewLine + jt.ToString());
            }
            return ins;
        }
    }
}
