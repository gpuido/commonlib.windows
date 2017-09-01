using System;

namespace OasCommonLib.Data
{
    public class CompanyInfo
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Abbr { get; private set; }
        public string Role { get; private set; }
        public long InsuranceGroupId { get; private set; }
        public bool IsDefault { get; private set; }

        public string TimeZone { get; private set; }

        public CompanyInfo(long id, string name, string abbr, string role, long insGrpId, bool isDefault, string companyTZ)
        {
            Id = id;
            Name = name;
            Abbr = abbr;
            Role = role;
            InsuranceGroupId = insGrpId;
            IsDefault = isDefault;
            if (!String.IsNullOrEmpty(companyTZ))
            {
                TimeZone = companyTZ;
            } else
            {
                TimeZone = TimeZoneInfo.Local.StandardName;
            }
        }
    }
}
