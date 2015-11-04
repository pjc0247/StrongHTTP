using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    public class ParameterData
    {
        public string name { get; set; }
        public int position { get; set; }
        public object value { get; set; }
        public ParameterType type { get; set; }
    }
}
