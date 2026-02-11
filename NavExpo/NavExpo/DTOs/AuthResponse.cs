namespace NavExpo.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public UserDTO User { get; set; } = null!;
    }

    public class UserDTO
    {
        public string? Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int Age { get; set; }
        public string Role { get; set; } = null!;
    }
}
