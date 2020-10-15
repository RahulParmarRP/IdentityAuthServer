using Google.Apis.Auth;
using IdentityAuthServer.Interfaces;
using IdentityAuthServer.Models;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAuthServer.Processors
{
    public class NonEmailUserProcessor : INonEmailUserProcessor
    {
        private readonly UserManager<AppUser> _userManager;
        public NonEmailUserProcessor(UserManager<AppUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<GrantValidationResult> ProcessAsync(GoogleJsonWebSignature.Payload userInfo,
            string provider)
        {
            var userEmail = userInfo.Email;
            var userExternalId = userInfo.Subject;

            //if (provider.ToLower() == "linkedin")
            //    userEmail = userInfo.Value<string>("emailAddress");

            if (userEmail == null)
            {
                var existingUser = await _userManager.FindByLoginAsync(provider, userExternalId);
                if (existingUser == null)
                {
                    var customResponse = new Dictionary<string, object>();
                    customResponse.Add("userInfo", userInfo);
                    return new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                        "could not retrieve user's email from the given provider, include email parameter and send request again.", customResponse);
                }
                else
                {
                    existingUser = await _userManager.FindByIdAsync(existingUser.Id);
                    // get user claims and add into access token
                    var userClaims = await _userManager.GetClaimsAsync(existingUser);
                    return new GrantValidationResult(existingUser.Id,
                        provider, userClaims, provider, null);
                }
            }
            else
            {
                var newUser = new AppUser
                {
                    Email = userEmail,
                    UserName = userEmail
                };

                var identityResult = await _userManager.CreateAsync(newUser);

                // new user created
                if (identityResult.Succeeded)
                {
                    // https://docs.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes?view=netcore-3.1
                    // If they exist, add claims to the user for:
                    //    Given (first) name
                    //    Locale
                    //    Picture
                    //if (info.Principal.HasClaim(c => c.Type == ClaimTypes.GivenName))
                    //{
                    //    await _userManager.AddClaimAsync(user,
                    //        info.Principal.FindFirst(ClaimTypes.GivenName));
                    //}

                    //if (info.Principal.HasClaim(c => c.Type == "urn:google:locale"))
                    //{
                    //    await _userManager.AddClaimAsync(user,
                    //        info.Principal.FindFirst("urn:google:locale"));
                    //}

                    //if (info.Principal.HasClaim(c => c.Type == "urn:google:picture"))
                    //{
                    //    await _userManager.AddClaimAsync(user,
                    //        info.Principal.FindFirst("urn:google:picture"));
                    //}

                    // create user login object
                    var userLoginInfo = new UserLoginInfo(provider,
                        userExternalId, provider);

                    await _userManager.AddLoginAsync(newUser, userLoginInfo);

                    var userClaims = await _userManager.GetClaimsAsync(newUser);
                    return new GrantValidationResult(newUser.Id, provider, userClaims, provider, null);
                }
                return new GrantValidationResult(TokenRequestErrors.InvalidRequest, 
                    "user could not be created, please try again");
            }
        }
    }
}
