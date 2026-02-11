using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NavExpo.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NavExpo.Services
{
    // This service manages users, allowing us to create, read, update, and delete user documents in the database
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        // Constructor: We inject the Database and get the specific collection
        public UserService(IMongoDatabase mongoDatabase)
        {
            // "Users" is the name of the collection in MongoDB
            _usersCollection = mongoDatabase.GetCollection<User>("Users");
        }

        // 1. GET ALL
        public async Task<List<User>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        // 2. GET ONE BY ID
        public async Task<User?> GetAsync(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        //Email is unique, so we can also get a user by email
        public async Task<User?> GetByEmailAsync(string email) =>
            await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();

        // 3. CREATE
        public async Task CreateAsync(User newUser)
        {
            await _usersCollection.InsertOneAsync(newUser);
            // After this runs, newUser.Id will be populated automatically
        }

        // 4. UPDATE
        public async Task UpdateAsync(string id, User updatedUser)
        {
            // Ensures the ID in the object matches the ID in the URL
            updatedUser.Id = id;

            // Replaces the whole document with the new one
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);
        }

        // 5. DELETE
        public async Task RemoveAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);
    }
    // This service manages maps, allowing us to create and read map documents in the database
    public class MapService
    {
        private readonly IMongoCollection<MapDocument> _mapsCollection;

        public MapService(IMongoDatabase mongoDatabase)
        {
            _mapsCollection = mongoDatabase.GetCollection<MapDocument>("Maps");
        }

        public async Task CreateAsync(MapDocument map)
        {
            await _mapsCollection.InsertOneAsync(map);
        }

        public async Task<List<MapDocument>> GetAllAsync()
        {
            return await _mapsCollection.Find(_ => true).ToListAsync();
        }
    }
    // This service manages events, allowing us to create, read, update, delete, and search for events in the database
    public class EventService
    {
        private readonly IMongoCollection<EventForm> _eventsCollection;

        public EventService(IMongoDatabase mongoDatabase)
        {
           
            _eventsCollection = mongoDatabase.GetCollection<EventForm>("EventForms");
        }

        public async Task<List<EventForm>> GetAsync() =>
            await _eventsCollection.Find(_ => true).ToListAsync();

        public async Task<EventForm?> GetAsync(string id) =>
            await _eventsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<List<EventForm>> GetByOrganizerAsync(string organizerId) =>
            await _eventsCollection.Find(x => x.OrganizerId == organizerId).ToListAsync();

        public async Task CreateAsync(EventForm newEvent) =>
            await _eventsCollection.InsertOneAsync(newEvent);

        public async Task UpdateAsync(string id, EventForm updatedEvent)
        {
            updatedEvent.UpdatedAt = DateTime.UtcNow;
            await _eventsCollection.ReplaceOneAsync(x => x.Id == id, updatedEvent);
        }

        public async Task RemoveAsync(string id) =>
            await _eventsCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<List<EventForm>> SearchAsync(string searchTerm)
        {
            var filter = Builders<EventForm>.Filter.Or(
                Builders<EventForm>.Filter.Regex(x => x.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<EventForm>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<EventForm>.Filter.Regex(x => x.Location, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            return await _eventsCollection.Find(filter).ToListAsync();
        }

        public async Task IncrementAttendeeCountAsync(string id)
        {
            var update = Builders<EventForm>.Update.Inc(x => x.AttendeeCount, 1);
            await _eventsCollection.UpdateOneAsync(x => x.Id == id, update);
        }

        public async Task DecrementAttendeeCountAsync(string id)
        {
            var update = Builders<EventForm>.Update.Inc(x => x.AttendeeCount, -1);
            await _eventsCollection.UpdateOneAsync(x => x.Id == id, update);
        }
    }
    // This service manages attendees for events, allowing us to track who is attending which event
    public class AttendeeService
    {
        private readonly IMongoCollection<Attendee> _attendeesCollection;

        public AttendeeService(IMongoDatabase mongoDatabase)
        {           
            _attendeesCollection = mongoDatabase.GetCollection<Attendee>("Attendees");
        }

        public async Task<List<Attendee>> GetAsync() =>
            await _attendeesCollection.Find(_ => true).ToListAsync();

        public async Task<Attendee?> GetAsync(string id) =>
            await _attendeesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<List<Attendee>> GetByEventAsync(string eventId) =>
            await _attendeesCollection.Find(x => x.EventId == eventId).ToListAsync();

        public async Task<Attendee?> GetByEventAndEmailAsync(string eventId, string email) =>
            await _attendeesCollection.Find(x => x.EventId == eventId && x.Email == email).FirstOrDefaultAsync();

        public async Task CreateAsync(Attendee newAttendee) =>
            await _attendeesCollection.InsertOneAsync(newAttendee);

        public async Task RemoveAsync(string id) =>
            await _attendeesCollection.DeleteOneAsync(x => x.Id == id);
    }
    // This service handles authentication-related tasks like password hashing and JWT generation
    public class AuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ======================
        // PASSWORD HASHING
        // ======================
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        // ======================
        // JWT GENERATION
        // ======================
        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is missing from configuration.");
            }

            var keyBytes = Convert.FromBase64String(jwtKey);

            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}