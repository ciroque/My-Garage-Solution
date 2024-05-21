using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyGarage;
using MyGarage.Services;
using TheGarage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    var mockConfigBuilder = new ConfigurationBuilder();
    mockConfigBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
        { AppConfiguration.Keys.TheGarageUrlKey, Environment.GetEnvironmentVariable(AppConfiguration.Keys.TheGarageUrlKey) ?? AppConfiguration.Defaults.TheGarageUrl },
    });
}
else
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        var settings = builder.Configuration.GetSection("ConnectionStrings");
        var appConfigConnectionString = settings["AppConfig"];

        Console.WriteLine($">>>>>>>>>>>>>> AppConfig ConnectionString: {appConfigConnectionString}");

        options.Connect(appConfigConnectionString)
            .ConfigureRefresh(refresh =>
            {
                refresh.Register(AppConfiguration.Keys.TheGarageUrlKey, refreshAll: true)
                    .SetCacheExpiration(TimeSpan.FromSeconds(5));
            });
    });
}


var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Ensure the HttpClient is available for injection
builder.Services.AddScoped(sp => httpClient);

builder.Services.AddScoped<ITheGarageClient, TheGarageClient>();

await builder.Build().RunAsync();
