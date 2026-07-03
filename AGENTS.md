# AGENTS.md

## Project Overview

PG-Partial-Search is a .NET Aspire sample that demonstrates fast partial-name search against PostgreSQL using the `pg_trgm` trigram extension. It exposes an ASP.NET Core Minimal API (`/api/search`) and serves a small static web UI from `PGPartialSearch.ApiService/wwwroot`.

### Repository layout

- `PGPartialSearch.sln` — solution file for all projects.
- `PGPartialSearch.AppHost/` — .NET Aspire orchestration host. It starts PostgreSQL 17, pgAdmin, and the API service.
- `PGPartialSearch.ApiService/` — ASP.NET Core API, EF Core data model, database seeding, search endpoint, and static frontend assets.
- `PGPartialSearch.ApiService/Data/` — EF Core `SearchDbContext`, `Person` model, trigram search query builder, and database seeder.
- `PGPartialSearch.ApiService/wwwroot/` — static HTML/CSS/JavaScript UI served by the API project.
- `PGPartialSearch.ServiceDefaults/` — shared Aspire defaults for health checks, service discovery, resilience, and OpenTelemetry.
- `PGPartialSearch.ApiService.Tests/` — xUnit tests for query generation and related API-service behavior.
- `pg-schema.md`, `IDEA.md`, `FUNCTIONALITY_ROADMAP.md`, and `IMPLEMENTATION_SUMMARY.md` — design and implementation notes.

### Key technologies

- .NET 10 / C# 13 with nullable reference types and implicit usings enabled.
- ASP.NET Core Minimal APIs.
- Entity Framework Core with Npgsql/PostgreSQL.
- PostgreSQL 17 with `pg_trgm` and GIN trigram indexes.
- .NET Aspire AppHost and service defaults.
- xUnit for tests.
- Bogus and EFCore.BulkExtensions for seed-data generation.

## Setup Commands

Prerequisites:

- Install the .NET 10 SDK.
- Install Docker Desktop or another Docker-compatible engine.
- Ensure Docker is running before starting the Aspire AppHost or manual PostgreSQL container.

Common setup commands from the repository root:

```bash
dotnet restore PGPartialSearch.sln
```

Run with Aspire orchestration (preferred):

```bash
dotnet run --project PGPartialSearch.AppHost/PGPartialSearch.AppHost.csproj
```

Manual local database setup, useful when Aspire DCP is unavailable:

```bash
docker run -d --name pg-search \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=searchdb \
  -p 5432:5432 \
  postgres:17-alpine
```

Then run the API service:

```bash
dotnet run --project PGPartialSearch.ApiService/PGPartialSearch.ApiService.csproj
```

The API service expects a PostgreSQL connection named `searchdb` when running under Aspire. For manual runs, provide an equivalent connection string via user secrets, environment variables, or `appsettings.Development.json` as appropriate for your local environment.

## Development Workflow

- Prefer working from the repository root so solution-level commands include all projects.
- Start the full development environment with `dotnet run --project PGPartialSearch.AppHost/PGPartialSearch.AppHost.csproj`; the AppHost provisions PostgreSQL 17 and wires the API service to the `searchdb` database.
- The API maps OpenAPI only in Development and serves the static frontend via default files/static files.
- On API startup, `DbSeeder.SeedAsync` creates the database if needed, enables `pg_trgm`, creates trigram GIN indexes, and seeds data if the `People` table is empty.
- Be aware that the current seeder generates a large dataset. Changes that cause reseeding can take noticeable time and require enough local database/storage capacity.
- Use `dotnet watch --project PGPartialSearch.ApiService/PGPartialSearch.ApiService.csproj` for quick API/frontend iteration when a PostgreSQL instance is already available.
- Keep Aspire orchestration concerns in `PGPartialSearch.AppHost`; keep reusable service defaults in `PGPartialSearch.ServiceDefaults`.

## Testing Instructions

Run all tests:

```bash
dotnet test PGPartialSearch.sln
```

Run only the test project:

```bash
dotnet test PGPartialSearch.ApiService.Tests/PGPartialSearch.ApiService.Tests.csproj
```

Run a specific test by name:

```bash
dotnet test PGPartialSearch.ApiService.Tests/PGPartialSearch.ApiService.Tests.csproj --filter "FullyQualifiedName~SearchQueryTests.Apply_UsesTrigramSimilarityPredicate"
```

Build without running tests:

```bash
dotnet build PGPartialSearch.sln --no-restore
```

Test guidance:

- Add or update tests in `PGPartialSearch.ApiService.Tests` for behavior changes in the API service or query layer.
- Current query tests inspect generated SQL with `ToQueryString()` and do not require a live PostgreSQL connection for execution, but package restore/build still require a working .NET SDK.
- For database behavior changes, prefer tests that validate EF query generation separately from integration tests that require PostgreSQL.
- Before committing code changes, run `dotnet test PGPartialSearch.sln` whenever the environment has the .NET SDK available.

## Code Style Guidelines

### C# conventions

- Use file-scoped namespaces, as in the existing projects.
- Keep nullable reference types enabled and model required data with `required` properties where appropriate.
- Prefer async EF Core APIs (`CountAsync`, `ToListAsync`, `ExecuteSqlRawAsync`, etc.) in request and database paths.
- Keep Minimal API endpoint definitions concise; move reusable query/database logic into `PGPartialSearch.ApiService/Data` or another dedicated type rather than expanding `Program.cs` heavily.
- Preserve provider-translatable LINQ in `SearchQuery`; do not introduce client-side filtering or sorting into search queries.
- Use `EF.Functions.TrigramsAreSimilar` and `EF.Functions.TrigramsSimilarity` for trigram search unless intentionally changing the search strategy.
- Do not replace trigram predicates with `ILIKE` for the main search path unless there is a documented performance reason and matching tests are updated.
- Use structured logging with message templates instead of string interpolation in log messages.

### Frontend/static assets

- Keep the static UI dependency-free unless there is a clear project need to add a frontend toolchain.
- Place browser code in PGPartialSearch.ApiService/wwwroot/app.js, styles in PGPartialSearch.ApiService/wwwroot/styles.css, and markup in PGPartialSearch.ApiService/wwwroot/index.html.
- If you make a visible UI change, capture or update screenshots when the environment supports running the app.

### Formatting

- Match the existing C# formatting style: 4-space indentation, braces on new lines, and clear blank lines between logical sections.
- Run `dotnet format PGPartialSearch.sln` before committing formatting-heavy changes when the SDK/tooling is available.

## Build and Deployment

Build the full solution:

```bash
dotnet build PGPartialSearch.sln
```

Publish the API service:

```bash
dotnet publish PGPartialSearch.ApiService/PGPartialSearch.ApiService.csproj -c Release
```

Operational notes:

- The API requires PostgreSQL with the `pg_trgm` extension available. The seeder executes `CREATE EXTENSION IF NOT EXISTS pg_trgm;` and creates GIN indexes.
- In non-Aspire deployments, configure the `searchdb` connection string for the API service using standard ASP.NET Core configuration providers.
- Health endpoints from `MapDefaultEndpoints` are only mapped in Development by default. Review `PGPartialSearch.ServiceDefaults/Extensions.cs` before exposing health checks differently in production.
- The static UI is published with the API service from `wwwroot`.

## Security Considerations

- Do not commit secrets, passwords, connection strings for shared environments, `.env` files, or user-secrets data.
- The README's local Docker password is for disposable development only.
- Validate and clamp externally supplied parameters. The search endpoint currently clamps `pageSize` to `1..100`; preserve similar bounds for new query parameters.
- Use EF Core parameterization for database input. Do not build SQL with string concatenation from user input.
- Review health-check exposure before enabling endpoints outside Development.

## Pull Request Guidelines

- Keep PRs focused and describe user-visible behavior, database/query implications, and test coverage.
- Before opening a PR, run the most relevant checks, typically:

```bash
dotnet format PGPartialSearch.sln --verify-no-changes
dotnet test PGPartialSearch.sln
```

- If a command cannot run because local prerequisites are missing (for example, no .NET SDK or Docker daemon), mention that explicitly in the PR notes.
- Update `README.md` or the design notes when commands, prerequisites, architecture, or user-facing behavior changes.
- Include screenshots for perceptible web UI changes when practical.

## Debugging and Troubleshooting

- If Aspire startup fails, verify Docker is running and that the .NET Aspire workload/tooling required by the installed SDK is available.
- If manual API startup cannot connect to PostgreSQL, confirm the `pg-search` container is running and port `5432` is not already in use.
- If search returns unexpectedly few results, confirm `pg_trgm` is enabled and the GIN indexes exist on `People.FirstName` and `People.LastName`.
- If startup is slow on a fresh database, check seeding progress in logs; initial seed generation and bulk insert can be expensive.
- Use the generated SQL from `SearchQuery.Apply(...).ToQueryString()` when debugging provider translation or search predicate changes.
