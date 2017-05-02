using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoodForSheep.Models
{
    public class GameUser
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }

        public int GameID { get; set; }
        public Game Game { get; set; }

        public string GameStatus { get; set; }
    }
}
