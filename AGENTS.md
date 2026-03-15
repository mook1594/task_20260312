# Repository Guidelines

## Read This First
This repository already has implementation-specific guidance under `docs/`. Before changing code, read the relevant documents in this order:

1. `docs/1. requirements.md`
2. `docs/3. clean-architecture.md`
3. `docs/4. tdd-and-delivery-guide.md`
4. `docs/implementation-plan/domain-implementation-plan.md`
5. `docs/implementation-plan/application-implementation-plan.md`
6. The matching checklist in `docs/implementation-plan/`

When documents conflict with the current codebase, prefer the current code and the latest checklist entries that were updated to match the implementation snapshot.

## Current Implementation Status
The repository is not at the same maturity level in every layer.

- `src/EmployeeContacts.Domain` is implemented for the current `Employee` aggregate scope.
- `src/EmployeeContacts.Application` is implemented for the current CQRS use cases, validators, behaviors, DTOs, and DI registration.
- `src/EmployeeContacts.Api` is still close to the default ASP.NET Core template. `Program.cs` currently wires controllers and OpenAPI only.
- `src/EmployeeContacts.Infrastructure` currently contains only the project file and has no concrete persistence or parser implementation yet.
- `tests/EmployeeContacts.Domain.Tests` and `tests/EmployeeContacts.Application.Tests` contain real automated coverage.
- `tests/EmployeeContacts.Api.IntegrationTests` currently contains only the test project and still needs actual integration tests.

## Project Structure & Module Organization
The solution follows clean-architecture layering under `src/` and mirrored test projects under `tests/`.

- `src/EmployeeContacts.Api`: ASP.NET Core host, HTTP endpoints, content-type branching, ProblemDetails mapping, OpenAPI.
- `src/EmployeeContacts.Application`: CQRS requests/handlers, validators, DTOs, pipeline behaviors, persistence/parsing abstractions, `AddApplication()`.
- `src/EmployeeContacts.Domain`: business rules, value objects, `Employee`, and domain error/exception primitives with no outgoing project references.
- `src/EmployeeContacts.Infrastructure`: future EF Core, SQLite, repository implementations, parser implementations, and external integrations.
- `tests/EmployeeContacts.Domain.Tests`: domain unit tests.
- `tests/EmployeeContacts.Application.Tests`: application unit tests built with manual test doubles.
- `tests/EmployeeContacts.Api.IntegrationTests`: reserved for API integration coverage once endpoints are implemented.
- `docs/`: requirements, architecture, TDD guide, and implementation plans/checklists.

## Build, Test, and Development Commands
- `dotnet restore EmployeeContacts.slnx`: restore solution dependencies.
- `dotnet build EmployeeContacts.slnx`: compile all projects.
- `dotnet run --project src/EmployeeContacts.Api`: start the API locally.
- `dotnet test EmployeeContacts.slnx`: run all test projects.
- `dotnet test --collect:"XPlat Code Coverage"`: run tests with Coverlet collection enabled.

## Coding Style & Naming Conventions
Use standard C# conventions: 4-space indentation, `PascalCase` for public types/members, `camelCase` for locals/parameters, and one class per file named after the type. Keep nullable reference types enabled and preserve strict layer boundaries.

- `Domain` stays framework-free and must not reference other projects.
- `Application` may reference `Domain` only; do not reimplement domain normalization or validation rules in handlers or validators.
- `Infrastructure` may depend on `Application` and `Domain`.
- `Api` may depend on `Application` and `Infrastructure`.
- Organize features by use case, for example `Employees/Commands/BulkCreateEmployees`.
- Internal model naming uses `PhoneNumber`; external DTO/API contracts use `Tel`.
- Application registration must continue to flow through `DependencyInjection.AddApplication()`.

Reflect the current code, not earlier drafts:

- Domain validation currently uses `DomainException` plus `EmployeeDomainErrors`; do not introduce a parallel exception style unless the repository is intentionally being refactored.
- `Employee.Create(...)` is the aggregate factory and remains the place where aggregate-level invariants are enforced.
- Bulk create processing records at most one error per row and uses normalized email/phone values for duplicate checks.
- Query ordering guarantees belong to the repository contract.

## Testing Guidelines
This repository uses xUnit v3 with `Microsoft.NET.Test.Sdk` and `coverlet.collector`. Follow the TDD flow in `docs/4. tdd-and-delivery-guide.md`: write failing tests first, implement the minimum code to pass, then refactor.

- Keep test folder structure aligned with the implementation structure.
- Name test files after the subject under test.
- Use English test method names and Korean `DisplayName` text for intent, matching the existing suite style.
- Prefer xUnit `Assert` to match the current test suite. Do not introduce `FluentAssertions` unless the repository is intentionally standardizing on it.
- In `tests/EmployeeContacts.Application.Tests`, prefer the existing manual test double pattern over adding a mocking framework unless there is a clear need.
- Add domain tests for value objects and aggregate rules, application tests for handlers/validators/behaviors, and integration tests for API contracts and ProblemDetails responses.
- If you implement `Api` or `Infrastructure` features, add or expand `tests/EmployeeContacts.Api.IntegrationTests`; that project is currently missing real test cases.

## Implementation Priorities
The current codebase indicates this sequence for upcoming work:

1. Implement `Infrastructure` for persistence and import parsing behind the existing application abstractions.
2. Connect `Api` to `Application` and `Infrastructure` through `AddApplication()` and future infrastructure DI.
3. Add HTTP endpoints for employee queries and bulk create import.
4. Implement Content-Type handling, parser resolution, and ProblemDetails/validation error mapping.
5. Back the API with integration tests once the host behavior exists.

When adding features, preserve the already-implemented `Domain` and `Application` contracts unless the change explicitly requires revisiting them.

## Commit & Pull Request Guidelines
Use Conventional Commits such as `feat`, `fix`, `test`, `docs`, and `chore`, keeping the summary short and specific. Pull requests should include a concise description, linked issue or requirement, test evidence, and sample request/response details when API behavior changes. Include screenshots only when Swagger or other UI-visible output changes.

## Architecture Notes
Target runtime is `.NET 10` and the planned development database is SQLite. Preserve the reference direction from `docs/3. clean-architecture.md`:

- `Api -> Application`
- `Api -> Infrastructure`
- `Infrastructure -> Application`
- `Infrastructure -> Domain`
- `Application -> Domain`
- `Domain -> (none)`

Keep these public contracts stable unless the task explicitly changes them:

- `DependencyInjection.AddApplication(IServiceCollection)`
- `IEmployeeRepository`
- `IUnitOfWork`
- `IEmployeeImportParser`
- `IPlainTextEmployeeImportDetector`
- `GetEmployeesQuery`
- `GetEmployeesByNameQuery`
- `BulkCreateEmployeesCommand`
- `EmployeeDto`
- `PagedResult<T>`
- `BulkEmployeeRecord`
- `BulkCreateEmployeesResult`
- `BulkCreateEmployeesError`
