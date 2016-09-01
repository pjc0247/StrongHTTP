using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes.Response
{
    public class StatusCodeAttribute : Attribute
    {
        public int? expectedStatusCode { get; set; }

        public StatusCodeAttribute()
        {
            this.expectedStatusCode = null;
        }
        public StatusCodeAttribute(int expectedStatusCode)
        {
            this.expectedStatusCode = expectedStatusCode;
        }
    }

    /// <summary>
    /// </summary>
    public class JsonPathAttribute : Attribute
    {
        public string jsonPath { get; set; }
        public bool isArray { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="jsonPath">
        /// (see http://www.newtonsoft.com/json/help/html/SelectToken.htm) </param>
        public JsonPathAttribute(string jsonPath, bool isArray = false)
        {
            this.jsonPath = jsonPath;
            this.isArray = isArray;
        }
    }
}
