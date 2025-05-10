using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Bookbox.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Member")]
    public class BookmarkController : Controller
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly IBookService _bookService;
        private readonly IBookFilterService _bookFilterService;

        public BookmarkController(IBookmarkService bookmarkService, 
                                 IBookService bookService,
                                 IBookFilterService bookFilterService)
        {
            _bookmarkService = bookmarkService;
            _bookService = bookService;
            _bookFilterService = bookFilterService;
        }

        // GET: Bookmark - Display all bookmarks with pagination and filtering
        public async Task<IActionResult> Index(string searchTerm = "", string genre = "", string format = "", 
            decimal? minPrice = null, decimal? maxPrice = null, string sortBy = "newest", int page = 1)
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim?.Value) || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            // Get all bookmarked books for the user
            var bookmarks = await _bookmarkService.GetBookmarksByUserIdAsync(userId);
            var bookIds = bookmarks.Select(b => b.BookId).ToList();
            var books = await _bookService.GetBooksByIdsAsync(bookIds);

            // Apply filters
            var filteredBooks = books.AsQueryable();
            filteredBooks = _bookFilterService.ApplyFilters(
                filteredBooks, 
                ViewData, 
                searchTerm, 
                genre, 
                format, 
                null, 
                null, 
                minPrice, 
                maxPrice, 
                null
            );

            // Apply sorting
            filteredBooks = _bookFilterService.ApplySorting(filteredBooks, ViewData, sortBy);
            
            // Set up pagination
            int pageSize = 12;
            int totalItems = filteredBooks.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var paginatedBooks = filteredBooks
                .Skip((Math.Max(1, Math.Min(page, totalPages)) - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            // Set view data for pagination
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            
            // Set filter values for the view
            ViewData["SearchTerm"] = searchTerm;
            ViewData["SelectedGenre"] = genre;
            ViewData["SelectedFormat"] = format;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["SortBy"] = sortBy;
            ViewData["CategoryName"] = "My Bookmarked Books";
            
            // Map bookmarks to books to get the DateAdded for sorting
            ViewData["BookmarkDates"] = bookmarks.ToDictionary(
                b => b.BookId, 
                b => b.DateAdded
            );
            
            return View(paginatedBooks);
        }

        // POST: Bookmark/Add/5 - Add a book to bookmarks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Guid id, string returnUrl = null)
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim?.Value) || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await _bookmarkService.AddBookmarkAsync(id, userId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }
            
            // Redirect back to the original page or to default location
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction("Details", "Book", new { id });
        }

        // POST: Bookmark/Remove/5 - Remove a book from bookmarks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid id, string returnUrl = null)
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim?.Value) || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var result = await _bookmarkService.RemoveBookmarkAsync(id, userId);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }
            
            // Redirect back to the original page or to default location
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction("Index");
        }

        // GET: Bookmark/GetRecentBookmarks - API endpoint for recent bookmarks
        [HttpGet]
        public async Task<IActionResult> GetRecentBookmarks(int count = 5)
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim?.Value) || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var bookmarks = await _bookmarkService.GetRecentBookmarksByUserIdAsync(userId, count);
            return Json(bookmarks);
        }
    }
}