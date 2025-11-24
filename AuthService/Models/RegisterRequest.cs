namespace AuthService.Models
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public string? Password { get; set; }
    }
}