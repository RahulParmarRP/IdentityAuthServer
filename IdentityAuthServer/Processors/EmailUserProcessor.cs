using IdentityAuthServer.Interfaces;
using IdentityAuthServer.Models;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace IdentityAuthServer.Processors
{
    public class EmailUserProcessor : IEmailUserProcessor
    {
        private readonly UserManager<AppUser> _userManager;
        public EmailUserProcessor(UserManager<AppUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<GrantValidationResult> ProcessAsync(Payload userInfoPayload, string email, string provider)
        {
            var userEmail = email;
            var userExternalId = userInfoPayload.Subject;

            if (string.IsNullOrWhiteSpace(userExternalId))
            {
                return new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "could not retrieve user Id from the token provided");
            }

            var existingUser = _userManager.FindByEmailAsync(userEmail).Result;
            if (existingUser != null)
            {
                return new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "User with specified email already exists");
            }

            var newUser = new AppUser
            {
                Email = userEmail,
                UserName = userEmail
            };

            var result = _userManager.CreateAsync(newUser).Result;
            if (result.Succeeded)
            {
                var userLoginInfo = new UserLoginInfo(provider, userExternalId, provider);
                await _userManager.AddLoginAsync(newUser, userLoginInfo);
                // get user claims and add into access token
                var userClaims = _userManager.GetClaimsAsync(newUser).Result;
                return new GrantValidationResult(newUser.Id, provider, userClaims, provider, null);
            }
            return new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                "could not create user , please try again.");
        }
    }
}
