using Azure.Storage.Blobs;

namespace TheGarage.Services
{

    /*--
     * Implements the IPhotoStorage interface to store photos in an Azure Blob Storage container.
    --*/
    public class AzurePhotoStorageService(IConfiguration configuration) : IPhotoStorage
    {
        private readonly Lazy<BlobContainerClient> _blobContainerClient = new Lazy<BlobContainerClient>(() => CreateBlobServiceClient(configuration));

        private static BlobContainerClient CreateBlobServiceClient(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue(AppConfiguration.Keys.AzureStorageConnectionString, AppConfiguration.Defaults.AzureStorageConnectionString);
            var containerName = configuration.GetValue(AppConfiguration.Keys.AzureStorageContainerName, AppConfiguration.Defaults.AzureStorageContainerName);

            var blobServiceClient = new BlobServiceClient(connectionString);
            return blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> StorePhoto(string filename, Stream photo)
        {
            var blobClient = _blobContainerClient.Value.GetBlobClient(filename);

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
