namespace NavExpo.Models
{
    public class Register
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Age { get; set; }
        public string Role { get; set; } = "Guest";
    }
}
