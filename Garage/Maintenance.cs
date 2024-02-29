namespace Garage
{
    public class Maintenance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; }
        public MaintenanceItem MaintenanceItem { get; set; }
        public int Mileage { get; set; }
        public string? Notes { get; set; }
    }

    public enum MaintenanceItem
    {
        OilChange,
        TireRotation,
        AirFilter,
        Transmission,
        BrakePads,
        WiperBlades
    }
}
