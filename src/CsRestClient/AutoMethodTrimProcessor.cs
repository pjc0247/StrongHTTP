using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    using CsRestClient.Attributes;

    public class AutoMethodTrimProcessor : INameProcessor
    {
        public string OnParameter(object api, ParameterData param)
        {
            return param.name;
        }

        public string OnResource(object api, string name)
        {
            if (api.GetType().GetCustomAttributes(true).Any(m => m is AutoHttpMethod) == false)
                return name;

            foreach (var prefix in HttpMethodDeduction.deductionTable)
            {
                if (name.ToLower().StartsWith(prefix.Key.ToLower()))
                {
                    return name.Substring(prefix.Key.Length);
                }
            }

            return name;
        }
    }
}
