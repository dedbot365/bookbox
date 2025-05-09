using System;
using System.Threading.Tasks;
using Bookbox.DTOs;
using Bookbox.Models;

namespace Bookbox.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(Guid userId);
        Task<CartDto> AddItemToCartAsync(Guid userId, Guid bookId, int quantity);
        Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, int quantity);
        Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId);
        Task<bool> ClearCartAsync(Guid userId);
    }
}