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
            var providerName = context.Request.Raw.Get("provider");
            if (string.IsNullOrEmpty(providerName))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "invalid provider");
                return;
            }

            var idToken = context.Request.Raw.Get("id_token");
            if (string.IsNullOrEmpty(idToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "invalid external token");
                return;
            }

            var validationSettings = new ValidationSettings();
            //{ Audience = new[] { "YOUR_CLIENT_ID" } };

            Payload userInfoPayload = await GoogleJsonWebSignature
                .ValidateAsync(idToken, validationSettings);

            if (userInfoPayload == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "couldn't retrieve user info from specified provider, please make sure that token is not expired.");
                return;
            }

            /**
             *  // Get profile information from payload
             *   String email = payload.getEmail();
             *   boolean emailVerified = Boolean.valueOf(payload.getEmailVerified());
             *   String name = (String) payload.get("name");
             *   String pictureUrl = (String) payload.get("picture");
             *   String locale = (String) payload.get("locale");
             *   String familyName = (String) payload.get("family_name");
             *   String givenName = (String) payload.get("given_name");
             *
             *   // Use or store profile information
             */

            // this part signs in user directly if user already exists
            var userExternalId = userInfoPayload.Subject;
            if (!string.IsNullOrEmpty(userExternalId))
            {
                // find if external user is already logged in 
                var extenalLoggedInUser = await _userManager.FindByLoginAsync(providerName, userExternalId);

                // this part if external user is already logged in
                if (extenalLoggedInUser != null)
                {
                    // so find the user from db to retrieve claims to send into token
                    var user = await _userManager.FindByIdAsync(extenalLoggedInUser.Id);

                    // to find the db user claims
                    var userClaims = await _userManager.GetClaimsAsync(user);

                    context.Result = new GrantValidationResult(user.Id,
                        providerName, userClaims, providerName, null);
                    return;
                }
            }

            // this part registers user
            // get email from the post request context
            var requestEmail = context.Request.Raw.Get("email");
            if (string.IsNullOrEmpty(requestEmail))
            {
                context.Result = await _nonEmailUserProcessor.ProcessAsync(userInfoPayload,
                    providerName);
                return;
            }

            // if email found in post request context
            // that is from client application email parameter is sent
            context.Result = await _emailUserProcessor.ProcessAsync(userInfoPayload,
                requestEmail, providerName);
            return;
        }
    }
}
