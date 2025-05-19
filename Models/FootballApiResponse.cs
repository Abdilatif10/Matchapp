namespace SimpleApp.Models
{
    public class FootballApiResponse
    {
        public Filters Filters { get; set; }
        public ResultSet ResultSet { get; set; }
        public List<Match> Matches { get; set; }
         public List<Team>? Teams { get; set; }
    }
}
