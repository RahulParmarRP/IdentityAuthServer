using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace IdentityAuthServer
{
    public static class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("myresourceapi", "My Resource API")
                {
                    Scopes = {"api"}
                }
            };
        }

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api1", "My API")
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // for public API
                new Client
                {
                    ClientId = "clientId_for_public_user_api",
                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    // scopes that client has access to
                    AllowedScopes = { "api1" },
                },
                /*
                 * The presence (or absence) of the sub claim lets the API 
                 * distinguish between calls on behalf of clients 
                 * and calls on behalf of users.
                 */
                // for role claims
                new Client
                {
                    ClientId = "clientId_for_claims_based_api",
                    // now the user requires username and password
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    // scopes that client has access to
                    AllowedScopes = { "api1" },
                },

                new Client
                {
                    ClientId = "custom_claims",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "ApiThree" },
                    Claims =
                    {
                        new ClientClaim("role","ClientAdmin")
                    }
                },

                new Client
                {
                    ClientId = "openid",
                    // now the user requires username and password
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = new List<string>
                    {
                        "api1",
                        //IdentityServerConstants.StandardScopes.OpenId,
                        //IdentityServerConstants.StandardScopes.Profile
                    }
                }
            };

        public static List<TestUser> TestUsers =>
             new List<TestUser>
             {
                 new TestUser
                 {
                     SubjectId = "1",
                     Username = "user",
                     Password = "user",
                     Claims = new[]
                     {
                         new Claim("roleType", "CanReaddata")
                     }
                 },
                 new TestUser
                 {
                     SubjectId = "2",
                     Username = "admin",
                     Password = "admin",
                     Claims = new[]
                     {
                         new Claim("roleType", "CanUpdatedata")
                     }
                 }
             };
    }
}
