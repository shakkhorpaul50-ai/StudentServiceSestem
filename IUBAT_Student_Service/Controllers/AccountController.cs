using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IUBAT_Student_Service.Models;
using IUBAT_Student_Service.Models.ViewModels;

namespace IUBAT_Student_Service.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Normalize StudentId
            if (!string.IsNullOrWhiteSpace(model.StudentId))
                model.StudentId = model.StudentId.Trim();

            // Pre-check uniqueness for friendly error (DB also enforces unique filtered index)
            if (ModelState.IsValid)
            {
                var trimmedId = model.StudentId.Trim();
                var exists = await _userManager.Users.AnyAsync(u => u.StudentId != null && u.StudentId.ToLower() == trimmedId.ToLower());
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.StudentId), "This Student ID is already taken. Please use a different ID.");
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    StudentId = model.StudentId.Trim(),
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    // Map duplicate StudentId DB error to friendly message
                    if (error.Description.Contains("StudentId") || error.Description.Contains("IX_AspNetUsers_StudentId"))
                        ModelState.AddModelError(nameof(model.StudentId), "This Student ID is already taken.");
                    else
                        ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!string.IsNullOrWhiteSpace(model.StudentId))
                model.StudentId = model.StudentId.Trim();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
                    var isStudent = await _userManager.IsInRoleAsync(user, "Student");

                    // Enforce StudentId check for Student accounts (ID supplied by student at login)
                    if (isStudent && !string.IsNullOrWhiteSpace(user.StudentId))
                    {
                        if (string.IsNullOrWhiteSpace(model.StudentId))
                        {
                            ModelState.AddModelError(nameof(model.StudentId), "Student ID is required for student login.");
                            return View(model);
                        }

                        if (!string.Equals(user.StudentId.Trim(), model.StudentId!.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            ModelState.AddModelError(nameof(model.StudentId), "Invalid Student ID.");
                            return View(model);
                        }
                    }
                    // If student has no StudentId yet (legacy), allow login without check
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var loggedUser = await _userManager.FindByEmailAsync(model.Email);
                    if (loggedUser != null && await _userManager.IsInRoleAsync(loggedUser, "Staff"))
                    {
                        return RedirectToAction("AllRequests", "Staff");
                    }
                    return RedirectToAction("MyRequests", "Student");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
