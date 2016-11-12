namespace OasCommonLib.Data
{
    using Helpers;

    public enum ServerTypeEnum { web, local };

    public class ServerInfo
    {
        public ServerTypeEnum ServerType { get; set; }
        public string ServerVersion { get; set; }
        public string DbVersion { get; set; }
    }
}
