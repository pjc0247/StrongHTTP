using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = Google.Maps.MapsAPIFactory.Create();

            Console.WriteLine(
                api.ReverseGeocode("", "40.714224,-73.961452"));
        }
    }
}
