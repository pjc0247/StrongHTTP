StrongHTTP
====
![man](img/man.png)<br>
Make Interface All the REST Requests!<br>
[![Build Status](https://travis-ci.org/pjc0247/StrongHTTP.svg?branch=master)](https://travis-ci.org/pjc0247/StrongHTTP)

* [documentation](doc)


Overview
----
If you want to call REST method which has a specification like below:
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

just make a interface and describe your API.
```cs
[Service("/mymath")]
interface MyRESTClient {
    [Resource("/sum")]
    [ResponsePath("result")]
    int Sum(int x, int y);
    
    // also supports `Async` functions.
    Task<int> SumAsync(int x, int y);
}
```

then __StrongHTTP__ will make a proper implementation for you.
```cs
var client = RemotePoint.Create<MyRESTClient>("https://api.example.com");

var result = await client.SumAsync(10, 20); // 30
```


Usage with NaverTranslate API
----
https://github.com/pjc0247/NaverTranslator

Features
----
* supports `async` 
* 없음

외 많들었나?
----
* 왭 API의 파라미터와 리턴 타입을 정의하는것 만으로도 동작하게 하기 위해서
* 강타입 언어인 C#의 특성을 잘 이용하기 위해서 (VS 인텔리센스, 타입체크 등)
