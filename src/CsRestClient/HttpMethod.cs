using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete
    }

    internal static class HttpMethodHelper
    {
        public static bool IsPayloadAllowed(this HttpMethod httpMethod)
        {
            switch (httpMethod)
            {
                // FALSE
                case HttpMethod.Get: return false;
                case HttpMethod.Delete: return false;

                // TRUE
                case HttpMethod.Post: return true;
                case HttpMethod.Put: return true;
            }

            throw new InvalidOperationException("Unexpected HttpMethod");
        }
    }
}
