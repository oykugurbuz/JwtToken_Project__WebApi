﻿using JwtTokenProject.Models;
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
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { message = "Geçersiz giriş.", errors });
        }

        if (_context.AppUserInfos.Any(u => u.UserName == signup.UserName))
        {
            return BadRequest(new { message = "Bu kullanıcı adı zaten kayıtlı!" });
        }
        if (_context.AppUserInfos.Any(u => u.IdentityNumber == signup.IdentityNumber))
        {
            return BadRequest(new { message = "Bu tc.kimlik numarası zaten kayıtlı!" });
        }
        if (_context.AppUserInfos.Any(u => u.Email == signup.Email))
        {
            return BadRequest(new { message = "Bu e-posta adresi zaten kayıtlı!" });
        }

        var user = new AppUserInfo
        {
            UserName = signup.UserName,
            Password = signup.Password,
            IdentityNumber = signup.IdentityNumber,
            Email = signup.Email,
            IsActive = true,
            FailedAttempt = 0,
            LastLoginDate = null,
            RememberMe = false,
            UserTypeName = "user",
            Token = null,
            AuthorityLevel = signup.AuthorityLevel ?? 4 // Varsayılan olarak 4 (user) olarak ayarlanıyor
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

    [Authorize(Roles ="admin")]
    [HttpGet("admin-only")]

    public IActionResult AdminOnly()
    {
        var userClaims = User.Claims;
        var userType = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        return Ok(new { UserType = userType });
    }
    private string GenerateJwtToken(string username)
    {
        var user = _context.AppUserInfos.FirstOrDefault(u => u.UserName == username);
        if (user == null)
        {
            throw new ArgumentException("Kullanıcı bulunamadı.");
        }
        var claims = new List<Claim>
    {
            new Claim(ClaimTypes.Name, username),
           // new Claim(ClaimTypes.Role, user.UserTypeName ?? "user"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
        if(user.IdentityNumber != null)
        {
            claims.Add(new Claim("IdentityNumber", user.IdentityNumber.Value.ToString()));
        }
        if(user.AuthorityLevel != null)
        {
            claims.Add(new Claim("AuthorityLevel", user.AuthorityLevel.ToString()));
        }
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
