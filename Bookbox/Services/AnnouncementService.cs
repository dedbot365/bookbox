using Bookbox.Data;
using Bookbox.DTOs;
using Bookbox.Models;
using Bookbox.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookbox.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements
                .Include(a => a.User)
                .OrderByDescending(a => a.LastModified)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
        {
            DateTime now = DateTime.UtcNow;
            return await _context.Announcements
                .Where(a => a.IsActive && 
                           a.StartDate <= now && 
                           (a.EndDate == null || a.EndDate >= now))
                .OrderByDescending(a => a.LastModified)
                .ToListAsync();
        }

        public async Task<Announcement?> GetAnnouncementByIdAsync(Guid id)
        {
            return await _context.Announcements
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);
        }

        public async Task<Announcement> CreateAnnouncementAsync(AnnouncementDTO announcementDTO, Guid userId)
        {
            // Convert dates to UTC for PostgreSQL
            DateTime startDate = DateTime.SpecifyKind(announcementDTO.StartDate, DateTimeKind.Utc);
            DateTime? endDate = announcementDTO.EndDate.HasValue 
                ? DateTime.SpecifyKind(announcementDTO.EndDate.Value, DateTimeKind.Utc)
                : null;

            var announcement = new Announcement
            {
                Title = announcementDTO.Title,
                Content = announcementDTO.Content,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = announcementDTO.IsActive,
                UserId = userId,
                LastModified = DateTime.UtcNow
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        public async Task<Announcement?> UpdateAnnouncementAsync(AnnouncementDTO announcementDTO)
        {
            if (!announcementDTO.AnnouncementId.HasValue)
                return null;

            var announcement = await _context.Announcements.FindAsync(announcementDTO.AnnouncementId.Value);
            if (announcement == null)
                return null;

            // Convert dates to UTC for PostgreSQL
            DateTime startDate = DateTime.SpecifyKind(announcementDTO.StartDate, DateTimeKind.Utc);
            DateTime? endDate = announcementDTO.EndDate.HasValue 
                ? DateTime.SpecifyKind(announcementDTO.EndDate.Value, DateTimeKind.Utc)
                : null;

            announcement.Title = announcementDTO.Title;
            announcement.Content = announcementDTO.Content;
            announcement.StartDate = startDate;
            announcement.EndDate = endDate;
            announcement.IsActive = announcementDTO.IsActive;
            announcement.LastModified = DateTime.UtcNow;

            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        public async Task<bool> DeleteAnnouncementAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            _context.Announcements.Remove(announcement);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}