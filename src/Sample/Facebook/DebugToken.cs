using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes.Request;
using CsRestClient.Attributes.Response;

namespace Sample.Facebook
{
    [Service("v2.7")]
    public interface DebugTokenInterface : WithNamingPolicy
    {
        [Resource("debug_token")]
        [JsonPath("data")]
        Task<DebugTokenResult> Query([RequestUri]string inputToken, [RequestUri]string accessToken);

    }

    public class DebugToken
    {
        private static DebugTokenInterface impl
        {
            get
            {
                var impl = RemotePoint.Create<DebugTokenInterface>("https://graph.facebook.com");
                impl.namingPolicy = CsRestClient.NamingConvention.NamingPolicy.CreateDefault();

                impl.namingPolicy.requestUri = CsRestClient.NamingConvention.ConventionType.Snake;
                return impl;
            }
        }

        public static Task<DebugTokenResult> Query(string tokenToInspect, string appToken)
        {
            return impl.Query(tokenToInspect, appToken);
        }
    }

    public class DebugTokenResult
    {
        public string appId { get; set; }
        public string application { get; set; }
        public DateTime expiresAt { get; set; }
        public bool isValid { get; set; }
        public DateTime issuedAt { get; set; }
        public Dictionary<string, string> metadata { get; set; }
        public string[] scopes { get; set; }
        public long userId { get; set; }
    }
}
