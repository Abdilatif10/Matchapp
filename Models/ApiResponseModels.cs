using SimpleApp.Models;

namespace SimpleApp.Services.Models
{
    public class TeamsApiResponse
    {
        public required List<Team>? Teams { get; set; }
    }

    public class FootballApiResponse
    {
        public required List<Match>? Matches { get; set; }
        public required List<Team>? Teams { get; set; }
    }

    public class StandingResponse
    {
        public required List<StandingGroup>? Standings { get; set; }
    }
}
