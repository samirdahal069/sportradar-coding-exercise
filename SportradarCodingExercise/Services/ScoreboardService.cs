using Microsoft.EntityFrameworkCore;
using Models;

namespace Services
{
    public class ScoreboardService : IScoreboardService
    {
        private readonly ApplictionDBContext _dbContext;
        private readonly ILogger<ScoreboardService> _logger;

        public ScoreboardService(ILogger<ScoreboardService> logger, ApplictionDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task StartNewMatch(Match match)
        {
            try
            {
                await _dbContext.Matches.AddAsync(match);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in StartNewMatch for matchId: {match.MatchId}");
                throw;
            }

        }


#pragma warning disable CS8603 // Possible null reference return.
        public async Task<Match> GetMatchById(Guid matchId) => await _dbContext.Matches.FindAsync(matchId);
#pragma warning restore CS8603 // Possible null reference return.


        public async Task UpdateScore(Match match)
        {

            try
            {

                //get match by Id
                var matchToUpdate = await GetMatchById(match.MatchId);

                if (matchToUpdate == null)
                    return;

                if (matchToUpdate != null && matchToUpdate.EndTime == null)
                {
                    matchToUpdate.HomeTeamScore = match.HomeTeamScore;
                    matchToUpdate.AwayTeamScore = match.AwayTeamScore;
                    await _dbContext.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdateScore for matchId: {match.MatchId}");
                throw;
            }
        }

        public async Task FinishMatch(Guid matchId)
        {
            try
            {
                var matchToFinish = await GetMatchById(matchId);

                if (matchToFinish == null)
                    return;

                if (matchToFinish != null && matchToFinish.EndTime == null)
                {
                    matchToFinish.EndTime = DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in FinishMatch for matchId: {matchId}");
                throw;
            }

        }

        public async Task<IEnumerable<Match>> GetMatchesInProgressSummary()
        {

            try
            {
                return await _dbContext.Matches
                   .Where(m => m.EndTime == null)
                   .OrderByDescending(m => m.HomeTeamScore + m.AwayTeamScore)
                   .ThenByDescending(m => m.StartTime)
                   .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetMatchesInProgressSummary");
                throw;
            }
        }

        public async Task<bool> HasActiveMatch(Match match)
        {

            // Check if any matches in the list are currently in progress
            var isAnyMatchWithHomeTeamOrAwayTeam = await _dbContext.Matches
                 .AnyAsync(x =>
                     (x.HomeTeam.Equals(match.HomeTeam, StringComparison.CurrentCultureIgnoreCase) ||
                      x.AwayTeam.Equals(match.HomeTeam, StringComparison.CurrentCultureIgnoreCase) ||
                      x.HomeTeam.Equals(match.AwayTeam, StringComparison.CurrentCultureIgnoreCase) ||
                      x.AwayTeam.Equals(match.AwayTeam, StringComparison.CurrentCultureIgnoreCase)) &&
                     !x.EndTime.HasValue);


            return isAnyMatchWithHomeTeamOrAwayTeam;
        }
    }
}
