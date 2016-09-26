namespace OasCommonLib.Data
{
    using System;

    public class CommonInfo
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public DateTime Updated { get; set; }
        public string Note { get; set; }
        public bool FileMissing { get; set; }
        public string ProofStamp { get; set; }
        public string TZ { get; set; }

        public CommonInfo()
        {
            Id = 0L;
            Note = string.Empty;
            Updated = DateTime.UtcNow;
            FileMissing = false;

            TZ = TimeZoneInfo.Local.StandardName;
        }
    }
}
