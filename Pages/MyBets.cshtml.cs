using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using SimpleApp.Data;
using SimpleApp.Models;
using SimpleApp.Services;

namespace SimpleApp.Pages
{
    public class MyBetsModel : PageModel
    {
        private readonly FootballDataService _footballDataService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public Match SelectedMatch { get; set; }
       
        public MyBetsModel(FootballDataService footballDataService, UserManager<User> userManager, ApplicationDbContext dbContext)
        {
            _footballDataService = footballDataService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> OnGetAsync(int matchId)
        {
            var matches = await _footballDataService.GetMatchesAsync(DateTime.Now.Date.ToString("yyyy-MM-dd"), DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), new List<string> { "PL" });

            if (matches == null || !matches.Any())
            {
                TempData["Error"] = "Inga matcher hittades.";
                return RedirectToPage("/Index");
            }

            SelectedMatch = matches.FirstOrDefault(m => m.Id == matchId);
           

            if (SelectedMatch == null)
            {
                return NotFound();
            }
            SelectedMatch.Odds = CalculateOdds(SelectedMatch.HomeTeam, SelectedMatch.AwayTeam);


            if (SelectedMatch.UtcDate <= DateTime.UtcNow)
            {
                TempData["Error"] = "Du kan inte lägga bet på en redan spelad match.";
                return RedirectToPage("/Index");
            }

          
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                SelectedMatch.User = user; 
            }
            return Page();

           
        }

      
       
        public async Task<IActionResult> OnPostPlaceBetAsync(int matchId, int betAmount, string betType)    
        {
          
            var matches = await _footballDataService.GetMatchesAsync(DateTime.Now.Date.ToString("yyyy-MM-dd"), DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), new List<string> { "PL" });
            var user = await _userManager.GetUserAsync(User);
            SelectedMatch = matches.FirstOrDefault(m => m.Id == matchId);
           
            if (SelectedMatch == null)
            {
                TempData["Error"] = "Matchen kunde inte hittas.";
                return RedirectToPage("/Index");
            }

            if (betAmount > user.Points)
            {
                TempData["Error"] = "Du har inte tillräckligt med poäng.";
                return RedirectToPage("/MyBets");
            }


            if (SelectedMatch?.UtcDate <= DateTime.UtcNow)
            {
                TempData["Error"] = "Du kan inte lägga bet på en redan spelad match.";
                return RedirectToPage("/Index");
            }

            // Beräkna den potentiella utbetalningen beroende på odds
            double potentialPayout = 0;
            switch (betType)
            {
                case "HomeWin":
                    potentialPayout = betAmount * SelectedMatch.Odds.HomeWin;
                    break;
                case "Draw":
                    potentialPayout = betAmount * SelectedMatch.Odds.Draw;
                    break;
                case "AwayWin":
                    potentialPayout = betAmount * SelectedMatch.Odds.AwayWin;
                    break;
                default:
                    TempData["Error"] = "Ogiltig bet-typ.";
                    return RedirectToPage("/MyBets");
            }

            // Skapa ett bet och spara det
            var bet = new Bet
            {
                UserId = user.Id,
                MatchId = SelectedMatch.Id,
                Amount = betAmount,
                BetType = betType,
                IsSettled = false, // Spelet är inte avgjort än
                PotentialPayout = potentialPayout,
                HomeWinOdds = SelectedMatch.Odds.HomeWin,
                DrawOdds = SelectedMatch.Odds.Draw,
                AwayWinOdds = SelectedMatch.Odds.AwayWin
            };

            // Lägg till betet i databasen
            _dbContext.Bets.Add(bet);
            await _dbContext.SaveChangesAsync();

            // Uppdatera användarens poäng
            user.Points -= betAmount;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Ditt bet är lagt!";
            return RedirectToPage("/MyBets");
        }
        public Odds CalculateOdds(Team homeTeam, Team awayTeam)
        {

            bool isHomeFavorite = homeTeam.TeamRatingScale > awayTeam.TeamRatingScale;

            // Om det är en jämn match
            if (Math.Abs(homeTeam.TeamRatingScale - awayTeam.TeamRatingScale) <= 1)
            {
                return new Odds
                {
                    HomeWin = 2.0,
                    Draw = 4.0,
                    AwayWin = 3.0
                };
            }

            // Om hemmalaget är favoriten
            if (isHomeFavorite)
            {
                return new Odds
                {
                    HomeWin = 2.0,
                    Draw = 3.0,
                    AwayWin = 5.0
                };
            }
            else
            {

                return new Odds
                {
                    HomeWin = 5.0,
                    Draw = 4.0,
                    AwayWin = 3.0
                };
            }


        }
    }
}






