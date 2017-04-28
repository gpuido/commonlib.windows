namespace OasCommonLib.Data
{
    using Newtonsoft.Json.Linq;

    public class PreconditionInfo
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public int Index { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int PicturesToTake { get; set; }

        internal static PreconditionInfo Parse(JToken c)
        {
            var pi = new PreconditionInfo()
            {
                CompanyId = c["company_id"].Value<long>(),
                Index = c["idx"].Value<int>(),
                Code = c["code"].Value<string>(),
                Description = c["description"].Value<string>(),
                PicturesToTake = c["pictures_to_take"].Value<int>()
            };
            return pi;

        }
    }
}
