using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleApp.Models;
using SimpleApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly FootballDataService _footballDataService;
        public List<Match> Matches { get; set; }
        
        public IndexModel(FootballDataService footballDataService)
        {
            _footballDataService = footballDataService;
        }

        public async Task OnGetAsync()
        {
            string dateFrom = "2024-11-22"; // Exempel på startdatum
            string dateTo = "2024-11-25";   // Exempel på slutdatum
            var selectedLeagues = new List<string> {"PL"};
            Matches = await _footballDataService.GetMatchesAsync(dateFrom, dateTo, selectedLeagues);
        }
        //test
    }


}
