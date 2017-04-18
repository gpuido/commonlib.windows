namespace OasCommonLib.VinParsers
{
    using System;

    public sealed class BuildYearRecoverHelper
    {
        public static int Recover(string buildString)
        {
            if (!int.TryParse(buildString, out int build))
            {
                build = 2000;
            }
            else
            {
                if (build < 50)
                {
                    build += 2000;
                }
                else if (build > 50 && build < 100)
                {
                    build += 1900;
                }

                if (build > DateTime.Now.Year)
                {
                    build -= 100;
                }
            }

            return build;
        }
    }
}
