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
            var subject = isStaff 
                ? $"Order #{order.OrderNumber} Has Been Processed" 
                : $"Your BookBox Order #{order.OrderNumber} is Ready for Pickup";
            
            // Build the order items table
            var orderItemsHtml = "";
            decimal subtotal = 0;
            
            foreach (var item in order.OrderItems)
            {
                var itemSubtotal = item.Price * item.Quantity;
                subtotal += itemSubtotal;
                
                orderItemsHtml += $@"
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.Book?.Title ?? "Book"}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.Book?.Author ?? "Unknown"}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>NPR {item.Price:N2}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>NPR {itemSubtotal:N2}</td>
                    </tr>";
            }
            
            // Calculate discount if any
            var discount = subtotal - order.TotalAmount;
            
            if (isStaff)
            {
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Order #{order.OrderNumber} Processed</h2>
                        <p>You have successfully processed order #{order.OrderNumber} for {order.User?.FirstName} {order.User?.LastName}.</p>
                        <p>The customer has been notified that their order is ready for pickup.</p>
                        
                        <div style='margin-top: 20px; margin-bottom: 20px;'>
                            <h3>Order Invoice</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr style='background-color: #f5f5f5;'>
                                    <th colspan='2' style='text-align: left; padding: 10px;'>Order Details</th>
                                    <th colspan='3' style='text-align: right; padding: 10px;'>Date: {order.OrderDate:MMM dd, yyyy}</th>
                                </tr>
                                <tr>
                                    <td colspan='5' style='padding: 10px; border-bottom: 1px solid #ddd;'>
                                        <strong>Customer:</strong> {order.User?.FirstName} {order.User?.LastName}<br>
                                        <strong>Email:</strong> {order.User?.Email}<br>
                                        <strong>Status:</strong> Completed<br>
                                        <strong>Claim Code:</strong> {order.ClaimCode}
                                    </td>
                                </tr>
                                <tr style='background-color: #f5f5f5;'>
                                    <th style='padding: 10px; text-align: left;'>Book Title</th>
                                    <th style='padding: 10px; text-align: left;'>Author</th>
                                    <th style='padding: 10px; text-align: center;'>Qty</th>
                                    <th style='padding: 10px; text-align: right;'>Price</th>
                                    <th style='padding: 10px; text-align: right;'>Subtotal</th>
                                </tr>
                                {orderItemsHtml}
                                <tr>
                                    <td colspan='4' style='text-align: right; padding: 10px;'><strong>Subtotal:</strong></td>
                                    <td style='text-align: right; padding: 10px;'>NPR {subtotal:N2}</td>
                                </tr>
                                {(discount > 0 ? $@"
                                <tr>
                                    <td colspan='4' style='text-align: right; padding: 10px;'><strong>Discount:</strong></td>
                                    <td style='text-align: right; padding: 10px; color: #28a745;'>-NPR {discount:N2}</td>
                                </tr>" : "")}
                                <tr>
                                    <td colspan='4' style='text-align: right; padding: 10px;'><strong>Total:</strong></td>
                                    <td style='text-align: right; padding: 10px; font-weight: bold;'>NPR {order.TotalAmount:N2}</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                ";
                
                await SendEmailAsync(user.Email, subject, body);
            }
            else
            {
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Good news, {user.FirstName}!</h2>
                        <p>Your order #{order.OrderNumber} has been processed .</p>
                        
                        <div style='margin-top: 20px; margin-bottom: 20px;'>
                            <h3>Order Invoice</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr style='background-color: #f5f5f5;'>
                                    <th colspan='2' style='text-align: left; padding: 10px;'>Order #{order.OrderNumber}</th>
                                    <th colspan='3' style='text-align: right; padding: 10px;'>Date: {order.OrderDate:MMM dd, yyyy}</th>
                                </tr>
                                <tr style='background-color: #f5f5f5;'>
                                    <th style='padding: 10px; text-align: left;'>Book Title</th>
                                    <th style='padding: 10px; text-align: left;'>Author</th>
                                    <th style='padding: 10px; text-align: center;'>Qty</th>
                                    <th style='padding: 10px; text-align: right;'>Price</th>
                                    <th style='padding: 10px; text-align: right;'>Subtotal</th>
                                </tr>
                                {orderItemsHtml}
                                <tr>
                                    <td colspan='4' style='text-align: right; padding: 10px;'><strong>Subtotal:</strong></td>
                                    <td style='text-align: right; padding: 10px;'>NPR {subtotal:N2}</td>
                                </tr>
                                {(discount > 0 ? $@"
                                <tr>
                                    <td colspan='4' style='text-align: right; padding: 10px;'><strong>Discount:</strong></td>
                                    <td style='text-align: right; padding: 10px; color: #28a745;'>-NPR {discount:N2}</td>
                                </tr>" : "")}
                                <tr>
                                    <td colspan='4' style='text-align: right; padding: 10px;'><strong>Total:</strong></td>
                                    <td style='text-align: right; padding: 10px; font-weight: bold;'>NPR {order.TotalAmount:N2}</td>
                                </tr>
                            </table>
                        </div>
                        
                       
                        <p>Thank you for shopping with BookBox!</p>
                    </div>
                ";
                
                await SendEmailAsync(user.Email, subject, body);
            }
        }

        public async Task SendRegistrationConfirmationEmailAsync(User user)
        {
            var subject = "Welcome to BookBox - Registration Successful";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2>Welcome to BookBox, {user.FirstName}!</h2>
                    <p>Thank you for creating an account with us. Your registration was successful!</p>
                    
                    <div style='background-color: #f5f5f5; padding: 15px; margin: 15px 0; border-radius: 5px;'>
                        <h3>Your Account Information:</h3>
                        <p><strong>Username:</strong> {user.Username}</p>
                        <p><strong>Email:</strong> {user.Email}</p>
                    </div>
                    
                    <div style='margin: 20px 0;'>
                        <h3>What's Next?</h3>
                        <ul style='line-height: 1.6;'>
                            <li>Explore our collection of books</li>
                            <li>Add interesting titles to your wishlist</li>
                            <li>Enjoy exclusive member discounts</li>
                            <li>Receive personalized recommendations</li>
                        </ul>
                    </div>
                    
                    <p>If you have any questions or need assistance, please don't hesitate to contact our customer support.</p>
                    
                    <p>Happy reading!</p>
                    <p>The BookBox Team</p>
                </div>
            ";

            await SendEmailAsync(user.Email, subject, body);
        }
    }
}