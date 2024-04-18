namespace Garage;

public class SeedDataGenerator
{
    private readonly Dictionary<string, IEnumerable<Vehicle>> _vehicles = new()
    {
        [VehicleCategories.FamilyVehicles] = new List<Vehicle>
        {
            new()
            {
                Make = "Toyota",
                Model = "Highlander",
                Year = 2022,
                Color = "Silver",
                Vin = "1G1ND52JXM6311909"
            },
            new()
            {
                Make = "Honda",
                Model = "CR-V",
                Year = 2018,
                Color = "White",
                Vin = "1HGCM66583A053386"
            },
            new()
            {
                Make = "Ford",
                Model = "Explorer",
                Year = 2005,
                Color = "Blue",
                Vin = "1FMCU9K30JUA03305"
            }

        },
        [VehicleCategories.PerformanceVehicles] = new List<Vehicle>
        {
            new()
            {
                Make = "Porsche",
                Model = "911 Carrera",
                Year = 2024,
                Color = "Red",
                Vin = "WP0AA2A92ES180221"
            },
            new()
            {
                Make = "Chevrolet",
                Model = "Corvette",
                Year = 2022,
                Color = "Black",
                Vin = "1G1Y72D45L5109361"
            },
            new()
            {
                Make = "BMW",
                Model = "M5",
                Year = 2023,
                Color = "Silver",
                Vin = "WBSJV6C51NCE42652"
            }
        },
        [VehicleCategories.CorporateFleet] = new List<Vehicle>
        {
            new()
            {
                Make = "Ford",
                Model = "Fusion",
                Year = 2022,
                Color = "Gray",
                Vin = "3FA6P0G71LR155004"
            },
            new()
            {
                Make = "Toyota",
                Model = "Camry",
                Year = 2023,
                Color = "White",
                Vin = "4T1B11HK3JU080636"
            },
            new()
            {
                Make = "Nissan",
                Model = "Altima",
                Year = 2019,
                Color = "Black",
                Vin = "1N4BL4BV8KC220076"
            }
        }
    };

    public IEnumerable<Vehicle> this[string category] => _vehicles[category];
}