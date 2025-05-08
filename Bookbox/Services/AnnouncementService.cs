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

        public async Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync()
        {
            var announcements = await _context.Announcements
                .Include(a => a.User)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            return announcements.Select(MapToDto);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetActiveAnnouncementsAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            var announcements = await _context.Announcements
                .Include(a => a.User)
                .Where(a => a.IsActive && 
                           a.StartDate.Date <= today && 
                           (!a.EndDate.HasValue || a.EndDate.Value.Date >= today))
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            return announcements.Select(MapToDto);
        }

        public async Task<AnnouncementDto> GetAnnouncementByIdAsync(Guid id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);

            if (announcement == null)
                return null;

            return MapToDto(announcement);
        }

        public async Task<AnnouncementDto> CreateAnnouncementAsync(AnnouncementDto announcementDto, Guid currentUserId)
        {
            var announcement = new Announcement
            {
                AnnouncementId = Guid.NewGuid(),
                Title = announcementDto.Title,
                Content = announcementDto.Content,
                StartDate = DateTime.SpecifyKind(announcementDto.StartDate, DateTimeKind.Utc),
                EndDate = announcementDto.EndDate.HasValue 
                          ? DateTime.SpecifyKind(announcementDto.EndDate.Value, DateTimeKind.Utc) 
                          : null,
                IsActive = announcementDto.IsActive,
                UserId = currentUserId,
                LastModified = DateTime.UtcNow
            };

            await _context.Announcements.AddAsync(announcement);
            await _context.SaveChangesAsync();

            // Reload to get user details
            announcement = await _context.Announcements
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnnouncementId == announcement.AnnouncementId);

            return MapToDto(announcement);
        }

        public async Task<AnnouncementDto> UpdateAnnouncementAsync(AnnouncementDto announcementDto, Guid currentUserId)
        {
            var announcement = await _context.Announcements
                .FindAsync(announcementDto.AnnouncementId);

            if (announcement == null)
                return null;

            // Update properties
            announcement.Title = announcementDto.Title;
            announcement.Content = announcementDto.Content;
            announcement.StartDate = DateTime.SpecifyKind(announcementDto.StartDate, DateTimeKind.Utc);
            announcement.EndDate = announcementDto.EndDate.HasValue 
                                  ? DateTime.SpecifyKind(announcementDto.EndDate.Value, DateTimeKind.Utc) 
                                  : null;

            announcement.IsActive = announcementDto.IsActive;
            announcement.LastModified = DateTime.UtcNow;

            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();

            // Reload with user details
            announcement = await _context.Announcements
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnnouncementId == announcement.AnnouncementId);

            return MapToDto(announcement);
        }

        public async Task<bool> DeleteAnnouncementAsync(Guid id, Guid currentUserId)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            
            if (announcement == null)
                return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            
            return true;
        }

        // Helper method to map Announcement entity to AnnouncementDto
        private AnnouncementDto MapToDto(Announcement announcement)
        {
            return new AnnouncementDto
            {
                AnnouncementId = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.Content,
                StartDate = announcement.StartDate,
                EndDate = announcement.EndDate,
                IsActive = announcement.IsActive,
                UserId = announcement.UserId,
                UserName = announcement.User?.Username, // Add this line to map username
                LastModified = announcement.LastModified
            };
        }
    }
}