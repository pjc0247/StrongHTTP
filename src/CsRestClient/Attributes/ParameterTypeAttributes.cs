using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PostJson : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RequestUri : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Suffix : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Header : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Binding : Attribute
    {
    }
}
