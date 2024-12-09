using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleApp.Data;
using SimpleApp.Models;
using SimpleApp.Services;
using Microsoft.EntityFrameworkCore;

namespace SimpleApp.Pages
{
    public class MyBetsModel : PageModel
    {
        private readonly FootballDataService _footballDataService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public MyBetsModel(FootballDataService footballDataService, UserManager<User> userManager, ApplicationDbContext dbContext)
        {
            _footballDataService = footballDataService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public List<BetViewModel> UserBets { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            // Hämta alla bets för användaren
            var userBets = await _dbContext.Bets
                .Where(b => b.UserId == user.Id)
                .ToListAsync();
            

            // Skapa en lista med BetViewModels
            List<BetViewModel> betViewModels = new List<BetViewModel>();

            foreach (var bet in userBets)
            {
                var match = await _footballDataService.GetMatchByIdAsync(bet.MatchId); // Hämtar matchdata baserat på MatchId
                if (match == null)
                {
                    Console.WriteLine($"Matchen med ID {bet.MatchId} kunde inte hämtas.");
                }


                if (match != null)
                {
                    // Fyll i BetViewModel med Bet och Match
                    var betViewModel = new BetViewModel
                    {
                        Bet = bet,
                        Match = match
                    };

                    betViewModels.Add(betViewModel);
                }
            }

           UserBets = betViewModels; 

            return Page();
        }




    }

}
