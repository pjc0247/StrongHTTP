using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Pipeline
{
    public interface IRequestProcessor
    {
        /// <summary>
        /// HttpRequest 요청에 대해서 변경하는 파이프라인
        /// </summary>
        /// <param name="api">api 객체</param>
        /// <param name="request">HttpRequest 객체</param>
        void OnRequest(object api, HttpRequest request);
    }
}
