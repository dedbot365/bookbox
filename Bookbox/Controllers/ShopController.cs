using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bookbox.Services.Interfaces;
using Bookbox.Constants;

namespace Bookbox.Controllers
{
    public class ShopController : Controller
    {
        private readonly IBookService _bookService;

        public ShopController(IBookService bookService)
        {
            _bookService = bookService;
        }

        public async Task<IActionResult> Index(string searchTerm = "", string genre = "", string format = "", 
            string publisher = "", string language = "", decimal? minPrice = null, decimal? maxPrice = null, 
            bool? inStock = null, bool? isOnSale = null, string sortBy = "newest", int page = 1)
        {
            int pageSize = 12;
            
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
            
            // Publisher filter
            if (!string.IsNullOrWhiteSpace(publisher))
            {
                filteredBooks = filteredBooks.Where(b => b.Publisher.Contains(publisher, StringComparison.OrdinalIgnoreCase));
                ViewData["SelectedPublisher"] = publisher;
            }
            
            // Language filter
            if (!string.IsNullOrWhiteSpace(language))
            {
                filteredBooks = filteredBooks.Where(b => b.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
                ViewData["SelectedLanguage"] = language;
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
            string category = Request.Query["category"].ToString();
            ViewData["Category"] = category;
            
            switch (category)
            {
                case "bestsellers":
                    filteredBooks = filteredBooks.OrderByDescending(b => b.SalesCount).Take(50);
                    ViewData["CategoryName"] = "Bestsellers";
                    break;
                case "award-winners":
                    filteredBooks = filteredBooks.Where(b => !string.IsNullOrEmpty(b.Awards));
                    ViewData["CategoryName"] = "Award Winners";
                    break;
                case "new-releases":
                    DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
                    filteredBooks = filteredBooks.Where(b => b.PublicationDate >= threeMonthsAgo);
                    ViewData["CategoryName"] = "New Releases";
                    break;
                case "new-arrivals":
                    DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);
                    filteredBooks = filteredBooks.Where(b => b.ArrivalDate >= oneMonthAgo);
                    ViewData["CategoryName"] = "New Arrivals";
                    break;
                case "coming-soon":
                    filteredBooks = filteredBooks.Where(b => b.IsComingSoon);
                    ViewData["CategoryName"] = "Coming Soon";
                    break;
                default:
                    ViewData["CategoryName"] = "All Books";
                    break;
            }
            
            // Apply sorting
            filteredBooks = sortBy switch
            {
                "price_asc" => filteredBooks.OrderBy(b => b.Price),
                "price_desc" => filteredBooks.OrderByDescending(b => b.Price),
                "title" => filteredBooks.OrderBy(b => b.Title),
                "author" => filteredBooks.OrderBy(b => b.Author),
                "bestselling" => filteredBooks.OrderByDescending(b => b.SalesCount),
                "publication_date" => filteredBooks.OrderByDescending(b => b.PublicationDate),
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
    }
}