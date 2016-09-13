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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CsRestClient
{
    using Utility;
    using Attributes;
    using Attributes.Response;

    public class RemotePoint
    {
        public string host { get; private set; }

        private RemotePoint(string host)
        {
            this.host = host;
        }

        /// <summary>
        /// This is an internal API. Do NOT call this method directly.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="host"></param>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns>HttpResponse | string | object</returns>
        public static object RPCCall(object obj, string host, Type type, MethodInfo method, object[] args)
        {
            var request = new HttpRequest(obj, host, type, method, args);
            var response = request.GetResponse();
            var returnType = method.ReturnType.Unwrap();
            object value = null;

            if (Config.logOutput)
                Console.WriteLine(response.body);

            if (ProcessResponseAttributes(method, response, out value))
                return value;

            if (returnType == typeof(string))
                return response.body;
            else if (returnType == typeof(HttpResponse))
                return response;
            else
            {
                return JsonConvert.DeserializeObject(response.body, returnType);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="response"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <see cref="JsonPathAttribute"/>
        /// <see cref=" StatusCodeAttribute"/>
        /// <exception cref="InvalidCastException">
        /// StatusCodeAttributes가 지정되어 있지만 메소드 반환 타입이 int 또는 string이 아님.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// JsonPathAttribute에 지정된 경로가 존재하지 않음.
        /// </exception>
        private static bool ProcessResponseAttributes(
            MethodInfo method, HttpResponse response,
            out object value)
        {
            var returnType = method.ReturnType.Unwrap();

            var jsonPathAttr = method.GetCustomAttribute<JsonPathAttribute>();
            if (jsonPathAttr != null)
            {
                JToken jobj = null;
                if (jsonPathAttr.isArray)
                    jobj = JArray.Parse(response.body);
                else
                    jobj = JObject.Parse(response.body); 

                var jtoken = jobj.SelectToken(jsonPathAttr.jsonPath);
                if (jtoken == null)
                    throw new InvalidOperationException($"JsonPath[{jsonPathAttr.jsonPath}] not avaliable");

                value = jtoken.ToObject(returnType);
                return true;
            }

            var statusCodeAttr = method.GetCustomAttribute<StatusCodeAttribute>();
            if (statusCodeAttr != null)
            {
                if (returnType == typeof(string))
                    value = response.statusCode.ToString();
                else if (returnType == typeof(int))
                    value = response.statusCode;
                else
                    throw new InvalidCastException("StatusCodeAttribute::ret type != int or string");

                return true;
            }

            value = null;
            return false;
        }

        public static T Create<T>(string host)
        {
            var type = RuntimeAssemblyPool.GetType<T>(host);

            return (T)Activator.CreateInstance(type);
        }
        public static async Task<T> CreateAsync<T>(string host)
        {
            var type = await RuntimeAssemblyPool.GetTypeAsync<T>(host);
                
            return (T)Activator.CreateInstance(type);
        }
    }
}
