using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Application;
using ProManagerOnline.Site.Infrastructure;
using ProManagerOnline.Site.Infrastructure.Identity;
using ProManagerOnline.Site.Infrastructure.Persistence;
using ProManagerOnline.Site.Web.Components;
using ProManagerOnline.Site.Web.Components.Account;

var builder = WebApplication.CreateBuilder(args);

// Presentation + application + infrastructure services.
builder.Services.AddRazorPages(options =>
{
    // The whole /Admin area requires a signed-in admin, except the account pages
    // (login, forgot/reset password and the lockout/access-denied notices).
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToFolder("/Admin/Account");
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("SiteDatabase")!);

// ASP.NET Core Identity (cookie auth, password hashing, lockout, reset tokens). The EF stores
// live on SiteDbContext; the account/email services are registered by AddInfrastructure.
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

// Blazor (interactive server) powers the admin CMS, sharing the Identity cookie auth.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // In development, bring the database up to date, seed the catalogue, and ensure the
    // owner admin account exists (credentials come from configuration / user-secrets).
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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Required by Blazor's interactive components (form posts / SignalR handshake).
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// The Blazor admin lives under /admin and requires an authenticated admin (unauthenticated
// requests are redirected to the Razor Pages sign-in via the application cookie's LoginPath).
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .RequireAuthorization();

app.Run();
