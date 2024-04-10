using System.Text.Json;
using Garage;
using StackExchange.Redis;

namespace TheGarage.Services
{
    /*
     * Implements The IVehicleStorage interface to store vehicles in a Redis database.
     *
     * It is recognized that the Redis database is not the best fit for this application, but it is used here for demonstration purposes.
     *
     */
    public class RedisVehicleStorageService(IConfiguration configuration) : IVehicleStorage
    {
        private readonly Lazy<IDatabase> _database = new Lazy<IDatabase>(() => CreateDatabaseInstance(configuration));

        private static IDatabase CreateDatabaseInstance(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>(AppConfiguration.Keys.RedisConnectionString,
                AppConfiguration.Defaults.RedisConnectionString);
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
            return redis.GetDatabase();
        }

        public IEnumerable<Vehicle> AddVehicle(string key, Vehicle vehicle)
        {
            var json = GetVehicles(key);
            var enumerable = json.ToList();
            var vehicles = enumerable.Append(vehicle).ToList();
            return PutVehicles(key, vehicles);
        }

        public IEnumerable<Vehicle> GetVehicles(string key)
        {
            var vehicles = _database.Value.StringGet(key);
            return vehicles == RedisValue.Null ? new List<Vehicle>() : JsonSerializer.Deserialize<Vehicle[]>(vehicles);
        }

        public Vehicle GetVehicle(string key, string id)
        {
            return GetVehicles(key).ToList().Find(v => v.Id.ToString() == id);
        }

        public IEnumerable<Vehicle> RemoveVehicle(string key, Guid id)
        {
            var json = GetVehicles(key);
            var enumerable = json.ToList();
            var vehicles = enumerable.Where(v => v.Id != id).ToList();
            return PutVehicles(key, vehicles);
        }

        public IEnumerable<Vehicle> UpdateVehicle(string key, Vehicle vehicle)
        {
            var json = GetVehicles(key);
            var enumerable = json.ToList();
            var vehicles = enumerable.Where(v => v.Id != vehicle.Id).Append(vehicle);
            return PutVehicles(key, vehicles);
        }

        private IEnumerable<Vehicle> PutVehicles(string key, IEnumerable<Vehicle> vehicles)
        {
            var enumerable = vehicles.ToList();
            _database.Value.StringSet(key, JsonSerializer.Serialize(enumerable));
            return enumerable;
        }
    }
}
