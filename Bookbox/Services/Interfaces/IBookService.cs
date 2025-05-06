using Bookbox.Models;
using Bookbox.DTOs;

namespace Bookbox.Services.Interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(Guid id);
        Task<Book?> AddBookAsync(BookDTO bookDTO);
        Task<Book?> UpdateBookAsync(Guid id, BookDTO bookDTO);
        Task<bool> DeleteBookAsync(Guid id);
    }
}