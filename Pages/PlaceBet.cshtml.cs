using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleApp.Data;
using SimpleApp.Models;
using SimpleApp.Services;
using SimpleApp.Services.Interfaces;

namespace SimpleApp.Pages
{    
    public class PlaceBetModel : PageModel
    {
        private readonly IFootballDataService _footballDataService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public Match? SelectedMatch { get; set; }

        [BindProperty(SupportsGet = true)]
        public int MatchId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Datetime { get; set; }

        public PlaceBetModel(IFootballDataService footballDataService, UserManager<User> userManager, ApplicationDbContext dbContext)
        {
            _footballDataService = footballDataService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (MatchId == 0)
            {
                return RedirectToPage("/Index");
            }

            DateTime startDate = !string.IsNullOrEmpty(Datetime) ? DateTime.Parse(Datetime) : DateTime.Now.Date;
            DateTime endDate = startDate.AddDays(1);

            var match = await _footballDataService.GetMatchByIdAsync(MatchId);
            
            if (match == null)
            {
                TempData["Error"] = "Match not found.";
                return RedirectToPage("/Index");
            }

            SelectedMatch = match;
            
          
            var timezone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            SelectedMatch.UtcDate = TimeZoneInfo.ConvertTimeFromUtc(SelectedMatch.UtcDate, timezone);

            
            if (SelectedMatch.UtcDate <= DateTime.Now)
            {
                TempData["Error"] = "Cannot place bets on matches that have already started.";
                return RedirectToPage("/Index");
            }

           
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            ViewData["SelectedDate"] = startDate;
            return Page();
        }

        public async Task<IActionResult> OnPostPlaceBetAsync(int matchId, int betAmount, string betType)
        {
            var matches = await _footballDataService.GetMatchByIdAsync(matchId);
            var user = await _userManager.GetUserAsync(User);
            SelectedMatch = matches;

            if (SelectedMatch == null)
            {
                TempData["Error"] = "Matchen kunde inte hittas.";
                return RedirectToPage("/Index");
            } 
             if (user == null || user.Points == null || betAmount > user.Points)
            {
                TempData["Error"] = "Du har inte tillräckligt med poäng.";
                return RedirectToPage("/MyBets");
            }

            if (SelectedMatch?.UtcDate <= DateTime.UtcNow)
            {
                TempData["Error"] = "Du kan inte lägga bet på en redan spelad match.";
                return RedirectToPage("/Index");
            }            if (SelectedMatch?.HomeTeam == null || SelectedMatch?.AwayTeam == null)
            {
                TempData["Error"] = "Felaktig matchdata.";
                return RedirectToPage("/MyBets");
            }

            var odds = CalculateOdds(SelectedMatch.HomeTeam, SelectedMatch.AwayTeam);

            
            double potentialPayout = 0;
            switch (betType)
            {
                case "HomeWin":
                    potentialPayout = betAmount * odds.HomeWin;
                    break;
                case "Draw":
                    potentialPayout = betAmount * odds.Draw;
                    break;
                case "AwayWin":
                    potentialPayout = betAmount * odds.AwayWin;
                    break;
                default:
                    TempData["Error"] = "Ogiltig bet-typ.";
                    return RedirectToPage("/MyBets");
            }

          
            var bet = new Bet
            {
                UserId = user.Id,
                MatchId = SelectedMatch.Id,
                Amount = betAmount,
                BetType = betType,
                IsSettled = false, 
                PotentialPayout = potentialPayout,
                HomeWinOdds = odds.HomeWin,
                DrawOdds = odds.Draw,
                AwayWinOdds = odds.AwayWin
            };

            _dbContext.Bets.Add(bet);
            await _dbContext.SaveChangesAsync();

           

            user.Points -= betAmount;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Ditt bet är lagt!";
            return RedirectToPage("/MyBets");
        }

        public Odds CalculateOdds(Team homeTeam, Team awayTeam)
        {

            bool isHomeFavorite = homeTeam.TeamRatingScale > awayTeam.TeamRatingScale;

          
            if (Math.Abs(homeTeam.TeamRatingScale - awayTeam.TeamRatingScale) <= 1)
            {
                return new Odds
                {
                    HomeWin = 2.0,
                    Draw = 4.0,
                    AwayWin = 3.0
                };
            }

     
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








