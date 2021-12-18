﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
                    ApiSecrets = { new Secret("photos_service_secret".Sha256()) }
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
        
                    // NOTE: показывать ли пользователю страницу consent со списком запрошенных разрешений
                    RequireConsent = true,

                    // NOTE: куда отправлять после логина
                    RedirectUris = { "https://localhost:5001/signin-passport" },

                    // NOTE: куда предлагать перейти после логаута
                    PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-passport" },

                    AllowedScopes = new List<string>
                    {
                        // NOTE: Позволяет запрашивать id token
                        IdentityServerConstants.StandardScopes.OpenId,
                        // NOTE: Позволяет запрашивать профиль пользователя через id token
                        IdentityServerConstants.StandardScopes.Profile,
                        // NOTE: Позволяет запрашивать email пользователя через id token
                        IdentityServerConstants.StandardScopes.Email,
                        "photos_app", "photos"
                    },

                    // NOTE: Надо ли добавлять информацию о пользователе в id token при запросе одновременно
                    // id token и access token, как это происходит в code flow.
                    // Либо придется ее получать отдельно через user info endpoint.
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AccessTokenLifetime = 30,
                    AllowOfflineAccess = true
                },
                new Client
                {
                    ClientId = "Photos SPA",
                    // NOTE: SPA не может хранить секрет, потому что полные исходники доступны в браузере
                    RequireClientSecret = false,
                    // NOTE: Поэтому для безопасного получения токена необходимо использовать Proof Key for Code Exchange
                    // Эта опция включена по умолчанию, но здесь пусть будет включена явно
                    RequirePkce = true,

                    AllowedGrantTypes = GrantTypes.Code,
    
                    // NOTE: показывать ли пользователю страницу consent со списком запрошенных разрешений
                    RequireConsent = false,

                    // NOTE: куда отправлять после логина
                    RedirectUris = { "TODO: полный URL куда возвращать после логина на сервере авторизации" },

                    // NOTE: куда предлагать перейти после логаута
                    PostLogoutRedirectUris = { "TODO: полный URL куда возвращать после выхода на сервере авторизации" },

                    // NOTE: откуда могут приходить запросы из JS
                    AllowedCorsOrigins = { "TODO: надо зарегистрировать адрес приложения в качестве допустимого origin" },

                    AllowedScopes = new List<string>
                    {
                        // NOTE: Позволяет запрашивать id token
                        IdentityServerConstants.StandardScopes.OpenId,
                        // NOTE: Позволяет запрашивать профиль пользователя через id token
                        IdentityServerConstants.StandardScopes.Profile,
                        // NOTE: Позволяет запрашивать email пользователя через id token
                        IdentityServerConstants.StandardScopes.Email,
                    },

                    // NOTE: Надо ли добавлять информацию о пользователе в id token при запросе одновременно
                    // id token и access token, как это происходит в code flow.
                    // Либо придется ее получать отдельно через user info endpoint.
                    AlwaysIncludeUserClaimsInIdToken = true,

                    // NOTE: refresh token точно не будет использоваться
                    AllowOfflineAccess = false,
                    AccessTokenLifetime = 2*60,
                }
            };
    }
}