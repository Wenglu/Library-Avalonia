using Library.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Library.Persistence
{
    public interface IAppDbContext
    {
        DbSet<Book> Books { get; set; }
        DbSet<User> Users { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<CheckOut> CheckOuts { get; set; }

        Task<int> SaveChangesAsync();
        bool UserExists(string? username, string? email);
    }
}