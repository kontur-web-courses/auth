// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using IdentityServer4;

namespace IdentityServerHost.Quickstart.UI
{
    public class TestUsers
    {
        public static List<TestUser> Users
        {
            get
            {
                var address = new
                {
                    street_address = "One Hacker Way",
                    locality = "Heidelberg",
                    postal_code = 69118,
                    country = "Germany"
                };
                
                return new List<TestUser>
                {
                    new TestUser
                    {
                        SubjectId = "818727",
                        Username = "alice",
                        Password = "alice",
                        Claims =
                        {
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                            new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
                        }
                    },
                    new TestUser
                    {
                        SubjectId = "88421113",
                        Username = "bob",
                        Password = "bob",
                        Claims =
                        {
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Bob"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                            new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                            new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
                        }
                    },
                    new TestUser
                    {
                        SubjectId = "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd",
                        Username = "vicky@gmail.com",
                        Password = "Pass!2",
                        Claims =
                        {
                            new Claim(JwtClaimTypes.Name, "vicky@gmail.com"),
                            new Claim(JwtClaimTypes.Email, "vicky@gmail.com"),
                            new Claim("testing", "beta"),
                        }
                    },
                    new TestUser
                    {
                        SubjectId = "dcaec9ce-91c9-4105-8d4d-eee3365acd82",
                        Username = "cristina@gmail.com",
                        Password = "Pass!2",
                        Claims =
                        {
                            new Claim(JwtClaimTypes.Name, "cristina@gmail.com"),
                            new Claim(JwtClaimTypes.Email, "cristina@gmail.com"),
                            new Claim("subscription", "paid"),
                        }
                    },
                    new TestUser
                    {
                        SubjectId = "b9991f69-b4c1-477d-9432-2f7cf6099e02",
                        Username = "dev@gmail.com",
                        Password = "Pass!2",
                        Claims =
                        {
                            new Claim(JwtClaimTypes.Name, "dev@gmail.com"),
                            new Claim(JwtClaimTypes.Email, "dev@gmail.com"),
                            new Claim("subscription", "paid"),
                            new Claim("role", "Dev")
                        }
                    }
                };
            }
        }
    }
}