namespace OasConfig.Data
{
    using System.Collections.Generic;

    public enum EstimatorEnum
    {
        Mitchell = 0,
        CCC = 1,
        Audatex = 2
    }

    public class Estimators
    {
        public static readonly Dictionary<EstimatorEnum, string> List = new Dictionary<EstimatorEnum, string>()
        {
            { EstimatorEnum.Mitchell, "Mitchell" },
            { EstimatorEnum.CCC, "CCC Pathways" },
            { EstimatorEnum.Audatex, "Audatex" },
        };
    }
}
