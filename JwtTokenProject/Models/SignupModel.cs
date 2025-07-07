using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JwtTokenProject.Models
{
    public class SignupModel
    {
        public int Id { get; set; }

        public long IdentityNumber { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? UserName { get; set; }

        public string? Password { get; set; }
    }
}
