using Garage;

namespace MyGarage.Services;

public interface ITheGarageClient
{
    Task<IEnumerable<Vehicle>?> GetVehiclesAsync();
    Task<Vehicle?> GetVehicleAsync(Guid id);
    Task CreateVehicleAsync(Vehicle vehicle);
    Task UpdateVehicleAsync(Vehicle vehicle);
    Task DeleteVehicleAsync(Guid id);
    Task<string> StoreImageAsync(string filename, Stream stream);
    Task SeedVehicles(IEnumerable<Vehicle> vehicles);
}
