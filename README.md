# KhelTrack 🏆
### Intra-College Sports Tournament & Roster Management System
**Built with ASP.NET Core MVC (.NET 10 / LTS Compatible), Entity Framework Core, ASP.NET Core Identity, and Bootstrap 5.**

---

## 📌 Project Overview
**KhelTrack** is a full-featured web application engineered for university athletic directorates, sports committees, team captains, and campus spectators. It centralizes:
- **Tournament Lifecycle:** Create, manage, and monitor sports categories (Cricket, Football/Futsal, Badminton, Basketball, Volleyball, etc.).
- **Team & Roster Registration:** Student captains can register departmental squads and submit player details (Name, College Roll Number, Department, Jersey #, Role/Position).
- **Sports Committee Review:** One-click approval/rejection workflow with feedback notes.
- **Fixture Scheduling & Live Scorecards:** Manage match rounds, grounds/venues, ongoing live scores, and winners.
- **Automated Points Table / Leaderboard:** Real-time computation of matches played, wins, losses, draws, points, and 5-game form guide.
- **Tamper-Evident Audit Trail:** Every critical state transition (team registration, committee review, match schedule, score entry) is immutably logged with timestamps, user credentials, action tags, and IP addresses.

---

## 🚀 How to Run in Visual Studio Community

### Option A: Open with Visual Studio Community (Recommended)
1. Launch **Visual Studio Community** (2022 or later).
2. Click **Open a project or solution**.
3. Select `KhelTrack.sln` located in the root project folder.
4. Set `KhelTrack` as the startup project.
5. Press **F5** (Debug) or **Ctrl + F5** (Run without debugging).
6. The browser will open automatically at `http://localhost:5062` (or your assigned IIS Express / Kestrel port).

### Option B: Run via .NET CLI (Terminal / PowerShell)
```powershell
cd KhelTrack
dotnet run
```
Then navigate to: **`http://localhost:5062`**

---

## 🗄️ Database Architecture & Dual-Provider Switch
KhelTrack supports both **SQLite** and **Microsoft SQL Server (LocalDB / SQL Express / SSMS)** with a single configuration flag.

### 1. Default Mode: SQLite (Active by Default)
- **Why SQLite?** Provides instant, zero-configuration evaluation. No local SQL Server service configuration is required, and the database file (`kheltrack.db`) is automatically initialized and seeded on first run.
- You can inspect `kheltrack.db` using any SQLite Viewer extension in Visual Studio, DB Browser for SQLite, or DBeaver.

### 2. Switching to SQL Server / SSMS
If you want to use Microsoft SQL Server or SQL Server Management Studio (SSMS):
1. Open `KhelTrack/appsettings.json`.
2. Change `"DatabaseProvider"` from `"Sqlite"` to `"SqlServer"`:
```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "SqlServerConnection": "Server=(localdb)\\mssqllocaldb;Database=KhelTrackDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```
3. Run the application. EF Core will automatically create `KhelTrackDb` in your local SQL Server instance and seed all default accounts, tournaments, teams, fixtures, and audit records.

---

## 🔑 Pre-Seeded Demo Accounts & Credentials
The database automatically seeds with roles and verified demo users:

| Role | Email | Password | Full Name & Department |
| :--- | :--- | :--- | :--- |
| **Admin (Sports Director)** | `admin@kheltrack.com` | `Admin@123` | Prof. Rajesh Sharma (Physical Education) |
| **Team Captain (CS)** | `captain1@kheltrack.com` | `Captain@123` | Aarav Patel (Computer Science) |
| **Team Captain (Mech)** | `captain2@kheltrack.com` | `Captain@123` | Rohan Verma (Mechanical Engineering) |
| **Team Captain (ECE)** | `captain3@kheltrack.com` | `Captain@123` | Neha Kulkarni (Electronics & Comm) |
| **Student / Captain (Civil)** | `student1@kheltrack.com` | `Student@123` | Vikas Mehra (Civil Engineering) |

> 💡 **Tip:** On the **Log In** page, convenient 1-click quick-fill buttons allow instant authentication as Admin, Captain, or Student.

---

## 🛡️ Core Workflows & Features

### 1. Admin & Sports Committee
- **Dashboard:** At `/Admin/Dashboard`, view campus metrics, live match tickers, pending approvals queue, and recent governance activities.
- **Team Approvals:** At `/Admin/PendingApprovals`, inspect submitted team rosters, student roll numbers, and choose **Approve** or **Reject** (with custom reviewer comments).
- **Fixture Scheduling:** At `/Match/Create`, schedule match fixtures by choosing the tournament, Team A, Team B, ground/venue, and round.
- **Score & Highlights Entry:** At `/Match/UpdateScore/{id}`, update live scores, declare match winners, update status (`Scheduled`, `Ongoing`, `Completed`), and write commentary notes.
- **Audit Logs Dashboard:** At `/Admin/AuditLogs`, filter audit trail records by **Action Type** (e.g. `TEAM_REGISTERED`, `SCORE_UPDATED`, `STATUS_APPROVED`), **Entity Type**, **Keyword Search**, and **Date Range**.

### 2. Team Captains & Students
- **Account Registration:** Register at `/Account/Register` with student roll number, department, and role selection.
- **Dynamic Team Roster Builder:** At `/Team/Register`, captains select a tournament, department, phone, and dynamically add athlete roster rows (`+ Add Player`) without page reloads.
- **My Teams:** At `/Team/MyTeams`, captains monitor application statuses (`Pending`, `Approved`, `Rejected`), read reviewer feedback, and manage rosters.
- **Roster Management:** At `/Team/ManageRoster/{id}`, captains can add or remove players from their official squad.

### 3. Public & Spectators
- **Public Schedule:** At `/Home/Schedule`, filter fixtures by tournament, sport category, or match status.
- **Live Scorecards:** At `/Match/Details/{id}`, view head-to-head scorecards, venues, commentary highlights, and player lineups.
- **Leaderboard / Points Table:** At `/Home/Leaderboard`, view real-time standings calculated automatically (Matches Played, Won, Lost, Drawn, Points, and Form Guide).

---

## 📂 Project Structure
```
web.net project/
├── KhelTrack.sln                 <-- Visual Studio Solution File
├── README.md
└── KhelTrack/
    ├── KhelTrack.csproj          <-- Project File (.NET 10 / LTS)
    ├── Program.cs                <-- Dependency Injection, Identity, Auth, DB Seeder
    ├── appsettings.json          <-- Flexible Provider Connection Strings
    ├── kheltrack.db              <-- Pre-seeded SQLite Database
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   └── DbInitializer.cs      <-- Automatic Seeding for Roles, Users, Tournaments, Teams, Fixtures
    ├── Models/
    │   ├── ApplicationUser.cs    <-- Extends IdentityUser (RollNo, Dept)
    │   ├── Tournament.cs         <-- Sport categories & guidelines
    │   ├── Team.cs               <-- Departmental squads & Captain FK
    │   ├── Player.cs             <-- Verified athlete details & jersey #
    │   ├── MatchFixture.cs       <-- Round, grounds, scores, status
    │   ├── AuditLog.cs           <-- Immutable audit records
    │   ├── Enums.cs              <-- Status enumerations
    │   └── ViewModels/           <-- Strongly-typed ViewModels & validation
    ├── Services/
    │   ├── IAuditService.cs
    │   └── AuditService.cs       <-- Injected Audit logger
    ├── Controllers/
    │   ├── HomeController.cs     <-- Landing page, schedule, leaderboard
    │   ├── AccountController.cs  <-- Identity login, register, profile
    │   ├── TournamentController.cs <-- Tournament lifecycle
    │   ├── TeamController.cs     <-- Team registration & dynamic roster
    │   ├── MatchController.cs    <-- Fixtures & live score entry
    │   └── AdminController.cs    <-- Dashboard, approvals, audit logs, users
    ├── Views/                    <-- Razor Views (.cshtml) with Bootstrap 5
    └── wwwroot/
        ├── css/site.css          <-- Custom athletic dark theme & glowing UI
        └── js/site.js           <-- Dynamic client-side scripts
```

---

## 🔒 Security & Data Integrity
- Role-based authorization (`[Authorize(Roles = "Admin")]`, `[Authorize]`).
- Anti-Forgery token validation (`[ValidateAntiForgeryToken]`) across all state-mutating POST actions.
- Client-side validation (`jquery.validate`) coupled with server-side `ModelState.IsValid`.
- Relational integrity with `DeleteBehavior.Restrict` on foreign keys to eliminate cascade cycles.
