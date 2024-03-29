using Garage;

namespace MyGarage.Services;

public interface ITheGarageClient
{
    Task<TheGarage?> GetVehiclesAsync();
    Task<TheGarage?> GetVehicleAsync(Guid id);
    Task CreateVehicleAsync(Vehicle vehicle);
    Task UpdateVehicleAsync(Vehicle vehicle);
    Task DeleteVehicleAsync(Guid id);
    Task<string> StoreImageAsync(string filename, Stream stream);
}