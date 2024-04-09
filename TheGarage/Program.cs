using TheGarage;
using TheGarage.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var appConfiguration = new AppConfiguration();
configuration.Bind("AppConfiguration", appConfiguration);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(appConfiguration);
builder.Services.AddScoped<IVehicleStorage>(sp => RedisVehicleStorageService.Create(appConfiguration.RedisConnectionString));
builder.Services.AddScoped<IPhotoStorage>(sp => AzurePhotoStorageService.Create(appConfiguration.AzureStorageConnectionString, appConfiguration.AzureStorageContainerName));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("x-sas-token");
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
