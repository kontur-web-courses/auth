// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource("photos_app", "Web Photos", new []
                {
                    "role", "subscription", "testing"
                })
            };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[] 
            {
                new ApiResource("photos_service", "Сервис фотографий")
                {
                    Scopes = { "photos" },
                    ApiSecrets = { new Secret("photos_service_secret".Sha256()) },
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("photos", "Фотографии")
            };
        
        public static IEnumerable<Client> Clients =>
            new Client[] 
            {
                new Client
                {
                    ClientId = "Photos App by OAuth",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "photos" }
                },

                new Client
                {
                    ClientId = "Photos App by OIDC",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-passport" },
                    AllowOfflineAccess = true,

                    RequireConsent = true,
                    RedirectUris = { "https://localhost:5001/signin-passport" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "photos_app",
                        "photos"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AccessTokenLifetime = 30,
                },
                new Client
                {
                    ClientId = "Photos SPA",
                    RequireClientSecret = false,
                    RequirePkce = true,

                    AllowedGrantTypes = GrantTypes.Code,

                    AccessTokenLifetime = 2*60,

                    RequireConsent = false,

                    RedirectUris = { "https://localhost:8001/authentication/login-callback" },

                    PostLogoutRedirectUris = { "https://localhost:8001/authentication/logout-callback" },

                    AllowedCorsOrigins = { "https://localhost:8001" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "photos"
                    },

                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = false,
                }
            };
    }
}