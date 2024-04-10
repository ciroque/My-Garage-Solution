namespace TheGarage;

public class AppConfiguration
{
    public class Keys
    {
        public const string RedisConnectionString = "RedisConnectionString";
        public const string AzureStorageConnectionString = "AzureStorageConnectionString";
        public const string AzureStorageContainerName = "AzureStorageContainerName";
        public const string AzureStorageSasToken = "AzureStorageSasToken";
    }

    public class Defaults
    {
        public const string RedisConnectionString = "localhost:6379";
        public const string AzureStorageConnectionString = "AzureStorageConnectionString";
        public const string AzureStorageContainerName = "my-vehicles";
        public const string AzureStorageSasToken = "AzureStorageSasToken";
    }


    public const string SasHeaderName = "x-sas-token";
}
