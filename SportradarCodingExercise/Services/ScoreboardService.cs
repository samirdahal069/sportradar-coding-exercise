using Microsoft.EntityFrameworkCore;
using Models;

namespace Services
{
    public class ScoreboardService(ApplictionDBContext dbContext) : IScoreboardService
    {
        private readonly ApplictionDBContext _dbContext = dbContext;

        public async Task StartNewMatch(Match match)
        {
            await _dbContext.Matches.AddAsync(match);
            await _dbContext.SaveChangesAsync();
        }


        #pragma warning disable CS8603 // Possible null reference return.
        public async Task<Match> GetMatchById(Guid matchId) => await _dbContext.Matches.FindAsync(matchId);
        #pragma warning restore CS8603 // Possible null reference return.


        public async Task UpdateScore(Match match)
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

        public async Task FinishMatch(Guid matchId)
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

        public async Task<IEnumerable<Match>> GetMatchesInProgressSummary()
        {
            return await _dbContext.Matches
            .Where(m => m.EndTime == null)
            .OrderByDescending(m => m.HomeTeamScore + m.AwayTeamScore)
            .ThenByDescending(m => m.StartTime)
            .ToListAsync();
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
