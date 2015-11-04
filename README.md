CsRestClient
====
![man](img/man.png)<br>
Make Interface All The REST Requests!

Usage with GoogleMaps API
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

ToDo
----
* `await` support
```c#
var ret = await math.Sum(5, 5);
```
