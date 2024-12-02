using Avalonia.Controls;
using Avalonia.Interactivity;
using Library.Models;
using Library.Persistence;
using Library.Services;
using MsgBox;
using System.Collections.Generic;
using System.Linq;

namespace Library;

public partial class EditBookWindow : Window
{
    private readonly int _bookID;
    private readonly IAppDbContext _dbContext;
    private readonly BookService _bookService;
    private readonly int _currentUserId;
    private readonly Book _book;
    private List<BookCategory> _categories;

    public EditBookWindow(int bookId, IAppDbContext dbContext, int userId)
    {
        _currentUserId = userId;
        _dbContext = dbContext;
        _bookService = new BookService(_dbContext, _currentUserId);
        _bookID = bookId;
        _book = _bookService.GetBook(_bookID);
        InitializeComponent();
        WireUpEvents();
        LoadData();
    }

    private void WireUpEvents()
    {
        TitleTextBox.KeyUp += TextBox_KeyUp;
        AuthorTextBox.KeyUp += TextBox_KeyUp;
        CategoryComboBox.SelectionChanged += TextBox_KeyUp;
    }

    private async void LoadData()
    {
        _categories = await _bookService.GetAllCategories();
        CategoryComboBox.ItemsSource = _categories?.Select(cat => cat.Name).ToList();
        TitleTextBox.Text = _book.Title;
        AuthorTextBox.Text = _book.Author;

        CategoryComboBox.SelectedItem = _categories?.FirstOrDefault(cat => cat.Id == _book.CategoryId)?.Name;
    }

    private void TextBox_KeyUp(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        EditButton.IsEnabled = IsBookDataChanged();
    }

    private bool IsBookDataChanged()
    {
        return TitleTextBox.Text != _book.Title ||
               AuthorTextBox.Text != _book.Author ||
               CategoryComboBox.SelectedItem as string != _book.Category.Name;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        OpenAdminWindow();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (CategoryComboBox.SelectedItem is string categoryName)
        {
            var catId = _categories?.FirstOrDefault(c => c.Name == categoryName)?.Id;

            if (catId.HasValue && IsBookDataChanged())
            {
                await _bookService.EditBook(_bookID, TitleTextBox.Text, AuthorTextBox.Text, catId.Value);
                await MessageBox.Show(this, "Book updated successfully", "Success", MessageBox.MessageBoxButtons.Ok);
                OpenAdminWindow();
            }
        }
    }

    private void OpenAdminWindow()
    {
        var adminWindow = new AdminWindow(_dbContext, _currentUserId);
        adminWindow.Show();
        Close();
    }
}
