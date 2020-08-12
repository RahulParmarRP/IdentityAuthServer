using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAuthServer.ViewModel
{
    public class ExternalLoginModel
    {
        public string IdToken { get; set; }

        public string UserRole { get; set; }
    }
}
