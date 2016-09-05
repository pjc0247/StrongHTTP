using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    public class Config
    {
        internal static bool logOutput { get; set; }

        public static void EnableLog()
        {
            logOutput = true;
        }
    }
}
