namespace Garage
{
    public class Vehicle
    {
        public static readonly Vehicle Empty = new Vehicle();

        public Vehicle()
        {
            Id = Guid.Empty;
        }

        public Vehicle(string  vin, string? make, string? model, int year, string? color, string? licensePlate)
        {
            Vin = vin;
            Make = make;
            Model = model;
            Year = year;
            Color = color;
            LicensePlate = licensePlate;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Vin { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public string? Color { get; set; }
        public string? LicensePlate { get; set; }
        public Maintenance[]? MaintenanceHistory { get; set; }
        public IEnumerable<Photo>? Photos { get; set; }

        public void AddMaintenance(Maintenance maintenance)
        {
            if (MaintenanceHistory == null)
            {
                MaintenanceHistory = new Maintenance[] { maintenance };
            }
            else
            {
                var list = MaintenanceHistory.ToList();
                list.Add(maintenance);
                MaintenanceHistory = list.ToArray();
            }
        }

        public Maintenance? GetMaintenance(Guid id)
        {
            if (MaintenanceHistory != null)
            {
                var list = MaintenanceHistory.ToList();
                return list.Find(m => m.Id == id);
            }

            return null;
        }

        public void RemoveMaintenance(Guid id)
        {
            if (MaintenanceHistory != null)
            {
                var list = MaintenanceHistory.ToList();
                list.RemoveAll(m => m.Id == id);
                MaintenanceHistory = list.ToArray();
            }
        }

        public void UpdateMaintenance(Maintenance maintenance)
        {
            if (MaintenanceHistory != null)
            {
                var list = MaintenanceHistory.ToList();
                var index = list.FindIndex(m => m.Id == maintenance.Id);
                list[index] = maintenance;
                MaintenanceHistory = list.ToArray();
            }
        }

        public void AddPhoto(Photo photo)
        {
            if (Photos == null)
            {
                Photos = new Photo[] { photo };
            }
            else
            {
                var list = Photos.ToList();
                list.Add(photo);
                Photos = list.ToArray();
            }
        }

        public Photo? GetPhoto(Guid id)
        {
            if (Photos != null)
            {
                var list = Photos.ToList();
                return list.Find(p => p.Id == id);
            }

            return null;
        }

        public void RemovePhoto(Guid id)
        { // TODO: Remove from cloud storage
            if (Photos != null)
            {
                var list = Photos.ToList();
                list.RemoveAll(p => p.Id == id);
                Photos = list.ToArray();
            }
        }

        public void UpdatePhoto(Photo photo)
        {
            if (Photos != null)
            {
                var list = Photos.ToList();
                var index = list.FindIndex(p => p.Id == photo.Id);
                list[index] = photo;
                Photos = list.ToArray();
            }
        }
    }
}
    