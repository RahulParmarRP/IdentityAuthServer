using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAuthServer.Models
{
    public class AppUser : IdentityUser
    {
        // Add additional profile data for application users by adding properties to this class
        //public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
    }
}
