using Bookbox.DTOs;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        // GET: Discount/Create/id
        public async Task<IActionResult> Create(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var model = new DiscountDTO
            {
                BookId = book.BookId,
                BookTitle = book.Title,
                StartDate = DateTime.Today,
                IsOnSale = true
            };
            
            return View(model);
        }

        // POST: Discount/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountDTO discountDTO)
        {
            if (ModelState.IsValid)
            {
                var discount = await _discountService.AddDiscountAsync(discountDTO);
                if (discount != null)
                {
                    TempData["SuccessMessage"] = "Discount added successfully!";
                    return RedirectToAction("Details", "Book", new { id = discountDTO.BookId });
                }
            }

            // If we got this far, something failed; redisplay form
            var book = await _bookService.GetBookByIdAsync(discountDTO.BookId);
            if (book != null)
            {
                discountDTO.BookTitle = book.Title;
            }
            return View(discountDTO);
        }
    }
}