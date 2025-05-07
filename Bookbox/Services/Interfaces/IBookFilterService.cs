using Bookbox.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq;

namespace Bookbox.Services.Interfaces
{
    public interface IBookFilterService
    {
        IQueryable<Book> ApplyFilters(
            IQueryable<Book> books, 
            ViewDataDictionary viewData,
            string searchTerm = "", 
            string genre = "", 
            string format = "", 
            string publisher = "", 
            string language = "", 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            bool? inStock = null);

        IQueryable<Book> ApplyCategory(
            IQueryable<Book> books, 
            ViewDataDictionary viewData,
            string category = "");

        IQueryable<Book> ApplySorting(
            IQueryable<Book> books, 
            ViewDataDictionary viewData,
            string sortBy = "newest");
    }
}