using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ProManagerOnline.Site.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// When the admin runs in the browser it reaches the server's JSON API over HTTP, on the same origin
// the app was served from; the browser sends the Identity auth cookie automatically.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IDocsAdminApi, HttpDocsAdminApi>();

// Authorization in WebAssembly: rebuild the authentication state the server persisted into the page.
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

await builder.Build().RunAsync();
