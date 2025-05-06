using Bookbox.Data;
using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookbox.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Discount>> GetAllDiscountsAsync()
        {
            return await _context.Discounts
                .Include(d => d.Book)
                .OrderByDescending(d => d.StartDate)
                .ToListAsync();
        }

        public async Task<List<Discount>> GetActiveDiscountsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Discounts
                .Include(d => d.Book)
                .Where(d => d.IsOnSale && 
                            d.StartDate <= now && 
                            (d.EndDate == null || d.EndDate >= now))
                .OrderByDescending(d => d.StartDate)
                .ToListAsync();
        }

        public async Task<List<Discount>> GetDiscountsByBookIdAsync(Guid bookId)
        {
            return await _context.Discounts
                .Include(d => d.Book)
                .Where(d => d.BookId == bookId)
                .OrderByDescending(d => d.StartDate)
                .ToListAsync();
        }

        public async Task<Discount?> GetDiscountByIdAsync(Guid id)
        {
            return await _context.Discounts
                .Include(d => d.Book)
                .FirstOrDefaultAsync(d => d.DiscountId == id);
        }

        public async Task<Discount?> AddDiscountAsync(DiscountDTO discountDTO)
        {
            // Convert dates to UTC for PostgreSQL
            if (discountDTO.StartDate.Kind != DateTimeKind.Utc)
            {
                discountDTO.StartDate = DateTime.SpecifyKind(discountDTO.StartDate, DateTimeKind.Utc);
            }
            
            if (discountDTO.EndDate.HasValue && discountDTO.EndDate.Value.Kind != DateTimeKind.Utc)
            {
                discountDTO.EndDate = DateTime.SpecifyKind(discountDTO.EndDate.Value, DateTimeKind.Utc);
            }

            var discount = new Discount
            {
                BookId = discountDTO.BookId,
                DiscountPercentage = discountDTO.DiscountPercentage,
                StartDate = discountDTO.StartDate,
                EndDate = discountDTO.EndDate,
                IsOnSale = discountDTO.IsOnSale
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<Discount?> UpdateDiscountAsync(Guid id, DiscountDTO discountDTO)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return null;

            // Convert dates to UTC for PostgreSQL
            if (discountDTO.StartDate.Kind != DateTimeKind.Utc)
            {
                discountDTO.StartDate = DateTime.SpecifyKind(discountDTO.StartDate, DateTimeKind.Utc);
            }
            
            if (discountDTO.EndDate.HasValue && discountDTO.EndDate.Value.Kind != DateTimeKind.Utc)
            {
                discountDTO.EndDate = DateTime.SpecifyKind(discountDTO.EndDate.Value, DateTimeKind.Utc);
            }

            discount.BookId = discountDTO.BookId;
            discount.DiscountPercentage = discountDTO.DiscountPercentage;
            discount.StartDate = discountDTO.StartDate;
            discount.EndDate = discountDTO.EndDate;
            discount.IsOnSale = discountDTO.IsOnSale;

            _context.Update(discount);
            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<bool> DeleteDiscountAsync(Guid id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return false;

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetDiscountedPriceAsync(Guid bookId, decimal originalPrice)
        {
            var now = DateTime.UtcNow;
            var activeDiscount = await _context.Discounts
                .Where(d => d.BookId == bookId && 
                           d.IsOnSale && 
                           d.StartDate <= now && 
                           (d.EndDate == null || d.EndDate >= now))
                .OrderByDescending(d => d.DiscountPercentage)
                .FirstOrDefaultAsync();

            if (activeDiscount == null)
                return originalPrice;

            var discountAmount = originalPrice * (activeDiscount.DiscountPercentage / 100);
            return Math.Round(originalPrice - discountAmount, 2);
        }
    }
}