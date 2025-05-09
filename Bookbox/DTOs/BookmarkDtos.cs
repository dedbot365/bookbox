using System;
using System.Collections.Generic;

namespace Bookbox.DTOs
{
    // DTO for creating a new bookmark
    public class CreateBookmarkDto
    {
        public Guid BookId { get; set; }
    }

    // DTO for returning basic bookmark information
    public class BookmarkDto
    {
        public Guid BookmarkId { get; set; }
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public DateTime DateAdded { get; set; }
    }

    // DTO for returning bookmark with book details
    public class BookmarkWithDetailsDto
    {
        public Guid BookmarkId { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; }
        public string Author { get; set; }
        public string CoverImage { get; set; }
        public decimal Price { get; set; }
        public DateTime DateAdded { get; set; }
    }

    // DTO for returning a collection of bookmarks
    public class BookmarkListDto
    {
        public List<BookmarkWithDetailsDto> Bookmarks { get; set; } = new List<BookmarkWithDetailsDto>();
    }
}