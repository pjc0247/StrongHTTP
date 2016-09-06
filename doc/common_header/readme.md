공통 헤더 사용하기
=====

특정 인터페이스의 모든 API가 공통적으로 같은 헤더를 보내야 하는 경우 (ex: 클라이언트 키, 인증 정보)<br>
__WithCommonHeader__기능을 이용해 간편하게 처리할 수 있습니다.

```cs
[Service("v1/language")]
public interface TranslateAPIInterface : WithCommonHeader
{
  /* API INTERFACES... */
}
```

```cs
public class TranslateAPI
{
    public static TranslateAPIInterface Create(string clientId, string clientSecret)
    {
        var api = RemotePoint.Create<TranslateAPIInterface>("https://openapi.naver.com");

        api.commonHeaders = new Dictionary<string, string>()
        {
            ["X-Naver-Client-Id"] = clientId,
            ["X-Naver-Client-Secret"] = clientSecret
        };

        return api;
    }
}
```
