using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsRestClient
{
    public interface INameProcessor
    {
        string OnResource(object api, string name);
        string OnParameter(object api, ParameterData param);
    }
}