# Application 구현 체크리스트

> 현재 저장소에서 코드/테스트로 검증 가능한 항목만 완료 처리했다.
> `먼저 작성`, `Red 상태 확인`처럼 구현 당시 절차를 요구하는 항목은 현재 스냅샷만으로 입증할 수 없어 미체크로 남긴다.

## 1. 구현 범위와 고정 결정 확인

- [x] Application은 모든 유스케이스의 진입점으로 동작한다
- [x] Application은 Domain 규칙을 재구현하지 않고 값 객체와 `Employee.Create(...)`를 사용한다
- [x] Application은 HTTP 세부사항, `ProblemDetails`, Content-Type 분기, endpoint 구현을 알지 않는다
- [x] Application은 Infrastructure 구현 타입을 참조하지 않는다
- [x] 구현 대상은 CQRS 요청/응답 모델, Handler, Validator, DTO, 추상화, MediatR behavior, `AddApplication()`, Application 테스트로 한정한다
- [x] EF Core `DbContext`, SQLite 인덱스/마이그레이션, 실제 CSV/JSON 파싱, `text/plain` 판별 구현, API 예외 처리, OpenTelemetry exporter 설정은 이번 범위에서 제외한다
- [x] Application DI 진입점은 `AddApplication(this IServiceCollection services)` 하나로 고정한다
- [x] 조회 유스케이스는 `GetEmployeesQuery`, `GetEmployeesByNameQuery` 두 개로 고정한다
- [x] 등록 유스케이스는 `BulkCreateEmployeesCommand` 하나로 고정한다
- [x] 조회 정렬 보장은 `name ASC`, 그다음 `id ASC`이며 저장소 계약의 일부로 본다
- [x] `GetEmployeesByNameQuery`는 `trim` 후 exact match만 수행한다
- [x] `BulkEmployeeRecord.Row`는 실제 데이터 행 기준 1부터 시작한 값을 사용한다
- [x] 일괄 등록 결과는 `BulkCreateEmployeesResult`와 `BulkCreateEmployeesError` 계약으로 고정한다
- [x] 한 행에서는 여러 오류를 누적하지 않고 첫 번째 결정적 오류 1건만 기록한다
- [x] 필드 오류 우선순위는 `name -> email -> tel -> joined`로 고정한다
- [x] 중복 오류 우선순위는 `email -> tel`로 고정한다
- [x] 요청 내부 중복은 먼저 등장한 행을 유효로 보고 뒤에 나온 행을 실패 처리한다
- [x] 중복 비교는 정규화된 이메일/전화번호 기준으로 수행한다
- [x] 저장 가능한 행의 식별자는 Handler가 `Guid.CreateVersion7()`로 생성한다
- [x] 조회와 쓰기 계약은 초기 구현에서 하나의 `IEmployeeRepository`로 유지한다
- [x] 커밋은 `IUnitOfWork`로 분리한다
- [x] 파서 구현은 Infrastructure 책임이며, Application에는 파서/포맷 판별 계약만 둔다
- [x] 모든 `IRequest<TResponse>`는 FluentValidation 대상으로 본다
- [x] Behavior 순서는 `LoggingBehavior -> ValidationBehavior -> TracingBehavior`로 고정한다

## 2. 파일 구조 준비

- [x] `src/EmployeeContacts.Application/Abstractions/Persistence/IEmployeeRepository.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Abstractions/Persistence/IUnitOfWork.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Abstractions/Parsing/IEmployeeImportParser.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Abstractions/Parsing/IPlainTextEmployeeImportDetector.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Behaviors/LoggingBehavior.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Behaviors/ValidationBehavior.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Behaviors/TracingBehavior.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Common/Models/PagedResult.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/DependencyInjection/DependencyInjection.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Dtos/EmployeeDto.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Queries/GetEmployees/GetEmployeesQuery.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Queries/GetEmployees/GetEmployeesQueryHandler.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Queries/GetEmployees/GetEmployeesQueryValidator.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Queries/GetEmployeesByName/GetEmployeesByNameQuery.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Queries/GetEmployeesByName/GetEmployeesByNameQueryHandler.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Queries/GetEmployeesByName/GetEmployeesByNameQueryValidator.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Commands/BulkCreateEmployees/BulkCreateEmployeesCommand.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Commands/BulkCreateEmployees/BulkCreateEmployeesCommandHandler.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Commands/BulkCreateEmployees/BulkCreateEmployeesCommandValidator.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Commands/BulkCreateEmployees/BulkCreateEmployeesError.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Commands/BulkCreateEmployees/BulkCreateEmployeesResult.cs` 생성 또는 위치 확인
- [x] `src/EmployeeContacts.Application/Employees/Commands/BulkCreateEmployees/BulkEmployeeRecord.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Behaviors/ValidationBehaviorTests.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Behaviors/DependencyInjectionTests.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Common/Models/PagedResultTests.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Employees/Queries/GetEmployees/GetEmployeesQueryHandlerTests.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Employees/Queries/GetEmployees/GetEmployeesQueryValidatorTests.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Employees/Queries/GetEmployeesByName/GetEmployeesByNameQueryHandlerTests.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Employees/Queries/GetEmployeesByName/GetEmployeesByNameQueryValidatorTests.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Employees/Commands/BulkCreateEmployees/BulkCreateEmployeesCommandHandlerTests.cs` 생성 또는 위치 확인
- [x] `tests/EmployeeContacts.Application.Tests/Employees/Commands/BulkCreateEmployees/BulkCreateEmployeesCommandValidatorTests.cs` 생성 또는 위치 확인
- [x] 테스트 폴더 구조가 구현 폴더 구조를 그대로 따르는지 확인

## 3. Phase 1. 프로젝트 준비

- [x] `EmployeeContacts.Application`에 `EmployeeContacts.Domain` 프로젝트 참조 추가
- [x] `EmployeeContacts.Application`에 `MediatR` 패키지 추가
- [x] `EmployeeContacts.Application`에 `FluentValidation` 패키지 추가
- [x] `EmployeeContacts.Application.Tests`에 `EmployeeContacts.Application` 프로젝트 참조 추가
- [x] `EmployeeContacts.Application.Tests`는 `Application` 참조와 test double 전략으로 현재 테스트 범위를 충족한다
- [x] `EmployeeContacts.Application.Tests`는 mocking 라이브러리 대신 수동 test double을 사용한다
- [x] `EmployeeContacts.Application.Tests`는 `FluentAssertions` 대신 xUnit `Assert`를 사용한다
- [x] nullable reference types가 유지되는지 확인
- [x] 계층 참조 방향이 `Application -> Domain`만 유지되는지 확인

## 4. Phase 2. 조회 유스케이스

### 공통 DTO 및 모델

- [x] `PagedResult<T>` 추가
- [x] `PagedResult<T>.TotalPages`가 `TotalCount == 0`일 때 `0`으로 계산되는지 확인
- [x] `EmployeeDto` 추가
- [x] `EmployeeDto`가 외부 계약 필드명에 맞춰 `Tel` 속성을 사용하는지 확인

### GetEmployeesQuery

- [x] `GetEmployeesQuery`에 `page`, `pageSize` 입력을 정의
- [x] 기본값이 `page = 1`, `pageSize = 20`인지 확인
- [ ] `GetEmployeesQueryValidatorTests` 먼저 작성
- [x] `Validate_ShouldFail_WhenPageIsLessThanOne()` 작성
- [x] `Validate_ShouldFail_WhenPageSizeExceedsMaximum()` 작성
- [ ] 실패 테스트 실행으로 Red 상태 확인
- [x] `GetEmployeesQueryValidator` 구현
- [x] `page >= 1` 검증 구현
- [x] `pageSize >= 1` 검증 구현
- [x] `pageSize <= 100` 검증 구현
- [x] `GetEmployeesQueryHandlerTests` 작성
- [x] `Handle_ShouldReturnPagedEmployees_WithRequestedPaging()` 작성
- [x] `GetEmployeesQueryHandler` 구현
- [x] 저장소 `GetPagedAsync(page, pageSize, cancellationToken)` 호출 여부 확인
- [x] Handler가 `PagedResult<EmployeeDto>`를 그대로 반환하는지 확인

### GetEmployeesByNameQuery

- [x] `GetEmployeesByNameQuery`에 `name` 입력을 정의
- [ ] `GetEmployeesByNameQueryValidatorTests` 먼저 작성
- [x] `Validate_ShouldFail_WhenNameIsEmptyAfterTrim()` 작성
- [ ] 실패 테스트 실행으로 Red 상태 확인
- [x] `GetEmployeesByNameQueryValidator` 구현
- [x] `name`이 `trim` 후 비어 있지 않아야 하는 검증 구현
- [x] `GetEmployeesByNameQueryHandlerTests` 작성
- [x] `Handle_ShouldTrimNameBeforeSearching()` 작성
- [x] `Handle_ShouldReturnEmptyList_WhenNoEmployeeExists()` 작성
- [x] `GetEmployeesByNameQueryHandler` 구현
- [x] Handler 진입 전에 `trim` 처리하는지 확인
- [x] exact match 조회만 수행하는지 확인
- [x] 결과 없음 시 빈 컬렉션을 반환하는지 확인
- [x] 저장소 `GetByNameAsync(trimmedName, cancellationToken)` 호출 여부 확인

## 5. Phase 3. 등록 유스케이스

### 계약 및 Validator

- [x] `BulkEmployeeRecord` 추가
- [x] `BulkCreateEmployeesResult` 추가
- [x] `BulkCreateEmployeesError` 추가
- [x] `BulkCreateEmployeesCommand`에 `IReadOnlyList<BulkEmployeeRecord> Records` 입력 정의
- [x] `BulkCreateEmployeesCommandValidatorTests` 작성
- [x] `Records`가 `null`일 때 실패하는 테스트 작성
- [x] `Records`가 비어 있을 때 실패하는 테스트 작성
- [x] 각 행의 `Row < 1`일 때 실패하는 테스트 작성
- [ ] 실패 테스트 실행으로 Red 상태 확인
- [x] `BulkCreateEmployeesCommandValidator` 구현
- [x] `Records` null 금지 검증 구현
- [x] `Records` 1건 이상 검증 구현
- [x] 각 `BulkEmployeeRecord.Row >= 1` 검증 구현

### Handler 핵심 흐름

- [x] `BulkCreateEmployeesCommandHandlerTests` 작성
- [x] `Handle_ShouldCreateEmployees_ForValidRows()` 작성
- [x] `Handle_ShouldFailDuplicateEmailWithinRequest()` 작성
- [x] `Handle_ShouldFailDuplicateTelWithinRequest()` 작성
- [x] `Handle_ShouldFail_WhenEmailAlreadyExists()` 작성
- [x] `Handle_ShouldFail_WhenTelAlreadyExists()` 작성
- [x] `Handle_ShouldMapDomainException_ToBulkError()` 작성
- [x] `Handle_ShouldReturnPartialSuccessResult()` 작성
- [x] `Handle_ShouldReturnFailureResult_WhenNothingIsCreated()` 작성
- [x] `Handle_ShouldPreserveErrorOrderByRow()` 작성
- [x] `Handle_ShouldPrioritizeRequestDuplicate_BeforeRepositoryDuplicateChecks()` 작성
- [ ] 실패 테스트 실행으로 Red 상태 확인
- [x] `BulkCreateEmployeesCommandHandler` 구현
- [x] 입력 행을 순서대로 순회하는지 확인
- [x] 각 행을 Domain 값 객체로 먼저 검증/정규화하고, 저장 시점에 `Employee.Create(...)`를 호출하는지 확인
- [x] 정규화 성공 행만 요청 내부 중복 검사 대상으로 올리는지 확인
- [x] 요청 내부 이메일 중복 시 뒤에 나온 행을 실패 처리하는지 확인
- [x] 요청 내부 전화번호 중복 시 뒤에 나온 행을 실패 처리하는지 확인
- [x] 요청 내부 중복이 아닌 행만 저장소 중복 검사 대상으로 넘기는지 확인
- [x] 저장소 이메일 중복은 `duplicate_email`로 매핑하는지 확인
- [x] 저장소 전화번호 중복은 `duplicate_tel`로 매핑하는지 확인
- [x] 요청 내부 중복과 저장소 중복이 동시에 가능할 때 요청 내부 중복을 우선하는지 확인
- [x] 저장 가능한 행에만 `Guid.CreateVersion7()` 식별자를 부여하는지 확인
- [x] 저장 가능한 `Employee`만 `AddRangeAsync` 대상으로 모으는지 확인
- [x] 저장 가능한 행이 하나라도 있으면 `AddRangeAsync` 후 `SaveChangesAsync`를 호출하는지 확인
- [x] 저장 가능한 행이 하나도 없으면 커밋 없이 실패 결과를 반환하는지 확인
- [x] `Errors`가 입력 순서를 유지하는지 확인
- [x] 한 행당 오류 1건만 기록하는지 확인
- [x] `Total`, `Created`, `Failed` 집계가 올바른지 확인

### 오류 코드 및 메시지 매핑

- [x] 이름 Domain 예외를 `field = "name"`, `code = "invalid_name"`으로 매핑
- [x] 이메일 Domain 예외를 `field = "email"`, `code = "invalid_email"`으로 매핑
- [x] 전화번호 Domain 예외를 `field = "tel"`, `code = "invalid_tel"`으로 매핑
- [x] 입사일 오류를 `field = "joined"`, `code = "invalid_joined"`으로 매핑
- [x] `invalid_name`, `invalid_email`, `invalid_tel` 메시지는 `DomainException.Detail`을 그대로 사용하는지 확인
- [x] `invalid_joined` 메시지는 항상 `joined must be yyyy-MM-dd`로 고정하는지 확인
- [x] 중복 이메일 메시지는 `email already exists`로 고정하는지 확인
- [x] 중복 전화번호 메시지는 `tel already exists`로 고정하는지 확인
- [x] `Field` 값이 `name`, `email`, `tel`, `joined` 중 하나만 사용되는지 확인

## 6. Phase 4. 공통 구성

### 추상화

- [x] `IEmployeeRepository` 추가
- [x] `GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)` 계약 추가
- [x] `GetByNameAsync(string exactName, CancellationToken cancellationToken)` 계약 추가
- [x] `GetExistingEmailsAsync(IReadOnlyCollection<string> emails, CancellationToken cancellationToken)` 계약 추가
- [x] `GetExistingPhoneNumbersAsync(IReadOnlyCollection<string> phoneNumbers, CancellationToken cancellationToken)` 계약 추가
- [x] `AddRangeAsync(IReadOnlyCollection<Employee> employees, CancellationToken cancellationToken)` 계약 추가
- [x] `IUnitOfWork` 추가
- [x] `SaveChangesAsync(CancellationToken cancellationToken)` 계약 추가
- [x] `IEmployeeImportParser` 추가
- [x] `ParseAsync(string content, CancellationToken cancellationToken)` 계약 추가
- [x] `IPlainTextEmployeeImportDetector` 추가
- [x] `Resolve(string content)` 계약 추가

### MediatR Behaviors

- [x] `ValidationBehaviorTests` 작성
- [x] `Handle_ShouldThrowValidationException_WhenValidationFails()` 작성
- [x] `LoggingBehavior` 구현
- [x] 요청명과 처리 시간을 남길 수 있는 최소 구조만 가지는지 확인
- [x] `ValidationBehavior` 구현
- [x] 등록된 Validator를 모두 실행하는지 확인
- [x] Validation 실패 시 `ValidationException`을 던지는지 확인
- [x] `TracingBehavior` 구현
- [x] `ActivitySource`를 받아 요청 단위 span을 생성하는지 확인
- [x] Behavior 등록 순서가 `Logging -> Validation -> Tracing`인지 확인

### DI

- [x] `DependencyInjectionTests` 작성
- [x] `AddApplication_ShouldRegisterApplicationServices()` 작성
- [x] `DependencyInjection.AddApplication()` 구현
- [x] MediatR 등록 여부 확인
- [x] Validators 등록 여부 확인
- [x] Behaviors 등록 여부 확인
- [x] Infrastructure와 Api가 `AddApplication()`만으로 Application 서비스를 등록할 수 있는지 확인

## 7. Phase 5. 정리

- [x] 테스트 이름이 문서 기준과 어긋나지 않는지 점검
- [x] 테스트 메서드명은 영문, 의도는 `DisplayName` 한글 설명으로 유지되는지 점검
- [x] 공통 중복 제거
- [x] 공개 타입 이름이 문서와 실제 코드에서 일치하는지 확인
- [x] 기능별 폴더에 Request, Handler, Validator가 함께 배치되었는지 확인
- [x] 공통 모델은 `Common`, 기능 DTO는 `Employees/Dtos` 아래에 있는지 확인

## 8. 필수 테스트 체크

### 조회

- [x] `Handle_ShouldReturnPagedEmployees_WithRequestedPaging()` 통과
- [x] `Validate_ShouldFail_WhenPageIsLessThanOne()` 통과
- [x] `Validate_ShouldFail_WhenPageSizeExceedsMaximum()` 통과
- [x] `Handle_ShouldTrimNameBeforeSearching()` 통과
- [x] `Validate_ShouldFail_WhenNameIsEmptyAfterTrim()` 통과
- [x] `Handle_ShouldReturnEmptyList_WhenNoEmployeeExists()` 통과

### 등록

- [x] `Handle_ShouldCreateEmployees_ForValidRows()` 통과
- [x] `Handle_ShouldFailDuplicateEmailWithinRequest()` 통과
- [x] `Handle_ShouldFailDuplicateTelWithinRequest()` 통과
- [x] `Handle_ShouldFail_WhenEmailAlreadyExists()` 통과
- [x] `Handle_ShouldFail_WhenTelAlreadyExists()` 통과
- [x] `Handle_ShouldMapDomainException_ToBulkError()` 통과
- [x] `Handle_ShouldReturnPartialSuccessResult()` 통과
- [x] `Handle_ShouldReturnFailureResult_WhenNothingIsCreated()` 통과
- [x] `Handle_ShouldPreserveErrorOrderByRow()` 통과

### Behavior 및 DI

- [x] `Handle_ShouldThrowValidationException_WhenValidationFails()` 통과
- [x] `AddApplication_ShouldRegisterApplicationServices()` 통과

## 9. 완료 기준 점검

- [x] `EmployeeContacts.Application`이 `Infrastructure`를 참조하지 않는지 확인
- [x] 조회/등록 유스케이스가 모두 MediatR 요청과 Handler로 구현되었는지 확인
- [x] 페이징, 이름 검색, 부분 성공 등록 규칙이 자동화 테스트로 보호되는지 확인
- [x] Domain 예외가 등록 오류 계약으로 일관되게 변환되는지 확인
- [x] 중복 이메일/전화번호 처리가 요청 내부와 저장소 기준 모두 검증되는지 확인
- [x] `AddApplication()`만으로 Application 서비스 구성이 가능한지 확인
- [x] Query/Command Validator가 Application 입력 규칙만 검증하고 Domain 규칙을 재구현하지 않는지 확인
- [x] 조회 정렬 보장이 저장소 계약으로 유지되는지 확인

## 10. 다음 계층 전달 전 확인

- [x] Infrastructure에 전달할 구현 계약이 `IEmployeeRepository`, `IUnitOfWork`, parser 추상화로 정리되었는지 확인
- [x] API가 `BulkEmployeeRecord` 목록을 만든 뒤 Command를 보내는 흐름을 사용할 수 있는지 확인
- [x] API가 `Created > 0`이면 `201 Created`, `Created == 0`이면 `400 Bad Request`로 매핑할 수 있는 결과 계약인지 확인
- [x] API가 FluentValidation 예외를 `ValidationProblemDetails`로 변환할 수 있는지 확인
- [x] API가 그 외 예외와 등록 실패 결과를 `ProblemDetails`로 변환할 수 있는지 확인
