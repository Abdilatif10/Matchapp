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
            string apiPLUrl = "v4/competitions/PL/teams";
            string apiCLUrl = "v4/competitions/CL/teams";

            List<Team> allTeams = new List<Team>();

            try
            {
                // Hämta PL teams
                HttpResponseMessage responsePL = await _httpClient.GetAsync(apiPLUrl);
                if (responsePL.IsSuccessStatusCode)
                {
                    string responsePLString = await responsePL.Content.ReadAsStringAsync();
                    var dataPL = JsonConvert.DeserializeObject<FootballApiResponse>(responsePLString);
                    foreach (var team in dataPL.Teams)
                    {
                        team.TeamRatingScale = AssignTeamRating(team.Name);
                    }
                    allTeams.AddRange(dataPL.Teams);
                }
                else
                {
                    Console.WriteLine($"PL API-anrop misslyckades med statuskod: {responsePL.StatusCode}");
                }

                // Hämta CL teams
                HttpResponseMessage responseCL = await _httpClient.GetAsync(apiCLUrl);
                if (responseCL.IsSuccessStatusCode)
                {
                    string responseCLString = await responseCL.Content.ReadAsStringAsync();
                    var dataCL = JsonConvert.DeserializeObject<FootballApiResponse>(responseCLString);
                    foreach (var team in dataCL.Teams)
                    {
                        team.TeamRatingScale = AssignTeamRating(team.Name);
                    }
                    allTeams.AddRange(dataCL.Teams);
                }
                else
                {
                    Console.WriteLine($"CL API-anrop misslyckades med statuskod: {responseCL.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel inträffade: {ex.Message}");
            }

            return allTeams;
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
                //CL LAG
                "GNK Dinamo Zagreb" => 3,
                "Celtic FC" => 4,
                "Club Brugge KV" => 4,
                "Girona FC" => 2,
                "Sporting Clube de Portugal" => 4,
                "Atalanta BC" => 5,
                "Real Madrid CF" => 7,
                "Stade Brestois 29" => 3,
                "PSV" => 2,
                "FK Shakhtar Donetsk" => 1,
                "FC Bayern München" => 8,
                "RB Leipzig" => 1,
                "Paris Saint-Germain FC" => 5,
                "FC Red Bull Salzburg" => 1,
                "Bayer 04 Leverkusen" => 6,
                "FC Internazionale Milano" => 8,
                "Club Atlético de Madrid" => 7,
                "ŠK Slovan Bratislava" => 1,
                "Lille OSC" => 6,
                "SK Sturm Graz" => 2,
                "AS Monaco FC" => 6,
                "Borussia Dortmund" => 7,
                "FC Barcelona" => 8,
                "Feyenoord Rotterdam" => 3,
                "AC Sparta Praha" => 2,
                "VfB Stuttgart" => 2,
                "BSC Young Boys" => 1,
                "Juventus FC" => 5,
                "FK Crvena Zvezda" => 1,
                "AC Milan" => 6,
                "Sport Lisboa e Benfica" => 5,
                "Bologna FC 1909" => 1,
                

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
    