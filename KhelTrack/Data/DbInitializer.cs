using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KhelTrack.Models;

namespace KhelTrack.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            // Ensure database schema is created
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roles = { "Admin", "Captain", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Seeded role: {Role}", role);
                }
            }

            // 2. Seed Users
            var adminEmail = "admin@kheltrack.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Prof. Rajesh Sharma (Sports Director)",
                    CollegeRollNo = "EMP-PE-001",
                    Department = "Physical Education & Athletics",
                    EmailConfirmed = true,
                    RegisteredAt = DateTime.UtcNow.AddMonths(-6)
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Seeded Admin user: {Email}", adminEmail);
                }
            }

            var captain1Email = "captain1@kheltrack.com";
            var captain1 = await userManager.FindByEmailAsync(captain1Email);
            if (captain1 == null)
            {
                captain1 = new ApplicationUser
                {
                    UserName = captain1Email,
                    Email = captain1Email,
                    FullName = "Aarav Patel",
                    CollegeRollNo = "CS2023-042",
                    Department = "Computer Science & Engineering",
                    EmailConfirmed = true,
                    RegisteredAt = DateTime.UtcNow.AddMonths(-2)
                };
                var result = await userManager.CreateAsync(captain1, "Captain@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(captain1, "Captain");
                }
            }

            var captain2Email = "captain2@kheltrack.com";
            var captain2 = await userManager.FindByEmailAsync(captain2Email);
            if (captain2 == null)
            {
                captain2 = new ApplicationUser
                {
                    UserName = captain2Email,
                    Email = captain2Email,
                    FullName = "Rohan Verma",
                    CollegeRollNo = "ME2023-018",
                    Department = "Mechanical Engineering",
                    EmailConfirmed = true,
                    RegisteredAt = DateTime.UtcNow.AddMonths(-2)
                };
                var result = await userManager.CreateAsync(captain2, "Captain@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(captain2, "Captain");
                }
            }

            var captain3Email = "captain3@kheltrack.com";
            var captain3 = await userManager.FindByEmailAsync(captain3Email);
            if (captain3 == null)
            {
                captain3 = new ApplicationUser
                {
                    UserName = captain3Email,
                    Email = captain3Email,
                    FullName = "Neha Kulkarni",
                    CollegeRollNo = "EC2023-089",
                    Department = "Electronics & Communication",
                    EmailConfirmed = true,
                    RegisteredAt = DateTime.UtcNow.AddMonths(-1)
                };
                var result = await userManager.CreateAsync(captain3, "Captain@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(captain3, "Captain");
                }
            }

            var student1Email = "student1@kheltrack.com";
            var student1 = await userManager.FindByEmailAsync(student1Email);
            if (student1 == null)
            {
                student1 = new ApplicationUser
                {
                    UserName = student1Email,
                    Email = student1Email,
                    FullName = "Vikas Mehra",
                    CollegeRollNo = "CV2023-104",
                    Department = "Civil Engineering",
                    EmailConfirmed = true,
                    RegisteredAt = DateTime.UtcNow.AddMonths(-1)
                };
                var result = await userManager.CreateAsync(student1, "Student@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student1, "Captain");
                }
            }

            // 3. Seed Tournaments
            if (!context.Tournaments.Any())
            {
                var cricketTourney = new Tournament
                {
                    Name = "Inter-Department Cricket Premier League (CPL 2026)",
                    SportType = "Cricket",
                    Description = "The premier annual campus cricket tournament featuring 16 departmental teams competing in T20 format for the Rolling Trophy.",
                    Rules = "ICC T20 standard rules apply. 20 overs per side. Minimum 11 squad players with verified student roll numbers. White leather ball and colored jerseys.",
                    StartDate = DateTime.UtcNow.AddDays(-3),
                    EndDate = DateTime.UtcNow.AddDays(7),
                    Venue = "University Central Cricket Ground",
                    MaxTeams = 16,
                    Status = TournamentStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-14)
                };

                var footballTourney = new Tournament
                {
                    Name = "Annual Campus Futsal Championship 2026",
                    SportType = "Football",
                    Description = "High-octane 5-a-side futsal tournament under the floodlights at the Campus Sports Complex.",
                    Rules = "FIFA Futsal rules. 20-minute halves. Rolling substitutions. Mandatory shin guards.",
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(5),
                    Venue = "Campus Synthetic Futsal Arena",
                    MaxTeams = 12,
                    Status = TournamentStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                };

                var badmintonTourney = new Tournament
                {
                    Name = "Campus Smash Badminton Open 2026",
                    SportType = "Badminton",
                    Description = "Men's and Women's singles and doubles tournament for collegiate athletes.",
                    Rules = "BWF 21-point rally scoring system, best of 3 games. Yonex Mavis 350 shuttles provided.",
                    StartDate = DateTime.UtcNow.AddDays(10),
                    EndDate = DateTime.UtcNow.AddDays(14),
                    Venue = "Indoor Sports Complex - Court 1 & 2",
                    MaxTeams = 8,
                    Status = TournamentStatus.Upcoming,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                };

                context.Tournaments.AddRange(cricketTourney, footballTourney, badmintonTourney);
                await context.SaveChangesAsync();

                // 4. Seed Teams
                var team1 = new Team
                {
                    TeamName = "CS Cyber Knights",
                    CaptainId = captain1.Id,
                    TournamentId = cricketTourney.Id,
                    Department = "Computer Science & Engineering",
                    ContactPhone = "+91 98765 43210",
                    RegistrationStatus = TeamStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    Players = new List<Player>
                    {
                        new Player { StudentName = "Aarav Patel", RollNumber = "CS2023-042", Department = "Computer Science", JerseyNumber = 7, Position = "Captain / All-Rounder", ContactNumber = "9876543210" },
                        new Player { StudentName = "Devansh Joshi", RollNumber = "CS2023-019", Department = "Computer Science", JerseyNumber = 18, Position = "Opening Batsman", ContactNumber = "9876543214" },
                        new Player { StudentName = "Sameer Rao", RollNumber = "CS2023-078", Department = "Computer Science", JerseyNumber = 45, Position = "Fast Bowler", ContactNumber = "9876543215" },
                        new Player { StudentName = "Ritvik Gupta", RollNumber = "CS2023-031", Department = "Computer Science", JerseyNumber = 99, Position = "Spin Bowler", ContactNumber = "9876543216" },
                        new Player { StudentName = "Pranav Nair", RollNumber = "CS2023-102", Department = "Computer Science", JerseyNumber = 12, Position = "Wicket-Keeper Batsman", ContactNumber = "9876543217" },
                        new Player { StudentName = "Aniket Sen", RollNumber = "CS2023-055", Department = "Computer Science", JerseyNumber = 33, Position = "Middle-Order Batsman", ContactNumber = "9876543218" }
                    }
                };

                var team2 = new Team
                {
                    TeamName = "Mech Dynamos",
                    CaptainId = captain2.Id,
                    TournamentId = cricketTourney.Id,
                    Department = "Mechanical Engineering",
                    ContactPhone = "+91 98765 43220",
                    RegistrationStatus = TeamStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-11),
                    Players = new List<Player>
                    {
                        new Player { StudentName = "Rohan Verma", RollNumber = "ME2023-018", Department = "Mechanical", JerseyNumber = 10, Position = "Captain / Fast Bowler", ContactNumber = "9876543220" },
                        new Player { StudentName = "Siddharth Kale", RollNumber = "ME2023-044", Department = "Mechanical", JerseyNumber = 17, Position = "Top-Order Batsman", ContactNumber = "9876543221" },
                        new Player { StudentName = "Kunal Roy", RollNumber = "ME2023-082", Department = "Mechanical", JerseyNumber = 24, Position = "All-Rounder", ContactNumber = "9876543222" },
                        new Player { StudentName = "Manpreet Singh", RollNumber = "ME2023-061", Department = "Mechanical", JerseyNumber = 88, Position = "Pace Bowler", ContactNumber = "9876543223" },
                        new Player { StudentName = "Tushar Deshmukh", RollNumber = "ME2023-093", Department = "Mechanical", JerseyNumber = 5, Position = "Wicket-Keeper", ContactNumber = "9876543224" },
                        new Player { StudentName = "Varun Bhatt", RollNumber = "ME2023-011", Department = "Mechanical", JerseyNumber = 21, Position = "Finisher", ContactNumber = "9876543225" }
                    }
                };

                var team3 = new Team
                {
                    TeamName = "ECE Thunderbolts",
                    CaptainId = captain3.Id,
                    TournamentId = footballTourney.Id,
                    Department = "Electronics & Communication",
                    ContactPhone = "+91 98765 43230",
                    RegistrationStatus = TeamStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    Players = new List<Player>
                    {
                        new Player { StudentName = "Neha Kulkarni", RollNumber = "EC2023-089", Department = "ECE", JerseyNumber = 10, Position = "Captain / Forward", ContactNumber = "9876543230" },
                        new Player { StudentName = "Riya Sharma", RollNumber = "EC2023-014", Department = "ECE", JerseyNumber = 1, Position = "Goalkeeper", ContactNumber = "9876543231" },
                        new Player { StudentName = "Tanmay Hegde", RollNumber = "EC2023-057", Department = "ECE", JerseyNumber = 4, Position = "Defender", ContactNumber = "9876543232" },
                        new Player { StudentName = "Aryan Saxena", RollNumber = "EC2023-071", Department = "ECE", JerseyNumber = 8, Position = "Midfielder", ContactNumber = "9876543233" },
                        new Player { StudentName = "Harshita Jain", RollNumber = "EC2023-022", Department = "ECE", JerseyNumber = 11, Position = "Winger", ContactNumber = "9876543234" }
                    }
                };

                var team4 = new Team
                {
                    TeamName = "Civil Titans",
                    CaptainId = student1.Id,
                    TournamentId = footballTourney.Id,
                    Department = "Civil Engineering",
                    ContactPhone = "+91 98765 43240",
                    RegistrationStatus = TeamStatus.Pending, // Pending for Admin review demo
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Players = new List<Player>
                    {
                        new Player { StudentName = "Vikas Mehra", RollNumber = "CV2023-104", Department = "Civil", JerseyNumber = 9, Position = "Captain / Striker", ContactNumber = "9876543240" },
                        new Player { StudentName = "Saurabh Mishra", RollNumber = "CV2023-049", Department = "Civil", JerseyNumber = 1, Position = "Goalkeeper", ContactNumber = "9876543241" },
                        new Player { StudentName = "Amit Jha", RollNumber = "CV2023-012", Department = "Civil", JerseyNumber = 3, Position = "Defender", ContactNumber = "9876543242" },
                        new Player { StudentName = "Nitin Soni", RollNumber = "CV2023-088", Department = "Civil", JerseyNumber = 6, Position = "Midfielder", ContactNumber = "9876543243" },
                        new Player { StudentName = "Yashwardhan Rao", RollNumber = "CV2023-035", Department = "Civil", JerseyNumber = 7, Position = "Playmaker", ContactNumber = "9876543244" }
                    }
                };

                context.Teams.AddRange(team1, team2, team3, team4);
                await context.SaveChangesAsync();

                // 5. Seed Matches
                var match1 = new MatchFixture
                {
                    TournamentId = cricketTourney.Id,
                    RoundName = "Group Stage - Match 1",
                    TeamAId = team1.Id,
                    TeamBId = team2.Id,
                    MatchDate = DateTime.UtcNow.AddDays(-1).AddHours(4),
                    Ground = "University Central Cricket Ground (Pitch 1)",
                    TeamAScore = "186/4 (20 ov)",
                    TeamBScore = "172/9 (20 ov)",
                    WinnerTeamId = team1.Id,
                    MatchStatus = MatchStatus.Completed,
                    HighlightsSummary = "CS Cyber Knights won by 14 runs! Aarav Patel smashed 68 off 39 balls; Rohan Verma took 3/28.",
                    UpdatedAt = DateTime.UtcNow.AddDays(-1).AddHours(8)
                };

                var match2 = new MatchFixture
                {
                    TournamentId = footballTourney.Id,
                    RoundName = "Group Stage - Match 2",
                    TeamAId = team3.Id,
                    TeamBId = team4.Id,
                    MatchDate = DateTime.UtcNow.AddHours(-1),
                    Ground = "Campus Synthetic Futsal Arena - Turf A",
                    TeamAScore = "2",
                    TeamBScore = "1",
                    WinnerTeamId = null,
                    MatchStatus = MatchStatus.Ongoing,
                    HighlightsSummary = "Intense 2nd half action! Neha Kulkarni scored a stunning curling strike; Civil Titans pressing hard.",
                    UpdatedAt = DateTime.UtcNow
                };

                var match3 = new MatchFixture
                {
                    TournamentId = cricketTourney.Id,
                    RoundName = "Semi-Final",
                    TeamAId = team1.Id,
                    TeamBId = team2.Id,
                    MatchDate = DateTime.UtcNow.AddDays(2).AddHours(5),
                    Ground = "University Central Cricket Ground (Main Pavilion)",
                    TeamAScore = null,
                    TeamBScore = null,
                    WinnerTeamId = null,
                    MatchStatus = MatchStatus.Scheduled,
                    HighlightsSummary = "Anticipated high-stakes rematch in the CPL 2026 Semi-Finals.",
                    UpdatedAt = DateTime.UtcNow
                };

                context.MatchFixtures.AddRange(match1, match2, match3);
                await context.SaveChangesAsync();

                // 6. Seed Initial Audit Logs
                var auditLogs = new List<AuditLog>
                {
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        Action = "TOURNAMENT_CREATED",
                        EntityType = "Tournament",
                        EntityId = cricketTourney.Id.ToString(),
                        Details = $"Created tournament '{cricketTourney.Name}' for Sport: Cricket with Max Teams: 16",
                        TimestampUtc = DateTime.UtcNow.AddDays(-14),
                        IpAddress = "192.168.1.10"
                    },
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        Action = "TOURNAMENT_CREATED",
                        EntityType = "Tournament",
                        EntityId = footballTourney.Id.ToString(),
                        Details = $"Created tournament '{footballTourney.Name}' for Sport: Football with Max Teams: 12",
                        TimestampUtc = DateTime.UtcNow.AddDays(-10),
                        IpAddress = "192.168.1.10"
                    },
                    new AuditLog
                    {
                        UserId = captain1.Id,
                        UserEmail = captain1.Email,
                        Action = "TEAM_REGISTERED",
                        EntityType = "Team",
                        EntityId = team1.Id.ToString(),
                        Details = $"Captain Aarav Patel submitted team '{team1.TeamName}' with 6 roster players for CPL 2026",
                        TimestampUtc = DateTime.UtcNow.AddDays(-12),
                        IpAddress = "192.168.1.45"
                    },
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        Action = "STATUS_APPROVED",
                        EntityType = "Team",
                        EntityId = team1.Id.ToString(),
                        Details = $"Sports Committee approved registration for team '{team1.TeamName}'. Status changed: Pending -> Approved",
                        TimestampUtc = DateTime.UtcNow.AddDays(-11),
                        IpAddress = "192.168.1.10"
                    },
                    new AuditLog
                    {
                        UserId = captain2.Id,
                        UserEmail = captain2.Email,
                        Action = "TEAM_REGISTERED",
                        EntityType = "Team",
                        EntityId = team2.Id.ToString(),
                        Details = $"Captain Rohan Verma registered team '{team2.TeamName}' with 6 roster players for CPL 2026",
                        TimestampUtc = DateTime.UtcNow.AddDays(-11),
                        IpAddress = "192.168.1.52"
                    },
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        Action = "STATUS_APPROVED",
                        EntityType = "Team",
                        EntityId = team2.Id.ToString(),
                        Details = $"Sports Committee approved registration for team '{team2.TeamName}'. Status changed: Pending -> Approved",
                        TimestampUtc = DateTime.UtcNow.AddDays(-10),
                        IpAddress = "192.168.1.10"
                    },
                    new AuditLog
                    {
                        UserId = student1.Id,
                        UserEmail = student1.Email,
                        Action = "TEAM_REGISTERED",
                        EntityType = "Team",
                        EntityId = team4.Id.ToString(),
                        Details = $"Captain Vikas Mehra submitted team '{team4.TeamName}' with 5 players for Futsal Championship 2026. Awaiting committee review.",
                        TimestampUtc = DateTime.UtcNow.AddDays(-1),
                        IpAddress = "192.168.1.66"
                    },
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        Action = "FIXTURE_SCHEDULED",
                        EntityType = "MatchFixture",
                        EntityId = match1.Id.ToString(),
                        Details = $"Scheduled match: CS Cyber Knights vs Mech Dynamos on University Central Cricket Ground",
                        TimestampUtc = DateTime.UtcNow.AddDays(-3),
                        IpAddress = "192.168.1.10"
                    },
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        Action = "SCORE_UPDATED",
                        EntityType = "MatchFixture",
                        EntityId = match1.Id.ToString(),
                        Details = $"Updated final score: CS Cyber Knights (186/4) vs Mech Dynamos (172/9). Winner: CS Cyber Knights. Match Status: Completed.",
                        TimestampUtc = DateTime.UtcNow.AddDays(-1).AddHours(8),
                        IpAddress = "192.168.1.10"
                    },
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        Action = "SCORE_UPDATED",
                        EntityType = "MatchFixture",
                        EntityId = match2.Id.ToString(),
                        Details = $"Live score updated: ECE Thunderbolts 2 - 1 Civil Titans. Match Status set to Ongoing.",
                        TimestampUtc = DateTime.UtcNow,
                        IpAddress = "192.168.1.10"
                    }
                };

                context.AuditLogs.AddRange(auditLogs);
                await context.SaveChangesAsync();
                logger.LogInformation("Database seeded successfully with initial tournaments, teams, players, fixtures, and audit logs.");
            }
        }
    }
}
