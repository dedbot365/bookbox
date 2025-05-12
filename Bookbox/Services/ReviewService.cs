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
            // Get books from completed orders (Status = Completed (2))
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
                    IsReviewed = _context.Reviews.Any(r => r.BookId == oi.BookId && r.UserId == userId)
                })
                .ToListAsync();

            // For books that have been reviewed, include the review data
            foreach (var book in purchasedBooks.Where(b => b.IsReviewed))
            {
                book.Review = await GetReviewByUserAndBookAsync(userId, book.BookId);
            }

            return purchasedBooks;
        }

        public async Task<bool> AddReviewAsync(Guid userId, ReviewDTO reviewDto)
        {
            // Verify the user has purchased this book in a completed order
            var hasCompletedPurchase = await _context.OrderItems
                .AnyAsync(oi => oi.BookId == reviewDto.BookId 
                    && oi.Order.UserId == userId 
                    && oi.Order.Status == OrderStatus.Completed);

            if (!hasCompletedPurchase)
            {
                return false;
            }

            // Check if user already reviewed this book
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.BookId == reviewDto.BookId && r.UserId == userId);

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
                    ReviewDate = DateTime.UtcNow
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
    }
}