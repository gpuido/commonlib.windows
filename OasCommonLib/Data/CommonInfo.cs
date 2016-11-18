namespace OasCommonLib.Data
{
    using Helpers;
    using System;
    using Newtonsoft.Json.Linq;
    using System.Diagnostics;
    using System.Text;
    using Newtonsoft.Json;

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
                LastError = String.Format("error: during move file '{0}' to '{1}' - {2}", src, dst, FileHelper.LastError);
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return String.Format("id:{0}, image:{1}, note:{2}, updated:{3}, tz:{4}, proof:{5}", Id, FileName, Note, Updated, TZ, ProofStamp);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static CommonInfo Parse(string json)
        {
            JObject jObj = JObject.Parse(json);
            return Parse(jObj);
        }

        public static CommonInfo Parse(JToken ai)
        {
            CommonInfo ci = new CommonInfo();

            try
            {
                ci.Id = ai["id"].Value<long>();
                ci.Note = ai["note"].Value<string>();
                ci.TZ = ai["tz"].Value<string>();

                if (null != ai["proof"])
                {
                    ci.ProofStamp = ai["proof"].Value<string>();
                }
                else if (null != ai["proofStamp"])
                {
                    ci.ProofStamp = ai["proofStamp"].Value<string>();
                }

                if (null != ai["updated"])
                {
                    ci.Updated = JsonHelper.GetDateTime(ai["updated"]);
                }

                if (null != ai["file_name"])
                {
                    ci.FileName = ai["file_name"].Value<string>();
                }
                else if (null != ai["fileName"])
                {
                    ci.FileName = ai["fileName"].Value<string>();
                }
                else if (null != ai["FileName"])
                {
                    ci.FileName = ai["FileName"].Value<string>();
                }

                if (null != ai["is_file_missing"])
                {
                    ci.FileMissing = ai["is_file_missing"].Value<bool>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + Environment.NewLine + ai.ToString());
            }

            return ci;
        }
    }
}
