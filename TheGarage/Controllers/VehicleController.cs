using Garage;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TheGarage.Services;

namespace TheGarage.Controllers
{
    [ApiController]
    [Route("vehicles")]
    public class VehicleController : ControllerBase
    {
        private const string MyKey = "vehicles-r2112";

        [HttpGet(Name = "GetVehicles")]
        public IEnumerable<Vehicle> Index()
        {
            var redis = RedisVehicleStorageService.Create("kungdu.wagner-x.net:6379");
            return redis.GetVehicles(MyKey);
        }

        [HttpGet("{id}", Name = "GetVehicle")]
        public Vehicle GetVehicle(string id)
        {
            var redis = RedisVehicleStorageService.Create("kungdu.wagner-x.net:6379");
            return redis.GetVehicle(MyKey, id);
        }

        [HttpPost(Name = "AddVehicle")]
        public IActionResult AddVehicle(Vehicle vehicle)
        {
            var redis = RedisVehicleStorageService.Create("kungdu.wagner-x.net:6379");

            redis.AddVehicle(MyKey, vehicle);

            return Ok();
        }

        [HttpPut("{Id}", Name = "UpdateVehicle")]
        public IActionResult UpdateVehicle(Vehicle vehicle)
        {
            var redis = RedisVehicleStorageService.Create("kungdu.wagner-x.net:6379");

            redis.UpdateVehicle(MyKey, vehicle);

            return Ok();
        }

        [HttpDelete("{Id}", Name = "RemoveVehicle")]
        public IActionResult RemoveVehicle(Guid id)
        {
            var redis = RedisVehicleStorageService.Create("kungdu.wagner-x.net:6379");

            var removed = redis.RemoveVehicle(MyKey, id);

            return Ok();
        }

        //[HttpDelete(Name = "ClearVehicles")]
        //public IActionResult ClearVehicles()
        //{
        //    var redis = RedisVehicleStorageService.Create("kungdu.wagner-x.net:6379");

        //    redis.ClearVehicles();

        //    return Ok();
        //}
    }
}
