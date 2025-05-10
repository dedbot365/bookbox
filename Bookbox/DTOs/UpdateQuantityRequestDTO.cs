using System;

namespace Bookbox.DTOs
{
    public class UpdateQuantityRequest
    {
        public Guid CartItemId { get; set; }
        public int Change { get; set; }
    }
}