using Google.Apis.Auth;
using IdentityAuthServer.Interfaces;
using IdentityAuthServer.Models;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        public async Task<GrantValidationResult> ProcessAsync(GoogleJsonWebSignature.Payload userInfoPayload,
            string provider)
        {
            var userEmail = userInfoPayload.Email;
            var userExternalId = userInfoPayload.Subject;

            //if (provider.ToLower() == "linkedin")
            //    userEmail = userInfo.Value<string>("emailAddress");

            if (userEmail == null)
            {
                // if user is already logged in using external provider
                var existingUser = await _userManager.FindByLoginAsync(provider, userExternalId);
                if (existingUser == null)
                {
                    var customResponse = new Dictionary<string, object>();
                    customResponse.Add("userInfo", userInfoPayload);

                    return new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                        "could not retrieve user's email from the given provider, include email parameter and send request again.", customResponse);
                }
                else
                {
                    // find user from db
                    var appUser = await _userManager.FindByIdAsync(existingUser.Id);
                    // get user claims and add into access token
                    var userClaims = await _userManager.GetClaimsAsync(appUser);
                    return new GrantValidationResult(existingUser.Id,
                        provider, userClaims, provider, null);
                }
            }
            else
            {
                var newUser = new AppUser
                {
                    Email = userEmail,
                    UserName = userEmail,
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

                    var idResult = await _userManager.AddLoginAsync(newUser, userLoginInfo);

                    //var result = await _userManager.AddClaimsAsync(newUser, new Claim[]{
                    //      new Claim(IdentityModel.JwtClaimTypes.Name, "Bob Smith"),
                    //      new Claim(IdentityModel.JwtClaimTypes.GivenName, "Bob"),
                    //      new Claim(IdentityModel.JwtClaimTypes.FamilyName, "Smith"),
                    //      new Claim(IdentityModel.JwtClaimTypes.WebSite, "http://bob.com"),
                    //      new Claim("location", "somewhere")
                    //  })
                    //    ;

                    var userClaimsToAdd = new List<Claim>
                {
                    //new Claim(ClaimTypes.NameIdentifier, userInfoPayload.Name),
                    new Claim(IdentityModel.JwtClaimTypes.Name, userInfoPayload.Name),
                    new Claim(IdentityModel.JwtClaimTypes.FamilyName, userInfoPayload.FamilyName),
                    new Claim(IdentityModel.JwtClaimTypes.GivenName, userInfoPayload.GivenName),
                    new Claim(IdentityModel.JwtClaimTypes.Email, userInfoPayload.Email),
                    //new Claim(IdentityModel.JwtClaimTypes.Subject, userInfoPayload.Subject),
                    new Claim(IdentityModel.JwtClaimTypes.Issuer, userInfoPayload.Issuer),
                    new Claim(IdentityModel.JwtClaimTypes.Picture, userInfoPayload.Picture),
                }
                    ;

                    var claimsIdentityResult = await _userManager.AddClaimsAsync(newUser, userClaimsToAdd);
                    // create custom claims for the user found from Google idToken
                    //var userRoleCustomClaim = new Claim("userRole", externalLoginModel.UserRole);
                    //var b = await _userManager.AddClaimsAsync(newUser);
                    //var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);


                    //var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                    //foreach (var Rol in roles)
                    //{
                    //    identity.AddClaim(new Claim(ClaimTypes.Role, Rol));
                    //}
                    //identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                    //identity.AddClaim(new Claim(ClaimTypes.Email, user.Correo));
                    //identity.AddClaim(new Claim(ClaimTypes.MobilePhone, user.Celular));
                    //identity.AddClaim(new Claim("FullName", user.FullName));
                    //identity.AddClaim(new Claim("Empresa", user.Empresa));
                    //identity.AddClaim(new Claim("ConnectionStringsName", user.ConnectionStringsName));

                    var userClaims = await _userManager.GetClaimsAsync(newUser);

                    return new GrantValidationResult(newUser.Id, provider, userClaims, provider, null);
                }
                return new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "user could not be created, please try again");
            }
        }
    }
}
