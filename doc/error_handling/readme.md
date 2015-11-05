에러 핸들링
----

슬프게도, HttpRequest 보내면 항상 200에 딱 떨어지는 응답이 오지 않는 경우가 있습니다. 이 페이지에서는 예외 경우에 대한 처리 방법을 보여줍니다.

현재
----
* 200이 아닌 리스폰스가 올 경우 [WebException](https://msdn.microsoft.com/ko-kr/library/system.net.webexception(v=vs.110).aspx)이 발생합니다.
* (리턴을 커스텀 클래스로 받을 때) json 파싱에 실패할 경우 Json.Net의 익셉션이 발생합니다.

ToDo
----
* CsRestClient의 자체 익셉션 생성
* 익셉션 정책 설정 가능
