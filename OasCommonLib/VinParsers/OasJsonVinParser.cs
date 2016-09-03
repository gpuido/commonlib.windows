namespace OasCommonLib.VinParser
{
    using Newtonsoft.Json.Linq;
    using System;

    public class OasJsonVinParser : IVinParser
    {
        public string LastError { get; private set; }

        public string Vin { get; private set; }

        public OasJsonVinParser(string vin)
        {
            Vin = vin;
        }

        public VinInfo Parse(string json)
        {
            LastError = string.Empty;

            try
            {
                JObject jObj = JObject.Parse(json);
                if (null != jObj["error"])
                {
                    LastError = jObj["error"].Value<string>();
                }
                else
                {
                    var j = jObj["result"];

                    int year = j["year"].Value<int>();
                    string make = j["make"].Value<string>();
                    string model = j["model"].Value<string>();

                    VinInfo vi = new VinInfo();
                    vi.Vin = Vin;
                    vi.Year = year;
                    vi.Model = model;
                    vi.Make = make;

                    return vi;
                }
            }
            catch (Exception ex)
            {
                LastError = "parser failed : " + ex.Message;
            }

            return null;
        }
    }
}
