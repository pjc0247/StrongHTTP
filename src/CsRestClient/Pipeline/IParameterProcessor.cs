using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Pipeline
{
    public interface IParameterProcessor
    {
        void OnParameter(object api, MethodInfo method, List<ParameterData> parameterData);
    }
}
