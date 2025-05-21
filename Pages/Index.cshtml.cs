using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BettingApp.Models;
using BettingApp.Services;
using Microsoft.Extensions.Logging;

namespace BettingApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly FootballDataService _footballDataService;
        private readonly ILogger<IndexModel> _logger;
        public required List<Team> Teams { get; set; } = new();
        public required List<Match> Matches { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime UtcDate { get; set; } = DateTime.Today;

        public IndexModel(FootballDataService footballDataService, ILogger<IndexModel> logger)
        {
            _footballDataService = footballDataService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("[DEBUG] Starting OnGetAsync with SearchTerm: {SearchTerm}", SearchTerm);

                // Handle datetime parameter
                var datetimeParam = HttpContext.Request.Query["datetime"].ToString();
                if (!string.IsNullOrEmpty(datetimeParam) && DateTime.TryParse(datetimeParam, out var parsedDate))
                {
                    UtcDate = parsedDate;
                }

                // Get teams with search
                var allTeams = await _footballDataService.GetPremierLeagueTeamsAsync();
                Teams = string.IsNullOrWhiteSpace(SearchTerm)
                    ? allTeams
                    : allTeams.Where(t => t.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false).ToList();

                _logger.LogInformation("[DEBUG] Found {Count} matching teams", Teams.Count);                // Get matches for the next 30 days from selected date
                string dateFrom = UtcDate.ToString("yyyy-MM-dd");
                string dateTo = UtcDate.AddDays(30).ToString("yyyy-MM-dd");
                _logger.LogInformation("[DEBUG] Date range: {DateFrom} to {DateTo}", dateFrom, dateTo);

                var selectedLeagues = new List<string> { "PL", "CL" };
                var allMatches = await _footballDataService.GetMatchesAsync(dateFrom, dateTo, selectedLeagues);
                
                _logger.LogInformation("[DEBUG] Total matches from API: {Count}", allMatches.Count);

                // Filter matches by searched team
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    _logger.LogInformation("[DEBUG] Filtering matches for team: {SearchTerm}", SearchTerm);
                    
                    // Try exact match first
                    Matches = allMatches.Where(m =>
                        (m.HomeTeam?.Name?.Equals(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (m.AwayTeam?.Name?.Equals(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();

                    // If no exact matches, try contains
                    if (!Matches.Any())
                    {
                        _logger.LogInformation("[DEBUG] No exact matches found, trying partial matches");
                        Matches = allMatches.Where(m =>
                            (m.HomeTeam?.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (m.AwayTeam?.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                        ).ToList();
                    }

                    _logger.LogInformation("[DEBUG] Found {Count} matches for team", Matches.Count);
                    foreach (var match in Matches)
                    {
                        _logger.LogInformation("[DEBUG] Match: {Home} vs {Away} on {Date}", 
                            match.HomeTeam?.Name, match.AwayTeam?.Name, match.UtcDate);
                    }
                }
                else
                {
                    Matches = allMatches.Take(5).ToList();
                    _logger.LogInformation("[DEBUG] No search term, showing {Count} recent matches", Matches.Count);
                }

                // Convert times and calculate odds
                foreach (var match in Matches)
                {
                    try
                    {
                        match.UtcDate = TimeZoneInfo.ConvertTimeFromUtc(match.UtcDate, 
                            TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
                        
                        if (match.HomeTeam != null && match.AwayTeam != null)
                        {
                            match.Odds = CalculateOdds(match.HomeTeam, match.AwayTeam);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[DEBUG] Error processing match times or odds");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DEBUG] Error in OnGetAsync");
                Teams = new List<Team>();
                Matches = new List<Match>();
            }
        }

        private static Odds CalculateOdds(Team homeTeam, Team awayTeam)
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
            
            return new Odds
            {
                HomeWin = 5.0,
                Draw = 4.0,
                AwayWin = 3.0
            };
        }
    }
}







