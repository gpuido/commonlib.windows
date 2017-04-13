namespace OasCommonLib.VinParser
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;

    public class NHTSAJsonVinParser : IVinParser
    {
        public string LastError { get; private set; }

        public string Vin { get; private set; }

        public NHTSAJsonVinParser(string vin)
        {
            Vin = vin;
        }
        public VinInfo Parse(string json)
        {
            LastError = String.Empty;

            try
            {
                JObject jObj = JObject.Parse(json);
                if (null != jObj["Message"] || "Results returned successfully".Equals( jObj["Message"].Value<string>()))
                {
                    var array = jObj["Results"];
                    int fieldsFound = 0;
                    VinInfo vi = new VinInfo();

                    for (int i = 0; i < array.Count(); ++i)
                    {
                        var j = array[i];

                        long variableId = j["VariableId"].Value<long>();
                        string valueId = j["ValueId"].Value<string>();
                        string value = j["Value"].Value<string>();

                        if (143 == variableId && "7".Equals(valueId))
                        {
                            LastError = value;
                            return null;
                        }
                        else if (26 == variableId)
                        {
                            vi.Make = value;
                            ++fieldsFound;
                        }
                        else if (28 == variableId)
                        {
                            vi.Model = value;
                            ++fieldsFound;
                        }
                        else if (29 == variableId)
                        {
                            int year;

                            if (int.TryParse(value, out year))
                            {
                                vi.Year = year;
                            }
                            ++fieldsFound;
                        }

                        if (3 == fieldsFound)
                        {
                            vi.Vin = Vin;
                            break;
                        }
                    }

                    return vi;
                }
                else
                {
                    LastError = jObj["message"].Value<string>();
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
