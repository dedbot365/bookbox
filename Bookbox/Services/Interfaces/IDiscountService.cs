using Bookbox.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<Discount> GetDiscountByIdAsync(Guid id);
        Task<Discount> GetActiveDiscountForBookAsync(Guid bookId);
        Task<IEnumerable<Discount>> GetAllActiveDiscountsAsync();
        Task<bool> CreateDiscountAsync(Discount discount);
        Task<bool> UpdateDiscountAsync(Discount discount);
        Task<bool> DeleteDiscountAsync(Guid id);
        decimal CalculateDiscountedPrice(decimal originalPrice, decimal discountPercentage);
        int GetRemainingDays(DateTime? endDate);
    }
}