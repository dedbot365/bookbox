using Bookbox.Data;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookbox.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Discount?> GetDiscountByIdAsync(Guid id)
        {
            return await _context.Discounts
                .Include(d => d.Book)
                .FirstOrDefaultAsync(d => d.DiscountId == id);
        }

        public async Task<Discount?> GetActiveDiscountForBookAsync(Guid bookId)
        {
            return await _context.Discounts
                .FirstOrDefaultAsync(d => d.BookId == bookId && d.IsOnSale && 
                            (d.EndDate == null || d.EndDate > DateTime.UtcNow));
        }

        public async Task<IEnumerable<Discount>> GetAllActiveDiscountsAsync()
        {
            var today = DateTime.UtcNow;
            return await _context.Discounts
                .Include(d => d.Book)
                .Where(d => d.IsOnSale && 
                           d.StartDate <= today && 
                           (!d.EndDate.HasValue || d.EndDate >= today))
                .ToListAsync();
        }

        public async Task<bool> CreateDiscountAsync(Discount discount)
        {
            // Enforce the 90% maximum discount rule
            if (discount.DiscountPercentage > 90)
            {
                discount.DiscountPercentage = 90;
            }
            
            // Convert dates to UTC for PostgreSQL
            discount.StartDate = DateTime.SpecifyKind(discount.StartDate, DateTimeKind.Utc);
            
            if (discount.EndDate.HasValue)
            {
                discount.EndDate = DateTime.SpecifyKind(discount.EndDate.Value, DateTimeKind.Utc);
            }

            _context.Discounts.Add(discount);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateDiscountAsync(Discount discount)
        {
            // Enforce the 90% maximum discount rule
            if (discount.DiscountPercentage > 90)
            {
                discount.DiscountPercentage = 90;
            }
            
            // Convert dates to UTC for PostgreSQL
            discount.StartDate = DateTime.SpecifyKind(discount.StartDate, DateTimeKind.Utc);
            
            if (discount.EndDate.HasValue)
            {
                discount.EndDate = DateTime.SpecifyKind(discount.EndDate.Value, DateTimeKind.Utc);
            }

            _context.Discounts.Update(discount);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteDiscountAsync(Guid id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return false;

            _context.Discounts.Remove(discount);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public decimal CalculateDiscountedPrice(decimal originalPrice, decimal discountPercentage)
        {
            var discount = originalPrice * (discountPercentage / 100);
            return Math.Round(originalPrice - discount, 2);
        }

        public int GetRemainingDays(DateTime? endDate)
        {
            if (!endDate.HasValue)
                return -1; // Indicating no end date

            var daysRemaining = (endDate.Value - DateTime.UtcNow).Days;
            return daysRemaining > 0 ? daysRemaining : 0;
        }
    }
}