namespace JwtTokenProject.Models
{
    public class UserImportResult
    {
        public bool IsSuccess { get; set; } = true;

        public List<string> Messages { get; set; } = new List<string>();

        public int RowNumber { get; set; } //hata hangi satırda 

        public SignupModel? User { get; set; }
    }
}
