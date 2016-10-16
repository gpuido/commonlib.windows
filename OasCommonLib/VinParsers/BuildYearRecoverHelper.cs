using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OasCommonLib.VinParsers
{
    public sealed class BuildYearRecoverHelper
    {
        public static int Recover(string buildString)
        {
            int build;


            if (!int.TryParse(buildString, out build))
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
