using Bookbox.DTOs;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookbox.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        // GET: Announcement
        // Shows active announcements to all users
        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementService.GetActiveAnnouncementsAsync();
            return View(announcements);
        }

        // GET: Announcement/All
        // Shows all announcements to admins
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> All()
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            return View(announcements);
        }

        // GET: Announcement/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // GET: Announcement/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new AnnouncementDto
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1)
            });
        }

        // POST: Announcement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AnnouncementDto announcementDto)
        {
            if (ModelState.IsValid)
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _announcementService.CreateAnnouncementAsync(announcementDto, userId);
                return RedirectToAction(nameof(All));
            }
            return View(announcementDto);
        }

        // GET: Announcement/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Announcement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, AnnouncementDto announcementDto)
        {
            if (id != announcementDto.AnnouncementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _announcementService.UpdateAnnouncementAsync(announcementDto, userId);
                TempData["SuccessMessage"] = "Announcement updated successfully!";
                return RedirectToAction(nameof(All));
            }
            return View(announcementDto);
        }

        // GET: Announcement/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // POST: Announcement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _announcementService.DeleteAnnouncementAsync(id, userId);
            TempData["SuccessMessage"] = "Announcement deleted successfully!";
            return RedirectToAction(nameof(All));
        }
    }
}