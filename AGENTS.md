# Repository Guidelines

## Project Structure & Module Organization
The solution is split by clean-architecture layers under `src/` and mirrored test projects under `tests/`.

- `src/EmployeeContacts.Api`: ASP.NET Core entry point, DI, OpenAPI, controllers/endpoints.
- `src/EmployeeContacts.Application`: CQRS use cases, validators, DTOs, abstractions.
- `src/EmployeeContacts.Domain`: core business rules and entities with no outgoing project references.
- `src/EmployeeContacts.Infrastructure`: persistence, parser implementations, and external integrations.
- `tests/EmployeeContacts.Domain.Tests`, `tests/EmployeeContacts.Application.Tests`, `tests/EmployeeContacts.Api.IntegrationTests`: unit and integration coverage by layer.
- `docs/`: requirements, architecture, and TDD guidance. Read this first when adding features.

## Build, Test, and Development Commands
- `dotnet restore EmployeeContacts.slnx`: restore solution dependencies.
- `dotnet build EmployeeContacts.slnx`: compile all projects.
- `dotnet run --project src/EmployeeContacts.Api`: start the API locally.
- `dotnet test EmployeeContacts.slnx`: run all xUnit test projects.
- `dotnet test --collect:"XPlat Code Coverage"`: run tests with Coverlet collection enabled.

## Coding Style & Naming Conventions
Use standard C# conventions: 4-space indentation, `PascalCase` for public types/members, `camelCase` for locals/parameters, and one class per file named after the type. Keep nullable reference types enabled and prefer explicit layer boundaries: `Domain` stays framework-free, `Application` depends only on `Domain`, and infrastructure details stay out of handlers. Organize features by use case, for example `Employees/Commands/BulkCreateEmployees`.

## Testing Guidelines
This repository uses xUnit v3 with `Microsoft.NET.Test.Sdk` and `coverlet.collector`. Follow the TDD flow documented in `docs/4. tdd-and-delivery-guide.md`: write failing tests first, then implement the minimum code to pass. Name test files after the subject under test, and write test methods to describe behavior clearly, such as `Create_ShouldNormalizeEmail()`. Add domain tests for rules, application tests for handlers/validators, and integration tests for API contracts and ProblemDetails responses.

## Commit & Pull Request Guidelines
Recent history uses Conventional Commits such as `docs: ...` and `chore: ...`. Continue with prefixes like `feat`, `fix`, `test`, `docs`, and `chore`, keeping the summary short and specific. Pull requests should include a concise description, linked issue or requirement, test evidence (`dotnet test` output), and sample request/response details when API behavior changes. Include screenshots only when Swagger or other UI-visible output changes.

## Architecture Notes
Target runtime is `.NET 10` and the planned development database is SQLite. Preserve the reference direction from `docs/3. clean-architecture.md`: `Api -> Application`, `Api -> Infrastructure`, `Infrastructure -> Application`, `Infrastructure -> Domain`, `Application -> Domain`, and `Domain -> (none)`.
