# ==========================================================================
# ProManager Online — .NET 10 Blazor Web App (Interactive Auto) + SQL Server.
# Multi-stage: build the Tailwind CSS, publish the app (incl. the WASM client),
# then run on the ASP.NET runtime image (listens on port 8080).
# ==========================================================================

# ── Stage 1: build the Tailwind CSS bundle ────────────────────────────────
FROM node:20-alpine AS css
WORKDIR /src/src/ProManagerOnline.Site.Web
COPY src/ProManagerOnline.Site.Web/package*.json ./
RUN npm install
# Tailwind scans .razor in both the Web and Web.Client projects (see app.css @source).
COPY src/ProManagerOnline.Site.Web/ ./
COPY src/ProManagerOnline.Site.Web.Client/ /src/src/ProManagerOnline.Site.Web.Client/
RUN npm run build:css

# ── Stage 2: restore, build and publish the .NET app ──────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
# Use the CSS produced by the css stage (wwwroot is otherwise build output).
COPY --from=css /src/src/ProManagerOnline.Site.Web/wwwroot/css/app.css src/ProManagerOnline.Site.Web/wwwroot/css/app.css
RUN dotnet publish src/ProManagerOnline.Site.Web/ProManagerOnline.Site.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# ── Stage 3: runtime image ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProManagerOnline.Site.Web.dll"]
