using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Application;
using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Infrastructure;
using ProManagerOnline.Site.Infrastructure.Persistence;
using ProManagerOnline.Site.Web.Api;
using ProManagerOnline.Site.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Presentation: a Blazor Web App with Interactive Auto components. Razor Pages stays registered
// so the parallel documentation pages keep working alongside Blazor.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddRazorPages();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("SiteDatabase")!);

// Server-side implementation of the admin API, used when Interactive Auto components render on
// the server and by the JSON endpoints below.
builder.Services.AddScoped<IProductAdminApi, ServerProductAdminApi>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    // In development, bring the database up to date and seed the initial catalogue.
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<SiteDbContext>();
    await database.Database.MigrateAsync();
    await SiteDbSeeder.SeedAsync(database);
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ProManagerOnline.Site.Web.Client._Imports).Assembly);
app.MapRazorPages();
app.MapProductAdminApi();

app.Run();
