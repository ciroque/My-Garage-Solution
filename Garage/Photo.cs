namespace Garage
{
    public class Photo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Title { get; set; }
        public string? Uri { get; set; }
        public string? Description { get; set; }
    }
}
