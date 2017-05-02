using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoodForSheep.Models
{
    public class Trade
    {
        public int ID { get; set; }
        public int GameInitID { get; set; }
        public int GameReceiveID { get; set; }
        public string UserInitID { get; set; }
        public string UserReceiveID { get; set; }
        public string UserInitStatus { get; set; }
        public string UserReceiveStatus { get; set; }
    }
}
