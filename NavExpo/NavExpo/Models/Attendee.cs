using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NavExpo.Models
{
    public class Attendee
    {
         
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("eventId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string EventId { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("phone")]
        public string Phone { get; set; } = null!;

        [BsonElement("dietaryRestrictions")]
        public string? DietaryRestrictions { get; set; }

        [BsonElement("specialRequests")]
        public string? SpecialRequests { get; set; }

        [BsonElement("registeredAt")]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
