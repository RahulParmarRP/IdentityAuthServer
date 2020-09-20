using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using static IdentityServer4.IdentityServerConstants;

namespace IdentityAuthServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource()
                {
                    Name = "customRoleClaim",
                    DisplayName="Your profile data",
                    UserClaims =  new List<string>
                    {
                        "userRole"
                        //"TestRole",
                        //"roleType"
                    }
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            var efeResourceClaimTypes = new List<string>
            {
                JwtClaimTypes.Name,
                JwtClaimTypes.Email,
                JwtClaimTypes.Profile,
                "userRole","TestRole","roleType"
            };

            return new List<ApiResource>
            {
                new ApiResource("api1", "API1", efeResourceClaimTypes),
                new ApiResource
                {
                    Name = "api2",
                    DisplayName = "API #2",
                    Description = "Allow the application to access API #2 on your behalf",
                    Scopes = new List<string> {"api2.read", "api2.write"},
                    ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
                    UserClaims = new List<string> {"role"}
                }
            };
        }

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api1", "My API"),
                new ApiScope("api2.read", "Read Access to API #2"),
                new ApiScope("api2.write", "Write Access to API #2"),
                new ApiScope(name: "read",   displayName: "Read your data."),
                new ApiScope(name: "write",  displayName: "Write your data."),
                new ApiScope(name: "delete", displayName: "Delete your data.")
            };

        /*
         * https://www.scottbrady91.com/Identity-Server/Getting-Started-with-IdentityServer-4
         Resource Owner Password Credentials (ROPC) Grant Type
            At some point, you’ll be asked why the login page cannot be hosted within 
            the client application, or maybe you’ll have someone on a UX team scream at you 
            for asking them to use a web browser to authenticate the user. 
            Sure, this could be achieved using the ROPC/password grant type; however, 
            this is a security anti-pattern, and this grant type is only included 
            in the OAuth 2.0 specification to help legacy applications. 
            That’s applications considered legacy in 2012.
            For a full write-up of everything wrong with the Resource Owner grant type, 
            check out my article Why the Resource Owner Password Credentials Grant Type 
            is not Authentication nor Suitable for Modern Applications.
        */
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
                    AllowedScopes =
                    {
                        "api1"
                    },
                },
                /*
                 * The presence (or absence) of the sub claim lets the API 
                 * distinguish between calls on behalf of clients 
                 * and calls on behalf of users.
                 */
                // for role claims
                new Client
                {
                    AllowOfflineAccess = true,
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    ClientId = "clientId_for_claims_based_api",
                    // now the user requires username and password
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    // scopes that client has access to
                    AllowedScopes =
                    {
                        "api1",
                        StandardScopes.Profile,
                        StandardScopes.Email,
                        //StandardScopes.OpenId,
                        //"read", "write", "delete",
                        "customRoleClaim"
                    },

                    /* AlwaysSendClientClaims
                     * If set, the client claims will be sent for every flow. 
                     * If not, only for client credentials flow (default is false)
                     */
                    Claims =
                    {
                        new ClientClaim("roleType","ClientAdmin")
                    }
                },
                new Client
                {
                    ClientId = "custom_claims",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "ApiThree"
                    },
                    Claims =
                    {
                        new ClientClaim("roleType","ClientAdmin")
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
                },
                new Client
                {
                    ClientId = "spa_example",
                    ClientName = "SPA Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    AllowedScopes = new List<string> {"openid", "profile", "api1"},
                    RedirectUris = new List<string> {
                        "http://localhost:3000/auth-callback",
                        "http://localhost:3000/silent-refresh.html"
                    },
                    PostLogoutRedirectUris = new List<string> {
                        "http://localhost:3000/"
                    },
                    AllowedCorsOrigins = new List<string> {
                        "http://localhost:3000"
                    },
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientId = "spa_custom_external_login_provider_grant_extension",
                    ClientName = "SPA Client Custom Grant Extension",
                    AllowedGrantTypes =  new List<string> {
                        GrantType.ResourceOwnerPassword,
                        "custom_external_grant"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    // scopes that client has access to
                    AllowedScopes =
                    {
                        "api1",
                        StandardScopes.Profile,
                        StandardScopes.Email,
                        // openid scope also enables userinfo endpoint
                        StandardScopes.OpenId,
                        //"read", "write", "delete",
                        "customRoleClaim"
                    },
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
