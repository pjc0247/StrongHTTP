using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsRestClient
{
    public interface IParameterProcessor
    {
        void OnParameter(object api, MethodInfo request, List<ParameterData> parameterData); 
    }
}