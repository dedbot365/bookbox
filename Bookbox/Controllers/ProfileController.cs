using Bookbox.Data;
using Bookbox.DTOs;
using Bookbox.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Bookbox.Services.Interfaces; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace Bookbox.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAuthService _authService; 

        public ProfileController(
            ApplicationDbContext context, 
            IWebHostEnvironment webHostEnvironment,
            IAuthService authService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _authService = authService;
        }

        // GET: Profile/Index or Profile/
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User identity could not be determined");
            }

            var user = await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            var address = user.Addresses.FirstOrDefault();

            var model = new ProfileDTO
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ContactNo = user.ContactNo,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                ImageUrl = user.ImageUrlText,
                RegisteredDate = user.RegisteredDate,
                SuccessfulOrderCount = user.SuccessfulOrderCount
            };

            if (address != null)
            {
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.City = address.City;
                model.State = address.State;
                model.Country = address.Country;
            }

            return View(model);
        }        // GET: Profile/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User identity could not be determined");
            }

            var user = await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            var address = user.Addresses.FirstOrDefault();

            var model = new ProfileEditDTO
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ContactNo = user.ContactNo,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                CurrentImageUrl = user.ImageUrlText,
                OriginalUsername = user.Username,
                OriginalEmail = user.Email
            };

            if (address != null)
            {
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.City = address.City;
                model.State = address.State;
                model.Country = address.Country;
                model.AddressId = address.AddressId;
            }

            return View(model);
        }

        // POST: Profile/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileEditDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User identity could not be determined");
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Check if username or email has changed and if they're already taken
            if (model.Username != model.OriginalUsername || model.Email != model.OriginalEmail)
            {
                // Check if username or email is already taken
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => 
                    (u.Username == model.Username && u.UserId != userId) || 
                    (u.Email == model.Email && u.UserId != userId));
                
                if (existingUser != null)
                {
                    if (existingUser.Username == model.Username)
                    {
                        ModelState.AddModelError("Username", "This username is already taken.");
                    }
                    if (existingUser.Email == model.Email)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                    }
                    return View(model);
                }
            }

            // FIX FOR POSTGRESQL DATETIME ERROR
            // Convert DateOfBirth to UTC if it's not null
            if (model.DateOfBirth.HasValue)
            {
                model.DateOfBirth = DateTime.SpecifyKind(model.DateOfBirth.Value, DateTimeKind.Utc);
            }            // Update user information
            user.Username = model.Username;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.ContactNo = model.ContactNo;
            user.DateOfBirth = model.DateOfBirth;
            user.Gender = model.Gender;

            // Update or create address
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId);
            
            if (address != null)
            {
                // Update existing address
                address.Address1 = model.Address1;
                address.Address2 = model.Address2;
                address.City = model.City;
                address.State = model.State;
                address.Country = model.Country;
                
                _context.Addresses.Update(address);
            }
            else
            {
                // Create new address
                var newAddress = new Address
                {
                    UserId = userId,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    City = model.City,
                    State = model.State,
                    Country = model.Country
                };
                
                _context.Addresses.Add(newAddress);
            }

            // Handle profile image upload
            if (model.ImageFile != null)
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                
                // Changed from 5MB to 10MB (10485760 bytes)
                if (!allowedExtensions.Contains(extension) || model.ImageFile.Length > 10485760) 
                {
                    ModelState.AddModelError("ImageFile", "Invalid file type or size (max 10MB, allowed formats: JPG, PNG, GIF)");
                    return View(model);
                }

                // Create profiles directory if it doesn't exist
                var imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ImageUrlText))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ImageUrlText.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string fileName = $"profile_{userId}_{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(imagesPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                user.ImageUrlText = $"/images/profiles/{fileName}";
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            // Update claims - includes the updated username and email
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                // Fix this line to preserve Staff role:
                new Claim(ClaimTypes.Role, _authService.GetRoleFromUserType((int)user.UserType))
            };

            if (!string.IsNullOrEmpty(user.ImageUrlText))
            {
                claims.Add(new Claim("ImageUrl", user.ImageUrlText));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Index");  // Change this to redirect to Index instead of EditProfile
        }

        // GET: Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordDTO());
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User identity could not be determined");
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                return View(model);
            }

            // Update password
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Index");  // Change this to redirect to Index instead of ChangePassword
        }
    }
}