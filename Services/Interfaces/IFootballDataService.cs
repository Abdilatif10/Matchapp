using SimpleApp.Models;

namespace SimpleApp.Services.Interfaces
{
    public interface IFootballDataService
    {
        
        Task<List<Team>> GetPremierLeagueTeamsAsync();

     
        Task<List<Team>> GetTeamsAsync();

       
        Task<List<Match>> GetMatchesAsync(string dateFrom, string dateTo, List<string> leagues);

        
        Task<Match?> GetMatchByIdAsync(int matchId);

        
        Task<List<Standing>> GetPremierLeagueStandingsAsync();
    }
}
