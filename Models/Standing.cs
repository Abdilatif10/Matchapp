namespace SimpleApp.Models
{
    public class Standing
    {
        public required int Position { get; set; }
        public required Team? Team { get; set; }
        public required int PlayedGames { get; set; }
        public required int Points { get; set; }
        public required int Won { get; set; }
        public required int Draw { get; set; }
        public required int Lost { get; set; }
        public required int GoalsFor { get; set; }
        public required int GoalsAgainst { get; set; }
        public required int GoalDifference { get; set; }
    }

    public class StandingResponse
    {
        public required SeasonInfo? Season { get; set; }
        public required List<StandingGroup>? Standings { get; set; }
    }

    public class StandingGroup
    {
        public required string? Stage { get; set; }
        public required string? Type { get; set; }
        public required List<StandingTable>? Table { get; set; }
    }

    public class StandingTable
    {
        public required int Position { get; set; }
        public required Team? Team { get; set; }
        public required int PlayedGames { get; set; }
        public required int Points { get; set; }
        public required int Won { get; set; }
        public required int Draw { get; set; }
        public required int Lost { get; set; }
        public required int GoalsFor { get; set; }
        public required int GoalsAgainst { get; set; }
        public required int GoalDifference { get; set; }
    }

    public class SeasonInfo
    {
        public required int Id { get; set; }
        public required string? StartDate { get; set; }
        public required string? EndDate { get; set; }
        public required int? CurrentMatchday { get; set; }
        public required string? Winner { get; set; }
    }
}
