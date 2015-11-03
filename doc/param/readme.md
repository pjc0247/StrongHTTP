param 전달 타입 설정하기
====

기본값
----
파라미터 타입을 설정하지 않은 모든 파라미터는 __request_uri__타입으로 취급됩니다.

request_uri 타입
----
파라미터가 __request_uri__에 추가되어 전달됩니다.
<br>
__예제__
```c#
void Foo([RequestUri]string name, [RequestUri]int level);
```
```c#
Foo("park", 10);
```
```
GET /foo?name=park&level=10 HTTP/1.1
```

header 타입
----
파라미터가 __http_header__에 추가되어 전달됩니다.
<br>
__예제__
```c#
void Foo([Header]string name, [Header]int level);
```
```c#
Foo("park", 10);
```
```
GET /foo HTTP/1.1
Name : park
Level : 10
```

suffix 타입
----
파라미터가 __request_uri__에 접미사로 추가되어 전달됩니다. 이 경우 파라미터 자체의 네임은 무시되며, 왼쪽에 위치한 파라미터일수록 접미사에 먼저 추가됩니다.
<br>
__예제__
```c#
void Foo([Suffix]string name, [Suffix]int level);
```
```c#
Foo("park", 10);
```
```
GET /foo/park/10 HTTP/1.1
```

post_json 타입
----
파라미터가 __post__에 json 형식으로 추가되어 전달됩니다.
<br>
__예제__
```c#
void Foo([PostJson]string name, [PostJson]int level);
```
```c#
Foo("park", 10);
```
```
GET /foo HTTP/1.1

{
  "name" : "park",
  "level" : 10
}
```

혼합하여 사용하기
----
모든 파라미터 타입은 혼용될 수 있습니다.
<br>
__예제__
```c#
void Foo([Suffix]string name, [RequestUri]int level);
```
```c#
Foo("park", 10);
```
```
GET /foo?level&=10 HTTP/1.1

{
  "name" : "park"
}
```

중복하여 사용하기
----
한개의 파라미터에 두가지 이상의 속성을 적용할 수 있습니다.
<br>
__예제__
```c#
void Foo([PostJson][Header]string name, [PostJson][RequestUri]int level);
```
```c#
Foo("park", 10);
```
```
GET /foo?level=10 HTTP/1.1
Name : park

{
  "name" : "park",
  "level" : 10
}
```

주의
----
이 페이지의 모든 예제에서는 모든 요청이 GET, HTTP/1.1 으로 동작한다는 가정 하에 작성되었습니다. 하지만 환경 설정 또는 버전별 구현에 따라 GET, HTTP/1.1 스펙으로 동작하지 않을 수 있습니다.
