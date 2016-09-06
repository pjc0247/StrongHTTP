using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    public interface IRestAPI
    {
    }

    public interface WithCommonHeader
    {
        Dictionary<string, string> commonHeaders { get; set; } 
    }
}
