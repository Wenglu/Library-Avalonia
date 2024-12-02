using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DynamicData;
using Library.Persistence;
using Library.Services;
using Library.Views;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library
{
    public partial class AdminWindow : Window
    {
        private readonly IAppDbContext _dbContext;
        private readonly int UserId;
        private readonly BookService _bookService;
        public IEnumerable<BookViewModel> AvailableBooks { get; set; }
        public IEnumerable<UserWithBooksViewModel> UsersWithBooks { get; set; }
        public AdminWindow(IAppDbContext dbContext, int userId)
        {
            _dbContext = dbContext;
            _bookService = new BookService(dbContext, userId);
            InitializeComponent();
            UserId = userId;
            LoadData();
            LoadSortOptions();
        }

        public class BookViewModel
        {
            public int ID {  get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Category { get; set; }
            public string CheckOutDate { get; set; } 
            public string DueDate { get; set; }
            public string DaysLeft { get; set; }
            public float Penalty { get; set; }
            public int User_ID { get; set; }
        }

        public class UserWithBooksViewModel
        {
            public int UserId { get; set; }
            public required string UserName { get; set; }
            public required string Email { get; set; }
            public required string PhoneNumber { get; set; }
            public required List<BookViewModel> Books { get; set; }
        }

        private void LoadData()
        {
            AvailableBooks = [.. _dbContext.Books
                .Select(book => new BookViewModel
                {
                    ID = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Category = book.Category.Name
                    
                })];

            UsersWithBooks = [.. _dbContext.Users
            .Where(user => user.CheckOuts.Count != 0)
            .Select(user => new UserWithBooksViewModel
            {
                UserId = user.Id,
                UserName = user.Username,
                Email = user.EmailAddress,
                PhoneNumber = user.PhoneNumber,
                Books = user.CheckOuts.Select(checkout => new BookViewModel
                {
                    ID = checkout.BookId,
                    Title = checkout.Book.Title,
                    Author = checkout.Book.Author,
                    Category = checkout.Book.Category.Name,
                    CheckOutDate = checkout.CheckOutDate.ToString("dd.MM.yyyy"),
                    DueDate = checkout.DueDate.ToString("dd.MM.yyyy"),
                    DaysLeft = checkout.DaysDifference.ToString(),
                    User_ID = user.Id,
                    Penalty = checkout.Penalty,
                }).ToList()

        })];
            AvailableBooksListBox.ItemsSource = AvailableBooks;
            UsersDataGrid.ItemsSource = UsersWithBooks;
        }

        private void LoadSortOptions()
        {
            string[] sortOptions = ["Title", "Author", "Category"];
            string[] sortMethods = ["asc", "desc"];
            SortOptionComboBox.ItemsSource = sortOptions;
            SortOptionComboBox.SelectedIndex = 0;
            SortMethodComboBox.ItemsSource = sortMethods;
            SortMethodComboBox.SelectedIndex = 0;
        }

        private void SortOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if(SortOptionComboBox.SelectedItem != null && SortMethodComboBox.SelectedItem != null)
                {
                    string selectedSortOption = (string)SortOptionComboBox.SelectedItem;
                    string selectedMethodOption = (string)SortMethodComboBox.SelectedItem;

                    switch (selectedSortOption)
                    {
                        case "Title":
                            if (selectedMethodOption == "asc")
                            {
                                UsersWithBooks = UsersWithBooks.OrderBy(user => user.Books.FirstOrDefault()?.Title);
                                AvailableBooks = AvailableBooks.OrderBy(user => user.Title);
                            }
                            else
                            {
                                UsersWithBooks = UsersWithBooks.OrderByDescending(user => user.Books.FirstOrDefault()?.Title);
                                AvailableBooks = AvailableBooks.OrderByDescending(user => user.Title);
                            }
                            break;
                        case "Author":
                            if (selectedMethodOption == "asc")
                            {
                                UsersWithBooks = UsersWithBooks.OrderBy(user => user.Books.FirstOrDefault()?.Author);
                                AvailableBooks = AvailableBooks.OrderBy(user => user.Author);
                            }
                            else
                            {
                                UsersWithBooks = UsersWithBooks.OrderByDescending(user => user.Books.FirstOrDefault()?.Author);
                                AvailableBooks = AvailableBooks.OrderByDescending(user => user.Author);
                            }
                            break;
                        case "Category":
                            if (selectedMethodOption == "asc")
                            {
                                UsersWithBooks = UsersWithBooks.OrderBy(user => user.Books.FirstOrDefault()?.Category);
                                AvailableBooks = AvailableBooks.OrderBy(user => user.Category);
                            }
                            else
                            {
                                UsersWithBooks = UsersWithBooks.OrderByDescending(user => user.Books.FirstOrDefault()?.Category);
                                AvailableBooks = AvailableBooks.OrderByDescending(user => user.Category);
                            }
                            break;
                        default:
                            break;
                    }

                    AvailableBooksListBox.ItemsSource = AvailableBooks;
                    UsersDataGrid.ItemsSource = UsersWithBooks;
                }
                return;
            }
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_dbContext, UserId);
            settingsWindow.Show();

        }

        private async void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var book = (BookViewModel)button.DataContext;
            if (book != null)
            {

                bool isBookBorrowed = UsersWithBooks.Any(user => user.Books.Any(b => b.ID == book.ID));

                if (!isBookBorrowed)
                {
                    _bookService.DeleteBook(book.ID);

                    AvailableBooks = AvailableBooks.Where(b => b.ID != book.ID).ToList();
                    AvailableBooksListBox.ItemsSource = AvailableBooks;
                }
                else
                {
                    var usersWithBook = UsersWithBooks.Where(user => user.Books.Any(b => b.ID == book.ID));
                    var userNames = "";
                    var usersWithIndices = usersWithBook.Select((user, index) => new { User = user, Index = index });

                    foreach (var item in usersWithIndices)
                    {
                        var user = item.User;
                        var index = item.Index;
                        userNames += $"\n{index+1}.{user.UserName}";
                    }

                    await MessageBox.Show(this, $"This book is currently borrowed by users: {userNames}", "Error", MessageBox.MessageBoxButtons.Ok);
                }

            }
            else
            {
                await MessageBox.Show(this, "Book not found", "Error", MessageBox.MessageBoxButtons.Ok);
            }
        }

        private async void EditBook_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var book = (BookViewModel)button.DataContext;
            if( book != null )
            {
                var editBookWindow = new EditBookWindow(book.ID, _dbContext, UserId);
                editBookWindow.Show();
                Close();
            }
            else
            {
                await MessageBox.Show(this, "Book not found", "Error", MessageBox.MessageBoxButtons.Ok);
            }
        }
        private void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            var manageUsersWindow = new ManageUsersWindow(_dbContext,UserId);
            manageUsersWindow.Show();
            Close();
        }

        private async void DeleteUserBook_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var book = (BookViewModel)button.DataContext;
            if (book != null)
            {
                _bookService.DeleteUserBook(book.ID, book.User_ID);
                var userWithBookToRemove = UsersWithBooks.FirstOrDefault(u => u.UserId == book.User_ID);
                if (userWithBookToRemove != null)
                {
                    var bookToRemove = userWithBookToRemove.Books.FirstOrDefault(b => b.ID == book.ID);
                    if (bookToRemove != null)
                    {
                        userWithBookToRemove.Books.Remove(bookToRemove);
                    }

                    if (userWithBookToRemove.Books.Count == 0)
                    {
                        UsersWithBooks = UsersWithBooks.Where(u => u.UserId != userWithBookToRemove.UserId).ToList();
                    }
                }
                UsersDataGrid.ItemsSource = null;
                UsersDataGrid.ItemsSource = UsersWithBooks;
            }
            else
            {
                await MessageBox.Show(this, "Book not found", "Error", MessageBox.MessageBoxButtons.Ok);
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void AddNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            var addNewItemWindow = new AddNewItemWindow(_dbContext, UserId);
            addNewItemWindow.Closed += AddNewItemWindow_Closed;
            addNewItemWindow.Show();
        }

        private void AddNewItemWindow_Closed(object sender, EventArgs e)
        {
            LoadData();
        }

        private void SearchBooksTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string searchText;
                if(!string.IsNullOrEmpty(textBox.Text))
                {
                    searchText = textBox.Text.ToLower();

                }
                else
                {
                    searchText = "";
                }
                AvailableBooks = _bookService.GetAllAvailableBooks()
                    .Where(book => 
                        book.Title.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) || 
                        book.Author.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) || 
                        book.Category.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
                    .Select(book => new BookViewModel
                    {
                        ID = book.Id,
                        Title = book.Title,
                        Author = book.Author,
                        Category = book.Category.Name
                    })
                    .ToList();
                AvailableBooksListBox.ItemsSource = AvailableBooks;

                var UsersWithBooksShow = UsersWithBooks
                .Where(user =>
                    user.UserName.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    user.Books.Any(book =>
                        book.Title.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                        book.Author.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                        book.Category.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)))
                .Select(user =>
                {
                    var filteredBooks = user.Books
                        .Where(book =>
                            book.Title.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                            book.Author.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                            book.Category.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
                        .ToList();

                    return new UserWithBooksViewModel
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Books = filteredBooks
                    };
                })
                .ToList();

            UsersDataGrid.ItemsSource = UsersWithBooksShow;

            }

        }
    }
}
