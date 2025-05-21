using BettingApp.Pages;
using BettingApp.Models;
using BettingApp.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using BettingApp.Services;
using System.Net.Http;
using Moq.Protected;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BettingApp.Tests;

public class OddsCalculationTests
{
    private readonly Mock<FootballDataService> _mockFootballService;
    private readonly UserManager<User> _mockUserManager;
    private readonly ApplicationDbContext _mockDbContext;
    private readonly Mock<ILogger<FootballDataService>> _mockLogger;

    public OddsCalculationTests()
    {
        _mockLogger = new Mock<ILogger<FootballDataService>>();
            
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _mockDbContext = new ApplicationDbContext(options);
        
        _mockFootballService = new Mock<FootballDataService>(
            new HttpClient(),
            _mockLogger.Object);

        var store = new Mock<IUserStore<User>>();
        var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<User>>();
        var userValidators = new List<IUserValidator<User>>();
        var passwordValidators = new List<IPasswordValidator<User>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<User>>>();

        _mockUserManager = new UserManager<User>(
            store.Object,
            optionsAccessor.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);
    }

    [Fact]
    public void CalculateOdds_ShouldReturnBalancedOdds_WhenTeamsAreEvenlyMatched()
    {
        // Arrange
        var placeBetModel = new PlaceBetModel(
            _mockFootballService.Object,
            _mockUserManager,
            _mockDbContext);

        var homeTeam = new Team { Name = "Tottenham Hotspur FC", TeamRatingScale = 7 };
        var awayTeam = new Team { Name = "Manchester United FC", TeamRatingScale = 7 };

        // Act
        var result = placeBetModel.CalculateOdds(homeTeam, awayTeam);

        // Assert
        Assert.Equal(2.0, result.HomeWin);
        Assert.Equal(4.0, result.Draw);
        Assert.Equal(3.0, result.AwayWin);
    }

    [Fact]
    public void CalculateOdds_ShouldReturnHomeFavoriteOdds_WhenHomeTeamIsBetter()
    {
        // Arrange
        var placeBetModel = new PlaceBetModel(
            _mockFootballService.Object,
            _mockUserManager,
            _mockDbContext);

        var homeTeam = new Team { Name = "Manchester City FC", TeamRatingScale = 10 };
        var awayTeam = new Team { Name = "Aston Villa FC", TeamRatingScale = 6 };

        // Act
        var result = placeBetModel.CalculateOdds(homeTeam, awayTeam);

        // Assert
        Assert.Equal(2.0, result.HomeWin);
        Assert.Equal(3.0, result.Draw);
        Assert.Equal(5.0, result.AwayWin);
    }

    [Fact]
    public void CalculateOdds_ShouldReturnAwayFavoriteOdds_WhenAwayTeamIsBetter()
    {
        // Arrange
        var placeBetModel = new PlaceBetModel(
            _mockFootballService.Object,
            _mockUserManager,
            _mockDbContext);

        var homeTeam = new Team { Name = "Chelsea", TeamRatingScale = 7 };
        var awayTeam = new Team { Name = "Liverpool FC", TeamRatingScale = 10 };

        // Act
        var result = placeBetModel.CalculateOdds(homeTeam, awayTeam);

        // Assert
        Assert.Equal(5.0, result.HomeWin);
        Assert.Equal(4.0, result.Draw);
        Assert.Equal(3.0, result.AwayWin);
    }
}

public class FootballDataServiceTests
{
    private readonly Mock<ILogger<FootballDataService>> _loggerMock;

    public FootballDataServiceTests()
    {
        _loggerMock = new Mock<ILogger<FootballDataService>>();
    }

    [Fact]
    public async Task GetTeamsAsync_ShouldAssignTeamRatings()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var fakeTeamsResponse = new
        {
            teams = new[]
            {
                new { id = 1, name = "Real Madrid CF" },
                new { id = 2, name = "Liverpool FC" }
            }
        };

        var jsonResponse = JsonConvert.SerializeObject(fakeTeamsResponse);

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new FootballDataService(httpClient, _loggerMock.Object);

        // Act
        var teams = await service.GetTeamsAsync();
        var teamsList = teams
            .GroupBy(team => team.Name)
            .Select(group => group.First())
            .ToList();

        // Assert
        Assert.NotEmpty(teamsList);
        Assert.True(teamsList.All(team => team.TeamRatingScale > 0));
    }
}
