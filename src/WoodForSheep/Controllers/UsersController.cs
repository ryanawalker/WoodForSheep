using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WoodForSheep.Data;
using Microsoft.AspNetCore.Identity;
using WoodForSheep.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WoodForSheep.Models.UserViewModels;

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
            // Create ViewModel.
            ProfileViewModel model = new ProfileViewModel();

            // Find User whose pages is being viewed, add user to ViewModel.
            ApplicationUser user = context.Users.Single(u => u.UserName == userName);
            model.User = user;
            
            // Find Selected User's library.
            model.Library = context
                .GameUsers
                .Include(g => g.Game)
                .Where(u => u.UserID == user.Id)
                .ToList();

            // Determine if viewer is a signed in user.
            // If not, determine if user is profile owner, and fill in viewer's library if not profile owner.
            if (!_signInManager.IsSignedIn(User))
            {
                model.UserIsSignedIn = false;
            }
            else
            {
                model.UserIsSignedIn = true;
                ClaimsPrincipal currentUser = this.User;
                var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                model.UserIsProfileOwner = (user.Id == userId);
                if (!model.UserIsProfileOwner)
                {
                    model.ViewerLibrary = context
                        .GameUsers
                        .Include(g => g.Game)
                        .Where(u => u.UserID == userId)
                        .ToList();
                }
            }

            return View(model);
        }
    }
}
