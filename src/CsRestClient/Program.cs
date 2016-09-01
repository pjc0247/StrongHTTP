using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Net;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CsRestClient
{
    using CsRestClient.Attributes.Request;

    [Service("maps/api/geocode")]
    public interface Sample
    {
        int Sum(int a, int b);

        string ReverseGeocode([RequestUri]string key, [RequestUri]string latlng);
    }

    public class HttpParameter
    {
        public string name { get; set; }
        public string value { get; set; }

    }
    public class RequestPipeline
    {
        public void OnRequest()
        {

        }
    }

    public static class CaseConv
    {
        private static string MakeDelimString(this string target, string delim)
        {
            return target.
                Select((m, i) => i > 0 && char.IsUpper(m) ? delim + m.ToString() : m.ToString())
                .Aggregate((a, b) => a + b)
                .ToLower();
        }
        public static string ToSnakeCase(this string target)
        {
            return target.MakeDelimString("_");
        }
        public static string ToUriPath(this string target)
        {
            return target.MakeDelimString("/");
        }
    }

    public class ServerResponseException : Exception
    {
        public Dictionary<string, object> body { get; private set; }

        public ServerResponseException(Dictionary<string,object> body)
        {
            this.body = body;
        }
    }
    public class Config
    {
        public delegate string UriProcessor(Type type, MethodInfo method);

        public UriProcessor uriProcessor { get; set; }

        public static Config defaults {
            get
            {
                var config = new Config();
                return config;
            }
        }
    }
    

    public class Program
    {
        public static int Sum(int a,int b)
        {
            Console.WriteLine(a);
            Console.WriteLine(b);
            return a + b;
        }
        static void Main(string[] args)
        {
            Console.WriteLine(
                RemotePoint.Create<Sample>("https://maps.googleapis.com")
                .ReverseGeocode("", "1600+Amphitheatre+Parkway,+Mountain+View,+CA"));
        }
    }
}
