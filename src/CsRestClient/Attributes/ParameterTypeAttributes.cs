using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.Attributes.Request
{
    using NamingConvention;

    public class RequestAttribute : Attribute
    {
        internal ConventionType convention { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class PostJson : RequestAttribute
    {
        public PostJson()
        {
            this.convention = ConventionType.None;
        }
        public PostJson(ConventionType convention)
        {
            this.convention = convention;
        }
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RequestUri : RequestAttribute
    {
        public RequestUri()
        {
            this.convention = ConventionType.None;
        }
        public RequestUri(ConventionType convention)
        {
            this.convention = convention;
        }
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Suffix : RequestAttribute
    {
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Header : RequestAttribute
    {
        public Header()
        {
            this.convention = ConventionType.None;
        }
        public Header(ConventionType convention)
        {
            this.convention = convention;
        }
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Binding : RequestAttribute
    {
    }
}
