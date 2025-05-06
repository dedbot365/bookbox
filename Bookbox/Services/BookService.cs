using Bookbox.Data;
using Bookbox.Models;
using Bookbox.DTOs;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookbox.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(Guid id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book?> AddBookAsync(BookDTO bookDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == bookDTO.ISBN);
                if (existingBook != null)
                    throw new InvalidOperationException("A book with this ISBN already exists");

                var book = new Book
                {
                    Title = bookDTO.Title,
                    Author = bookDTO.Author,
                    Genre = bookDTO.Genre,
                    Price = bookDTO.Price,
                    Format = bookDTO.Format,
                    Publisher = bookDTO.Publisher,
                    ISBN = bookDTO.ISBN,
                    Description = bookDTO.Description,
                    Stock = bookDTO.Stock,
                    Language = bookDTO.Language,
                    Awards = bookDTO.Awards,
                    PhysicalStock = bookDTO.PhysicalStock
                };

                var imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);

                if (bookDTO.ImageFile != null)
                {
                    // Add validation
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(bookDTO.ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension) || bookDTO.ImageFile.Length > 5242880) // 5MB
                        throw new ArgumentException("Invalid file type or size");
                    // Continue with file handling

                    string fileName = Guid.NewGuid() + Path.GetExtension(bookDTO.ImageFile.FileName);
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await bookDTO.ImageFile.CopyToAsync(stream);
                    }

                    book.ImageUrl = "/images/" + fileName;
                }

                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return book;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Book?> AddBookAsync(BookDTO bookDTO, Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == bookDTO.ISBN);
                if (existingBook != null)
                    throw new InvalidOperationException("A book with this ISBN already exists");

                var book = new Book
                {
                    Title = bookDTO.Title,
                    Author = bookDTO.Author,
                    Genre = bookDTO.Genre,
                    Price = bookDTO.Price,
                    Format = bookDTO.Format,
                    Publisher = bookDTO.Publisher,
                    ISBN = bookDTO.ISBN,
                    Description = bookDTO.Description,
                    Stock = bookDTO.Stock,
                    Language = bookDTO.Language,
                    Awards = bookDTO.Awards,
                    PhysicalStock = bookDTO.PhysicalStock,
                    UserId = userId  // Set the user ID from the parameter
                };

                var imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);

                if (bookDTO.ImageFile != null)
                {
                    // Add validation
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(bookDTO.ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension) || bookDTO.ImageFile.Length > 5242880) // 5MB
                        throw new ArgumentException("Invalid file type or size");
                    // Continue with file handling

                    string fileName = Guid.NewGuid() + Path.GetExtension(bookDTO.ImageFile.FileName);
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await bookDTO.ImageFile.CopyToAsync(stream);
                    }

                    book.ImageUrl = "/images/" + fileName;
                }

                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return book;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Book?> UpdateBookAsync(Guid id, BookDTO bookDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null) return null;

                var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == bookDTO.ISBN && b.BookId != id);
                if (existingBook != null)
                    throw new InvalidOperationException("A book with this ISBN already exists");

                book.Title = bookDTO.Title;
                book.Author = bookDTO.Author;
                book.Genre = bookDTO.Genre;
                book.Price = bookDTO.Price;
                book.Format = bookDTO.Format;
                book.Publisher = bookDTO.Publisher;
                book.ISBN = bookDTO.ISBN;
                book.Description = bookDTO.Description;
                book.Stock = bookDTO.Stock;
                book.Language = bookDTO.Language;
                book.Awards = bookDTO.Awards;
                book.PhysicalStock = bookDTO.PhysicalStock;
                // Don't update UserId as it shouldn't change
                // (book.UserId remains the same)

                // Add this before file operations
                var imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);

                // Add inside UpdateBookAsync before uploading new image
                if (bookDTO.ImageFile != null && !string.IsNullOrEmpty(book.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, book.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(oldImagePath))
                        File.Delete(oldImagePath);
                }

                if (bookDTO.ImageFile != null)
                {
                    // Add validation
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(bookDTO.ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension) || bookDTO.ImageFile.Length > 5242880) // 5MB
                        throw new ArgumentException("Invalid file type or size");
                    // Continue with file handling

                    string fileName = Guid.NewGuid() + Path.GetExtension(bookDTO.ImageFile.FileName);
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await bookDTO.ImageFile.CopyToAsync(stream);
                    }

                    book.ImageUrl = "/images/" + fileName;
                }

                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return book;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteBookAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null) return false;

                // Delete associated image
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, book.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(imagePath))
                        File.Delete(imagePath);
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}