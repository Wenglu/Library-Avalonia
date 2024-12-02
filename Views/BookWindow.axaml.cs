using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Library.Models;
using Library.Persistence;
using Library.Service;
using Library.Services;
using Library.Views;
using System.Collections.Generic;
using System.Linq;

namespace Library
{
    public partial class BookWindow : Window
    {
        private readonly IAppDbContext _dbContext;
        private readonly BookService _bookService;
        private readonly CheckOutService _checkOutService;
        public IEnumerable<Book>? AvailableBooks { get; set; }
        public IEnumerable<Book>? UserBooks { get; set; }
        private readonly int _currentUserId;

        public BookWindow(IAppDbContext dbContext, int currentUserId)
        {
            _dbContext = dbContext;
            _bookService = new BookService(dbContext, currentUserId);
            _checkOutService = new CheckOutService(dbContext);
            InitializeComponent();
            _currentUserId = currentUserId;
            LoadData();
            LoadSortOptions();
        }

        private void LoadData()
        {
            AvailableBooks = _bookService.GetAllAvailableBooksForUser();
            UserBooks = _bookService.GetUserBorrowedBooks();
            AvailableBooksListBox.ItemsSource = AvailableBooks;
            UserBooksListBox.ItemsSource = UserBooks;
        }

        private void SearchBooksTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string searchText = string.IsNullOrEmpty(textBox.Text) ? "" : textBox.Text.ToLower();

                AvailableBooks = BookService.FilterBooks(_bookService.GetAllAvailableBooksForUser(), searchText);
                AvailableBooksListBox.ItemsSource = AvailableBooks;

                UserBooks = BookService.FilterBooks(_bookService.GetUserBorrowedBooks(), searchText);
                UserBooksListBox.ItemsSource = UserBooks;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
            ClearCachedData();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_dbContext, _currentUserId);
            settingsWindow.Show();
        }

        private void LoadSortOptions()
        {
            string[] sortOptions = { "Title", "Author", "Category" };
            string[] sortMethods = { "asc", "desc" };
            SortOptionComboBox.ItemsSource = sortOptions;
            SortOptionComboBox.SelectedIndex = 0;
            SortMethodComboBox.ItemsSource = sortMethods;
            SortMethodComboBox.SelectedIndex = 0;
        }

        private void SortOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (SortOptionComboBox.SelectedItem != null && SortMethodComboBox.SelectedItem != null && AvailableBooks != null && UserBooks != null)
                {
                    string selectedSortOption = (string)SortOptionComboBox.SelectedItem;
                    string selectedMethodOption = (string)SortMethodComboBox.SelectedItem;

                    SortBooks(selectedSortOption, selectedMethodOption);
                }
            }
        }

        private void SortBooks(string sortOption, string sortMethod)
        {
            switch (sortOption)
            {
                case "Title":
                    SortByTitle(sortMethod);
                    break;
                case "Author":
                    SortByAuthor(sortMethod);
                    break;
                case "Category":
                    SortByCategory(sortMethod);
                    break;
                default:
                    break;
            }

            AvailableBooksListBox.ItemsSource = AvailableBooks;
            UserBooksListBox.ItemsSource = UserBooks;
        }

        private void SortByTitle(string method)
        {
            if (method == "asc")
            {
                AvailableBooks = AvailableBooks.OrderBy(book => book.Title).ToList();
                UserBooks = UserBooks.OrderBy(book => book.Title).ToList();
            }
            else
            {
                AvailableBooks = AvailableBooks.OrderByDescending(book => book.Title).ToList();
                UserBooks = UserBooks.OrderByDescending(book => book.Title).ToList();
            }
        }

        private void SortByAuthor(string method)
        {
            if (method == "asc")
            {
                AvailableBooks = AvailableBooks.OrderBy(book => book.Author).ToList();
                UserBooks = UserBooks.OrderBy(book => book.Author).ToList();
            }
            else
            {
                AvailableBooks = AvailableBooks.OrderByDescending(book => book.Author).ToList();
                UserBooks = UserBooks.OrderByDescending(book => book.Author).ToList();
            }
        }

        private void SortByCategory(string method)
        {
            if (method == "asc")
            {
                AvailableBooks = AvailableBooks.OrderBy(book => book.Category).ToList();
                UserBooks = UserBooks.OrderBy(book => book.Category).ToList();
            }
            else
            {
                AvailableBooks = AvailableBooks.OrderByDescending(book => book.Category).ToList();
                UserBooks = UserBooks.OrderByDescending(book => book.Category).ToList();
            }
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Book book)
            {
                _checkOutService.AddCheckOut(book.Id, _currentUserId);

                var selectedBook = AvailableBooks?.FirstOrDefault(b => b.Id == book.Id);
                if (selectedBook != null)
                {
                    AvailableBooks = AvailableBooks.Where(b => b.Id != selectedBook.Id).ToList();
                    AvailableBooksListBox.ItemsSource = AvailableBooks;

                    var userBookList = UserBooks?.ToList();
                    userBookList?.Add(selectedBook);
                    UserBooks = userBookList;
                    UserBooksListBox.ItemsSource = UserBooks;
                }
            }
        }

        private void ClearCachedData()
        {
            AvailableBooks = null;
            UserBooks = null;
            AvailableBooksListBox.ItemsSource = null;
            UserBooksListBox.ItemsSource = null;
        }
    }
}
