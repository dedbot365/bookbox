using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bookbox.Constants;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        private readonly IAnnouncementService _announcementService;

        public AdminController(
            IBookService bookService,
            IUserService userService,
            IAnnouncementService announcementService)
        {
            _bookService = bookService;
            _userService = userService;
            _announcementService = announcementService;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Get real data where available
            var books = await _bookService.GetAllBooksAsync();
            var users = await _userService.GetAllUsersAsync();
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            var recentAnnouncements = await _announcementService.GetRecentAnnouncementsAsync(5);

            // Calculate books by genre
            var booksByGenre = books
                .GroupBy(b => b.Genre)
                .Select(g => new { Genre = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Calculate books by format 
            var booksByFormat = books
                .GroupBy(b => b.Format)
                .Select(g => new { Format = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Pass data to view
            ViewBag.TotalBooks = books.Count;
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalAnnouncements = announcements.Count();
            ViewBag.Announcements = recentAnnouncements;
            ViewBag.BooksByGenre = booksByGenre;
            ViewBag.BooksByFormat = booksByFormat; // Add this line

            return View();
        }
    }
}