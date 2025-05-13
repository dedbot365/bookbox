using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bookbox.DTOs;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Member")]
    public class PurchasedBooksController : Controller
    {
        private readonly IReviewService _reviewService;

        public PurchasedBooksController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index(string sortBy = "newest", int page = 1)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var allPurchasedBooks = await _reviewService.GetPurchasedBooksAsync(userId);
            
            // Apply sorting
            var sortedBooks = sortBy switch
            {
                "oldest" => allPurchasedBooks.OrderBy(b => b.PurchaseDate).ToList(),
                _ => allPurchasedBooks.OrderByDescending(b => b.PurchaseDate).ToList() // default: newest
            };
            
            // Set up pagination
            int pageSize = 6;
            int totalItems = sortedBooks.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure valid page number
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
            
            // Get current page items
            var paginatedBooks = sortedBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            // Store data for the view
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            ViewData["SortBy"] = sortBy;
            
            return View(paginatedBooks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(ReviewDTO reviewDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please ensure all required fields are filled correctly.";
                return RedirectToAction(nameof(Index));
            }

            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _reviewService.AddReviewAsync(userId, reviewDto);
            
            // Get the book title (you'll need to add a method to your service to fetch the book title)
            var bookTitle = await _reviewService.GetBookTitleAsync(reviewDto.BookId);

            if (result)
            {
                // Set TempData for the modal instead of inline alert
                TempData["ReviewUpdated"] = "True";
                TempData["ReviewBookTitle"] = bookTitle;
            }
            else
            {
                TempData["Error"] = "You can only review books from completed orders.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}