namespace TheGarage;

public class AppConfiguration
{
    public string RedisConnectionString { get; set; } // Environment.GetEnvironmentVariable("REDIS_HOST");
    public string AzureStorageConnectionString { get; set; }
    public string AzureStorageContainerName { get; set; }
    public string AzureStorageSasToken { get; set; }
}
