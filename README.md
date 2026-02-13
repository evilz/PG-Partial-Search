# PG-Partial-Search

A .NET Aspire application demonstrating fast partial search on PostgreSQL using pg_trgm (Trigram) extension, providing Elastic-like search behavior.

## 🚀 Features

- **Fast Partial Search**: Search by fragments, prefixes, or any part of first/last names
- **PostgreSQL with pg_trgm**: Uses trigram indexes (GIN) for high-performance fuzzy matching
- **Auto-Seeded Database**: Automatically generates 10,000 random people on startup using Bogus
- **Modern Web UI**: Clean, responsive interface with real-time search results
- **Paginated Results**: Efficient pagination for large result sets
- **Minimal API**: Simple RESTful endpoint (`/api/search`) for easy integration
- **.NET Aspire Orchestration**: Manages PostgreSQL container, health checks, and observability

## 🏗️ Architecture

### Components

1. **PGPartialSearch.AppHost**: Aspire orchestration host that manages PostgreSQL container and API service
2. **PGPartialSearch.ApiService**: ASP.NET Core API with EF Core, minimal API endpoints, and static web UI
3. **PGPartialSearch.ServiceDefaults**: Shared Aspire service configuration

### Database Schema

```sql
CREATE TABLE "People" (
    "Id" SERIAL PRIMARY KEY,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(200) NOT NULL,
    "Phone" VARCHAR(50) NOT NULL,
    "City" VARCHAR(100) NOT NULL
);

-- GIN indexes for trigram search
CREATE INDEX "IX_People_FirstName_gin" ON "People" USING gin ("FirstName" gin_trgm_ops);
CREATE INDEX "IX_People_LastName_gin" ON "People" USING gin ("LastName" gin_trgm_ops);
```

## 🛠️ Technologies

- **.NET 10** / **C# 13**
- **ASP.NET Core** (Minimal APIs)
- **Entity Framework Core 10**
- **PostgreSQL 17** with **pg_trgm extension**
- **.NET Aspire 13.1** for orchestration
- **Bogus** for fake data generation
- **Docker** for containerization

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## 🚀 Quick Start

### Option 1: Using .NET Aspire (Recommended)

```bash
cd PGPartialSearch.AppHost
dotnet run
```

> **Note**: Requires Aspire DCP (Developer Control Panel). If not available, use Option 2.

### Option 2: Manual Setup (Development)

```bash
# 1. Start PostgreSQL container
docker run -d --name pg-search \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=searchdb \
  -p 5432:5432 \
  postgres:17-alpine

# 2. Run the API service
cd PGPartialSearch.ApiService
dotnet run

# 3. Open browser to http://localhost:5000
```

## 🔍 Usage

### Web UI

Navigate to `http://localhost:5000`:
- Enter any part of a first or last name (e.g., "john", "son", "smith")
- Results are paginated (20 per page)

### API Endpoint

**GET** `/api/search?query={term}&page={page}&pageSize={size}`

Example:
```bash
curl "http://localhost:5000/api/search?query=john&page=1&pageSize=5"
```

Response:
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 5,
  "totalCount": 75,
  "totalPages": 15
}
```

## 📊 Search Examples

- `"john"` → 75 results (John, Johnathan, Johnson, Johns)
- `"son"` → 486 results (Anderson, Johnson, Wilson, Addison)
- Partial matching works on both first and last names

## 📸 Screenshots

### Initial Interface
![Initial UI](https://github.com/user-attachments/assets/84ab0efc-6235-4ff9-861a-b11b2214148b)

### Search Results - "john"
![Search for john](https://github.com/user-attachments/assets/995f68df-cca7-45e7-b3e3-4d0b667bcd77)

### Partial Search - "son"
![Search for son](https://github.com/user-attachments/assets/6561fcbb-8e74-4d2a-bb6c-323965083bc1)

## 📄 License

MIT License - see [LICENSE](LICENSE)
