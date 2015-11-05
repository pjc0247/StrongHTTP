파이프라인 프로세서
====

INameProcessor
----
__목적__<br>
* C# API 컨벤션을 REST API 서버 스펙에 맞도록 변경합니다.
```c#
// C# 스타일입니다.
UserProfile GetUserProfile();
```
```
// 하지만 대부분의 REST API는 snake_case 스펙을 가집니다.
/user/profile
```


IParameterProcessor
----
__목적__<br>
* C# API의 파라미터 인터페이스를 REST API 스펙에 맞도록 변경합니다.
```
// google reverse-geocode API는 좌표 정보를 아래와같이 받습니다.
maps/api/geocode/json?latlng=33.33333,11.11111
```
```C#
// 하지만 C# 인터페이스는 아래와 같이 처리하는것이 훨씬 깔끔합니다.
GeocodeResponse ReverseGeocode(double lat, double lng);
```

IRequestProcessor
----
__목적__<br>

프로세서 타겟 설정하기
----

프로세서 실행순서 설정하기
----
