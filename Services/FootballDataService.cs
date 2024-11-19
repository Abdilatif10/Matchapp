
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
        private const string ApiKey = "";

        public FootballDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.football-data.org/");
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Unfold-Goals", "true");
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
                return data.Matches;
            }
            else
            {
                Console.WriteLine($"API-anrop misslyckades med statuskod: {response.StatusCode}");
                return new List<Match>();
            }
        }
        public class FootballApiResponse
        {
            public List<Match> Matches { get; set; }
        }

    }
}