using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Application;
using ProManagerOnline.Site.Application.Documentation;
using ProManagerOnline.Site.Infrastructure;
using ProManagerOnline.Site.Infrastructure.Persistence;
using ProManagerOnline.Site.Web.Admin;
using ProManagerOnline.Site.Web.Components;
using ProManagerOnline.Site.Web.Media;
using ProManagerOnline.Site.Web.Rendering;
using ProManagerOnline.Site.Web.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Presentation: the public site is server-rendered Razor Pages; the documentation admin is Blazor
// with the Interactive Auto render mode (server first, then WebAssembly once it is cached).
builder.Services.AddRazorPages();
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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // In development, bring the database up to date and seed the initial catalogue.
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<SiteDbContext>();
    await database.Database.MigrateAsync();
    await SiteDbSeeder.SeedAsync(database);
}

app.UseHttpsRedirection();
app.UseStaticFiles();       // serves runtime-uploaded screenshots from wwwroot/docs-media
app.UseRouting();
app.UseAntiforgery();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// The documentation admin (Blazor routes + the JSON API it calls) has no authentication yet, so it
// is mapped only in Development — real auth is a flagged follow-up before exposing it in production.
if (app.Environment.IsDevelopment())
{
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(IDocsAdminApi).Assembly);

    app.MapAdminDocsApi();
}

app.Run();
