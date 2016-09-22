meta-data
====
각각의 API 인스턴스마다 분리되어 저장되어야 하는 데이터가 있습니다.<br>
예를들어 `Google.Maps` API는 모든 API 호출에 Key를 집어넣어야 합니다.
```c#
var api1 = Google.Maps.APIFactory.Create("YOUR_API_KEY_HERE_1");
var api2 = Google.Maps.APIFactory.Create("YOUR_API_KEY_HERE_2");
```

API 인터페이스에 선언한 __property__들은 `Create`시에 자동으로 구현됩니다.
```c#
public interface MapsAPI
{
    string apiKey { get; set; }
    
    /* .... */
}
```
```c#
public static class MapsAPIFactory
{
    public static MapsAPI Create(string apiKey)
    {
        var api = RemotePoint.Create<MapsAPI>("https://maps.googleapis.com");
        api.apiKey = apiKey;
        return api;
    }
}
```

메타 데이터 바인딩
----
경로를 지정할 때 `:이름`과 같이 작성하면, 런타임에 메타데이터의 값이 바인딩됩니다.
```cs
interface GithubUser {
    string user {get;set;}
    
    [Resource(":user/repo")]
    Repo GetRepo();
}
```
