using Bookbox.DTOs;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AnnouncementController : Controller
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        // GET: Announcement
        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync();
            return View(announcements);
        }

        // GET: Announcement/Create
        public IActionResult Create()
        {
            TimeZoneInfo nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kathmandu");
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nepalTimeZone);
            
            return View(new AnnouncementDTO
            {
                StartDate = localNow,
                EndDate = localNow.AddDays(7)
            });
        }

        // POST: Announcement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementDTO announcementDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current user's ID from the claims
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    {
                        ModelState.AddModelError("", "User identity could not be determined.");
                        return View(announcementDTO);
                    }

                    await _announcementService.CreateAnnouncementAsync(announcementDTO, userId);
                    TempData["SuccessMessage"] = "Announcement created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while creating the announcement.");
                }
            }
            return View(announcementDTO);
        }

        // GET: Announcement/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            // Convert UTC times to Nepal time for display
            TimeZoneInfo nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kathmandu");
            DateTime localStartDate = TimeZoneInfo.ConvertTimeFromUtc(announcement.StartDate, nepalTimeZone);
            DateTime? localEndDate = announcement.EndDate.HasValue ? 
                TimeZoneInfo.ConvertTimeFromUtc(announcement.EndDate.Value, nepalTimeZone) : null;

            var announcementDTO = new AnnouncementDTO
            {
                AnnouncementId = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.Content,
                StartDate = localStartDate,
                EndDate = localEndDate,
                IsActive = announcement.IsActive
            };

            return View(announcementDTO);
        }

        // POST: Announcement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AnnouncementDTO announcementDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var announcement = await _announcementService.UpdateAnnouncementAsync(announcementDTO);
                    if (announcement != null)
                    {
                        TempData["SuccessMessage"] = "Announcement updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while updating the announcement.");
                }
            }
            return View(announcementDTO);
        }

        // GET: Announcement/Delete/5
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
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _announcementService.DeleteAnnouncementAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Announcement deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the announcement.";
            }
            return RedirectToAction(nameof(Index));
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

        // GET: Announcement/DeleteConfirm/5
        public async Task<IActionResult> DeleteConfirm(Guid id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View("Delete", announcement);
        }
    }
}
