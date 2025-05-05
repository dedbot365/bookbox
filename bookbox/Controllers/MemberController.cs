using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookbox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Member,Admin")] // Both members and admins can access 
    public class MemberController : ControllerBase
    {
        public MemberController()
        {
        }

        [HttpGet("dashboard-data")]
        public IActionResult GetDashboardData()
        {
            // This endpoint will be accessible by both regular members and admins
            return Ok(new { 
                message = "You have access to the member dashboard",
                // Add any member-specific data here
            });
        }
    }
}