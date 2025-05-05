using Microsoft.EntityFrameworkCore;

namespace JwtTokenProject.Models
{
    public class LoginRequest
    {
        public string? UserName { get; set; }

        public string? Password { get; set; }
        
    }
}
