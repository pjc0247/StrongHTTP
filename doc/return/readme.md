API 실행 결과 받아오기
====

string 타입
----
API 반환값을 __string__으로 설정합니다. 이 경우 HttpResponse의 body가 그대로 전달됩니다.
```c#
string Foo(int id);
```

HttpResponse 타입
----
API 반환값을 __HttpResponse__로 설정합니다. 이 경우 body 뿐만 아니라 상태 코드 등 다른 정보도 같이 받아올 수 있습니다.
```c#
HttpResponse Foo(int id);
```
```c#
var response = Foo(10);

response.statusCode;
response.statusDescription;
response.body;
```

커스텀 클래스 타입
----
HttpResponse의 body가 Json이라고 가정하고, body를 커스텀 클래스 타입으로 Deserialize합니다. Json Deserialize 메소드는 [Json.Net](http://www.newtonsoft.com/json)을 사용합니다.<br>
이 경우 body가 올바르지 않은 json 포멧으로 구성되었을 경우 익셉션이 발생합니다.
```c#
public class MyFooResult {
  public string name {get;set;}
  public int level {get;set;}
}
```
```c#
MyFooResult Foo(int id);
```
```c#
var response = Foo(10);

response.name;
response.level;
```

Task`T 타입
----
리턴값을 `Task<T>`으로 선언했을 경우 자동으로 비동기로 동작합니다.<br>
__T__에 대한 제약은 없으며, 위에 설명된 규칙을 그대로 따릅니다.

```cs
Task<string> GetNicknameAsync();
```
```cs
string nickname = await impl.GetNicknameAsync();
```
