# Com2us ServerCampus

## 소개
> Com2us ServerCampus 1기 본과제 입니다.
> 서머너즈워 게임의 컨텐츠의 일부를 모작하며, WebAPI 서버로 구현할 수 있는 내용으로 구성되어있습니다. 게임의 근간은 스테이지 클리어 형식이고, 스테이지를 클리어하면 보상을 받는 구조입니다.

과제의 내용과 설계, 구현에 대해서는 아래 Gitbook에 메모하며 작업하고 있습니다. 자세한 내용은 아래 링크에서 확인할 수 있습니다.

[https://dong-d.gitbook.io/main-subject/](https://dong-d.gitbook.io/main-subject/)

<br><br>
## Coding conventions
> 이 코드는 Com2us에서 사용하는 코딩 컨벤션을 따릅니다. 내용은 아래와 같습니다.
- [dotnet/runtime 코딩 스타일](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)을 따라간다
- 클래스와 구조체의 이름은 파스칼식 대/소문자를 사용한다.
- 함수
    - 함수의 이름은 파스칼식 대/소문자를 사용한다.
	- 매개 변수 이름에는 카멜식 대/소문자를 사용합니다.
	- 설명이 포함된 매개 변수 이름을 사용합니다.
	- 접근 제한자 중 `private`은 기본 적용이므로 별도로 표기하지 않는다. 예) `public string UserID` (0), `private string UserID` (X)
- 필드 
    - Private, Internal 필드:	 
	    - `_`로 시작해서 `_camelCase` 식을 사용한다.
	    - 필드 이름은 명사 또는 명사구를 사용하여 지정합니다.
		- 정적 필드는 `s_`를 붙인다.
		- thread static 필드는 `t_`를 붙인다
    - Public 필드
     	   - 최대한 쓰지 않는다. 
     	   - 파스칼식 대/소문자 사용
- 상수는 파스칼식 대/소문자를 사용한다.
- 매개 변수 타입을 기반으로 하는 이름이 아닌, 매개 변수의 의미를 기반으로 하는 이름을 사용할 수 있습니다.
- 변수, 함수, 인스턴스등을 하드 프린트하는 대신 nameof("...")을 사용한다 
- 코드 블락 정리
	- [올맨 스타일](http://en.wikipedia.org/wiki/Indent_style#Allman_style) 의 각 중괄호는 새로운 줄에서 시작한다. 조건문이 한줄이라도 중괄호는 사용한다
	- 인덴트는 4빈칸을 사용한다
	- 빈줄은 최대 1개만 사용한다. (가독성용)
	- 코드 끝자락에 빈칸은 생략한다. 
- 닷넷 타입은 기본 타입을 사용한다(예 String(닷넷 기본 타입), string(String의 별칭)
    - 정수 타입은 비트의 크기가 붙은 것을 사용한다. int -> Int32
- 쉽게 읽을 수 있는 식별자 이름을 선택한다. 
    - 예를 들어, 영어에서는 AlignmentHorizontal 라는 속성 이름보다 HorizontalAlignment 라는 이름이 읽기가 더 쉽다.
- 간결성보다는 가독성에 중점을 둔다. 
    - 예를 들어, 전자의 경우 X 축에 대한 참조가 명확하지 않은 ScrollableX라는 속성 이름보다는 CanScrollHorizontally라는 이름이 더 좋다.
- 밑줄, 하이픈 또는 기타 영숫자가 아닌 문자를 사용하지 않는다.
- 헝가리어 표기법을 사용하지 않는다.
- 널리 사용되는 프로그래밍 언어의 키워드와 충돌하는 식별자를 사용하지 않는다.
- **예외적으로 IDE에서 자동으로 포맷팅하여 코딩룰이 바뀌는 경우는 이것에 따라도 괜찮다. 단 이 경우 사용하는 IDE를 일치시킨다**

