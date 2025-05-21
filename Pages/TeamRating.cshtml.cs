using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BettingApp.Services;
using BettingApp.Models;


namespace BettingApp.Pages
{
    public class TeamRatingModel : PageModel
    {
        private readonly FootballDataService _footballDataService;

        public List<Team> Teams { get; set; } = new List<Team>();
        public TeamRatingModel(FootballDataService footballDataService)
        {
            _footballDataService = footballDataService;
        }
        public async Task OnGetAsync()
        {
            Teams = await _footballDataService.GetPremierLeagueTeamsAsync();
        }


    }
}
