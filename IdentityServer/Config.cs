// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId()
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
                }
            };
    }
}