using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoodForSheep.Models.UserViewModels
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public List<GameUser> Library { get; set; }
        public bool UserIsProfileOwner { get; set; }
        public bool UserIsSignedIn { get; set; }
        public List<GameUser> ViewerLibrary { get; set; }
    }
}
