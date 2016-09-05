using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Pipeline;
using CsRestClient.Attributes;
using CsRestClient.Attributes.Request;

namespace Sample.Example.PipelineProcessor
{
    /* INameProcessor는 네이밍 컨벤션을 변경할 때 사용됩니다.
       
        아래의 예제는 요청의 각 파라미터들과, 리소스 주소를 모두
        소문자로 바꾸는 방법을 보여줍니다.
     */

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
        public string OnParameter(object api, ParameterData param)
        {
            return param.name.ToLower();
        }
        public string OnResource(object api, string name)
        {
            return name.ToLower();
        }
    }
}
