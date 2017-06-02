using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WoodForSheep.Data;
using Microsoft.AspNetCore.Identity;
using WoodForSheep.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WoodForSheep.Controllers
{
    public class TradesController : Controller
    {
        private ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public TradesController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            context = dbContext;
        }

        // GET: /Trades/
        public IActionResult Index()
        {
            // If user isn't logged in, redirect to login page.
            if (!_signInManager.IsSignedIn(User))
            {
                return Redirect("/Account/Login");
            }

            // Else, get user ID.
            ClaimsPrincipal currentUser = this.User;
            var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            // TODO: Remove this and substitute for a ViewModel solution.
            ViewBag.UserID = userId;

            // query Trades table, search for trades where UserInitID or UserReceiveID == userId. Return list of those trades to view.
            // TODO: There has to be a dozen better ways to do this, figure one out.
            // TODO: Add "awaiting other user completion" category with according logic.
            ViewBag.proposedTrades = context.Trades
                .Include(t => t.GameInit).Include(t => t.GameReceive).Include(t => t.UserInit).Include(t => t.UserReceive)
                .Where(t => t.UserInitID == userId || t.UserReceiveID == userId)
                .Where(t => t.UserInitStatus == "Accepted" && t.UserReceiveStatus == "Pending")
                .ToList();

            ViewBag.acceptedTrades = context.Trades
                .Include(t => t.GameInit).Include(t => t.GameReceive).Include(t => t.UserInit).Include(t => t.UserReceive)
                .Where(t => t.UserInitID == userId || t.UserReceiveID == userId)
                .Where(t => ((t.UserInitStatus == "Accepted" && t.UserReceiveStatus == "Accepted")
                || (t.UserInitStatus == "Completed" && t.UserReceiveStatus == "Accepted")
                || (t.UserInitStatus == "Accepted" && t.UserReceiveStatus == "Completed")))
                .ToList();

            ViewBag.rejectedTrades = context.Trades
                .Include(t => t.GameInit).Include(t => t.GameReceive).Include(t => t.UserInit).Include(t => t.UserReceive)
                .Where(t => t.UserInitID == userId || t.UserReceiveID == userId)
                .Where(t => t.UserInitStatus == "Rejected" || t.UserReceiveStatus == "Rejected")
                .ToList();

            ViewBag.completedTrades = context.Trades
                .Include(t => t.GameInit).Include(t => t.GameReceive).Include(t => t.UserInit).Include(t => t.UserReceive)
                .Where(t => t.UserInitID == userId || t.UserReceiveID == userId)
                .Where(t => t.UserInitStatus == "Completed" && t.UserReceiveStatus == "Completed")
                .ToList();

            return View();
        }

        // POST: /Trades/Propose
        [HttpPost]
        public IActionResult Propose(int gameReceiveID, int gameInitID, string userReceiveID)
        {
            // If user isn't logged in, redirect to login page.
            if (!_signInManager.IsSignedIn(User))
            {
                return Redirect("/Account/Login");
            }

            ClaimsPrincipal currentUser = this.User;
            var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            Trade newTrade = new Trade
            {
                UserInitID = userId,
                GameInitID = gameInitID,
                UserInitStatus = "Accepted",
                UserReceiveID = userReceiveID,
                GameReceiveID = gameReceiveID,
                UserReceiveStatus = "Pending"
            };

            GameUser gameUser = context.GameUsers
                .Single(gu => gu.GameID == gameInitID && gu.UserID == userId);

            gameUser.GameStatus = "Reserved";

            context.GameUsers.Update(gameUser);
            context.Trades.Add(newTrade);
            context.SaveChanges();

            return Redirect("/Trades");
        }

        // POST: /Trades/Reject
        [HttpPost]
        public IActionResult Reject(int tradeId)
        {
            Trade trade = context.Trades
                .Single(t => t.ID == tradeId);
            GameUser gameUserReceive = context.GameUsers
                .Single(gu => gu.GameID == trade.GameReceiveID && gu.UserID == trade.UserReceiveID);
            GameUser gameUserInit = context.GameUsers
                .Single(gu => gu.GameID == trade.GameInitID && gu.UserID == trade.UserInitID);

            trade.UserReceiveStatus = "Rejected";
            trade.UserInitStatus = "Rejected";

            gameUserReceive.GameStatus = "Free";
            gameUserInit.GameStatus = "Free";

            context.Trades.Update(trade);
            context.GameUsers.Update(gameUserReceive);
            context.GameUsers.Update(gameUserInit);
            context.SaveChanges();

            return Redirect("/Trades");
        }

        // POST: /Trades/Accept
        [HttpPost]
        public IActionResult Accept(int tradeId)
        {
            // Finds trade being accepted.
            Trade trade = context.Trades
                .Single(t => t.ID == tradeId);

            // Gets list of all currently proposed trades where the user accepting is considering trading the game involved in the current trade.
            IList<Trade> tradesList = context.Trades
                .Where(t => (t.GameInitID == trade.GameReceiveID && t.UserInitID == trade.UserReceiveID) || (t.GameReceiveID == trade.GameReceiveID && t.UserReceiveID == trade.UserReceiveID))
                .Where(t => t.UserInitStatus != "Rejected" || t.UserInitStatus  != "Completed")
                .Where(t => t.ID != tradeId)
                .ToList();

            // Iterates through list, rejecting open trades due to game being reserved for new trade, freeing up those games for their respective owners.
            foreach (var tempTrade in tradesList)
            {
                tempTrade.UserInitStatus = "Rejected";
                tempTrade.UserReceiveStatus = "Rejected";
                
                GameUser tempGameUserReceive = context.GameUsers
                    .Single(gu => gu.GameID == tempTrade.GameReceiveID && gu.UserID == tempTrade.UserReceiveID);
                GameUser tempGameUserInit = context.GameUsers
                    .Single(gu => gu.GameID == tempTrade.GameInitID && gu.UserID == tempTrade.UserInitID);

                tempGameUserInit.GameStatus = "Free";
                tempGameUserReceive.GameStatus = "Free";

                context.GameUsers.Update(tempGameUserInit);
                context.GameUsers.Update(tempGameUserReceive);
                context.Trades.Update(tempTrade);
            }

            // Finds GameUser object for accepted trade and user.
            GameUser gameUserReceive = context.GameUsers
                .Single(gu => gu.GameID == trade.GameReceiveID && gu.UserID == trade.UserReceiveID);
            GameUser gameUserInit = context.GameUsers
                    .Single(gu => gu.GameID == trade.GameInitID && gu.UserID == trade.UserInitID);

            // Sets status of trade to accepted and reserves game in accepting user's library.
            trade.UserReceiveStatus = "Accepted";
            trade.UserInitStatus = "Accepted";
            gameUserReceive.GameStatus = "Reserved";
            gameUserInit.GameStatus = "Reserved";

            context.Trades.Update(trade);
            context.GameUsers.Update(gameUserReceive);
            context.SaveChanges();

            return Redirect("/Trades");
        }

        // POST: /Trades/Complete
        [HttpPost]
        public IActionResult Complete(int tradeId)
        {
            Trade trade = context.Trades
                .Single(t => t.ID == tradeId);

            ClaimsPrincipal currentUser = this.User;
            var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (userId == trade.UserInitID)
            {
                trade.UserInitStatus = "Completed";
            }
            else
            {
                trade.UserReceiveStatus = "Completed";
            }

            if (trade.UserInitStatus == "Completed" && trade.UserReceiveStatus == "Completed")
            {
                GameUser gameUserReceive = context.GameUsers
                    .Single(gu => gu.GameID == trade.GameReceiveID && gu.UserID == trade.UserReceiveID);
                GameUser gameUserInit = context.GameUsers
                    .Single(gu => gu.GameID == trade.GameInitID && gu.UserID == trade.UserInitID);

                string tempUserID = gameUserReceive.UserID;
                gameUserReceive.UserID = gameUserInit.UserID;
                gameUserInit.UserID = tempUserID;

                gameUserReceive.GameStatus = "Free";
                gameUserInit.GameStatus = "Free";

                context.GameUsers.Update(gameUserReceive);
                context.GameUsers.Update(gameUserInit);
            }

            context.Trades.Update(trade);
            context.SaveChanges();

            return Redirect("/Trades");
        }

    }
}
