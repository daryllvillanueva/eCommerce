using BestStoreMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("Admin/[controller]/{action=Index}/{id?}")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var users = userManager.Users.OrderByDescending(u => u.CreatedAt).ToList();


            return View(users);
        }

        public async Task<IActionResult> DeleteAccount(string? id)
        {
            if(id == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var appUser = await userManager.FindByNameAsync(id);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var currentUser = await userManager.GetUserAsync(User);
            if(currentUser!.Id == appUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
            }

            var result = await userManager.DeleteAsync(appUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Users");
            }
            return View();
        }

           

        public async Task<IActionResult> Details(string? id)
        {
            if(id == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var appUser = await userManager.FindByNameAsync(id);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Users");
            }

            ViewBag.Roles = await userManager.GetRolesAsync(appUser);

            return View(appUser);
        }

    }
}
