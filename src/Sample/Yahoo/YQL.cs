using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Utility;
using CsRestClient.Attributes.Request;
using CsRestClient.Attributes.Response;

using Newtonsoft.Json;

namespace Sample.Yahoo
{
    [Service("v1/public")]
    public interface YQLInterface
    {
        [Resource("yql?q={0}&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys")]
        Task<string> Query([Binding]string query);
    }

    public class YQL
    {
        private static YQLInterface impl
        {
            get
            {
                return RemotePoint.Create<YQLInterface>("https://query.yahooapis.com");
            }
        }

        public static Task<string> Query(string query)
        {
            return impl.Query(query);
        }

        public static async Task<float> GetCurrencyRate(string code1, string code2)
        {
            string json = await impl.Query(
                $"select * from yahoo.finance.xchange where pair in (\"{code1 + code2}\")");
            return JsonPathParser.ParseObject<float>(json, "query.results.rate.Rate");
        }
        public static async Task<CurrencyInfo> GetCurrencyInfo(string code1, string code2)
        {
            string json = await impl.Query(
                $"select * from yahoo.finance.xchange where pair in (\"{code1 + code2}\")");
            return JsonPathParser.ParseObject<CurrencyInfo>(json, "query.results.rate");
        }
    }

    public class CurrencyInfo
    {
        public string Name { get; set; }
        public float Rate { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public float Ask { get; set; }
        public float Bid { get; set; }
    }
}
