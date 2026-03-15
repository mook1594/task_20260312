# Employee Contacts API

직원 긴급 연락망 시스템. 직원 정보 조회 및 CSV/JSON 기반 일괄 등록 API.

## 기술 스택

- **Runtime**: .NET 10
- **Database**: SQLite (EF Core)
- **Architecture**: Clean Architecture + CQRS + MediatR
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI
- **Observability**: OpenTelemetry, Structured Logging

## 주요 기능

- **조회 API**
  - `GET /api/employee?page={page}&pageSize={pageSize}` — 페이지 단위 직원 목록 조회
  - `GET /api/employee/{name}` — 이름으로 직원 검색

- **등록 API**
  - `POST /api/employee` — CSV/JSON 형식 일괄 등록 (multipart/form-data, text/csv, application/json, text/plain 지원)

## 빠른 시작

```bash
# 프로젝트 복원 및 빌드
dotnet restore EmployeeContacts.slnx
dotnet build EmployeeContacts.slnx

# 테스트 실행
dotnet test EmployeeContacts.slnx

# API 실행
dotnet run --project src/EmployeeContacts.Api
```

API는 `https://localhost:7000`에서 실행되며, Swagger UI는 `/swagger` 경로에서 확인 가능합니다.

## 데이터 규칙

- **이메일**: trim 후 소문자 정규화, 시스템 전체 유일
- **전화번호**: 하이픈 입력 허용, 숫자만 저장, `010` 시작 11자리만 허용, 시스템 전체 유일
- **이름**: 중복 허용
- **날짜**: `yyyy-MM-dd` 형식만 허용

## 아키텍처

```
Domain (비즈니스 규칙, 프레임워크 독립)
  ↑
Application (CQRS 유스케이스, 검증)
  ↑
Infrastructure + Api (영속성, HTTP)
```

- **Domain**: Employee Aggregate, 도메인 값 객체, 예외 정의
- **Application**: Command/Query/Handler, Validator, DTO
- **Infrastructure**: EF Core, SQLite, CSV/JSON Parser
- **Api**: ASP.NET Core Controller, ProblemDetails 응답 매핑

## 개발

```bash
# 특정 테스트 실행
dotnet test --filter "FullyQualifiedName~EmployeeContacts.Domain.Tests.Employees"

# 코드 커버리지 수집
dotnet test EmployeeContacts.slnx --collect:"XPlat Code Coverage"
```

더 자세한 내용은 `docs/` 폴더를 참고하세요.
