요청의 HttpMethod 설정하기
====

기본값
----
아무런 HttpMethod도 설정되지 않으면, 항상 "GET"으로 동작합니다.

API별 HttpMethod 설정하기
----
```c#
class Users {
  [Get]
  UserInfo GetById(string userId);
  
  [Post]
  bool Create(UserInfo userInfo);
}
```

네이밍 컨벤션 사용하기
----
```c#
[AutoHttpMethod]
class Users {
  UserInfo GetById(string userId);
  bool Create(UserInfo userInfo);
}
```

 HttpMethod | 접두사
------      |-----
 GET        | get
            | query
 POST       | create
 PUT        | update
            | apply
 DELETE     | delete
            | remove
