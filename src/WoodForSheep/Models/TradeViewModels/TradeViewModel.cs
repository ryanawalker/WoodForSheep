using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoodForSheep.Models.TradeViewModels
{
    public class TradeViewModel
    {
        public List<Trade> AcceptedTrades { get; set; }
        public List<Trade> CompletedTrades { get; set; }
        public List<Trade> ProposedTrades { get; set; }
        public List<Trade> RejectedTrades { get; set; }
        public string UserId { get; set; }
    }
}
