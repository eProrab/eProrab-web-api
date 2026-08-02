# eProrab API

Trilingual (🇦🇿 Azerbaijani / 🇬🇧 English / 🇷🇺 Russian) backend for **eProrab** — a
construction marketplace: a materials/tools catalog, a role-based admin panel,
and a hiring system with a **worker cabinet** for tradespeople to find jobs.

Built with **ASP.NET Core 8 minimal APIs** in a Clean Architecture layout, PostgreSQL
via EF Core, JWT auth, and ASP.NET Core Identity for role-based access control.

> **Built without a .NET toolchain available to generate/verify it.** Every file was
> hand-written against .NET 8 / EF Core 8 conventions, but it has **not** been
> `dotnet restore`/`build`'d in this environment. Do that first thing after
> downloading — see "First run" below — and expect to fix the odd typo.

---

## Architecture

```
eProrab.Domain           → entities, enums, constants. Zero framework dependencies.
eProrab.Application      → DTOs, service interfaces + implementations, FluentValidation
                            validators. References EF Core only for IQueryable extensions
                            (Include/ToListAsync) — the pragmatic Jason-Taylor-template
                            approach, not a hard EF dependency in the business logic.
eProrab.Infrastructure   → EF Core DbContext, ASP.NET Core Identity, JWT issuance,
                            repositories, database seeding.
eProrab.API              → composition root. Minimal API endpoint groups, JWT bearer
                            auth, Swagger, global exception → ProblemDetails mapping.
```

Dependency direction is strictly inward: API → Infrastructure → Application → Domain.
`ApplicationUser`/`ApplicationRole` (ASP.NET Core Identity types) live in
**Infrastructure**, not Domain — Application only ever sees users through
`UserSummary`/`UserDto`, so swapping auth providers later wouldn't touch business logic.

## Roles

| Role | Can do |
|---|---|
| **Admin** | Everything: catalog CRUD, user CRUD, verify workers, moderate any job posting |
| **Manager** | Foreman/"prorab" account — can post jobs to hire workers |
| **Client** | Customer account — can post jobs to hire workers |
| **Worker** | Has a **worker cabinet**: trade profile, browse/apply to jobs, track applications |

New self-registrations (`POST /api/auth/register`) always land as **Client**.
Create Manager/Worker/Admin accounts via `POST /api/admin/users` (Admin only).

## Localization strategy

- **Admin-curated content** (categories, items, specializations) is *fully* trilingual:
  every create/update requires exactly one translation for `az`, `en` and `ru`
  (enforced by `TranslationRuleExtensions.MustCoverAllLanguages` in every validator).
  Public read endpoints resolve to the caller's language automatically.
- **Language resolution** for reads: `?lang=az|en|ru` query param → `Accept-Language`
  header → default `az`. See `ILanguageProvider`.
- **System messages** (errors like "invalid credentials") come from a small static
  az/en/ru dictionary (`Application/Localization/Messages.cs`) — no .resx/satellite
  assemblies needed.
- **User-generated content** (job postings, worker bios) is stored as written, tagged
  with the `Language` the author used, and is *not* machine-translated.

## First run

### Option A — Docker (fastest)

```bash
docker compose up --build
```

This starts PostgreSQL + the API. On first boot the API applies EF Core migrations
and seeds roles, a default Admin account, and a starter trilingual catalog.
Swagger UI: `http://localhost:8080/swagger` isn't exposed in Production mode by
default — set `ASPNETCORE_ENVIRONMENT=Development` in `docker-compose.yml` if you
want it there, or just run locally with `dotnet run` for Swagger.

### Option B — Local dotnet + Postgres

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Start Postgres (Docker one-liner): 
   `docker run -d --name eprorab-pg -e POSTGRES_DB=eprorab_dev -e POSTGRES_USER=eprorab -e POSTGRES_PASSWORD=eprorab_dev_password -p 5432:5432 postgres:16-alpine`
3. Restore & build:
   ```bash
   dotnet restore
   dotnet build
   ```
4. **Set a real JWT signing key** (don't ship the appsettings placeholder):
   ```bash
   cd src/eProrab.API
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)"
   ```
5. Create the initial migration (none is checked in — generate it once EF Core
   tools are available):
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate --project src/eProrab.Infrastructure --startup-project src/eProrab.API
   ```
6. Run:
   ```bash
   dotnet run --project src/eProrab.API
   ```
   Migrations apply and the database seeds automatically on startup
   (`ApplicationDbSeeder.SeedAsync`). Swagger: `http://localhost:5080/swagger`.

### Default seeded admin

```
email:    admin@eprorab.az   (override via SeedAdmin:Email)
password: ChangeMe123!       (override via SeedAdmin:Password)
```
**Change this password immediately** outside local development — it's logged as a
warning on every startup precisely so it isn't forgotten.

## Endpoint reference

All `/api/admin/*` routes require the `Admin` role. Trailing `?lang=az|en|ru`
works on every read endpoint.

**Auth** (`/api/auth`)
`POST /register` · `POST /login` · `POST /refresh` · `POST /logout` · `GET /me` 🔒

**Catalog — public** 
`GET /api/categories` · `GET /api/items` · `GET /api/items/{id}` · `GET /api/specializations`

**Catalog — admin CRUD** (`/api/admin/categories`, `/api/admin/items`, `/api/admin/specializations`) 
Full `GET (paged)` / `GET {id}` / `POST` / `PUT {id}` / `DELETE {id}` on each.

**Users — admin CRUD** (`/api/admin/users`) 
`GET` (paged, `?role=`) · `GET {id}` · `POST` · `PUT {id}` · `PATCH {id}/role` ·
`PATCH {id}/active` · `POST {id}/reset-password` · `DELETE {id}`
(Guards against deleting/deactivating/demoting the last remaining Admin.)

**Jobs — public browse** 
`GET /api/jobs` (open only) · `GET /api/jobs/{id}`

**Jobs — employer side** 🔒 *Client/Manager/Admin* 
`POST /api/jobs` · `GET /api/jobs/mine` · `PUT /api/jobs/{id}` · `DELETE /api/jobs/{id}` ·
`GET /api/jobs/{id}/applicants` · `POST /api/jobs/{id}/applicants/{appId}/accept` ·
`POST /api/jobs/{id}/applicants/{appId}/reject`
(Owner-or-Admin enforced in the service layer; accepting one applicant
auto-declines the rest and closes the posting.)

**Workers — public browse** 
`GET /api/workers` · `GET /api/workers/{id}`

**Worker cabinet** 🔒 *Worker role* (`/api/worker`) 
`GET /profile` · `POST /profile` · `PUT /profile` ·
`POST /jobs/{jobId}/apply` · `GET /applications` · `DELETE /applications/{id}`

**Hiring — admin moderation** 
`GET /api/admin/workers` (paged) · `PATCH /api/admin/workers/{id}/verify?isVerified=true` ·
`DELETE /api/admin/workers/{id}` · `GET /api/admin/jobs` (paged, any status)

## Notes & next steps

- **No EF Core migration is checked in** — this sandbox has no .NET SDK/nuget.org
  access to generate one. Run the `dotnet ef migrations add InitialCreate` command
  above once; after that, commit the generated `Migrations/` folder normally.
- **CORS** defaults wide-open (`AllowAnyOrigin`) when `Cors:AllowedOrigins` is empty,
  so the API is usable immediately — lock this down (`appsettings.Production.json`
  → `Cors:AllowedOrigins: ["https://your-frontend.com"]`) before going live.
- **Soft delete** is global (`BaseEntity.IsDeleted` + an EF Core query filter applied
  by reflection in `ApplicationDbContext`), except for `ApplicationUser`, which
  Identity doesn't support soft-deleting — admin "delete user" is a real delete;
  prefer `PATCH .../active` to deactivate instead.
- **Refresh tokens** rotate on every use and are stored server-side
  (`RefreshTokens` table) so `POST /api/auth/logout` can actually revoke them.
