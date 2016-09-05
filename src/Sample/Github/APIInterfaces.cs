using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes.Request;
using CsRestClient.Attributes.Response;

namespace Sample.Github
{
    public interface APIBase
    {
        string authString { get; set; }
    }

    [AutoHttpMethod]
    [Service("users")]
    public interface UserAPI : APIBase
    {
        string id { get; set; } 

        [Resource(":id/gists")]
        string GetGists([Binding]string username);

        [JsonPath("[0].url",isArray = true)]
        [Resource(":id/gists")]
        Task<string> GetGistsAsync([Binding]string username);

        [Resource("repos")]
        string GetRepos();
    }

    [AutoHttpMethod]
    [Service("repos")]
    public interface RepoAPI : APIBase
    {
        string owner { get; set; }
        string name { get; set; }

        string GetReadme();
    }
    [AutoHttpMethod]
    [Service("gists")]
    public interface GistAPI : APIBase
    {
        string id { get; set; }

        [Resource("")]
        string Get();
        [Resource("")]
        string Get([Suffix]string revision);

        [Put]
        [Resource("star")]
        string Star();
        [Put]
        [Resource("star")]
        string Unstar();
    }
}
