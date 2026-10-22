using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KhelTrack.Data;
using KhelTrack.Models;
using KhelTrack.Models.ViewModels;
using KhelTrack.Services;

namespace KhelTrack.Controllers
{
    public class TournamentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TournamentController(
            ApplicationDbContext context,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _auditService = auditService;
            _userManager = userManager;
        }

        // GET: /Tournament/
        public async Task<IActionResult> Index(string? sport, TournamentStatus? status, string? search)
        {
            var query = _context.Tournaments
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(sport))
            {
                query = query.Where(t => t.SportType.ToLower() == sport.ToLower());
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(term) ||
                                         (t.Description != null && t.Description.ToLower().Contains(term)) ||
                                         t.Venue.ToLower().Contains(term));
            }

            var tournaments = await query
                .OrderByDescending(t => t.Status == TournamentStatus.Active)
                .ThenBy(t => t.StartDate)
                .ToListAsync();

            ViewBag.SelectedSport = sport;
            ViewBag.SelectedStatus = status;
            ViewBag.SearchTerm = search;

            // Distinct sports for filter chips
            ViewBag.SportsList = await _context.Tournaments
                .Select(t => t.SportType)
                .Distinct()
                .ToListAsync();

            return View(tournaments);
        }

        // GET: /Tournament/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                    .ThenInclude(tm => tm.Captain)
                .Include(t => t.Teams)
                    .ThenInclude(tm => tm.Players)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.TeamA)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.TeamB)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.WinnerTeam)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            bool isUserRegistered = false;
            Team? userTeam = null;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                userTeam = tournament.Teams.FirstOrDefault(tm => tm.CaptainId == currentUserId);
                isUserRegistered = userTeam != null;
            }

            // Calculate Leaderboard for approved teams
            var approvedTeams = tournament.Teams
                .Where(t => t.RegistrationStatus == TeamStatus.Approved)
                .ToList();

            var completedMatches = tournament.Matches
                .Where(m => m.MatchStatus == MatchStatus.Completed)
                .ToList();

            var leaderboard = new List<TournamentLeaderboardItem>();
            foreach (var team in approvedTeams)
            {
                int played = 0;
                int won = 0;
                int lost = 0;
                int drawn = 0;
                var form = new List<string>();

                var teamMatches = completedMatches
                    .Where(m => m.TeamAId == team.Id || m.TeamBId == team.Id)
                    .OrderBy(m => m.MatchDate)
                    .ToList();

                foreach (var m in teamMatches)
                {
                    played++;
                    if (m.WinnerTeamId == team.Id)
                    {
                        won++;
                        form.Add("W");
                    }
                    else if (m.WinnerTeamId.HasValue && m.WinnerTeamId != team.Id)
                    {
                        lost++;
                        form.Add("L");
                    }
                    else
                    {
                        drawn++;
                        form.Add("D");
                    }
                }

                int points = (won * 2) + (drawn * 1);
                leaderboard.Add(new TournamentLeaderboardItem
                {
                    TeamId = team.Id,
                    TeamName = team.TeamName,
                    Department = team.Department,
                    MatchesPlayed = played,
                    Won = won,
                    Lost = lost,
                    Drawn = drawn,
                    Points = points,
                    Form = form.Count > 0 ? string.Join("-", form.TakeLast(5)) : "-"
                });
            }

            leaderboard = leaderboard
                .OrderByDescending(l => l.Points)
                .ThenByDescending(l => l.Won)
                .ThenBy(l => l.Lost)
                .ToList();

            var viewModel = new TournamentDetailsViewModel
            {
                Tournament = tournament,
                Teams = tournament.Teams.OrderByDescending(t => t.RegistrationStatus == TeamStatus.Approved).ThenBy(t => t.TeamName).ToList(),
                Matches = tournament.Matches.OrderBy(m => m.MatchDate).ToList(),
                Leaderboard = leaderboard,
                IsUserCaptainRegistered = isUserRegistered,
                UserTeam = userTeam
            };

            return View(viewModel);
        }

        // GET: /Tournament/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new Tournament
            {
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(14),
                MaxTeams = 16,
                Status = TournamentStatus.Upcoming
            };
            return View(model);
        }

        // POST: /Tournament/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tournament tournament)
        {
            if (tournament.EndDate < tournament.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be greater than or equal to Start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(tournament);
            }

            tournament.CreatedAt = DateTime.UtcNow;
            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "TOURNAMENT_CREATED",
                entityType: "Tournament",
                entityId: tournament.Id.ToString(),
                details: $"Admin created new tournament '{tournament.Name}' ({tournament.SportType}), Venue: {tournament.Venue}, Max Teams: {tournament.MaxTeams}"
            );

            TempData["Success"] = $"Tournament '{tournament.Name}' created successfully!";
            return RedirectToAction(nameof(Details), new { id = tournament.Id });
        }

        // GET: /Tournament/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        // POST: /Tournament/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tournament tournament)
        {
            if (id != tournament.Id)
            {
                return NotFound();
            }

            if (tournament.EndDate < tournament.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be greater than or equal to Start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(tournament);
            }

            try
            {
                _context.Update(tournament);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    action: "TOURNAMENT_UPDATED",
                    entityType: "Tournament",
                    entityId: tournament.Id.ToString(),
                    details: $"Admin updated tournament details for '{tournament.Name}', Status: {tournament.Status}"
                );

                TempData["Success"] = $"Tournament '{tournament.Name}' updated successfully!";
                return RedirectToAction(nameof(Details), new { id = tournament.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tournaments.Any(e => e.Id == tournament.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // POST: /Tournament/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            string tourneyName = tournament.Name;
            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "TOURNAMENT_DELETED",
                entityType: "Tournament",
                entityId: id.ToString(),
                details: $"Admin deleted tournament '{tourneyName}' and associated matches/registrations."
            );

            TempData["Success"] = $"Tournament '{tourneyName}' was removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
