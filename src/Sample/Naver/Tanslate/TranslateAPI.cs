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
    public interface TranslateAPIInterface : WithCommonHeader
    {
        [Post]
        [Resource("translate")]
        [JsonPath("message.result.translatedText")]
        Task<string> Translate([RequestUri]string source, [RequestUri]string target, [RequestUri]string text);
    }

    public class TranslateAPI
    {
        public static TranslateAPIInterface Create(string clientId, string clientSecret)
        {
            var api = RemotePoint.Create<TranslateAPIInterface>("https://openapi.naver.com");

            api.commonHeaders = new Dictionary<string, string>()
            {
                ["X-Naver-Client-Id"] = clientId,
                ["X-Naver-Client-Secret"] = clientSecret
            };

            return api;
        }
    }
}
