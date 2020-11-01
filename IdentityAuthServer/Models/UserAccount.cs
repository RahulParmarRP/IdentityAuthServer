using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAuthServer.Models
{
    public class UserAccount
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string PrimaryEmail { get; set; }
        public string WorkEmail { get; set; }
        public string Contact { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
    }
}
