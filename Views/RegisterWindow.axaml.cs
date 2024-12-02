using Avalonia.Controls;
using Library.Models;
using Library.Persistence;
using Library.Service;
using MsgBox;
using System.Linq;
using System.Threading.Tasks;

namespace Library
{
    public partial class RegisterWindow : Window
    {
        private readonly IAppDbContext _dbContext;

        public RegisterWindow(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
            InitializeComponent();
            WireUpEvents();
        }

        private void WireUpEvents()
        {
            var textBoxes = new[] { UsernameTextBox, PasswordBox1, PasswordBox2, EmailTextBox, PhoneNumberTextBox };
            textBoxes.ToList().ForEach(tb => tb.KeyUp += TextBox_KeyUp);
        }

        private void TextBox_KeyUp(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var fields = new[]
            {
                UsernameTextBox.Text, PasswordBox1.Text, PasswordBox2.Text, EmailTextBox.Text, PhoneNumberTextBox.Text
            };

            RegisterButton.IsEnabled = fields.Take(3).All(f => ValidateField(f, 8)) &&
                                       fields.Skip(3).All(f => !string.IsNullOrWhiteSpace(f));
        }

        private static bool ValidateField(string fieldValue, int minLength) =>
            !string.IsNullOrWhiteSpace(fieldValue) && fieldValue.Length >= minLength;

        private async void Register_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var fields = new[]
            {
                UsernameTextBox.Text, PasswordBox1.Text, PasswordBox2.Text, EmailTextBox.Text, PhoneNumberTextBox.Text
            };

            if (fields.Any(string.IsNullOrWhiteSpace))
            {
                await ShowMessageBox("Please fill in all fields", "Error");
                return;
            }

            if (fields[1] != fields[2])
            {
                await ShowMessageBox("Different passwords", "Error");
                return;
            }

            if (!UserService.IsValidEmail(fields[3]))
            {
                await ShowMessageBox("Invalid email address", "Error");
                return;
            }

            if (!fields.Take(2).All(f => ValidateField(f, 8)))
            {
                await ShowMessageBox("Minimal length of 8 characters required", "Error");
                return;
            }

            if (_dbContext.UserExists(fields[0], fields[3]))
            {
                await ShowMessageBox("User with that username or email exists", "Error");
                return;
            }

            var newUser = new User
            {
                Username = fields[0],
                PasswordHash = PasswordHelper.HashPas(fields[1]),
                EmailAddress = fields[3],
                PhoneNumber = fields[4]
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            await ShowMessageBox("User registered successfully", "Success");
            Close();
        }

        private async Task ShowMessageBox(string message, string title) =>
            await MessageBox.Show(this, message, title, MessageBox.MessageBoxButtons.Ok);
    }
}
