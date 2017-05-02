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

        // POST: /Trades/Reject
        [HttpPost]
        public IActionResult Reject(int tradeId)
        {
            Trade trade = context.Trades
                .Single(t => t.ID == tradeId);

            trade.UserReceiveStatus = "Rejected";

            context.Trades.Update(trade);
            context.SaveChanges();

            return Redirect("/Trades");
        }

        // POST: /Trades/Accept
        [HttpPost]
        public IActionResult Accept(int tradeId)
        {
            Trade trade = context.Trades
                .Single(t => t.ID == tradeId);

            trade.UserReceiveStatus = "Accepted";

            context.Trades.Update(trade);
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

            context.Trades.Update(trade);
            context.SaveChanges();

            return Redirect("/Trades");
        }

    }
}
