using Microsoft.EntityFrameworkCore;

namespace JwtTokenProject.Models
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {
            
        }

        public DbSet<AppUserInfo> AppUserInfos { get; set; }
    }
}
