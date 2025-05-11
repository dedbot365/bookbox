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
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        
        public OrderService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }
        
        public async Task<OrderDTO> CreateOrderFromCheckoutAsync(CheckoutDTO checkoutData)
        {
            // Begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Create the order
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    UserId = checkoutData.UserId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = checkoutData.FinalTotal,
                    DiscountApplied = checkoutData.TotalDiscountAmount,
                    ClaimCode = checkoutData.ClaimCode,
                    Status = OrderStatus.Pending,
                    PaymentMethod = checkoutData.PaymentMethod,
                    Notes = checkoutData.Notes
                };
                
                // Create order items
                var orderItems = new List<OrderItem>();
                foreach (var item in checkoutData.Items)
                {
                    orderItems.Add(new OrderItem
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        DiscountedPrice = item.Price // No individual item discount in this implementation
                    });
                }
                
                // Add order and items to context
                await _context.Orders.AddAsync(order);
                await _context.OrderItems.AddRangeAsync(orderItems);
                
                // Save changes
                await _context.SaveChangesAsync();
                
                // Extract the book IDs from checkout items to remove only these items from cart
                var bookIdsToRemove = checkoutData.Items.Select(i => i.BookId).ToList();
                await _cartService.RemoveCartItemsByBookIdsAsync(checkoutData.UserId, bookIdsToRemove);
                
                // Commit transaction
                await transaction.CommitAsync();
                
                // Return the created order
                return await GetOrderByIdAsync(order.OrderId) 
                    ?? throw new ApplicationException("Order was created but could not be retrieved");
            }
            catch (Exception)
            {
                // Roll back transaction on error
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<List<OrderDTO>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            
            return orders.Select(MapOrderToDTO).ToList();
        }
        
        public async Task<OrderDTO?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            
            return order != null ? MapOrderToDTO(order) : null;
        }
        
        public async Task<bool> CancelOrderAsync(Guid orderId, Guid userId)
        {
            var canCancel = await CanOrderBeCancelledAsync(orderId, userId);
            if (!canCancel)
            {
                return false;
            }
            
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }
            
            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        public async Task<bool> CanOrderBeCancelledAsync(Guid orderId, Guid userId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            
            if (order == null || order.UserId != userId)
            {
                return false;
            }
            
            // Check if order is pending and less than 24 hours old
            return order.Status == OrderStatus.Pending &&
                   (DateTime.UtcNow - order.OrderDate).TotalHours <= 24;
        }
        
        private OrderDTO MapOrderToDTO(Order order)
        {
            var orderDTO = new OrderDTO
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                UserName = order.User?.Username ?? string.Empty,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                DiscountApplied = order.DiscountApplied,
                ClaimCode = order.ClaimCode,
                Status = order.Status,
                CompletedDate = order.CompletedDate,
                PaymentMethod = order.PaymentMethod,
                Notes = order.Notes ?? string.Empty,
                Items = order.OrderItems
                    .Select(oi => new OrderItemDTO
                    {
                        OrderItemId = oi.OrderItemId,
                        OrderId = oi.OrderId,
                        BookId = oi.BookId,
                        BookTitle = oi.Book?.Title ?? "Unknown Book",
                        BookAuthor = oi.Book?.Author ?? "Unknown Author",
                        CoverImageUrl = oi.Book?.ImageUrl ?? string.Empty,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        DiscountedPrice = oi.DiscountedPrice
                    })
                    .ToList()
            };
            
            return orderDTO;
        }
    }
}