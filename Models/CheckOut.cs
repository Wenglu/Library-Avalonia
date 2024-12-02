using System;
using System.Data.SqlTypes;

namespace Library.Models
{
    public class CheckOut
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public DateTime CheckOutDate { get; set; }
        public DateTime DueDate { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public Book Book { get; set; }
        public int DaysDifference => (DueDate - DateTime.Today).Days;
        public float Penalty
        {
            get
            {
                int daysDifference = DaysDifference;
                if (daysDifference < 0)
                {
                    float penaltyPerDay = 15.0f;
                    return Math.Abs(daysDifference) * penaltyPerDay;
                }
                else
                {
                    return 0.0f;
                }
            }
        }
    }
}
