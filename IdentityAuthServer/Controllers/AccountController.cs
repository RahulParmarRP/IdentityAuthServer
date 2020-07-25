using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityAuthServer.Models;
using IdentityAuthServer.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAuthServer.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // asp identity
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [Route("test")]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
            })
            .ToArray();
        }

        [HttpPost]
        [Route("register")]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //_logger.LogInformation("User created a new account with password.");

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    //_logger.LogInformation("User created a new account with password.");
                    //return RedirectToLocal(returnUrl);
                    return Ok();
                }
                //AddErrors(result);
            }

            // If execution got this far, something failed, redisplay the form.
            //return View(model);
            return BadRequest();
        }

        [HttpPost]
        [Route("login")]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This does not count login failures towards account lockout
                // To enable password failures to trigger account lockout,
                // set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //_logger.LogInformation("User logged in.");
                    return Ok();
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning("User account locked out.");
                //    return RedirectToAction(nameof(Lockout));
                //}
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return BadRequest();
                }
            }

            // If execution got this far, something failed, redisplay the form.
            return BadRequest();
        }

        [HttpPost]
        [Route("logout")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            //_logger.LogInformation("User logged out.");
            return Ok();
        }
    }
}