# Infrastructure 구현 체크리스트

> 현재 저장소에서 코드/테스트로 검증 가능한 항목만 완료 처리했다.
> `먼저 작성`, `Red 상태 확인`, `마이그레이션 생성`처럼 구현 절차나 산출물이 필요한 항목은 현재 스냅샷만으로 입증할 수 없어 미체크로 남긴다.

## 1. 구현 범위와 고정 결정 확인

- [x] Infrastructure는 Application이 정의한 저장소, 트랜잭션, parser 추상화의 실제 구현을 제공한다
- [x] Infrastructure는 외부 저장 형식과 DB 세부사항을 캡슐화하고 Application 공개 계약을 변경하지 않는다
- [x] Infrastructure는 API의 요청 형식 분기, HTTP 상태코드, `ProblemDetails` 생성을 알지 않는다
- [x] 구현 대상은 SQLite 기반 persistence, parser, DI 조립, 마이그레이션, Infrastructure 테스트로 한정한다
- [x] Controller 또는 Minimal API 엔드포인트는 이번 범위에서 제외한다
- [x] HTTP `Content-Type` 분기와 `ProblemDetails` 응답 생성은 이번 범위에서 제외한다
- [x] Swagger/OpenAPI 문서화와 실제 OpenTelemetry 외부 exporter 연동은 이번 범위에서 제외한다
- [x] DI 진입점은 `DependencyInjection.AddInfrastructure(this IServiceCollection services, IConfiguration configuration)`로 고정한다
- [x] 연결 문자열 키는 `ConnectionStrings:EmployeeContacts`로 고정한다
- [x] 개발 기본 SQLite 연결 문자열은 API `appsettings.json`의 `Data Source=employee-contacts.db`를 사용한다
- [x] EF Core는 Domain `Employee`를 직접 추적하지 않고 `EmployeeEntity` persistence model을 사용한다
- [x] `AppDbContext`는 단일 aggregate 저장소로 시작하고 `Employees` 테이블만 다룬다
- [x] 조회 정렬 보장은 repository 쿼리 계약으로 유지하며 DB 기본 정렬에 의존하지 않는다
- [x] `EmployeeRepository.GetPagedAsync(...)`와 `GetByNameAsync(...)`는 `OrderBy(Name).ThenBy(Id)`를 적용한다
- [x] `AddRangeAsync(...)`는 저장만 예약하고 `SaveChanges`를 직접 호출하지 않는다
- [x] `EfUnitOfWork.SaveChangesAsync(...)`가 실제 커밋 경계를 담당한다
- [x] 저장 시점 유니크 충돌은 행 단위 오류로 재구성하지 않고 요청 수준 오류로 상위 계층에 전달한다
- [x] CSV parser는 구조만 검증하고 정규화/비즈니스 규칙 검증은 Application과 Domain에 남긴다
- [x] JSON parser는 루트 배열 및 필수 문자열 속성만 검증하고 날짜/필드 규칙 검증은 수행하지 않는다
- [x] `text/plain` 판별은 먼저 유효한 JSON 배열인지 확인하고 아니면 CSV parser를 선택한다
- [x] parser의 포맷 오류는 `ApplicationException`과 `ApplicationError InvalidFormat`으로 표현한다

## 2. 파일 구조 준비

- [x] [docs/implementation-plan/infrastructure-implementation-plan.md](D:/Source/GitHub/task_20260312/docs/implementation-plan/infrastructure-implementation-plan.md) 존재 확인
- [x] [src/EmployeeContacts.Infrastructure/EmployeeContacts.Infrastructure.csproj](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/EmployeeContacts.Infrastructure.csproj) 존재 확인
- [ ] [src/EmployeeContacts.Infrastructure/DependencyInjection/DependencyInjection.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/DependencyInjection/DependencyInjection.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Parsing/Csv/CsvEmployeeImportParser.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Parsing/Csv/CsvEmployeeImportParser.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Parsing/Json/JsonEmployeeImportParser.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Parsing/Json/JsonEmployeeImportParser.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Parsing/Text/PlainTextEmployeeImportDetector.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Parsing/Text/PlainTextEmployeeImportDetector.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Persistence/AppDbContext.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Persistence/AppDbContext.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Persistence/Entities/EmployeeEntity.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Persistence/Entities/EmployeeEntity.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Persistence/Repositories/EmployeeRepository.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Persistence/Repositories/EmployeeRepository.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Persistence/UnitOfWork/EfUnitOfWork.cs](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Persistence/UnitOfWork/EfUnitOfWork.cs) 생성 또는 위치 확인
- [ ] [src/EmployeeContacts.Infrastructure/Migrations](D:/Source/GitHub/task_20260312/src/EmployeeContacts.Infrastructure/Migrations) 폴더 생성 또는 위치 확인
- [ ] [tests/EmployeeContacts.Infrastructure.Tests](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Infrastructure.Tests) 프로젝트 생성 또는 위치 확인
- [x] [tests/EmployeeContacts.Api.IntegrationTests/EmployeeContacts.Api.IntegrationTests.csproj](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Api.IntegrationTests/EmployeeContacts.Api.IntegrationTests.csproj) 존재 확인
- [ ] 테스트 폴더 구조가 구현 폴더 구조와 대응되도록 준비되었는지 확인

## 3. Phase 1. 프로젝트 준비

- [ ] `EmployeeContacts.Infrastructure`에 `EmployeeContacts.Application` 프로젝트 참조 추가
- [ ] `EmployeeContacts.Infrastructure`에 `EmployeeContacts.Domain` 프로젝트 참조 추가
- [ ] `EmployeeContacts.Infrastructure`에 `Microsoft.EntityFrameworkCore` 패키지 추가
- [ ] `EmployeeContacts.Infrastructure`에 `Microsoft.EntityFrameworkCore.Sqlite` 패키지 추가
- [ ] `EmployeeContacts.Infrastructure`에 `Microsoft.EntityFrameworkCore.Design` 패키지 추가
- [ ] `EmployeeContacts.Infrastructure`에 DI 확장 구현에 필요한 패키지 참조 추가
- [ ] `tests/EmployeeContacts.Infrastructure.Tests` 프로젝트 생성
- [ ] `tests/EmployeeContacts.Infrastructure.Tests`에 `Infrastructure` 프로젝트 참조 추가
- [ ] `tests/EmployeeContacts.Infrastructure.Tests`에 `Application` 프로젝트 참조 추가
- [ ] `tests/EmployeeContacts.Infrastructure.Tests`에 `Domain` 프로젝트 참조 추가
- [ ] `tests/EmployeeContacts.Infrastructure.Tests`에 SQLite 실제 provider 기반 테스트 패키지 참조 추가
- [ ] `tests/EmployeeContacts.Infrastructure.Tests`는 xUnit `Assert`를 사용하도록 유지
- [ ] nullable reference types가 유지되는지 확인
- [ ] 계층 참조 방향이 `Infrastructure -> Application`, `Infrastructure -> Domain`만 유지되는지 확인

## 4. Phase 2. Persistence 골격

### EmployeeEntity 및 DbContext

- [ ] `EmployeeEntity` 추가
- [ ] `EmployeeEntity`에 `Id`, `Name`, `Email`, `PhoneNumber`, `Joined`, `CreatedAt`, `UpdatedAt` 속성 정의
- [ ] `AppDbContext` 추가
- [ ] `AppDbContext`가 `DbSet<EmployeeEntity> Employees`를 가지는지 확인
- [ ] `AppDbContext`가 이번 범위에서 `Employees` 외 다른 `DbSet`을 추가하지 않는지 확인
- [ ] `OnModelCreating()`에서 `EmployeeConfiguration`만 적용하는지 확인

### EmployeeConfiguration 매핑

- [ ] 테이블명이 `Employees`인지 확인
- [ ] 기본 키가 `Id`인지 확인
- [ ] `Name`을 필수로 설정
- [ ] `Email`을 필수로 설정
- [ ] `PhoneNumber`를 필수로 설정
- [ ] `PhoneNumber`에 `MaxLength(15)`를 설정
- [ ] `PhoneNumber` 컬럼명을 그대로 유지
- [ ] `Email` 유니크 인덱스를 설정
- [ ] `PhoneNumber` 유니크 인덱스를 설정
- [ ] `Name` 조회 인덱스를 설정
- [ ] `Joined`, `CreatedAt`, `UpdatedAt`을 모두 필수로 설정
- [ ] `Joined`를 `yyyy-MM-dd` 형식의 SQLite `TEXT`로 저장하도록 converter를 명시
- [ ] `CreatedAt`, `UpdatedAt`을 UTC round-trip 형식의 SQLite `TEXT`로 저장하도록 converter를 명시

## 5. Phase 3. Repository 및 UnitOfWork

### EmployeeRepository

- [ ] `EmployeeRepository` 구현
- [ ] `IEmployeeRepository`를 그대로 구현하는지 확인
- [ ] `GetPagedAsync(page, pageSize, cancellationToken)` 구현
- [ ] `GetPagedAsync(...)`가 `OrderBy(Name).ThenBy(Id)` 후 `Skip/Take`를 적용하는지 확인
- [ ] `GetPagedAsync(...)`가 `AsNoTracking()`을 사용하는지 확인
- [ ] 조회가 `EmployeeEntity`에서 `EmployeeDto`로 직접 projection 되는지 확인
- [ ] `GetByNameAsync(exactName, cancellationToken)` 구현
- [ ] `GetByNameAsync(...)`가 `Name == exactName` exact match로 조회하는지 확인
- [ ] `GetByNameAsync(...)`가 `OrderBy(Name).ThenBy(Id)`를 적용하는지 확인
- [ ] `GetByNameAsync(...)`가 `AsNoTracking()`을 사용하는지 확인
- [ ] `GetExistingEmailsAsync(...)` 구현
- [ ] `GetExistingEmailsAsync(...)`가 입력이 비어 있으면 DB 조회 없이 빈 `HashSet<string>`을 반환하는지 확인
- [ ] `GetExistingEmailsAsync(...)`가 전달된 정규화 이메일과 일치하는 값만 반환하는지 확인
- [ ] `GetExistingPhoneNumbersAsync(...)` 구현
- [ ] `GetExistingPhoneNumbersAsync(...)`가 입력이 비어 있으면 DB 조회 없이 빈 `HashSet<string>`을 반환하는지 확인
- [ ] `GetExistingPhoneNumbersAsync(...)`가 전달된 정규화 전화번호와 일치하는 값만 반환하는지 확인
- [ ] `AddRangeAsync(...)` 구현
- [ ] `AddRangeAsync(...)`가 Domain `Employee`를 `EmployeeEntity`로 변환하는지 확인
- [ ] `AddRangeAsync(...)`가 `SaveChanges`를 직접 호출하지 않는지 확인

### EfUnitOfWork

- [ ] `EfUnitOfWork` 구현
- [ ] `IUnitOfWork`를 구현하는지 확인
- [ ] `SaveChangesAsync(...)`가 내부적으로 `AppDbContext.SaveChangesAsync(...)`를 호출하는지 확인
- [ ] 유니크 인덱스 충돌 시 가능한 범위에서 `email` 또는 `tel` 필드를 식별하는지 확인
- [ ] 저장 시점 충돌을 행 단위 오류로 재구성하지 않는지 확인
- [ ] 예기치 않은 DB 오류는 래핑하지 않고 상위로 전파하는지 확인

## 6. Phase 4. Parser 구현

### JsonEmployeeImportParser

- [ ] `JsonEmployeeImportParser` 구현
- [ ] `IJsonEmployeeImportParser`와 `IEmployeeImportParser`를 구현하는지 확인
- [ ] `System.Text.Json`을 사용하는지 확인
- [ ] 루트가 JSON 배열이 아니면 `invalid_format`으로 실패하는지 확인
- [ ] 각 원소가 JSON 객체가 아니면 `invalid_format`으로 실패하는지 확인
- [ ] `name`, `email`, `tel`, `joined` 문자열 속성이 모두 필요하도록 구현
- [ ] 속성명 비교가 대소문자를 구분하지 않는지 확인
- [ ] 추가 속성은 무시하는지 확인
- [ ] `null`, 숫자, 배열, 객체 값은 형식 오류로 처리하는지 확인
- [ ] 배열 순서대로 `Row = 1..N`을 부여하는지 확인
- [ ] 빈 배열이면 형식 오류로 처리하는지 확인
- [ ] 날짜 문자열을 parser에서 `DateOnly`로 변환하지 않는지 확인

### CsvEmployeeImportParser

- [ ] `CsvEmployeeImportParser` 구현
- [ ] `ICsvEmployeeImportParser`와 `IEmployeeImportParser`를 구현하는지 확인
- [ ] 빈 문자열 또는 공백만 있는 입력을 `invalid_format`으로 실패시키는지 확인
- [ ] `\r\n`, `\n` 줄 구분을 모두 허용하는지 확인
- [ ] UTF-8 BOM이 있는 입력을 허용하는지 확인
- [ ] 마지막의 빈 줄은 무시하는지 확인
- [ ] 중간의 빈 줄은 형식 오류로 처리하는지 확인
- [ ] 헤더가 `name,email,tel,joined` 순서일 때만 헤더로 인식하는지 확인
- [ ] 헤더 비교가 대소문자만 무시하는지 확인
- [ ] 헤더가 있으면 첫 데이터 줄부터 `Row = 1`을 부여하는지 확인
- [ ] 헤더가 없으면 첫 줄을 `Row = 1`로 처리하는지 확인
- [ ] 각 데이터 줄이 정확히 4개 컬럼이 아니면 형식 오류로 처리하는지 확인
- [ ] 큰따옴표 인용이 포함된 줄을 형식 오류로 처리하는지 확인
- [ ] 파싱 결과 데이터 행 수가 0이면 형식 오류로 처리하는지 확인
- [ ] 각 데이터 줄을 `BulkEmployeeRecord(Row, Name, Email, Tel, Joined)`로 변환하는지 확인
- [ ] 필드 `trim`을 parser에서 수행하지 않고 원문을 유지하는지 확인

### PlainTextEmployeeImportDetector

- [ ] `PlainTextEmployeeImportDetector` 구현
- [ ] `IPlainTextEmployeeImportDetector`를 구현하는지 확인
- [ ] 유효한 JSON 배열이면 JSON parser를 반환하는지 확인
- [ ] JSON 배열이 아니면 CSV parser를 반환하는지 확인
- [ ] detector가 parser 선택만 수행하고 예외를 던지지 않는지 확인

### parser 오류 계약

- [ ] parser와 detector가 `EmployeeContacts.Application.Common.Errors.ApplicationException`을 사용하는지 확인
- [ ] parser 오류 코드가 초기 구현에서 `invalid_format` 하나로 고정되는지 확인
- [ ] 기본 `detail`이 `Request body format is invalid.`인지 확인
- [ ] 대표 구조 오류에 한해 더 구체적인 `detail`을 허용하는지 확인
- [ ] `invalid_content_type` 오류를 Infrastructure에서 만들지 않는지 확인

## 7. Phase 5. DI 및 마이그레이션

### DependencyInjection

- [ ] `DependencyInjection.AddInfrastructure()` 구현
- [ ] `ConnectionStrings:EmployeeContacts`가 없거나 비어 있으면 즉시 실패하는지 확인
- [ ] `DbContext`, repository, unit of work, parser, detector를 모두 등록하는지 확인
- [ ] `AppDbContext`, repository, unit of work를 `Scoped`로 등록하는지 확인
- [ ] parser와 detector를 `Singleton`으로 등록하는지 확인
- [ ] 마이그레이션 assembly를 `EmployeeContacts.Infrastructure`로 고정하는지 확인

### SQLite 및 마이그레이션

- [ ] provider로 `Microsoft.EntityFrameworkCore.Sqlite`를 사용하는지 확인
- [ ] 마이그레이션 경로가 `src/EmployeeContacts.Infrastructure/Migrations`인지 확인
- [ ] 초기 마이그레이션 이름이 `InitialCreate`인지 확인
- [ ] 초기 마이그레이션 파일이 생성되었는지 확인
- [ ] API 시작 시 자동 마이그레이션 적용을 구현 범위에 포함하지 않는지 확인
- [ ] 테스트에서 빈 데이터베이스에 마이그레이션을 적용해 최신 스키마를 맞추는지 확인
- [ ] API `appsettings.json`에 기본 SQLite 연결 문자열 샘플이 추가되었는지 확인

## 8. Phase 6. 테스트 구현

### parser 테스트

- [ ] [tests/EmployeeContacts.Infrastructure.Tests/Parsing/Csv/CsvEmployeeImportParserTests.cs](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Infrastructure.Tests/Parsing/Csv/CsvEmployeeImportParserTests.cs) 생성 또는 위치 확인
- [ ] [tests/EmployeeContacts.Infrastructure.Tests/Parsing/Json/JsonEmployeeImportParserTests.cs](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Infrastructure.Tests/Parsing/Json/JsonEmployeeImportParserTests.cs) 생성 또는 위치 확인
- [ ] [tests/EmployeeContacts.Infrastructure.Tests/Parsing/Text/PlainTextEmployeeImportDetectorTests.cs](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Infrastructure.Tests/Parsing/Text/PlainTextEmployeeImportDetectorTests.cs) 생성 또는 위치 확인
- [ ] `ParseAsync_ShouldAssignRowNumbers_FromFirstDataRow_WhenHeaderExists()` 작성
- [ ] `ParseAsync_ShouldAssignRowNumbers_FromFirstLine_WhenHeaderDoesNotExist()` 작성
- [ ] `ParseAsync_ShouldThrow_WhenCsvHeaderOrderIsInvalid()` 작성
- [ ] `ParseAsync_ShouldThrow_WhenCsvColumnCountIsInvalid()` 작성
- [ ] `ParseAsync_ShouldThrow_WhenCsvContainsQuotedField()` 작성
- [ ] `ParseAsync_ShouldParseJsonArray_ToBulkEmployeeRecords()` 작성
- [ ] `ParseAsync_ShouldThrow_WhenJsonRootIsNotArray()` 작성
- [ ] `ParseAsync_ShouldThrow_WhenJsonPropertyIsMissing()` 작성
- [ ] `Resolve_ShouldReturnJsonParser_WhenContentIsValidJsonArray()` 작성
- [ ] `Resolve_ShouldReturnCsvParser_WhenContentIsNotJsonArray()` 작성
- [ ] parser 실패 테스트 실행으로 Red 상태를 확인
- [ ] parser 구현 후 필수 테스트를 녹색화

### repository 및 persistence 테스트

- [ ] [tests/EmployeeContacts.Infrastructure.Tests/Persistence/Repositories/EmployeeRepositoryTests.cs](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Infrastructure.Tests/Persistence/Repositories/EmployeeRepositoryTests.cs) 생성 또는 위치 확인
- [ ] [tests/EmployeeContacts.Infrastructure.Tests/Persistence/UnitOfWork/EfUnitOfWorkTests.cs](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Infrastructure.Tests/Persistence/UnitOfWork/EfUnitOfWorkTests.cs) 생성 또는 위치 확인
- [ ] [tests/EmployeeContacts.Infrastructure.Tests/Persistence/Migrations/DatabaseMigrationTests.cs](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Infrastructure.Tests/Persistence/Migrations/DatabaseMigrationTests.cs) 생성 또는 위치 확인
- [ ] `GetPagedAsync_ShouldReturnEmployees_OrderedByNameThenId()` 작성
- [ ] `GetByNameAsync_ShouldReturnEmployees_WithExactNameMatch()` 작성
- [ ] `GetExistingEmailsAsync_ShouldReturnMatchingEmailsOnly()` 작성
- [ ] `GetExistingPhoneNumbersAsync_ShouldReturnMatchingPhoneNumbersOnly()` 작성
- [ ] `SaveChangesAsync_ShouldPersistEmployees_AddedByRepository()` 작성
- [ ] `DatabaseSchema_ShouldCreateUniqueIndexes_ForEmailAndPhoneNumber()` 작성
- [ ] repository/persistence 실패 테스트 실행으로 Red 상태를 확인
- [ ] SQLite 실제 provider 기반으로 필수 테스트를 녹색화

### DI 및 마이그레이션 테스트

- [ ] [tests/EmployeeContacts.Infrastructure.Tests/DependencyInjection/DependencyInjectionTests.cs](D:/Source/GitHub/task_20260312/tests/EmployeeContacts.Infrastructure.Tests/DependencyInjection/DependencyInjectionTests.cs) 생성 또는 위치 확인
- [ ] `AddInfrastructure_ShouldRegisterInfrastructureServices()` 작성
- [ ] `DatabaseMigration_ShouldApplyInitialCreateMigration()` 작성
- [ ] DI와 마이그레이션 연결 테스트 실행으로 Red 상태를 확인
- [ ] `AddInfrastructure()` 및 마이그레이션 테스트를 녹색화

## 9. 정리

- [ ] parser, repository, unit of work 네임스페이스가 문서 구조와 일치하는지 점검
- [ ] 공개 타입 이름이 문서와 실제 코드에서 일치하는지 점검
- [ ] 불필요한 EF Core 세부사항이 Application 계약으로 새어 나오지 않는지 점검
- [ ] 조회 projection이 DTO 계약의 `Tel` 필드와 일치하는지 점검
- [ ] 중복 검사 입력이 정규화 값 기준으로만 수행되는지 점검
- [ ] 파일/폴더 네이밍을 최종 점검

## 10. 완료 기준 점검

- [ ] `EmployeeContacts.Infrastructure`가 `Application` 추상화를 모두 구현하는지 확인
- [ ] SQLite DB 스키마와 초기 마이그레이션이 준비되었는지 확인
- [ ] 조회, 중복 검사, 저장 repository 동작이 자동화 테스트로 보호되는지 확인
- [ ] CSV, JSON, `text/plain` parser 경로가 요구사항대로 동작하는지 확인
- [ ] `AddInfrastructure()`만으로 API가 Infrastructure 서비스를 조립할 수 있는지 확인
- [ ] API 계층이 parser 선택과 repository 구현 세부를 직접 알지 않아도 되는지 확인
- [ ] Infrastructure 전용 테스트 프로젝트에서 parser, repository, migration, DI 계약이 보호되는지 확인
- [ ] 현재 체크리스트 상태가 실제 저장소 스냅샷과 일치하는지 확인

## 11. 다음 계층 전달 전 확인

- [ ] API가 `builder.Services.AddInfrastructure(builder.Configuration)`로 서비스를 등록할 수 있는지 확인
- [ ] API가 `application/json`, `text/csv`, `text/plain`, `multipart/form-data` 분기를 올릴 수 있는 parser 조합이 준비되었는지 확인
- [ ] API가 parser의 `invalid_format` 예외를 `400 Bad Request` + `ProblemDetails`로 변환할 수 있는지 확인
- [ ] API가 저장 시점 DB 유니크 충돌을 요청 수준 `ProblemDetails`로 변환할 수 있는지 확인
- [ ] API가 `ConnectionStrings:EmployeeContacts`로 SQLite 파일 위치를 제어할 수 있는지 확인
- [ ] API 통합 테스트가 실제 SQLite provider와 마이그레이션을 사용한 end-to-end 검증으로 확장 가능한지 확인
