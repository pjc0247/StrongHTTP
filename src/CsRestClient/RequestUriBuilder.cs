using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    internal class RequestUriBuilder
    {
        public static string Build(Dictionary<string, object> data)
        {
            var sb = new StringBuilder();
            var first = true;

            foreach (var param in data)
            {
                if (first == false)
                    sb.Append("&");

                sb.Append($"{param.Key}={Uri.EscapeUriString(param.Value.ToString())}");

                first = false;
            }

            return sb.ToString();
        }
    }
}
