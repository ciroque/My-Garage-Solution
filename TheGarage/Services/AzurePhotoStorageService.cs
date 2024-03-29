using Azure.Storage.Blobs;

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

        public async Task<string> StorePhoto(string filename, Stream photo)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(filename);

            try
            {
                if (!await blobClient.ExistsAsync())
                {
                    await blobClient.UploadAsync(photo);
                }
            }
            catch (Azure.RequestFailedException aex)
            {
                Console.WriteLine(aex.ErrorCode);
            }

        

            return blobClient.Uri.ToString();
        }

        public void DeletePhoto(Guid vehicleId, string photoUrl)
        {
            throw new NotImplementedException();
        }
    }
}
