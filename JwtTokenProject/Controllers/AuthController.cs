using JwtTokenProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using System.Text.Json;
using JwtTokenProject.Hubs;
using Microsoft.AspNetCore.SignalR;
using JwtTokenProject.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<UserHub> _hubContext;
    private readonly INotificationServices _notificationServices;
    private readonly IHubContext<ExcelProgressBarHub> _excelProgressBarHub;
    public AuthController(IConfiguration configuration, ApplicationDbContext context,IHubContext<UserHub> hubContext,INotificationServices notificationServices,IHubContext<ExcelProgressBarHub> excelProgressBarHub)
    {
        _configuration = configuration;
        _context = context;
        _hubContext = hubContext;
        _notificationServices = notificationServices;
        _excelProgressBarHub = excelProgressBarHub;
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

        // SignalR ile yeni kullanıcı kaydı bildirimi gönder

         _hubContext.Clients.All.SendAsync("CreatedNewUser", new
        {
            userName = user.UserName,
            email = user.Email
        }); // kullanıcı adı  gönderiyoruz
        // E-posta bildirim servisini kullanarak hoş geldiniz e-postası gönder

        _notificationServices.SendWelcomeEmailAsync(user.Email ?? "none", user.UserName?? "noneUserName");
        return Ok(new { message = "Kullanıcı başarıyla kaydedildi!" });


    }

    

[AllowAnonymous]
[HttpPost("signup-bulk")]
public async Task<IActionResult> SignupBulk()
{
        //***List<SignupModel> tipinde veri gönderdiğinde ASP.NET Core otomatik olarak model validasyonu yapıyor
        //ve JSON içinde tek bir item bile invalid ise metoduna girmeden 400 BadRequest dönüyor.Çözüm:
        //[FromBody] List<SignupModel> users kullanmak yerine HttpContext.Request.Body okuyup jsonı manuel deserialize etmek
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

    if (string.IsNullOrEmpty(body))
        return BadRequest("Gönderilen veri boş.");

    List<SignupModel> users;
    try
    {
        users = JsonSerializer.Deserialize<List<SignupModel>>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch
    {
        return BadRequest("Gönderilen JSON geçersiz.");
    }

    if (users == null || !users.Any())
        return BadRequest("Gönderilen liste boş.");

    var results = new List<UserImportResult>();
       
    for (int i = 0; i < users.Count; i++)
    {
        var signup = users[i];
        var userResult = new UserImportResult
        {
            RowNumber = i + 2,
            User = signup,
            Messages = new List<string>()
        };

    
        var context = new ValidationContext(signup);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(signup, context, validationResults, true))
            userResult.Messages.AddRange(validationResults.Select(v => v.ErrorMessage));

       
        if (_context.AppUserInfos.Any(u => u.UserName == signup.UserName))
            userResult.Messages.Add("Bu kullanıcı adı zaten kayıtlı!");
        if (_context.AppUserInfos.Any(u => u.IdentityNumber == signup.IdentityNumber))
            userResult.Messages.Add("Bu TC kimlik numarası zaten kayıtlı!");
        if (_context.AppUserInfos.Any(u => u.Email == signup.Email))
            userResult.Messages.Add("Bu e-posta adresi zaten kayıtlı!");

       
        if (!userResult.Messages.Any())
        {   
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
                AuthorityLevel = signup.AuthorityLevel ?? 4
            };
            _context.AppUserInfos.Add(user);
               
            }

        results.Add(userResult);

            int percent = ((i + 1) * 100) / users.Count;
            await _excelProgressBarHub.Clients.All.SendAsync("ExcelImportProgress", percent);
            
            await Task.Delay(50); 
        }

    _context.SaveChanges(); // yalnızca hatasızlar DB’ye eklenir

    return Ok(results); 
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
