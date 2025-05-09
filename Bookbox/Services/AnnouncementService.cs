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
            // Use UTC time for database queries
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
            // Convert local dates to UTC for storage
            TimeZoneInfo nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kathmandu");
            
            // First ensure dates are treated as Nepal time
            DateTime localStartDate = DateTime.SpecifyKind(announcementDTO.StartDate, DateTimeKind.Unspecified);
            DateTime? localEndDate = announcementDTO.EndDate.HasValue 
                ? DateTime.SpecifyKind(announcementDTO.EndDate.Value, DateTimeKind.Unspecified)
                : null;
            
            // Then convert to UTC
            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(localStartDate, nepalTimeZone);
            DateTime? utcEndDate = localEndDate.HasValue 
                ? TimeZoneInfo.ConvertTimeToUtc(localEndDate.Value, nepalTimeZone)
                : null;

            var announcement = new Announcement
            {
                Title = announcementDTO.Title,
                Content = announcementDTO.Content,
                StartDate = utcStartDate,
                EndDate = utcEndDate,
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
            // First check if the ID has a value
            if (!announcementDTO.AnnouncementId.HasValue)
            {
                return null;
            }
            
            // Same timezone conversion as in Create method
            TimeZoneInfo nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kathmandu");
            
            // Rest of your method remains the same
            DateTime localStartDate = DateTime.SpecifyKind(announcementDTO.StartDate, DateTimeKind.Unspecified);
            DateTime? localEndDate = announcementDTO.EndDate.HasValue 
                ? DateTime.SpecifyKind(announcementDTO.EndDate.Value, DateTimeKind.Unspecified)
                : null;
            
            // Then convert to UTC
            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(localStartDate, nepalTimeZone);
            DateTime? utcEndDate = localEndDate.HasValue 
                ? TimeZoneInfo.ConvertTimeToUtc(localEndDate.Value, nepalTimeZone)
                : null;
            
            // Now it's safe to access .Value
            var announcement = await _context.Announcements.FindAsync(announcementDTO.AnnouncementId.Value);
            if (announcement == null)
                return null;
            
            announcement.Title = announcementDTO.Title;
            announcement.Content = announcementDTO.Content;
            announcement.StartDate = utcStartDate;
            announcement.EndDate = utcEndDate;
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

            DateTime endDate = announcement.EndDate ?? DateTime.MaxValue;

            _context.Announcements.Remove(announcement);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<Announcement>> GetExpiredAnnouncementsAsync()
        {
            var announcements = await _context.Announcements.ToListAsync();
            var result = announcements.Where(a => a.IsActive && a.EndDate.HasValue && a.EndDate.Value < DateTime.Now);
            return result;
        }
    }
}
