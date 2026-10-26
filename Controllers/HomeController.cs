
using CYBP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CYBP.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public HomeController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
       
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrator"))
            {
                ViewBag.UserEmails = await _userManager.Users
                    .Where(u => u.Email != null)
                    .OrderBy(u => u.Email)
                    .Select(u => u.Email!)
                    .ToListAsync();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult KeepAlive()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> IdleLogout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}
