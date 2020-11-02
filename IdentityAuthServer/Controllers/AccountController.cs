using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityAuthServer.Models;
using IdentityAuthServer.Utilities;
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
        public async Task<IActionResult> RegisterAndSignInSimultaneously(RegisterViewModel model)
        {
            // ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };
                var password = model.Password;
                var result = password != null ?
                    await _userManager.CreateAsync(user, password) :
                    await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.UserRole))
                    {
                        //await _userManager.AddToRoleAsync(user, "SomeRole");
                        var userRoleClaim = new Claim("userRole", model.UserRole);
                        var identityResult = await _userManager.AddClaimAsync(user, userRoleClaim);
                    }

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
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

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


        [HttpPost]
        [Route("updateProfile")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromBody]UserAccount userAccount)
        {
            var claims = from c in User.Claims
                         select new
                         {
                             c.Type,
                             c.Value
                         };

            //var userClaimsToAdd = new List<Claim>
            //    {
            //        //new Claim(ClaimTypes.NameIdentifier, userInfoPayload.Name),
            //      new Claim(IdentityModel.JwtClaimTypes.Name, userInfoPayload.Name),
            //      new Claim(IdentityModel.JwtClaimTypes.FamilyName, userInfoPayload.FamilyName),
            //      new Claim(IdentityModel.JwtClaimTypes.GivenName, userInfoPayload.GivenName),
            //      new Claim(IdentityModel.JwtClaimTypes.Email, userInfoPayload.Email),
            //      //new Claim(IdentityModel.JwtClaimTypes.Subject, userInfoPayload.Subject),
            //      new Claim(IdentityModel.JwtClaimTypes.Issuer, userInfoPayload.Issuer),
            //      new Claim(IdentityModel.JwtClaimTypes.Picture, userInfoPayload.Picture),
            //  }
            //;

            AppUser user;
            //IEnumerable<Claim> userClaims;
            if (!string.IsNullOrEmpty(userAccount.UserId))
            {
                // so find the user from db to retrieve claims to send into token
                user = await _userManager.FindByIdAsync(userAccount.UserId);
                //if (user != null)
                //{
                //    // to find the db user claims
                //    userClaims = await _userManager.GetClaimsAsync(user);
                //}
            }
            else
            {
                return BadRequest("userId is not valid!");
            }


            if (!string.IsNullOrEmpty(userAccount.Name))
            {
                // to find the db user claims
                var userClaims = await _userManager.GetClaimsAsync(user);
                var nameClaim = userClaims.FirstOrDefault(claim => claim.Type.Equals(IdentityModel.JwtClaimTypes.Name));
                if (nameClaim != null)
                {
                    var removeClaimResult = await _userManager.RemoveClaimAsync(user, nameClaim);
                }
                var newNameClaim = new Claim(IdentityModel.JwtClaimTypes.Name, userAccount.Name);
                var claimsIdentityResult = await _userManager.AddClaimAsync(user, newNameClaim);
            }

            if (!string.IsNullOrEmpty(userAccount.Gender))
            {
                // to find the db user claims
                var userClaims = await _userManager.GetClaimsAsync(user);
                var genderClaim = userClaims.FirstOrDefault(claim => claim.Type.Equals(IdentityModel.JwtClaimTypes.Gender));
                if (genderClaim != null)
                {
                    var removeGenderClaimResult = await _userManager.RemoveClaimAsync(user, genderClaim);
                }
                var newGenderClaim = new Claim(IdentityModel.JwtClaimTypes.Gender, userAccount.Gender);
                var claimsIdentityResult = await _userManager.AddClaimAsync(user, newGenderClaim);
            }

            if (!string.IsNullOrEmpty(userAccount.DateOfBirth))
            {
                // to find the db user claims
                var userClaims = await _userManager.GetClaimsAsync(user);
                var birthDateClaim = userClaims.FirstOrDefault(claim => claim.Type.Equals(IdentityModel.JwtClaimTypes.BirthDate));
                if (birthDateClaim != null)
                {
                    var removeBirthDateClaimResult = await _userManager.RemoveClaimAsync(user, birthDateClaim);
                }
                var newBirthDateClaim = new Claim(IdentityModel.JwtClaimTypes.BirthDate, userAccount.DateOfBirth, ClaimValueTypes.Date);
                var claimsIdentityResult = await _userManager.AddClaimAsync(user, newBirthDateClaim);
            }

            if (!string.IsNullOrEmpty(userAccount.WorkEmail))
            {
                // to find the db user claims
                var userClaims = await _userManager.GetClaimsAsync(user);
                var workEmailClaim = userClaims.FirstOrDefault(claim => claim.Type.Equals(CustomClaimsConstants.WorkEmail));
                if (workEmailClaim != null)
                {
                    var removeWorkEmailClaimResult = await _userManager.RemoveClaimAsync(user, workEmailClaim);
                }
                var customClaimWorkEmail = new Claim(CustomClaimsConstants.WorkEmail, userAccount.WorkEmail);
                // add work email to user claims
                var claimsIdentityResult = await _userManager.AddClaimAsync(user, customClaimWorkEmail);
            }

            if (!string.IsNullOrEmpty(userAccount.PrimaryEmail))
            {
                // Update it with the values from the view model
                user.Email = userAccount.PrimaryEmail;
            }

            if (!string.IsNullOrEmpty(userAccount.PhoneNumber))
            {
                user.PhoneNumber = userAccount.PhoneNumber;
            }

            await _userManager.UpdateAsync(user);

            return Ok("profile updated");
        }
    }
}