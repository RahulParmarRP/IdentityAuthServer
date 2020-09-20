using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityAuthServer.Models;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalAuth2Controller : ControllerBase
    {
        // asp identity
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public ExternalAuth2Controller(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        //[AllowAnonymous]
        [Route("ExternalLogin")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            //var callbackUrl = Url.Action(
            //    action: nameof(ExternalLoginCallback),
            //    controller: "ExternalAuth2",
            //    values: new { ReturnUrl = returnUrl });

            //var props = new AuthenticationProperties
            //{
            //    RedirectUri = callbackUrl,
            //    Items =
            //    {
            //        { "scheme", provider },
            //        { "returnUrl", returnUrl }
            //    }
            //};

            // start challenge and roundtrip the return URL and scheme 
            //var props = new AuthenticationProperties
            //{
            //    RedirectUri = Url.Action(nameof(ExternalLoginCallback)),
            //    Items =
            //    {
            //        { "returnUrl", returnUrl },
            //        { "scheme", provider },
            //    }
            //};
            var scheme = "Google";
            var defaultReturnUrl = "http://localhost:3000";
            var callbackUrl = Url.Action(nameof(ExternalLoginCallback));
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = callbackUrl,
                //items =
                //{
                //    { "returnurl", "http://localhost:3000" },
                //    { "scheme", scheme },
                //}
            };

            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "ExternalAuth2", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(scheme, redirectUrl);

            //AuthenticationProperties properties = _signInManager
            //    .ConfigureExternalAuthenticationProperties("Google", callbackUrl);
            return Challenge(properties, scheme);
        }

        /**
         * 
         * 
         * Hi all. Can anyone point me in the right direction for this?:
We have an existing resource server (RS) that currently handles all of the authentication.
We have a single page application (SPA), and mobile clients (MC).
We use external providers (Google, Twitter, etc.) (EP)
Social sign in with the web client is currently the following flow: SPA redirects to RS, which redirects to the EP. EP redirects to the RS and then the RS passes tokens in SPA redirect URL. SPA uses tokens to register/login by calling the correct API, passing the tokens back to the server. This call results in resource server access/refresh tokens.
Login with our database is a straight-forward oauth token call.
We would like to create users on the RS because we feel that's a separate concern to authentication.
What we're trying to do now is start to migrate this flow to IdentityServer4, without trusting the client with the external provider's tokens. It seems like the hybrid flow might be a way to do this, but I don't understand how we can make that work with our SPA and creating users on the RS. Any advice would be appreciated. Even if we created the user on the identity server, I don't see how we can then create the authentication tokens for use with the SPA and mobile clients.
         */

        //[HttpGet]
        [HttpGet]
        //[AllowAnonymous]
        [Route("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            var result2 = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // read external identity from the temporary cookie
            var result1 = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            ////get the tokens
            //var tokens = result2.Properties.GetTokens();
            //var idToken = tokens.Where(x => x.Name.Equals("id_token")).FirstOrDefault().Value;
            //get the tokens
            var tokens = result1.Properties.GetTokens();
            //var idToken = tokens.Where(x => x.Name.Equals("id_token")).FirstOrDefault().Value;
            var accessToken2 = tokens.Where(x => x.Name.Equals("access_token")).FirstOrDefault().Value;

            // read external identity from the temporary cookie
            //var authresult = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            //if (authresult?.Succeeded != true)
            //{
            //    throw new Exception("External authentication error");
            //}

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return new RedirectResult($"{returnUrl}?error=externalsigninerror");

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                //CredentialsDTO credentials = _authService.ExternalSignIn(info);
                //return new RedirectResult($"{returnUrl}?token={credentials.JWTToken}");
                //return RedirectToLocal(returnUrl);
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var externalResult = await _signInManager
                    .UpdateExternalAuthenticationTokensAsync(info);
                //_logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                //return Redirect(returnUrl);
            }
            else if (!result.Succeeded) //user does not exist yet
            {
                // If the user does not have an account, then ask the user to create an account.
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var newUser = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(newUser);

                if (!createResult.Succeeded)
                {
                    throw new Exception(createResult.Errors
                        .Select(e => e.Description)
                        .Aggregate((errors, error) => $"{errors}, {error}"));
                }
                //await _userManager.AddClaimAsync(newUser,
                //       info.Principal.FindFirst("urn:google:picture"));

                var a = await _userManager.AddLoginAsync(newUser, info);

                var additionalClaims = info.Principal.Claims.Append(new Claim("userId", newUser.Id));

                var b = await _userManager.AddClaimsAsync(newUser, additionalClaims);

                // Include the access token in the properties
                var props = new AuthenticationProperties();
                props.StoreTokens(info.AuthenticationTokens);
                props.IsPersistent = true;


                await _signInManager.SignInAsync(newUser, isPersistent: false);

                // remove external auth provider cookie
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                //_logger.LogInformation(
                //    "User created an account using {Name} provider.",
                //    info.LoginProvider);

                //return LocalRedirect(returnUrl);
                return Redirect(returnUrl);
            }
            return Redirect(returnUrl);
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            var result2 = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            //get the tokens
            var tokens = result.Properties.GetTokens();
            var idToken = tokens.Where(x => x.Name.Equals("id_token")).FirstOrDefault().Value;
            //if (_logger.IsEnabled(LogLevel.Debug))
            //{
            //    var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
            //    _logger.LogDebug("External claims: {@claims}", externalClaims);
            //}

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return BadRequest("Error loading external login information.");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            var emailClaim = ClaimTypes.Email;

            if (signInResult.Succeeded)
            {
                //_logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                //return RedirectToLocal(returnUrl);
                var user = await _userManager.FindByNameAsync(info.Principal.FindFirstValue(emailClaim));
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
            }
            else if (!signInResult.Succeeded) //user does not exist yet
            {
                // If the user does not have an account, then ask the user to create an account.
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var newUser = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(newUser);
            }
            return Ok();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("http://localhost:3000");
        }
    }
}