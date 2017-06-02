using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoodForSheep.Models
{
    public class Game
    {
        public int ID { get; set; }
        public int BGGID { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
