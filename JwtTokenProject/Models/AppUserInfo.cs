using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JwtTokenProject.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(IdentityNumber), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class AppUserInfo
    {
        public int Id { get; set; }
        
        [Required]
        public string? UserName { get; set; } //username

        [Required]
        public long? IdentityNumber { get; set; }
        [DataType(DataType.Password)]
        [Required]
        public string? Password { get; set; }
        public string? UserTypeName { get; set; } //admin-user vs

        public bool IsActive { get; set; }
        public int FailedAttempt { get; set; } //başarısız giriş denemeleri

        public DateTime? LastLoginDate { get; set; }

        public bool RememberMe { get; set; }
        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        public string? Token { get; set; }
        public int AuthorityLevel { get; set; }
    }
}
