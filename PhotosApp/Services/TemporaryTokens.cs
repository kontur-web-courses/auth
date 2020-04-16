using System;
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
            };

            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: null,
                expires: null,
                signingCredentials: null);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}