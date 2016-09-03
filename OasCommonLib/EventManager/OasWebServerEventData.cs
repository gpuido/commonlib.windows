namespace OasCommonLib.EventManager
{
    public sealed class OasWebServerEventData
    {
        public long EnvelopeId { get; set; }
        public long DbReference { get; set; }
        public string FileName { get; set; }
        public string ExportName { get; set; }
        public long AddInfoId { get; set; }
        public object Case { get; set; }
    }
}
