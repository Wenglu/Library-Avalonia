using Avalonia.Controls;
using Avalonia.Interactivity;
using Library.Models;
using Library.Persistence;
using System.Collections.Generic;
using Library.Service;
using MsgBox;
using System.Linq;
using Avalonia.Input;
using System;
using Library.Views;

namespace Library
{
    public partial class ManageUsersWindow : Window
    {
        private readonly IAppDbContext _dbContext;
        private readonly UserService _userService;
        public IEnumerable<User> Users { get; set; }
        private readonly int UserId;

        public ManageUsersWindow(IAppDbContext dbContext, int userId)
        {
            _dbContext = dbContext;
            InitializeComponent();
            UserId = userId;
            _userService = new UserService(_dbContext);
            LoadData();
            LoadSortOptions();
        }

        private void LoadSortOptions()
        {
            var sortOptions = new[] { "Username", "Email", "Phone Number", "Role" };
            var sortMethods = new[] { "asc", "desc" };

            SortOptionComboBox.ItemsSource = sortOptions;
            SortOptionComboBox.SelectedIndex = 0;
            SortMethodComboBox.ItemsSource = sortMethods;
            SortMethodComboBox.SelectedIndex = 0;
        }

        private void LoadData()
        {
            Users = _dbContext.Users;
            UsersListBox.ItemsSource = null;
            UsersListBox.ItemsSource = Users;
        }

        private void SortOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortOptionComboBox.SelectedItem is string selectedSortOption &&
                SortMethodComboBox.SelectedItem is string selectedMethodOption)
            {
                Func<User, object> keySelector = selectedSortOption switch
                {
                    "Username" => user => user.Username,
                    "Email" => user => user.EmailAddress,
                    "Phone Number" => user => user.PhoneNumber,
                    "Role" => user => user.Role,
                    _ => user => user.Username
                };

                Users = selectedMethodOption == "asc"
                    ? Users.OrderBy(keySelector)
                    : Users.OrderByDescending(keySelector);

                UsersListBox.ItemsSource = Users;
            }
        }

        private async void MakeAdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is User user)
            {
                var dbUser = await _dbContext.Users.FindAsync(user.Id);
                if (dbUser != null)
                {
                    dbUser.Role = UserRole.Admin;
                    await _dbContext.SaveChangesAsync();
                    LoadData();
                }
            }
        }

        private async void MakeUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is User user)
            {
                var dbUser = await _dbContext.Users.FindAsync(user.Id);
                if (dbUser != null)
                {
                    dbUser.Role = UserRole.User;
                    await _dbContext.SaveChangesAsync();
                    if (dbUser.Id != UserId)
                    {
                        LoadData();
                    }
                    else
                    {
                        await MessageBox.Show(this, "GL HF", "[*]", MessageBox.MessageBoxButtons.Ok);
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        Close();
                    }
                }
            }
        }

        private void SearchBooksTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string searchText = textBox.Text?.ToLower() ?? "";
                var usersToShow = Users.Where(user =>
                    user.Username.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    user.Id.ToString() == searchText).ToList();

                UsersListBox.ItemsSource = usersToShow;
            }
        }

        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is User user)
            {
                await _userService.DeleteUser(user.Id);
                await MessageBox.Show(this, "Successfully deleted user", "Success", MessageBox.MessageBoxButtons.Ok);
                LoadData();
            }
        }

        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(_dbContext);
            registerWindow.Closed += (s, args) => LoadData();
            registerWindow.Show();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow(_dbContext, UserId);
            adminWindow.Show();
            Close();
        }
    }
}
