using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient
{
    public class HttpResponse
    {
        public int statusCode { get; internal set; }
        public string statusDescription { get; internal set; }
        public string body { get; internal set; }
    }
}
