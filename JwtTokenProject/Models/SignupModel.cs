using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JwtTokenProject.Models
{
    public class SignupModel
    {
        public int Id { get; set; }

        [Required]
        public long IdentityNumber { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? UserName { get; set; }

        public string? Password { get; set; }
    }
}
