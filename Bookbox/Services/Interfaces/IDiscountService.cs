using Bookbox.DTOs;
using Bookbox.Models;

namespace Bookbox.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<List<Discount>> GetAllDiscountsAsync();
        Task<List<Discount>> GetActiveDiscountsAsync();
        Task<List<Discount>> GetDiscountsByBookIdAsync(Guid bookId);
        Task<Discount?> GetDiscountByIdAsync(Guid id);
        Task<Discount?> AddDiscountAsync(DiscountDTO discountDTO);
        Task<Discount?> UpdateDiscountAsync(Guid id, DiscountDTO discountDTO);
        Task<bool> DeleteDiscountAsync(Guid id);
        Task<decimal> GetDiscountedPriceAsync(Guid bookId, decimal originalPrice);
    }
}