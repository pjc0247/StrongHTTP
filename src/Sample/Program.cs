using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes;

using System.Reflection;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var processors2 = Assembly.GetCallingAssembly().GetTypes()
                .Where(m => m.GetInterface(nameof(IRequestProcessor)) != null);

            foreach (var p in processors2)
                Console.WriteLine(p);

            var api = Google.Maps.MapsAPIFactory.Create();

            
            Console.WriteLine(
                api.ReverseGeocode("", 40.714224,-73.961452).results[0].formatted_address);
                
            var github = Github.GithubAPIFactory.Create();

            Console.WriteLine(
                github.GetRepos("pjc0247"));
        }
    }
}
