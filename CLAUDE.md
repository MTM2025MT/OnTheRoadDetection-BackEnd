# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

PotholeApi is an ASP.NET Core 8 Web API backing a pothole-reporting/road-maintenance system. It exposes a REST API (Controllers) for a React frontend (CORS is locked to `http://localhost:5173`), plus a gRPC endpoint that ingests pothole detections from an external AI/detection client. Data is stored in SQL Server with spatial columns (`geography`) via NetTopologySuite, and ASP.NET Identity + JWT bearer tokens handle auth with role-based authorization (`Admin`, `Officer`, etc.).

## Commands

Run from the repo root (`PotholeApi.sln`).

```bash
# Build
dotnet build PotholeApi.sln

# Run the API (from PotholeApi/)
dotnet run --project PotholeApi/PotholeApi.csproj

# Run all tests
dotnet test PotholeApi.Tests/PotholeApi.Tests.csproj

# Run a single test class or method (standard VSTest filter syntax)
dotnet test PotholeApi.Tests/PotholeApi.Tests.csproj --filter "FullyQualifiedName~PotholeRepositoryTests"
dotnet test PotholeApi.Tests/PotholeApi.Tests.csproj --filter "FullyQualifiedName~PotholeRepositoryTests.CreatePotholeAsync_SetsNeighbourhoodId_WhenPointInsideNeighbourhood"

# EF Core migrations (run from PotholeApi/ so the DbContext/startup project resolve)
dotnet ef migrations add <Name> --project PotholeApi/PotholeApi.csproj
dotnet ef database update --project PotholeApi/PotholeApi.csproj
```

Tests use xUnit + `Microsoft.Testing.Platform` (see `PotholeApi.Tests/global.json`), EF Core's InMemory provider, Moq/Moq.EntityFrameworkCore, AutoFixture(+AutoMoq), and FluentAssertions.

## Architecture

### Solution layout
- `PotholeApi/` — the Web API project (target `net8.0`).
- `PotholeApi.Tests/` — xUnit test project, references `PotholeApi` directly and exercises repositories against an EF Core InMemory `Context`.

### Layering — mid-refactor, read before assuming a layer is wired up
Most controllers today talk **directly to repositories** (`IPotholeRepository`, `IOfficerRepository`, etc.) or even the `Context` (`DbContext`) itself — see `PotholeController`, `OfficerController`, `AccountController`. A parallel set of `Service`/`IService` classes (`AccountService`, `AdminService`, `DistrictService`, `JobTaskService`, `NeighbourhoodService`, `OfficerService`, `PotholeService`, `WeatherForecastService`, etc. under `PotholeApi/Services/`) was recently added to introduce a proper service layer, but as of now **most of them are not registered in `Program.cs`'s DI container and are not consumed by any controller yet**. Only `IPotholeDashboardsService`/`PotholeDashboardsService` (dashboard aggregation) and the `IRoleHandler` implementations are actually wired in. When adding features, check `Program.cs` to see what's actually active before assuming a `*Service` class is on the call path — don't assume the new service layer is live without verifying the DI registration and controller usage.

### Detection ingestion pipeline (gRPC → queue → background worker)
1. `GrpcServices/DetectionImplementationService` (`Protos/DetectionService.proto`) receives a client-streamed `AddData` RPC of pothole detections (location, confidence, image bytes, bounding boxes).
2. Each detection is pushed onto `DetectionQueue`, an in-memory `Channel`-backed singleton (registered via `AddSingleton<DetectionQueue>()`), so the RPC returns immediately without touching the DB.
3. `DetectionDatabaseWorker` (a `BackgroundService` registered via `AddHostedService`) drains the queue, uploads each image through `IPhotoService` (Cloudinary), maps severity from a numeric confidence into `Enums.Level` (`>=7` High, `>=4` Medium, else Low), and batches inserts into `Potholes` in groups of 100.

This is the pattern to follow for any other high-throughput ingestion path: don't write to the DB on the request thread, queue and let a hosted service batch it.

### Role assignment — strategy pattern via discriminator
`IRoleHandler` implementations (`OfficerHandler`, `AdminHandler`, `ManagerHandler` in `Services/RoleHandlers/`) are all registered as `IRoleHandler` in DI. Each has a `RoleName` used as a discriminator against `BaseRoleAssignmentDto` to pick the right handler for assigning a role-specific record (Officer, etc.) when a user is granted a role. `RepairTeamHandler` is referenced in a comment in `Program.cs` but not implemented/registered — don't assume repair-team role assignment works end-to-end.

### Data model and spatial data
- `Context` (`PotholeApi/Context.cs`) is an `IdentityDbContext<ApplicationUser>` — Identity tables plus `Potholes`, `Officers`, `JobTasks`, `RepairTeams`, `Streets`, `Districts`, `Neighbourhoods`, `ScanExecutionTasks`, `RoutePoints`, `GpsCoordinates`.
- `Pothole.Location`, `District.Geometry`, `Neighbourhood.Geometry` are mapped to SQL Server `geography` columns via NetTopologySuite (`UseNetTopologySuite()` on the SQL Server provider, `GeoJsonConverterFactory` for JSON (de)serialization in `Program.cs`). When constructing points, NTS uses `(X=longitude, Y=latitude)` ordering — see the tests and `DetectionDatabaseWorker` for the convention.
- `JobTask` links an `Officer`, a `RepairTeam`, and (1:1) a `Pothole`, all with `DeleteBehavior.Restrict`.
- Migrations live in `PotholeApi/Migrations/`; regenerate via `dotnet ef` as above, don't hand-edit generated migration files.

### Auth
JWT bearer auth (`Microsoft.AspNetCore.Authentication.JwtBearer`) with a fixed issuer (`PotholeServer`) and audience (`PotholeReactApp`) validated in `Program.cs`. `AccountController` issues tokens; most other controllers gate access with `[Authorize(Roles = "...")]`.

## Known repo state to be aware of

- **`PotholeApi/appsettings.json` currently has real, non-placeholder secrets committed** (SQL Server connection string with a password, Cloudinary API secret, JWT signing secret). Be careful not to further leak these (e.g. don't echo them into commit messages, logs, or generated docs) and flag to the user if you notice new secrets being committed — this should eventually move to user secrets / environment variables / a secret store.
- There's a stray `proto PotholeApi/Protos/DetectionService.proto` directory at the repo root (space in the name) that duplicates `PotholeApi/Protos/DetectionService.proto` — it isn't referenced by any `.csproj`; treat the one under `PotholeApi/Protos/` as canonical.
- Large chunks of `Program.cs` (identity/district seeding calls) are commented out — seeding is not currently run on startup even though `Data/IdentitySeed.cs` and `Helpers/DistrictSeeder.cs` exist.
