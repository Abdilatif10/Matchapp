using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleApp.Models;
using SimpleApp.Services;

namespace SimpleApp.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly FootballDataService _footballDataService;
        private readonly IWebHostEnvironment _environment;

        [BindProperty]
        public User UserProfile { get; set; }

        public SelectList Teams { get; set; }

        public ProfileModel(
            UserManager<User> userManager,
            FootballDataService footballDataService,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _footballDataService = footballDataService;
            _environment = environment;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            UserProfile = await _userManager.GetUserAsync(User);
            if (UserProfile == null)
            {
                return NotFound();
            }

            await LoadTeamsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadTeamsAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = UserProfile.FirstName;
            user.LastName = UserProfile.LastName;
            user.Bio = UserProfile.Bio;
            user.FavoriteTeam = UserProfile.FavoriteTeam;
            user.DateOfBirth = UserProfile.DateOfBirth;
            user.LastUpdated = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your profile has been updated";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadTeamsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadPhotoAsync(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "No file was uploaded.");
                await LoadTeamsAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(profilePicture.ContentType.ToLower()))
            {
                ModelState.AddModelError(string.Empty, "Only JPEG, PNG and GIF files are allowed.");
                await LoadTeamsAsync();
                return Page();
            }

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            // Delete old profile picture if it exists
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath, user.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Generate unique filename
            var fileName = $"{user.Id}_{DateTime.UtcNow.Ticks}{Path.GetExtension(profilePicture.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            // Update user profile picture path
            user.ProfilePicture = $"/uploads/profiles/{fileName}";
            user.LastUpdated = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your profile picture has been updated";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetToDefaultAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Delete the existing profile picture file if it exists
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath, user.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Reset the profile picture to null to use default
            user.ProfilePicture = null;
            user.LastUpdated = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your profile picture has been reset to default";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToPage();
        }

        private async Task LoadTeamsAsync()
        {
            var teams = await _footballDataService.GetPremierLeagueTeamsAsync();
            Teams = new SelectList(teams, "Name", "Name");
        }
    }
}
