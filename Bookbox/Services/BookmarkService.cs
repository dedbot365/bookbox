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

        public async Task<BookmarkListDto> GetUserBookmarksAsync(Guid userId)
        {
            var bookmarks = await _context.Bookmarks
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DateAdded)
                .ToListAsync();

            var bookmarkListDto = new BookmarkListDto
            {
                Bookmarks = bookmarks.Select(b => new BookmarkWithDetailsDto
                {
                    BookmarkId = b.BookmarkId,
                    BookId = b.BookId,
                    BookTitle = b.Book.Title,
                    Author = b.Book.Author,
                    CoverImage = b.Book.ImageUrl,
                    Price = b.Book.Price,
                    DateAdded = b.DateAdded
                }).ToList()
            };

            return bookmarkListDto;
        }

        public async Task<BookmarkDto> GetBookmarkByIdAsync(Guid bookmarkId)
        {
            var bookmark = await _context.Bookmarks
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookmarkId == bookmarkId);

            if (bookmark == null)
                return null;

            return new BookmarkDto
            {
                BookmarkId = bookmark.BookmarkId,
                UserId = bookmark.UserId,
                BookId = bookmark.BookId,
                DateAdded = bookmark.DateAdded
            };
        }

        public async Task<BookmarkDto> GetBookmarkAsync(Guid userId, Guid bookId)
        {
            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);

            if (bookmark == null)
                return null;

            return new BookmarkDto
            {
                BookmarkId = bookmark.BookmarkId,
                UserId = bookmark.UserId,
                BookId = bookmark.BookId,
                DateAdded = bookmark.DateAdded
            };
        }

        public async Task<bool> IsBookmarkedByUserAsync(Guid userId, Guid bookId)
        {
            return await _context.Bookmarks
                .AnyAsync(b => b.UserId == userId && b.BookId == bookId);
        }

        public async Task<BookmarkDto> AddBookmarkAsync(Guid userId, CreateBookmarkDto createBookmarkDto)
        {
            // Check if book exists
            bool bookExists = await _context.Books.AnyAsync(b => b.BookId == createBookmarkDto.BookId);
            if (!bookExists)
            {
                throw new ArgumentException("The specified book does not exist.");
            }

            // Check if bookmark already exists
            bool bookmarkExists = await _context.Bookmarks
                .AnyAsync(b => b.UserId == userId && b.BookId == createBookmarkDto.BookId);
            if (bookmarkExists)
            {
                throw new InvalidOperationException("This book is already bookmarked by the user.");
            }

            // Create new bookmark
            var bookmark = new Bookmark
            {
                BookmarkId = Guid.NewGuid(),
                UserId = userId,
                BookId = createBookmarkDto.BookId,
                DateAdded = DateTime.UtcNow
            };

            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            // Return DTO
            return new BookmarkDto
            {
                BookmarkId = bookmark.BookmarkId,
                UserId = bookmark.UserId,
                BookId = bookmark.BookId,
                DateAdded = bookmark.DateAdded
            };
        }
        
        public async Task<bool> ToggleBookmarkAsync(Guid userId, Guid bookId)
        {
            // Check if bookmark exists
            var existingBookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);
            
            if (existingBookmark != null)
            {
                // Remove bookmark if it exists
                _context.Bookmarks.Remove(existingBookmark);
                await _context.SaveChangesAsync();
                return false; // Return false to indicate bookmark was removed
            }
            else
            {
                // Create bookmark if it doesn't exist
                try
                {
                    var createDto = new CreateBookmarkDto { BookId = bookId };
                    await AddBookmarkAsync(userId, createDto);
                    return true; // Return true to indicate bookmark was added
                }
                catch (Exception)
                {
                    return false; // Return false if adding fails
                }
            }
        }

        public async Task<bool> RemoveBookmarkAsync(Guid userId, Guid bookmarkId)
        {
            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.BookmarkId == bookmarkId && b.UserId == userId);
            
            if (bookmark == null)
                return false;
            
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}