using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes;

namespace Sample.Github
{
    [ProcessorTarget(new Type[] { typeof(RepoAPI) })]
    public class RepoNameProcessor : INameProcessor
    {
        public string OnParameter(object api, ParameterData param)
        {
            return param.name;
        }
        public string OnResource(object api, string name)
        {
            var repo = ((RepoAPI)api);
            return $"{repo.owner}/{repo.name}/{name}";
        }
    }
    [ProcessorTarget(new Type[] { typeof(GistAPI) })]
    public class GistNameProcessor : INameProcessor
    {
        public string OnParameter(object api, ParameterData param)
        {
            return param.name;
        }
        public string OnResource(object api, string name)
        {
            var gist = ((GistAPI)api);
            return $"{gist.id}/{name}";
        }
    }

    [ProcessorOrder(1000)]
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
