using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CsRestClient.Pipeline
{
    public interface INameProcessor
    {
        /// <summary>
        /// 리소스 경로 이름을 결정하는 파이프라인
        /// </summary>
        /// <param name="api">api 객체</param>
        /// <param name="name">리소스 경로</param>
        /// <returns>변경할 이름</returns>
        string OnResource(object api, string name);

        /// <summary>
        /// 파라미터 이름을 결정하는 파이프라인
        /// </summary>
        /// <param name="api">api 객체</param>
        /// <param name="param">파라미터 정보</param>
        /// <returns>변경할 이름</returns>
        string OnParameter(object api, ParameterData param);
    }
}