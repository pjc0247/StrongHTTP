using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.NamingConvention
{
    using Attributes;
    using Pipeline;

    [ProcessorTarget(typeof(WithNamingPolicy))]
    public class NamingPolicyProcessor : INameProcessor
    {
        public string OnParameter(object api, ParameterData param)
        {
            if (param.requestAttribute == null)
                return param.name;

            return ProcessConvention(param);
        }

        public string OnResource(object api, string name)
        {
            return name;
        }

        private string ProcessConvention(ParameterData param)
        {
            var tokens = CaseTokenizer.Tokenize(param.name);

            switch (param.requestAttribute.convention)
            {
                case ConventionType.HttpHeader:
                    return CaseConv.Join(tokens, CaseType.HttpHeader);
                case ConventionType.Pascal:
                    return CaseConv.Join(tokens, CaseType.Pascal);
                case ConventionType.Snake:
                    return CaseConv.Join(tokens, CaseType.Snake);
                case ConventionType.Camel:
                    return CaseConv.Join(tokens, CaseType.Camel);
            }

            return param.name;
        }
    }
}
