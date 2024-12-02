using Avalonia.Controls;
using Avalonia.Interactivity;
using Library.Persistence;
using Library.Service;
using Microsoft.EntityFrameworkCore;
using MsgBox;
using System;
using System.Linq;

namespace Library.Views
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString = $"Server={Environment.MachineName};Database=Library;Integrated Security=True;TrustServerCertificate=True;";

        private readonly AppDbContext _dbContext;

        public MainWindow()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            _dbContext = new AppDbContext(optionsBuilder.Options);
            InitializeComponent();
            WireUpEvents();
        }

        private void WireUpEvents()
        {
            var textBoxes = new[] { UsernameTextBox, PasswordTextBox };
            textBoxes.ToList().ForEach(tb => tb.KeyUp += TextBox_GotFocus);
        }

        private async void Login_ClickAsync(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Text;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user != null && PasswordHelper.Verify(password, user.PasswordHash))
            {
                Window nextWindow = user.Role switch
                {
                    Models.UserRole.Admin => new AdminWindow(_dbContext, user.Id),
                    _ => new BookWindow(_dbContext, user.Id)
                };

                nextWindow.Show();
                Close();
            }
            else
            {
                await MessageBox.Show(this, "Invalid username or password.", "Error", MessageBox.MessageBoxButtons.Ok);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(_dbContext);
            registerWindow.Show();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = IsInputValid();
        }

        private bool IsInputValid()
        {
            return !string.IsNullOrWhiteSpace(UsernameTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(PasswordTextBox.Text) &&
                   UsernameTextBox.Text.Length >= 8 &&
                   PasswordTextBox.Text.Length >= 8;
        }
    }
}
