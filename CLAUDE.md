# ProManager Online — project guide for Claude

A .NET 10 Clean Architecture web app (Domain → Application → Infrastructure → Web) with a
Blazor front end, EF Core + SQL Server, and Tailwind CSS. Solution: `ProManagerOnline.Site.slnx`.

## Running the app

- **When the user says "run it" (or "run the app", "start it", etc.), it means run the ENTIRE
  STACK IN DOCKER** — i.e. `docker compose up --build` (web app + SQL Server), not `dotnet run`.
  Use the local `dotnet run` / preview server only for quick checks when explicitly asked.
