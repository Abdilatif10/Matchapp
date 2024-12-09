using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using SimpleApp.Data;
using SimpleApp.Models;
using SimpleApp.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SimpleApp.Pages
{
    public class PlaceBetModel : PageModel
    {
        private readonly FootballDataService _footballDataService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public Match SelectedMatch { get; set; }

        public PlaceBetModel(FootballDataService footballDataService, UserManager<User> userManager, ApplicationDbContext dbContext)
        {
            _footballDataService = footballDataService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> OnGetAsync(int matchId, DateTime? datetime)
        {
            // Om inget datum skickas används dagens datum
            DateTime startDate = datetime ?? DateTime.Now.Date;
            DateTime endDate = startDate.AddDays(1);

            // Hämta matcher för det valda datumet
            var matches = await _footballDataService.GetMatchesAsync(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), new List<string> { "PL" });

            if (matches == null || !matches.Any())
            {
                TempData["Error"] = "Inga matcher hittades.";
                return RedirectToPage("/Index", new { datetime = startDate.ToString("yyyy-MM-dd") }); // Skicka tillbaka valt datum
            }

            // Hitta den valda matchen baserat på matchId
            SelectedMatch = matches.FirstOrDefault(m => m.Id == matchId);

            if (SelectedMatch == null)
            {
                return NotFound();
            }
            DateTime localMatchTime = TimeZoneInfo.ConvertTimeFromUtc(SelectedMatch.UtcDate, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
            SelectedMatch.UtcDate = localMatchTime; 

            // Beräkna odds för den valda matchen
            SelectedMatch.Odds = CalculateOdds(SelectedMatch.HomeTeam, SelectedMatch.AwayTeam);

            // Kontrollera om matchen redan är spelad
            if (SelectedMatch.UtcDate <= DateTime.UtcNow)
            {
                TempData["Error"] = "Du kan inte lägga bet på en redan spelad match.";
                return RedirectToPage("/Index", new { datetime = startDate.ToString("yyyy-MM-dd") }); // Skicka tillbaka valt datum
            }

            // Associera användaren med matchen
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                SelectedMatch.User = user;
            }

            // Returnera sidan med valt datum (för att behålla sammanhanget)
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

            // Beräkna odds för matchen
            var odds = CalculateOdds(SelectedMatch.HomeTeam, SelectedMatch.AwayTeam);

            // Beräkna potentiell utbetalning beroende på bettypen
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

            // Skapa och spara bet
            var bet = new Bet
            {
                UserId = user.Id,
                MatchId = SelectedMatch.Id,
                Amount = betAmount,
                BetType = betType,
                IsSettled = false, // Spelet är inte avgjort än
                PotentialPayout = potentialPayout,
                HomeWinOdds = odds.HomeWin,
                DrawOdds = odds.Draw,
                AwayWinOdds = odds.AwayWin
            };

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






