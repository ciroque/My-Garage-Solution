namespace Garage;

public class VehicleCategories
{
    public const string FamilyVehicles = "Family Vehicles";
    public const string PerformanceVehicles = "Performance Vehicles";
    public const string CorporateFleet = "Corporate Fleet";

    public static readonly IEnumerable<string> Categories = new List<string> 
    {
        FamilyVehicles,
        PerformanceVehicles,
        CorporateFleet
    };
}
