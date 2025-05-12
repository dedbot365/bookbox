using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Bookbox.Models;
using Microsoft.Extensions.Configuration;
using Bookbox.Services.Interfaces;

namespace Bookbox.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient
                {
                    Host = _configuration["Email:MAIL_SERVER"],
                    Port = int.Parse(_configuration["Email:MAIL_PORT"]),
                    EnableSsl = bool.Parse(_configuration["Email:MAIL_STARTTLS"]),
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        _configuration["Email:MAIL_USERNAME"],
                        _configuration["Email:MAIL_PASSWORD"])
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:MAIL_FROM"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(email);
                
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }

        public async Task SendOrderCreationEmailToMemberAsync(User user, Order order)
        {
            var subject = $"Your BookBox Order #{order.OrderNumber} has been placed";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2>Thank you for your order, {user.FirstName}!</h2>
                    <p>We're happy to confirm that your order #{order.OrderNumber} has been received and is being processed.</p>
                    <div style='background-color: #f5f5f5; padding: 15px; margin: 15px 0; border-radius: 5px;'>
                        <h3>Your Claim Code: <span style='color: #1b6ec2; font-weight: bold;'>{order.ClaimCode}</span></h3>
                        <p>Please present this code when collecting your books.</p>
                    </div>
                    <p><strong>Order Total:</strong> NPR {order.TotalAmount:N2}</p>
                    <p>You'll receive another email once your order has been processed and is ready for pickup.</p>
                    <p>Thank you for shopping with BookBox!</p>
                </div>
            ";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendOrderCreationEmailToStaffAsync(User staff, User member, Order order)
        {
            var subject = $"New Order #{order.OrderNumber} Requires Review";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2>New Order Requires Your Attention</h2>
                    <p>Order #{order.OrderNumber} from {member.FirstName} {member.LastName} has been placed and requires processing.</p>
                    <p><strong>Order Total:</strong> NPR {order.TotalAmount:N2}</p>
                    <p>Please log in to the staff dashboard to review and process this order.</p>
                </div>
            ";

            await SendEmailAsync(staff.Email, subject, body);
        }

        public async Task SendOrderProcessedEmailAsync(User user, Order order, bool isStaff)
        {
            var subject = $"Order #{order.OrderNumber} Has Been Processed";
            string body;

            if (isStaff)
            {
                body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Order #{order.OrderNumber} Processed</h2>
                        <p>You have successfully processed order #{order.OrderNumber}.</p>
                        <p>The customer has been notified that their order is ready for pickup.</p>
                    </div>
                ";
            }
            else
            {
                body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Good news, {user.FirstName}!</h2>
                        <p>Your order #{order.OrderNumber} has been processed .</p>
                        <p>Thank you for shopping with BookBox!</p>
                    </div>
                ";
            }

            await SendEmailAsync(user.Email, subject, body);
        }
    }
}