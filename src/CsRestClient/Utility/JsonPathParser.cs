using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace CsRestClient.Utility
{
    public sealed class JsonPathParser
    {
        public static T ParseObject<T>(string json, string jsonPath)
        {
            JToken jobj = null;
            jobj = JObject.Parse(json);

            var jtoken = jobj.SelectToken(jsonPath);
            if (jtoken == null)
                throw new InvalidOperationException($"JsonPath[{jsonPath}] not avaliable");

            return (T)jtoken.ToObject(typeof(T));
        }
        public static T ParseArray<T>(string json, string jsonPath)
        {
            JToken jobj = null;
            jobj = JArray.Parse(json);

            var jtoken = jobj.SelectToken(jsonPath);
            if (jtoken == null)
                throw new InvalidOperationException($"JsonPath[{jsonPath}] not avaliable");

            return (T)jtoken.ToObject(typeof(T));
        }
    }
}
