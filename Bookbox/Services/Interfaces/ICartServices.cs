using Bookbox.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItem>> GetUserCartItemsAsync(Guid userId, int count = 0);
        
        Task<bool> AddToCartAsync(Guid userId, Guid bookId, int quantity = 1, decimal price = 0);
        
        Task<bool> UpdateCartItemQuantityAsync(Guid userId, Guid cartItemId, int change);
        
        Task<bool> RemoveCartItemAsync(Guid userId, Guid cartItemId);
        
        Task<int> GetCartItemCountAsync(Guid userId);
        
        Task<Cart> GetOrCreateCartAsync(Guid userId);
        
        Task<bool> IsInCartAsync(Guid userId, Guid bookId);
        
        Task<Order> CreateOrderFromCartAsync(Guid userId);
        
        Task<Order> GetOrderAsync(Guid orderId);
        
        Task<bool> ClearCartAsync(Guid userId);
        
        Task<bool> RemoveCartItemsByBookIdsAsync(Guid userId, List<Guid> bookIds);
    }
}