# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Quick Start

**Build, restore, and test:**
```bash
dotnet restore EmployeeContacts.slnx
dotnet build EmployeeContacts.slnx
dotnet test EmployeeContacts.slnx
```

**Run the API locally:**
```bash
dotnet run --project src/EmployeeContacts.Api
```

**Run a single test file or method:**
```bash
dotnet test --filter "FullyQualifiedName~EmployeeContacts.Domain.Tests.Employees.EmployeeTests"
```

**Run tests with code coverage:**
```bash
dotnet test EmployeeContacts.slnx --collect:"XPlat Code Coverage"
```

## Architecture Overview

This is a clean-architecture .NET 10 application with **strict layer boundaries**:

```
Domain (no dependencies)
  ↑
Application (depends on Domain)
  ↑
Infrastructure + Api (both depend on Application and Domain)
```

- **Domain** (`src/EmployeeContacts.Domain`): Business rules, `Employee` aggregate, value objects, domain errors via `DomainException` and `EmployeeDomainErrors`. Framework-free.
- **Application** (`src/EmployeeContacts.Application`): CQRS commands/queries, validators (FluentValidation), DTOs, pipeline behaviors, persistence/parser abstractions, DI registration via `AddApplication()`.
- **Infrastructure** (`src/EmployeeContacts.Infrastructure`): EF Core, SQLite persistence, repository implementations, bulk-import parsers (placeholder stage).
- **Api** (`src/EmployeeContacts.Api`): ASP.NET Core host, controllers, OpenAPI, global exception handling, ProblemDetails mapping, content-type branching.

**Test mirrors:** `tests/EmployeeContacts.Domain.Tests`, `tests/EmployeeContacts.Application.Tests`, `tests/EmployeeContacts.Infrastructure.Tests`, `tests/EmployeeContacts.Api.IntegrationTests`.

## Key Implementation Details

### Naming & Model Contracts
- **PhoneNumber** (internal domain model) ↔ **Tel** (external API/DTO contract).
- `Employee.Create(...)` is the aggregate factory where invariants are enforced.
- Bulk create processes **at most one error per row** and uses **normalized email/phone for duplicate checks**.
- CQRS use cases: `GetEmployeesQuery`, `GetEmployeesByNameQuery`, `BulkCreateEmployeesCommand`.
- Public DTOs: `EmployeeDto`, `PagedResult<T>`, `BulkEmployeeRecord`, `BulkCreateEmployeesResult`, `BulkCreateEmployeesError`.
- Repositories: `IEmployeeRepository`, `IUnitOfWork`.
- Parsers: `IEmployeeImportParser`, `IPlainTextEmployeeImportDetector`.

### Validation & Error Handling
- Domain validation raises `DomainException` with `EmployeeDomainErrors` enum values.
- Application layer uses FluentValidation for command/query validators.
- Api layer maps validation errors and domain exceptions to ProblemDetails (RFC 9457).
- Global exception handler in `Program.cs` via `AddExceptionHandler<GlobalExceptionHandler>()`.

### Testing
- **Framework**: xUnit v3 with `Microsoft.NET.Test.Sdk` and `coverlet.collector`.
- **Approach**: TDD (test first). Use English test method names; use Korean `[Trait("DisplayName", "...")]` for intent/description.
- **Doubles**: Prefer manual test doubles (no mocking framework) in Application tests.
- **Guidance**: See `docs/4. tdd-and-delivery-guide.md` for the full TDD flow.

### Current Implementation Status
- ✅ **Domain**: Complete for Employee aggregate scope.
- ✅ **Application**: CQRS handlers, validators, behaviors, DTOs, DI wired.
- 🔨 **Infrastructure**: Mostly template; needs EF Core/SQLite persistence and parser implementations.
- 🔨 **Api**: Template setup; needs endpoint controllers and integration tests.

## Code Style & Conventions

- **Indentation**: 4 spaces; CRLF line endings.
- **Naming**: `PascalCase` for public types/members, `camelCase` for locals/parameters, one file per class.
- **Nullable**: Strict mode enabled (`<Nullable>enable</Nullable>`).
- **Reflection of Current Code**: Do not refactor existing validation patterns or exception styles unless explicitly required. Changes to core contracts (IEmployeeRepository, etc.) need explicit justification.

## Documentation & Decision Basis

Read these in order for implementation context:
1. `docs/1. requirements.md` — Feature requirements.
2. `docs/3. clean-architecture.md` — Layer boundaries and references.
3. `docs/4. tdd-and-delivery-guide.md` — TDD workflow and testing strategy.
4. `docs/implementation-plan/` — Checklists and implementation tasks.

When docs conflict with current code, **prefer current code**. Update checklists to match the snapshot.

## Commit & PR Guidance

- Use [Conventional Commits](https://www.conventionalcommits.org/): `feat`, `fix`, `test`, `docs`, `chore`, etc.
- Keep summary short and specific.
- Include test evidence and sample request/response for API changes.
- Link to requirements or issues where relevant.
