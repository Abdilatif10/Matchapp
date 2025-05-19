using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleApp.Models;

namespace SimpleApp.Pages
{
    public class StandingsModel : PageModel
    {
        private readonly FootballDataService _footballDataService;
        public List<Standing> Standings { get; set; } = new();

        public StandingsModel(FootballDataService footballDataService)
        {
            _footballDataService = footballDataService;
        }

        public async Task OnGetAsync()
        {
            Standings = await _footballDataService.GetPremierLeagueStandingsAsync();
        }
    }
}
