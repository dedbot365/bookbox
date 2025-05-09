using System;

namespace Bookbox.DTOs
{
    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid BookId { get; set; }
        public string? BookTitle { get; set; }
        public string? BookAuthor { get; set; }
        public decimal BookPrice { get; set; }
        public string? BookImageUrl { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; }
    }
}