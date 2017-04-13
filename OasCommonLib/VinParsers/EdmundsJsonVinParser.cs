namespace OasCommonLib.VinParser
{
    using Newtonsoft.Json.Linq;
    using System;

    public class EdmundsJsonVinParser : IVinParser
    {
        public string LastError { get; private set; }

        public string Vin { get; private set; }

        public EdmundsJsonVinParser(string vin)
        {
            Vin = vin;
        }

        public VinInfo Parse(string json)
        {
            LastError = String.Empty;

            try
            {
                JObject jObj = JObject.Parse(json);
                if (null != jObj["error"] || null != jObj["status"])
                {
                    LastError = jObj["message"].Value<string>();
                }
                else
                {
                    var array = jObj["styleHolder"];
                    var j = array[0];

                    int year = j["year"].Value<int>();
                    string make = j["makeName"].Value<string>();
                    string model = j["modelName"].Value<string>();

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
