using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    internal static class WebRequestExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <returns>WebResponse</returns>
        /// <exception cref="WebException">
        /// 응답을 받지도 못하고 실패했을 경우
        /// </exception>
        public static WebResponse GetResponseWithoutException(this WebRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            try
            {
                return request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response == null)
                    throw e;

                return e.Response;
            }
        }
    }
}
