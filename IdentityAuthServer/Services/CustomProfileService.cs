using IdentityAuthServer.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityAuthServer.Services
{
    public class CustomProfileService : IProfileService
    {

        private readonly UserManager<AppUser> _userManager;

        public CustomProfileService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            /*
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);
            var claims = principal.Claims.ToList();

            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            if (user.Configuration != null)
                claims.Add(new Claim(PropertyConstants.Configuration, user.Configuration));

            context.IssuedClaims = claims;
            */

            /*
             *  var user = _userManager.GetUserAsync(context.Subject).Result;
                var claims = new List<Claim>
                {
                    new Claim("TestFullName", user.FullName),
                };
                context.IssuedClaims.AddRange(claims);          
                return Task.FromResult(0);
             */

            /*
            context.AddRequestedClaims(context.Subject.Claims);
            context.IssuedClaims.AddRange(context.Subject.Claims);
            */

            /*
            var requestedClaims = context.RequestedClaimTypes;
            var userId = context.Subject.Claims.FirstOrDefault(x => x.Type == "sub");
            var user = await _userManager.FindByIdAsync(userId?.Value);
            var userClaims = await _userManager.GetClaimsAsync(user);
            context.IssuedClaims = userClaims
                .Where(x => context.RequestedClaimTypes.Contains(x.Type))
                .ToList()
                ;
            */
            /* var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);
 
            var claims = principal.Claims.ToList();
            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
 
            // Add custom claims in token here based on user properties or any other source
            claims.Add(new Claim("employee_id", user.EmployeeId ?? string.Empty));
 
            context.IssuedClaims = claims;
             * 
             */
            var subjectId = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(subjectId);
            if (user == null) return;
            var userClaims = await _userManager.GetClaimsAsync(user);
            var claims = new List<Claim>();
            foreach (var userClaim in userClaims)
            {
                claims.Add(new Claim(userClaim.Type, userClaim.Value));
            }
            context.IssuedClaims = claims;
            //return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(0);
        }
    }
}
