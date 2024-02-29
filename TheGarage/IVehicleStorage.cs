using Garage;

namespace TheGarage
{
    public interface IVehicleStorage
    {
        IEnumerable<Vehicle> AddVehicle(string key, Vehicle vehicle);
        IEnumerable<Vehicle> GetVehicles(string key);
        Vehicle GetVehicle(string key, string id);
        IEnumerable<Vehicle> RemoveVehicle(string key, Guid id);
        IEnumerable<Vehicle> UpdateVehicle(string key, Vehicle vehicle);
    }
}
