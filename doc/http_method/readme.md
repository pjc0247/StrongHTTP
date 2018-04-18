HTTP METHOD (GET, POST, PUT, DELETE)
====

Default
----
아무런 HttpMethod도 설정되지 않으면, 항상 "GET"으로 동작합니다.

API별 HttpMethod 설정하기
----
API별로 수동으로 HttpMethod를 설정하는 방법입니다.
```c#
class Users {
  [Get]
  UserInfo GetById(string userId);
  
  [Post]
  bool Create(UserInfo userInfo);
}
```

Use predefined naming convention
----
__CsRestClient__에서 제공하는 네이밍 컨벤션에 맞게 API를 네이밍하면, 이름에 기반해 자동으로 HttpMethod를 유추합니다. 이 경우 반드시 클래스에 `[AutoHttpMethod]` 속성을 추가해야 합니다.
```c#
[AutoHttpMethod]
class Users {
  UserInfo GetById(string userId);
  bool Create(UserInfo userInfo);
}
```


 HttpMethod | 접두사
------      | -----
 GET        | get
            | query
 POST       | create
 PUT        | update
            | apply
 DELETE     | delete
            | remove
