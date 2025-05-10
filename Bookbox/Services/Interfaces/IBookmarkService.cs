using Bookbox.Models;
using Bookbox.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task<IEnumerable<Bookmark>> GetBookmarksByUserIdAsync(Guid userId);
        Task<IEnumerable<BookmarkDTO>> GetRecentBookmarksByUserIdAsync(Guid userId, int count = 5);
        Task<(bool Success, string Message)> AddBookmarkAsync(Guid bookId, Guid userId);
        Task<(bool Success, string Message)> RemoveBookmarkAsync(Guid bookId, Guid userId);
        Task<bool> IsBookmarkedByUserAsync(Guid bookId, Guid userId);
    }
}