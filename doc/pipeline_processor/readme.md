파이프라인 프로세서
====

공통
----
* 모든 파이프라인 프로세서는 인터페이스를 구현함과 동시에 적용됩니다.

INameProcessor
----
__목적__<br>
* C# API 컨벤션을 REST API 서버 스펙에 맞도록 변경합니다.
```c#
// C# 스타일입니다.
UserProfile GetUserProfile();
```
```c#
// 하지만 대부분의 REST API는 snake_case 스펙을 가집니다.
/user/profile
```


IParameterProcessor
----
__목적__<br>
* C# API의 파라미터 인터페이스를 REST API 스펙에 맞도록 변경합니다.
```c#
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
* 요청에 대해 후처리를 수행합니다.
```C#
// Github.v3 API 스펙에는 'User-Agent' 헤더를 반드시 포함하라고 되어 있습니다.
```
```C#
// 아래와 같이 모든 API마다 userAgent를 파라미터로 설정하는것은 매우 불편합니다.
// 따라서 Github에 대한 모든 요청에 일관성 있는 후처리 루틴이 필요합니다.
GistResponse GetGist([Header] userAgent, [Suffix]id);
```

프로세서 타겟 설정하기
----
`ProcessorTarget` 속성을 이용하여 프로세서가 적용될 인터페이스를 설정할 수 있습니다. 만약 설정하지 않는다면 모든 인터페이스에 대해 적용됩니다.
```C#
// 적용하고 싶은 대상 인터페이스를 배열 형태로 전달합니다.
[ProcessorTarget(new Type[] { typeof(GithubAPI) })]
public class BasicAuthProcessor : IRequestProcessor
{ /* .... */ }
```

프로세서 실행순서 설정하기
----
`ProcessorOrder` 속성을 이용해서 프로세서가 실행되는 순서를 설정할 수 있습니다. 만약 설정하지 않는다면 기본값은 0입니다.<br>
같은 번호의 프로세서끼리의 실행순서는 보장되지 않습니다.
```C#
[ProcessorOrder(1000)]
public class NameProcessor : INameProcessor
{ /* .... */ }
```
