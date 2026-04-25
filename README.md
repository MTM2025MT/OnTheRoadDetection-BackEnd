# 🛣️ OnTheRoadDetection - Backend API

> A high-throughput backend service designed to process spatial data, manage automated repair dispatch workflows, and ingest real-time pothole detections.

This repository contains the core backend infrastructure for the OnTheRoadDetection platform. Implemented as a decoupled service, it utilizes REST and gRPC endpoints to serve the frontend client while supporting asynchronous, high-volume data ingestion from AI/hardware overlays via a background processing pipeline.

## 🛠️ Technology Stack
* **Framework:** ASP.NET Core Web API (.NET 8), ASP.NET Core Identity, gRPC
* **Language:** C# 12
* **Database:** SQL Server via Entity Framework Core (with Spatial Support)
* **Core Libraries / Infrastructure:** * `NetTopologySuite` (GIS/spatial geometry)
  * `System.Threading.Channels` (bounded async queues)
  * JWT Bearer Authentication
  * Cloudinary integration (Cloud image storage)

## ⚙️ Core System Mechanics & Architecture

Instead of a standard CRUD application, this system was architected to handle complex spatial querying and high-traffic bursts from automated detection hardware:

* **High-Throughput Asynchronous Ingestion:** * A bounded `Channel<DetectionQueueItem>` decouples the raw ingestion of pothole data from database persistence.
  * A background worker (`DetectionDatabaseWorker`) reads queued detections, processes media uploads, and persists data in optimized batches. This minimizes database round-trips, increases write efficiency during traffic bursts, and provides backpressure protection for memory stability.
* **Geospatial Domain Modeling (GIS):** * Pothole locations are stored as spatial `Point` geometries (SRID 4326), and city district boundaries utilize `MultiPolygon` shapes. 
  * The system actively parses and fixes GeoJSON geometries to enable highly optimized map-bounds filtering and neighborhood-level analytics.
* **Clean Architecture & SOLID Principles:**
  * **Repository Pattern:** Controllers depend strictly on abstractions (e.g., `IPotholeRepository`, `IJobTaskRepository`), isolating persistence logic from API orchestration.
  * **Dependency Inversion:** Services and repositories are resolved via constructor injection, enforcing loose coupling between the HTTP layer and infrastructure.
  * **Strategy Pattern:** Extensible role management delegates specific behaviors to interchangeable `IRoleHandler` implementations (Admin, Manager, Officer), reducing conditional complexity and adhering to the Open/Closed Principle.
* **Security & Authentication:** * Full JWT Bearer authentication is integrated with EF Identity stores. Protected endpoints dynamically derive user identity and map data access strictly to the authenticated officer's context.

## 💻 Running Locally

1. Clone the repository:
   ```bash
   git clone [https://github.com/MTM2025MT/OnTheRoadDetection-BackEnd.git](https://github.com/MTM2025MT/OnTheRoadDetection-BackEnd.git)
