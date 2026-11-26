# 🛣️ # OnTheRoadDetection: Backend API

The backend for a road-maintenance platform. A municipality uses it to see road damage on a map, assign repairs to officers and repair teams, and track each pothole until it is fixed. An AI detection client streams new potholes to the API over gRPC.

This was a team project. I built the backend; teammates built the AI detection model and the hardware. A React dashboard (separate repository) uses this API.

## Features

- **Pothole map data:** potholes are stored with their GPS location, severity, status and photo. You can filter them by district, neighbourhood, status, severity, or a rectangle on the map.
- **Detection ingestion over gRPC:** the AI client streams detections. The API queues them and saves them in the background, so the stream is never blocked by the database.
- **Districts and neighbourhoods:** real district and neighbourhood boundaries (GeoJSON) are stored as spatial shapes.
- **Repair workflow:** job tasks link a pothole, an officer and a repair team. Officers also have scan tasks.
- **Dashboard:** per-neighbourhood pothole counts and severity for the logged-in officer, cached and refreshed every 5 minutes.
- **Auth:** ASP.NET Core Identity with JWT access tokens and refresh tokens (HttpOnly cookie, 7 days). Roles include `Admin` and `Officer`.
- **Photos:** uploaded to Cloudinary.
- **Logging:** Serilog to the console, and to [Seq](https://datalust.co/seq) if it is running.

## Tech stack

| Area | Technology |
|---|---|
| Runtime | .NET 8, ASP.NET Core Web API |
| Database | SQL Server, Entity Framework Core 8.0.22 (24 migrations) |
| Spatial | NetTopologySuite (SQL Server `geography` columns, GeoJSON) |
| Streaming | gRPC (Grpc.AspNetCore 2.76.0), `System.Threading.Channels` |
| Auth | ASP.NET Core Identity 8.0.22, JWT Bearer 8.0.22 |
| Storage | Cloudinary (CloudinaryDotNet 1.27.9) |
| Logging | Serilog 8.0.3, Seq sink |
| Tests | xUnit 2.9.3, Moq 4.20.72, FluentAssertions 8.10.0, AutoFixture 4.18.1, EF Core InMemory |
| Container | Docker (multi-stage Dockerfile) |

## Project structure

```
OnTheRoadDetection-BackEnd/
├── PotholeApi/
│   ├── Controllers/        # REST endpoints
│   ├── GrpcServices/       # DetectionImplementationService (gRPC)
│   ├── Protos/             # DetectionService.proto
│   ├── Services/           # Detection queue + worker, dashboards, photos, role handlers
│   ├── Repositories/       # Data access (interfaces in IRepositories/)
│   ├── Models/ DTO/        # Entities and request/response shapes
│   ├── Helpers/            # DistrictSeeder, token helpers, settings
│   ├── Data/               # District/neighbourhood GeoJSON files
│   ├── Migrations/         # EF Core migrations
│   └── Dockerfile
└── PotholeApi.Tests/       # Unit tests
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express, Developer or LocalDB), or SQL Server in Docker
- The EF Core CLI: `dotnet tool install --global dotnet-ef`
- A free [Cloudinary](https://cloudinary.com/) account, for photo uploads
- *Optional:* Docker, to run the API in a container
- *Optional:* Seq, to browse structured logs at `http://localhost:5341`

## Getting started

1. **Clone the repository**

   ```bash
   git clone https://github.com/MTM2025MT/OnTheRoadDetection-BackEnd.git
   cd OnTheRoadDetection-BackEnd
   ```

2. **Set the connection string.** `PotholeApi/appsettings.json` points to a local server named `MO_TALAL`. Change `ConnectionStrings:DefaultConnection` to your own server.

3. **Set the secrets** (see [Configuration](#configuration)):

   ```bash
   cd PotholeApi
   dotnet user-secrets set "JWT:Secret" "<a random string of at least 32 characters>"
   dotnet user-secrets set "CloudinarySettings:CloudName" "<your cloud name>"
   dotnet user-secrets set "CloudinarySettings:ApiKey" "<your api key>"
   dotnet user-secrets set "CloudinarySettings:ApiSecret" "<your api secret>"
   ```

4. **Create the database**

   ```bash
   dotnet ef database update
   ```

5. **Load the districts and neighbourhoods (first run only).** In `Program.cs`, uncomment the block that calls `DistrictSeeder.Seed(context, logger, app.Environment.ContentRootPath)`, run the app once, then comment it out again. It reads `Data/export2.geojson`.

6. **Run the API**

   ```bash
   dotnet run --launch-profile https
   ```

   Swagger UI opens at **https://localhost:7099/swagger**.

## Configuration

Secrets are never stored in `appsettings.json`. Use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) locally, or environment variables elsewhere (double underscore instead of colon, for example `JWT__Secret`).

| Setting | Required | Where | Description |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | Yes | `appsettings.json` | SQL Server connection string |
| `JWT:Secret` | Yes | user secrets | Key that signs the JWTs (32+ characters) |
| `CloudinarySettings:CloudName` | Yes | user secrets | Cloudinary cloud name |
| `CloudinarySettings:ApiKey` | Yes | user secrets | Cloudinary API key |
| `CloudinarySettings:ApiSecret` | Yes | user secrets | Cloudinary API secret |
| `Serilog:WriteTo:1:Args:serverUrl` | No | `appsettings.json` | Seq URL (default `http://localhost:5341`) |

CORS allows the React dev server at `http://localhost:5173`.

## Using the API

1. Register with `POST /api/Account/Register`. New users get the `Officer` role.
2. Log in with `POST /api/Account/Login` and copy the access token. The refresh token is set as an HttpOnly cookie; call `GET /api/Account/Refresh-Token` to get a new access token.
3. Send the access token as `Authorization: Bearer <token>`.

### Endpoints

| Area | Endpoints |
|---|---|
| Account | `POST /api/Account/Register` · `POST /api/Account/Login` · `GET /api/Account/Refresh-Token` · `GET /api/Account/GetUserById` · `PUT /api/Account/UpdateUser` · `PUT /api/Account/ChangePassword` |
| Potholes (`Admin`, `Officer`) | `GET /api/Pothole/GetAll` · `GET /api/Pothole/{id}` · `GET /api/Pothole/bounds` · `GET /api/Pothole/Filter?status=&severity=` · `GET /api/Pothole/GetByDistrictId/{districtId}` · `GET /api/Pothole/GetByNeighbourhoodId/{neighbourhoodId}` · `POST /api/Pothole` · `POST /api/Pothole/CreateWithFotoAsUrl` · `PUT /api/Pothole/{id}` · `PATCH /api/Pothole/{id}/Status` · `DELETE /api/Pothole/{id}` · `GET /api/Pothole/DashboardPotholeData` |
| Job tasks | `GET /api/JobTask` · `GET /api/JobTask/{id}` · `POST /api/JobTask` · `PUT /api/JobTask/{id}` · `DELETE /api/JobTask/{id}` |
| Officers | `GET /api/Officer` · `GET /api/Officer/{id}` · `GET /api/Officer/{id}/jobtasks` · `POST /api/Officer` · `PUT /api/Officer/{id}` · `DELETE /api/Officer/{id}` · `POST /api/Officer/CreateScanTask` · `GET /api/Officer/GetScanTasks` |
| Districts | `GET /api/District` · `GET /api/District/{id}` · `GET /api/District/GetAllWithName` · `GET /api/Neighbourhood/{id}` · `GET /api/Neighbourhood/GetNeighbourhoodByDistrictId/{id}` |
| Admin | `POST /api/Admin/CreateRole` · `GET /api/Admin/GetAllRoles` · `DELETE /api/Admin/DeleteRole` · `POST /api/Admin/AssignRole` · `GET /api/Admin/GetAllUser` |

`GET /api/Pothole/bounds` takes the two corners of the visible map: `first_latitude`, `first_longitude`, `second_latitude`, `second_longitude`.

### gRPC detection stream

Contract: [`PotholeApi/Protos/DetectionService.proto`](PotholeApi/Protos/DetectionService.proto)

```proto
service DetectionService {
  rpc AddData (stream PotholeRequest) returns (StreamSummary);
}
```

The client opens one stream, sends a `PotholeRequest` for each detection, and receives a `StreamSummary` when it closes the stream. gRPC needs HTTP/2, so connect to the HTTPS address: `https://localhost:7099`.

<!--
## Screenshots

![Swagger UI](docs/swagger.png)
![React dashboard map](docs/dashboard-map.png)
-->

## Run with Docker

Build from the repository root:

```bash
docker build -f PotholeApi/Dockerfile -t potholeapi .
```

Run it. The container listens on port 8080. Inside a container, Windows authentication does not work, so use a SQL login, and reach SQL Server on your machine through `host.docker.internal`:

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=PotholeDb;User Id=<user>;Password=<password>;TrustServerCertificate=True" \
  -e JWT__Secret="<32+ characters>" \
  -e CloudinarySettings__CloudName="<cloud name>" \
  -e CloudinarySettings__ApiKey="<api key>" \
  -e CloudinarySettings__ApiSecret="<api secret>" \
  potholeapi
```

Swagger is then at http://localhost:8080/swagger.

## Tests

25 unit tests cover the repositories (potholes, officers, repair teams, districts and neighbourhoods). They use EF Core InMemory, so no database is needed.

```bash
dotnet test
```

## Design notes

- **Detections never wait for the database.** The gRPC service writes each detection into a bounded `Channel` (capacity 10,000). `DetectionDatabaseWorker`, a `BackgroundService`, reads from it, uploads the photo, sets the severity level, and saves potholes in batches of 100. When the queue is full, the stream slows down instead of using more and more memory.
- **Spatial data in the database.** Pothole locations are `geography` points (SRID 4326), and districts and neighbourhoods are polygons. Filtering by area runs as a SQL Server spatial query.
- **Cached dashboard.** `PotholeDashboardsService` caches each officer's dashboard in `IMemoryCache` for 5 minutes, and `DashboardCacheRefreshWorker` refreshes it every 5 minutes. Measured locally, the endpoint went from about 13.5 ms to about 4.5 ms.
- **Role handlers (Strategy pattern).** Assigning a role is delegated to an `IRoleHandler` per role (`AdminHandler`, `ManagerHandler`, `OfficerHandler`) inside one database transaction. A new role means a new class, not another `if` branch.
