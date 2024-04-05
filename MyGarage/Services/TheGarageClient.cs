using System.Net.Http.Json;
using System.Text.Json;
using Garage;

namespace MyGarage.Services
{
    public class TheGarage(IEnumerable<Vehicle> vehicles, string sasToken)
    {
        public readonly string SasToken = sasToken;
        public readonly IEnumerable<Vehicle> Vehicles = vehicles;
    }

    public class TheGarageClient(HttpClient httpClient) : ITheGarageClient
    {
        // TODO: Figure out how to get this value from configuration / for deployment in Azure.
        private const string BaseUrl = $"http://localhost:8080/vehicles";

        public async Task<TheGarage?> GetVehiclesAsync()
        {
            var response = await httpClient.GetAsync(BaseUrl);
            var sasToken = response.Headers.GetValues("x-sas-token").FirstOrDefault();
            var vehicles = await response.Content.ReadFromJsonAsync<IEnumerable<Vehicle>>();
            return new TheGarage(vehicles, sasToken);
        }

        public async Task<TheGarage?> GetVehicleAsync(Guid id)
        {
            var response = await httpClient.GetAsync($"{BaseUrl}/{id}");
            var sasToken = response.Headers.GetValues("x-sas-token").FirstOrDefault();
            var vehicles = await response.Content.ReadFromJsonAsync<Vehicle>();
            return new TheGarage([vehicles], sasToken);
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
    }
}
