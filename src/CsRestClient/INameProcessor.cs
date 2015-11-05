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
        string OnResource(string name);
        string OnParameter(ParameterData param);
    }
}