using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bookbox.Constants;  // Add this import for Genre and Format enums

namespace Bookbox.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET: Book
        public async Task<IActionResult> Index(string searchTerm = "", string genre = "", string format = "", 
            decimal? minPrice = null, decimal? maxPrice = null, bool? inStock = null, string sortBy = "newest", 
            string category = "", int page = 1)
        {
            int pageSize = 12; // Match the page size used in ShopController
            
            // Get all books for filtering
            var allBooks = await _bookService.GetAllBooksAsync();
            
            // Apply filters
            var filteredBooks = allBooks.AsQueryable();
            
            // Text search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredBooks = filteredBooks.Where(b => 
                    b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    b.ISBN.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    b.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                    
                ViewData["SearchTerm"] = searchTerm;
            }
            
            // Genre filter
            if (!string.IsNullOrWhiteSpace(genre) && Enum.TryParse<Genre>(genre, out var genreEnum))
            {
                filteredBooks = filteredBooks.Where(b => b.Genre == genreEnum);
                ViewData["SelectedGenre"] = genre;
            }
            
            // Format filter
            if (!string.IsNullOrWhiteSpace(format) && Enum.TryParse<Format>(format, out var formatEnum))
            {
                filteredBooks = filteredBooks.Where(b => b.Format == formatEnum);
                ViewData["SelectedFormat"] = format;
            }
            
            // Price range
            if (minPrice.HasValue)
            {
                filteredBooks = filteredBooks.Where(b => b.Price >= minPrice.Value);
                ViewData["MinPrice"] = minPrice.Value;
            }
            
            if (maxPrice.HasValue)
            {
                filteredBooks = filteredBooks.Where(b => b.Price <= maxPrice.Value);
                ViewData["MaxPrice"] = maxPrice.Value;
            }
            
            // Stock availability
            if (inStock.HasValue && inStock.Value)
            {
                filteredBooks = filteredBooks.Where(b => b.Stock > 0);
                ViewData["InStock"] = true;
            }

            // Category filters for special collections
            ViewData["Category"] = category;
            
            switch (category)
            {
                case "bestsellers":
                    filteredBooks = filteredBooks.OrderByDescending(b => b.SalesCount).Take(50);
                    break;
                case "award-winners":
                    filteredBooks = filteredBooks.Where(b => !string.IsNullOrEmpty(b.Awards));
                    break;
                case "new-releases":
                    DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
                    filteredBooks = filteredBooks.Where(b => b.PublicationDate >= threeMonthsAgo);
                    break;
                case "new-arrivals":
                    DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);
                    filteredBooks = filteredBooks.Where(b => b.ArrivalDate >= oneMonthAgo);
                    break;
                case "coming-soon":
                    filteredBooks = filteredBooks.Where(b => b.IsComingSoon);
                    break;
            }
            
            // Apply sorting
            filteredBooks = sortBy switch
            {
                "price_asc" => filteredBooks.OrderBy(b => b.Price),
                "price_desc" => filteredBooks.OrderByDescending(b => b.Price),
                "title" => filteredBooks.OrderBy(b => b.Title),
                "bestselling" => filteredBooks.OrderByDescending(b => b.SalesCount),
                _ => filteredBooks.OrderByDescending(b => b.ArrivalDate) // Default: newest
            };
            
            ViewData["SortBy"] = sortBy;
            
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
        public async Task<IActionResult> Details(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

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
                    TempData["SuccessMessage"] = "Book added successfully!";
                    return RedirectToAction(nameof(Details), new { id = book!.BookId });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                // Rest of your existing code...
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
        [Authorize(Roles = "Admin")]
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
                // ImageFile will be null here - can't map back from URL to file
                UserId = book.UserId 
            };

            ViewData["CurrentImageUrl"] = book.ImageUrl;
            return View(bookDTO);
        }

        // POST: Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, BookDTO bookDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var book = await _bookService.UpdateBookAsync(id, bookDTO);
                    if (book != null)
                    {
                        TempData["SuccessMessage"] = "Book updated successfully!";
                        return RedirectToAction(nameof(Details), new { id = book.BookId });
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

        // POST: Book/Delete/5
        /*[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _bookService.DeleteBookAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Book deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Book not found.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed to delete the book.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }*/

        // GET: Book/Search
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
        }
    }
}