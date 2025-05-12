using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bookbox.DTOs;
using Bookbox.Services.Interfaces; // Make sure this namespace is correct
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<IActionResult> Index()
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var purchasedBooks = await _reviewService.GetPurchasedBooksAsync(userId);
            return View(purchasedBooks);
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

            if (result)
            {
                TempData["Success"] = "Your review has been submitted successfully.";
            }
            else
            {
                TempData["Error"] = "You can only review books from completed orders.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}