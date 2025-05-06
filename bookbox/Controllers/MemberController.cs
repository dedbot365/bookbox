using Bookbox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookbox.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}