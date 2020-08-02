using IdentityAuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityAuthServer.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            IOptions<IdentityOptions> optionsAccessor
            ) : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var claimsIdentity = await base.GenerateClaimsAsync(user);
            //if (!claimsIdentity.HasClaim(x => x.Type == JwtClaimTypes.Subject))
            //{
            //    var sub = user.SubjectId;
            //    claimsIdentity.AddClaim(new Claim(JwtClaimTypes.Subject, sub));
            //}
            // add custom claims
            claimsIdentity.AddClaim(new Claim("TestClaim", "role"));
            return claimsIdentity;
        }
    }
}
