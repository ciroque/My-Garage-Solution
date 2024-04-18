using Garage;
using Microsoft.AspNetCore.Mvc;


namespace TheGarage.Controllers
{
    [ApiController, Route("vehicles")]
    public class VehicleController(IVehicleStorage vehicleStorage) : ControllerBase
    {
        [HttpGet(Name = "GetVehicles")]
        public IEnumerable<Vehicle> Index()
        {
            return vehicleStorage.GetVehicles();
        }

        [HttpGet("{id}", Name = "GetVehicle")]
        public Vehicle GetVehicle(string id)
        {
            return vehicleStorage.GetVehicle(id);
        }

        [HttpPost(Name = "AddVehicle")]
        public IActionResult AddVehicle(Vehicle vehicle)
        {
            vehicleStorage.AddVehicle(vehicle);

            return Ok();
        }

        [HttpPut("{Id}", Name = "UpdateVehicle")]
        public IActionResult UpdateVehicle(Vehicle vehicle)
        {
            vehicleStorage.UpdateVehicle(vehicle);

            return Ok();
        }

        [HttpDelete("{Id}", Name = "RemoveVehicle")]
        public IActionResult RemoveVehicle(Guid id)
        {
            vehicleStorage.RemoveVehicle(id);

            return Ok();
        }
    }
}
