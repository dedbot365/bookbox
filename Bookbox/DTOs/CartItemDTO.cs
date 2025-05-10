using System;

namespace Bookbox.DTOs
{
    public class CartItemDTO
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
        public BookCartDTO Book { get; set; }
    }
}