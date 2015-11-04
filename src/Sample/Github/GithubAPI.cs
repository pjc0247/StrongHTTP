using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes;

namespace Sample.Github
{
    [Service("users")]
    public interface GithubAPI
    {
        [Resource("{0}/repos")]
        string GetRepos([Binding]string id);
    }

    public static class GithubAPIFactory
    {
        public static GithubAPI Create()
        {
            return RemotePoint.Create<GithubAPI>("https://api.github.com");
        }
    }
     
    [ProcessorTarget(new Type[] { typeof(GithubAPI) })]
    public class UserAgentProcessor : IRequestProcessor
    {
        public void OnRequest(HttpRequest request)
        {
            request.headers["User-Agent"] = "MyAwesomeGitClient";
        }
    }
}
