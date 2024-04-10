using TheGarage;
using TheGarage.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


if (builder.Environment.IsDevelopment())
{
    // Create a new ConfigurationBuilder
    var mockConfigBuilder = new ConfigurationBuilder();

    // Add mock configuration sources
    mockConfigBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
        { AppConfiguration.Keys.AzureStorageConnectionString, Environment.GetEnvironmentVariable(AppConfiguration.Keys.AzureStorageConnectionString) ?? AppConfiguration.NoDefaultProvided },
        { AppConfiguration.Keys.AzureStorageSasToken, Environment.GetEnvironmentVariable(AppConfiguration.Keys.AzureStorageSasToken) ?? AppConfiguration.NoDefaultProvided },
        { AppConfiguration.Keys.AzureStorageContainerName, Environment.GetEnvironmentVariable(AppConfiguration.Keys.AzureStorageContainerName) ?? AppConfiguration.NoDefaultProvided },
        { AppConfiguration.Keys.RedisConnectionString, Environment.GetEnvironmentVariable(AppConfiguration.Keys.RedisConnectionString) ?? AppConfiguration.NoDefaultProvided },
    });
    
    var defaultConfiguration = mockConfigBuilder.Build();
    
    builder.Services.AddSingleton<IConfiguration>(defaultConfiguration);
}
else
{
    builder.Services.AddAzureAppConfiguration();
}


builder.Services.AddScoped<IVehicleStorage, RedisVehicleStorageService>();
builder.Services.AddScoped<IPhotoStorage, AzurePhotoStorageService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(AppConfiguration.SasHeaderName);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();
