namespace OasCommonLib.Data.Insurance
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class InsuranceList
    {

        #region static
        private static InsuranceList _instance;
        public static InsuranceList Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new InsuranceList();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        #endregion


        public readonly List<InsuranceGroupInfo> List = new List<InsuranceGroupInfo>();

        public int FindIdByIndex(string index)
        {
            var i = List.FirstOrDefault((x) => x.Index.Equals(index, System.StringComparison.CurrentCultureIgnoreCase));
            if (null == i)
            {
                return -1;
            }

            return i.Id;
        }
    }
}
