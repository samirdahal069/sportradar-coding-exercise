using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using SportradarCodingExercise.Controllers;
using Xunit;

public class ScoreboardControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithMatchesInProgressSummary()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ScoreboardController>>();
        var scoreboardServiceMock = new Mock<IScoreboardService>();
        scoreboardServiceMock.Setup(service => service.GetMatchesInProgressSummary())
            .ReturnsAsync(new List<Models.Match>());

        var controller = new ScoreboardController(loggerMock.Object, scoreboardServiceMock.Object);

        // Act
        var result = await controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ViewName ?? "Index");

    }
    [Fact]
    public async Task NewMatch_WhenHomeAndAwayTeamsAreDifferent_ReturnsRedirectToActionResult()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ScoreboardController>>();
        var scoreboardServiceMock = new Mock<IScoreboardService>();
        scoreboardServiceMock.Setup(service => service.HasActiveMatch(It.IsAny<Models.Match>()))
            .ReturnsAsync(false); // Simulate no active match

        var controller = new ScoreboardController(loggerMock.Object, scoreboardServiceMock.Object);
        var match = new Models.Match { HomeTeam = "TeamA", AwayTeam = "TeamB" };

        // Act
        var result = await controller.NewMatch(match) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName); // Check if it redirects to the "Index" action
    }

    [Fact]
    public async Task NewMatch_WhenHomeAndAwayTeamsAreSame_ReturnsViewWithModelError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ScoreboardController>>();
        var scoreboardServiceMock = new Mock<IScoreboardService>();
        scoreboardServiceMock.Setup(service => service.HasActiveMatch(It.IsAny<Models.Match>()))
            .ReturnsAsync(false); // Simulate no active match

        var controller = new ScoreboardController(loggerMock.Object, scoreboardServiceMock.Object);
        var match = new Models.Match { HomeTeam = "TeamA", AwayTeam = "TeamA" };

        // Act
        var result = await controller.NewMatch(match) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Single(controller.ModelState["Same Name"].Errors);
        Assert.Equal("Away team name must be different from the home team name.", controller.ModelState["Same Name"].Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NewMatch_WhenActiveMatchExists_ReturnsViewWithModelError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ScoreboardController>>();
        var scoreboardServiceMock = new Mock<IScoreboardService>();
        scoreboardServiceMock.Setup(service => service.HasActiveMatch(It.IsAny<Models.Match>()))
            .ReturnsAsync(true); // Simulate an active match

        var controller = new ScoreboardController(loggerMock.Object, scoreboardServiceMock.Object);
        var match = new Models.Match { HomeTeam = "TeamA", AwayTeam = "TeamB" };

        // Act
        var result = await controller.NewMatch(match) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Single(controller.ModelState["ActiveMatch"].Errors);
        Assert.Equal("Cannot add a new match while an active match is running.", controller.ModelState["ActiveMatch"].Errors[0].ErrorMessage);
    }


    [Fact]
    public async Task UpdateScore_WhenValidModel_ReturnsRedirectToActionResult()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ScoreboardController>>();
        var scoreboardServiceMock = new Mock<IScoreboardService>();
        var controller = new ScoreboardController(loggerMock.Object, scoreboardServiceMock.Object);
        var match = new Models.Match { MatchId = Guid.NewGuid(), HomeTeam = "TeamA", AwayTeam = "TeamB", HomeTeamScore = 1, AwayTeamScore = 0 };

        // Act
        var result = await controller.UpdateScore(match) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public async Task FinishMatch_WhenValidMatchId_ReturnsRedirectToActionResult()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ScoreboardController>>();
        var scoreboardServiceMock = new Mock<IScoreboardService>();
        var controller = new ScoreboardController(loggerMock.Object, scoreboardServiceMock.Object);
        var matchId = Guid.NewGuid();

        // Act
        var result = await controller.FinishMatch(matchId) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }
}