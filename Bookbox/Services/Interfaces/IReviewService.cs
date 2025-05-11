using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookbox.DTOs;

namespace Bookbox.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<PurchasedBookDTO>> GetPurchasedBooksAsync(Guid userId);
        Task<bool> AddReviewAsync(Guid userId, ReviewDTO reviewDto);
        Task<bool> HasUserReviewedBookAsync(Guid userId, Guid bookId);
        Task<ReviewDTO> GetReviewByUserAndBookAsync(Guid userId, Guid bookId);
    }
}