using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsRestClient
{
    public class Config
    {
        internal static bool logOutput { get; set; }
        internal static List<Assembly> pipelineLookups { get; set; }

        static Config()
        {
            pipelineLookups = new List<Assembly>();
        }

        public static void EnableLog()
        {
            logOutput = true;
        }

        public static void AddPipelineLookupReference(Assembly reference)
        {
            pipelineLookups.Add(reference);
        }
    }
}
