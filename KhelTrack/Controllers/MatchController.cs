using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KhelTrack.Data;
using KhelTrack.Models;
using KhelTrack.Models.ViewModels;
using KhelTrack.Services;

namespace KhelTrack.Controllers
{
    public class MatchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public MatchController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: /Match/
        public async Task<IActionResult> Index(int? tournamentId, MatchStatus? status)
        {
            var query = _context.MatchFixtures
                .Include(m => m.Tournament)
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.WinnerTeam)
                .AsQueryable();

            if (tournamentId.HasValue && tournamentId.Value > 0)
            {
                query = query.Where(m => m.TournamentId == tournamentId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.MatchStatus == status.Value);
            }

            var fixtures = await query.OrderBy(m => m.MatchDate).ToListAsync();

            ViewBag.Tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            ViewBag.SelectedTournamentId = tournamentId;
            ViewBag.SelectedStatus = status;

            return View(fixtures);
        }

        // GET: /Match/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var match = await _context.MatchFixtures
                .Include(m => m.Tournament)
                .Include(m => m.TeamA)
                    .ThenInclude(t => t!.Players)
                .Include(m => m.TeamA)
                    .ThenInclude(t => t!.Captain)
                .Include(m => m.TeamB)
                    .ThenInclude(t => t!.Players)
                .Include(m => m.TeamB)
                    .ThenInclude(t => t!.Captain)
                .Include(m => m.WinnerTeam)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // GET: /Match/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? tournamentId)
        {
            var tournaments = await _context.Tournaments
                .Where(t => t.Status != TournamentStatus.Cancelled)
                .OrderBy(t => t.Name)
                .ToListAsync();

            if (!tournaments.Any())
            {
                TempData["Warning"] = "Please create a tournament first before scheduling fixtures.";
                return RedirectToAction("Create", "Tournament");
            }

            int selectedTourneyId = tournamentId ?? tournaments.First().Id;
            var teams = await _context.Teams
                .Where(t => t.TournamentId == selectedTourneyId && t.RegistrationStatus == TeamStatus.Approved)
                .OrderBy(t => t.TeamName)
                .ToListAsync();

            var model = new CreateMatchFixtureViewModel
            {
                TournamentId = selectedTourneyId,
                MatchDate = DateTime.Now.AddDays(1).Date.AddHours(14), // default 2 PM tomorrow
                TournamentsList = new SelectList(tournaments, "Id", "Name", selectedTourneyId),
                TeamsList = new SelectList(teams, "Id", "TeamName")
            };

            return View(model);
        }

        // POST: /Match/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMatchFixtureViewModel model)
        {
            if (model.TeamAId == model.TeamBId)
            {
                ModelState.AddModelError("TeamBId", "Team A and Team B cannot be the same team.");
            }

            if (!ModelState.IsValid)
            {
                var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
                var teams = await _context.Teams
                    .Where(t => t.TournamentId == model.TournamentId && t.RegistrationStatus == TeamStatus.Approved)
                    .OrderBy(t => t.TeamName)
                    .ToListAsync();

                model.TournamentsList = new SelectList(tournaments, "Id", "Name", model.TournamentId);
                model.TeamsList = new SelectList(teams, "Id", "TeamName");
                return View(model);
            }

            var fixture = new MatchFixture
            {
                TournamentId = model.TournamentId,
                RoundName = model.RoundName.Trim(),
                TeamAId = model.TeamAId,
                TeamBId = model.TeamBId,
                MatchDate = model.MatchDate,
                Ground = model.Ground.Trim(),
                MatchStatus = MatchStatus.Scheduled,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MatchFixtures.Add(fixture);
            await _context.SaveChangesAsync();

            var teamA = await _context.Teams.FindAsync(model.TeamAId);
            var teamB = await _context.Teams.FindAsync(model.TeamBId);
            var tourney = await _context.Tournaments.FindAsync(model.TournamentId);

            await _auditService.LogAsync(
                action: "FIXTURE_SCHEDULED",
                entityType: "MatchFixture",
                entityId: fixture.Id.ToString(),
                details: $"Fixture scheduled: {teamA?.TeamName} vs {teamB?.TeamName} ({tourney?.Name}) on {fixture.MatchDate:g} at {fixture.Ground}"
            );

            TempData["Success"] = $"Match fixture '{teamA?.TeamName} vs {teamB?.TeamName}' scheduled successfully!";
            return RedirectToAction(nameof(Index), new { tournamentId = model.TournamentId });
        }

        // GET: /Match/UpdateScore/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateScore(int id)
        {
            var match = await _context.MatchFixtures
                .Include(m => m.Tournament)
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            var winnerOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Draw / No Winner Yet --" },
                new SelectListItem { Value = match.TeamAId.ToString(), Text = $"{match.TeamA?.TeamName} (Team A)" },
                new SelectListItem { Value = match.TeamBId.ToString(), Text = $"{match.TeamB?.TeamName} (Team B)" }
            };

            var model = new MatchScoreUpdateViewModel
            {
                Id = match.Id,
                TournamentId = match.TournamentId,
                TournamentName = match.Tournament?.Name ?? "Tournament",
                SportType = match.Tournament?.SportType ?? "Sport",
                RoundName = match.RoundName,
                TeamAId = match.TeamAId,
                TeamAName = match.TeamA?.TeamName ?? "Team A",
                TeamBId = match.TeamBId,
                TeamBName = match.TeamB?.TeamName ?? "Team B",
                MatchDate = match.MatchDate,
                Ground = match.Ground,
                TeamAScore = match.TeamAScore,
                TeamBScore = match.TeamBScore,
                MatchStatus = match.MatchStatus,
                WinnerTeamId = match.WinnerTeamId,
                HighlightsSummary = match.HighlightsSummary,
                TeamsForWinnerList = winnerOptions
            };

            return View(model);
        }

        // POST: /Match/UpdateScore/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateScore(MatchScoreUpdateViewModel model)
        {
            var match = await _context.MatchFixtures
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            if (match == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var winnerOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- Draw / No Winner Yet --" },
                    new SelectListItem { Value = match.TeamAId.ToString(), Text = $"{match.TeamA?.TeamName} (Team A)" },
                    new SelectListItem { Value = match.TeamBId.ToString(), Text = $"{match.TeamB?.TeamName} (Team B)" }
                };
                model.TeamsForWinnerList = winnerOptions;
                return View(model);
            }

            string oldScores = $"{match.TeamA?.TeamName}: {match.TeamAScore ?? "N/A"} vs {match.TeamB?.TeamName}: {match.TeamBScore ?? "N/A"} (Status: {match.MatchStatus})";

            match.TeamAScore = model.TeamAScore?.Trim();
            match.TeamBScore = model.TeamBScore?.Trim();
            match.MatchStatus = model.MatchStatus;
            match.WinnerTeamId = model.WinnerTeamId > 0 ? model.WinnerTeamId : null;
            match.HighlightsSummary = model.HighlightsSummary?.Trim();
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            string newScores = $"{match.TeamA?.TeamName}: {match.TeamAScore ?? "N/A"} vs {match.TeamB?.TeamName}: {match.TeamBScore ?? "N/A"} (Status: {match.MatchStatus}, WinnerId: {match.WinnerTeamId})";

            await _auditService.LogAsync(
                action: "SCORE_UPDATED",
                entityType: "MatchFixture",
                entityId: match.Id.ToString(),
                details: $"Score updated for match #{match.Id} ({match.TeamA?.TeamName} vs {match.TeamB?.TeamName}). Previous: [{oldScores}] -> Updated: [{newScores}]. Highlights: {match.HighlightsSummary}"
            );

            TempData["Success"] = "Match scores and status updated successfully!";
            return RedirectToAction(nameof(Details), new { id = match.Id });
        }

        // POST: /Match/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var match = await _context.MatchFixtures
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            int tourneyId = match.TournamentId;
            string matchTitle = $"{match.TeamA?.TeamName} vs {match.TeamB?.TeamName}";

            _context.MatchFixtures.Remove(match);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "FIXTURE_DELETED",
                entityType: "MatchFixture",
                entityId: id.ToString(),
                details: $"Admin removed match fixture: '{matchTitle}'"
            );

            TempData["Success"] = $"Match '{matchTitle}' was removed.";
            return RedirectToAction(nameof(Index), new { tournamentId = tourneyId });
        }

        // AJAX helper for dynamic teams dropdown by tournament
        [HttpGet]
        public async Task<IActionResult> GetTeamsByTournament(int tournamentId)
        {
            var teams = await _context.Teams
                .Where(t => t.TournamentId == tournamentId && t.RegistrationStatus == TeamStatus.Approved)
                .OrderBy(t => t.TeamName)
                .Select(t => new { id = t.Id, name = t.TeamName, dept = t.Department })
                .ToListAsync();

            return Json(teams);
        }
    }
}
