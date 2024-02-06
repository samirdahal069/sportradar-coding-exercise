using Models;

namespace Services
{
    public interface IScoreboardService
    {
        Task StartNewMatch(Match match);

        Task UpdateScore(Match match);

        Task<Match> GetMatchById(Guid matchId);

        Task FinishMatch(Guid matchId);

        Task<IEnumerable<Match>> GetMatchesInProgressSummary();

        Task<bool> HasActiveMatch(Match match);

    }
}
