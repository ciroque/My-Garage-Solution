using Garage;

namespace TheGarage
{
    public interface IVehicleStorage
    {
        IEnumerable<Vehicle> AddVehicle(Vehicle vehicle);
        IEnumerable<Vehicle> GetVehicles();
        Vehicle GetVehicle(string id);
        IEnumerable<Vehicle> PutVehicles(IEnumerable<Vehicle> vehicles);
        IEnumerable<Vehicle> RemoveVehicle(Guid id);
        IEnumerable<Vehicle> UpdateVehicle(Vehicle vehicle);
    }
}
