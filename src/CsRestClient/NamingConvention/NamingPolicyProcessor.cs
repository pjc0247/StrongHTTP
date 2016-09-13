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
            return ProcessConvention(api, param);
        }

        public string OnResource(object api, string name)
        {
            return name;
        }

        private string ProcessConvention(object api, ParameterData param)
        {
            string output = param.name;

            if (param.requestAttribute != null &&
                Conv(param.requestAttribute.convention, param.name, out output))
            {
                return output;
            }

            var policy = (NamingPolicy)api
                .GetType()
                .GetProperty("namingPolicy")
                .GetValue(api);
            ConventionType convention = ConventionType.None;

            switch (param.type)
            {
                case ParameterType.PostJson:
                    convention = policy.postJson;
                    break;
                case ParameterType.Header:
                    convention = policy.header;
                    break;
                case ParameterType.RequestUri:
                    convention = policy.requestUri;
                    break;
            }

            if (Conv(convention, param.name, out output))
            {
                return output;
            }

            return param.name;
        }

        private bool Conv(ConventionType convention, string input, out string output)
        {
            var tokens = CaseTokenizer.Tokenize(input);

            switch (convention)
            {
                case ConventionType.HttpHeader:
                    output = CaseConv.Join(tokens, CaseType.HttpHeader);
                    return true;
                case ConventionType.Pascal:
                    output = CaseConv.Join(tokens, CaseType.Pascal);
                    return true;
                case ConventionType.Snake:
                    output = CaseConv.Join(tokens, CaseType.Snake);
                    return true;
                case ConventionType.Camel:
                    output = CaseConv.Join(tokens, CaseType.Camel);
                    return true;

                case ConventionType.Dontcare:
                    output = input;
                    return true;
            }

            output = input;
            return false;
        }
    }
}
