using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes;

namespace Sample.Example.PipelineProcessor
{
    public interface NameProcessorTestAPI
    {
        /* /mytestrequest */
        string MyTestRequest();

        /* /allcapitalletter */
        string ALLCAPITALLETTER();

        /* /suffix?username={0} */
        string Suffix([RequestUri]string UserName);
    }

    [ProcessorTarget(new Type[] { typeof(NameProcessorTestAPI) })]
    public class NameProcessor : INameProcessor
    {
        public string OnParameter(ParameterData param)
        {
            return param.name.ToLower();
        }
        public string OnResource(string name)
        {
            return name.ToLower();
        }
    }
}
