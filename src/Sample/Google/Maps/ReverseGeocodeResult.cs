using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Google.Maps
{
    /* response sample */
    /* https://maps.googleapis.com/maps/api/geocode/json?latlng=40.714224,-73.961452& */
    public class ReverseGeocodeResult
    {
        public class Result
        {
            public class AddressComponent
            {
                public string long_name { get; set; }
                public string short_name { get; set; }
                public string[] types { get; set; }
            }

            public AddressComponent[] address_components { get; set; }
            public string formatted_address { get; set; }
            public string place_id { get; set; }
            public string[] types { get; set; }
        }

        public Result[] results { get; set; }
    }
}
