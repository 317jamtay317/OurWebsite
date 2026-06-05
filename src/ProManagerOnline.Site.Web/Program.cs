using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Application;
using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Infrastructure;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;
using ProManagerOnline.Site.Web.Admin;
using ProManagerOnline.Site.Web.Components;
using ProManagerOnline.Site.Web.Components.Account;
using ProManagerOnline.Site.Web.Media;
using ProManagerOnline.Site.Web.Rendering;
using ProManagerOnline.Site.Web.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Presentation: the public marketing site and the admin account (sign-in) pages are Razor Pages;
// the admin CMS is Blazor. The whole /Admin Razor Pages folder requires a signed-in admin, except
// the account pages (login, forgot/reset password and the lockout/access-denied notices).
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToFolder("/Admin/Account");
});

// The products admin renders Interactive Server; the documentation admin renders Interactive Auto
// (server first, then WebAssembly once cached), so both component runtimes are registered.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSingleton<IMarkdownRenderer, MarkdigMarkdownRenderer>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("SiteDatabase")!);

// The documentation admin's screenshot storage and its in-process data gateway (the server side of
// the Interactive Auto components; the browser side calls the JSON API instead).
builder.Services.AddScoped<IDocMediaStorage, WwwrootDocMediaStorage>();
builder.Services.AddScoped<IDocsAdminApi, ServerDocsAdminApi>();

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
// security stamp on the server circuit AND persists the signed-in user to the page, so the
// Interactive Auto documentation admin can authorize once it is running in WebAssembly.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // In development, bring the database up to date, seed the catalogue, and ensure the owner admin
    // account exists (credentials come from configuration / user-secrets).
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var database = services.GetRequiredService<SiteDbContext>();
    await database.Database.MigrateAsync();
    await SiteDbSeeder.SeedAsync(database);

    var adminEmail = app.Configuration["Admin:Email"];
    var adminPassword = app.Configuration["Admin:InitialPassword"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await AdminSeeder.SeedAsync(userManager, adminEmail, adminPassword);
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();       // serves runtime-uploaded screenshots from wwwroot/docs-media
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Required by Blazor's interactive components (form posts / SignalR handshake).
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Model Context Protocol endpoint (Streamable HTTP) for MCP clients.
app.MapMcp("/mcp");

// The Blazor admin lives under /admin and requires an authenticated admin (unauthenticated requests
// are redirected to the Razor Pages sign-in via the application cookie's LoginPath). The
// documentation admin's components live in the Web.Client assembly and reach data through the JSON
// API, which is likewise authorized.
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddInteractiveWebAssemblyRenderMode()
   .AddAdditionalAssemblies(typeof(IDocsAdminApi).Assembly)
   .RequireAuthorization();

app.MapAdminDocsApi();

app.Run();
