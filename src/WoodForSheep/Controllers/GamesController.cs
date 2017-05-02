using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WoodForSheep.Models;
using WoodForSheep.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WoodForSheep.Controllers
{
    public class GamesController : Controller
    {
        private ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public GamesController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, 
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            context = dbContext;
        }

        // TODO: When search is implemented, will have to handle redirects
        // from the search page to create new db entries if game doesn't exist.

        // GET: /Games/
        public IActionResult Index()
        {
            return View();
        }
        
        // GET: /Games/ViewGame/<ID>
        
        public IActionResult ViewGame(int id)
        {
            // Make sure game with ID exists.
            if (context.Games.SingleOrDefault(g => g.ID == id) == null)
            {
                return Redirect("/Games");
            }

            // Pass game info from database to view.
            Game theGame = context.Games.SingleOrDefault(g => g.ID == id);

            ViewBag.Game = theGame;
            return View();
        }

        public IActionResult Add(int gameId)
        {
            // If user isn't logged in, redirect to login page.
            if (!_signInManager.IsSignedIn(User))
            {
                return Redirect("/Account/Login");
            }

            // Else, get user ID.
            ClaimsPrincipal currentUser = this.User;
            var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Make sure game doesn't already exist in user's Library.
            IList<GameUser> existingItems = context.GameUsers
                .Where(gu => gu.GameID == gameId)
                .Where(gu => gu.UserID == userId).ToList();
            if (existingItems.Count == 0)
            {
                // Add game to user's library.
                GameUser gameUser = new GameUser
                {
                    GameID = gameId,
                    UserID = userId,
                };

                context.GameUsers.Add(gameUser);
                context.SaveChanges();
            }

            return Redirect("/Games");
        }
    }
}
