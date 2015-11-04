using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class Service : Attribute
    {
        public string path { get; private set; }

        public Service(string path)
        {
            this.path = path;
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class Resource : Attribute
    {
        public string name { get; private set; }

        public Resource(string name)
        {
            this.name = name;
        }
    }
}
