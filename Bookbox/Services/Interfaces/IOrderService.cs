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
        
        Task<OrderDTO> CreateOrderFromCheckoutAsync(CheckoutDTO checkoutData);
        
      
        Task<List<OrderDTO>> GetUserOrdersAsync(Guid userId);
        
       
        Task<OrderDTO?> GetOrderByIdAsync(Guid orderId);
        
        
        Task<bool> CancelOrderAsync(Guid orderId, Guid userId);
        
        
        Task<bool> CanOrderBeCancelledAsync(Guid orderId, Guid userId);

        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
    }
}