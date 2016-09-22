using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRestClient.NamingConvention
{
    public enum ConventionType
    {
        /// <summary>
        /// for internal use
        /// </summary>
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

        /// <summary>
        /// 기본 설정을 사용한 NamingPolicy를 생성한다.
        ///   * postJson = Snake
        ///   * header = HttpHeader
        ///   * requestUri = Snake
        /// </summary>
        /// <returns></returns>
        /// <seealso cref="ConventionType"/>
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
