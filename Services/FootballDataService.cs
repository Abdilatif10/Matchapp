using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using BettingApp.Models;

namespace BettingApp.Services
{      public class FootballDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FootballDataService> _logger;
        private const string ApiUrl = "v4/matches";
        private const string ApiKey = "328dbcfa6fc4408f9e7c8e9b7a8cc1c0";

        public FootballDataService(HttpClient httpClient, ILogger<FootballDataService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://api.football-data.org/");
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Unfold-Goals", "true");
        }        private class TeamsApiResponse
        {
            public required List<Team>? Teams { get; set; }
        }

        private class FootballApiResponse
        {
            public required List<Match>? Matches { get; set; }
            public required List<Team>? Teams { get; set; }
        }

        private int AssignTeamRating(string? teamName)
        {
            if (string.IsNullOrEmpty(teamName))
                return 0;            return teamName switch
            {
                "Manchester City FC" => 10,
                "Liverpool FC" => 10,
                "Imomerycath FC" => 10,
                "Arsenal FC" => 9,
                "Manchester United FC" => 9,
                "Chelsea FC" => 8,
                "Tottenham Hotspur FC" => 8,
                "Newcastle United FC" => 8,
                "Aston Villa FC" => 7,
                "Brighton & Hove Albion FC" => 7,
                "West Ham United FC" => 7,
                "Brentford FC" => 6,
                "Crystal Palace FC" => 6,
                "Fulham FC" => 6,
                "Wolverhampton Wanderers FC" => 5,
                "Everton FC" => 5,
                "Nottingham Forest FC" => 5,
                "AFC Bournemouth" => 4,
                "Luton Town FC" => 4,
                "Burnley FC" => 4,
                "Sheffield United FC" => 4,
                _ => 5 // Default rating for unknown teams
            };
        }

        private bool IsPremierLeagueTeam(string? teamName)
        {
            if (string.IsNullOrEmpty(teamName))
                return false;            return teamName switch
            {
                "Manchester City FC" => true,
                "Arsenal FC" => true,
                "Liverpool FC" => true,
                "Imomerycath FC" => true,
                "Tottenham Hotspur FC" => true,
                "Newcastle United FC" => true,
                "Manchester United FC" => true,
                "Chelsea FC" => true,
                "Aston Villa FC" => true,
                "Brighton & Hove Albion FC" => true,
                "West Ham United FC" => true,
                "Brentford FC" => true,
                "Crystal Palace FC" => true,
                "Fulham FC" => true,
                "Wolverhampton Wanderers FC" => true,
                "Everton FC" => true,
                "Nottingham Forest FC" => true,
                "AFC Bournemouth" => true,
                "Luton Town FC" => true,
                "Burnley FC" => true,
                "Sheffield United FC" => true,
                _ => false
            };
        }        public async Task<List<Team>> GetPremierLeagueTeamsAsync()
        {
            string apiUrl = "v4/competitions/PL/teams";

            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<TeamsApiResponse>(responseString);
                    
                    if (data?.Teams == null)
                    {
                        _logger.LogWarning("No teams data received from the API");
                        return new List<Team>();
                    }

                    // Keep API data but use our existing ratings
                    foreach (var team in data.Teams)
                    {
                        if (team?.Name != null)
                        {
                            team.TeamRatingScale = AssignTeamRating(team.Name);
                        }
                    }

                    // Only return Premier League teams ordered by their rating
                    var premierLeagueTeams = data.Teams
                        .Where(t => t.Name != null && IsPremierLeagueTeam(t.Name))
                        .OrderByDescending(t => t.TeamRatingScale)
                        .ToList();

                    return premierLeagueTeams;
                }
                
                _logger.LogWarning($"Premier League API call failed with status code: {response.StatusCode}");
                return new List<Team>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Premier League teams");
                return new List<Team>();
            }
        }

        public async Task<List<Team>> GetTeamsAsync()
        {
            string apiPLUrl = "v4/competitions/PL/teams";
            string apiCLUrl = "v4/competitions/CL/teams";

            var allTeams = new List<Team>();

            try
            {
                // Get PL teams
                var responsePL = await _httpClient.GetAsync(apiPLUrl);
                if (responsePL.IsSuccessStatusCode)
                {
                    var responsePLString = await responsePL.Content.ReadAsStringAsync();
                    var dataPL = JsonConvert.DeserializeObject<FootballApiResponse>(responsePLString);
                    
                    if (dataPL?.Teams != null)
                    {
                        foreach (var team in dataPL.Teams)
                        {
                            if (team != null)
                            {
                                team.TeamRatingScale = AssignTeamRating(team.Name);
                            }
                        }
                        allTeams.AddRange(dataPL.Teams);
                    }
                }
                else
                {
                    _logger.LogWarning($"Premier League API call failed with status code: {responsePL.StatusCode}");
                }

                // Get CL teams
                var responseCL = await _httpClient.GetAsync(apiCLUrl);
                if (responseCL.IsSuccessStatusCode)
                {
                    var responseCLString = await responseCL.Content.ReadAsStringAsync();
                    var dataCL = JsonConvert.DeserializeObject<FootballApiResponse>(responseCLString);
                    
                    if (dataCL?.Teams != null)
                    {
                        foreach (var team in dataCL.Teams)
                        {
                            if (team != null)
                            {
                                team.TeamRatingScale = AssignTeamRating(team.Name);
                            }
                        }
                        allTeams.AddRange(dataCL.Teams);
                    }
                }
                else
                {
                    _logger.LogWarning($"Champions League API call failed with status code: {responseCL.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching teams");
            }

            return allTeams;
        }        public async Task<List<Match>> GetMatchesAsync(string dateFrom, string dateTo, List<string> leagues)
        {
            string leagueFilter = string.Join(",", leagues);
            
            // API only allows fetching matches up to 10 days ahead
            DateTime startDate = DateTime.Parse(dateFrom);
            DateTime endDate = DateTime.Parse(dateTo);
            DateTime maxAllowedDate = startDate.AddDays(10);
            
            // Use the earlier of the requested end date or max allowed date
            string effectiveDateTo = (endDate > maxAllowedDate ? maxAllowedDate : endDate).ToString("yyyy-MM-dd");
            string filterUrl = $"{ApiUrl}?status=SCHEDULED,TIMED,LIVE&dateFrom={dateFrom}&dateTo={effectiveDateTo}&competitions={leagueFilter}";
            _logger.LogInformation($"Fetching matches with URL: {filterUrl}");
            
            var response = await _httpClient.GetAsync(filterUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"API call failed with status code: {response.StatusCode}");
                return new List<Match>();
            }

            try
            {
                string responseString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<FootballApiResponse>(responseString);

                if (data?.Matches == null)
                {
                    _logger.LogWarning("No matches data received from the API");
                    return new List<Match>();
                }

                // Update team ratings for each match
                foreach (var match in data.Matches)
                {
                    if (match.HomeTeam != null)
                    {
                        match.HomeTeam.TeamRatingScale = AssignTeamRating(match.HomeTeam.Name);
                    }
                    if (match.AwayTeam != null)
                    {
                        match.AwayTeam.TeamRatingScale = AssignTeamRating(match.AwayTeam.Name);
                    }
                }

                return data.Matches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing matches data");
                return new List<Match>();
            }
        }

        public async Task<Match?> GetMatchByIdAsync(int matchId)
        {
            var response = await _httpClient.GetAsync($"v4/matches/{matchId}");

            // Log the status code and response
            _logger.LogInformation($"API Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"API Response: {content}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to get match with ID {matchId}. Status: {response.StatusCode}");
                return null;
            }

            try
            {
                var match = JsonConvert.DeserializeObject<Match>(content);
                if (match == null)
                {
                    _logger.LogError("Failed to deserialize match data");
                    return null;
                }

                if (match.HomeTeam?.Name != null)
                {
                    match.HomeTeam.TeamRatingScale = AssignTeamRating(match.HomeTeam.Name);
                }

                if (match.AwayTeam?.Name != null)
                {
                    match.AwayTeam.TeamRatingScale = AssignTeamRating(match.AwayTeam.Name);
                }

                return match;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deserializing match {matchId}");
                return null;
            }
        }



        public async Task<List<Standing>> GetPremierLeagueStandingsAsync()
        {
            string apiUrl = "v4/competitions/PL/standings";

            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {                    _logger.LogWarning($"Premier League standings API call failed with status code: {response.StatusCode}");
                    return GetMockStandings();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<StandingResponse>(responseString);
                
                if (data?.Standings == null || !data.Standings.Any())
                {                    _logger.LogWarning("No standings data found in the API response");
                    return GetMockStandings();
                }

                // Get the total standings (usually the first group)
                var totalStandings = data.Standings.FirstOrDefault(s => s.Type?.ToUpper() == "TOTAL");
                if (totalStandings?.Table == null || !totalStandings.Table.Any())
                {
                    _logger.LogWarning("No table data found in the standings");
                    return GetMockStandings();
                }

                var standings = totalStandings.Table.Select(item => new Standing
                {
                    Position = item.Position,
                    Team = item.Team == null ? null : new Team
                    {
                        Name = item.Team.Name ?? "Unknown Team",
                        Crest = item.Team.Crest ?? "",
                        TeamRatingScale = item.Team.Name != null ? AssignTeamRating(item.Team.Name) : 0
                    },
                    PlayedGames = item.PlayedGames,
                    Points = item.Points,
                    Won = item.Won,
                    Draw = item.Draw,
                    Lost = item.Lost,
                    GoalsFor = item.GoalsFor,
                    GoalsAgainst = item.GoalsAgainst,
                    GoalDifference = item.GoalDifference
                }).ToList();

                return standings;
            }
            catch (Exception ex)
            {                _logger.LogError(ex, "An error occurred while fetching standings");
                return GetMockStandings();
            }
        }

        private List<Standing> GetMockStandings()
        {
            return new List<Standing>
            {
                CreateStanding(1, "Manchester City FC", 28, 63, 20, 3, 5, 63, 26, 37),
                CreateStanding(2, "Liverpool FC", 28, 62, 19, 5, 4, 65, 25, 40),
                CreateStanding(3, "Arsenal FC", 28, 61, 19, 4, 5, 62, 24, 38),
                CreateStanding(4, "Imomerycath FC", 28, 60, 18, 6, 4, 58, 28, 30),
                CreateStanding(5, "Tottenham Hotspur FC", 28, 53, 16, 5, 7, 55, 39, 16),
                CreateStanding(6, "Manchester United FC", 28, 50, 15, 5, 8, 40, 36, 4),
                CreateStanding(7, "Chelsea FC", 28, 48, 14, 6, 8, 45, 35, 10),
                CreateStanding(8, "Newcastle United FC", 28, 47, 14, 5, 9, 58, 41, 17),
                CreateStanding(9, "Aston Villa FC", 28, 46, 14, 4, 10, 48, 42, 6),
                CreateStanding(10, "Brighton & Hove Albion FC", 28, 42, 11, 9, 8, 49, 44, 5),
                CreateStanding(11, "West Ham United FC", 28, 39, 11, 6, 11, 40, 50, -10),
                CreateStanding(12, "Brentford FC", 28, 35, 9, 8, 11, 40, 44, -4),
                CreateStanding(13, "Crystal Palace FC", 28, 31, 8, 7, 13, 31, 48, -17),
                CreateStanding(14, "Fulham FC", 28, 31, 8, 7, 13, 36, 43, -7),
                CreateStanding(15, "Wolverhampton Wanderers FC", 28, 29, 8, 5, 15, 39, 51, -12),
                CreateStanding(16, "Everton FC", 28, 25, 8, 7, 13, 27, 39, -12),
                CreateStanding(17, "Nottingham Forest FC", 28, 24, 6, 6, 16, 34, 48, -14),
                CreateStanding(18, "Luton Town FC", 28, 20, 5, 5, 18, 35, 54, -19),
                CreateStanding(19, "Burnley FC", 28, 17, 4, 5, 19, 25, 58, -33),
                CreateStanding(20, "Sheffield United FC", 28, 14, 3, 5, 20, 24, 77, -53)
            };
        }

        private Standing CreateStanding(int position, string teamName, int played, int points, int won, int draw, int lost, int goalsFor, int goalsAgainst, int goalDifference)
        {
            return new Standing
            {
                Position = position,
                Team = new Team
                {
                    Name = teamName,
                    TeamRatingScale = AssignTeamRating(teamName),
                    Crest = $"https://crests.football-data.org/{teamName.ToLower().Replace(" ", "-")}.png"
                },
                PlayedGames = played,
                Points = points,
                Won = won,
                Draw = draw,
                Lost = lost,
                GoalsFor = goalsFor,
                GoalsAgainst = goalsAgainst,
                GoalDifference = goalDifference
            };
        }
    }
}
