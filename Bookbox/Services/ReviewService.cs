using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookbox.Constants;
using Bookbox.Data;
using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookbox.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PurchasedBookDTO>> GetPurchasedBooksAsync(Guid userId)
        {
            // Get books from completed orders (Status = Completed)
            var purchasedBooks = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Book)
                .Where(oi => oi.Order.UserId == userId && oi.Order.Status == OrderStatus.Completed)
                .Select(oi => new PurchasedBookDTO
                {
                    BookId = oi.BookId,
                    Title = oi.Book.Title,
                    Author = oi.Book.Author,
                    ImageUrl = oi.Book.ImageUrl,
                    PurchaseDate = oi.Order.CompletedDate ?? oi.Order.OrderDate,
                    IsReviewed = _context.Reviews.Any(r => 
                        r.BookId == oi.BookId && 
                        r.UserId == userId && 
                        r.OrderItemId == oi.OrderItemId),
                    OrderId = oi.OrderId,
                    OrderItemId = oi.OrderItemId,
                    OrderNumber = oi.Order.OrderNumber.ToString() // Add ToString() here
                })
                .ToListAsync();

            // For books that have been reviewed, include the review data
            foreach (var book in purchasedBooks.Where(b => b.IsReviewed))
            {
                var review = await _context.Reviews
                    .Where(r => r.BookId == book.BookId && 
                           r.UserId == userId && 
                           r.OrderItemId == book.OrderItemId)
                    .Select(r => new ReviewDTO
                    {
                        ReviewId = r.ReviewId,
                        BookId = r.BookId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        ReviewDate = r.ReviewDate,
                        OrderId = r.OrderId,
                        OrderItemId = r.OrderItemId
                    })
                    .FirstOrDefaultAsync();
                
                if (review != null)
                {
                    book.Review = review;
                }
            }

            return purchasedBooks;
        }

        public async Task<bool> AddReviewAsync(Guid userId, ReviewDTO reviewDto)
        {
            // Verify the user has purchased this book in a completed order
            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => 
                    oi.OrderItemId == reviewDto.OrderItemId && 
                    oi.BookId == reviewDto.BookId &&
                    oi.Order.UserId == userId && 
                    oi.Order.Status == OrderStatus.Completed);

            if (orderItem == null)
            {
                return false; // Order item not found or order not completed
            }

            // Check if user already reviewed this specific order item
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => 
                    r.BookId == reviewDto.BookId && 
                    r.UserId == userId && 
                    r.OrderItemId == reviewDto.OrderItemId);

            if (existingReview != null)
            {
                // Update existing review
                existingReview.Rating = reviewDto.Rating;
                existingReview.Comment = reviewDto.Comment;
                existingReview.ReviewDate = DateTime.UtcNow;
            }
            else
            {
                // Create new review
                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    BookId = reviewDto.BookId,
                    UserId = userId,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    ReviewDate = DateTime.UtcNow,
                    OrderId = reviewDto.OrderId,
                    OrderItemId = reviewDto.OrderItemId
                };

                await _context.Reviews.AddAsync(review);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasUserReviewedBookAsync(Guid userId, Guid bookId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.BookId == bookId && r.UserId == userId);
        }

        public async Task<ReviewDTO> GetReviewByUserAndBookAsync(Guid userId, Guid bookId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId);

            if (review == null)
            {
                return null;
            }

            return new ReviewDTO
            {
                BookId = review.BookId,
                Rating = review.Rating,
                Comment = review.Comment
            };
        }

        public async Task<double> GetAverageRatingForBookAsync(Guid bookId)
        {
            var ratings = await _context.Reviews
                .Where(r => r.BookId == bookId)
                .Select(r => r.Rating)
                .ToListAsync();

            if (!ratings.Any())
                return 0;

            return Math.Round(ratings.Average(), 1);
        }

        public async Task<int> GetReviewCountForBookAsync(Guid bookId)
        {
            return await _context.Reviews
                .CountAsync(r => r.BookId == bookId);
        }

        public async Task<List<ReviewDTO>> GetRecentReviewsForBookAsync(Guid bookId, int count = 5)
        {
            return await _context.Reviews
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.ReviewDate)
                .Take(count)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    BookId = r.BookId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    UserName = r.User.Username,
                    UserImageUrl = r.User.ImageUrlText // Add this line to include user's image
                })
                .ToListAsync();
        }

        public async Task<string> GetBookTitleAsync(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            return book?.Title ?? "Unknown Book";
        }
    }
}