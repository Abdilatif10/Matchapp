using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleApp.Models;
using SimpleApp.Services;

namespace SimpleApp.Pages
{
    [Authorize]
    public class SelectTeamModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly FootballDataService _footballDataService;

        public List<Team> Teams { get; set; } = new List<Team>();

        public SelectTeamModel(
            UserManager<User> userManager,
            FootballDataService footballDataService)
        {
            _userManager = userManager;
            _footballDataService = footballDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(user.FavoriteTeam))
            {
                return RedirectToPage("/Index");
            }

            Teams = await _footballDataService.GetPremierLeagueTeamsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string selectedTeam)
        {
            if (string.IsNullOrEmpty(selectedTeam))
            {
                ModelState.AddModelError(string.Empty, "Please select a team.");
                Teams = await _footballDataService.GetPremierLeagueTeamsAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FavoriteTeam = selectedTeam;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, "Failed to update your team selection. Please try again.");
            Teams = await _footballDataService.GetPremierLeagueTeamsAsync();
            return Page();
        }
    }
}
