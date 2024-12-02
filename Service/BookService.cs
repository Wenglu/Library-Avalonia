using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Models;
using Library.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    public class BookService(IAppDbContext dbContext, int userId)
    {
        private readonly IAppDbContext _dbContext = dbContext;
        private readonly int _userId = userId;

        public Book GetBook(int bookId)
        {
            return _dbContext.Books.FirstOrDefault(x => x.Id == bookId);

        }

        public List<Book> GetAllAvailableBooksForUser()
        {
            var allBooks = GetAllAvailableBooks();
            var userCheckOuts = _dbContext.CheckOuts.Where(co => co.UserId == _userId).ToList();
            var availableBooks = allBooks.Where(book => !userCheckOuts.Any(co => co.BookId == book.Id)).ToList();
            return availableBooks;
        }

        public List<Book> GetAllAvailableBooks()
        {
            var allBooks = _dbContext.Books.Include(book => book.Category).ToList();
            return allBooks;
        }

        public static List<Book> FilterBooks(IEnumerable<Book> books, string searchText)
        {
            return books.Where(book =>
                book.Title.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                book.Author.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                book.Category.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
            ).ToList();
        }



        public List<Book> GetUserBorrowedBooks()
        {
            var userCheckOuts = _dbContext.CheckOuts.Where(co => co.UserId == _userId).ToList();
            var borrowedBooks = userCheckOuts.Select(co => co.Book).ToList();
            return borrowedBooks;
        }

        public async void DeleteBook(int bookId)
        {
            var bookToRemove = _dbContext.Books.FirstOrDefault(b => b.Id == bookId);
            if (bookToRemove != null)
            {
                _dbContext.Books.Remove(bookToRemove);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task EditBook(int bookId, string newTitle, string newAuthor, int newCategory)
        {
            var bookToEdit = _dbContext.Books.FirstOrDefault(b => b.Id == bookId);
            if (bookToEdit != null)
            {
                bookToEdit.Title = newTitle;
                bookToEdit.Author = newAuthor;
                bookToEdit.CategoryId = newCategory;
                _dbContext.Books.Update(bookToEdit);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async void DeleteUserBook(int bookId,int userId)
        {
            var checkOutToRemove = _dbContext.CheckOuts.FirstOrDefault(c => c.BookId == bookId && c.UserId == userId);
            if (checkOutToRemove != null)
            {
                _dbContext.CheckOuts.Remove(checkOutToRemove);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async void AddBook(Book book)
        {
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();
            
        }

        public async void AddCategory(BookCategory category)
        {
            _dbContext.BookCategories.Add(category);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<List<BookCategory>> GetAllCategories()
        {
            return await _dbContext.BookCategories.ToListAsync();    
        }
    }
}
