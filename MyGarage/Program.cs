using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyGarage;
using MyGarage.Services;


var appConfiguration = new AppConfiguration();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(appConfiguration);

// Ensure the HttpClient is available for injection
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the TheGarageClient as a service, so it can be injected into components
builder.Services.AddScoped<ITheGarageClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new TheGarageClient(httpClient, appConfiguration);
});

await builder.Build().RunAsync();
