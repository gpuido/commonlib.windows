namespace OasCommonLib.Data
{
    using Helpers;
    using Newtonsoft.Json.Linq;
    using OasCommonLib.Interfaces;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class AddInfoTypeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool MultiReference { get; set; }
        public int Export { get; set; }

        public static string LastError { get; private set; }

        public static readonly IDictionary<int, AddInfoTypeInfo> AddInfoTypes = new ConcurrentDictionary<int, AddInfoTypeInfo>();

        public static bool Parse(JToken jt, out AddInfoTypeInfo aid)
        {
            aid = new AddInfoTypeInfo();
            LastError = String.Empty;
            try
            {
                aid.Id = jt["id"].Value<int>();
                aid.Name = jt["type_name"].Value<string>();
                aid.Enabled = jt["enabled"].Value<int>() != 0;
                aid.Export= jt["export"].Value<int>();
                aid.MultiReference = jt["multi_reference"].Value<int>() != 0;

                if (null != jt["date"])
                {
                    aid.Created = JsonHelper.GetDateTime(jt["date"]);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Debug.WriteLine(ex.Message + Environment.NewLine + jt.ToString());
                return false;
            }

            return true;
        }
    }
}
