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

          
            var userBets = await _dbContext.Bets
                .Where(b => b.UserId == user.Id)
                .ToListAsync();
            

           
            List<BetViewModel> betViewModels = new List<BetViewModel>();

            foreach (var bet in userBets)
            {
                var match = await _footballDataService.GetMatchByIdAsync(bet.MatchId); 
                if (match == null)
                {
                    Console.WriteLine($"Matchen med ID {bet.MatchId} kunde inte h�mtas.");
                }


                if (match != null)
                {
                    
                    var betViewModel = new BetViewModel
                    {
                        Bet = bet,
                        Match = match,
                        IsWon = IsBetWon(bet, match)

                    };
                    if (match.Status == "FINISHED" && IsBetWon(bet, match) && !bet.IsPayoutDone)
                    {
                        user.Points += bet.PotentialPayout; 
                        bet.IsPayoutDone = true; 
                    }


                    betViewModels.Add(betViewModel);
                }
            }

           UserBets = betViewModels;
           await _dbContext.SaveChangesAsync();

            return Page();
        }
        private bool IsBetWon(Bet bet, Match match)
        {
            var homeScore = match.Score.FullTime.Home;
            var awayScore = match.Score.FullTime.Away;

            return bet.BetType switch
            {
                "HomeWin" => homeScore > awayScore,
                "Draw" => homeScore == awayScore,
                "AwayWin" => homeScore < awayScore,
                _ => false
            };
        }





    }

}
