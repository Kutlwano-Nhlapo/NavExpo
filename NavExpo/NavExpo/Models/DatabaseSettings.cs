namespace NavExpo.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string EventsCollectionName { get; set; } = null!;
        public string AttendeesCollectionName { get; set; } = null!;
    }
}
