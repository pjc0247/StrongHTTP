using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using CsRestClient;
using CsRestClient.Attributes.Request;

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

        [Resource("json")]
        GeocodeResult ReverseGeocodeWithPlaceId([RequestUri]string place_id);
    }

    public static class MapsAPIFactory
    {
        public static MapsAPI Create(string apiKey)
        {
            var api = RemotePoint.Create<MapsAPI>("https://maps.googleapis.com");
            api.apiKey = apiKey;
            return api;
        }
    }
}
