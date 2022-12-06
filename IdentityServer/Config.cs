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
                new IdentityResources.Email()

            };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[] 
            {
                new ApiResource("photos_service", "Сервис фотографий")
                {
                    Scopes = { "photos" }
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
        
                 RequireConsent = false,

                // NOTE: ÐºÑÐ´Ð° Ð¾ÑÐ¿ÑÐ°Ð²Ð»ÑÑÑ Ð¿Ð¾ÑÐ»Ðµ Ð»Ð¾Ð³Ð¸Ð½Ð°
                RedirectUris = { "https://localhost:5001/signin-passport" },

                AllowedScopes = new List<string>
                {
                    // NOTE: ÐÐ¾Ð·Ð²Ð¾Ð»ÑÐµÑ Ð·Ð°Ð¿ÑÐ°ÑÐ¸Ð²Ð°ÑÑ id token
                    IdentityServerConstants.StandardScopes.OpenId,
                    // NOTE: ÐÐ¾Ð·Ð²Ð¾Ð»ÑÐµÑ Ð·Ð°Ð¿ÑÐ°ÑÐ¸Ð²Ð°ÑÑ Ð¿ÑÐ¾ÑÐ¸Ð»Ñ Ð¿Ð¾Ð»ÑÐ·Ð¾Ð²Ð°ÑÐµÐ»Ñ ÑÐµÑÐµÐ· id token
                    IdentityServerConstants.StandardScopes.Profile,
                    // NOTE: ÐÐ¾Ð·Ð²Ð¾Ð»ÑÐµÑ Ð·Ð°Ð¿ÑÐ°ÑÐ¸Ð²Ð°ÑÑ email Ð¿Ð¾Ð»ÑÐ·Ð¾Ð²Ð°ÑÐµÐ»Ñ ÑÐµÑÐµÐ· id token
                    IdentityServerConstants.StandardScopes.Email
                },

                // NOTE: ÐÐ°Ð´Ð¾ Ð»Ð¸ Ð´Ð¾Ð±Ð°Ð²Ð»ÑÑÑ Ð¸Ð½ÑÐ¾ÑÐ¼Ð°ÑÐ¸Ñ Ð¾ Ð¿Ð¾Ð»ÑÐ·Ð¾Ð²Ð°ÑÐµÐ»Ðµ Ð² id token Ð¿ÑÐ¸ Ð·Ð°Ð¿ÑÐ¾ÑÐµ Ð¾Ð´Ð½Ð¾Ð²ÑÐµÐ¼ÐµÐ½Ð½Ð¾
                // id token Ð¸ access token, ÐºÐ°Ðº ÑÑÐ¾ Ð¿ÑÐ¾Ð¸ÑÑ
                // ÐÐ¸Ð±Ð¾ Ð¿ÑÐ¸Ð´ÐµÑÑÑ ÐµÐµ Ð¿Ð¾Ð»ÑÑÐ°ÑÑ Ð¾ÑÐ´ÐµÐ»ÑÐ½Ð¾ ÑÐµÑÐµÐ· user info endpoint.
                AlwaysIncludeUserClaimsInIdToken = true,
                }


            };
        
    }
}