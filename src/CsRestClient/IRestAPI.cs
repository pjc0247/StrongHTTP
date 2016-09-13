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

    public interface WithNamingPolicy
    {
        NamingConvention.NamingPolicy namingPolicy { get; set; }
    }
}
