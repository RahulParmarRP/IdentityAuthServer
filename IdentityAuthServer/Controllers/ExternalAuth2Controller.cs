using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityAuthServer.Models;
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
            var callbackUrl = Url.Action(
                action: nameof(ExternalLoginCallback),
                controller: "ExternalAuth2",
                values: new { ReturnUrl = returnUrl });

            //var props = new AuthenticationProperties
            //{
            //    RedirectUri = callbackUrl,
            //    Items =
            //    {
            //        { "scheme", provider },
            //        { "returnUrl", returnUrl }
            //    }
            //};
            AuthenticationProperties properties = _signInManager
                .ConfigureExternalAuthenticationProperties(provider, callbackUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
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

                /*
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
                */
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

                await _userManager.AddLoginAsync(newUser, info);
                var newUserClaims = info.Principal.Claims.Append(new Claim("userId", newUser.Id));
                await _userManager.AddClaimsAsync(newUser, newUserClaims);
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                return Redirect("http://localhost:3000");
            }
            return Redirect("http://localhost:3000");
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("http://localhost:3000");
        }
    }
}