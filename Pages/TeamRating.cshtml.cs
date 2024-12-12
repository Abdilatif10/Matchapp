using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleApp.Services;
using SimpleApp.Models;


namespace SimpleApp.Pages
{
    public class TeamRatingModel : PageModel
    {
        private readonly FootballDataService _footballDataService;

        public List<Team> Teams { get; set; }
        public TeamRatingModel(FootballDataService footballDataService)
        {
            _footballDataService = footballDataService;
        }
        public async Task OnGetAsync()
        {
            
            Teams = await _footballDataService.GetTeamsAsync();
        }


    }
}
