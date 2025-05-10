using System;

namespace Bookbox.DTOs
{
    public class BookCartDTO
    {
        public Guid BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsOnSale { get; set; }
        public decimal? DiscountedPrice { get; set; }
    }
}