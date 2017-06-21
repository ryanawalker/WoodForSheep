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
using Microsoft.EntityFrameworkCore;
using WoodForSheep.Models.GamesViewModels;

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

        // GET: /Games/
        public IActionResult Index()
        {
            return View();
        }
        
        // GET: /Games/ViewGame/<ID>
        
        public IActionResult ViewGame(int id)
        {
            ViewGameViewModel model = new ViewGameViewModel();
            model.BGGID = id;
            return View(model);
        }

        public IActionResult Add(int BGGID, string name)
        {
            // If user isn't logged in, redirect to login page.
            if (!_signInManager.IsSignedIn(User))
            {
                return Redirect("/Account/Login");
            }

            // Else, get user ID.
            ClaimsPrincipal currentUser = this.User;
            var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Check to make sure game exists in database. If not, add game to database.
            IList<Game> existingGames = context.Games
                .Where(g => g.BGGID == BGGID)
                .ToList();
            if (existingGames.Count == 0)
            {
                Game newGame = new Game
                {
                    BGGID = BGGID,
                    Name = name
                };
                context.Games.Add(newGame);
                context.SaveChanges();
            }

            Game game = context.Games.Single(g => g.BGGID == BGGID);

            // Make sure game doesn't already exist in user's Library.
            IList<GameUser> existingItems = context.GameUsers
                .Where(gu => gu.Game.BGGID == BGGID)
                .Where(gu => gu.UserID == userId).ToList();
            if (existingItems.Count == 0)
            {
                // Add game to user's library.
                GameUser gameUser = new GameUser
                {
                    GameID = game.ID,
                    UserID = userId,
                    GameStatus = "Free"
                };

                context.GameUsers.Add(gameUser);
                context.SaveChanges();
            }

            return Redirect("/Games");
        }
    }
}
