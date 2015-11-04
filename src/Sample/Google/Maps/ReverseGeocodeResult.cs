using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Google.Maps
{
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

            public AddressComponent[] address_components;
            public string formatted_address;
        }

        public Result[] results { get; set; }
    }
}
