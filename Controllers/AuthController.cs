using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using URUN.Models;

namespace URUN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Mock kullanici listesi (rollerle birlikte)
        private static readonly List<User> MockUsers = new List<User>
        {
            new User { Username = "admin", Password = "password", Role = "Admin" },
            new User { Username = "user", Password = "12345", Role = "User" }
        };

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest(new { Mesaj = "Gecersiz istek. username ve password zorunludur." });
            }

            // Kullanici dogrulama
            var validUser = MockUsers.FirstOrDefault(u =>
                u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == user.Password);

            if (validUser != null)
            {
                var token = GenerateJwtToken(validUser.Username, validUser.Role);
                return Ok(new { Token = token });
            }

            return Unauthorized(new { Mesaj = "Gecersiz kullanici adi veya sifre." });
        }

        private string GenerateJwtToken(string username, string role)
        {
            // JWT ayarlarini alin
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiresInMinutes = jwtSettings["ExpiresInMinutes"];

            // Dogrulama: Anahtar uzunlugunu kontrol edin
            if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            {
                throw new SecurityTokenException("JWT anahtari en az 32 karakter uzunlugunda olmalidir.");
            }

            // Token olusturma
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role) // Rol bilgisi ekleniyor
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(expiresInMinutes)),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
