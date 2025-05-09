using System;
using System.Collections.Generic;

namespace Bookbox.DTOs
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalPrice => Items.Sum(i => i.BookPrice * i.Quantity);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}