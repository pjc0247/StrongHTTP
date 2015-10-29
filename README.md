CsRestClient
====
![man](img/man.png)<br>
jsonhttprpc

__RestClientInterface__
```c#
public class MyStrangeClass {
  public string name {get;set;}
  public int level {get;set;}
}
```
```c#
public interface Math {
  [Ret("result")]
  int Sum(int a,int b);
  
  MyStrangeClass Foo();
}
```

__Request__
```c#
var math = RemotePoint.Create<Math>(
  "http://www.rinistudio.com");
  
// http://www.rinistudio.com/math/sum?a=5&b=5
Console.WriteLine(math.Sum(5, 5)); // 10

// http://www.rinistudio.com/math/foo
math.Foo(); // name : hello, level : 10
```

__ApiServer__
```json
{
  "result" : 10
}
```
```json
{
  "name" : "hello",
  "level" : 5
}
```

ToDo
----
* `await` support
```c#
var ret = await math.Sum(5, 5);
```
