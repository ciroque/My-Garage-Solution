using Garage;
using Microsoft.AspNetCore.Mvc;

namespace TheGarage.Controllers
{
    [ApiController, Route("seed-data")]
    public class SeedDataController(IVehicleStorage vehicleStorage) : Controller
    {
        [HttpPost(Name = "SeedData")]
        public IActionResult SeedData(IEnumerable<Vehicle> vehicles)
        {
            vehicleStorage.PutVehicles(vehicles);
            
            return Ok();
        }
    }
}
