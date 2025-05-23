using Bookbox.Models;
using Bookbox.Constants;
using Bookbox.DTOs;  
using Bookbox.Services.Interfaces;
using Bookbox.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Bookbox.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService; // Add IUserService
        private readonly IEmailService _emailService; // Add this field

        public AuthController(IAuthService authService, IUserService userService, IEmailService emailService)
        {
            _authService = authService;
            _userService = userService; // Initialize it
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If user is already logged in, redirect to appropriate page
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin");
                else if (User.IsInRole("Staff"))
                    return RedirectToAction("Dashboard", "Staff");
                else
                    return RedirectToAction("Index", "Member");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
                return View(loginDTO);

            var (isAuthenticated, user) = await _authService.AuthenticateUserAsync(loginDTO);

            if (!isAuthenticated || user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(loginDTO);
            }

            // Create claims for the authenticated user
            string role = _authService.GetRoleFromUserType((int)user.UserType);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            // Add the image URL claim if it exists
            if (!string.IsNullOrEmpty(user.ImageUrlText))
            {
                claims.Add(new Claim("ImageUrl", user.ImageUrlText));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = loginDTO.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect based on role
            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");
            else if (role == "Staff")
                return RedirectToAction("Dashboard", "Staff");
            else
                return RedirectToAction("Index", "Member");
        }

        [HttpGet]
        public IActionResult Register()
        {
            // If user is already logged in, redirect to home page
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Create user from DTO
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ContactNo = model.ContactNo,
                Gender = model.Gender,
                UserType = UserType.Member,  // Default as member
                RegisteredDate = DateTimeOffset.UtcNow
            };
            
            // Create address from DTO
            var address = new Address
            {
                Address1 = model.Address1,
                Address2 = model.Address2,
                City = model.City,
                State = model.State,
                Country = model.Country
                // UserId will be set by the service
            };

            var result = await _userService.RegisterAsync(user, model.Password, address);
            
            if (result == null)
            {
                ModelState.AddModelError("", "Username or Email already exists.");
                return View(model);
            }

            // Send registration confirmation email
            await _emailService.SendRegistrationConfirmationEmailAsync(result);

            // Registration successful, redirect to login page
            TempData["SuccessMessage"] = "Registration successful! Please check your email for confirmation and log in.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}