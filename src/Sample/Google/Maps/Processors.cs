using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using CsRestClient;
using CsRestClient.Pipeline;
using CsRestClient.Attributes;

namespace Sample.Google.Maps
{
    /// <summary>
    /// MapsAPI의 모든 요청은 API_KEY를 필요로 합니다.
    /// 이 프로세서는 요청에 key 파라미터를 자동으로 추가합니다.
    /// </summary>
    [ProcessorTarget(new Type[] { typeof(MapsAPI) })]
    public class APIKeyProcessor : IParameterProcessor
    {
        public void OnParameter(object api, MethodInfo method, List<ParameterData> param)
        {
            param.Add(new ParameterData()
            {
                type = ParameterType.RequestUri,
                name = "key",
                value = ((MapsAPI)api).apiKey
            });
        }
    }

    /// <summary>
    /// GoogleMapsAPI 스펙은 latlng=11.1111,22.2222 형식으로 좌표를 받지만,
    /// C# 인터페이스는 double lat, double lng 으로 분리하여 받는게 더 편리합니다.
    /// 
    /// 이 프로세서는 lat, lng 처럼 따로받은 파라미터를
    /// API 스펙에 맞는 latlng 으로 합쳐줍니다.
    /// </summary>
    [ProcessorTarget(new Type[] { typeof(MapsAPI) })]
    public class LatLngProcessor : IParameterProcessor
    {
        public void OnParameter(object api, MethodInfo method, List<ParameterData> param)
        {
            if (method.Name == nameof(MapsAPI.ReverseGeocode))
            {
                var lat = param.Find(m => m.name == "lat");
                lat.type = ParameterType.Ignore;
                var lng = param.Find(m => m.name == "lng");
                lng.type = ParameterType.Ignore;

                param.Add(new ParameterData()
                {
                    type = ParameterType.RequestUri,
                    name = "latlng",
                    value = lat.value + "," + lng.value
                });
            }
        }
    }
}
