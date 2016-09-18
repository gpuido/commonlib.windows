namespace OasCommonLib.Data
{
    public class CommonUploadInfo : CommonInfo
    {
        public bool Uploaded { get; set; }
        public InfoTypeEnum Type { get; set; }
        public long DbReference { get; set; }
    }
}
