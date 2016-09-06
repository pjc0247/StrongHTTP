using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete
    }

    internal static class HttpMethodHelper
    {
        /// <summary>
        /// HttpMethod가 요청 데이터를 추가로 보낼 수 있는지를 조사한다.
        /// </summary>
        /// <param name="httpMethod">조사할 HTTP 메소드</param>
        /// <returns>페이로드 허용 여부</returns>
        public static bool IsPayloadAllowed(this HttpMethod httpMethod)
        {
            switch (httpMethod)
            {
                // FALSE
                case HttpMethod.Get: return false;
                case HttpMethod.Delete: return false;

                // TRUE
                case HttpMethod.Post: return true;
                case HttpMethod.Put: return true;
            }

            throw new InvalidOperationException("Unexpected HttpMethod");
        }
    }
}
