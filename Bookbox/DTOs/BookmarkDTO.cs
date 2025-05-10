using System;

namespace Bookbox.DTOs
{
    public class BookmarkDTO
    {
        public Guid BookmarkId { get; set; }
        public Guid BookId { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateAdded { get; set; }
        public BookmarkBookDTO Book { get; set; } = null!;
    }
}