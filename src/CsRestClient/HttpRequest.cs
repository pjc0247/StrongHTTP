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

        internal HttpRequest(object api, string host, Type type, MethodInfo method, object[] args)
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

            var httpMethodString = "GET";
            byte[] payload = null;

            switch(httpMethod)
            {
                case HttpMethod.Get: httpMethodString = "GET"; break;
                case HttpMethod.Post: httpMethodString = "POST"; break;
                case HttpMethod.Put: httpMethodString = "PUT"; break;
                case HttpMethod.Delete: httpMethodString = "DELETE"; break;
            }

            if (httpMethod.IsPayloadAllowed())
            {
                ParameterType paramType = ParameterType.Ignore;
                string requestPayload = "";
                var parameters = new Dictionary<string, object>();

                foreach (var param in parameterData)
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
                {
                    requestPayload = JsonConvert.SerializeObject(parameters);

                    if (headers.ContainsKey(HeaderKey.ContentType) == false)
                        headers[HeaderKey.ContentType] = "application/json";
                }
                else if (paramType == ParameterType.RequestUri)
                {
                    requestPayload = RequestUriBuilder.Build(parameters);

                    if (headers.ContainsKey(HeaderKey.ContentType) == false)
                        headers[HeaderKey.ContentType] = "application/x-www-form-urlencoded";
                }

                if (requestPayload.Length > 0)
                {
                    payload = Encoding.UTF8.GetBytes(requestPayload);
                }
            }

            return PerformRequest(uri, httpMethodString, headers, payload);
        }

        private HttpResponse PerformRequest(
            string uri, string httpMethod,
            Dictionary<string, string> headers,
            byte[] payload)
        {
            var req = WebRequest.CreateHttp(uri);

            req.Method = httpMethod;

            foreach (var header in headers)
            {
                if (string.Compare(HeaderKey.UserAgent, header.Key, true) == 0)
                    req.UserAgent = header.Value;
                else if (string.Compare(HeaderKey.ContentType, header.Key, true) == 0)
                    req.ContentType = header.Value;
                else if (string.Compare(HeaderKey.Host, header.Key, true) == 0)
                    req.Host = header.Value;
                else
                    req.Headers.Set(header.Key, header.Value);
            }

            if (payload?.Length > 0)
            {
                using (var request = req.GetRequestStream())
                {
                    request.Write(payload, 0, payload.Length);
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

        /// <summary>
        /// 요청에 포함되어야하는 헤더값들을 가져온다.
        ///   * ParameterType.Header 로 지정된 파라미터
        ///   * CommonHeader 로 지정된 값
        /// </summary>
        /// <returns>헤더 목록</returns>
        /// <see cref="WithCommonHeader"/>
        private Dictionary<string,string> BuildHeader()
        {
            Dictionary<string, string> headers = null;
            var headerParams =
                parameterData.Where(m => m.type == ParameterType.Header);
            var commonHeaders =
                api.GetType().GetProperty(nameof(WithCommonHeader.commonHeaders));

            if (commonHeaders != null)
                headers = (Dictionary<string, string>)commonHeaders.GetValue(api);
            else
                headers = new Dictionary<string, string>();

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
                        Uri.EscapeUriString(value));
                }
            }
            
            return sb.ToString();
        }
        private string BuildURI()
        {
            var serviceName = type.GetCustomAttribute<Service>()?.path ?? type.Name;
            var apiName = method.GetCustomAttribute<Resource>()?.name ?? method.Name;
            var suffix = "";

            apiName = BindColonValues(apiName);
            apiName = 
                string.Format(
                    apiName,
                    parameterData
                        .Where(m => m.type == ParameterType.Binding)
                        .Select(m => Uri.EscapeUriString(m.value.ToString()))
                        .ToArray());

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
            if (requestUriParams.Count() != 0)
            {
                suffix = "?" + RequestUriBuilder.Build(
                    requestUriParams.ToDictionary(x => x.name, y => y.value));
            }

            return $"{host}/{serviceName}/{apiName}{suffix}";
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"[HttpRequest  {httpMethod} {uri}]\r\n");
            sb.Append($"    - Impl : {type}\r\n");
            sb.Append($"    - Host : {host}\r\n");
            sb.Append($"    - Headers");
            foreach (var header in headers)
            {
                sb.Append($"         - {header.Key} : {header.Value}\r\n");
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
