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
            var suffix = "";
            var first = true;
            foreach (var param in data)
            {
                if (first)
                    suffix = "?";
                else suffix += "&";

                suffix += $"{param.Key}={param.Value}";

                first = false;
            }

            return suffix;
        }
    }
}
