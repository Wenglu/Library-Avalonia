using Library.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Library.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            if (Database.EnsureCreated())
            {
                Database.Migrate();
                Seed();
            }
        }

        private void Seed()
        {
            var adminUser = Users.FirstOrDefault(u => u.Username == "1");

            if (adminUser == null)
            {
                adminUser = new User
                {
                    Username = "viscaBarca",
                    PasswordHash = "$2a$11$W4aJUlagYJOGpHzrByqGoeCNZp1BeZiY5LuyyT/oQ6Z9Lccg0ZEBW",//12345678
                    Role = UserRole.Admin,
                    PhoneNumber = "12345678",
                    EmailAddress = "admin@admin.pl"
                };

                Users.Add(adminUser);
                SaveChanges();
            }
        }
        public bool UserExists(string username, string email)
        {
            return Users.Any(u => u.Username == username || u.EmailAddress == email);
        }



        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<CheckOut> CheckOuts { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }
    }
}