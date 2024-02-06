using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;
using Services;
using Xunit;

public class ScoreboardServiceTests
{
    [Fact]
    public async Task StartNewMatch_AddsMatchToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplictionDBContext>()
            .UseInMemoryDatabase(databaseName: "StartNewMatch_Database")
            .Options;

        using (var dbContext = new ApplictionDBContext(options))
        {
            var service = new ScoreboardService(dbContext);
            var match = new Match { HomeTeam = "TeamA", AwayTeam = "TeamB" };

            // Act
            await service.StartNewMatch(match);

            // Assert
            var savedMatch = await dbContext.Matches.FirstOrDefaultAsync();
            Assert.NotNull(savedMatch);
            Assert.Equal("TeamA", savedMatch.HomeTeam);
            Assert.Equal("TeamB", savedMatch.AwayTeam);
        }
    }

    [Fact]
    public async Task GetMatchById_ReturnsCorrectMatch()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplictionDBContext>()
            .UseInMemoryDatabase(databaseName: "GetMatchById_Database")
            .Options;

        using (var dbContext = new ApplictionDBContext(options))
        {
            var service = new ScoreboardService(dbContext);
            var match1 = new Match { HomeTeam = "TeamA", AwayTeam = "TeamB" };

            // Add the match to the database
            await dbContext.Matches.AddAsync(match1);
            await dbContext.SaveChangesAsync();

            // Act
            var retrievedMatch = await service.GetMatchById(match1.MatchId);

            // Assert
            Assert.NotNull(retrievedMatch);
            Assert.Equal(match1.HomeTeam, retrievedMatch.HomeTeam);
            Assert.Equal(match1.AwayTeam, retrievedMatch.AwayTeam);
        }
    }

    [Fact]
    public async Task UpdateScore_UpdatesMatchScoreInDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplictionDBContext>()
            .UseInMemoryDatabase(databaseName: "UpdateScore_Database")
            .Options;

        using (var dbContext = new ApplictionDBContext(options))
        {
            var service = new ScoreboardService(dbContext);
            var match = new Match { HomeTeam = "TeamA", AwayTeam = "TeamB" };

            // Add the match to the database
            await dbContext.Matches.AddAsync(match);
            await dbContext.SaveChangesAsync();

            // Update the match score
            match.HomeTeamScore = 2;
            match.AwayTeamScore = 1;

            // Act
            await service.UpdateScore(match);

            // Assert
            var updatedMatch = await dbContext.Matches.FindAsync(match.MatchId);
            Assert.NotNull(updatedMatch);
            Assert.Equal(2, updatedMatch.HomeTeamScore);
            Assert.Equal(1, updatedMatch.AwayTeamScore);
        }
    }
    [Fact]
    public async Task FinishMatch_FinishesMatchInDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplictionDBContext>()
            .UseInMemoryDatabase(databaseName: "FinishMatch_Database")
            .Options;

        using (var dbContext = new ApplictionDBContext(options))
        {
            var service = new ScoreboardService(dbContext);
            var match = new Match { HomeTeam = "TeamA", AwayTeam = "TeamB" };

            // Add the match to the database
            await dbContext.Matches.AddAsync(match);
            await dbContext.SaveChangesAsync();

            // Act
            await service.FinishMatch(match.MatchId);

            // Assert
            var finishedMatch = await dbContext.Matches.FindAsync(match.MatchId);
            Assert.NotNull(finishedMatch);
            Assert.NotNull(finishedMatch.EndTime);
            // Add more assertions based on the expected behavior of your service
        }
    }

    [Fact]
    public async Task GetMatchesInProgressSummary_ReturnsMatchesInOrder()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplictionDBContext>()
            .UseInMemoryDatabase(databaseName: "GetMatchesInProgressSummary_Database")
            .Options;

        using (var dbContext = new ApplictionDBContext(options))
        {

            // Get all matches from the database
            var allMatches = dbContext.Matches.ToList();
            // Remove all matches from the DbSet
            dbContext.Matches.RemoveRange(allMatches);
            // Save changes to the database
            dbContext.SaveChanges();

            var service = new ScoreboardService(dbContext);
            var match1 = new Match { HomeTeam = "TeamA", AwayTeam = "TeamB", StartTime = DateTime.Now.AddMinutes(-5) };
            var match2 = new Match { HomeTeam = "TeamC", AwayTeam = "TeamD", StartTime = DateTime.Now.AddMinutes(-3) };

            // Add matches to the database
            await dbContext.Matches.AddRangeAsync(match1, match2);
            await dbContext.SaveChangesAsync();

            // Act
            var matchesInProgress = await service.GetMatchesInProgressSummary();

            // Assert
            Assert.NotNull(matchesInProgress);
            Assert.Equal(2, matchesInProgress.Count());

            // Check if matches are ordered by total score and then by start time
            Assert.Equal(match2.MatchId, matchesInProgress.First().MatchId); // Match2 has a higher total score
            Assert.Equal(match1.MatchId, matchesInProgress.Last().MatchId);
        }
    }
}