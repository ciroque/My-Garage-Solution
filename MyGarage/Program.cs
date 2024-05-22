using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyGarage;
using MyGarage.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Ensure the HttpClient is available for injection
builder.Services.AddScoped(sp => httpClient);

// Register the TheGarageClient as a service, so it can be injected into components
builder.Services.AddScoped<ITheGarageClient>(sp =>
{
    var theGarageHost = "the-garage-service";
    if (builder.HostEnvironment.IsDevelopment())
    {
        theGarageHost = "https://localhost:7213";
    }

    return new TheGarageClient(httpClient, theGarageHost);
});

await builder.Build().RunAsync();
