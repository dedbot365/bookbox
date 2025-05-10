using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookbox.Constants;
using Bookbox.DTOs;
using Bookbox.Models;

namespace Bookbox.Services.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order from checkout data
        /// </summary>
        /// <param name="checkoutData">The checkout data to create an order from</param>
        /// <returns>The created order</returns>
        Task<OrderDTO> CreateOrderFromCheckoutAsync(CheckoutDTO checkoutData);
        
        /// <summary>
        /// Gets all orders for a user
        /// </summary>
        /// <param name="userId">The user ID to get orders for</param>
        /// <returns>A list of orders</returns>
        Task<List<OrderDTO>> GetUserOrdersAsync(Guid userId);
        
        /// <summary>
        /// Gets an order by ID
        /// </summary>
        /// <param name="orderId">The order ID to get</param>
        /// <returns>The order if found, null otherwise</returns>
        Task<OrderDTO?> GetOrderByIdAsync(Guid orderId);
        
        /// <summary>
        /// Cancels an order if it's eligible
        /// </summary>
        /// <param name="orderId">The order ID to cancel</param>
        /// <param name="userId">The user attempting to cancel the order</param>
        /// <returns>True if cancelled successfully, false otherwise</returns>
        Task<bool> CancelOrderAsync(Guid orderId, Guid userId);
        
        /// <summary>
        /// Checks if an order can be cancelled
        /// </summary>
        /// <param name="orderId">The order ID to check</param>
        /// <param name="userId">The user attempting to cancel the order</param>
        /// <returns>True if the order can be cancelled, false otherwise</returns>
        Task<bool> CanOrderBeCancelledAsync(Guid orderId, Guid userId);
    }
}