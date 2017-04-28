namespace OasCommonLib.Helpers
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;

    public class JsonHelper
    {
        public static DateTime GetDateTime(JToken p)
        {
            string tmp = p.Value<string>();
            DateTime.TryParse(tmp, out DateTime dt);

            return dt;
        }

        public static int[] ReadIntArray(JObject jt, int arraySize)
        {
            int[] counts = Enumerable.Repeat(0, arraySize).ToArray();

            foreach (var t in jt)
            {
                if (int.TryParse(t.Key, out int index))
                {
                    counts[index] = jt[t.Key].Value<int>();
                }
            }

            return counts;
        }
    }
}
