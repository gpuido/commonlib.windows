namespace OasCommonLib.Data
{
    using Helpers;
    using System;
    using Newtonsoft.Json.Linq;
    using System.Diagnostics;

    public class CommonInfo
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public DateTime Updated { get; set; }
        public string Note { get; set; }
        public bool FileMissing { get; set; }
        public string ProofStamp { get; set; }
        public string TZ { get; set; }

        private static long _uniqueId = -1;
        public static long UniqueId
        {
            get
            {
                return _uniqueId;
            }
        }

        public static string LastError { get; private set; }

        public CommonInfo()
        {
            Id = UniqueId;
            Note = string.Empty;
            Updated = DateTime.UtcNow;
            FileMissing = false;

            TZ = TimeZoneInfo.Local.StandardName;
        }

        public static bool Clear(long envelopeId, CommonInfo ai)
        {
            string src = ImageHelper.CaseImagePath(envelopeId, ai.FileName);
            string dst = ImageHelper.ImagePath(ai.FileName);

            LastError = string.Empty;

            if (!FileHelper.Copy(src, dst, true) || !FileHelper.DeleteFile(src))
            {
                LastError = string.Format("error: during move file '{0}' to '{1}' - {2}", src, dst, FileHelper.Error);
                return false;
            }

            return true;
        }

        public static CommonInfo Parse(JToken ai)
        {
            CommonInfo ci = new CommonInfo();

            try
            {
                ci.Id = ai["id"].Value<long>();
                ci.Note = ai["note"].Value<string>();
                ci.TZ = ai["tz"].Value<string>();
                ci.ProofStamp = ai["proof"].Value<string>();
                ci.Updated = JsonHelper.GetDateTime(ai["updated"]);
                ci.FileName = ai["file_name"].Value<string>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + Environment.NewLine + ai.ToString());
            }

            return ci;
        }
    }
}
