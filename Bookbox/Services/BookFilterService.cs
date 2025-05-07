using Bookbox.Constants;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Linq;

namespace Bookbox.Services
{
    public class BookFilterService : IBookFilterService
    {
        public IQueryable<Book> ApplyFilters(
            IQueryable<Book> books, 
            ViewDataDictionary viewData,
            string searchTerm = "", 
            string genre = "", 
            string format = "", 
            string publisher = "", 
            string language = "", 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            bool? inStock = null)
        {
            // Text search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                books = books.Where(b => 
                    b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    b.ISBN.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    b.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                    
                viewData["SearchTerm"] = searchTerm;
            }
            
            // Genre filter
            if (!string.IsNullOrWhiteSpace(genre) && Enum.TryParse<Genre>(genre, out var genreEnum))
            {
                books = books.Where(b => b.Genre == genreEnum);
                viewData["SelectedGenre"] = genre;
            }
            
            // Format filter
            if (!string.IsNullOrWhiteSpace(format) && Enum.TryParse<Format>(format, out var formatEnum))
            {
                books = books.Where(b => b.Format == formatEnum);
                viewData["SelectedFormat"] = format;
            }
            
            // Publisher filter
            if (!string.IsNullOrWhiteSpace(publisher))
            {
                books = books.Where(b => b.Publisher.Contains(publisher, StringComparison.OrdinalIgnoreCase));
                viewData["SelectedPublisher"] = publisher;
            }
            
            // Language filter
            if (!string.IsNullOrWhiteSpace(language))
            {
                books = books.Where(b => b.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
                viewData["SelectedLanguage"] = language;
            }
            
            // Price range
            if (minPrice.HasValue)
            {
                books = books.Where(b => b.Price >= minPrice.Value);
                viewData["MinPrice"] = minPrice.Value;
            }
            
            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.Price <= maxPrice.Value);
                viewData["MaxPrice"] = maxPrice.Value;
            }
            
            // Stock availability
            if (inStock.HasValue && inStock.Value)
            {
                books = books.Where(b => b.Stock > 0);
                viewData["InStock"] = true;
            }
            
            return books;
        }
        
        public IQueryable<Book> ApplyCategory(
            IQueryable<Book> books, 
            ViewDataDictionary viewData,
            string category = "")
        {
            viewData["Category"] = category;
            
            switch (category)
            {
                case "bestsellers":
                    books = books.OrderByDescending(b => b.SalesCount).Take(50);
                    viewData["CategoryName"] = "Bestsellers";
                    break;
                case "award-winners":
                    books = books.Where(b => !string.IsNullOrEmpty(b.Awards));
                    viewData["CategoryName"] = "Award Winners";
                    break;
                case "new-releases":
                    DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
                    books = books.Where(b => b.PublicationDate >= threeMonthsAgo);
                    viewData["CategoryName"] = "New Releases";
                    break;
                case "new-arrivals":
                    DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);
                    books = books.Where(b => b.ArrivalDate >= oneMonthAgo);
                    viewData["CategoryName"] = "New Arrivals";
                    break;
                case "coming-soon":
                    books = books.Where(b => b.IsComingSoon);
                    viewData["CategoryName"] = "Coming Soon";
                    break;
                default:
                    viewData["CategoryName"] = "All Books";
                    break;
            }
            
            return books;
        }
        
        public IQueryable<Book> ApplySorting(
            IQueryable<Book> books, 
            ViewDataDictionary viewData,
            string sortBy = "newest")
        {
            books = sortBy switch
            {
                "price_asc" => books.OrderBy(b => b.Price),
                "price_desc" => books.OrderByDescending(b => b.Price),
                "title" => books.OrderBy(b => b.Title),
                "author" => books.OrderBy(b => b.Author),
                "bestselling" => books.OrderByDescending(b => b.SalesCount),
                "publication_date" => books.OrderByDescending(b => b.PublicationDate),
                _ => books.OrderByDescending(b => b.ArrivalDate) // Default: newest
            };
            
            viewData["SortBy"] = sortBy;
            return books;
        }
    }
}