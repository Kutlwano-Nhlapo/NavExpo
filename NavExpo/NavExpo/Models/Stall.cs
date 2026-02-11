using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NavExpo.Models
{
    public class Stall
    {
        [BsonId]
        public int StallId { get; set; }
        public string StallName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string location { get; set; }
        [ForeignKey("EventName")]
        [Required]
        public string FormId { get; set; }
    }    
}
