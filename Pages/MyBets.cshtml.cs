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
        }        public List<BetViewModel> ActiveBets { get; set; } = new();
        public List<BetViewModel> CompletedBets { get; set; } = new();
        public double UserPoints { get; set; }
        public List<BetViewModel> UserBets { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            UserPoints = user.Points ?? 0;
          
            var userBets = await _dbContext.Bets
                .Where(b => b.UserId == user.Id)
                .ToListAsync();
            
            List<BetViewModel> betViewModels = new List<BetViewModel>();

            foreach (var bet in userBets)
            {
                var match = await _footballDataService.GetMatchByIdAsync(bet.MatchId); 
                if (match == null)
                {
                    Console.WriteLine($"Matchen med ID {bet.MatchId} kunde inte hämtas.");
                    continue;
                }

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

            ActiveBets = betViewModels.Where(b => b.Match.Status != "FINISHED").ToList();
            CompletedBets = betViewModels.Where(b => b.Match.Status == "FINISHED").ToList();
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
