using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Pipeline
{
    public interface IRequestProcessor
    {
        void OnRequest(object api, HttpRequest request);
    }
}
