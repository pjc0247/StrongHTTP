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

        /// <summary>
        /// 파이프라인 프로세서를 검색할 어셈블리를 추가한다.
        /// </summary>
        /// <param name="reference">추가할 어셈블리</param>
        public static void AddPipelineLookupReference(Assembly reference)
        {
            pipelineLookups.Add(reference);
        }
    }
}
