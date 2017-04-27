using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WoodForSheep.Data;
using Microsoft.AspNetCore.Identity;
using WoodForSheep.Models;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WoodForSheep.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsersController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            context = dbContext;
        }

        // GET: /Users/
        public IActionResult Index()
        {
            // Returns all users.
            IList<ApplicationUser> users = context.Users.ToList();

            return View(users);
        }

        // GET: /Users/Profile
        [HttpGet("Users/Profile/{userName}")]
        public IActionResult Profile(string userName)
        {
            // Find User.
            ApplicationUser user = context.Users.Single(u => u.UserName == userName);
            // Find User's library.
            // TODO: Replace with viewmodel solution.
            ViewBag.Library = context
                .GameUsers
                .Include(g => g.Game)
                .Where(u => u.UserID == user.Id)
                .ToList();
            return View(user);
        }
    }
}
