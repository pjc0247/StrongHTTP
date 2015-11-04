using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using CsRestClient;
using CsRestClient.Attributes;

namespace Sample.Google.Maps
{
    [Service("maps/api/geocode")]
    public interface MapsAPI : IRestAPI
    {
        string apiKey { get; set; }

        [Resource("json")]
        GeocodeResult Geocode([RequestUri]string address);

        [Resource("json")]
        GeocodeResult ReverseGeocode([RequestUri]double lat, [RequestUri]double lng);
    }

    public static class MapsAPIFactory
    {
        public static MapsAPI Create(string appKey)
        {
            var api = RemotePoint.Create<MapsAPI>("https://maps.googleapis.com");
            api.apiKey = appKey;
            return api;
        }
    }

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

    [ProcessorTarget(new Type[] { typeof(MapsAPI) })]
    public class LatLngProcessor : IParameterProcessor
    {
        public void OnParameter(object api, MethodInfo method, List<ParameterData> param)
        {
            if(method.Name == nameof(MapsAPI.ReverseGeocode))
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
