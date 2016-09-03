namespace OasCommonLib.Data
{

    public enum CommonInfoType
    {
        AdditionalInfo = 0, 
        Precondition = 1,
        Suppliment = 2,
        Audio = 3
    }

    public class CommonUploadInfo : CommonInfo
    {
        public bool Uploaded { get; set; }
        public CommonInfoType Type { get; set; }
        public long DbReference { get; set; }
    }
}
