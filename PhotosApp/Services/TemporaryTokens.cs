﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PhotosApp.Services
{
    public static class TemporaryTokens
    {
        public static SymmetricSecurityKey SigningKey =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes("Ne!0_0!vzlomayesh!^_^!nikogda!"));

        public const string CookieName = "TemporaryToken";

        public static string GenerateEncoded()
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, "SecretUser"),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "Dev")
            };

            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddSeconds(30),
                signingCredentials: new SigningCredentials(TemporaryTokens.SigningKey, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}