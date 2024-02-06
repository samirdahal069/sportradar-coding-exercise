using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using SportradarCodingExercise.Models;
using System.Diagnostics;

namespace SportradarCodingExercise.Controllers
{
    public class ScoreboardController : Controller
    {
        private readonly ILogger<ScoreboardController> _logger;
        private readonly IScoreboardService _scoreboardService;  

        public ScoreboardController(ILogger<ScoreboardController> logger, IScoreboardService scoreboardService)
        {
            _logger = logger;
            _scoreboardService = scoreboardService; 
        }

        public async Task<IActionResult> Index()
        {
           return View(await _scoreboardService.GetMatchesInProgressSummary());
        }

        [HttpGet]
        public IActionResult NewMatch()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewMatch(Match match)
        {

            if (!ModelState.IsValid)
                return View(match);

            if (match.HomeTeam.Equals(match.AwayTeam, StringComparison.CurrentCultureIgnoreCase))
            {
                // The home team and away team names are the same, handle accordingly
                ModelState.AddModelError("Same Name","Away team name must be different from the home team name.");
                return View(match);
            }

            if (await _scoreboardService.HasActiveMatch(match))
            {
                ModelState.AddModelError("ActiveMatch", "Cannot add a new match while an active match is running.");
                return View(match);
            }

            if (ModelState.IsValid)
            {
                // Continue with creating and saving the new match to the database
                await _scoreboardService.StartNewMatch(match);
                return RedirectToAction("Index");
            }
            return View(match);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateScore(Guid matchId)
        {
            var matchToUpdate = await _scoreboardService.GetMatchById(matchId);

            if (matchToUpdate == null)
            {
                return NotFound();
            }

            return View(matchToUpdate);
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateScore(Match match)
        {
            if (!ModelState.IsValid)
                return View(match);

            if (ModelState.IsValid)
            {
                await _scoreboardService.UpdateScore(match);
                return RedirectToAction("Index");
            }
            return View(match);
        }

        [HttpGet]
        public async Task<IActionResult> FinishMatch(Guid matchId)
        {
            await _scoreboardService.FinishMatch(matchId);
            return RedirectToAction("Index");
        }
    }
}
