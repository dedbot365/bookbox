using Bookbox.DTOs;
using Bookbox.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    public interface IAnnouncementService
    {
        // Get all announcements
        Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync();
        
        // Get active announcements (where IsActive is true and date is valid)
        Task<IEnumerable<AnnouncementDto>> GetActiveAnnouncementsAsync();
        
        // Get a specific announcement by ID
        Task<AnnouncementDto> GetAnnouncementByIdAsync(Guid id);
        
        // Create a new announcement (admin only)
        Task<AnnouncementDto> CreateAnnouncementAsync(AnnouncementDto announcementDto, Guid currentUserId);
        
        // Update an existing announcement (admin only)
        Task<AnnouncementDto> UpdateAnnouncementAsync(AnnouncementDto announcementDto, Guid currentUserId);
        
        // Delete an announcement (admin only)
        Task<bool> DeleteAnnouncementAsync(Guid id, Guid currentUserId);
    }
}