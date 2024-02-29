using System.Net.Http.Json;
using Garage;

namespace MyGarage.Services
{
    public interface ITheGarageClient
    {
        Task<IEnumerable<Vehicle>?> GetVehiclesAsync();
        Task<Vehicle?> GetVehicleAsync(Guid id);
        Task CreateVehicleAsync(Vehicle vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(Guid id);
    }

    public class TheGarageClient(HttpClient httpClient) : ITheGarageClient
    {
        // TODO: Figure out how to get this value from configuration / for deployment in Azure.
        private const string BaseUrl = $"https://localhost:7213/vehicles";

        public async Task<IEnumerable<Vehicle>?> GetVehiclesAsync()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<Vehicle>>(BaseUrl);
        }

        public async Task<Vehicle?> GetVehicleAsync(Guid id)
        {
            return await httpClient.GetFromJsonAsync<Vehicle>($"{BaseUrl}/{id}");
        }

        public async Task CreateVehicleAsync(Vehicle vehicle)
        {
            var response = await httpClient.PostAsJsonAsync(BaseUrl, vehicle);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            var response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{vehicle.Id}", vehicle);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteVehicleAsync(Guid id)
        {
            var response = await httpClient.DeleteAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
