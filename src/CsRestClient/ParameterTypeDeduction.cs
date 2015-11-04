using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace CsRestClient
{
    using CsRestClient.Attributes;

    internal static class ParameterTypeDeduction
    {
        public static ParameterType GetParamType(this ParameterInfo param, MethodInfo method)
        {
            if (param.GetCustomAttribute<Header>() != null)
                return ParameterType.Header;
            else if (param.GetCustomAttribute<RequestUri>() != null)
                return ParameterType.RequestUri;
            else if (param.GetCustomAttribute<Suffix>() != null)
                return ParameterType.Suffix;
            else if (param.GetCustomAttribute<PostJson>() != null)
                return ParameterType.PostJson;

            return ParameterType.RequestUri;
        }
    }
}
