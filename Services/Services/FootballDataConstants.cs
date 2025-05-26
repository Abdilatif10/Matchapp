namespace SimpleApp.Services
{
    public static class FootballDataConstants
    {
        public const string ApiUrl = "v4/matches";
        public const string ApiKey = "328dbcfa6fc4408f9e7c8e9b7a8cc1c0";
        public const string BaseUrl = "https://api.football-data.org/";
        public const string TeamsEndpoint = "v4/competitions/PL/teams";
        public const string StandingsEndpoint = "v4/competitions/PL/standings";
        public const string ChampionsLeagueTeamsEndpoint = "v4/competitions/CL/teams";

        public static class TeamRatings
        {
            public const int TopTier = 10;
            public const int HighTier = 9;
            public const int UpperMidTier = 8;
            public const int MidTier = 7;
            public const int LowerMidTier = 6;
            public const int LowTier = 5;
            public const int BottomTier = 4;
            public const int DefaultRating = 5;
        }
    }
}
