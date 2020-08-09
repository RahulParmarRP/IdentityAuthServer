using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityAuthServer.Models;
using IdentityAuthServer.ViewModel;
using IdentityModel.Client;
using IdentityServer4.Validation;
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

        // asp identity
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public ExternalAuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            /*
             * After you have verified the token, check if the user is already in your user database. If so, establish an authenticated session for the user. If the user isn't yet in your user database, create a new user record from the information in the ID token payload, and establish a session for the user. You can prompt the user for any additional profile information you require when you detect a newly created user in your app
             */

            /*
import com.google.api.client.googleapis.auth.oauth2.GoogleIdToken;
import com.google.api.client.googleapis.auth.oauth2.GoogleIdToken.Payload;
import com.google.api.client.googleapis.auth.oauth2.GoogleIdTokenVerifier;

...

GoogleIdTokenVerifier verifier = new GoogleIdTokenVerifier.Builder(transport, jsonFactory)
    // Specify the CLIENT_ID of the app that accesses the backend:
    .setAudience(Collections.singletonList(CLIENT_ID))
    // Or, if multiple clients access the backend:
    //.setAudience(Arrays.asList(CLIENT_ID_1, CLIENT_ID_2, CLIENT_ID_3))
    .build();

// (Receive idTokenString by HTTPS POST)

GoogleIdToken idToken = verifier.verify(idTokenString);
if (idToken != null) {
  Payload payload = idToken.getPayload();

  // Print user identifier
  String userId = payload.getSubject();
  System.out.println("User ID: " + userId);

  // Get profile information from payload
  String email = payload.getEmail();
  boolean emailVerified = Boolean.valueOf(payload.getEmailVerified());
  String name = (String) payload.get("name");
  String pictureUrl = (String) payload.get("picture");
  String locale = (String) payload.get("locale");
  String familyName = (String) payload.get("family_name");
  String givenName = (String) payload.get("given_name");

  // Use or store profile information
  // ...

} else {
  System.out.println("Invalid ID token.");
}
             */

            Payload payload;
            var provider = "google";
            try
            {
                var validationSettings = new ValidationSettings
                {
                    //Audience = new[] { "YOUR_CLIENT_ID" }
                };

                payload = await ValidateAsync(externalLoginModel.IdToken, validationSettings);
                // It is important to add your ClientId as an audience in order to make sure
                // that the token is for your application!
                var user = await GetOrCreateExternalLoginUser(
                    "google",
                    payload.Subject,
                    payload.Email);

                // way 1
                //var token = await GenerateToken(user);


                // way 2
                /// add extra claims for the external login user
                // If they exist, add claims to the user for:
                //    Given (first) name
                //    Locale
                //    Picture
                // Get the information about the user from the external login provider
                //var info = await _signInManager.GetExternalLoginInfoAsync();
                //if (info.Principal.HasClaim(c => c.Type == "urn:google:picture"))
                //{
                //    await _userManager.AddClaimAsync(user,
                //        info.Principal.FindFirst("urn:google:picture"));
                //}
                // Include the access token in the properties
                //var props = new AuthenticationProperties();
                //props.StoreTokens(info.AuthenticationTokens);
                //props.IsPersistent = true;
                //await _signInManager.SignInAsync(user, props);




                // way 3 
                //var client = new HttpClient();
                //var tokenreq = new TokenRequest()
                //{
                //    Address = "https://demo.identityserver.io/connect/token",
                //    GrantType = "custom",
                //    ClientId = "client",
                //    ClientSecret = "secret",
                //    Parameters = {
                //        { "custom_parameter", "custom value"},
                //        { "scope", "api1" }
                //    }
                //};
                //var response = await client.RequestTokenAsync(tokenreq);



                // way 4
                //var user = await _userManager.FindByLoginAsync(provider, externalId);
                //if (null != user)
                //{
                //    user = await _userManager.FindByIdAsync(user.Id);
                //    var userClaims = await _userManager.GetClaimsAsync(user);
                //    context.Result = new GrantValidationResult(user.Id, provider, userClaims, provider, null);
                //    return;
                //}

                //if (user != null)
                //{
                //    user = await _userManager.FindByIdAsync(user.Id);
                //    var userClaims = await _userManager.GetClaimsAsync(user);
                //    context.Result = new GrantValidationResult(
                //        user.Id,
                //        provider,
                //        userClaims,
                //        provider,
                //        null);
                //}
                return new JsonResult(Ok("token"));
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