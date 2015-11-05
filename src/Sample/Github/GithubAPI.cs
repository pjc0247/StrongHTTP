using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes;

namespace Sample.Github
{
    [AutoHttpMethod]
    public interface APIBase
    {
        string authString { get; set; }
    }

    [Service("user")]
    public interface UserAPI : APIBase
    {
        string GetRepos();
    }
    [Service("gists")]
    public interface GistAPI : APIBase
    {
        [Resource("")]
        string GetGists();

        [Resource("")]
        string GetGist([Suffix]string id);
        [Resource("")]
        string GetGist([Suffix]string id, [Suffix]string revision);
    }

    public class GithubAPI
    {
        public string authString { get; set; }

        public UserAPI user { get; set; }
        public GistAPI gists { get; set; }
    }
    public static class GithubAPIFactory
    {
        public static GithubAPI Create(string username, string password)
        {
            var api = new GithubAPI();

            api.authString = "Basic " + Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{username}:{password}"));

            api.user = Create<UserAPI>(api.authString);
            api.gists = Create<GistAPI>(api.authString);

            return api;
        }
        private static T Create<T>(string authString)
            where T : APIBase
        {
            var api = RemotePoint.Create<T>("https://api.github.com");
            api.authString = authString;          
            return api;
        }
    }

    [ProcessorTarget(new Type[] { typeof(APIBase) })]
    public class NameProcessor : INameProcessor
    {
        public string OnParameter(object api, ParameterData param)
        {
            return param.name;
        }
        public string OnResource(object api, string name)
        {
            return name.ToLower();
        }
    }
    [ProcessorTarget(new Type[] { typeof(APIBase) })]
    public class BasicAuthProcessor : IRequestProcessor
    {
        public void OnRequest(object api, HttpRequest request)
        {
            request.headers["Authorization"] = ((APIBase)api).authString;
        }
    }
    [ProcessorTarget(new Type[] { typeof(APIBase) })]
    public class UserAgentProcessor : IRequestProcessor
    {
        public void OnRequest(object api, HttpRequest request)
        {
            /* all requests sent to github api server must include 
               'User-Agent' header.
            /* https://developer.github.com/v3/#user-agent-required */
            request.headers["User-Agent"] = "MyAwesomeGitClient";
        }
    }
}
