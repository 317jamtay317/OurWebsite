# ProManager Online

The ProManager Online website and admin: a **.NET 10 Clean Architecture web app** with a
Blazor front end, EF Core + SQL Server, ASP.NET Core Identity for the admin, and Tailwind CSS.

The site presents the business — subscription software products (**Workflows.AI** and
**Air Compliance Record Keeping**) plus custom development — and hosts an authenticated admin for
managing the product catalogue, pricing, and per-product documentation. A small Model Context
Protocol (MCP) endpoint is exposed for MCP clients.

> This replaces an earlier hand-written static HTML site. The whole site is now rendered by the
> .NET app; there is no separate static-site build.

---

## Architecture

Clean Architecture — dependencies point inward toward the domain:

```
Domain  ←  Application  ←  Infrastructure
   ↑            ↑                ↑
   └──────── Web / Web.Client (presentation) ┘
                  ↑
              Contracts (shared admin transport types)
```

| Project | Responsibility |
|---|---|
| `ProManagerOnline.Site.Domain` | Aggregates, value objects (`Money`, `Slug`, `Currency`), repository interfaces. No dependencies. |
| `ProManagerOnline.Site.Application` | Use-case handlers (one per file), DTOs for server-rendered reads. |
| `ProManagerOnline.Site.Contracts` | Transport types + typed errors for the admin HTTP API, shared by server and WebAssembly. |
| `ProManagerOnline.Site.Infrastructure` | EF Core (SQL Server), repositories, Identity account service, email, reCAPTCHA. |
| `ProManagerOnline.Site.Web` | Blazor Web App host (Interactive Auto), public pages, admin JSON API, Identity Razor Pages, MCP. |
| `ProManagerOnline.Site.Web.Client` | Blazor WebAssembly admin components and the HTTP clients they use. |

Solution file: `ProManagerOnline.Site.slnx`. Tests live under `tests/` (one project per layer).

### Conventions

- **Public pages** are server-rendered and consume **Application DTOs** in process.
- **Admin features** run as Interactive Auto components against an `I*AdminApi` abstraction with two
  implementations — a server one (calls handlers in process) and a WebAssembly one (calls the JSON
  API over HTTP). The shared transport types and a typed `*AdminException` (with an error enum) live
  in the `Contracts` project, so both execution contexts and both directions map errors uniformly.
- Repositories follow a **unit-of-work** model: `AddAsync`/`RemoveAsync` stage changes and the
  handler commits them with `SaveChangesAsync`.

---

## Run it with Docker (the whole stack)

Runs the web app **and** SQL Server together. From this folder:

```bash
docker compose up --build        # build + start  → http://localhost:8080
docker compose up -d --build     # same, detached
docker compose down              # stop and remove
docker compose down -v           # also drop the database volume
```

In Development the database is migrated and seeded on first start, and an admin account is created
from configuration:

```
email:    admin@promanageronline.com
password: Admin123!
```

---

## Run locally without Docker (quick checks)

Requires the .NET 10 SDK, a reachable SQL Server, and Node (for the Tailwind build).

```bash
# 1. Build the Tailwind CSS bundle
cd src/ProManagerOnline.Site.Web
npm install
npm run build:css

# 2. Configure the connection string + seed admin (user-secrets recommended)
dotnet user-secrets set "ConnectionStrings:SiteDatabase" "Server=...;Database=ProManagerOnlineSite;..."
dotnet user-secrets set "Admin:Email" "admin@promanageronline.com"
dotnet user-secrets set "Admin:InitialPassword" "Admin123!"

# 3. Run the host (applies migrations + seeds in Development)
dotnet run
```

---

## Tests

```bash
dotnet test                      # all layers
```

---

## Before going live

A few real-world details still need filling in (owner legal name, business location, governing-law
jurisdiction, a domain support mailbox, confirmed prices, and the contact-form provider keys). These
are configuration/content rather than code.

> **Legal note:** the Terms, Privacy, and Refunds pages are general templates, **not legal advice**.
> Have them reviewed for your jurisdiction before relying on them.
