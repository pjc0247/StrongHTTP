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
        /// <summary>
        /// 요청에 보내질 파라미터를 변경하는 파이프라인
        /// </summary>
        /// <param name="api">api 객체</param>
        /// <param name="method">api 객체에서 실제로 호출된 메소드</param>
        /// <param name="parameterData">파라미터들의 목록</param>
        void OnParameter(object api, MethodInfo method, List<ParameterData> parameterData);
    }
}
