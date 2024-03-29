namespace TheGarage
{

    public interface IPhotoStorage
    {
        Task<string> StorePhoto(string filename, Stream photo); // TODO: What type should photo be? Return the URL of the photo.
        void DeletePhoto(Guid vehicleId, string photoUrl);
    }
}