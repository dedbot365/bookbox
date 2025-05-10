using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}