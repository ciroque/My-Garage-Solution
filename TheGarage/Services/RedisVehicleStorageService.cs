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
        private readonly string _key = configuration.GetValue<string>(AppConfiguration.Keys.RedisKey, AppConfiguration.Defaults.RedisKey);

        private static IDatabase CreateDatabaseInstance(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>(AppConfiguration.Keys.RedisConnectionString,
                AppConfiguration.Defaults.RedisConnectionString);
            Console.WriteLine($">>>>>> Connecting to Redis at {connectionString}");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
            return redis.GetDatabase();
        }

        public IEnumerable<Vehicle> AddVehicle(Vehicle vehicle)
        {
            var json = GetVehicles();
            var enumerable = json.ToList();  
            var vehicles = enumerable.Append(vehicle).ToList();
            return PutVehicles(vehicles);
        }

        public IEnumerable<Vehicle> GetVehicles()
        {
            var vehicles = _database.Value.StringGet(_key);
            return vehicles == RedisValue.Null ? new List<Vehicle>() : JsonSerializer.Deserialize<Vehicle[]>(vehicles);
        }

        public Vehicle GetVehicle(string id)
        {
            return GetVehicles().ToList().Find(v => v.Id.ToString() == id);
        }

        public IEnumerable<Vehicle> RemoveVehicle(Guid id)
        {
            var json = GetVehicles();
            var enumerable = json.ToList();
            var vehicles = enumerable.Where(v => v.Id != id).ToList();
            return PutVehicles(vehicles);
        }

        public IEnumerable<Vehicle> UpdateVehicle(Vehicle vehicle)
        {
            var json = GetVehicles();
            var enumerable = json.ToList();
            var vehicles = enumerable.Where(v => v.Id != vehicle.Id).Append(vehicle);
            return PutVehicles(vehicles);
        }

        /*
         * HULK SMASH.
         * This will overwrite the existing vehicles in the Redis database with the provided vehicles.
         */
        public IEnumerable<Vehicle> PutVehicles(IEnumerable<Vehicle> vehicles)
        {
            var enumerable = vehicles.ToList();
            _database.Value.StringSet(_key, JsonSerializer.Serialize(enumerable));
            return enumerable;
        }
    }
}
