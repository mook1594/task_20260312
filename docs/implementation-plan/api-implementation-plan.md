# API 최종 구현 문서

## 1. 문서 목적

이 문서는 `EmployeeContacts.Api` 구현의 최종 기준이다.

- Presentation 계층의 구현 범위와 비범위를 확정한다.
- HTTP 계약, 예외 응답, Swagger, 통합 테스트 기준을 결정 완료 상태로 고정한다.
- 구현 중 남는 선택지를 없애고, 테스트 우선 순서와 완료 기준을 명확히 한다.

이 문서의 기준이 기존 초안보다 우선한다.

## 2. 현재 기준점

현재 저장소 상태는 아래를 기준으로 본다.

- `EmployeeContacts.Domain`은 현재 `Employee` aggregate 범위에서 구현 완료 상태다.
- `EmployeeContacts.Application`은 CQRS 요청/응답, validator, behavior, DI가 구현되어 있다.
- `EmployeeContacts.Infrastructure`는 SQLite persistence, repository, parser, detector, DI가 구현되어 있다.
- `EmployeeContacts.Api`는 현재 ASP.NET Core 기본 템플릿 수준이며, 실제 endpoint와 예외 매핑은 아직 없다.
- `tests/EmployeeContacts.Api.IntegrationTests`는 프로젝트만 있고 실제 API 계약 테스트는 아직 없다.

결정:

- 이번 API 구현은 이미 구현된 `Application`과 `Infrastructure`를 조립해 요구사항의 HTTP 계약을 완성하는 작업으로 본다.
- API는 `Infrastructure`의 parser/repository 구체 구현을 사용할 수 있다.

## 3. 구현 대상과 비대상

### 구현 대상

- ASP.NET Core host 부트스트랩
- `AddApplication()` / `AddInfrastructure()` 조립
- `EmployeeController`
- `GET /api/employee`
- `GET /api/employee/{name}`
- `POST /api/employee`
- Content-Type 분기
- multipart 파일 읽기
- `ProblemDetails` / `ValidationProblemDetails` 예외 응답 매핑
- Swagger/OpenAPI 설정
- 최소 관측성 부트스트랩
- API 통합 테스트

### 구현 대상 아님

- 인증/인가
- 비동기 배치 처리
- parser/repository/domain 규칙 자체 변경
- 저장 시점 유니크 충돌을 행 단위 오류 목록으로 재구성하는 로직
- 프론트엔드 UI

## 4. 최종 결정 사항

### 4.1 엔드포인트 스타일

API 엔드포인트 스타일은 `Controllers`로 고정한다.

결정:

- `EmployeeController`를 생성한다.
- Minimal API는 이번 범위에 포함하지 않는다.

결정 이유:

- 현재 템플릿이 controller 기반으로 시작되어 있다.
- `docs/4. tdd-and-delivery-guide.md`의 API 체크리스트와 일치한다.
- Swagger, `ProblemDetails`, 통합 테스트 구성이 단순해진다.

### 4.2 Program 조립

`Program.cs`는 아래 역할을 갖는 진입점으로 고정한다.

결정:

- `AddControllers()` 등록
- `AddApplication()` 등록
- `AddInfrastructure(builder.Configuration)` 등록
- `AddProblemDetails()` 등록
- 예외 처리기 등록
- Swagger/OpenAPI 등록
- 최소 OpenTelemetry / 요청 로깅 등록
- `MapControllers()`로 endpoint 노출

적용 원칙:

- 자동 마이그레이션 실행은 이번 범위에 포함하지 않는다.
- `ConnectionStrings:EmployeeContacts`는 API 설정에서 유지한다.
- API는 `Infrastructure`의 DbContext 세부를 직접 사용하지 않는다.

### 4.3 직원 목록 조회 계약

목록 조회 endpoint는 아래 계약으로 고정한다.

```http
GET /api/employee?page={page}&pageSize={pageSize}
```

결정:

- `page`, `pageSize`는 query string으로 받는다.
- 지정되지 않으면 `page = 1`, `pageSize = 20`을 사용한다.
- API는 `GetEmployeesQuery(page, pageSize)`를 생성해 MediatR로 전달한다.
- 성공 시 `PagedResult<EmployeeDto>`를 그대로 `200 OK`로 반환한다.
- 잘못된 `page`, `pageSize`는 `ValidationProblemDetails`와 함께 `400 Bad Request`로 반환한다.

결정 이유:

- paging validation은 이미 Application validator가 책임지고 있다.
- API는 HTTP 입력과 Application 요청 간의 매핑만 담당한다.

### 4.4 직원 이름 조회 계약

이름 조회 endpoint는 아래 계약으로 고정한다.

```http
GET /api/employee/{name}
```

결정:

- `name`은 path parameter로 받는다.
- API는 값을 그대로 `GetEmployeesByNameQuery(name)`에 담아 전달한다.
- 실제 `trim` 및 exact match 검증은 Application이 수행한다.
- 검색 결과가 없으면 빈 배열과 `200 OK`를 반환한다.
- `name`이 공백만 있는 경우는 `ValidationProblemDetails`와 함께 `400 Bad Request`로 반환한다.

### 4.5 일괄 등록 계약

등록 endpoint는 아래 계약으로 고정한다.

```http
POST /api/employee
```

지원 Content-Type:

- `multipart/form-data`
- `text/plain`
- `text/csv`
- `application/json`

결정:

- API는 parser가 반환하는 `IReadOnlyList<BulkEmployeeRecord>`를 `BulkCreateEmployeesCommand`로 감싸 MediatR에 전달한다.
- `BulkCreateEmployeesResult.Created > 0`이면 `201 Created`와 결과 본문을 반환한다.
- `BulkCreateEmployeesResult.Created == 0`이면 `400 Bad Request` + `ProblemDetails`로 변환한다.
- 성공 응답에는 `Location` 헤더를 포함하지 않는다.

### 4.6 Content-Type 분기 규칙

Content-Type 분기 규칙은 아래로 고정한다.

#### `application/json`

- 요청 body 전체를 문자열로 읽는다.
- `JsonEmployeeImportParser`에 직접 전달한다.

#### `text/csv`

- 요청 body 전체를 문자열로 읽는다.
- `CsvEmployeeImportParser`에 직접 전달한다.

#### `text/plain`

- 요청 body 전체를 문자열로 읽는다.
- `IPlainTextEmployeeImportDetector.Resolve(content)`로 parser를 선택한다.
- detector가 JSON 배열을 감지하면 JSON parser를 사용하고, 아니면 CSV parser를 사용한다.

#### `multipart/form-data`

- 파일 파트는 `employeesFile` 하나만 허용한다.
- 파일 내용을 문자열로 읽는다.
- 파일 내용은 `IPlainTextEmployeeImportDetector`로 판별한다.

결정:

- multipart 업로드는 파일의 파트 Content-Type이나 확장자에 의존하지 않고, 파일 내용 기준으로 JSON/CSV를 판별한다.
- 파일 파트 누락, 다중 파일, 잘못된 파트명, 빈 파일은 모두 `400 Bad Request` + `ProblemDetails`로 처리한다.
- 지원하지 않는 `Content-Type`은 `415 Unsupported Media Type` + `ProblemDetails`로 처리한다.

결정 이유:

- detector를 재사용하면 직접 입력과 파일 업로드 모두 동일한 파싱 규칙을 유지할 수 있다.
- 파일 메타데이터 의존을 줄여 클라이언트 구현 차이에 덜 민감하게 만든다.

### 4.7 요청 본문 오류 처리 기준

등록 요청 자체 오류는 아래와 같이 처리한다.

- 빈 요청 body: `400 Bad Request` + `ProblemDetails`
- multipart 파일 파트 누락: `400 Bad Request` + `ProblemDetails`
- multipart 파일이 비어 있음: `400 Bad Request` + `ProblemDetails`
- JSON 배열 아님: `400 Bad Request` + `ProblemDetails`
- CSV 구조 오류: `400 Bad Request` + `ProblemDetails`
- parser의 `invalid_format`: `400 Bad Request` + `ProblemDetails`
- 지원하지 않는 `Content-Type`: `415 Unsupported Media Type` + `ProblemDetails`

응답 원칙:

- 내부 예외 메시지를 그대로 노출하지 않는다.
- 표준 필드 `type`, `title`, `detail`, `status`를 채운다.
- `traceId`를 항상 포함한다.

### 4.8 예외 매핑 전략

API 예외 매핑은 아래 기준으로 고정한다.

- `FluentValidation.ValidationException`
  - `400 Bad Request`
  - `ValidationProblemDetails`
- `EmployeeContacts.Application.Common.Errors.ApplicationException`
  - `400 Bad Request`
  - `ProblemDetails`
- 저장 시점 DB 유니크 충돌
  - `400 Bad Request`
  - 요청 수준 `ProblemDetails`
- 그 외 예외
  - `500 Internal Server Error`
  - 일반 `ProblemDetails`

세부 결정:

- validation failure의 key는 외부 입력 이름 기준으로 `page`, `pageSize`, `name` 형태를 사용한다.
- `500` detail은 일반화된 문구로 고정하고 내부 예외 메시지는 숨긴다.
- 저장 시점 유니크 충돌은 행 단위 `errors[]`로 변환하지 않는다.

### 4.9 Swagger/OpenAPI

Swagger는 아래 기준으로 고정한다.

결정:

- `/swagger`에서 API 계약을 확인할 수 있어야 한다.
- `POST /api/employee`는 4개 Content-Type을 모두 request body에 노출한다.
- `multipart/form-data` request body는 `employeesFile` binary 필드를 사용한다.
- `text/plain`, `text/csv`, `application/json` 예시를 제공한다.
- `BulkCreateEmployeesResult`와 `ProblemDetails` 응답 스키마를 노출한다.

### 4.10 관측성

이번 API 구현에서 관측성은 최소 구성으로 고정한다.

결정:

- ASP.NET Core 요청 로깅을 켠다.
- OpenTelemetry는 ASP.NET Core, HttpClient, EF Core instrumentation 등록 경로를 둔다.
- metrics는 ASP.NET Core, runtime, process instrumentation 등록 경로를 둔다.
- exporter 연결은 필수가 아니며, exporter가 없어도 애플리케이션은 정상 기동해야 한다.

결정 이유:

- 운영 품질 요구를 반영하되, exporter 설정 미완료가 개발 흐름을 막지 않게 한다.

## 5. API 구조

최종 구조는 아래 수준으로 고정한다.

```text
src/EmployeeContacts.Api
├─ Controllers
│  └─ EmployeeController.cs
├─ OpenApi
│  └─ EmployeeImportOperationFilter.cs
├─ ProblemDetails
│  └─ GlobalExceptionHandler.cs
├─ Program.cs
├─ appsettings.json
└─ appsettings.Development.json
```

구조 원칙:

- HTTP endpoint는 `Controllers`에 둔다.
- Swagger 보강 코드는 `OpenApi` 아래에 둔다.
- 예외 매핑은 전역 처리기로 캡슐화한다.
- parser/repository 구현은 계속 `Infrastructure`에 둔다.

## 6. 테스트 구현 기준

API는 통합 테스트 중심으로 보호한다.

구현 원칙:

1. 실제 host를 `WebApplicationFactory`로 띄운다.
2. `Infrastructure`의 실제 SQLite provider와 마이그레이션을 사용한다.
3. 테스트 DB는 케이스별로 분리해 상태 누수를 막는다.
4. API 테스트는 HTTP 계약, 상태코드, body shape를 직접 검증한다.

### 6.1 필수 조회 테스트

#### `GET /api/employee`

- `정상 조회 시 페이지 결과를 반환한다.`
  테스트: `GetEmployees_ShouldReturnPagedEmployees()`
- `잘못된 page, pageSize는 ValidationProblemDetails를 반환한다.`
  테스트: `GetEmployees_ShouldReturnValidationProblem_WhenPagingIsInvalid()`
- `결과 정렬이 name, id 오름차순인지 검증한다.`
  테스트: `GetEmployees_ShouldReturnEmployees_OrderedByNameThenId()`

#### `GET /api/employee/{name}`

- `이름 검색은 trim 후 exact match로 동작한다.`
  테스트: `GetEmployeesByName_ShouldTrimNameBeforeSearching()`
- `검색 결과가 없으면 빈 배열을 반환한다.`
  테스트: `GetEmployeesByName_ShouldReturnEmptyArray_WhenNoEmployeeExists()`
- `공백 이름은 ValidationProblemDetails를 반환한다.`
  테스트: `GetEmployeesByName_ShouldReturnValidationProblem_WhenNameIsEmptyAfterTrim()`

### 6.2 필수 등록 테스트

- `multipart/form-data` CSV 업로드 성공
  테스트: `BulkCreateEmployees_ShouldCreateEmployees_FromMultipartCsv()`
- `text/csv` 등록 성공
  테스트: `BulkCreateEmployees_ShouldCreateEmployees_FromCsvBody()`
- `application/json` 배열 등록 성공
  테스트: `BulkCreateEmployees_ShouldCreateEmployees_FromJsonArray()`
- `text/plain` JSON 판별 성공
  테스트: `BulkCreateEmployees_ShouldPreferJsonParser_ForPlainTextJsonArray()`
- `text/plain` CSV fallback 성공
  테스트: `BulkCreateEmployees_ShouldFallbackToCsvParser_ForPlainTextCsv()`
- `부분 성공 시 201과 결과 집계를 반환한다.`
  테스트: `BulkCreateEmployees_ShouldReturnPartialSuccess_WhenSomeRowsFail()`
- `부분 성공 시 Location 헤더를 포함하지 않는다.`
  테스트: `BulkCreateEmployees_ShouldNotReturnLocationHeader_OnSuccess()`
- `헤더가 있는 CSV에서도 errors.row는 데이터 행 기준이다.`
  테스트: `BulkCreateEmployees_ShouldReturnRowNumbers_FromDataRows_WhenCsvHasHeader()`
- `지원하지 않는 Content-Type은 415를 반환한다.`
  테스트: `BulkCreateEmployees_ShouldReturnUnsupportedMediaType_WhenContentTypeIsNotSupported()`
- `잘못된 포맷은 400 ProblemDetails를 반환한다.`
  테스트: `BulkCreateEmployees_ShouldReturnProblemDetails_WhenFormatIsInvalid()`
- `한 건도 생성되지 않으면 400 ProblemDetails를 반환한다.`
  테스트: `BulkCreateEmployees_ShouldReturnProblemDetails_WhenNothingIsCreated()`
- `파일 파트 누락은 400 ProblemDetails를 반환한다.`
  테스트: `BulkCreateEmployees_ShouldReturnProblemDetails_WhenMultipartFileIsMissing()`

### 6.3 ProblemDetails 테스트

- `ValidationProblemDetails`에 입력 키와 오류 메시지가 포함된다.
  테스트: `ValidationProblem_ShouldContainErrorDictionary()`
- `ProblemDetails`에 traceId가 포함된다.
  테스트: `ProblemDetails_ShouldContainTraceId()`
- `예기치 않은 예외는 500 ProblemDetails로 변환된다.`
  테스트: `UnhandledException_ShouldReturnInternalServerErrorProblemDetails()`

## 7. 구현 순서

구현 순서는 아래로 확정한다.

### Phase 1. 프로젝트 준비

- `EmployeeContacts.Api`에 `Application`, `Infrastructure` 프로젝트 참조 추가
- Swagger/OpenTelemetry 패키지 추가
- `EmployeeContacts.Api.IntegrationTests`에 `Microsoft.AspNetCore.Mvc.Testing` 추가
- 테스트 프로젝트에 `Api`, `Infrastructure` 프로젝트 참조 추가

### Phase 2. 부트스트랩과 예외 처리

- `Program.cs`를 실제 API host 구성이 되도록 확장
- `AddProblemDetails()`와 전역 예외 처리기 구성
- 요청 로깅과 OpenTelemetry 최소 구성 추가
- Swagger UI 등록

### Phase 3. 조회 endpoint

- `EmployeeController` 생성
- `GET /api/employee` 구현
- `GET /api/employee/{name}` 구현
- 조회 통합 테스트 작성 및 녹색화

### Phase 4. 등록 endpoint

- `POST /api/employee` 구현
- Content-Type 분기와 raw body/file 읽기 구현
- parser 선택 로직 연결
- `Created > 0`, `Created == 0` 분기 구현
- 등록 통합 테스트 작성 및 녹색화

### Phase 5. Swagger와 마감

- `POST /api/employee` request body 문서 보강
- 응답 예시와 ProblemDetails 스키마 노출 확인
- 불필요한 중복 제거
- 문서와 실제 공개 계약 이름이 일치하는지 점검

## 8. 완료 기준

아래 조건을 모두 만족하면 API 구현을 완료로 본다.

- `GET /api/employee`, `GET /api/employee/{name}`, `POST /api/employee`가 요구사항대로 동작한다.
- API가 `AddApplication()`과 `AddInfrastructure()`만으로 조립된다.
- 잘못된 query parameter가 `ValidationProblemDetails`로 반환된다.
- 등록 요청의 요청 자체 오류가 `ProblemDetails`로 반환된다.
- 지원하지 않는 `Content-Type`이 `415 Unsupported Media Type`으로 반환된다.
- 부분 성공 등록이 `201 Created`와 계약된 결과 모델로 반환된다.
- `Created == 0` 결과가 `400 Bad Request` + `ProblemDetails`로 변환된다.
- Swagger에서 3개 API 계약과 등록 요청 형식을 확인할 수 있다.
- API 통합 테스트가 실제 host와 SQLite provider 기반으로 녹색이다.

## 9. 다음 단계 전달 기준

API 구현 완료 후 다음 작업은 아래 전제를 사용한다.

- 클라이언트나 프론트엔드는 `/api/employee` 계약만 알면 된다.
- API는 parser 선택과 ProblemDetails 변환을 캡슐화하므로 상위 소비자는 `Application` 세부를 직접 알 필요가 없다.
- 이후 확장 기능은 인증/인가, exporter 연결, Swagger 예제 고도화, 비동기 배치 처리 순서로 올린다.
