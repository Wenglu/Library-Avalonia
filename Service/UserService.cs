using Library.Models;
using Library.Persistence;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Service
{
    public class UserService(IAppDbContext dbContext)
    {
        private readonly IAppDbContext _dbContext = dbContext;

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Contains('@') && email.Contains('.');
            }
            catch
            {
                return false;
            }
        }

        public async Task DeleteUser(int userId)
        {
            var userToRemove = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (userToRemove != null)
            {
                _dbContext.Users.Remove(userToRemove);
                await _dbContext.SaveChangesAsync();
            }
        }

        public User FindUser(int userId)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        }

        public async Task UpdateUser(int userId, string userName, string email, string phoneNumber)
        {
            var user = FindUser(userId);
            user.Username = userName;
            user.EmailAddress = email;
            user.PhoneNumber = phoneNumber;
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdatePasswordUser(int userId, string password)
        {
            var user = FindUser(userId);
            user.PasswordHash = PasswordHelper.HashPas(password);
            await _dbContext.SaveChangesAsync();
        }

    }
}
