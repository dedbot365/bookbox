using System.Threading.Tasks;
using Bookbox.Models;

namespace Bookbox.Services
{
    public interface IEmailService
    {
        Task SendOrderCreationEmailToMemberAsync(User user, Order order);
        Task SendOrderCreationEmailToStaffAsync(User staff, User member, Order order);
        Task SendOrderProcessedEmailAsync(User user, Order order, bool isStaff);
    }
}