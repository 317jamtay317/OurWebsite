using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProManagerOnline.Site.Contracts;
using ProManagerOnline.Site.Web.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Talk to the host's JSON API for product administration when running in WebAssembly.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IProductAdminApi, HttpClientProductAdminApi>();

await builder.Build().RunAsync();
