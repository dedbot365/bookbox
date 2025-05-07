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
        private readonly IBookFilterService _filterService;

        public ShopController(IBookService bookService, IBookFilterService filterService)
        {
            _bookService = bookService;
            _filterService = filterService;
        }

        public async Task<IActionResult> Index(string searchTerm = "", string genre = "", string format = "", 
            string publisher = "", string language = "", decimal? minPrice = null, decimal? maxPrice = null, 
            bool? inStock = null, bool? isOnSale = null, string sortBy = "newest", int page = 1)
        {
            int pageSize = 12;
            
            // Get all books for filtering
            var allBooks = await _bookService.GetAllBooksAsync();
            
            // Apply filters through the filter service
            var filteredBooks = allBooks.AsQueryable();
            filteredBooks = _filterService.ApplyFilters(filteredBooks, ViewData, searchTerm, genre, format, publisher, language, minPrice, maxPrice, inStock);
            
            // Category filters
            string category = Request.Query["category"].ToString();
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
    }
}