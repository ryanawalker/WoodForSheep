using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WoodForSheep.Models
{
    public class Trade
    {
        public int ID { get; set; }

        public int GameInitID { get; set; }
        public Game GameInit { get; set; }
        public int GameReceiveID { get; set; }
        public Game GameReceive { get; set; }

        public string UserInitID { get; set; }
        public string UserInitStatus { get; set; }
        public ApplicationUser UserInit { get; set; }

        public string UserReceiveID { get; set; }
        public string UserReceiveStatus { get; set; }
        public ApplicationUser UserReceive { get; set; }
    }
}
