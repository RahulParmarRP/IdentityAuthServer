using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpPost]
        [Route("signinexternal")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(
                action: nameof(ExternalLoginCallback),
                controller: "ExternalAuth2",
                values: new { ReturnUrl = returnUrl });

            AuthenticationProperties properties = _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("signinexternalcallback")]
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
            }
            else if (!result.Succeeded)
            {
                // If the user does not have an account, then ask the user to create an account.
            }
        }
    }
}