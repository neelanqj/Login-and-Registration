using Login_and_Registration.Models;
using Microsoft.EntityFrameworkCore;

namespace Login_and_Registration.Persistence
{
    public class Login_and_RegistrationDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        
        public Login_and_RegistrationDbContext(DbContextOptions<Login_and_RegistrationDbContext> options)
            : base(options)
        {
            
        }
    }
}