using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class AutoHttpMethod : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class Method : Attribute
    {
    }
}
