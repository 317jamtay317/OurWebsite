using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProManagerOnline.Site.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// When the admin runs in the browser it reaches the server's JSON API over HTTP, on the same
// origin the app was served from.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IDocsAdminApi, HttpDocsAdminApi>();

await builder.Build().RunAsync();
