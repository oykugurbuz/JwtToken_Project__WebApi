using JwtTokenProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

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

    [AllowAnonymous]
    [HttpPost("signup")]
    public IActionResult Signup([FromBody] SignupModel signup)
    {
        if (_context.AppUserInfos.Any(u => u.UserName == signup.UserName))
        {
            return BadRequest(new { message = "Kullanıcı adı zaten var!" });
        }
        var user = new AppUserInfo
        {
            UserName = signup.UserName,
            Password = signup.Password,
            Email = signup.Email,
            IsActive = true,
            FailedAttempt = 0,
            LastLoginDate = null,
            RememberMe = false,
            UserTypeName = "user",
            Token = null
        };

        _context.AppUserInfos.Add(user);
        _context.SaveChanges();

        return Ok(new { message = "Kullanıcı başarıyla kaydedildi!" });


    }

    // Kullanıcı girişi (Login) 

    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes("application/json")]
    public IActionResult Login([FromBody] LoginRequest login)
    {
        var user = _context.AppUserInfos.SingleOrDefault(u => u.UserName == login.UserName);

        if (user == null || user.Password != login.Password)
        {
            return Unauthorized(new { message = "Yanlış kullanıcı adı veya şifre" });
        }
        if (user.UserName == null)
        {
            return Unauthorized(new { message = "Kullanıcı bulunamadı." });
        }
        var token = GenerateJwtToken(user.UserName);
        
        return Ok(token);

    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult GetProtectedData()
    {
        var userClaims = User.Claims;

        var userName = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized(new { message = "Kullanıcı bilgileri alınamadı." });
        }

        return Ok(new { UserName = userName });
    }


    private string GenerateJwtToken(string username)
    {
        var claims = new List<Claim>
    {
            new Claim(ClaimTypes.Name, username),

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var secretKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new ArgumentException("JWT anahtarı yapılandırılmamış. Lütfen appsettings.json dosyasını kontrol edin.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
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
