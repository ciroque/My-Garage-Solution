using System.Net.Http.Json;
using Garage;

namespace MyGarage.Services
{
    public class TheGarageClient(HttpClient httpClient, string theGarageHost) : ITheGarageClient
    {
        private string BaseUrl = $"{theGarageHost}/vehicles";

        public async Task<IEnumerable<Vehicle>?> GetVehiclesAsync()
        {
            var response = await httpClient.GetAsync(BaseUrl);
            var vehicles = await response.Content.ReadFromJsonAsync<IEnumerable<Vehicle>>();
            return vehicles;
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

        public async Task<string> StoreImageAsync(string filename, Stream stream)
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StreamContent(stream), "file", filename);

            var response = await httpClient.PostAsync($"{BaseUrl}/photos", formData);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseBody);

            return response.Headers.Location?.ToString() ?? responseBody;
        }

        public async Task SeedVehicles(IEnumerable<Vehicle> vehicles)
        {
            var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/seed-data", vehicles);

            response.EnsureSuccessStatusCode();
        }
    }
}
