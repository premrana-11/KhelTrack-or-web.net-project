using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KhelTrack.Data;
using KhelTrack.Models;
using KhelTrack.Models.ViewModels;

namespace KhelTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalTournaments = await _context.Tournaments.CountAsync();
            var activeTournaments = await _context.Tournaments.CountAsync(t => t.Status == TournamentStatus.Active);
            var totalTeams = await _context.Teams.CountAsync();
            var approvedTeams = await _context.Teams.CountAsync(t => t.RegistrationStatus == TeamStatus.Approved);
            var pendingTeams = await _context.Teams.CountAsync(t => t.RegistrationStatus == TeamStatus.Pending);
            var totalMatches = await _context.MatchFixtures.CountAsync();
            var liveMatchesCount = await _context.MatchFixtures.CountAsync(m => m.MatchStatus == MatchStatus.Ongoing);
            var completedMatchesCount = await _context.MatchFixtures.CountAsync(m => m.MatchStatus == MatchStatus.Completed);
            var totalAthletes = await _context.Players.CountAsync();

            var liveMatches = await _context.MatchFixtures
                .Include(m => m.Tournament)
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Where(m => m.MatchStatus == MatchStatus.Ongoing)
                .OrderBy(m => m.MatchDate)
                .Take(4)
                .ToListAsync();

            var upcomingFixtures = await _context.MatchFixtures
                .Include(m => m.Tournament)
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Where(m => m.MatchStatus == MatchStatus.Scheduled)
                .OrderBy(m => m.MatchDate)
                .Take(6)
                .ToListAsync();

            var featuredTournaments = await _context.Tournaments
                .Include(t => t.Teams)
                .OrderByDescending(t => t.Status == TournamentStatus.Active)
                .ThenBy(t => t.StartDate)
                .Take(3)
                .ToListAsync();

            var recentActivities = await _context.AuditLogs
                .OrderByDescending(a => a.TimestampUtc)
                .Take(5)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalTournaments = totalTournaments,
                ActiveTournaments = activeTournaments,
                TotalTeams = totalTeams,
                ApprovedTeams = approvedTeams,
                PendingTeams = pendingTeams,
                TotalMatches = totalMatches,
                LiveMatchesCount = liveMatchesCount,
                CompletedMatchesCount = completedMatchesCount,
                TotalAthletes = totalAthletes,
                LiveMatches = liveMatches,
                UpcomingFixtures = upcomingFixtures,
                FeaturedTournaments = featuredTournaments,
                RecentActivities = recentActivities
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Schedule(int? tournamentId, string? sport, MatchStatus? status)
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

            if (!string.IsNullOrEmpty(sport))
            {
                query = query.Where(m => m.Tournament != null && m.Tournament.SportType.ToLower() == sport.ToLower());
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.MatchStatus == status.Value);
            }

            var fixtures = await query.OrderBy(m => m.MatchDate).ToListAsync();

            ViewBag.Tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            ViewBag.SelectedTournamentId = tournamentId;
            ViewBag.SelectedSport = sport;
            ViewBag.SelectedStatus = status;

            return View(fixtures);
        }

        public async Task<IActionResult> Leaderboard(int? tournamentId)
        {
            var tournaments = await _context.Tournaments.OrderBy(t => t.Name).ToListAsync();
            ViewBag.Tournaments = tournaments;

            int selectedId = tournamentId ?? (tournaments.FirstOrDefault(t => t.Status == TournamentStatus.Active)?.Id ?? tournaments.FirstOrDefault()?.Id ?? 0);
            ViewBag.SelectedTournamentId = selectedId;

            var leaderboard = new List<TournamentLeaderboardItem>();
            var selectedTournament = tournaments.FirstOrDefault(t => t.Id == selectedId);
            ViewBag.SelectedTournament = selectedTournament;

            if (selectedId > 0)
            {
                var approvedTeams = await _context.Teams
                    .Where(t => t.TournamentId == selectedId && t.RegistrationStatus == TeamStatus.Approved)
                    .ToListAsync();

                var completedMatches = await _context.MatchFixtures
                    .Where(m => m.TournamentId == selectedId && m.MatchStatus == MatchStatus.Completed)
                    .ToListAsync();

                foreach (var team in approvedTeams)
                {
                    int played = 0;
                    int won = 0;
                    int lost = 0;
                    int drawn = 0;
                    var formList = new List<string>();

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
                            formList.Add("W");
                        }
                        else if (m.WinnerTeamId.HasValue && m.WinnerTeamId != team.Id)
                        {
                            lost++;
                            formList.Add("L");
                        }
                        else
                        {
                            drawn++;
                            formList.Add("D");
                        }
                    }

                    int points = (won * 2) + (drawn * 1);
                    string formString = formList.Count > 0 ? string.Join("-", formList.TakeLast(5)) : "-";

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
                        Form = formString
                    });
                }

                leaderboard = leaderboard.OrderByDescending(l => l.Points)
                                         .ThenByDescending(l => l.Won)
                                         .ThenBy(l => l.Lost)
                                         .ToList();
            }

            return View(leaderboard);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
