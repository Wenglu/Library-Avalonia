using System.Collections.Generic;

namespace Library.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int CategoryId { get; set; }
        public BookCategory Category { get; set; }
        public ICollection<CheckOut> CheckOuts { get; set; }
    }
}
