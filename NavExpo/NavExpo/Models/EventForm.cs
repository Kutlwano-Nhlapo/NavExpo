using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace NavExpo.Models
{
    public class EventForm
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("date")]
        public string Date { get; set; } = null!;

        [BsonElement("time")]
        public string Time { get; set; } = null!;

        [BsonElement("location")]
        public string Location { get; set; } = null!;

        [BsonElement("organizer")]
        public string Organizer { get; set; } = null!;

        [BsonElement("organizerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string OrganizerId { get; set; } = null!;

        [BsonElement("capacity")]
        public int Capacity { get; set; }

        [BsonElement("attendeeCount")]
        public int AttendeeCount { get; set; } = 0;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public byte[]? ImageFile { get; set; }
    }
}
