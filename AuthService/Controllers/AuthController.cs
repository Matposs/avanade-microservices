using AuthService.Models;
using AuthService.Services;
using AuthService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public AuthController(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register(RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { erro = "Email é obrigatório", codigo = "AUTH_001" });

            var password = string.IsNullOrWhiteSpace(req.Password)
                ? PasswordGenerator.Generate()
                : req.Password;

            try
            {
                var user = _userService.Register(req.Email, password);
                return Ok(new { user.Id, user.Email, password });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message, codigo = "AUTH_002" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { erro = "Email e senha são obrigatórios", codigo = "AUTH_004" });

            var user = _userService.Login(req.Email, req.Password);
            if (user == null)
                return Unauthorized(new { erro = "Credenciais inválidas", codigo = "AUTH_003" });

            var token = _tokenService.Generate(user);
            return Ok(new { token });
        }
    }
}
