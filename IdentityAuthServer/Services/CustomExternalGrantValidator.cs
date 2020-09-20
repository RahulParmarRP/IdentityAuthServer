using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Models;
using static Google.Apis.Auth.GoogleJsonWebSignature;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using IdentityAuthServer.Models;
using IdentityAuthServer.Interfaces;

namespace IdentityAuthServer.Services
{
    public class CustomExternalGrantValidator : IExtensionGrantValidator
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly INonEmailUserProcessor _nonEmailUserProcessor;
        private readonly IEmailUserProcessor _emailUserProcessor;
        public CustomExternalGrantValidator(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        INonEmailUserProcessor nonEmailUserProcessor,
        IEmailUserProcessor emailUserProcessor
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _nonEmailUserProcessor = nonEmailUserProcessor;
            _emailUserProcessor = emailUserProcessor;
        }

        public string GrantType => "custom_external_grant";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var provider = context.Request.Raw.Get("provider");
            if (string.IsNullOrEmpty(provider))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "invalid provider");
                return;
            }

            var token = context.Request.Raw.Get("id_token");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "invalid external token");
                return;
            }

            var validationSettings = new ValidationSettings();
            //{ Audience = new[] { "YOUR_CLIENT_ID" } };

            Payload userInfoPayload = await GoogleJsonWebSignature
                .ValidateAsync(token, validationSettings);

            if (userInfoPayload == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "couldn't retrieve user info from specified provider, please make sure that token is not expired.");
                return;
            }

            // this part signs in user if already exists
            var externalId = userInfoPayload.Subject;
            if (!string.IsNullOrEmpty(externalId))
            {
                var user = await _userManager.FindByLoginAsync(provider, externalId);
                if (user != null)
                {
                    // to find the user extra claims
                    user = await _userManager.FindByIdAsync(user.Id);
                    var userClaims = await _userManager.GetClaimsAsync(user);
                    context.Result = new GrantValidationResult(user.Id,
                        provider, userClaims, provider, null);
                    return;
                }
            }

            // this part registers user
            // get email from the post request context
            var requestEmail = context.Request.Raw.Get("email");
            if (string.IsNullOrEmpty(requestEmail))
            {
                context.Result = await _nonEmailUserProcessor.ProcessAsync(userInfoPayload,
                    provider);
                return;
            }

            context.Result = await _emailUserProcessor.ProcessAsync(userInfoPayload,
                requestEmail, provider);
            return;
        }
    }
}
