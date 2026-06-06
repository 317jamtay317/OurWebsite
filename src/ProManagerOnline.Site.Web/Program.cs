using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using ProManagerOnline.Site.Application;
using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Infrastructure;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;
using ProManagerOnline.Site.Web.Api;
using ProManagerOnline.Site.Web.Components;
using ProManagerOnline.Site.Web.Components.Account;
using ProManagerOnline.Site.Web.Media;
using ProManagerOnline.Site.Web.Rendering;
using ProManagerOnline.Site.Web.Startup;

var builder = WebApplication.CreateBuilder(args);

// Presentation: a Blazor Web App with Interactive Auto components, plus Razor Pages for the
// Identity account screens (sign in, password reset). The /Admin Razor Pages require a signed-in
// admin; the account pages are anonymous.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToFolder("/Admin/Account");
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Server-side implementation of the product admin API, used when Interactive Auto components
// render on the server and by the JSON endpoints the WebAssembly client calls.
builder.Services.AddScoped<IProductAdminApi, ServerProductAdminApi>();

// Documentation feature: Markdown rendering for the public reader, screenshot storage for the
// authoring admin, and the in-process gateway behind the Interactive Auto documentation admin
// (the browser side calls the JSON API instead).
builder.Services.AddSingleton<IMarkdownRenderer, MarkdigMarkdownRenderer>();
builder.Services.AddScoped<IDocMediaStorage, WwwrootDocMediaStorage>();
builder.Services.AddScoped<IDocsAdminApi, ServerDocsAdminApi>();

// About-page admin: the in-process gateway behind the Interactive Auto editor (the browser side
// calls the JSON API instead).
builder.Services.AddScoped<IAboutPageAdminApi, ServerAboutPageAdminApi>();

// Model Context Protocol server, exposing the site's tools over Streamable HTTP.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// ASP.NET Core Identity (cookie auth, password hashing, lockout, reset tokens). The EF stores live
// on SiteDbContext; the account/email services are registered by AddInfrastructure.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<SiteDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Flow the Identity auth state into the Blazor components. The persisting provider revalidates the
// security stamp on the server circuit AND persists the signed-in user to the page, so Interactive
// Auto admin components can authorize once they are running in WebAssembly.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

// Persist Data Protection keys to a durable location when one is configured (the cloud deployment
// mounts shared storage at this path). This keeps the keys that protect auth cookies and antiforgery
// tokens stable across container restarts and replicas. With no path configured the framework's
// per-instance, ephemeral key ring is used, which is fine for local development.
var keyRingPath = builder.Configuration["DataProtection:KeyRingPath"];
if (!string.IsNullOrWhiteSpace(keyRingPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
        .SetApplicationName("ProManagerOnline.Site");
}

// Behind the cloud ingress (Container Apps) TLS is terminated at the proxy and the app receives
// plain HTTP with the original scheme in X-Forwarded-Proto. Honour those headers so HTTPS
// redirection, HSTS and generated links use the real scheme instead of redirect-looping. Locally
// the headers are absent, so this is a harmless no-op.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Bring the database up to date and seed it on every start, in every environment, so a fresh
// deployment (local or cloud) is usable on first boot with no manual database step: migrations are
// applied, the catalogue is seeded, and the owner admin is created from configuration. Each step is
// idempotent.
await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration);

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();       // serves runtime-uploaded documentation screenshots from wwwroot/docs-media

app.UseAuthentication();
app.UseAuthorization();

// Gate the Blazor admin (/admin/*) on an authenticated admin. The account pages stay anonymous,
// and the public site and JSON API are handled elsewhere. The initial (server) request to an
// admin page is redirected to the sign-in page when there is no cookie.
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isAdmin = path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase)
        && !path.StartsWithSegments("/admin/account", StringComparison.OrdinalIgnoreCase);

    if (isAdmin && context.User.Identity?.IsAuthenticated != true)
    {
        var returnUrl = Uri.EscapeDataString(path + context.Request.QueryString);
        context.Response.Redirect($"/Admin/Account/Login?returnUrl={returnUrl}");
        return;
    }

    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ProManagerOnline.Site.Web.Client._Imports).Assembly);
app.MapRazorPages();

// JSON APIs the WebAssembly admin clients call (each requires an authenticated admin internally).
app.MapProductAdminApi();
app.MapDocsAdminApi();
app.MapAboutPageAdminApi();

// Model Context Protocol endpoint (Streamable HTTP) for MCP clients.
app.MapMcp("/mcp");

app.Run();
