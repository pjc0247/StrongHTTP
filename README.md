StrongHTTP
====
![man](img/man.png)<br>
Make Interface All The REST Requests!
[![Build Status](https://travis-ci.org/pjc0247/CsRestClient.svg?branch=master)](https://travis-ci.org/pjc0247/CsRestClient)

Guide
----
* [Doc](doc)

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

외 많들었나?
----
* 왭 API의 파라미터와 리턴 타입을 정의하는것 만으로도 동작하게 하기 위해서
* 강타입 언어인 C#의 특성을 잘 이용하기 위해서 (VS 인텔리센스, 타입체크 등)
