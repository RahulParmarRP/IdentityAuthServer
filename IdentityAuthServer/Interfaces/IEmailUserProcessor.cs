using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace IdentityAuthServer.Interfaces
{
    public interface IEmailUserProcessor
    {
        Task<GrantValidationResult> ProcessAsync(Payload userInfoPayload, string email, string provider);
    }
}
