using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CsRestClient
{
    using CsRestClient.Attributes;

    public class MyHeaderCollection : WebHeaderCollection
    {
        public new void Set(string name, string value)
        {
            AddWithoutValidate(name, value);
        }
        //or
        public new string this[string name]
        {
            get { return base[name]; }
            set { AddWithoutValidate(name, value); }
        }
    }

    public class HttpRequest
    {
        public string host { get; private set; }
        public Type type { get; private set; }
        public MethodInfo method { get; private set; }
        public object[] args { get; private set; }

        public Dictionary<string,string> headers { get; private set; }
        public string uri { get; private set; }
        public HttpMethod httpMethod { get; private set; }

        private List<ParameterData> parameterData { get; set; }

        public HttpRequest(string host, Type type, MethodInfo method, object[] args)
        {
            this.host = host;
            this.type = type;
            this.method = method;
            this.args = args;
            
            this.parameterData = BuildParameterData();

            var processors = Assembly.GetEntryAssembly().GetTypes()
                .Where(m => m.GetInterface(nameof(IParameterProcessor)) != null)
                .Where(m => m.GetCustomAttribute<ProcessorTarget>()?.targets.Contains(type) ?? false);

            foreach (var processor in processors)
            {
                var p = (IParameterProcessor)Activator.CreateInstance(processor);
                p.OnParameter(method, parameterData);    
            }

            this.headers = BuildHeader();
            this.uri = BuildURI();
            this.httpMethod = method.GetHttpMethod();

            processors = Assembly.GetEntryAssembly().GetTypes()
                .Where(m => m.GetInterface(nameof(IRequestProcessor)) != null)
                .Where(m => m.GetCustomAttribute<ProcessorTarget>()?.targets.Contains(type) ?? false);

            foreach (var processor in processors)
            {
                var p = (IRequestProcessor)Activator.CreateInstance(processor);
                p.OnRequest(this);
            }
        }

        public HttpResponse GetResponse()
        {
            var req = (HttpWebRequest)HttpWebRequest.Create(uri);
            Console.WriteLine(uri);

            foreach (var header in headers)
            {
                if (string.Compare("User-Agent", header.Key, true) == 0)
                    req.UserAgent = header.Value;
                else if (string.Compare("Content-Type", header.Key, true) == 0)
                    req.ContentType = header.Value;
                else if (string.Compare("Host", header.Key, true) == 0)
                    req.Host = header.Value;
                else
                    req.Headers.Set(header.Key, header.Value);
            }
            var resp = (HttpWebResponse)req.GetResponse();

            var reader = new StreamReader(resp.GetResponseStream());
            var body = reader.ReadToEnd();

            var response = new HttpResponse();
            response.statusCode = (int)resp.StatusCode;
            response.statusDescription = resp.StatusDescription;
            response.body = body;

            return response;
        }

        private Dictionary<string,string> BuildHeader()
        {
            var headers = new Dictionary<string, string>();
            var headerParams =
                parameterData.Where(m => m.type == ParameterType.Header);

            foreach(var param in headerParams)
            {
                headers[param.name] = param.value.ToString();
            }

            return headers;
        }
        private List<ParameterData> BuildParameterData()
        {
            var parameterData = new List<ParameterData>();

            foreach (var param in method.GetParameters())
            {
                var data = new ParameterData();
                data.name = param.Name;
                data.position = param.Position;
                data.value = args[param.Position];
                data.type = param.GetParamType(method);

                parameterData.Add(data);
            }

            return parameterData;
        }
        private string BuildURI()
        {
            var serviceName = type.GetCustomAttribute<Service>()?.path ?? type.Name;
            var apiName = method.GetCustomAttribute<Resource>()?.name ?? method.Name;
            var suffix = "";

            apiName = string.Format(
                apiName,
                parameterData.Where(m => m.type == ParameterType.Binding).Select(m => m.value).ToArray());
            var suffixParams =
                parameterData.Where(m => m.type == ParameterType.Suffix);
            foreach (var param in suffixParams)
            {
                if (suffix.Length == 0)
                    suffix = "/";

                suffix += param.value.ToString();
            }

            var requestUriParams =
                parameterData.Where(m => m.type == ParameterType.RequestUri);
            var first = true;
            foreach (var param in requestUriParams)
            {
                if (first)
                    suffix = "?";
                else suffix += "&";

                suffix += $"{param.name}={param.value}";

                first = false;
            }

            return $"{host}/{serviceName}/{apiName}{suffix}";
        }
    }
}
