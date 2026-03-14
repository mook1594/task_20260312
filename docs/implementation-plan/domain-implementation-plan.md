# Domain 최종 구현 문서

## 1. 문서 목적

이 문서는 `EmployeeContacts.Domain` 구현의 최종 기준이다.

- Domain 계층의 구현 범위와 비범위를 확정한다.
- 미결정 사항을 모두 결정해 구현 중 선택지를 남기지 않는다.
- 테스트 우선 순서와 완료 기준을 명확히 한다.

이 문서의 기준이 기존 초안보다 우선한다.

## 2. 구현 대상과 비대상

### 구현 대상

- `Employee` Entity 겸 Aggregate Root
- `EmployeeName`
- `EmployeeEmail`
- `EmployeePhoneNumber`
- `DomainException`
- `EmployeeDomainException` 및 `Employee` 전용 예외들
- Domain 단위 테스트

### 구현 대상 아님

- 저장소 인터페이스
- 이메일/전화번호 유일성 검사
- CSV/JSON 파싱
- `yyyy-MM-dd` 문자열 파싱
- 요청/응답 DTO
- EF Core 매핑
- ProblemDetails 응답 계약

## 3. 최종 결정 사항

### 3.0 Aggregate 경계

이번 Domain 구현에서 Aggregate 경계는 `Employee` 단건이다.

결정:

- `Employee`는 단일 Entity이자 Aggregate Root이다.
- 여러 `Employee`를 묶는 별도 Aggregate는 만들지 않는다.
- 여러 직원의 등록, 중복 검사, 일괄 처리 흐름은 Application에서 다룬다.

결정 이유:

- 현재 불변식은 모두 직원 1명 단위에서 완결된다.
- 여러 직원을 하나의 트랜잭션 경계로 묶어야 하는 Domain 규칙이 없다.
- 집합 관리 책임을 Domain Aggregate에 넣으면 초기 설계가 과도하게 무거워진다.

### 3.1 `Employee` 속성

Domain 1차 구현의 `Employee`는 아래 속성만 가진다.

- `Guid Id`
- `EmployeeName Name`
- `EmployeeEmail Email`
- `EmployeePhoneNumber PhoneNumber`
- `DateOnly Joined`
- `DateTimeOffset CreatedAt`
- `DateTimeOffset UpdatedAt`

결정 이유:

- 생성 및 변경 시점을 Domain 모델에 포함하면 감사 추적 기준이 명확해진다.
- 시간은 타임존 오프셋이 포함된 값으로 저장해 계층 간 해석 차이를 줄인다.
- 기본 저장 기준은 `+00:00` 오프셋의 UTC 시간이다.

### 3.2 생성 방식

`Employee`는 공개 생성자를 두지 않고 정적 팩터리만 제공한다.

```csharp
public sealed class Employee
{
    private Employee(
        Guid id,
        EmployeeName name,
        EmployeeEmail email,
        EmployeePhoneNumber phoneNumber,
        DateOnly joined,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        _name = name;
        _email = email;
        _phoneNumber = phoneNumber;
        Joined = joined;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }
    public EmployeeName Name { get; }
    public EmployeeEmail Email { get; }
    public EmployeePhoneNumber PhoneNumber { get; }
    public DateOnly Joined { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Employee Create(
        Guid id,
        EmployeeName name,
        EmployeeEmail email,
        EmployeePhoneNumber phoneNumber,
        DateOnly joined)
    {
        ...
    }
}
```

결정 이유:

- 생성 시점에 모든 불변식을 강제할 수 있다.
- Application이 값 객체를 통해 정규화와 검증을 명시적으로 수행하게 된다.
- 이후 수정 행위 메서드 추가가 쉽다.

### 3.3 값 객체 사용 방식

값 객체는 `Employee`의 공개 계약에도 사용한다.

결정:

- `Employee.Name`은 `EmployeeName`
- `Employee.Email`은 `EmployeeEmail`
- `Employee.PhoneNumber`는 `EmployeePhoneNumber`
- API 응답 바디의 필드명만 계약에 따라 `tel`로 매핑한다.

결정 이유:

- 정규화된 값이라는 의미를 타입 수준에서 유지할 수 있다.
- Domain 불변식의 우발적 우회를 줄인다.
- 현재 구현 및 테스트와 일치한다.

### 3.4 예외 전략

`Employee` 도메인은 공통 `DomainException`을 기반으로 한 전용 예외 계층을 사용한다.

결정:

- 공통 베이스: `EmployeeDomainException : DomainException`
- 규칙별 예외:
  - `EmployeeNameRequiredException`
  - `EmployeeEmailInvalidException`
  - `EmployeePhoneNumberInvalidException`
  - `EmployeeJoinedRequiredException`

결정 이유:

- 도메인 규칙을 에러 코드뿐 아니라 타입으로도 표현할 수 있다.
- Application이 필요하면 `EmployeeDomainException` 전체 또는 개별 예외 타입으로 분기할 수 있다.
- `Code`와 `Detail` 규약은 유지하면서도 정적 에러 정의 의존을 제거할 수 있다.

### 3.5 입사일 규칙

Domain은 `DateOnly`를 직접 받는다.

적용 규칙:

- `DateOnly.MinValue`는 허용하지 않는다.
- 미래 날짜 금지는 이번 구현에 포함하지 않는다.

결정 이유:

- 요구사항에 미래 날짜 금지 규칙이 없다.
- 불필요한 비즈니스 규칙 추정을 피한다.

## 4. 최종 폴더 구조

```text
src/EmployeeContacts.Domain
├─ Common
│  ├─ DomainError.cs
│  └─ DomainException.cs
└─ Employees
   ├─ Exceptions
   │  ├─ EmployeeDomainException.cs
   │  ├─ EmployeeEmailInvalidException.cs
   │  ├─ EmployeeJoinedRequiredException.cs
   │  ├─ EmployeeNameRequiredException.cs
   │  └─ EmployeePhoneNumberInvalidException.cs
   ├─ Employee.cs
   └─ ValueObjects
      ├─ EmployeeEmail.cs
      ├─ EmployeeName.cs
      └─ EmployeePhoneNumber.cs
```

테스트 구조는 아래로 확정한다.

```text
tests/EmployeeContacts.Domain.Tests
├─ Common
│  └─ DomainExceptionTests.cs
└─ Employees
   ├─ EmployeeTests.cs
   └─ ValueObjects
      ├─ EmployeeEmailTests.cs
      ├─ EmployeeNameTests.cs
      └─ EmployeePhoneNumberTests.cs
```

구조 원칙:

- 테스트 폴더 구조는 구현 폴더 구조와 동일하게 맞춘다.
- 테스트 파일명은 구현 타입명 + `Tests` 규칙을 따른다.
- 구현 파일이 `Employees/ValueObjects`에 있으면 테스트도 `Employees/ValueObjects`에 둔다.

## 5. 도메인 규칙

### 5.1 이름

`EmployeeName` 규칙:

- `null` 불가
- `trim` 후 빈 문자열 불가
- 저장값은 `trim` 결과 사용

적용하지 않는 규칙:

- 대소문자 변경
- 내부 공백 축약
- 문자셋 제한

### 5.2 이메일

`EmployeeEmail` 규칙:

- `null` 불가
- `trim`
- 소문자 정규화
- 빈 문자열 불가
- `@`는 정확히 1개여야 한다
- 로컬 파트는 비어 있으면 안 된다
- 도메인 파트는 비어 있으면 안 된다
- 도메인 파트에는 `.`가 포함되어야 한다

저장값은 정규화 결과를 사용한다.

예:

- `" Alice@Example.Com "` -> `"alice@example.com"`

### 5.3 전화번호

`EmployeePhoneNumber` 규칙:

- `null` 불가
- `trim`
- 입력 문자는 숫자와 하이픈만 허용
- 하이픈 제거 후 숫자만 저장
- 정규화 결과는 `010`으로 시작해야 한다
- 정규화 결과 길이는 11이어야 한다

허용 입력 예:

- `010-1234-5678`
- `01012345678`

거부 입력 예:

- `01112345678`
- `010-123-5678`
- `0101234567`
- `010123456789`
- `010-12A4-5678`

### 5.4 직원 엔터티

`Employee` 규칙:

- `Id`는 호출자가 전달한다.
- `Name`, `Email`, `PhoneNumber`는 값 객체 타입으로 노출되며 각 값 객체를 통해 검증한다.
- `Joined`는 `DateOnly.MinValue`를 허용하지 않는다.
- `CreatedAt`, `UpdatedAt`은 `DateTimeOffset`으로 관리한다.
- `CreatedAt`, `UpdatedAt`은 `+00:00` 오프셋의 UTC 시간이어야 한다.
- 생성 시 `CreatedAt`과 `UpdatedAt`은 같은 값으로 시작한다.
- 이름 중복은 Domain에서 금지하지 않는다.

네이밍 기준:

- Domain, Application, Infrastructure 내부 모델에서는 `PhoneNumber`를 사용한다.
- API response body에서만 외부 계약에 맞춰 `tel` 필드명으로 표현한다.

## 6. 에러 코드 및 detail 기준

도메인 예외는 `code`와 `detail`을 함께 가진다.

- `invalid_name` / `Employee name is required.`
- `invalid_email` / `Employee email is invalid.`
- `invalid_tel` / `Employee phone number must be an 11-digit mobile number starting with 010.`
- `invalid_joined` / `Employee joined date is required.`

구현 원칙:

- 값 객체 내부에서 검증 실패 시 해당 규칙의 `Employee` 전용 예외를 던진다.
- `Employee.Create`는 하위 값 객체와 입사일 검증을 조합한다.
- `DomainException.Message`는 `detail`과 같은 값으로 유지한다.
- Application은 `code`와 `detail`을 외부 계약에 맞게 매핑한다.

## 7. 테스트 구현 기준

테스트는 Red -> Green -> Refactor 순서를 따른다.

구현 원칙:

1. 테스트 코드를 먼저 작성한다.
2. 실패 테스트가 실제로 실패하는지 확인한다.
3. 테스트를 통과시키는 최소 구현만 추가한다.
4. 테스트가 녹색인 상태에서만 리팩터링한다.

구현 순서는 아래로 확정한다.

1. 값 객체 테스트 작성
2. 실패 테스트 실행으로 Red 상태 확인
3. `DomainException` 구현
4. 값 객체 최소 구현
5. `Employee` 테스트 작성
6. 실패 테스트 실행으로 Red 상태 확인
7. `Employee.Create` 구현
8. 리팩터링

### 7.1 필수 테스트 케이스

작성 규칙:

- 테스트 메서드명은 영문으로 유지한다.
- 테스트 의도는 한글 설명으로 함께 기록한다.
- 실제 구현 시 xUnit `DisplayName`에 한글 설명을 넣는다.

#### EmployeeName

- `이름 생성 시 앞뒤 공백을 제거한다.`
  테스트: `Create_ShouldTrimName()` 
- `이름이 null이면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenNameIsNull()`
- `이름이 공백만 있으면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenNameIsWhitespace()`

#### EmployeeEmail

- `이메일 생성 시 앞뒤 공백을 제거하고 소문자로 정규화한다.`
  테스트: `Create_ShouldTrimAndLowercaseEmail()`
- `이메일이 null이면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenEmailIsNull()`
- `이메일이 공백만 있으면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenEmailIsWhitespace()`
- `이메일에 @ 기호가 없으면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenEmailDoesNotContainAtSymbol()`
- `이메일에 @ 기호가 여러 개면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenEmailContainsMultipleAtSymbols()`
- `이메일 도메인 파트가 비어 있으면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenEmailHasNoDomainPart()`
- `이메일 도메인에 점이 없으면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenEmailDomainHasNoDot()`

#### EmployeePhoneNumber

- `하이픈이 포함된 전화번호를 숫자만 남도록 정규화한다.`
  테스트: `Create_ShouldNormalizeHyphenatedPhoneNumber()`
- `유효한 전화번호 입력은 숫자만 저장한다.`
  테스트: `Create_ShouldKeepDigitsOnlyForValidInput()`
- `전화번호에 숫자와 하이픈 외 문자가 있으면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenPhoneContainsInvalidCharacters()`
- `전화번호가 010으로 시작하지 않으면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenPhoneDoesNotStartWith010()`
- `전화번호 길이가 11자리가 아니면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenPhoneLengthIsNot11()`

#### Employee

- `직원 생성 시 정규화된 값으로 엔터티를 만든다.`
  테스트: `Create_ShouldBuildEmployee_WithNormalizedValues()`
- `입사일이 기본값이면 예외를 던진다.`
  테스트: `Create_ShouldThrow_WhenJoinedIsDefault()`
- `이름 중복은 도메인 금지 규칙이 아니므로 허용한다.`
  테스트: `Create_ShouldAllowDuplicateNamesAsDomainConcern()`

## 8. 구현 순서

### Phase 1. 테스트 준비

- `tests/EmployeeContacts.Domain.Tests` 프로젝트 참조 상태 확인
- `FluentAssertions` 사용 여부 확인
- `Employees` 폴더 아래 테스트 파일 생성

### Phase 2. 값 객체 테스트 Red 확인

- 값 객체 테스트 작성
- 실패 테스트 실행
- 기대한 실패 원인인지 확인

### Phase 3. 값 객체 구현

- `DomainException` 추가
- `EmployeeName` 구현
- `EmployeeEmail` 구현
- `EmployeePhoneNumber` 구현
- 값 객체 테스트 녹색화

### Phase 4. 엔터티 테스트 Red 확인

- `Employee` 테스트 작성
- 실패 테스트 실행
- 기대한 실패 원인인지 확인

### Phase 5. 엔터티 구현

- `Employee` 추가
- `Create` 팩터리 구현
- 입사일 검증 추가
- 엔터티 테스트 녹색화

### Phase 6. 정리

- 중복 검증 코드 최소화
- 메시지 상수화 필요성 재검토
- 파일/타입 네이밍 최종 점검

## 9. 완료 기준

아래 조건을 모두 만족하면 Domain 구현을 완료로 본다.

- `EmployeeContacts.Domain`이 외부 패키지 의존성 없이 유지된다.
- `Employee.Create(...)`가 항상 정규화된 값만 노출한다.
- 잘못된 이름, 이메일, 전화번호, 입사일이 Domain에서 차단된다.
- 필수 Domain 테스트가 모두 녹색이다.
- Application이 별도 정규화 로직 없이 Domain 규칙을 재사용할 수 있다.

## 10. 다음 계층 전달 기준

Domain 완료 후 다음 계층은 아래 전제를 사용한다.

- Application은 파싱된 원시 입력으로 값 객체를 만든 뒤 `Employee.Create(...)`에 전달한다.
- 중복 검사와 요청 형식 검증은 Application에서 처리한다.
- Infrastructure는 이후 EF Core 매핑 단계에서 값 객체 내부 사용 구조에 맞춰 매핑 전략을 선택한다.
