using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Auth;
using IdentityAuthServer.Models;
using IdentityAuthServer.ViewModel;
using IdentityModel.Client;
using IdentityServer4;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace IdentityAuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IdentityServerTools _identityServerTools;
        public ExternalAuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IdentityServerTools identityServerTools
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _identityServerTools = identityServerTools;
        }
        /*
         * Verify the integrity of the ID token
            After you receive the ID token by HTTPS POST, you must verify the integrity of the token. To verify that the token is valid, ensure that the following criteria are satisfied:
            The ID token is properly signed by Google. Use Google's public keys (available in JWK or PEM format) to verify the token's signature. These keys are regularly rotated; examine the Cache-Control header in the response to determine when you should retrieve them again.
            The value of aud in the ID token is equal to one of your app's client IDs. This check is necessary to prevent ID tokens issued to a malicious app being used to access data about the same user on your app's backend server.
            The value of iss in the ID token is equal to accounts.google.com or https://accounts.google.com.
            The expiry time (exp) of the ID token has not passed.
            If you want to restrict access to only members of your G Suite domain, verify that the ID token has an hd claim that matches your G Suite domain name.
            Rather than writing your own code to perform these verification steps, we strongly recommend using a Google API client library for your platform, or a general-purpose JWT library. For development and debugging, you can call our tokeninfo validation endpoint.
         */
        [HttpPost]
        [Route("google")]
        public async Task<IActionResult> AuthenticateGoogleSignin(ExternalLoginModel externalLoginModel)
        {
            Payload payload;
            var provider = "google";
            try
            {
                var validationSettings = new ValidationSettings();
                //{ Audience = new[] { "YOUR_CLIENT_ID" } };

                payload = await GoogleJsonWebSignature
                    .ValidateAsync(externalLoginModel.IdToken, validationSettings);

                var user = await GetOrCreateExternalLoginUser(
                    "google",
                    payload.Subject,
                    payload.Email);

                // create custom claims for the user found from Google idToken
                var userRoleClaim = new Claim("userRole", externalLoginModel.UserRole);

                var result = await _userManager.AddClaimAsync(user, userRoleClaim);

                // get the new IdentityServer4 issued access token for the user
                var userSubscriptionToken = await _identityServerTools
                    .IssueClientJwtAsync(
                    clientId: "userSubscriptionClient",
                    lifetime: 15,
                    scopes: new[] { "userSubscription.read" },
                    audiences: new[] { "https://user.subscription.service/" },
                    additionalClaims: new List<Claim>()
                    {
                        userRoleClaim
                    });

                var jwttoken = await _identityServerTools
                   .IssueJwtAsync(
                   lifetime: 15,
                   claims: new List<Claim>() {
                       userRoleClaim
                   });

                return Ok(new
                {
                    access_token = "want_to_give_identity_server4_provided_token"
                });
            }
            catch
            {
                // Invalid token
            }
            return BadRequest();
        }

        public async Task<AppUser> GetOrCreateExternalLoginUser(
            string provider,
            string key,
            string email)
        {
            // Login already linked to a user
            var user = await _userManager.FindByLoginAsync(provider, key);
            if (user != null)
                return user;

            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // No user exists with this email address, we create a new one
                user = new AppUser
                {
                    Email = email,
                    UserName = email
                };

                await _userManager.CreateAsync(user);
            }

            // Link the user to this login
            var info = new UserLoginInfo(provider, key, provider.ToUpperInvariant());
            var result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
                return user;

            // if nothing can be done return null user
            return null;
        }

        // How to generate a JWT token is not in this post's scope
        //public async Task<string> GenerateToken(User user)
        //{
        //    var claims = await GetUserClaims(user);
        //    return GenerateToken(user, claims);
        //}

        /*
         * public static async Task<JObject> GenerateLocalAccessTokenResponse(string userName, string role, string userId, string clientId, string provider)
    {

        var tokenExpiration = TimeSpan.FromDays(1);

        var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);

        identity.AddClaim(new Claim(ClaimTypes.Name, userName));
        identity.AddClaim(new Claim("ClientId", clientId));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        identity.AddClaim(new Claim(ClaimTypes.Role, role));


        var data = new Dictionary<string, string>
        {
            {"userName", userName},
            {"client_id", clientId},
            {"role", role},
            {"provider", provider},
            {"userId", userId}
        };

        var props = new AuthenticationProperties(data);

        var ticket = new AuthenticationTicket(identity, props);

        var accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);

        var tokenResponse = new JObject(
            new JProperty("userName", userName),
            new JProperty("client_id", clientId),
            new JProperty("role", role),
            new JProperty("provider", provider),
            new JProperty("userId", userId),
            new JProperty("access_token", accessToken),
            new JProperty("token_type", "bearer"),
            new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString()),
            new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
            new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString())
            );

        return tokenResponse;
    }
        */
    }
}