# Application 최종 구현 문서

## 1. 문서 목적

이 문서는 `EmployeeContacts.Application` 구현의 최종 기준이다.

- Application 계층의 구현 범위와 비범위를 확정한다.
- 유스케이스, 공개 계약, 추상화, 테스트 기준을 결정 완료 상태로 고정한다.
- 구현 중 남는 선택지를 없애고, 테스트 우선 순서와 완료 기준을 명확히 한다.

이 문서의 기준이 기존 초안보다 우선한다.

## 2. 구현 대상과 비대상

### 구현 대상

- CQRS 요청/응답 모델
- `GetEmployeesQuery`
- `GetEmployeesByNameQuery`
- `BulkCreateEmployeesCommand`
- 각 요청의 Handler
- FluentValidation Validator
- Application 공통 DTO 및 결과 모델
- 저장소/중복검사/트랜잭션/파서/포맷 판별 추상화
- MediatR pipeline behavior
- `AddApplication()` DI 등록 진입점
- Application 단위 테스트

### 구현 대상 아님

- EF Core `DbContext` 및 Entity 매핑
- SQLite 인덱스/마이그레이션
- 실제 CSV/JSON 파싱 구현
- `text/plain` JSON 우선 판별의 구체 구현
- HTTP Content-Type 분기
- `ProblemDetails` 직렬화와 ASP.NET Core 예외 처리
- Controller 또는 Minimal API 엔드포인트
- OpenTelemetry exporter와 로깅 sink 설정

## 3. 최종 결정 사항

### 3.0 계층 책임

이번 Application 구현에서 책임은 아래로 고정한다.

결정:

- Application은 모든 유스케이스의 진입점이다.
- Application은 Domain 규칙을 재구현하지 않고 값 객체와 `Employee.Create(...)`를 사용한다.
- Application은 HTTP 세부사항을 알지 않는다.
- Application은 Infrastructure 구현 타입을 참조하지 않는다.
- Application은 파싱 결과를 공통 입력 모델로 받아 일괄 등록 유스케이스를 처리한다.

결정 이유:

- 요구사항의 조회/등록 흐름은 모두 유스케이스 중심으로 정리하는 편이 테스트하기 쉽다.
- Domain 정규화 규칙을 그대로 재사용해야 중복 구현과 규칙 불일치를 막을 수 있다.
- Content-Type, multipart, ProblemDetails는 API 책임으로 두어 계층 경계를 유지한다.

### 3.1 공개 진입점

Application의 DI 진입점은 아래 메서드로 고정한다.

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ...
    }
}
```

결정:

- 네임스페이스는 `EmployeeContacts.Application` 루트 또는 `DependencyInjection` 하위로 둔다.
- 이 메서드는 MediatR, Validators, Behaviors를 모두 등록한다.
- Infrastructure와 Api는 이 메서드만 통해 Application 서비스를 등록한다.

결정 이유:

- 조립 지점을 하나로 고정하면 이후 계층 연결이 단순해진다.
- 테스트에서도 등록 여부를 단일 진입점 기준으로 확인할 수 있다.

### 3.2 조회 유스케이스 계약

조회 유스케이스는 아래 두 요청으로 고정한다.

- `GetEmployeesQuery`
- `GetEmployeesByNameQuery`

`GetEmployeesQuery` 규칙:

- 입력은 `page`, `pageSize`
- 기본값은 `page = 1`, `pageSize = 20`
- `page`는 1 이상이어야 한다
- `pageSize`는 1 이상 100 이하여야 한다
- 결과는 `PagedResult<EmployeeDto>`를 반환한다
- 정렬은 `name ASC`, 그다음 `id ASC`를 보장한다
- 정렬 보장은 조회 저장소 계약의 일부로 본다

`GetEmployeesByNameQuery` 규칙:

- 입력은 `name`
- 입력 문자열은 Handler 진입 전에 `trim` 처리한다
- `trim` 결과가 빈 문자열이면 Validation 실패로 처리한다
- exact match 조회만 수행한다
- 결과는 `IReadOnlyList<EmployeeDto>`를 반환한다
- 결과 없음은 빈 컬렉션으로 처리한다
- 정렬은 `name ASC`, 그다음 `id ASC`를 보장한다
- 정렬 보장은 조회 저장소 계약의 일부로 본다

결정 이유:

- 조회 응답은 API 계약을 직접 반영한 DTO를 반환하는 편이 단순하다.
- 이름 검색은 exact match 요구사항이 명확하므로 contains 또는 normalization 확장을 이번 범위에 넣지 않는다.

### 3.3 일괄 등록 유스케이스 계약

등록 유스케이스는 `BulkCreateEmployeesCommand` 하나로 고정한다.

Command 입력 모델은 아래 구조로 고정한다.

```csharp
public sealed record BulkCreateEmployeesCommand(
    IReadOnlyList<BulkEmployeeRecord> Records) : IRequest<BulkCreateEmployeesResult>;
```

`BulkEmployeeRecord`는 파싱 완료 후의 공통 입력 모델로 사용한다.

```csharp
public sealed record BulkEmployeeRecord(
    int Row,
    string Name,
    string Email,
    string Tel,
    string Joined);
```

결정:

- `Row`는 실제 데이터 행 기준 1부터 시작한 값을 사용한다.
- CSV 헤더 유무와 무관하게 parser가 `Row`를 정규화해 전달한다.
- Application은 `BulkEmployeeRecord`를 기준으로 유효성 검사, Domain 생성, 중복 검사를 수행한다.
- Application은 원시 JSON/CSV 텍스트를 직접 파싱하지 않는다.
- 저장 가능한 각 행의 `Employee.Id`는 Handler가 `Guid.CreateVersion7()`로 생성한다.

결정 이유:

- 파싱과 유스케이스 처리를 분리해야 테스트 경계가 분명해진다.
- 행 번호를 공통 입력 모델에 포함하면 실패 응답 구성 규칙을 Handler에서 안정적으로 지킬 수 있다.

### 3.4 일괄 등록 결과 계약

일괄 등록 결과는 아래 모델로 고정한다.

```csharp
public sealed record BulkCreateEmployeesResult(
    int Total,
    int Created,
    int Failed,
    IReadOnlyList<BulkCreateEmployeesError> Errors);

public sealed record BulkCreateEmployeesError(
    int Row,
    string Field,
    string Code,
    string Message);
```

결정:

- `Total`은 입력 데이터 행 수다.
- `Created`는 실제 생성에 성공한 직원 수다.
- `Failed`는 오류 목록에 의해 생성되지 못한 행 수다.
- `Errors`는 입력 순서를 유지한다.
- 한 행에서 여러 오류를 누적하지 않고 첫 번째 결정적 오류 1건만 기록한다.
- `Field` 값은 `name`, `email`, `tel`, `joined` 중 하나를 사용한다.
- Handler는 생성 건수가 0이어도 예외 대신 `BulkCreateEmployeesResult`를 반환한다.
- HTTP 상태코드와 `ProblemDetails` 변환은 API 계층이 담당한다.
- 필드 오류 우선순위는 `name -> email -> tel -> joined`다.
- 중복 오류 우선순위는 `email -> tel`이다.

결정 이유:

- API 요구사항의 응답 형태를 그대로 반영해야 매핑이 단순해진다.
- 한 행당 오류 1건으로 고정하면 구현과 테스트 복잡도가 낮아진다.
- 중복 및 Domain 예외를 동일 오류 모델로 표현할 수 있다.

### 3.5 오류 코드 매핑 기준

Application은 아래 오류 코드 매핑을 사용한다.

- Domain 이름 예외 -> `field = "name"`, `code = "invalid_name"`
- Domain 이메일 예외 -> `field = "email"`, `code = "invalid_email"`
- Domain 전화번호 예외 -> `field = "tel"`, `code = "invalid_tel"`
- Domain 입사일 예외 -> `field = "joined"`, `code = "invalid_joined"`
- 기존 데이터 이메일 중복 -> `field = "email"`, `code = "duplicate_email"`
- 기존 데이터 전화번호 중복 -> `field = "tel"`, `code = "duplicate_tel"`
- 같은 요청 내부 이메일 중복 -> `field = "email"`, `code = "duplicate_email"`
- 같은 요청 내부 전화번호 중복 -> `field = "tel"`, `code = "duplicate_tel"`

메시지 기준:

- `invalid_name`, `invalid_email`, `invalid_tel`은 `DomainException.Detail`을 그대로 사용한다.
- `invalid_joined`의 메시지는 원인과 무관하게 `joined must be yyyy-MM-dd`로 고정한다.
- 중복 오류 메시지는 `email already exists`, `tel already exists`로 고정한다.

결정 이유:

- 요구사항에 정의된 오류 코드를 그대로 유지해야 API 응답 계약과 일치한다.
- Domain이 가진 `code/detail`를 그대로 활용하면 중복 매핑 로직을 줄일 수 있다.

### 3.6 중복 검사 알고리즘

중복 검사는 아래 순서로 고정한다.

1. 입력 행을 순서대로 순회한다.
2. 각 행에 대해 Domain 값 객체와 `Employee.Create(...)`로 정규화 가능한지 먼저 확인한다.
3. 정규화에 성공한 행만 대상으로 요청 내부 이메일/전화번호 중복을 검사한다.
4. 요청 내부 중복이 아닌 행만 저장소 기준 기존 이메일/전화번호 중복을 검사한다.
5. 저장 가능한 행만 `Employee` 목록으로 수집한다.
6. 하나라도 저장 가능한 행이 있으면 배치 저장 후 결과를 반환한다.
7. 저장 가능한 행이 하나도 없으면 실패 결과를 반환한다.

결정:

- 요청 내부 중복은 "먼저 등장한 행만 유효, 뒤에 나온 행을 실패" 정책으로 고정한다.
- 이메일과 전화번호는 정규화된 값 기준으로 비교한다.
- 저장소 중복 검사는 정규화된 이메일/전화번호 집합을 한 번에 조회하는 방식으로 가정한다.
- 요청 내부 중복과 저장소 중복이 동시에 가능하면 요청 내부 중복을 우선한다.
- 저장 가능한 행은 `Guid.CreateVersion7()`로 식별자를 만든 뒤 `Employee.Create(...)`에 전달한다.

결정 이유:

- 행 순서를 기준으로 승자를 고정해야 부분 성공 결과가 예측 가능해진다.
- 정규화 전 비교를 하면 하이픈/대소문자 차이로 잘못된 중복 판정이 생길 수 있다.

### 3.7 저장소 및 유스케이스 추상화

Application 추상화는 아래로 고정한다.

```csharp
public interface IEmployeeRepository
{
    Task<PagedResult<EmployeeDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmployeeDto>> GetByNameAsync(string exactName, CancellationToken cancellationToken);
    Task<IReadOnlySet<string>> GetExistingEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken);
    Task<IReadOnlySet<string>> GetExistingPhoneNumbersAsync(
        IReadOnlyCollection<string> phoneNumbers,
        CancellationToken cancellationToken);
    Task AddRangeAsync(IReadOnlyCollection<Employee> employees, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
```

결정:

- 조회와 쓰기 저장소는 초기 구현에서 하나의 `IEmployeeRepository`로 유지한다.
- 조회 결과는 Domain Entity가 아니라 Application DTO로 직접 반환한다.
- 쓰기 저장은 Domain `Employee` 컬렉션을 받는다.
- 커밋은 `IUnitOfWork`로 분리한다.
- 조회 정렬 보장은 저장소 구현 세부가 아니라 `IEmployeeRepository` 계약의 일부로 본다.

결정 이유:

- 현재 기능 범위에서는 저장소를 과도하게 쪼갤 필요가 없다.
- 조회에서 DTO를 바로 반환하면 쿼리 Handler가 매핑 책임을 중복으로 가지지 않는다.
- 커밋을 분리해야 테스트에서 저장 호출과 커밋 호출을 각각 검증할 수 있다.

### 3.8 파서 및 포맷 판별 추상화

파서 구현은 Infrastructure 책임이지만 계약은 Application에 둔다.

```csharp
public interface IEmployeeImportParser
{
    Task<IReadOnlyList<BulkEmployeeRecord>> ParseAsync(string content, CancellationToken cancellationToken);
}

public interface IPlainTextEmployeeImportDetector
{
    IEmployeeImportParser Resolve(string content);
}
```

결정:

- `application/json`, `text/csv`, `text/plain` 처리에 필요한 parser 계약은 Application에 둔다.
- `text/plain`의 JSON 우선 판별은 `IPlainTextEmployeeImportDetector`로 캡슐화한다.
- `multipart/form-data`의 파일 읽기 자체는 API가 담당하고, 파일 내용을 문자열로 읽은 뒤 적절한 parser에 전달한다.
- `BulkCreateEmployeesCommand`는 parser를 직접 호출하지 않는다.

결정 이유:

- 실제 파싱 구현은 외부 입출력 성격이 강하므로 Infrastructure에 두는 편이 맞다.
- 다만 어떤 형식이 어떤 공통 레코드로 들어와야 하는지는 Application 계약으로 고정해야 한다.

### 3.9 공통 DTO와 페이지 모델

공개 응답 DTO는 아래 구조로 고정한다.

```csharp
public sealed record EmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string Tel,
    DateOnly Joined);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
```

결정:

- 외부 계약 필드명에 맞춰 DTO 속성은 `Tel`을 사용한다.
- `PagedResult<T>`는 재사용 가능한 공통 모델로 둔다.
- `TotalPages`는 `TotalCount == 0`일 때 0으로 계산한다.

결정 이유:

- API 응답과 동일한 이름을 가진 DTO를 사용하면 Presentation 계층 매핑을 최소화할 수 있다.
- 페이지 메타데이터는 다른 조회 기능이 생겨도 같은 모델을 재사용할 수 있다.

### 3.10 Validation 전략

Validation은 아래 원칙으로 고정한다.

결정:

- 모든 `IRequest<TResponse>`는 FluentValidation 대상으로 본다.
- Query/Command Validator는 Application 입력 규칙만 검증한다.
- Domain 규칙 검증은 Handler 내부에서 값 객체와 `Employee.Create(...)` 호출로 처리한다.
- `GetEmployeesQueryValidator`
  - `page >= 1`
  - `pageSize >= 1`
  - `pageSize <= 100`
- `GetEmployeesByNameQueryValidator`
  - `name`는 `trim` 후 비어 있지 않아야 한다
- `BulkCreateEmployeesCommandValidator`
  - `Records`는 `null`일 수 없다
  - `Records`는 1건 이상이어야 한다
  - 각 행의 `Row`는 1 이상이어야 한다

결정 이유:

- 쿼리 매개변수 검증은 Handler 전에 실패시키는 편이 맞다.
- 등록 행의 세부 필드 검증은 Domain 예외와 중복 검사 흐름에 포함시키는 편이 요구사항과 더 잘 맞는다.

### 3.11 MediatR Behavior 구성

Behavior는 아래 순서로 고정한다.

1. `LoggingBehavior<TRequest, TResponse>`
2. `ValidationBehavior<TRequest, TResponse>`
3. `TracingBehavior<TRequest, TResponse>`

결정:

- `LoggingBehavior`는 요청명과 처리 시간을 남길 수 있는 최소 구조만 가진다.
- `ValidationBehavior`는 등록된 Validator를 모두 실행하고 실패 시 `ValidationException`을 던진다.
- `TracingBehavior`는 `ActivitySource`를 받아 요청 단위 span을 생성한다.
- 실제 로그 출력 형식과 exporter 연결은 Application 범위 밖이다.

결정 이유:

- 문서/아키텍처에서 권장한 순서를 유지해 이후 계층과의 연결을 단순화한다.
- 관측성의 실제 구현보다 계약과 호출 지점을 먼저 고정해야 테스트가 가능하다.

### 3.12 폴더 구조

최종 폴더 구조는 아래로 확정한다.

```text
src/EmployeeContacts.Application
├─ Abstractions
│  ├─ Persistence
│  │  ├─ IEmployeeRepository.cs
│  │  └─ IUnitOfWork.cs
│  └─ Parsing
│     ├─ IEmployeeImportParser.cs
│     └─ IPlainTextEmployeeImportDetector.cs
├─ Behaviors
│  ├─ LoggingBehavior.cs
│  ├─ ValidationBehavior.cs
│  └─ TracingBehavior.cs
├─ Common
│  └─ Models
│     └─ PagedResult.cs
├─ DependencyInjection
│  └─ DependencyInjection.cs
└─ Employees
   ├─ Commands
   │  └─ BulkCreateEmployees
   │     ├─ BulkCreateEmployeesCommand.cs
   │     ├─ BulkCreateEmployeesCommandHandler.cs
   │     ├─ BulkCreateEmployeesCommandValidator.cs
   │     ├─ BulkCreateEmployeesError.cs
   │     ├─ BulkCreateEmployeesResult.cs
   │     └─ BulkEmployeeRecord.cs
   ├─ Dtos
   │  └─ EmployeeDto.cs
   └─ Queries
      ├─ GetEmployees
      │  ├─ GetEmployeesQuery.cs
      │  ├─ GetEmployeesQueryHandler.cs
      │  └─ GetEmployeesQueryValidator.cs
      └─ GetEmployeesByName
         ├─ GetEmployeesByNameQuery.cs
         ├─ GetEmployeesByNameQueryHandler.cs
         └─ GetEmployeesByNameQueryValidator.cs
```

테스트 구조는 아래로 확정한다.

```text
tests/EmployeeContacts.Application.Tests
├─ Behaviors
│  ├─ ValidationBehaviorTests.cs
│  └─ DependencyInjectionTests.cs
└─ Employees
   ├─ Commands
   │  └─ BulkCreateEmployees
   │     ├─ BulkCreateEmployeesCommandHandlerTests.cs
   │     └─ BulkCreateEmployeesCommandValidatorTests.cs
   └─ Queries
      ├─ GetEmployees
      │  ├─ GetEmployeesQueryHandlerTests.cs
      │  └─ GetEmployeesQueryValidatorTests.cs
      └─ GetEmployeesByName
         ├─ GetEmployeesByNameQueryHandlerTests.cs
         └─ GetEmployeesByNameQueryValidatorTests.cs
```

구조 원칙:

- 테스트 폴더 구조는 구현 폴더 구조를 그대로 따른다.
- 요청, Handler, Validator는 같은 기능 폴더에 함께 둔다.
- 공통 모델은 `Common`으로 올리고 기능별 DTO는 `Employees/Dtos` 아래에 둔다.

## 4. 테스트 구현 기준

테스트는 Red -> Green -> Refactor 순서를 따른다.

구현 원칙:

1. Query와 Command의 실패 테스트를 먼저 작성한다.
2. Validator 실패가 기대한 방식인지 먼저 확인한다.
3. Handler의 저장소 호출과 결과 매핑을 최소 구현으로 통과시킨다.
4. Behavior와 DI 테스트로 공통 구성을 고정한다.

### 4.1 필수 조회 테스트

#### GetEmployees

- `기본 page, pageSize 값으로 조회한다.`
  테스트: `Handle_ShouldReturnPagedEmployees_WithRequestedPaging()`
- `page가 1보다 작으면 검증 실패한다.`
  테스트: `Validate_ShouldFail_WhenPageIsLessThanOne()`
- `pageSize가 100을 초과하면 검증 실패한다.`
  테스트: `Validate_ShouldFail_WhenPageSizeExceedsMaximum()`

#### GetEmployeesByName

- `이름 검색 전 trim 처리한다.`
  테스트: `Handle_ShouldTrimNameBeforeSearching()`
- `trim 결과가 비면 검증 실패한다.`
  테스트: `Validate_ShouldFail_WhenNameIsEmptyAfterTrim()`
- `검색 결과가 없으면 빈 컬렉션을 반환한다.`
  테스트: `Handle_ShouldReturnEmptyList_WhenNoEmployeeExists()`

### 4.2 필수 등록 테스트

#### BulkCreateEmployeesCommand

- `정상 행은 Employee를 생성하고 저장한다.`
  테스트: `Handle_ShouldCreateEmployees_ForValidRows()`
- `요청 내부 이메일 중복은 뒤에 나온 행을 실패 처리한다.`
  테스트: `Handle_ShouldFailDuplicateEmailWithinRequest()`
- `요청 내부 전화번호 중복은 뒤에 나온 행을 실패 처리한다.`
  테스트: `Handle_ShouldFailDuplicateTelWithinRequest()`
- `기존 이메일 중복은 실패 처리한다.`
  테스트: `Handle_ShouldFail_WhenEmailAlreadyExists()`
- `기존 전화번호 중복은 실패 처리한다.`
  테스트: `Handle_ShouldFail_WhenTelAlreadyExists()`
- `Domain 예외를 오류 항목으로 변환한다.`
  테스트: `Handle_ShouldMapDomainException_ToBulkError()`
- `일부 행만 성공해도 결과 집계를 올바르게 반환한다.`
  테스트: `Handle_ShouldReturnPartialSuccessResult()`
- `한 건도 생성되지 않으면 실패 결과를 반환한다.`
  테스트: `Handle_ShouldReturnFailureResult_WhenNothingIsCreated()`
- `행 순서대로 오류를 유지한다.`
  테스트: `Handle_ShouldPreserveErrorOrderByRow()`

조회 정렬 보장 검증은 저장소 또는 통합 테스트 범위로 둔다.

### 4.3 필수 Behavior 및 DI 테스트

- `ValidationBehavior는 Handler 실행 전에 검증 예외를 던진다.`
  테스트: `Handle_ShouldThrowValidationException_WhenValidationFails()`
- `AddApplication은 MediatR, Validators, Behaviors를 등록한다.`
  테스트: `AddApplication_ShouldRegisterApplicationServices()`

## 5. 구현 순서

구현 순서는 아래로 확정한다.

### Phase 1. 프로젝트 준비

- `EmployeeContacts.Application`에 `Domain` 프로젝트 참조 추가
- MediatR, FluentValidation 패키지 추가
- `EmployeeContacts.Application.Tests`에 `Application`, `Domain` 프로젝트 참조 추가
- 테스트용 mocking 라이브러리와 `FluentAssertions` 추가

### Phase 2. 조회 유스케이스

- `PagedResult<T>`와 `EmployeeDto` 추가
- `GetEmployeesQuery`와 Validator 테스트 작성
- `GetEmployeesQueryHandler` 구현
- `GetEmployeesByNameQuery`와 Validator 테스트 작성
- `GetEmployeesByNameQueryHandler` 구현

### Phase 3. 등록 유스케이스

- `BulkEmployeeRecord`, 결과 모델 추가
- `BulkCreateEmployeesCommandValidator` 테스트 작성
- `BulkCreateEmployeesCommandHandler` 테스트 작성
- Domain 변환, 요청 내부 중복 검사, 저장소 중복 검사, 저장 흐름 구현

### Phase 4. 공통 구성

- 저장소/파서/트랜잭션 추상화 추가
- `ValidationBehavior`, `LoggingBehavior`, `TracingBehavior` 구현
- `AddApplication()` 구현

### Phase 5. 정리

- 테스트 이름과 폴더 구조 최종 점검
- 공통 중복 제거
- 문서와 실제 공개 타입 이름이 일치하는지 확인

## 6. 완료 기준

아래 조건을 모두 만족하면 Application 구현을 완료로 본다.

- `EmployeeContacts.Application`은 `Infrastructure`를 참조하지 않는다.
- 조회/등록 유스케이스가 MediatR 요청과 Handler로 모두 구현된다.
- 페이징, 이름 검색, 부분 성공 등록 규칙이 자동화 테스트로 보호된다.
- Domain 예외가 등록 오류 계약으로 일관되게 변환된다.
- 중복 이메일/전화번호 처리가 요청 내부와 저장소 기준 모두 검증된다.
- `AddApplication()`만으로 Application 서비스 구성이 가능하다.

## 7. 다음 계층 전달 기준

Application 완료 후 다음 계층은 아래 전제를 사용한다.

- Infrastructure는 `IEmployeeRepository`, `IUnitOfWork`, parser 추상화의 구현을 제공한다.
- API는 Content-Type별로 적절한 parser를 호출해 `BulkEmployeeRecord` 목록을 만든 뒤 Command를 보낸다.
- API는 `BulkCreateEmployeesResult.Created > 0`이면 `201 Created`와 결과 본문으로 매핑한다.
- API는 `BulkCreateEmployeesResult.Created == 0`이면 `400 Bad Request`와 `ProblemDetails`로 변환한다.
- API는 FluentValidation 예외와 기타 예외를 `ProblemDetails` 또는 `ValidationProblemDetails`로 변환한다.
