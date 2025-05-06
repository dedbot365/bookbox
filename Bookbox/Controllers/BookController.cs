using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllBooksAsync();
            return View(books);
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
                    var book = await _bookService.AddBookAsync(bookDTO);
                    TempData["SuccessMessage"] = "Book added successfully!";
                    return RedirectToAction(nameof(Details), new { id = book.BookId });
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
                PhysicalStock = book.PhysicalStock
                // ImageFile will be null here - can't map back from URL to file
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
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
        }

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