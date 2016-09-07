using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes.Request;
using CsRestClient.Attributes.Response;

namespace Sample.Yahoo
{
    [Service("v1/public")]
    public interface YQL
    {
        [Resource("yql?q=select%20*%20from%20yahoo.finance.xchange%20where%20pair%20in%20(%22{0}{1}%22)%3B&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys")]
        [JsonPath("query.results.rate.Rate")]
        Task<float> GetCurrencyRate(
            [Binding]string code1, [Binding]string code2);

        [Resource("yql?q=select%20*%20from%20yahoo.finance.xchange%20where%20pair%20in%20(%22{0}{1}%22)%3B&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys")]
        [JsonPath("query.results.rate")]
        Task<CurrencyInfo> GetCurrencyInfo(
            [Binding]string code1, [Binding]string code2);
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
