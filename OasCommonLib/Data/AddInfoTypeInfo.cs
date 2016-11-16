namespace OasCommonLib.Data
{
    using Helpers;
    using Newtonsoft.Json.Linq;
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
        public bool MiltyReference { get; set; }

        public static readonly IDictionary<int, AddInfoTypeInfo> AddInfoTypes = new ConcurrentDictionary<int, AddInfoTypeInfo>();

        public static AddInfoTypeInfo Parse(JToken jt)
        {
            AddInfoTypeInfo aid = new AddInfoTypeInfo();

            try
            {
                aid.Id = jt["id"].Value<int>();
                aid.Name = jt["type_name"].Value<string>();
                aid.Enabled = jt["enabled"].Value<bool>();
                aid.MiltyReference = jt["multi_reference"].Value<bool>();

                if (null != jt["date"])
                {
                    aid.Created = JsonHelper.GetDateTime(jt["date"]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + Environment.NewLine + jt.ToString());
            }
            return aid;
        }
    }
}
