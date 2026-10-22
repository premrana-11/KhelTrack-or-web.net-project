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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalTournaments = await _context.Tournaments.CountAsync();
            var activeTournaments = await _context.Tournaments.CountAsync(t => t.Status == TournamentStatus.Active);
            var totalTeams = await _context.Teams.CountAsync();
            var pendingTeamsCount = await _context.Teams.CountAsync(t => t.RegistrationStatus == TeamStatus.Pending);
            var totalMatches = await _context.MatchFixtures.CountAsync();
            var liveMatchesCount = await _context.MatchFixtures.CountAsync(m => m.MatchStatus == MatchStatus.Ongoing);
            var totalAthletes = await _context.Players.CountAsync();

            var pendingApprovals = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .Include(t => t.Players)
                .Where(t => t.RegistrationStatus == TeamStatus.Pending)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentAuditLogs = await _context.AuditLogs
                .OrderByDescending(a => a.TimestampUtc)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalTournaments = totalTournaments;
            ViewBag.ActiveTournaments = activeTournaments;
            ViewBag.TotalTeams = totalTeams;
            ViewBag.PendingTeamsCount = pendingTeamsCount;
            ViewBag.TotalMatches = totalMatches;
            ViewBag.LiveMatchesCount = liveMatchesCount;
            ViewBag.TotalAthletes = totalAthletes;
            ViewBag.PendingApprovals = pendingApprovals;
            ViewBag.RecentAuditLogs = recentAuditLogs;

            return View();
        }

        // GET: /Admin/PendingApprovals
        public async Task<IActionResult> PendingApprovals()
        {
            var pendingTeams = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .Include(t => t.Players)
                .Where(t => t.RegistrationStatus == TeamStatus.Pending)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(pendingTeams);
        }

        // POST: /Admin/ApproveTeam/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTeam(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            team.RegistrationStatus = TeamStatus.Approved;
            team.RejectionReason = null;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "STATUS_APPROVED",
                entityType: "Team",
                entityId: team.Id.ToString(),
                details: $"Team '{team.TeamName}' (Captain: {team.Captain?.FullName}) approved for tournament '{team.Tournament?.Name}'."
            );

            TempData["Success"] = $"Team '{team.TeamName}' has been approved!";
            return RedirectToAction(nameof(PendingApprovals));
        }

        // POST: /Admin/RejectTeam/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTeam(int id, string? reason)
        {
            var team = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            team.RegistrationStatus = TeamStatus.Rejected;
            team.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Application does not meet eligibility requirements." : reason.Trim();
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "STATUS_REJECTED",
                entityType: "Team",
                entityId: team.Id.ToString(),
                details: $"Team '{team.TeamName}' (Captain: {team.Captain?.FullName}) was rejected. Reason: '{team.RejectionReason}'"
            );

            TempData["Warning"] = $"Team '{team.TeamName}' has been rejected.";
            return RedirectToAction(nameof(PendingApprovals));
        }

        // GET: /Admin/AuditLogs
        public async Task<IActionResult> AuditLogs(string? actionFilter, string? entityTypeFilter, string? searchTerm, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(actionFilter))
            {
                query = query.Where(a => a.Action == actionFilter);
            }

            if (!string.IsNullOrWhiteSpace(entityTypeFilter))
            {
                query = query.Where(a => a.EntityType == entityTypeFilter);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim().ToLower();
                query = query.Where(a => (a.UserEmail != null && a.UserEmail.ToLower().Contains(term)) ||
                                         a.Details.ToLower().Contains(term) ||
                                         a.EntityId.Contains(term) ||
                                         (a.IpAddress != null && a.IpAddress.Contains(term)));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.TimestampUtc >= fromDate.Value.ToUniversalTime());
            }

            if (toDate.HasValue)
            {
                // Include entire day
                var toUtc = toDate.Value.Date.AddDays(1).ToUniversalTime();
                query = query.Where(a => a.TimestampUtc < toUtc);
            }

            var logs = await query
                .OrderByDescending(a => a.TimestampUtc)
                .Take(200) // latest 200 logs
                .ToListAsync();

            var availableActions = await _context.AuditLogs
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            var availableEntities = await _context.AuditLogs
                .Select(a => a.EntityType)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            var viewModel = new AuditLogFilterViewModel
            {
                ActionFilter = actionFilter,
                EntityTypeFilter = entityTypeFilter,
                SearchTerm = searchTerm,
                FromDate = fromDate,
                ToDate = toDate,
                Logs = logs,
                AvailableActions = availableActions,
                AvailableEntityTypes = availableEntities
            };

            return View(viewModel);
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .Include(u => u.CaptainedTeams)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userRoleMap = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoleMap[user.Id] = roles;
            }

            ViewBag.UserRoles = userRoleMap;
            return View(users);
        }
    }
}
