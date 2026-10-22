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
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IAuditService auditService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _auditService = auditService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                await _auditService.LogAsync(
                    action: "USER_LOGIN",
                    entityType: "User",
                    entityId: user.Id,
                    details: $"User {user.FullName} ({user.Email}) successfully logged in.",
                    userId: user.Id,
                    userEmail: user.Email
                );

                TempData["Success"] = $"Welcome back, {user.FullName}!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirect based on role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CollegeRollNo = model.CollegeRollNo,
                Department = model.Department,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Ensure role exists and assign
                string selectedRole = model.Role == "Captain" ? "Captain" : "Student";
                if (!await _roleManager.RoleExistsAsync(selectedRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(selectedRole));
                }

                await _userManager.AddToRoleAsync(user, selectedRole);

                // Sign in
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Log audit
                await _auditService.LogAsync(
                    action: "USER_REGISTERED",
                    entityType: "User",
                    entityId: user.Id,
                    details: $"New user registered: {user.FullName} ({user.Email}) as Role: {selectedRole}, Roll: {user.CollegeRollNo}, Dept: {user.Department}",
                    userId: user.Id,
                    userEmail: user.Email
                );

                TempData["Success"] = $"Account successfully created! You are registered as {selectedRole}.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = _userManager.GetUserId(User);
            var userEmail = User.Identity?.Name;

            await _signInManager.SignOutAsync();

            if (userId != null)
            {
                await _auditService.LogAsync(
                    action: "USER_LOGOUT",
                    entityType: "User",
                    entityId: userId,
                    details: $"User {userEmail} logged out.",
                    userId: userId,
                    userEmail: userEmail
                );
            }

            TempData["Info"] = "You have been logged out safely.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            var myTeams = await _context.Teams
                .Include(t => t.Tournament)
                .Include(t => t.Players)
                .Where(t => t.CaptainId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.MyTeams = myTeams;

            return View(user);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
