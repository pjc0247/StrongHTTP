CsRestClient
====
![man](img/man.png)<br>
Make Interface All The REST Requests!
[![Build Status](https://travis-ci.org/pjc0247/CsRestClient.svg?branch=master)](https://travis-ci.org/pjc0247/CsRestClient)

Usage with NaverTranslate API
----
https://github.com/pjc0247/NaverTranslator

Usage with GoogleMaps API [지울거]
----
[Full Sourcecode](https://github.com/pjc0247/CsRestClient/tree/master/src/Sample/Google/Maps)<br>
[API Response](https://maps.googleapis.com/maps/api/geocode/json?latlng=40.714224,-73.961452&)<br>
```c#
[Service("maps/api/geocode")]
public interface MapsAPI
{
    [Resource("json")]
    ReverseGeocodeResult ReverseGeocode(
        [RequestUri]double lat, [RequestUri]double lng);
}
```
```c#
var api = Google.Maps.MapsAPIFactory.Create();
var response = api.ReverseGeocode(40.714224,-73.961452);

foreach(var result in response.results) {
  Console.WriteLine(result.formatted_address);
}
```

Features
----
* `async` support
* 없음

Sample Projects
----
* [Cs.Github.v3](https://github.com/pjc0247/Cs.Github.v3)

