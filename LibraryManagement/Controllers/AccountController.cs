using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LibraryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            // Clear any existing errors
            ModelState.Clear();
            
            // Validate required fields
            if (string.IsNullOrEmpty(email))
                ModelState.AddModelError("email", "Email is required");
            if (string.IsNullOrEmpty(password))
                ModelState.AddModelError("password", "Password is required");
            
            if (ModelState.IsValid)
            {
                // Use the email as username for sign-in
                var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return await RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                    return View();
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            // Clear any existing errors
            ModelState.Clear();
            
            // Validate required fields
            if (string.IsNullOrEmpty(email))
                ModelState.AddModelError("email", "Email is required");
            if (string.IsNullOrEmpty(password))
                ModelState.AddModelError("password", "Password is required");
            if (string.IsNullOrEmpty(confirmPassword))
                ModelState.AddModelError("confirmPassword", "Confirm Password is required");
            if (password != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Passwords do not match");
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = email, Email = email };
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    // Check if User role exists, if not create it
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }

                    // Add user to User role
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    // Don't automatically sign in - redirect to login page instead
                    TempData["SuccessMessage"] = "Registration successful! You can now log in with your credentials.";
                    return RedirectToAction(nameof(Login));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private async Task<IActionResult> RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                // Check if user is in admin role
                var user = await _userManager.GetUserAsync(User);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else
                {
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
            }
        }
    }
}