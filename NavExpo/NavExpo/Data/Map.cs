using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NavExpo.Models
{
    public class MapDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("fileName")]
        public string FileName { get; set; } = string.Empty;

        [BsonElement("originalName")]
        public string OriginalName { get; set; } = string.Empty;

        [BsonElement("fileUrl")]
        public string FileUrl { get; set; } = string.Empty;

        [BsonElement("uploadDate")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }
}