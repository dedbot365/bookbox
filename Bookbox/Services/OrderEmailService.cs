using System;
using System.Linq;
using System.Threading.Tasks;
using Bookbox.Data;
using Bookbox.Models;
using Bookbox.Constants; 
using Microsoft.EntityFrameworkCore;
using Bookbox.Services.Interfaces;

namespace Bookbox.Services
{
    public class OrderEmailService : IOrderEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public OrderEmailService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendOrderCreationEmailsAsync(Guid orderId)
        {
            // Get order with user details
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return;

            // Send email to customer
            await _emailService.SendOrderCreationEmailToMemberAsync(order.User, order);

            // Send email to all staff members
            var staffUsers = await _context.Users.Where(u => u.UserType == UserType.Staff).ToListAsync(); 
            foreach (var staff in staffUsers)  // Add this foreach loop
            {
                await _emailService.SendOrderCreationEmailToStaffAsync(staff, order.User, order);
            }
        }

        public async Task SendOrderProcessedEmailsAsync(Guid orderId, Guid staffId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            var staffUser = await _context.Users.FindAsync(staffId);

            if (order == null || staffUser == null)
                return;

            // Send email to customer
            await _emailService.SendOrderProcessedEmailAsync(order.User, order, false);

            // Send email to staff who processed the order
            await _emailService.SendOrderProcessedEmailAsync(staffUser, order, true);
        }
    }
}