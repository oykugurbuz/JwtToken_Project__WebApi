using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JwtTokenProject.Models
{
    public class SignupModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Tc kimlik numarası girilmesi zoruludur.")]
        [Range(10000000000, 99999999999,ErrorMessage ="TC Kimlik numarası 11 haneli olmalıdır.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Kimlik numarası sadece rakamlardan oluşmalıdır.")]
        public long IdentityNumber { get; set; }
        [Required(ErrorMessage ="Email girilmesi zorunludur.")]
        [EmailAddress(ErrorMessage ="Lütfen E-maili doğru formatta giriniz.")]
        public string? Email { get; set; }
       
        [Required(ErrorMessage ="Kullanıcı adı girilmesi zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.")]
        public string? UserName { get; set; }
        [Required(ErrorMessage ="Parola girilmesi zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola en az 6 karakter olmalıdır.")]

        public string? Password { get; set; }
        [Range(1, 4, ErrorMessage = "Yetki seviyesi 1 ile 4 arasında olmalıdır.")]
        public int? AuthorityLevel { get; set; }
    }
}
