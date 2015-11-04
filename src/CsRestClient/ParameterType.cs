using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    public enum ParameterType
    {
        /// <summary>파라미터를 무시합니다.</summary>
        Ignore,
        RequestUri,
        Suffix,
        PostJson,
        Header
    }
}
