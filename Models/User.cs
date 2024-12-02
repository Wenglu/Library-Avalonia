using System.Collections.Generic;

namespace Library.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public ICollection<CheckOut> CheckOuts { get; set; }

        public bool IsAdmin => Role == UserRole.Admin;
    }
}
