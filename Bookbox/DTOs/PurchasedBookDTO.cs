using System;

namespace Bookbox.DTOs
{
    public class PurchasedBookDTO
    {
        public Guid BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageUrl { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool IsReviewed { get; set; }
        public ReviewDTO Review { get; set; }
    }
}