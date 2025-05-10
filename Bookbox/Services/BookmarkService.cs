using Bookbox.Data;
using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookbox.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ApplicationDbContext _context;

        public BookmarkService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Bookmark>> GetBookmarksByUserIdAsync(Guid userId)
        {
            return await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DateAdded)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookmarkDTO>> GetRecentBookmarksByUserIdAsync(Guid userId, int count = 5)
        {
            var bookmarks = await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DateAdded)
                .Take(count)
                .Include(b => b.Book)
                .ThenInclude(book => book.Discounts)
                .ToListAsync();
                
            var result = new List<BookmarkDTO>();
            
            foreach (var bookmark in bookmarks)
            {
                var activeDiscount = bookmark.Book?.Discounts
                    .FirstOrDefault(d => d.IsOnSale && 
                        d.StartDate <= DateTime.UtcNow && 
                        (d.EndDate == null || d.EndDate > DateTime.UtcNow));
                
                decimal? discountedPrice = null;
                if (activeDiscount != null)
                {
                    // Calculate discounted price
                    var discount = bookmark.Book.Price * (activeDiscount.DiscountPercentage / 100);
                    discountedPrice = Math.Round(bookmark.Book.Price - discount, 2);
                }
                
                result.Add(new BookmarkDTO
                {
                    BookmarkId = bookmark.BookmarkId,
                    BookId = bookmark.BookId,
                    UserId = bookmark.UserId,
                    DateAdded = bookmark.DateAdded,
                    Book = new BookmarkBookDTO
                    {
                        BookId = bookmark.Book.BookId,
                        Title = bookmark.Book.Title,
                        Author = bookmark.Book.Author,
                        Price = bookmark.Book.Price,
                        DiscountedPrice = discountedPrice,
                        IsOnSale = activeDiscount != null,
                        ImageUrl = bookmark.Book.ImageUrl
                    }
                });
            }
            
            return result;
        }

        public async Task<(bool Success, string Message)> AddBookmarkAsync(Guid bookId, Guid userId)
        {
            // Check if book exists
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return (false, "Book not found.");
            }

            // Check if already bookmarked
            var existingBookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId);
                
            if (existingBookmark != null)
            {
                return (false, "Book is already in your wishlist.");
            }

            // Create new bookmark
            var bookmark = new Bookmark
            {
                BookId = bookId,
                UserId = userId,
                DateAdded = DateTime.UtcNow
            };

            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();

            return (true, "Book added to your wishlist successfully.");
        }

        public async Task<(bool Success, string Message)> RemoveBookmarkAsync(Guid bookId, Guid userId)
        {
            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId);

            if (bookmark == null)
            {
                return (false, "Book is not in your wishlist.");
            }

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            return (true, "Book removed from your wishlist.");
        }

        public async Task<bool> IsBookmarkedByUserAsync(Guid bookId, Guid userId)
        {
            return await _context.Bookmarks
                .AnyAsync(b => b.BookId == bookId && b.UserId == userId);
        }
    }
}