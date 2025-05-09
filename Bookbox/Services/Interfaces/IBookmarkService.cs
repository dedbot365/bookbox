using Bookbox.DTOs;
using Bookbox.Models;
using System;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task<BookmarkListDto> GetUserBookmarksAsync(Guid userId);
        Task<BookmarkDto> GetBookmarkByIdAsync(Guid bookmarkId);
        Task<BookmarkDto> GetBookmarkAsync(Guid userId, Guid bookId);
        Task<bool> IsBookmarkedByUserAsync(Guid userId, Guid bookId);
        Task<BookmarkDto> AddBookmarkAsync(Guid userId, CreateBookmarkDto createBookmarkDto);
        Task<bool> ToggleBookmarkAsync(Guid userId, Guid bookId);
        Task<bool> RemoveBookmarkAsync(Guid userId, Guid bookmarkId);
    }
}