using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Application;
using ProManagerOnline.Site.Infrastructure;
using ProManagerOnline.Site.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Presentation + application + infrastructure services.
builder.Services.AddRazorPages();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("SiteDatabase")!);

// Model Context Protocol server, exposing the site's tools over Streamable HTTP.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

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
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Model Context Protocol endpoint (Streamable HTTP) for MCP clients.
app.MapMcp("/mcp");

app.Run();
