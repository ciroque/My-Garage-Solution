namespace TheGarage;

public class AppConfiguration
{
    public class Keys
    {
        public const string RedisConnectionString = "RedisConnectionString";
        public const string AzureStorageConnectionString = "AzureStorageConnectionString";
        public const string AzureStorageContainerName = "AzureStorageContainerName";
    }

    public class Defaults
    {
        public const string RedisConnectionString = "localhost:6379";
        public const string AzureStorageConnectionString = NoDefaultProvided;
        public const string AzureStorageContainerName = "vehicle-gallery";
    }

    public const string NoDefaultProvided = "Default Value Not Provided";
}
