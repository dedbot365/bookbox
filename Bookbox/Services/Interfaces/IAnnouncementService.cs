using Bookbox.Models;
using Bookbox.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
        Task<Announcement?> GetAnnouncementByIdAsync(Guid id);
        Task<Announcement> CreateAnnouncementAsync(AnnouncementDTO announcementDTO, Guid userId);
        Task<Announcement?> UpdateAnnouncementAsync(AnnouncementDTO announcementDTO);
        Task<bool> DeleteAnnouncementAsync(Guid id);
    }
}