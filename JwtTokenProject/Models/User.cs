using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JwtTokenProject.Models
{
    public class User: IdentityUser
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }   
    }
}
