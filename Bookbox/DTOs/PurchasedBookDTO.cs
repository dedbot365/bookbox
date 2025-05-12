using System;

namespace Bookbox.DTOs
{
    public class PurchasedBookDTO
    {
        public Guid BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool IsReviewed { get; set; }
        public ReviewDTO? Review { get; set; }
        public Guid OrderId { get; set; }
        public Guid OrderItemId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
    }
}