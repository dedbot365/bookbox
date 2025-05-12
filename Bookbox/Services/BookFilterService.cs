using Bookbox.Constants;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bookbox.Services
{
    public class BookFilterService : IBookFilterService
    {
        // Maximum edit distance for fuzzy matching (adjust based on desired strictness)
        private const int FuzzyMatchThreshold = 2;

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
            // Text search with fuzzy matching
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Convert to list for in-memory fuzzy search processing
                var booksList = books.ToList();
                
                // Apply fuzzy search
                var fuzzyResults = booksList.Where(b => 
                    IsFuzzyMatch(b.Title, searchTerm) || 
                    IsFuzzyMatch(b.Author, searchTerm) ||
                    IsFuzzyMatch(b.ISBN, searchTerm) ||
                    IsFuzzyMatch(b.Description, searchTerm)).AsQueryable();
                
                books = fuzzyResults;
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
        
        // Helper method to determine if a string is a fuzzy match
        private bool IsFuzzyMatch(string source, string target)
        {
            // First, try contains for exact substring matches (faster)
            if (source?.Contains(target, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
            
            // If no exact match, try fuzzy matching
            
            // Null check
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                return false;
            }
            
            // For very long texts, check if the target appears in word-sized chunks
            if (source.Length > 100)
            {
                var words = source.Split(new[] { ' ', ',', '.', ';', ':', '-', '\n', '\r', '\t' }, 
                    StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var word in words)
                {
                    if (LevenshteinDistance(word, target) <= FuzzyMatchThreshold)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            // For shorter strings, apply Levenshtein distance directly
            return LevenshteinDistance(source, target) <= FuzzyMatchThreshold;
        }
        
        // Implementation of Levenshtein Distance algorithm for string similarity
        private int LevenshteinDistance(string s, string t)
        {
            // Convert strings to lowercase for case-insensitive comparison
            s = s.ToLowerInvariant();
            t = t.ToLowerInvariant();
            
            // Matrix dimensions
            int m = s.Length;
            int n = t.Length;
            
            // Quick optimization for edge cases
            if (m == 0) return n;
            if (n == 0) return m;
            
            // Create distance matrix
            int[,] d = new int[m + 1, n + 1];
            
            // Initialize first column and row
            for (int i = 0; i <= m; i++) d[i, 0] = i;
            for (int j = 0; j <= n; j++) d[0, j] = j;
            
            // Fill matrix
            for (int j = 1; j <= n; j++)
            {
                for (int i = 1; i <= m; i++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            
            // Return the distance
            return d[m, n];
        }
        
        public IQueryable<Book> ApplyCategory(
            IQueryable<Book> books, 
            ViewDataDictionary viewData,
            string category = "")
        {
            // Rest of the method remains unchanged
            viewData["Category"] = category;
            
            switch (category)
            {
                case "bestsellers":
                    books = books.Where(b => b.SalesCount >= 1).OrderByDescending(b => b.SalesCount).Take(10);
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
            // Rest of the method remains unchanged
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