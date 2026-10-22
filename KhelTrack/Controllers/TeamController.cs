using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KhelTrack.Data;
using KhelTrack.Models;
using KhelTrack.Models.ViewModels;
using KhelTrack.Services;

namespace KhelTrack.Controllers
{
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public TeamController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET: /Team/
        public async Task<IActionResult> Index(int? tournamentId, TeamStatus? status, string? search)
        {
            var query = _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .Include(t => t.Players)
                .AsQueryable();

            if (tournamentId.HasValue && tournamentId.Value > 0)
            {
                query = query.Where(t => t.TournamentId == tournamentId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.RegistrationStatus == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(t => t.TeamName.ToLower().Contains(term) ||
                                         t.Department.ToLower().Contains(term) ||
                                         (t.Captain != null && t.Captain.FullName.ToLower().Contains(term)));
            }

            var teams = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            ViewBag.Tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            ViewBag.SelectedTournamentId = tournamentId;
            ViewBag.SelectedStatus = status;
            ViewBag.SearchTerm = search;

            return View(teams);
        }

        // GET: /Team/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Captain)
                .Include(t => t.Players)
                .Include(t => t.HomeMatches)
                    .ThenInclude(m => m.TeamB)
                .Include(t => t.AwayMatches)
                    .ThenInclude(m => m.TeamA)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            bool isCaptainOrAdmin = User.IsInRole("Admin") || (currentUserId != null && team.CaptainId == currentUserId);
            ViewBag.CanManageRoster = isCaptainOrAdmin;

            // Combine home and away matches
            var allMatches = await _context.MatchFixtures
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.WinnerTeam)
                .Where(m => m.TeamAId == id || m.TeamBId == id)
                .OrderBy(m => m.MatchDate)
                .ToListAsync();

            ViewBag.TeamMatches = allMatches;

            return View(team);
        }

        // GET: /Team/Register
        [Authorize]
        public async Task<IActionResult> Register(int? tournamentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // Only allow registering for upcoming or active tournaments
            var availableTournaments = await _context.Tournaments
                .Where(t => t.Status == TournamentStatus.Upcoming || t.Status == TournamentStatus.Active)
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            if (!availableTournaments.Any())
            {
                TempData["Warning"] = "There are currently no active or upcoming tournaments accepting registrations.";
                return RedirectToAction("Index", "Tournament");
            }

            ViewBag.Tournaments = new SelectList(availableTournaments, "Id", "Name", tournamentId);

            var model = new TeamRegistrationViewModel
            {
                TournamentId = tournamentId ?? availableTournaments.First().Id,
                Department = currentUser.Department ?? string.Empty,
                ContactPhone = currentUser.PhoneNumber ?? string.Empty,
                Players = new List<PlayerInputModel>
                {
                    // Pre-fill captain as the first player
                    new PlayerInputModel
                    {
                        StudentName = currentUser.FullName ?? "Captain",
                        RollNumber = currentUser.CollegeRollNo ?? "ID-001",
                        Department = currentUser.Department ?? string.Empty,
                        Position = "Captain",
                        JerseyNumber = 1
                    }
                }
            };

            return View(model);
        }

        // POST: /Team/Register
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(TeamRegistrationViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == model.TournamentId);

            if (tournament == null)
            {
                ModelState.AddModelError("TournamentId", "Selected tournament does not exist.");
            }
            else
            {
                // Check if captain already registered a team in this tournament
                bool alreadyRegistered = await _context.Teams
                    .AnyAsync(t => t.TournamentId == model.TournamentId && t.CaptainId == currentUser.Id);

                if (alreadyRegistered)
                {
                    ModelState.AddModelError(string.Empty, "You have already registered a team for this tournament.");
                }

                // Check capacity
                int approvedOrPendingCount = tournament.Teams.Count(t => t.RegistrationStatus != TeamStatus.Rejected);
                if (approvedOrPendingCount >= tournament.MaxTeams)
                {
                    ModelState.AddModelError(string.Empty, $"This tournament has reached its maximum capacity of {tournament.MaxTeams} teams.");
                }
            }

            // Filter out empty player rows if any
            model.Players = model.Players?
                .Where(p => !string.IsNullOrWhiteSpace(p.StudentName) && !string.IsNullOrWhiteSpace(p.RollNumber))
                .ToList() ?? new List<PlayerInputModel>();

            if (model.Players.Count == 0)
            {
                ModelState.AddModelError("Players", "Please provide at least 1 player in the team roster.");
            }

            if (!ModelState.IsValid)
            {
                var availableTournaments = await _context.Tournaments
                    .Where(t => t.Status == TournamentStatus.Upcoming || t.Status == TournamentStatus.Active)
                    .OrderBy(t => t.StartDate)
                    .ToListAsync();

                ViewBag.Tournaments = new SelectList(availableTournaments, "Id", "Name", model.TournamentId);
                return View(model);
            }

            var team = new Team
            {
                TeamName = model.TeamName.Trim(),
                CaptainId = currentUser.Id,
                TournamentId = model.TournamentId,
                Department = model.Department.Trim(),
                ContactPhone = model.ContactPhone.Trim(),
                RegistrationStatus = TeamStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var p in model.Players)
            {
                team.Players.Add(new Player
                {
                    StudentName = p.StudentName.Trim(),
                    RollNumber = p.RollNumber.Trim(),
                    Department = string.IsNullOrWhiteSpace(p.Department) ? model.Department.Trim() : p.Department.Trim(),
                    JerseyNumber = p.JerseyNumber,
                    Position = p.Position?.Trim(),
                    ContactNumber = p.ContactNumber?.Trim()
                });
            }

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "TEAM_REGISTERED",
                entityType: "Team",
                entityId: team.Id.ToString(),
                details: $"Team '{team.TeamName}' registered by Captain {currentUser.FullName} ({currentUser.Email}) with {team.Players.Count} players for tournament '{tournament?.Name}'. Status: Pending approval."
            );

            TempData["Success"] = $"Team '{team.TeamName}' registration submitted successfully! It is now pending committee approval.";
            return RedirectToAction(nameof(MyTeams));
        }

        // GET: /Team/MyTeams
        [Authorize]
        public async Task<IActionResult> MyTeams()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var teams = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Players)
                .Where(t => t.CaptainId == currentUser.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(teams);
        }

        // GET: /Team/ManageRoster/5
        [Authorize]
        public async Task<IActionResult> ManageRoster(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            bool isAuthorized = User.IsInRole("Admin") || (currentUserId != null && team.CaptainId == currentUserId);

            if (!isAuthorized)
            {
                return Forbid();
            }

            ViewBag.AddPlayerModel = new AddPlayerViewModel
            {
                TeamId = team.Id,
                TeamName = team.TeamName,
                Department = team.Department
            };

            return View(team);
        }

        // POST: /Team/AddPlayer
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPlayer(AddPlayerViewModel model)
        {
            var team = await _context.Teams.FindAsync(model.TeamId);
            if (team == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            bool isAuthorized = User.IsInRole("Admin") || (currentUserId != null && team.CaptainId == currentUserId);

            if (!isAuthorized)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields for the new player.";
                return RedirectToAction(nameof(ManageRoster), new { id = model.TeamId });
            }

            var player = new Player
            {
                TeamId = model.TeamId,
                StudentName = model.StudentName.Trim(),
                RollNumber = model.RollNumber.Trim(),
                Department = model.Department.Trim(),
                JerseyNumber = model.JerseyNumber,
                Position = model.Position?.Trim(),
                ContactNumber = model.ContactNumber?.Trim()
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "PLAYER_ADDED",
                entityType: "Player",
                entityId: player.Id.ToString(),
                details: $"Player '{player.StudentName}' (Roll: {player.RollNumber}, #{player.JerseyNumber}) added to Team '{team.TeamName}'"
            );

            TempData["Success"] = $"Player '{player.StudentName}' added to roster successfully.";
            return RedirectToAction(nameof(ManageRoster), new { id = model.TeamId });
        }

        // POST: /Team/RemovePlayer/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePlayer(int id)
        {
            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            bool isAuthorized = User.IsInRole("Admin") || (currentUserId != null && player.Team?.CaptainId == currentUserId);

            if (!isAuthorized)
            {
                return Forbid();
            }

            int teamId = player.TeamId;
            string playerName = player.StudentName;
            string teamName = player.Team?.TeamName ?? "Team";

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "PLAYER_REMOVED",
                entityType: "Player",
                entityId: id.ToString(),
                details: $"Player '{playerName}' removed from Team '{teamName}' roster."
            );

            TempData["Success"] = $"Player '{playerName}' removed from roster.";
            return RedirectToAction(nameof(ManageRoster), new { id = teamId });
        }
    }
}
