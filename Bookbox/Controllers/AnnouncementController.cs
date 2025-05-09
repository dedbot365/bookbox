using Bookbox.DTOs;
using Bookbox.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
        public async Task<IActionResult> Index(string searchTerm = "", string status = "", 
            DateTime? startDateFrom = null, DateTime? startDateTo = null, 
            DateTime? endDateFrom = null, DateTime? endDateTo = null,
            string sortBy = "last_modified", int page = 1)
        {
            // Set page size
            int pageSize = 10;
            
            // Get all announcements
            var allAnnouncements = await _announcementService.GetAllAnnouncementsAsync();
            var filteredAnnouncements = allAnnouncements.AsQueryable();
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => 
                    a.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    a.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                
                ViewData["SearchTerm"] = searchTerm;
            }
            
            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                DateTime now = DateTime.UtcNow;
                switch (status)
                {
                    case "active":
                        filteredAnnouncements = filteredAnnouncements.Where(a => 
                            a.IsActive && 
                            a.StartDate <= now && 
                            (a.EndDate == null || a.EndDate >= now));
                        break;
                    case "scheduled":
                        filteredAnnouncements = filteredAnnouncements.Where(a => 
                            a.IsActive && a.StartDate > now);
                        break;
                    case "expired":
                        filteredAnnouncements = filteredAnnouncements.Where(a => 
                            a.IsActive && a.EndDate != null && a.EndDate < now);
                        break;
                    case "inactive":
                        filteredAnnouncements = filteredAnnouncements.Where(a => !a.IsActive);
                        break;
                }
                ViewData["SelectedStatus"] = status;
            }
            
            // Apply date filters
            if (startDateFrom.HasValue)
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => a.StartDate >= startDateFrom.Value);
                ViewData["StartDateFrom"] = startDateFrom.Value.ToString("yyyy-MM-dd");
            }
            
            if (startDateTo.HasValue)
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => a.StartDate <= startDateTo.Value);
                ViewData["StartDateTo"] = startDateTo.Value.ToString("yyyy-MM-dd");
            }
            
            if (endDateFrom.HasValue)
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => a.EndDate >= endDateFrom.Value);
                ViewData["EndDateFrom"] = endDateFrom.Value.ToString("yyyy-MM-dd");
            }
            
            if (endDateTo.HasValue)
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => a.EndDate <= endDateTo.Value);
                ViewData["EndDateTo"] = endDateTo.Value.ToString("yyyy-MM-dd");
            }
            
            // Apply sorting
            filteredAnnouncements = sortBy switch
            {
                "title" => filteredAnnouncements.OrderBy(a => a.Title),
                "newest" => filteredAnnouncements.OrderByDescending(a => a.StartDate),
                "oldest" => filteredAnnouncements.OrderBy(a => a.StartDate),
                "last_modified" => filteredAnnouncements.OrderByDescending(a => a.LastModified),
                "first_modified" => filteredAnnouncements.OrderBy(a => a.LastModified),
                _ => filteredAnnouncements.OrderByDescending(a => a.LastModified) // Default: last modified
            };
            
            ViewData["SortBy"] = sortBy;
            
            // Calculate pagination
            var count = filteredAnnouncements.Count();
            var totalPages = (int)Math.Ceiling(count / (double)pageSize);
            
            var currentPage = Math.Max(1, Math.Min(page, totalPages));
            
            var paginatedAnnouncements = filteredAnnouncements
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            // Set pagination data for the view
            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = count;
            
            return View(paginatedAnnouncements);
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
