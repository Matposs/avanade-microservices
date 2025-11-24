using AuthService.Models;

namespace AuthService.Services
{
    public class UserService
    {
        private readonly List<User> _users = new();

        public User Register(string email, string password)
        {
            if (_users.Any(u => u.Email == email))
                throw new Exception("Usuário já existe");

            var hashed = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

            var user = new User { Email = email, PasswordHash = hashed };
            _users.Add(user);
            return user;
        }

        public User? Login(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user == null) return null;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
        }
    }
}
