using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Web.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// When the admin runs in the browser it reaches the host's JSON API over HTTP, on the same origin
// the app was served from; the browser sends the Identity auth cookie automatically. Both the
// products admin and the documentation admin resolve their gateway to the HTTP implementation here.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IProductAdminApi, HttpClientProductAdminApi>();
builder.Services.AddScoped<IDocsAdminApi, HttpClientDocsAdminApi>();
builder.Services.AddScoped<IAboutPageAdminApi, HttpClientAboutPageAdminApi>();

await builder.Build().RunAsync();
