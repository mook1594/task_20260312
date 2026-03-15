# Infrastructure 최종 구현 문서

## 1. 문서 목적

이 문서는 `EmployeeContacts.Infrastructure` 구현의 최종 기준이다.

- Infrastructure 계층의 구현 범위와 비범위를 확정한다.
- 영속화, 파싱, DI 조립, 마이그레이션 전략의 선택지를 제거한다.
- 테스트 우선 순서와 완료 기준을 명확히 한다.

이 문서의 기준이 기존 초안보다 우선한다.

## 2. 구현 대상과 비대상

### 구현 대상

- SQLite 기반 `AppDbContext`
- 직원 테이블 persistence model 및 EF Core 매핑
- `IEmployeeRepository` 구현
- `IUnitOfWork` 구현
- 초기 마이그레이션
- `tests/EmployeeContacts.Infrastructure.Tests` 프로젝트 신설 및 계약 테스트
- CSV parser 구현
- JSON parser 구현
- `text/plain` 포맷 판별 구현
- `AddInfrastructure()` DI 등록 진입점

### 구현 대상 아님

- Controller 또는 Minimal API 엔드포인트
- HTTP `Content-Type` 분기
- `ProblemDetails` 응답 생성
- Swagger/OpenAPI 문서화
- OpenTelemetry exporter와 logging sink의 실제 외부 연동

## 3. 최종 결정 사항

### 3.0 계층 책임

이번 Infrastructure 구현의 책임은 아래로 고정한다.

결정:

- Infrastructure는 Application이 정의한 저장소, 트랜잭션, parser 추상화의 실제 구현을 제공한다.
- Infrastructure는 외부 저장 형식과 DB 세부사항을 캡슐화하고, Application 공개 계약을 변경하지 않는다.
- Infrastructure는 API의 요청 형식 분기와 HTTP 상태코드 결정을 알지 않는다.
- Infrastructure는 Domain 규칙을 재구현하지 않고, 필요한 경우 Domain 타입을 그대로 사용하거나 값만 추출한다.

결정 이유:

- 파싱/영속화 세부 구현을 이 계층에 격리해야 Application과 API 테스트 경계가 흔들리지 않는다.
- HTTP 관심사와 DB 관심사를 분리해야 이후 API 변경 시 저장소/파서 구현이 영향받지 않는다.

### 3.1 DI 진입점

Infrastructure의 DI 진입점은 아래 메서드로 고정한다.

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ...
    }
}
```

결정:

- 연결 문자열 키는 `ConnectionStrings:EmployeeContacts`로 고정한다.
- 개발 기본 SQLite 연결 문자열은 API `appsettings.json`에 `Data Source=employee-contacts.db`로 추가한다.
- `ConnectionStrings:EmployeeContacts`가 없거나 비어 있으면 `AddInfrastructure()`에서 즉시 실패한다.
- `AddInfrastructure()`는 `DbContext`, repository, unit of work, parser, detector를 모두 등록한다.
- `AppDbContext`, repository, unit of work는 `Scoped`로 등록한다.
- parser와 detector는 상태가 없으므로 `Singleton`으로 등록한다.
- 마이그레이션 assembly는 `EmployeeContacts.Infrastructure`로 고정한다.

결정 이유:

- API 호스트는 설정 소스의 주도권을 가지므로 `IConfiguration`을 직접 받는 편이 단순하다.
- 연결 문자열 이름을 고정하면 마이그레이션, 테스트 호스트, 로컬 실행 구성이 일관된다.

### 3.2 영속화 모델 전략

EF Core는 Domain `Employee`를 직접 추적하지 않고 Infrastructure 전용 persistence model을 사용한다.

결정:

- `AppDbContext`는 `DbSet<EmployeeEntity>`를 가진다.
- `EmployeeEntity`는 아래 scalar 속성으로 구성한다.
  - `Guid Id`
  - `string Name`
  - `string Email`
  - `string PhoneNumber`
  - `DateOnly Joined`
  - `DateTimeOffset CreatedAt`
  - `DateTimeOffset UpdatedAt`
- `IEmployeeRepository.AddRangeAsync(...)`는 Domain `Employee`를 `EmployeeEntity`로 변환하여 추가한다.
- 조회는 `EmployeeEntity`에서 `EmployeeDto`로 직접 projection 한다.

결정 이유:

- 현재 Application 계약은 조회에서 DTO를 반환하고, 쓰기에서만 Domain `Employee`를 받는다.
- 별도 persistence model을 두면 EF Core 생성자 바인딩과 값 객체 변환 복잡도를 줄일 수 있다.
- 중복 조회와 정렬 SQL을 scalar 컬럼 기준으로 단순하게 작성할 수 있다.

### 3.3 `AppDbContext`와 테이블 설계

`AppDbContext`는 단일 aggregate 저장소로 시작한다.

결정:

- 테이블명은 `Employees`로 고정한다.
- `DbContext` 클래스명은 `AppDbContext`로 고정한다.
- `OnModelCreating()`에서는 `EmployeeConfiguration`만 적용한다.
- 이번 범위에서 `DbContext`는 `Employees` 집합 외 다른 `DbSet`을 추가하지 않는다.

결정 이유:

- 현재 기능 범위는 직원 연락망 단일 aggregate에 집중되어 있다.
- 초기 모델을 단순하게 유지해야 마이그레이션과 테스트 픽스처 구성이 쉬워진다.

### 3.4 `EmployeeConfiguration` 매핑 기준

직원 테이블 매핑은 아래 규칙으로 고정한다.

결정:

- 기본 키는 `Id`
- `Name`은 필수
- `Email`은 필수 + 유니크 인덱스
- `PhoneNumber`는 필수 + 유니크 인덱스
- `PhoneNumber`는 `MaxLength(15)`를 설정한다
- `PhoneNumber`는 컬럼명 `PhoneNumber`를 유지한다
- `Name` 조회 인덱스를 추가한다
- `Joined`, `CreatedAt`, `UpdatedAt`은 모두 필수다
- `Joined`, `CreatedAt`, `UpdatedAt` 저장 형식은 `ValueConverter`로 명시 고정한다
- 정렬 보장은 repository 쿼리에서 수행하고, DB 기본 정렬에 의존하지 않는다

컬럼 규칙:

- `Name`: `TEXT NOT NULL`
- `Email`: `TEXT NOT NULL`
- `PhoneNumber`: `TEXT NOT NULL`
- `Joined`: SQLite `TEXT`로 저장하며 값은 `yyyy-MM-dd`
- `CreatedAt`, `UpdatedAt`: SQLite `TEXT`로 저장하며 UTC round-trip 형식으로 유지한다

결정 이유:

- SQLite는 강한 타입 제약보다 텍스트 저장이 예측 가능하다.
- `Joined`를 ISO 문자열로 저장하면 계약 형식과 DB 저장 형식이 일치해 디버깅이 단순하다.
- 유니크 인덱스는 Application 중복 검사 외의 최종 안전장치 역할을 한다.

### 3.5 Repository 구현 기준

`EmployeeRepository`는 `IEmployeeRepository`를 그대로 구현한다.

결정:

- `GetPagedAsync(page, pageSize)`는 `OrderBy(Name).ThenBy(Id)` 후 `Skip/Take`를 적용한다.
- `GetByNameAsync(exactName)`는 exact match로 `Name == exactName` 조회 후 `OrderBy(Name).ThenBy(Id)`를 적용한다.
- 조회 쿼리는 모두 `AsNoTracking()`을 사용한다.
- `GetExistingEmailsAsync(...)`와 `GetExistingPhoneNumbersAsync(...)`는 전달받은 정규화 값 기준으로 `HashSet<string>`을 반환한다.
- 입력 컬렉션이 비어 있으면 DB를 조회하지 않고 빈 집합을 반환한다.
- `AddRangeAsync(...)`는 `SaveChanges`를 호출하지 않는다.

결정 이유:

- 커밋 시점은 `IUnitOfWork`에 남겨야 Application의 저장/커밋 분리 계약이 유지된다.
- 읽기 전용 쿼리에 `AsNoTracking()`을 기본 적용해야 불필요한 change tracker 비용을 줄일 수 있다.

### 3.6 Unit of Work 기준

트랜잭션 커밋은 EF Core `DbContext` 기반 `UnitOfWork`로 구현한다.

결정:

- 구현 타입명은 `EfUnitOfWork`로 고정한다.
- `EfUnitOfWork.SaveChangesAsync(...)`는 내부적으로 `AppDbContext.SaveChangesAsync(...)`를 호출한다.
- `DbUpdateException` 중 DB 유니크 인덱스 충돌은 가능한 경우 충돌 필드(`email` 또는 `tel`) 수준까지 식별한다.
- 저장 시점 유니크 충돌은 `BulkCreateEmployeesResult.Errors`의 행 단위 오류로 재구성하지 않는다.
- 저장 시점 유니크 충돌은 요청 수준 오류로 상위 계층에 전달하고, API는 이를 `ProblemDetails`로 변환한다.

결정 이유:

- 요청 내부/기존 데이터 중복은 Application이 사전 차단하므로 정상 경로에서는 row-level 오류가 이미 구성된다.
- 동시성 경합으로 인한 DB unique 충돌은 현재 계약만으로 특정 row에 재매핑할 수 없다.
- 대신 필드 수준까지는 식별하되, 근거 없는 row 추정은 하지 않는 편이 현재 응답 계약과 더 잘 맞는다.

### 3.7 SQLite 및 마이그레이션 전략

SQLite와 마이그레이션은 아래 기준으로 고정한다.

결정:

- DB provider는 `Microsoft.EntityFrameworkCore.Sqlite`
- 마이그레이션은 `src/EmployeeContacts.Infrastructure/Migrations` 아래에 둔다
- 초기 마이그레이션 이름은 `InitialCreate`
- API 시작 시 자동 마이그레이션 적용은 이번 문서 범위에 포함하지 않는다
- 실제 API는 자동 마이그레이션을 수행하지 않는다.
- 테스트는 빈 데이터베이스에 자동으로 마이그레이션을 적용해 최신 스키마를 맞춘다.

결정 이유:

- 마이그레이션 자동 적용 여부는 운영 정책과 연결되므로 API 부트스트랩 단계에서 별도 결정하는 편이 맞다.
- Infrastructure는 마이그레이션 생성과 적용 가능한 상태까지만 제공하면 충분하다.

### 3.8 CSV parser 기준

CSV parser 구현 타입은 `CsvEmployeeImportParser`로 고정한다.

결정:

- 구현 타입은 `ICsvEmployeeImportParser`와 `IEmployeeImportParser`를 구현한다.
- 입력은 `string content`
- 빈 문자열 또는 공백만 있는 입력은 `ApplicationError InvalidFormat` 기반 `ApplicationException`으로 실패한다
- 줄 구분은 `\r\n`, `\n` 모두 허용한다
- UTF-8 BOM이 있는 입력을 허용한다
- 파일 마지막의 빈 줄은 무시한다
- 중간에 나타나는 빈 줄은 형식 오류로 처리한다
- 첫 줄이 4개 컬럼이고 각 셀을 `trim`한 뒤 `name,email,tel,joined`와 순서까지 일치하면 헤더로 간주한다
- 헤더 비교는 대소문자만 무시한다
- 헤더가 있으면 결과 row 번호는 첫 데이터 줄부터 1로 시작한다
- 각 데이터 줄은 정확히 4개 컬럼이어야 한다
- 큰따옴표 인용은 지원하지 않으므로 `"`가 포함된 줄은 형식 오류로 처리한다
- 파싱 결과 데이터 행 수가 0이면 형식 오류로 처리한다
- 각 데이터 줄은 `BulkEmployeeRecord(Row, Name, Email, Tel, Joined)`로 변환한다
- 필드 `trim`은 parser가 하지 않고 원문을 유지한다
- `invalid_format`의 `detail`은 공통 메시지를 기본으로 하되, 대표 구조 오류에는 더 구체적인 메시지를 허용한다

결정 이유:

- 정규화와 필드 규칙 검증은 Domain/Application 책임이므로 parser는 구조만 맞춘다.
- 헤더 인식과 row 번호 계산을 parser에서 고정해야 API 응답의 `errors.row` 계약을 안정적으로 만족할 수 있다.

### 3.9 JSON parser 기준

JSON parser 구현 타입은 `JsonEmployeeImportParser`로 고정한다.

결정:

- 구현 타입은 `IJsonEmployeeImportParser`와 `IEmployeeImportParser`를 구현한다.
- `System.Text.Json`을 사용한다
- 루트는 반드시 JSON 배열이어야 한다
- 각 원소는 JSON 객체여야 한다
- 각 객체는 `name`, `email`, `tel`, `joined` 문자열 속성을 가져야 한다
- 속성명 비교는 대소문자를 구분하지 않는다
- 추가 속성은 무시한다
- `null`, 숫자, 배열, 객체 값은 모두 형식 오류로 처리한다
- 배열 순서대로 `Row = 1..N`을 부여한다
- JSON 자체가 깨졌거나 루트가 배열이 아니거나 필수 속성이 누락되면 `invalid_format`으로 실패한다
- 파싱 결과 요소 수가 0이면 형식 오류로 처리한다
- 날짜 문자열은 parser에서 `DateOnly`로 변환하지 않는다
- `invalid_format`의 `detail`은 공통 메시지를 기본으로 하되, 대표 구조 오류에는 더 구체적인 메시지를 허용한다

결정 이유:

- 등록 유스케이스의 필드 검증은 Application에 남겨야 CSV/JSON 경로가 같은 규칙을 공유한다.
- JSON parser는 구조 유효성만 보장하고, 비즈니스 검증은 내려보내지 않는다.

### 3.10 `text/plain` 판별 기준

`IPlainTextEmployeeImportDetector` 구현 타입은 `PlainTextEmployeeImportDetector`로 고정한다.

결정:

- detector는 내부적으로 먼저 "유효한 JSON 배열인지" 판별한다
- `JsonDocument.Parse(...)`가 성공하고 루트가 배열이면 JSON parser를 반환한다
- 그 외 모든 경우는 CSV parser를 반환한다
- 반환형은 `IEmployeeImportParser`로 유지한다
- detector 자체는 예외를 던지지 않고 parser 선택만 수행한다

결정 이유:

- 현재 Application 계약은 parser 선택 인터페이스만 제공하므로, JSON 우선 시도는 detector 내부 판별로 캡슐화하는 편이 맞다.
- 이 방식이면 `text/plain` 입력에서 JSON 배열이면 JSON 경로를, 그렇지 않으면 CSV 경로를 일관되게 선택할 수 있다.

### 3.11 예외와 오류 코드 기준

Infrastructure parser는 요청 수준 포맷 오류를 `ApplicationException`으로 표현한다.

결정:

- parser와 detector는 `EmployeeContacts.Application.Common.Errors.ApplicationException`을 사용할 수 있다
- parser는 `ApplicationError InvalidFormat` 상수를 통해 `ApplicationException`을 생성한다
- 지원하는 parser 오류 코드는 초기 구현에서 `invalid_format` 하나로 고정한다
- `invalid_format`의 기본 `detail`은 `Request body format is invalid.`로 둔다
- 대표 구조 오류에는 더 구체적인 `detail`을 허용한다
- `invalid_content_type`는 API 계층 책임이므로 Infrastructure에서 만들지 않는다
- DB 접근 중 예기치 않은 오류는 래핑하지 않고 상위로 전파한다

결정 이유:

- 포맷 오류는 비즈니스 행 단위 오류가 아니라 요청 자체 오류이므로 Application 오류 계약을 재사용하는 편이 단순하다.
- `Content-Type` 자체는 HTTP 메타데이터이므로 parser가 알면 계층 경계가 흐려진다.

### 3.12 최종 폴더 구조

최종 폴더 구조는 아래로 확정한다.

```text
src/EmployeeContacts.Infrastructure
├─ DependencyInjection
│  └─ DependencyInjection.cs
├─ Parsing
│  ├─ Csv
│  │  └─ CsvEmployeeImportParser.cs
│  ├─ Json
│  │  └─ JsonEmployeeImportParser.cs
│  └─ Text
│     └─ PlainTextEmployeeImportDetector.cs
├─ Persistence
│  ├─ AppDbContext.cs
│  ├─ Configurations
│  │  └─ EmployeeConfiguration.cs
│  ├─ Entities
│  │  └─ EmployeeEntity.cs
│  ├─ Repositories
│  │  └─ EmployeeRepository.cs
│  └─ UnitOfWork
│     └─ EfUnitOfWork.cs
└─ Migrations
```

테스트 검증 구조는 아래 원칙으로 고정한다.

```text
tests/EmployeeContacts.Infrastructure.Tests
tests/EmployeeContacts.Api.IntegrationTests
```

구조 원칙:

- Infrastructure 전용 테스트 프로젝트로 parser, repository, migration, DI 계약을 직접 검증한다.
- API 통합 테스트는 end-to-end 계약과 호스트 조립 검증에 집중한다.
- 저장소 SQL/SQLite 동작 확인은 `EmployeeContacts.Infrastructure.Tests`에서 SQLite 실제 provider를 사용해 보호한다.

## 4. 테스트 구현 기준

테스트는 Red -> Green -> Refactor 순서를 따른다.

구현 원칙:

1. parser와 persistence 계약을 보호하는 실패 테스트를 먼저 작성한다.
2. SQLite 실제 provider 기반 통합 검증을 우선한다.
3. repository와 parser는 최소 구현으로 녹색화한다.
4. 마지막에 DI와 마이그레이션 연결 상태를 확인한다.

### 4.1 필수 parser 테스트

- `CSV 헤더가 있으면 첫 데이터 행부터 row 1을 부여한다.`
  테스트: `ParseAsync_ShouldAssignRowNumbers_FromFirstDataRow_WhenHeaderExists()`
- `CSV 헤더가 없으면 첫 줄을 row 1로 처리한다.`
  테스트: `ParseAsync_ShouldAssignRowNumbers_FromFirstLine_WhenHeaderDoesNotExist()`
- `CSV 헤더 순서가 다르면 형식 오류로 실패한다.`
  테스트: `ParseAsync_ShouldThrow_WhenCsvHeaderOrderIsInvalid()`
- `CSV 컬럼 수가 4개가 아니면 형식 오류로 실패한다.`
  테스트: `ParseAsync_ShouldThrow_WhenCsvColumnCountIsInvalid()`
- `CSV 큰따옴표 인용은 지원하지 않는다.`
  테스트: `ParseAsync_ShouldThrow_WhenCsvContainsQuotedField()`
- `JSON 배열을 BulkEmployeeRecord 목록으로 변환한다.`
  테스트: `ParseAsync_ShouldParseJsonArray_ToBulkEmployeeRecords()`
- `JSON 루트가 배열이 아니면 형식 오류로 실패한다.`
  테스트: `ParseAsync_ShouldThrow_WhenJsonRootIsNotArray()`
- `JSON 필수 속성이 누락되면 형식 오류로 실패한다.`
  테스트: `ParseAsync_ShouldThrow_WhenJsonPropertyIsMissing()`
- `text/plain` 입력이 유효한 JSON 배열이면 JSON parser를 선택한다.
  테스트: `Resolve_ShouldReturnJsonParser_WhenContentIsValidJsonArray()`
- `text/plain` 입력이 JSON 배열이 아니면 CSV parser를 선택한다.
  테스트: `Resolve_ShouldReturnCsvParser_WhenContentIsNotJsonArray()`

### 4.2 필수 repository 및 persistence 테스트

- `GetPagedAsync는 Name, Id 오름차순 정렬을 보장한다.`
  테스트: `GetPagedAsync_ShouldReturnEmployees_OrderedByNameThenId()`
- `GetByNameAsync는 exact match로 조회한다.`
  테스트: `GetByNameAsync_ShouldReturnEmployees_WithExactNameMatch()`
- `GetExistingEmailsAsync는 전달한 값과 일치하는 이메일만 반환한다.`
  테스트: `GetExistingEmailsAsync_ShouldReturnMatchingEmailsOnly()`
- `GetExistingPhoneNumbersAsync는 전달한 값과 일치하는 전화번호만 반환한다.`
  테스트: `GetExistingPhoneNumbersAsync_ShouldReturnMatchingPhoneNumbersOnly()`
- `AddRangeAsync와 SaveChangesAsync 호출 후 직원이 실제 저장된다.`
  테스트: `SaveChangesAsync_ShouldPersistEmployees_AddedByRepository()`
- `Email, PhoneNumber 유니크 인덱스가 생성된다.`
  테스트: `DatabaseSchema_ShouldCreateUniqueIndexes_ForEmailAndPhoneNumber()`

### 4.3 필수 DI 및 마이그레이션 테스트

- `AddInfrastructure는 repository, unit of work, parser, detector, DbContext를 등록한다.`
  테스트: `AddInfrastructure_ShouldRegisterInfrastructureServices()`
- `초기 마이그레이션이 빈 데이터베이스에 적용 가능하다.`
  테스트: `DatabaseMigration_ShouldApplyInitialCreateMigration()`

## 5. 구현 순서

구현 순서는 아래로 확정한다.

### Phase 1. 프로젝트 준비

- `EmployeeContacts.Infrastructure`에 `Application`, `Domain` 프로젝트 참조 추가
- `tests/EmployeeContacts.Infrastructure.Tests` 프로젝트를 추가하고 `Infrastructure`, `Application`, `Domain` 프로젝트를 참조한다
- EF Core, SQLite, Design 패키지 추가
- DI 진입점과 연결 문자열 키를 확정한다

### Phase 2. Persistence 골격

- `EmployeeEntity` 추가
- `AppDbContext` 추가
- `EmployeeConfiguration` 추가
- SQLite 매핑과 인덱스 구성

### Phase 3. Repository 및 UnitOfWork

- `EmployeeRepository` 구현
- `EfUnitOfWork` 구현
- 조회 projection, 중복 조회, 쓰기 저장 로직 구현

### Phase 4. Parser 구현

- `JsonEmployeeImportParser` 구현
- `CsvEmployeeImportParser` 구현
- `PlainTextEmployeeImportDetector` 구현

### Phase 5. DI 및 마이그레이션

- `AddInfrastructure()` 구현
- 초기 마이그레이션 `InitialCreate` 생성
- API 호스트에서 사용할 연결 문자열 샘플 구성
- Infrastructure 전용 테스트 프로젝트에서 DI/마이그레이션 연결 상태를 검증한다

### Phase 6. 검증 및 정리

- SQLite provider 기반 통합 테스트 녹색화
- 파일/네임스페이스 정리
- 문서와 실제 타입 이름 일치 여부 점검

## 6. 완료 기준

아래 조건을 모두 만족하면 Infrastructure 구현을 완료로 본다.

- `EmployeeContacts.Infrastructure`가 `Application` 추상화를 모두 구현한다.
- SQLite DB 스키마와 마이그레이션이 준비되어 있다.
- 조회/중복검사/저장 repository 동작이 테스트로 보호된다.
- CSV/JSON/text/plain parser 경로가 요구사항대로 동작한다.
- `AddInfrastructure()`만으로 API가 Infrastructure 서비스를 조립할 수 있다.
- API 계층은 parser 선택과 repository 구현 세부를 직접 알지 않는다.
- Infrastructure 전용 테스트 프로젝트에서 parser, repository, migration, DI 계약이 자동화 테스트로 보호된다.

## 7. 다음 계층 전달 기준

Infrastructure 완료 후 API 계층은 아래 전제를 사용한다.

- API는 `builder.Services.AddInfrastructure(builder.Configuration)`로 서비스를 등록한다.
- API는 `application/json`, `text/csv`, `text/plain`, `multipart/form-data`를 구분해 적절한 형식별 parser 또는 detector를 호출한다.
- API는 parser가 던진 `ApplicationError InvalidFormat` 기반 `ApplicationException`을 `400 Bad Request` + `ProblemDetails`로 변환한다.
- API는 저장 시점 DB 유니크 충돌이 요청 수준 오류로 전파되면 이를 행 단위 오류가 아닌 `ProblemDetails`로 변환한다.
- API는 `ConnectionStrings:EmployeeContacts`를 통해 SQLite 파일 위치를 제어한다.
- API 통합 테스트는 실제 SQLite provider와 마이그레이션을 사용해 end-to-end 계약을 검증한다.
