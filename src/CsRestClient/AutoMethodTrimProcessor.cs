using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    using Pipeline;
    using Attributes;
    using Attributes.Request;

    /// <summary>
    /// AutoHttpMethod가 붙어있는 메소드 이름을 정리한다.
    /// GetProfile -> profile
    /// </summary>
    /// <see cref="AutoHttpMethod"/>
    [ProcessorOrder(-1000)]
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
