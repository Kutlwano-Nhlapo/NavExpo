using MongoDB.Bson.Serialization.Attributes;

namespace NavExpo.Data
{
    public class DbVersion
    {
        [BsonId]
        public string Id { get; set; } // e.g., "MainVersion"
        public int VersionNumber { get; set; }
    }
}
