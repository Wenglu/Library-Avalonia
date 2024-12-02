using Avalonia.Controls;
using Library.Models;
using Library.Persistence;
using Library.Service;
using MsgBox;
using Tmds.DBus.Protocol;
using System.Linq;

namespace Library
{
    public partial class SettingsWindow : Window
    {
        private readonly int userID;
        private readonly IAppDbContext _dbContext;
        private readonly UserService _userService;
        private readonly User _user;

        public SettingsWindow(IAppDbContext dbContext, int userId)
        {
            _dbContext = dbContext;
            userID = userId;
            _userService = new UserService(_dbContext);
            _user = _userService.FindUser(userID);
            InitializeComponent();
            LoadData();
            WireUpEvents();
        }

        private void LoadData()
        {
            UsernameTextBox.Text = _user.Username;
            EmailTextBox.Text = _user.EmailAddress;
            PhoneNumberTextBox.Text = _user.PhoneNumber;
        }

        private void WireUpEvents()
        {
            var textBoxes = new[] { UsernameTextBox, EmailTextBox, PhoneNumberTextBox };
            var passwordBoxes = new[] { PasswordBox1, PasswordBox2, PasswordBox3 };

            textBoxes.ToList().ForEach(tb => tb.KeyUp += TextBox_KeyUp);
            passwordBoxes.ToList().ForEach(pb => pb.KeyUp += PasswordBox_KeyUp);
        }

        private void TextBox_KeyUp(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var textBoxes = new[] { UsernameTextBox, EmailTextBox, PhoneNumberTextBox };
            if (textBoxes.All(tb => tb.Text != null))
            {
                var isTextChanged = textBoxes.Any(tb =>
                    (tb.Name == nameof(UsernameTextBox) && tb.Text != _user.Username) ||
                    (tb.Name == nameof(EmailTextBox) && tb.Text != _user.EmailAddress) ||
                    (tb.Name == nameof(PhoneNumberTextBox) && tb.Text != _user.PhoneNumber));

                var isValidLength = textBoxes.All(tb => tb.Text.Length >= 8);

                UpdateButton.IsEnabled = isTextChanged && isValidLength;
            }
        }

        private void PasswordBox_KeyUp(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var passwordBoxes = new[] { PasswordBox1, PasswordBox2, PasswordBox3 };
            if (passwordBoxes.All(pb => pb.Text != null))
            {
                var isValidPassword = PasswordBox1.Text.Length >= 8 &&
                                      PasswordBox2.Text == PasswordBox1.Text &&
                                      PasswordBox3.Text.Length >= 8;

                UpdatePasswordButton.IsEnabled = isValidPassword;
            }
        }

        private async void UpdateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var userName = UsernameTextBox.Text;
            var email = EmailTextBox.Text;
            var phoneNumber = PhoneNumberTextBox.Text;

            if (new[] { userName, email, phoneNumber }.All(field => field != null))
            {
                await _userService.UpdateUser(userID, userName, email, phoneNumber);
                await MessageBox.Show(this, "Successfully changed user data", "Success", MessageBox.MessageBoxButtons.Ok);
                Close();
            }
            else
            {
                await MessageBox.Show(this, "Error during update", "Error", MessageBox.MessageBoxButtons.Ok);
            }
        }

        private void BackButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();

        private async void UpdatePasswordButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var newPassword = PasswordBox1.Text;
            var oldPassword = PasswordBox3.Text;

            if (new[] { newPassword, oldPassword }.Any(pwd => pwd == null))
                return;

            if (newPassword == oldPassword)
            {
                await MessageBox.Show(this, "Same Password", "Error", MessageBox.MessageBoxButtons.Ok);
            }
            else if (PasswordHelper.Verify(oldPassword, _user.PasswordHash))
            {
                await _userService.UpdatePasswordUser(userID, newPassword);
                await MessageBox.Show(this, "Successfully changed password", "Success", MessageBox.MessageBoxButtons.Ok);
                Close();
            }
            else
            {
                await MessageBox.Show(this, "Wrong Password", "Error", MessageBox.MessageBoxButtons.Ok);
            }
        }
    }
}
