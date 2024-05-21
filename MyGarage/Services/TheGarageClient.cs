using System.Net.Http.Json;
using Garage;
using TheGarage;

namespace MyGarage.Services
{
    public class TheGarageClient(HttpClient httpClient, IConfiguration configuration) : ITheGarageClient
    {

        private string _baseUrl = $"{configuration.GetValue(AppConfiguration.Keys.TheGarageUrlKey, AppConfiguration.Defaults.TheGarageUrl)}/vehicles";

        public async Task<IEnumerable<Vehicle>?> GetVehiclesAsync()
        {
            var response = await httpClient.GetAsync(_baseUrl);
            var vehicles = await response.Content.ReadFromJsonAsync<IEnumerable<Vehicle>>();
            return vehicles;
        }

        public async Task<Vehicle?> GetVehicleAsync(Guid id)
        {
            return await httpClient.GetFromJsonAsync<Vehicle>($"{_baseUrl}/{id}");
        }

        public async Task CreateVehicleAsync(Vehicle vehicle)
        {
            var response = await httpClient.PostAsJsonAsync(_baseUrl, vehicle);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            var response = await httpClient.PutAsJsonAsync($"{_baseUrl}/{vehicle.Id}", vehicle);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteVehicleAsync(Guid id)
        {
            var response = await httpClient.DeleteAsync($"{_baseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> StoreImageAsync(string filename, Stream stream)
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StreamContent(stream), "file", filename);

            var response = await httpClient.PostAsync($"{_baseUrl}/photos", formData);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseBody);

            return response.Headers.Location?.ToString() ?? responseBody;
        }

        public async Task SeedVehicles(IEnumerable<Vehicle> vehicles)
        {
            var response = await httpClient.PostAsJsonAsync($"{_baseUrl}/seed-data", vehicles);

            response.EnsureSuccessStatusCode();
        }
    }
}
