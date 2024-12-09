using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using SimpleApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SimpleApp.Services
{
    public class FootballDataService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "v4/matches";
        private const string ApiKey = "328dbcfa6fc4408f9e7c8e9b7a8cc1c0";

        public FootballDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.football-data.org/");
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Unfold-Goals", "true");
        }
        public async Task<List<Team>> GetTeamsAsync()
        {
            string apiUrl = "v4/competitions/PL/teams";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<FootballApiResponse>(responseString);

                foreach (var team in data.Teams)
                {
                    team.TeamRatingScale = AssignTeamRating(team.Name);
                }
                return data.Teams;



            }
            else
            {
                Console.WriteLine($"API-anrop misslyckades med statuskod: {response.StatusCode}");
                return new List<Team>();
            }
        }


        public async Task<List<Match>> GetMatchesAsync(string dateFrom, string dateTo, List<string> leagues)
        {
            string leagueFilter = string.Join(",", leagues);
            string filterUrl = $"{ApiUrl}?dateFrom={dateFrom}&dateTo={dateTo}&competitions={leagueFilter}";
            HttpResponseMessage response = await _httpClient.GetAsync(filterUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<FootballApiResponse>(responseString);

                // Uppdatera teamrating för varje match
                foreach (var match in data.Matches)
                {
                    // Sätt teamrating för både hemmalag och bortalag
                    match.HomeTeam.TeamRatingScale = AssignTeamRating(match.HomeTeam.Name);
                    match.AwayTeam.TeamRatingScale = AssignTeamRating(match.AwayTeam.Name);
                }

                return data.Matches;
            }
            else
            {
                Console.WriteLine($"API-anrop misslyckades med statuskod: {response.StatusCode}");
                return new List<Match>();
            }
        }
        public int AssignTeamRating(string teamName)
        {
            return teamName switch
            {
                "Manchester City FC" => 10,
                "Arsenal FC" => 9,
                "Liverpool FC" => 10,
                "Tottenham Hotspur FC" => 7,
                "Newcastle United FC" => 6,
                "Manchester United FC" => 7,
                "Chelsea FC" => 7,
                "Aston Villa FC" => 6,
                "Brighton & Hove Albion FC" => 5,
                "West Ham United FC" => 4,
                "Brentford FC" => 4,
                "Crystal Palace FC" => 2,
                "Fulham FC" => 4,
                "Wolverhampton Wanderers FC" => 3,
                "Everton FC" => 3,
                "Leicester City FC" => 3,
                "Ipswich Town FC" => 2,
                "Southampton FC" => 1,
                "AFC Bournemouth" => 5,
                "Nottingham Forest FC" => 4,

            };
        }
        public class FootballApiResponse
        {
            public List<Match> Matches { get; set; }
            public List<Team> Teams { get; set; }
        }


        public async Task<Match> GetMatchByIdAsync(int matchId)
        {
            var response = await _httpClient.GetAsync($"https://api.football-data.org/v4/matches/{matchId}");

            // Logga statuskoden och svaret
            Console.WriteLine($"API Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {content}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                var match = JsonConvert.DeserializeObject<Match>(content);
                match.HomeTeam.TeamRatingScale = AssignTeamRating(match.HomeTeam.Name);
                match.AwayTeam.TeamRatingScale = AssignTeamRating(match.AwayTeam.Name);
                return match;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing match: {ex.Message}");
                return null;
            }
        }

    }
}
    