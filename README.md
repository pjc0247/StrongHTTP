StrongHTTP
====
![man](img/man.png)<br>
Make Interface All The REST Requests!
[![Build Status](https://travis-ci.org/pjc0247/StrongHTTP.svg?branch=master)](https://travis-ci.org/pjc0247/StrongHTTP)

Guide
----
* [Doc](doc)


무엇인가
----

만약 아래와 같은 스펙을 가지는 REST API가 있다면

```
GET /mymath/sum
{
    "x" : NUMBER,
    "y" : NUMBER
}
```
```
{
    "result" : NUMBER
}
```

아래와 같이 쓰면 됩니다.
```cs
[Service("/mymath")]
interface MyRESTClient {
    [Resource("/sum")]
    [ResponsePath("result")]
    int Sum(int x, int y);
}
```
```cs
var client = RemotePoint.Create<MyRESTClient>("https://api.example.com");

var result = client.Sum(10, 20); // 30
```
끝

Usage with NaverTranslate API
----
https://github.com/pjc0247/NaverTranslator

Features
----
* `async` support
* 없음

외 많들었나?
----
* 왭 API의 파라미터와 리턴 타입을 정의하는것 만으로도 동작하게 하기 위해서
* 강타입 언어인 C#의 특성을 잘 이용하기 위해서 (VS 인텔리센스, 타입체크 등)
