using Garage;
using Microsoft.AspNetCore.Mvc;

using TheGarage.ActionFilters;

namespace TheGarage.Controllers
{
    [ApiController, AddSasTokenHeaderActionFilter, Route("vehicles")]
    public class VehicleController(IVehicleStorage vehicleStorage) : ControllerBase
    {
        private const string MyKey = "vehicles-r2112";

        [HttpGet(Name = "GetVehicles")]
        public IEnumerable<Vehicle> Index()
        {
            return vehicleStorage.GetVehicles(MyKey);
        }

        [HttpGet("{id}", Name = "GetVehicle")]
        public Vehicle GetVehicle(string id)
        {
            return vehicleStorage.GetVehicle(MyKey, id);
        }

        [HttpPost(Name = "AddVehicle")]
        public IActionResult AddVehicle(Vehicle vehicle)
        {
            vehicleStorage.AddVehicle(MyKey, vehicle);

            return Ok();
        }

        [HttpPut("{Id}", Name = "UpdateVehicle")]
        public IActionResult UpdateVehicle(Vehicle vehicle)
        {
            vehicleStorage.UpdateVehicle(MyKey, vehicle);

            return Ok();
        }

        [HttpDelete("{Id}", Name = "RemoveVehicle")]
        public IActionResult RemoveVehicle(Guid id)
        {
            vehicleStorage.RemoveVehicle(MyKey, id);

            return Ok();
        }
    }
}
