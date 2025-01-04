using SimpleApp.Pages;
using SimpleApp.Models;
using SimpleApp.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
public class Tests
{
    [Fact]
    public void CalculateOdds_ShouldReturnBalancedOdds_WhenTeamsAreEvenlyMatched()
    {

        var placeBetModel = new PlaceBetModel(null, null, null);

        var homeTeam = new Team { Name = "Tottenham Hotspur FC", TeamRatingScale = 7 };
        var awayTeam = new Team { Name = "Manchester United FC", TeamRatingScale = 7 };


        var result = placeBetModel.CalculateOdds(homeTeam, awayTeam);


        Assert.Equal(2.0, result.HomeWin);
        Assert.Equal(4.0, result.Draw);
        Assert.Equal(3.0, result.AwayWin);
    }

    [Fact]
    public void CalculateOdds_ShouldReturnHomeFavoriteOdds_WhenHomeTeamIsBetter()
    {

        var placeBetModel = new PlaceBetModel(null, null, null);

        var homeTeam = new Team { Name = "Manchester City FC", TeamRatingScale = 10 };
        var awayTeam = new Team { Name = "Aston Villa FC", TeamRatingScale = 6 };


        var result = placeBetModel.CalculateOdds(homeTeam, awayTeam);


        Assert.Equal(2.0, result.HomeWin);
        Assert.Equal(3.0, result.Draw);
        Assert.Equal(5.0, result.AwayWin);
    }

    [Fact]
    public void CalculateOdds_ShouldReturnAwayFavoriteOdds_WhenAwayTeamIsBetter()
    {

        var placeBetModel = new PlaceBetModel(null, null, null);

        var homeTeam = new Team { Name = "Chelsea", TeamRatingScale = 7 };
        var awayTeam = new Team { Name = "Liverpool FC", TeamRatingScale = 10 };


        var result = placeBetModel.CalculateOdds(homeTeam, awayTeam);


        Assert.Equal(5.0, result.HomeWin);
        Assert.Equal(4.0, result.Draw);
        Assert.Equal(3.0, result.AwayWin);
    }
}
public class LeaderBoardModelTests
{
    private ApplicationDbContext GetTestDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        return new ApplicationDbContext(options);
    }
    [Fact]
    public async Task LeaderBoardModel_OnGetAsync_ReturnsUsersOrderedByPoints()
    {

        var dbContext = GetTestDbContext();

        dbContext.Users.AddRange(
      new User { Id = "1", UserName = "Vini", Points = 40, FirstName = "Vinicius" },
      new User { Id = "2", UserName = "Regex", Points = 87, FirstName = "Reginald" },
      new User { Id = "3", UserName = "Rasmus", Points = 330, FirstName = "Rasmus" }
  );

        await dbContext.SaveChangesAsync();

        var leaderboardModel = new LeaderBoardModel(dbContext);


        await leaderboardModel.OnGetAsync();


        Assert.NotNull(leaderboardModel.Users);
        Assert.Equal(3, leaderboardModel.Users.Count);


        var expectedOrder = new[] { "Rasmus", "Regex", "Vini" };
        var actualOrder = leaderboardModel.Users.Select(u => u.UserName).ToArray();

        Assert.Equal(expectedOrder, actualOrder);
    }
}