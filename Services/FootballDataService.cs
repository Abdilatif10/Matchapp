using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SimpleApp.Models;
using SimpleApp.Services.Constants;
using SimpleApp.Services.Helpers;
using SimpleApp.Services.Interfaces;
using SimpleApp.Services.Models;
using Microsoft.Extensions.Caching.Memory;

namespace SimpleApp.Services
{
    public class FootballDataService : IFootballDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FootballDataService> _logger;
        private readonly IMemoryCache _cache;
        private const string MATCHES_CACHE_KEY = "matches_{0}_{1}_{2}";
        private const string TEAMS_CACHE_KEY = "premier_league_teams";
        private const string MATCH_CACHE_KEY = "match_{0}";
        private const string STANDINGS_CACHE_KEY = "premier_league_standings";
        private static readonly TimeSpan DEFAULT_CACHE_DURATION = TimeSpan.FromMinutes(5);

        public FootballDataService(HttpClient httpClient, ILogger<FootballDataService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
            _httpClient.BaseAddress = new Uri(ApiConstants.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", ApiConstants.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Unfold-Goals", "true");
        }

        public async Task<List<Team>> GetPremierLeagueTeamsAsync()
        {
            if (_cache.TryGetValue<List<Team>>(TEAMS_CACHE_KEY, out var cachedTeams) && cachedTeams != null)
            {
                _logger.LogInformation("Returning Premier League teams from cache");
                return cachedTeams;
            }
            
            string apiUrl = ApiConstants.PremierLeagueTeamsEndpoint;

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

                    foreach (var team in data.Teams)
                    {
                        if (team?.Name != null)
                        {
                            team.TeamRatingScale = (int)TeamHelper.GetTeamRating(team.Name);
                        }
                    }

                    var premierLeagueTeams = data.Teams
                        .Where(t => t.Name != null && TeamHelper.IsPremierLeagueTeam(t.Name))
                        .OrderByDescending(t => t.TeamRatingScale)
                        .ToList();

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(DEFAULT_CACHE_DURATION);
                    _cache.Set(TEAMS_CACHE_KEY, premierLeagueTeams, cacheOptions);

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
            return await GetPremierLeagueTeamsAsync();
        }

        public async Task<List<Match>> GetMatchesAsync(string dateFrom, string dateTo, List<string> leagues)
        {
            string cacheKey = string.Format(MATCHES_CACHE_KEY, dateFrom, dateTo, string.Join("_", leagues));


            if (_cache.TryGetValue<List<Match>>(cacheKey, out var cachedMatches) && cachedMatches != null)
            {
                _logger.LogInformation("Returning matches from cache");
                return cachedMatches;
            }

            string leagueFilter = string.Join(",", leagues);


            DateTime startDate = DateTime.Parse(dateFrom);
            DateTime endDate = DateTime.Parse(dateTo);
            DateTime maxAllowedDate = startDate.AddDays(10);


            string effectiveDateTo = (endDate > maxAllowedDate ? maxAllowedDate : endDate).ToString("yyyy-MM-dd");
            string filterUrl = $"{ApiConstants.MatchesEndpoint}?status=SCHEDULED,TIMED,LIVE&dateFrom={dateFrom}&dateTo={effectiveDateTo}&competitions={leagueFilter}";
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


                foreach (var match in data.Matches)
                {
                    if (match.HomeTeam != null)
                    {
                        match.HomeTeam.TeamRatingScale = (int)TeamHelper.GetTeamRating(match.HomeTeam.Name);
                    }
                    if (match.AwayTeam != null)
                    {
                        match.AwayTeam.TeamRatingScale = (int)TeamHelper.GetTeamRating(match.AwayTeam.Name);
                    }
                }
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                _cache.Set(cacheKey, data.Matches, cacheOptions);

                return data.Matches ?? new List<Match>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing matches data");
                return new List<Match>();
            }
        }

        public async Task<Match?> GetMatchByIdAsync(int matchId)
        {
            string cacheKey = string.Format(MATCH_CACHE_KEY, matchId);


            if (_cache.TryGetValue<Match>(cacheKey, out var cachedMatch) && cachedMatch != null)
            {
                _logger.LogInformation("Returning match from cache");
                return cachedMatch;
            }

            var response = await _httpClient.GetAsync($"v4/matches/{matchId}");


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
                    match.HomeTeam.TeamRatingScale = (int)TeamHelper.GetTeamRating(match.HomeTeam.Name);
                }

                if (match.AwayTeam?.Name != null)
                {
                    match.AwayTeam.TeamRatingScale = (int)TeamHelper.GetTeamRating(match.AwayTeam.Name);
                }


                var cacheDuration = match.Status switch
                {
                    "LIVE" => TimeSpan.FromSeconds(30),
                    "FINISHED" => TimeSpan.FromHours(24),
                    _ => TimeSpan.FromMinutes(5)
                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(cacheDuration);
                _cache.Set(cacheKey, match, cacheOptions);

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

            if (_cache.TryGetValue<List<Standing>>(STANDINGS_CACHE_KEY, out var cachedStandings) && cachedStandings != null)
            {
                _logger.LogInformation("Returning standings from cache");
                return cachedStandings;
            }

            string apiUrl = ApiConstants.StandingsEndpoint;

            try
            {
                var response = await _httpClient.GetAsync(apiUrl); if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Premier League standings API call failed with status code: {response.StatusCode}");
                    return new List<Standing>();
                }
                var responseString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<SimpleApp.Services.Models.StandingResponse>(responseString);
                if (data?.Standings == null || !data.Standings.Any())
                {
                    _logger.LogWarning("No standings data found in the API response");
                    return new List<Standing>();
                }

                var totalStandings = data.Standings.FirstOrDefault(s => s.Type?.ToUpper() == "TOTAL");
                if (totalStandings?.Table == null || !totalStandings.Table.Any())
                {
                    _logger.LogWarning("No table data found in the standings");
                    return new List<Standing>();
                }

                var standings = totalStandings.Table.Select(item => new Standing
                {
                    Position = item.Position,
                    Team = item.Team == null ? null : new Team
                    {
                        Name = item.Team.Name ?? "Unknown Team",
                        Crest = item.Team.Crest ?? "",
                        TeamRatingScale = item.Team.Name != null ? (int)TeamHelper.GetTeamRating(item.Team.Name) : 0
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


                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set(STANDINGS_CACHE_KEY, standings, cacheOptions);

                return standings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching standings");
                return new List<Standing>();
            }
        }
    }
}