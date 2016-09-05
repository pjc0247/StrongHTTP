using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CsRestClient
{
    using Pipeline;
    using Attributes;
    using Attributes.Request;

    public class HttpRequest
    {
        public object api { get; private set; }
        public string host { get; private set; }
        public Type type { get; private set; }
        public MethodInfo method { get; private set; }
        public object[] args { get; private set; }

        public Dictionary<string,string> headers { get; private set; }
        public string uri { get; private set; }
        public HttpMethod httpMethod { get; private set; }

        public List<ParameterData> parameterData { get; private set; }

        public HttpRequest(object api, string host, Type type, MethodInfo method, object[] args)
        {
            this.api = api;
            this.host = host;
            this.type = type;
            this.method = method;
            this.args = args;
            
            this.parameterData = BuildParameterData();

            this.ExecuteParameterNameProcessors();
            this.ExecuteParameterProcessors();

            this.headers = BuildHeader();
            this.uri = BuildURI();
            this.httpMethod = method.GetHttpMethod();

            this.ExecuteParameterProcessors();
            this.ExecuteRequestProcessors();
        }

        public HttpResponse GetResponse()
        {
            if (Config.logOutput)
                Dump();

            var req = (HttpWebRequest)HttpWebRequest.Create(uri);

            switch(httpMethod)
            {
                case HttpMethod.Get: req.Method = "GET"; break;
                case HttpMethod.Post: req.Method = "POST"; break;
                case HttpMethod.Put: req.Method = "PUT"; break;
                case HttpMethod.Delete: req.Method = "DELETE"; break;
            }

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

            if (httpMethod.IsPayloadAllowed())
            {
                using (var request = req.GetRequestStream())
                {
                    ParameterType paramType = ParameterType.Ignore;
                    string requestPayload = "";
                    var parameters = new Dictionary<string, object>();

                    foreach(var param in parameterData)
                    {
                        if (param.type == ParameterType.PostJson)
                        {
                            if (paramType == ParameterType.RequestUri)
                                throw new ArgumentException();
                            paramType = ParameterType.PostJson;

                            parameters[param.name] = param.value;
                        }
                        else if (param.type == ParameterType.RequestUri)
                        {
                            if (paramType == ParameterType.PostJson)
                                throw new ArgumentException();
                            paramType = ParameterType.RequestUri;

                            parameters[param.name] = param.value;
                        }
                    }

                    if (paramType == ParameterType.PostJson)
                        requestPayload = JsonConvert.SerializeObject(parameters);
                    else if (paramType == ParameterType.RequestUri)
                        requestPayload = RequestUriBuilder.Build(parameters);

                    if (requestPayload.Length > 0)
                    {
                        var payloadBytes = Encoding.UTF8.GetBytes(requestPayload);
                        request.Write(payloadBytes, 0, payloadBytes.Length);
                    }
                }
            }

            using (var resp = (HttpWebResponse)req.GetResponseWithoutException())
            using (var reader = new StreamReader(resp.GetResponseStream()))
            {
                var body = reader.ReadToEnd();

                var response = new HttpResponse();
                response.statusCode = (int)resp.StatusCode;
                response.statusDescription = resp.StatusDescription;
                response.body = body;

                return response;
            }
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
                data.rawName = param.Name;
                data.position = param.Position;
                data.value = args[param.Position];
                data.type = param.GetParamType(method);

                parameterData.Add(data);
            }

            return parameterData;
        }

        /// <summary>
        /// 프로퍼티 바인딩을 수행한다.
        /// (`:property_name` 처럼 생긴 값)
        /// </summary>
        /// <param name="input">Resource 경로</param>
        /// <returns></returns>
        private string BindColonValues(string input)
        {
            var sb = new StringBuilder(input);
            var regex = new Regex(":([a-zA-Z0-9_]+)");

            foreach (Match match in regex.Matches(input))
            {
                var name = match.Groups[1].Value;
                var property = type.GetProperty(name);

                if (property != null)
                {
                    string value = property.GetValue(api).ToString();

                    sb.Replace(
                        $":{match.Groups[1].Value}",
                        value);
                }
            }
            
            return sb.ToString();
        }
        private string BuildURI()
        {
            var serviceName = type.GetCustomAttribute<Service>()?.path ?? type.Name;
            var apiName = method.GetCustomAttribute<Resource>()?.name ?? method.Name;
            var suffix = "";

            apiName = Uri.EscapeUriString(BindColonValues(apiName));
            apiName = 
                Uri.EscapeUriString(
                    string.Format(
                        apiName,
                        parameterData.Where(m => m.type == ParameterType.Binding).Select(m => m.value).ToArray()));

            this.ExecuteResourceNameProcessors(ref apiName);

            var suffixParams =
                parameterData.Where(m => m.type == ParameterType.Suffix);
            foreach (var param in suffixParams)
            {
                suffix += "/" + param.value.ToString();
            }
            if (apiName.Length == 0)
                suffix = suffix.TrimStart('/');

            var requestUriParams =
                parameterData.Where(m => m.type == ParameterType.RequestUri);
            suffix = "?" + RequestUriBuilder.Build(
                requestUriParams.ToDictionary(x => x.name, y => y.value));

            return $"{host}/{serviceName}/{apiName}{suffix}";
        }

        public void Dump()
        {
            Console.WriteLine($"[HttpRequest  {httpMethod} {uri}]");
            Console.WriteLine($"    - Impl : {type}");
            Console.WriteLine($"    - Host : {host}");
            Console.WriteLine($"    - Headers");
            foreach (var header in headers)
            {
            Console.WriteLine($"         - {header.Key} : {header.Value}");
            }
            Console.WriteLine();
        }
    }
}
