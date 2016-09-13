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
    public interface DebugTokenInterface
    {
        [Resource("debug_token?input_token={0}&access_token={1}")]
        [JsonPath("data")]
        Task<DebugTokenResult> Query([Binding]string tokenToInspect, [Binding]string appToken);

    }

    public class DebugToken
    {
        private static DebugTokenInterface impl
        {
            get
            {
                return RemotePoint.Create<DebugTokenInterface>("https://graph.facebook.com");
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
