namespace TheGarage.Services
{
    public class AzurePhotoStorageService : IPhotoStorage
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public static IPhotoStorage Create(string connectionString, string containerName)
        {
            return new AzurePhotoStorageService(connectionString, containerName);
        }

        private AzurePhotoStorageService(string connection, string containerName)
        {
            _connectionString = connection;
            _containerName = containerName;
        }

        public string StorePhoto(Guid vehicleId, string photo)
        {
            throw new NotImplementedException();
        }

        public void DeletePhoto(Guid vehicleId, string photoUrl)
        {
            throw new NotImplementedException();
        }
    }
}
