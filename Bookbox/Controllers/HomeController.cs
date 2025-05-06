using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Bookbox.Constants;

namespace Bookbox.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBookService _bookService;

    public HomeController(ILogger<HomeController> logger, IBookService bookService)
    {
        _logger = logger;
        _bookService = bookService;
    }

    public async Task<IActionResult> Index(string searchTerm = "", string genre = "", string format = "", 
        decimal? minPrice = null, decimal? maxPrice = null, string sortBy = "newest")
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Dashboard", "Admin");
            else
                return RedirectToAction("Index", "Member");
        }
        
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
        
        // Apply sorting
        filteredBooks = sortBy switch
        {
            "price_asc" => filteredBooks.OrderBy(b => b.Price),
            "price_desc" => filteredBooks.OrderByDescending(b => b.Price),
            "title" => filteredBooks.OrderBy(b => b.Title),
            "author" => filteredBooks.OrderBy(b => b.Author),
            _ => filteredBooks.OrderByDescending(b => b.ArrivalDate) // Default: newest
        };
        
        ViewData["SortBy"] = sortBy;
        
        // Take the 10 most recent books after filtering
        var displayBooks = filteredBooks.Take(10).ToList();
        
        return View(displayBooks);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
