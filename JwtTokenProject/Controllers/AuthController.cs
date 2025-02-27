using JwtTokenProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Runtime.ConstrainedExecution;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthController(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    // Kullanıcı kaydetme (Signup)
    [HttpPost("signup")]
    public IActionResult Signup([FromBody] SignupModel signup)
    {
        // Kullanıcı adı daha önce alınmış mı kontrol et
        if (_context.Users.Any(u => u.UserName == signup.UserName))
        {
            return BadRequest(new { message = "Kullanıcı adı zaten var!" });
        }

        // Yeni kullanıcı oluştur ve kaydet
        var user = new User
        {
            UserName = signup.UserName,
            Password = signup.Password // Düz şifreyi direkt olarak kaydediyoruz.
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(new { message = "Kullanıcı başarıyla kaydedildi!" });
    }

    // Kullanıcı girişi (Login)
    [HttpPost("login")]
    public IActionResult Login([FromBody] User login)
    {
        var user = _context.Users.SingleOrDefault(u => u.UserName == login.UserName);

        if (user == null || user.Password != login.Password) // Şifreyi karşılaştırıyoruz
        {
            return Unauthorized(new { message = "Yanlış kullanıcı adı veya şifre" });
        }

        var token = GenerateJwtToken(user.UserName);
        return Ok(new { token });
    }



    private string GenerateJwtToken(string username)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var keyString = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString))
        {
            throw new ArgumentException("JWT Key is not configured properly. Please check the configuration.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"]));
        var notBefore = now.AddSeconds(1);

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            notBefore: notBefore,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

    }
}
