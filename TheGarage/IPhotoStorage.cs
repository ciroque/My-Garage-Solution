namespace TheGarage
{

    public interface IPhotoStorage
    {
        string StorePhoto(Guid vehicleId, string photo); // TODO: What type should photo be? Return the URL of the photo.
        void DeletePhoto(Guid vehicleId, string photoUrl);
    }
}