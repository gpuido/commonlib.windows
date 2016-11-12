using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace OasCommonLib.Data
{
    public class CommonAdditionalInfo : CommonInfo
    {
        public InfoTypeEnum InfoType { get; set; }
        public long EnvelopeId { get; set; }
        public long Reference { get; set; }

        public CommonAdditionalInfo(long envelopeId, long dbReference, InfoTypeEnum type, CommonInfo ci)
        {
            EnvelopeId = envelopeId;
            Reference = dbReference;
            InfoType = type;

            Id = ci.Id;
            FileName = ci.FileName;
            Updated = ci.Updated;
            Note = ci.Note;
            FileMissing = ci.FileMissing;
            ProofStamp = ci.ProofStamp;
            TZ = ci.TZ;
        }

        public CommonAdditionalInfo() : base()
        {

        }

        public static CommonAdditionalInfo Parse(JToken j)
        {
            CommonAdditionalInfo cai = new CommonAdditionalInfo();

            try
            {
                cai.InfoType = (InfoTypeEnum)j["type"].Value<int>();
                cai.EnvelopeId = j["envelope_id"].Value<long>();
                cai.Reference = j["reference"].Value<long>();

                var ci = CommonInfo.Parse(j);

                cai.Id = ci.Id;
                cai.FileName = ci.FileName;
                cai.Updated = ci.Updated;
                cai.Note = ci.Note;
                cai.FileMissing = ci.FileMissing;
                cai.ProofStamp = ci.ProofStamp;
                cai.TZ = ci.TZ;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message + Environment.NewLine + j.ToString());
            }

            return cai;
        }
    }
}
