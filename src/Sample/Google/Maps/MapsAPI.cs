using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsRestClient;
using CsRestClient.Attributes;

namespace Sample.Google.Maps
{
    [Service("maps/api/geocode")]
    public interface MapsAPI
    {
        [Resource("json")]
        string ReverseGeocode([RequestUri]string key, [RequestUri]string latlng);
    }

    public static class MapsAPIFactory
    {
        public static MapsAPI Create()
        {
            return RemotePoint.Create<MapsAPI>("https://maps.googleapis.com");
        }
    }
}
