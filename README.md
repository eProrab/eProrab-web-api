# eProrab

Trilingual (Azerbaijani, English, Russian) construction marketplace backend — ASP.NET 8 minimal APIs, Clean Architecture.

## How to Run

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (local or Docker)
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Steps

```bash
# 1. Clone
git clone https://github.com/eProrab/eProrab-web-api.git
cd eProrab-web-api

# 2. Configure local settings
cp src/eProrab.API/appsettings.Development.json.example src/eProrab.API/appsettings.Development.json
# fill in your Postgres connection string + JWT secret

# 3. Restore packages
dotnet restore

# 4. Apply database migrations
dotnet ef database update --project src/eProrab.Infrastructure --startup-project src/eProrab.API

# 5. Run
dotnet run --project src/eProrab.API
```

Swagger UI: `https://localhost:{port}/swagger`
