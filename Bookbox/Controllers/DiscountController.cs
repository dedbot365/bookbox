using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DiscountController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly IBookService _bookService;

        public DiscountController(IDiscountService discountService, IBookService bookService)
        {
            _discountService = discountService;
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid bookId)
        {
            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book == null)
                return NotFound();

            var existingDiscount = await _discountService.GetActiveDiscountForBookAsync(bookId);
            if (existingDiscount != null)
            {
                // Redirect to edit if discount exists
                return RedirectToAction("Edit", new { id = existingDiscount.DiscountId });
            }

            ViewBag.Book = book;
            return View(new Discount { 
                BookId = bookId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30) // Default 30 day duration
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discount discount)
        {
            if (!ModelState.IsValid)
            {
                var book = await _bookService.GetBookByIdAsync(discount.BookId);
                ViewBag.Book = book;
                return View(discount);
            }

            await _discountService.CreateDiscountAsync(discount);
            
            // Change redirect to Book/Index and update success message
            var updatedBook = await _bookService.GetBookByIdAsync(discount.BookId);
            TempData["SuccessMessage"] = $"Discount for '{updatedBook?.Title}' added successfully!";
            return RedirectToAction("Index", "Book");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            if (discount == null)
                return NotFound();

            ViewBag.Book = discount.Book;
            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Discount discount)
        {
            if (!ModelState.IsValid)
            {
                var book = await _bookService.GetBookByIdAsync(discount.BookId);
                ViewBag.Book = book;
                return View(discount);
            }

            await _discountService.UpdateDiscountAsync(discount);
            
            // Change redirect to Book/Index and update success message
            var updatedBook = await _bookService.GetBookByIdAsync(discount.BookId);
            TempData["SuccessMessage"] = $"Discount for '{updatedBook?.Title}' updated successfully!";
            return RedirectToAction("Index", "Book");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid bookId)
        {
            // Get book details before deletion for the success message
            var book = await _bookService.GetBookByIdAsync(bookId);
            string bookTitle = book?.Title ?? "Unknown book";
            
            await _discountService.DeleteDiscountAsync(id);
            
            // Change redirect to Book/Index and update success message
            TempData["SuccessMessage"] = $"Discount for '{bookTitle}' removed successfully!";
            return RedirectToAction("Index", "Book");
        }
    }
}