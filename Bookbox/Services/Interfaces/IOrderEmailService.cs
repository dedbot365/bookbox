using System;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    /// <summary>
    /// Service for sending email notifications related to orders
    /// </summary>
    public interface IOrderEmailService
    {
        
        Task SendOrderCreationEmailsAsync(Guid orderId);

        
        Task SendOrderProcessedEmailsAsync(Guid orderId, Guid staffId);
    }
}