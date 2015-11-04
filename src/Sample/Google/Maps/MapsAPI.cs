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
    public interface MapsAPI
    {
        [Resource("json")]
        string ReverseGeocode([RequestUri]string key, [RequestUri]double lat, [RequestUri]double lng);
    }

    public static class MapsAPIFactory
    {
        public static MapsAPI Create()
        {
            return RemotePoint.Create<MapsAPI>("https://maps.googleapis.com");
        }
    }

    [ProcessorTarget(new Type[] { typeof(MapsAPI) })]
    public class LatLngProcessor : INameProcessor
    {
        public void OnParameter(MethodInfo method, List<ParameterData> param)
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
