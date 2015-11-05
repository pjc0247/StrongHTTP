요청 경로 설정하기
====

인터페이스 접두사 설정하기
----
```c#
[Service("v1/users")]
public interface UserAPI
{ }
```

메소드별 경로 설정하기
----
```c#
[Service("v1/users")]
public interface UserAPI
{
    /* v1/users/profile */
    [Resource("profile")]
    ProfileResponse GetProfile(string id);
}
```
