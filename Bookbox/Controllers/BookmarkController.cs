using Bookbox.DTOs;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookbox.Controllers
{
    [Authorize] // Ensure only authenticated users can access
    public class BookmarkController : Controller
    {
        private readonly IBookmarkService _bookmarkService;

        public BookmarkController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        // GET: Bookmark
       

        public async Task<IActionResult> MyBookmarks()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var bookmarks = await _bookmarkService.GetUserBookmarksAsync(userId);
            return View(bookmarks);
        }

        // POST: Bookmark/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CreateBookmarkDto model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", "Book", new { id = model.BookId });
            }

            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _bookmarkService.AddBookmarkAsync(userId, model);
                TempData["SuccessMessage"] = "Book added to your bookmarks!";
            }
            catch (InvalidOperationException)
            {
                TempData["InfoMessage"] = "This book is already in your bookmarks.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding bookmark: {ex.Message}";
            }

            return RedirectToAction("Details", "Book", new { id = model.BookId });
        }

        // POST: Bookmark/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid bookmarkId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var success = await _bookmarkService.RemoveBookmarkAsync(userId, bookmarkId);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Book removed from your bookmarks.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not remove the bookmark.";
            }

            return RedirectToAction(nameof(MyBookmarks));
        }

        // POST: Bookmark/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(Guid bookId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isBookmarked = await _bookmarkService.ToggleBookmarkAsync(userId, bookId);
            
            if (isBookmarked)
            {
                TempData["SuccessMessage"] = "Book added to your bookmarks!";
            }
            else
            {
                TempData["SuccessMessage"] = "Book removed from your bookmarks.";
            }

            return RedirectToAction("Details", "Book", new { id = bookId });
        }

        // For checking bookmark status via AJAX (optional)
        [HttpGet]
        public async Task<IActionResult> CheckStatus(Guid bookId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isBookmarked = await _bookmarkService.IsBookmarkedByUserAsync(userId, bookId);
            return Json(new { isBookmarked });
        }
    }
}