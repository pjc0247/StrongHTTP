using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes.Request
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class AutoHttpMethod : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Method : Attribute
    {
        public HttpMethod method { get; }

        public Method(HttpMethod method)
        {
            this.method = method;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Get : Method
    {
        public Get() :
            base(HttpMethod.Get)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class Post : Method
    {
        public Post() :
            base(HttpMethod.Post)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class Put : Method
    {
        public Put() :
            base(HttpMethod.Put)
        {
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class Delete : Method
    {
        public Delete() :
            base(HttpMethod.Delete)
        {
        }
    }
}
