using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bookbox.Constants;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bookbox.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IBookFilterService _filterService;
        private readonly IReviewService _reviewService;

        public BookController(IBookService bookService, IBookFilterService filterService, IReviewService reviewService)
        {
            _bookService = bookService;
            _filterService = filterService;
            _reviewService = reviewService;
        }

        // GET: Book
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index(string searchTerm = "", string genre = "", string format = "", 
            decimal? minPrice = null, decimal? maxPrice = null, bool? inStock = null, string sortBy = "newest", 
            string category = "", int page = 1)
        {
            int pageSize = 9;
            
            // Get all books for filtering
            var allBooks = await _bookService.GetAllBooksAsync();
            
            // Apply filters through the filter service
            var filteredBooks = allBooks.AsQueryable();
            filteredBooks = _filterService.ApplyFilters(filteredBooks, ViewData, searchTerm, genre, format, "", "", minPrice, maxPrice, inStock);
            
            // Category filters
            filteredBooks = _filterService.ApplyCategory(filteredBooks, ViewData, category);
            
            // Apply sorting
            filteredBooks = _filterService.ApplySorting(filteredBooks, ViewData, sortBy);
            
            // Pagination
            int totalItems = filteredBooks.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var paginatedBooks = filteredBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            
            return View(paginatedBooks);
        }

        // GET: Book/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            // Get review statistics
            ViewData["AverageRating"] = await _reviewService.GetAverageRatingForBookAsync(id);
            ViewData["ReviewCount"] = await _reviewService.GetReviewCountForBookAsync(id);
            ViewData["RecentReviews"] = await _reviewService.GetRecentReviewsForBookAsync(id, 5);

            // Get recommended books of same genre (excluding current book)
            var allBooks = await _bookService.GetAllBooksAsync();
            var recommendedBooks = allBooks
                .Where(b => b.Genre == book.Genre && b.BookId != book.BookId)
                .OrderBy(b => Guid.NewGuid()) // Random order
                .Take(4) // Take 4 books
                .ToList();
            ViewData["RecommendedBooks"] = recommendedBooks;

            return View(book);
        }

        // GET: Book/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(BookDTO bookDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current user's ID from the claims
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    {
                        ModelState.AddModelError("", "User identity could not be determined.");
                        return View(bookDTO);
                    }
                    
                    var book = await _bookService.AddBookAsync(bookDTO, userId);
                    TempData["SuccessMessage"] = $"Book '{book.Title}' added successfully!";
                    
                    // Redirect to Index instead of Details
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while adding the book.");
                }
            }
            return View(bookDTO);
        }

        // GET: Book/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            // Map Book to BookDTO
            var bookDTO = new BookDTO
            {
                Title = book.Title,
                Author = book.Author,
                Genre = book.Genre,
                Price = book.Price,
                Format = book.Format,
                Publisher = book.Publisher,
                ISBN = book.ISBN,
                Description = book.Description,
                Stock = book.Stock,
                Language = book.Language,
                Awards = book.Awards,
                PhysicalStock = book.PhysicalStock,
                UserId = book.UserId,
                IsComingSoon = book.IsComingSoon,
                PublicationDate = book.PublicationDate
            };

            ViewData["CurrentImageUrl"] = book.ImageUrl;
            return View(bookDTO);
        }

        // POST: Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(Guid id, BookDTO bookDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var book = await _bookService.UpdateBookAsync(id, bookDTO);
                    if (book != null)
                    {
                        TempData["SuccessMessage"] = $"Book '{book.Title}' updated successfully!";
                        return RedirectToAction(nameof(Index)); // Redirect to Index instead of Details
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while updating the book.");
                }
            }
            
            // If we got this far, something failed, redisplay form
            var existingBook = await _bookService.GetBookByIdAsync(id);
            if (existingBook != null)
                ViewData["CurrentImageUrl"] = existingBook.ImageUrl;
                
            return View(bookDTO);
        }

        // GET: Book/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction(nameof(Index));
            }

            // Delete the book directly
            var result = await _bookService.DeleteBookAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Book deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the book.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Book/Search
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var allBooks = await _bookService.GetAllBooksAsync();
            var results = allBooks.Where(b => 
                b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.ISBN.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            ViewData["SearchTerm"] = searchTerm;
            return View("Index", results);
        }        [HttpGet]
        public async Task<IActionResult> GetAllReviews(Guid id, int count = 10)
        {
            try
            {
                // Ensure reviews are being returned correctly with all required fields
                var reviews = await _reviewService.GetRecentReviewsForBookAsync(id, count);
                
                // Log the count for debugging
                Console.WriteLine($"Found {reviews.Count} reviews for book {id}");
                
                return Json(reviews);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error getting reviews: {ex.Message}");
                return Json(new List<ReviewDTO>());
            }
        }
    }
}