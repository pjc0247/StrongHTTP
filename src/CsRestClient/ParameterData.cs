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
        /// <summary>
        /// 파라미터의 원본 이름입니다.
        /// </summary>
        public string rawName { get; internal set; }
        public int position { get; set; }
        public object value { get; set; }
        public ParameterType type { get; set; }
    }
}
