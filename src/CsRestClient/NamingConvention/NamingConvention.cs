using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.NamingConvention
{
    public enum ConventionType
    {
        None,
        Dontcare,
        Camel,
        Pascal,
        HttpHeader,
        Snake
    }

    public class NamingPolicy
    {
        public ConventionType postJson { get; set; }
        public ConventionType header { get; set; }
        public ConventionType requestUri { get; set; }

        public static NamingPolicy CreateDefault()
        {
            NamingPolicy policy = new NamingPolicy();
            policy.postJson = ConventionType.Snake;
            policy.header = ConventionType.HttpHeader;
            policy.requestUri = ConventionType.Snake;
            return policy;
        }
    }

}
