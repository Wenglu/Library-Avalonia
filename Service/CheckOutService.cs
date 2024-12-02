using Library.Models;
using Library.Persistence;
using System;

namespace Library.Service
{
    public class CheckOutService(IAppDbContext dbContext)
    {
        private readonly IAppDbContext _dbContext = dbContext;

        public async void AddCheckOut(int bookId, int userId)
        {
            var newCheckOut = new CheckOut
            {
                BookId = bookId,
                CheckOutDate = DateTime.Now,
                DueDate = DateTime.Now.AddMonths(1),
                UserId = userId
            };
            _dbContext.CheckOuts.Add(newCheckOut);
            await _dbContext.SaveChangesAsync();
        }
    }
}
