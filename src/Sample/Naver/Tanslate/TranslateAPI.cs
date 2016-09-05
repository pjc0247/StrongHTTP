using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Pipeline;
using CsRestClient.Attributes;
using CsRestClient.Attributes.Request;
using CsRestClient.Attributes.Response;

namespace Sample.Naver.Tanslate
{
    [Service("v1/language")]
    public interface TranslateAPIInterface
    {
        string clientId { get; set; }
        string clientSecret { get; set; }

        [Post]
        [Resource("translate")]
        [JsonPath("message.result.translatedText")]
        Task<string> Translate([RequestUri]string source, [RequestUri]string target, [RequestUri]string text);
    }

    [ProcessorTarget(new Type[] { typeof(TranslateAPIInterface) })]
    public class ClientAuthProcessor : IRequestProcessor
    {
        public void OnRequest(object api, HttpRequest request)
        {
            request.headers["Content-Type"] = "application/x-www-form-urlencoded";
            request.headers["X-Naver-Client-Id"] = ((TranslateAPIInterface)api).clientId;
            request.headers["X-Naver-Client-Secret"] = ((TranslateAPIInterface)api).clientSecret;
        }
    }

    public class TranslateAPI
    {
        public static TranslateAPIInterface Create(string clientId, string clientSecret)
        {
            var api = RemotePoint.Create<TranslateAPIInterface>("https://openapi.naver.com");

            api.clientId = clientId;
            api.clientSecret = clientSecret;

            return api;
        }
    }
}
