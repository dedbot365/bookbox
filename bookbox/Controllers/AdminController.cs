using System.Threading.Tasks;
using bookbox.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookbox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only admin users can access this controller
    public class AdminController : ControllerBase
    {
        public AdminController()
        {
        }

        [HttpGet("dashboard-data")]
        public IActionResult GetDashboardData()
        {
            // This endpoint will only be accessible by admin users
            return Ok(new { 
                message = "You have access to the admin dashboard",
                // Add any admin-specific data here
            });
        }
    }
}