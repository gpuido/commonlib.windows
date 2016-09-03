namespace OasCommonLib.Config
{
    using Newtonsoft.Json.Linq;

    public interface IConfigData
    {
        void InitDefault(string dataPath);
        void ExtractData(JToken jt);
    }
}
